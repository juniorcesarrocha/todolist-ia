# Complete Todo — Especificação

**Feature:** `complete-todo`  
**Comando CQRS:** `CompleteTodoCommand`  
**Endpoint:** `PATCH /api/todos/{id}/complete`  
**Complexidade:** Média (CQRS + domínio existente + novo código HTTP 422)  
**Stack:** .NET 10, Clean Architecture, MediatR, EF Core + SQLite

## Problem Statement

Clientes da API precisam marcar uma tarefa como concluída com uma operação semântica e idempotente no sentido de negócio (não repetir conclusão), sem enviar corpo JSON com status. Hoje existe `PATCH /api/todos/{id}/status` com body `{ "status": "Done" }`, mas não há atalho dedicado nem rejeição explícita quando a tarefa já está concluída.

## Goals

- [x] Expor `PATCH /api/todos/{id}/complete` sem body, delegando ao MediatR
- [x] Transicionar tarefas em `Pending` ou `InProgress` para `Done` via `TodoItem.Complete()`
- [x] Retornar `TodoItemDto` atualizado em sucesso (`200 OK`)
- [x] Mapear ausência da tarefa para `404` e conclusão duplicada para `422 Unprocessable Entity`

## Out of Scope

| Item | Motivo |
|------|--------|
| Remover ou alterar `PATCH /api/todos/{id}/status` | Endpoint legado permanece; Complete é atalho semântico |
| Concluir várias tarefas em lote | Fora do escopo desta feature |
| Autenticação / autorização | Não existe no projeto atual |
| Campos `IsCompleted` / `CompletedAtUtc` legados | Modelo atual usa apenas `TodoItemStatus` + `UpdatedAt` |
| Retornar `200` quando já estiver `Done` | Requisito explícito: deve ser `422` |

---

## User Stories

### P1: Concluir tarefa existente ⭐ MVP

**User Story**: Como consumidor da API, quero marcar uma tarefa como concluída com um único PATCH, para encerrar o fluxo sem informar status manualmente.

**Why P1**: Entrega o valor principal da feature e o contrato HTTP descrito pelo produto.

**Acceptance Criteria**:

1. WHEN o cliente envia `PATCH /api/todos/{id}/complete` para uma tarefa existente com status `Pending` THEN o sistema SHALL responder `200 OK` com corpo `TodoItemDto` onde `status` é `Done` e `updatedAt` está preenchido
2. WHEN o cliente envia `PATCH /api/todos/{id}/complete` para uma tarefa existente com status `InProgress` THEN o sistema SHALL responder `200 OK` com corpo `TodoItemDto` onde `status` é `Done`
3. WHEN a conclusão é bem-sucedida THEN o sistema SHALL persistir a alteração via `ITodoItemRepository.UpdateAsync`
4. WHEN a conclusão é bem-sucedida THEN o handler SHALL aplicar a regra de domínio chamando `TodoItem.Complete()` (não `ChangeStatus` genérico)

**Independent Test**: Criar tarefa via `POST /api/todos`, chamar `PATCH .../complete`, verificar `200` e `status: Done` no JSON.

---

### P2: Erros previsíveis para o cliente

**User Story**: Como consumidor da API, quero códigos HTTP distintos para “não existe” e “já concluída”, para tratar cada caso na UI ou integração.

**Why P2**: Complementa o happy path; sem isso a feature não atende o contrato pedido.

**Acceptance Criteria**:

1. WHEN o `id` não corresponde a nenhuma tarefa persistida THEN o sistema SHALL responder `404 Not Found` com corpo JSON `{ "error": "<mensagem>" }` (mesmo padrão de `TodoNotFoundExceptionHandler`)
2. WHEN a tarefa existe e `status` já é `Done` THEN o sistema SHALL responder `422 Unprocessable Entity` com corpo JSON `{ "error": "<mensagem>" }` e SHALL NOT alterar `updatedAt` nem regravar a entidade
3. WHEN ocorre `404` ou `422` THEN o controller SHALL NOT engolir a exceção; o pipeline `IExceptionHandler` SHALL traduzir exceções de Application para HTTP

**Independent Test**: `PATCH` em GUID inexistente → `404`; concluir duas vezes a mesma tarefa → segunda chamada `422`.

---

## Edge Cases

- WHEN `{id}` não é um GUID válido THEN o ASP.NET Core SHALL responder `400 Bad Request` (comportamento padrão da rota `{id:guid}`) — fora da lógica do handler
- WHEN a tarefa está `Pending` ou `InProgress` e a conclusão é solicitada THEN o sistema SHALL aceitar ambos os estados (única transição permitida: para `Done`)
- WHEN a tarefa já está `Done` THEN o sistema SHALL responder `422` (não sucesso silencioso `200`)
- WHEN ocorre falha de persistência THEN o sistema SHALL propagar o erro (sem mascarar como `422` de negócio)

---

## Requisitos técnicos (rastreáveis)

| ID | Story | Requisito | Camada / artefato |
|----|-------|-----------|-------------------|
| COMPLETE-01 | P1 | `CompleteTodoCommand(Guid Id) : IRequest<TodoItemDto>` | `Application/Todos/Commands/CompleteTodo/` |
| COMPLETE-02 | P1 | `CompleteTodoCommandHandler` carrega por id, chama `Complete()`, `UpdateAsync`, retorna DTO | Application |
| COMPLETE-03 | P1 | `PATCH /api/todos/{id}/complete` no `TodosController`, sem body, retorna `Ok(result)` | Api |
| COMPLETE-04 | P1 | Sucesso HTTP `200` com `TodoItemDto` (mesmos campos dos demais endpoints) | Api + Application |
| COMPLETE-05 | P2 | Tarefa inexistente → `TodoNotFoundException` → `404` (handler existente) | Application + Api |
| COMPLETE-06 | P2 | Tarefa com `Status == Done` → nova exceção de Application (ex.: `TodoAlreadyCompletedException`) → `422` | Application + Api |
| COMPLETE-07 | P2 | Registrar `IExceptionHandler` para a exceção de “já concluída”, espelhando `TodoNotFoundExceptionHandler` | Api |
| COMPLETE-08 | P1 | Handler rejeita `Done` **antes** de persistir; não depende do early-return silencioso atual de `TodoItem.Complete()` | Application (ver nota abaixo) |
| COMPLETE-09 | P1 | Testes de Application: sucesso `Pending`/`InProgress`, `404`, `422` | `tests/TodoAI.Application.Tests` |
| COMPLETE-10 | P1 | Teste de Domain: `Complete` quando já `Done` (comportamento alinhado à regra — ver nota) | `tests/TodoAI.Domain.Tests` |
| COMPLETE-11 | P2 | Atualizar `AGENTS.md` (tabela de endpoints) após implementação | Docs |

**Status dos requisitos:** todos `Verified` (validação 2026-05-27 — ver `docs/validation/complete-todo-validation.md`).

**Cobertura:** 11/11 requisitos verificados (100%).

---

## Nota de brownfield (implementação)

O domínio hoje implementa `TodoItem.Complete()` como **idempotente** (retorno antecipado se já `Done`, sem exceção):

```47:56:src/TodoAI.Domain/Entities/TodoItem.cs
    public void Complete()
    {
        if (Status == TodoItemStatus.Done)
        {
            return;
        }

        Status = TodoItemStatus.Done;
        UpdatedAt = DateTime.UtcNow;
    }
```

Para cumprir **COMPLETE-06** e **COMPLETE-08**, a fase Execute deve escolher **uma** abordagem consistente:

| Opção | Onde | Prós |
|-------|------|------|
| A | Handler verifica `Status == Done` e lança `TodoAlreadyCompletedException` | Mudança mínima no Domain; HTTP fica na Application |
| B | `Complete()` lança `DomainException` se já `Done` | Regra centralizada no Domain; exige ajuste de testes de domínio |

**Recomendação:** Opção A na primeira entrega (alinha a `TodoNotFoundException` na Application). Opcionalmente evoluir para B em ADR se quiser simetria de invariantes no Domain.

`PATCH /api/todos/{id}/status` com `{ "status": "Done" }` permanece idempotente via `ChangeStatus`; apenas o endpoint `/complete` aplica a semântica `422` para duplicata.

---

## Contrato HTTP (referência)

### Request

```http
PATCH /api/todos/{id}/complete HTTP/1.1
```

Sem corpo. `{id}` = GUID da tarefa.

### Responses

| Cenário | Status | Corpo (exemplo) |
|---------|--------|-----------------|
| Sucesso | `200` | `TodoItemDto` serializado (JSON) |
| Não encontrada | `404` | `{ "error": "Todo with id '...' was not found." }` |
| Já concluída | `422` | `{ "error": "Todo is already completed." }` (mensagem final a definir na implementação, estável e em inglês como demais exceções de Application) |

### Exemplo de sucesso (`200`)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Comprar leite",
  "description": null,
  "status": "Done",
  "createdAt": "2026-05-27T12:00:00Z",
  "updatedAt": "2026-05-27T12:05:00Z"
}
```

---

## Success Criteria

- [x] `dotnet build` sem erros após implementação
- [x] `dotnet test` passa com novos testes de Application (e Domain se Opção B)
- [x] Documentação de endpoints em `AGENTS.md` inclui `PATCH /api/todos/{id}/complete`
- [x] Comportamento verificável manualmente: criar → complete → segundo complete retorna `422`

---

## Próximos passos (workflow TLC)

| Fase | Ação |
|------|------|
| **Design** | Pode ser omitida (padrão CQRS já estabelecido) |
| **Tasks** | Opcional; ≤5 passos implícitos: command/handler, exceção + handler HTTP, controller, testes, docs |
| **Execute** | Implementar após aprovação desta spec |
| **Validate** | `validate spec: complete-todo` ou testes + smoke HTTP |

**Confirmação:** Revise esta spec. Se estiver alinhada, prossiga com **design** (se quiser detalhar Opção A vs B) ou direto para **implement**.

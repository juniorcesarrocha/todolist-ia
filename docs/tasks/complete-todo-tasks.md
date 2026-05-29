# Complete Todo Tasks

**Design**: `docs/design/complete-todo-design.md`  
**Spec**: `.specs/features/complete-todo/spec.md`  
**Status**: Done

---

## Execution Plan

### Phase 1: Foundation (Sequential)

```
T1 -> T2
```

### Phase 2: Endpoint + Tests (Sequential for safety)

```
T2 -> T3 -> T4
```

### Phase 3: Documentation + Gate (Sequential)

```
T4 -> T5
```

---

## Task Breakdown

### T1: Criar command/handler de CompleteTodo com regra 422

**What**: Criar `CompleteTodoCommand` e `CompleteTodoCommandHandler` na Application com validações de not found e already completed.
**Where**:
- `src/TodoAI.Application/Todos/Commands/CompleteTodo/CompleteTodoCommand.cs`
- `src/TodoAI.Application/Todos/Commands/CompleteTodo/CompleteTodoCommandHandler.cs`
**Depends on**: None
**Reuses**:
- `src/TodoAI.Application/Todos/Commands/UpdateTodoStatus/UpdateTodoStatusCommandHandler.cs`
- `src/TodoAI.Domain/Entities/TodoItem.cs`
**Requirement**: COMPLETE-01, COMPLETE-02, COMPLETE-08

**Tools**:
- MCP: NONE
- Skill: `coding-guidelines`

**Done when**:
- [ ] Command implementa `IRequest<TodoItemDto>`
- [ ] Handler carrega item por `Id` com `GetByIdAsync`
- [ ] Handler lança `TodoNotFoundException` quando item inexistente
- [ ] Handler lança `TodoAlreadyCompletedException` quando `Status == Done`
- [ ] Handler chama `item.Complete()` e `UpdateAsync` apenas em caminho de sucesso
- [ ] Handler retorna `TodoItemDto` atualizado

**Tests**: unit (co-locado em T2)
**Gate**: quick

**Verify**:
- `dotnet test tests/TodoAI.Application.Tests --filter "FullyQualifiedName~CompleteTodo"`
- Esperado: testes de command handler passam

---

### T2: Criar exceção de negócio e mapeamento HTTP 422

**What**: Criar exceção `TodoAlreadyCompletedException`, criar `IExceptionHandler` de 422 e registrar no `Program.cs`.
**Where**:
- `src/TodoAI.Application/Exceptions/TodoAlreadyCompletedException.cs`
- `src/TodoAI.Api/ExceptionHandling/TodoAlreadyCompletedExceptionHandler.cs`
- `src/TodoAI.Api/Program.cs`
**Depends on**: T1
**Reuses**:
- `src/TodoAI.Application/Exceptions/TodoNotFoundException.cs`
- `src/TodoAI.Api/ExceptionHandling/TodoNotFoundExceptionHandler.cs`
**Requirement**: COMPLETE-06, COMPLETE-07

**Tools**:
- MCP: NONE
- Skill: `coding-guidelines`

**Done when**:
- [ ] Exceção possui mensagem estável para contrato HTTP
- [ ] Exception handler responde `422` com payload `{ "error": ... }`
- [ ] Novo handler está registrado no pipeline de exception handling
- [ ] Fluxo de `404` existente permanece inalterado

**Tests**: unit/integration (co-locado em T4)
**Gate**: quick

**Verify**:
- `dotnet build`
- Esperado: compilação sem erros após registro do handler

---

### T3: Expor endpoint PATCH /api/todos/{id}/complete

**What**: Adicionar action no `TodosController` que envia `CompleteTodoCommand` ao MediatR e retorna `200` com DTO.
**Where**:
- `src/TodoAI.Api/Controllers/TodosController.cs`
**Depends on**: T2
**Reuses**:
- Padrão de action em `TodosController` (`Update`, `UpdateStatus`)
**Requirement**: COMPLETE-03, COMPLETE-04, COMPLETE-05

**Tools**:
- MCP: NONE
- Skill: `coding-guidelines`

**Done when**:
- [ ] Endpoint usa rota `{id:guid}/complete`
- [ ] Endpoint não exige body
- [ ] Endpoint retorna `Ok(result)` no caminho feliz
- [ ] Exceções de negócio são propagadas para handlers globais

**Tests**: integration (co-locado em T4)
**Gate**: quick

**Verify**:
- `dotnet build`
- Esperado: API compila com novo endpoint

---

### T4: Cobrir com testes Application/Domain/API

**What**: Criar/atualizar testes para sucesso, `404`, `422` e comportamento de domínio relacionado a `Complete()`.
**Where**:
- `tests/TodoAI.Application.Tests/Todos/CompleteTodoCommandHandlerTests.cs` (novo)
- `tests/TodoAI.Domain.Tests/Entities/TodoItemTests.cs` (ajuste/aditivo)
- `tests/TodoAI.Api.Tests/` (novo teste de endpoint para 200/404/422)
**Depends on**: T3
**Reuses**:
- `tests/TodoAI.Application.Tests/Todos/UpdateTodoStatusCommandHandlerTests.cs`
- `tests/TodoAI.Api.Tests/Infrastructure/TodoApiWebApplicationFactory.cs`
**Requirement**: COMPLETE-09, COMPLETE-10

**Tools**:
- MCP: NONE
- Skill: `coding-guidelines`

**Done when**:
- [ ] Application tests cobrem: sucesso (`Pending`/`InProgress`), `404`, `422`
- [ ] API tests validam contratos HTTP `200/404/422` do endpoint `/complete`
- [ ] Domain tests permanecem coerentes com `TodoItem.Complete()`
- [ ] Gate check passa sem deletar testes existentes

**Tests**: unit + integration
**Gate**: full

**Verify**:
- `dotnet test`
- Esperado: suíte verde

---

### T5: Atualizar documentação de endpoint e checklist final

**What**: Atualizar tabela de endpoints no `AGENTS.md` e registrar conclusão da fase com rastreabilidade.
**Where**:
- `AGENTS.md`
- `docs/tasks/complete-todo-tasks.md` (status para In Progress/Done durante execução)
**Depends on**: T4
**Reuses**:
- Tabela atual de endpoints em `AGENTS.md`
**Requirement**: COMPLETE-11

**Tools**:
- MCP: NONE
- Skill: NONE

**Done when**:
- [ ] Tabela de endpoints inclui `PATCH /api/todos/{id}/complete`
- [ ] Nenhum endpoint existente foi alterado indevidamente na documentação
- [ ] Build e testes finais executados antes de marcar Done

**Tests**: none
**Gate**: full

**Verify**:
- `dotnet build && dotnet test`
- Esperado: build e testes passando após atualização de docs

---

## Parallel Execution Map

Nenhuma task foi marcada com `[P]` para reduzir risco de retrabalho, porque:
- T2 depende do contrato escolhido em T1
- T3 depende de T2 para garantir tradução 422 pronta
- T4 precisa da API e handlers já integrados
- T5 depende de resultado validado de T4

```
Phase 1:
  T1 -> T2

Phase 2:
  T2 -> T3 -> T4

Phase 3:
  T4 -> T5
```

---

## Task Granularity Check

| Task | Scope | Status |
|---|---|---|
| T1 | 1 capability CQRS (command+handler) | ✅ Granular |
| T2 | 1 capability de erro de negócio (422) | ✅ Granular |
| T3 | 1 endpoint HTTP | ✅ Granular |
| T4 | 1 deliverable coeso de validação da feature (testes) | ✅ Granular |
| T5 | 1 atualização de documentação + fechamento | ✅ Granular |

---

## Diagram-Definition Cross-Check

| Task | Depends On (task body) | Diagram Shows | Status |
|---|---|---|---|
| T1 | None | Início do fluxo | ✅ Match |
| T2 | T1 | `T1 -> T2` | ✅ Match |
| T3 | T2 | `T2 -> T3` | ✅ Match |
| T4 | T3 | `T3 -> T4` | ✅ Match |
| T5 | T4 | `T4 -> T5` | ✅ Match |

---

## Test Co-location Validation

| Task | Code Layer Modified | Matrix Requerida | Task Says | Status |
|---|---|---|---|---|
| T1 | Application | Unit | `unit (co-locado em T2)` | ✅ OK (executado no fluxo de testes da feature) |
| T2 | Application + Api | Unit/Integration | `unit/integration (co-locado em T4)` | ✅ OK |
| T3 | Api | Integration | `integration (co-locado em T4)` | ✅ OK |
| T4 | Tests (Application/Domain/Api) | Unit + Integration | `unit + integration` | ✅ OK |
| T5 | Docs | none | `none` | ✅ OK |

---

## Pergunta obrigatória antes do Execute

Para cada task, quais ferramentas você quer que eu use durante a implementação?

- **MCPs disponíveis**: `cursor-app-control`, `cursor-backend-control`, `cursor-ide-browser`, `project-0-todolist-ia-github`, `project-0-todolist-ia-context7`, `project-0-todolist-ia-agent-skills`
- **Skills disponíveis (relevantes)**: `coding-guidelines`, `tlc-spec-driven`, `code-reviewer`, `spec-validator`

Se não tiver preferência, sigo com ferramentas locais padrão (edição + `dotnet build` + `dotnet test`).

# Code Review: complete-todo

**Data:** 2026-05-27  
**Escopo:**
- `src/TodoAI.Application/Todos/Commands/CompleteTodo/CompleteTodoCommand.cs`
- `src/TodoAI.Application/Todos/Commands/CompleteTodo/CompleteTodoCommandHandler.cs`
- `src/TodoAI.Application/Exceptions/TodoAlreadyCompletedException.cs`
- `src/TodoAI.Api/Controllers/TodosController.cs` (action `Complete`)
- `src/TodoAI.Api/ExceptionHandling/TodoAlreadyCompletedExceptionHandler.cs`
- `src/TodoAI.Api/Program.cs` (registro do handler)
- `tests/TodoAI.Application.Tests/Todos/CompleteTodoCommandHandlerTests.cs`
- `tests/TodoAI.Api.Tests/Todos/CompleteTodoEndpointTests.cs`
- `tests/TodoAI.Domain.Tests/Entities/TodoItemTests.cs` (cenários `Complete`)
- `src/TodoAI.Application/TodoAI.Application.csproj`
- `src/TodoAI.Api/TodoAI.Api.csproj`

**Referências:** `.specs/features/complete-todo/spec.md`, `docs/validation/complete-todo-validation.md`

## Resumo executivo

- 🔴 CRÍTICO: 0 | 🟡 AVISO: 4 | 🟢 OK: 18
- **Gate PR:** **APROVADO**

## Checklist por categoria

### Convenções C#

| Item | Status | Evidência / notas |
|------|--------|-------------------|
| PascalCase em tipos e membros públicos | 🟢 OK | `CompleteTodoCommand`, `Complete`, `TryHandleAsync` |
| camelCase em parâmetros/locais | 🟢 OK | `request`, `cancellationToken`, `alreadyCompleted` |
| Prefixo `_` em campos privados | 🟢 OK | `_repository`, `_logger` |
| Nullable em APIs públicas | 🟢 OK | `TodoItemDto.Description` como `string?`; `Nullable` habilitado nos csproj |

### Clean Architecture

| Item | Status | Evidência / notas |
|------|--------|-------------------|
| Application só referencia Domain | 🟢 OK | `TodoAI.Application.csproj` → Domain + MediatR |
| Api compõe Application + Infrastructure | 🟢 OK | Handler e exceções na camada correta |
| Controller fino | 🟢 OK | `Complete` apenas `Send` + `Ok(result)` |
| Regra 422 na Application | 🟢 OK | Checagem `Status == Done` antes de persistir (Opção A da spec) |
| DbContext fora do escopo indevido | 🟢 OK | Persistência via `ITodoItemRepository` |

### Qualidade

| Item | Status | Evidência / notas |
|------|--------|-------------------|
| Magic values | 🟢 OK | `TodoItemStatus.Done` via enum |
| Exceptions para fluxo de negócio HTTP | 🟢 OK | Alinhado a `TodoNotFoundException` / handlers existentes |
| Guard antes de `Complete()` | 🟢 OK | Essencial: sem guard, `Complete()` idempotente + `UpdateAsync` retornaria `200` indevido |
| DRY (mapeamento DTO) | 🟡 AVISO | `new TodoItemDto(...)` repetido em todos os handlers (padrão pré-existente) |
| SRP | 🟢 OK | Handler orquestra; domínio transiciona estado |

### Testes

| Item | Status | Evidência / notas |
|------|--------|-------------------|
| Handler: happy path Pending/InProgress | 🟢 OK | `Handle_WhenTodoIsNotDone_ShouldCompleteAndReturnDto` (Theory) |
| Handler: 404 e 422 | 🟢 OK | Testes dedicados + `UpdateAsync` never no 422 |
| API: 200 / 404 / 422 | 🟢 OK | `CompleteTodoEndpointTests` (3 cenários) |
| Domain: `Complete` idempotente | 🟢 OK | `Complete_WhenAlreadyDone_ShouldRemainDoneWithoutUpdatingTimestamp` |
| Nomenclatura `Método_Cenário_Resultado` | 🟢 OK | Application e Api seguem padrão |
| Cobertura HTTP InProgress | 🟡 AVISO | `InProgress` coberto só na Application, não em teste de API |
| Asserts do DTO no teste API 200 | 🟡 AVISO | Valida `Id`, `Status`, `UpdatedAt`; não asserta `Title`/`Description`/`CreatedAt` |

## Achados detalhados

### 🔴 CRÍTICO

Nenhum.

### 🟡 AVISO

1. **Teste de API sem fluxo `InProgress`** — `CompleteTodoEndpointTests` cria tarefa via POST (`Pending`) e conclui; transição `InProgress → Done` só aparece em `CompleteTodoCommandHandlerTests`. Recomendação: adicionar teste API que chama `PATCH .../status` com `InProgress` antes de `/complete`, ou documentar aceitação explícita da cobertura apenas na Application.

2. **Asserções parciais no DTO (API 200)** — `PatchComplete_WhenTodoExists_ShouldReturn200WithDoneStatus` não verifica `Title`, `Description` e `CreatedAt` inalterados. Risco baixo, mas um regressão de serialização poderia passar despercebida.

3. **Mapeamento `TodoItemDto` duplicado** — `CompleteTodoCommandHandler` repete o construtor de DTO (L34–40), como os demais handlers. Consistente com o projeto; extrair helper/mapper seria melhoria transversal, não bloqueante desta PR.

4. **Mensagem de exceção vs exemplo da spec** — Implementação: `Todo with id '{id}' is already completed.`; spec de contrato citava `Todo is already completed.` Ambos válidos; alinhar documentação OpenAPI/clients se mensagem estável for contrato público rígido.

### 🟢 OK

- `CompleteTodoCommand` como `record` imutável com `IRequest<TodoItemDto>`.
- Handler: `GetByIdAsync` → validações → `Complete()` → `UpdateAsync` → DTO; usa `Complete()` e não `ChangeStatus`.
- Pré-checagem `Status == Done` **antes** de `UpdateAsync` (cumpre COMPLETE-08 e evita falso 200).
- `TodoAlreadyCompletedException` espelha estilo de `TodoNotFoundException`.
- `TodoAlreadyCompletedExceptionHandler` retorna `422` e JSON `{ error }` como o handler de 404.
- Registro em `Program.cs` com `AddExceptionHandler`.
- Controller sem try/catch; exceções sobem para o pipeline global.
- Rota `PATCH {id:guid}/complete` sem body, retorno `Ok`.
- Testes verificam que `UpdateAsync` não é chamado em 404/422.
- Teste de domínio documenta idempotência de `Complete()` no núcleo (coerente com Opção A).
- `AGENTS.md` documenta o novo endpoint.
- Validação de spec: 11/11 requisitos verificados, `dotnet test` verde (35 testes).

## Correções necessárias (se BLOQUEADO)

N/A — gate aprovado.

## Recomendações pós-merge (opcional)

1. Teste API: `InProgress` → `PATCH /complete` → `200`.
2. Asserts completos do `TodoItemDto` no teste de sucesso da API.
3. Considerar `ProducesResponseType` no action `Complete` quando o projeto padronizar documentação OpenAPI por endpoint.

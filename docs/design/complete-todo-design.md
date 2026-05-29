# Complete Todo Design

**Spec**: `.specs/features/complete-todo/spec.md`  
**Status**: Draft

---

## Architecture Overview

A feature segue o fluxo CQRS já consolidado no projeto: controller fino na API, comando/handler na Application, regra de negócio no Domain e persistência via repositório na Infrastructure.

```mermaid
graph TD
    Client[HTTP Client] --> Controller[TodosController PATCH /api/todos/{id}/complete]
    Controller --> Mediator[IMediator.Send CompleteTodoCommand]
    Mediator --> Handler[CompleteTodoCommandHandler]
    Handler --> RepoGet[ITodoItemRepository.GetByIdAsync]
    RepoGet --> Handler
    Handler --> DomainCheck{item.Status == Done?}
    DomainCheck -- Yes --> AlreadyCompletedEx[TodoAlreadyCompletedException]
    DomainCheck -- No --> DomainComplete[TodoItem.Complete()]
    DomainComplete --> RepoUpdate[ITodoItemRepository.UpdateAsync]
    RepoUpdate --> Dto[TodoItemDto]
    Dto --> Ok200[200 OK]
    AlreadyCompletedEx --> Ex422[TodoAlreadyCompletedExceptionHandler]
    Ex422 --> Http422[422 Unprocessable Entity]
    Handler --> NotFoundEx[TodoNotFoundException]
    NotFoundEx --> Ex404[TodoNotFoundExceptionHandler]
    Ex404 --> Http404[404 Not Found]
```

---

## Code Reuse Analysis

### Existing Components to Leverage

| Componente | Local | Reuso |
|---|---|---|
| `TodosController` | `src/TodoAI.Api/Controllers/TodosController.cs` | Adicionar novo action PATCH seguindo padrão dos endpoints atuais |
| `UpdateTodoStatusCommandHandler` | `src/TodoAI.Application/Todos/Commands/UpdateTodoStatus/UpdateTodoStatusCommandHandler.cs` | Reusar padrão de carregamento, validação de not found, persistência e retorno DTO |
| `TodoNotFoundException` | `src/TodoAI.Application/Exceptions/TodoNotFoundException.cs` | Reusar exceção de ausência para resposta 404 |
| `TodoNotFoundExceptionHandler` | `src/TodoAI.Api/ExceptionHandling/TodoNotFoundExceptionHandler.cs` | Espelhar padrão para novo handler de 422 |
| `TodoItem.Complete()` | `src/TodoAI.Domain/Entities/TodoItem.cs` | Reusar método de domínio para transição para `Done` |
| `TodoItemDto` | `src/TodoAI.Application/Todos/Dtos/TodoItemDto.cs` | Reusar contrato de saída já padronizado |
| `UpdateTodoStatusCommandHandlerTests` | `tests/TodoAI.Application.Tests/Todos/UpdateTodoStatusCommandHandlerTests.cs` | Reusar estilo de teste unitário com Moq + FluentAssertions |

### Integration Points

| Sistema | Integração |
|---|---|
| ASP.NET Core Exception Handling | Novo `IExceptionHandler` para mapear exceção de negócio para 422 |
| MediatR | Novo comando `CompleteTodoCommand` e handler no assembly Application |
| EF Core Repository | Persistência via `ITodoItemRepository.UpdateAsync` após `Complete()` |

---

## Components

### 1) `CompleteTodoCommand`

- **Purpose**: representar a intenção de concluir uma tarefa por `Id`.
- **Location**: `src/TodoAI.Application/Todos/Commands/CompleteTodo/CompleteTodoCommand.cs`
- **Interface**:
  - `record CompleteTodoCommand(Guid Id) : IRequest<TodoItemDto>`
- **Dependencies**: MediatR, `TodoItemDto`
- **Reuses**: padrão de comandos existentes (ex.: `UpdateTodoStatusCommand`)

### 2) `CompleteTodoCommandHandler`

- **Purpose**: orquestrar conclusão de tarefa com validações de negócio e persistência.
- **Location**: `src/TodoAI.Application/Todos/Commands/CompleteTodo/CompleteTodoCommandHandler.cs`
- **Interface**:
  - `Task<TodoItemDto> Handle(CompleteTodoCommand request, CancellationToken cancellationToken)`
- **Dependencies**: `ITodoItemRepository`, `TodoNotFoundException`, `TodoAlreadyCompletedException`, `TodoItem`
- **Reuses**: fluxo de recuperação/persistência/mapeamento do `UpdateTodoStatusCommandHandler`

### 3) `TodoAlreadyCompletedException`

- **Purpose**: sinalizar violação da regra de negócio quando a tarefa já está `Done`.
- **Location**: `src/TodoAI.Application/Exceptions/TodoAlreadyCompletedException.cs`
- **Interface**:
  - `sealed class TodoAlreadyCompletedException : Exception`
- **Dependencies**: `System.Exception`
- **Reuses**: convenção de exceções de Application

### 4) `TodoAlreadyCompletedExceptionHandler`

- **Purpose**: traduzir exceção de negócio para `422 Unprocessable Entity`.
- **Location**: `src/TodoAI.Api/ExceptionHandling/TodoAlreadyCompletedExceptionHandler.cs`
- **Interface**:
  - `ValueTask<bool> TryHandleAsync(HttpContext, Exception, CancellationToken)`
- **Dependencies**: `IExceptionHandler`, `ILogger<>`, `TodoAlreadyCompletedException`
- **Reuses**: estrutura de `TodoNotFoundExceptionHandler`

### 5) `TodosController.Complete`

- **Purpose**: expor endpoint `PATCH /api/todos/{id}/complete` sem body.
- **Location**: `src/TodoAI.Api/Controllers/TodosController.cs`
- **Interface**:
  - `Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)`
- **Dependencies**: IMediator, `CompleteTodoCommand`
- **Reuses**: estilo de actions existentes em `TodosController`

---

## Data Models

### Request Model

Sem body. Apenas `id` via rota.

### Response Model (`TodoItemDto`)

```csharp
public sealed record TodoItemDto(
    Guid Id,
    string Title,
    string? Description,
    TodoItemStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
```

**Relações**: DTO é projeção de `TodoItem` no retorno da API.

---

## Error Handling Strategy

| Cenário | Handling | Impacto para cliente |
|---|---|---|
| `Id` inexistente | `TodoNotFoundException` + handler existente | `404` com `{ "error": "..." }` |
| Tarefa já `Done` | `TodoAlreadyCompletedException` + novo handler | `422` com `{ "error": "..." }` |
| Falha de persistência | Propagação da exceção técnica | `5xx` padrão (fora de regra de negócio) |
| `id` inválido na rota | Validação automática ASP.NET (`{id:guid}`) | `400` |

---

## Tech Decisions

| Decisão | Escolha | Racional |
|---|---|---|
| Local da regra de 422 para duplicidade | Application handler (pré-checagem de `Status`) | Menor impacto no Domain atual; atende requisito imediatamente |
| Método de domínio para transição | `TodoItem.Complete()` | Mantém semântica explícita de “concluir”, evita uso genérico de `ChangeStatus` |
| Tradução HTTP de regra de negócio | Novo `IExceptionHandler` dedicado | Mantém controller fino e consistência com pipeline de exceções já adotado |

---

## Requirement Mapping

| Requisito | Componentes de design que cobrem |
|---|---|
| COMPLETE-01 | `CompleteTodoCommand` |
| COMPLETE-02 | `CompleteTodoCommandHandler` |
| COMPLETE-03 | `TodosController.Complete` |
| COMPLETE-04 | Handler + retorno `Ok(TodoItemDto)` |
| COMPLETE-05 | Reuso de `TodoNotFoundException` + handler 404 |
| COMPLETE-06 | `TodoAlreadyCompletedException` + handler 422 |
| COMPLETE-07 | Registro no `Program.cs` do novo exception handler |
| COMPLETE-08 | Pré-checagem de status no handler antes de `UpdateAsync` |
| COMPLETE-09 | Testes unitários da Application para sucesso/404/422 |
| COMPLETE-10 | Testes de Domain para `Complete` em cenários relevantes |
| COMPLETE-11 | Atualização da tabela de endpoints no `AGENTS.md` |

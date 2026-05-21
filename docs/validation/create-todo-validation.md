# Validação: Create Todo

**Data:** 2026-05-21  
**Spec:** docs/specs/create-todo-spec.md

## Resumo executivo

- Total de requisitos: 12
- ✅ Implementado: 12 | ⚠️ Parcial: 0 | ❌ Ausente: 0
- **Cobertura:** 100% — `round(100 × (12 + 0) / 12) = 100%`
- **Gate 80%:** **APROVADO**

## Tabela por requisito


| ID      | Requisito                        | Status         | Implementação                   | Testes                                                                                                     | Notas                                 |
| ------- | -------------------------------- | -------------- | ------------------------------- | ---------------------------------------------------------------------------------------------------------- | ------------------------------------- |
| REQ-001 | Comando CQRS `CreateTodoCommand` | ✅ Implementado | `CreateTodoCommand.cs`          | `CreateTodoCommandHandlerTests.cs`                                                                         | —                                     |
| REQ-002 | Handler MediatR                  | ✅ Implementado | `CreateTodoCommandHandler.cs`   | `CreateTodoCommandHandlerTests.cs`                                                                         | —                                     |
| REQ-003 | Criação via `TodoItem.Create`    | ✅ Implementado | Handler + `TodoItem.cs`         | Handler + `TodoItemTests.cs`                                                                               | —                                     |
| REQ-004 | Status inicial `Pending`         | ✅ Implementado | `TodoItem.Create`               | Handler test + `TodoItemTests`                                                                             | —                                     |
| REQ-005 | Persistência `AddAsync`          | ✅ Implementado | Handler + `ITodoItemRepository` | `Verify(AddAsync, Times.Once)`                                                                             | —                                     |
| REQ-006 | Retorno `TodoItemDto` completo   | ✅ Implementado | Handler mapeia todos os campos  | `CreateTodoCommandHandlerTests` — asserts `Id`, `Title`, `Description`, `Status`, `CreatedAt`, `UpdatedAt` | Gap fechado                           |
| REQ-007 | Título obrigatório               | ✅ Implementado | `TodoItem.Create`               | `TodoItemTests.Create_WithEmptyTitle`                                                                      | —                                     |
| REQ-008 | Limite 200 caracteres            | ✅ Implementado | `TodoItem.TitleMaxLength`       | `TodoItemTests.Create_WithTitleAboveMaxLength`                                                             | —                                     |
| REQ-009 | Trim do título                   | ✅ Implementado | `TodoItem.Create`               | `TodoItemTests.Create_WithValidTitle`                                                                      | —                                     |
| REQ-010 | Descrição `null` na criação      | ✅ Implementado | Handler `description: null`     | Handler test `Description.Should().BeNull()`                                                               | Gap fechado                           |
| REQ-011 | `POST /api/todos`                | ✅ Implementado | `TodosController.Create`        | `CreateTodoEndpointTests.PostTodos_...`                                                                    | Gap fechado — `WebApplicationFactory` |
| REQ-012 | HTTP 201 Created                 | ✅ Implementado | `CreatedAtAction`               | `CreateTodoEndpointTests` — `201` + `Location`                                                             | Gap fechado                           |


## Gaps

Nenhum gap pendente para esta feature.

## Recomendações

- Feature **create-todo** liberada para avanço (gate ≥ 80%).
- Opcional futuro: teste de `POST` com título inválido retornando 400/422 se a API passar a traduzir `DomainException` em ProblemDetails.


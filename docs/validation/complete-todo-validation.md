# Validação: Complete Todo

**Data:** 2026-05-27  
**Spec:** `.specs/features/complete-todo/spec.md`  
**Nota:** A spec oficial desta feature está em `.specs/features/complete-todo/spec.md` (workflow TLC). Não há espelho em `docs/specs/complete-todo-spec.md`.

## Resumo executivo

- Total de requisitos: 11 (`COMPLETE-01` … `COMPLETE-11`)
- ✅ Implementado: 11 | ⚠️ Parcial: 0 | ❌ Ausente: 0
- **Cobertura:** 100% — `round(100 × (11 + 0) / 11) = 100%`
- **Gate 80%:** **APROVADO**

### Gate de testes (build)

- Comando: `dotnet test TodoAI.sln`
- Resultado: **35 aprovados**, 0 falhas, 0 ignorados
  - Domain: 14 | Application: 15 | Infrastructure: 1 | Api: 5
- `dotnet build`: implícito no pipeline de teste (compilação OK)

---

## Tabela por requisito

| ID | Requisito | Status | Implementação | Testes | Notas |
|----|-----------|--------|---------------|--------|-------|
| COMPLETE-01 | `CompleteTodoCommand` : `IRequest<TodoItemDto>` | ✅ Implementado | `src/TodoAI.Application/Todos/Commands/CompleteTodo/CompleteTodoCommand.cs` | `CompleteTodoCommandHandlerTests.cs` (usa o command) | — |
| COMPLETE-02 | Handler: load, `Complete()`, `UpdateAsync`, DTO | ✅ Implementado | `CompleteTodoCommandHandler.cs` | `Handle_WhenTodoIsNotDone_*`, verifica `UpdateAsync` | Usa `TodoItem.Complete()`, não `ChangeStatus` |
| COMPLETE-03 | `PATCH /api/todos/{id}/complete` sem body | ✅ Implementado | `TodosController.Complete` | `CompleteTodoEndpointTests.PatchComplete_WhenTodoExists_*` | Rota `{id:guid}/complete` |
| COMPLETE-04 | HTTP 200 + `TodoItemDto` | ✅ Implementado | `return Ok(result)` | API: status `Done`, `UpdatedAt`; Application: DTO completo | — |
| COMPLETE-05 | Inexistente → `404` | ✅ Implementado | `TodoNotFoundException` no handler | Application + `CompleteTodoEndpointTests.PatchComplete_WhenTodoNotFound_*` | Handler existente em `Program.cs` |
| COMPLETE-06 | Já `Done` → `422` | ✅ Implementado | `TodoAlreadyCompletedException` | Application + API `PatchComplete_WhenTodoAlreadyDone_*` | Mensagem: `Todo with id '...' is already completed.` |
| COMPLETE-07 | `IExceptionHandler` para 422 | ✅ Implementado | `TodoAlreadyCompletedExceptionHandler.cs`, `Program.cs` | API retorna `422` com `{ "error": "..." }` | Espelha padrão do 404 |
| COMPLETE-08 | Rejeitar `Done` antes de persistir | ✅ Implementado | Checagem linhas 26–29 antes de `Complete()`/`UpdateAsync` | `Handle_WhenTodoAlreadyDone_*` — `UpdateAsync` nunca chamado | Opção A do design (Application) |
| COMPLETE-09 | Testes Application (sucesso/404/422) | ✅ Implementado | — | `CompleteTodoCommandHandlerTests.cs` (Theory Pending/InProgress, 404, 422) | — |
| COMPLETE-10 | Teste Domain `Complete` já `Done` | ✅ Implementado | `TodoItem.Complete()` idempotente no Domain | `TodoItemTests.Complete_WhenAlreadyDone_ShouldRemainDoneWithoutUpdatingTimestamp` | Domínio permanece idempotente; 422 só no endpoint |
| COMPLETE-11 | `AGENTS.md` atualizado | ✅ Implementado | Tabela de endpoints | N/A (documentação) | Linha PATCH `/complete` presente |

---

## User Stories (critérios WHEN/THEN)

### P1: Concluir tarefa existente ⭐ MVP

| Critério | Resultado |
|----------|-----------|
| PATCH em tarefa `Pending` → `200`, `status: Done`, `updatedAt` preenchido | ✅ PASS — `CompleteTodoEndpointTests` |
| PATCH em tarefa `InProgress` → `200`, `status: Done` | ✅ PASS — `CompleteTodoCommandHandlerTests` (Theory) |
| Persistência via `UpdateAsync` | ✅ PASS — verify no teste Application |
| Uso de `TodoItem.Complete()` | ✅ PASS — handler linha 31 |

**Status P1:** ✅ Completo

### P2: Erros previsíveis

| Critério | Resultado |
|----------|-----------|
| ID inexistente → `404` + `{ "error": "..." }` | ✅ PASS |
| Já `Done` → `422`, sem regravar | ✅ PASS — Application garante `UpdateAsync` never; API valida `422` |
| Exceções propagadas ao pipeline | ✅ PASS — controller sem try/catch |

**Status P2:** ✅ Completo

---

## Edge Cases

| Edge case | Resultado | Notas |
|-----------|-----------|-------|
| GUID inválido → `400` | ⚠️ Não testado automaticamente | Comportamento ASP.NET (`{id:guid}`); fora dos REQ numerados |
| `Pending` / `InProgress` aceitos | ✅ PASS | Application Theory |
| Já `Done` → `422` (não `200`) | ✅ PASS | API + Application |
| Falha de persistência → propaga erro | ✅ Aceito | Sem teste dedicado; padrão EF/repositório |

---

## Tasks (Execute)

| Task | Status | Evidência |
|------|--------|-----------|
| T1 | ✅ Done | commit `97a8b2b` |
| T2 | ✅ Done | commit `3c37aa3` |
| T3 | ✅ Done | commit `97ae9a8` |
| T4 | ✅ Done | commit `f058f14` |
| T5 | ✅ Done | commit `907dcad` |

---

## Gaps

Nenhum gap bloqueante nos requisitos `COMPLETE-01` … `COMPLETE-11`.

Melhorias opcionais (não bloqueiam gate):

- Teste de API com tarefa em `InProgress` (hoje coberto só na Application).
- Teste de integração para `PATCH` com GUID malformado → `400`.
- Espelhar spec em `docs/specs/complete-todo-spec.md` para alinhar ao validador legado (`spec-validator.mdc`).

---

## Recomendações

1. **Feature liberada** para merge/PR — gate ≥ 80% e suíte verde.
2. Atualizar `.specs/features/complete-todo/spec.md`: marcar requisitos como `Verified` e goals como concluídos.
3. Opcional: `review code: complete-todo` antes do PR.

---

## Requirement Traceability (sugerido para spec)

| Requirement | Status anterior | Novo status |
|-------------|-----------------|-------------|
| COMPLETE-01 … COMPLETE-11 | Pending | ✅ Verified |

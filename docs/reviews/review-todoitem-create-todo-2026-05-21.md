# Code Review: TodoItem e CreateTodoCommand

**Data:** 2026-05-21  
**Escopo:**
- `src/TodoAI.Domain/Entities/TodoItem.cs`
- `src/TodoAI.Application/Todos/Commands/CreateTodo/CreateTodoCommand.cs`
- `src/TodoAI.Application/Todos/Commands/CreateTodo/CreateTodoCommandHandler.cs`
- `tests/TodoAI.Domain.Tests/Entities/TodoItemTests.cs`
- `tests/TodoAI.Application.Tests/Todos/CreateTodoCommandHandlerTests.cs`
- `src/TodoAI.Domain/TodoAI.Domain.csproj`
- `src/TodoAI.Application/TodoAI.Application.csproj`

## Resumo executivo

- 🔴 CRÍTICO: 0 | 🟡 AVISO: 5 | 🟢 OK: 20
- **Gate PR:** APROVADO

## Checklist por categoria

### Convenções C#

| Item | Status | Evidência / notas |
|------|--------|-------------------|
| PascalCase em tipos e membros públicos | 🟢 OK | `TodoItem`, `Create`, `Handle`, `CreateTodoCommand` |
| camelCase em parâmetros/locais | 🟢 OK | `trimmedTitle`, `request`, `cancellationToken` |
| Prefixo `_` em campos privados | 🟢 OK | `_repository` em `CreateTodoCommandHandler` |
| Nullable em APIs públicas | 🟢 OK | `Description` como `string?`; propriedades inicializadas; `Nullable` nos csproj |

### Clean Architecture

| Item | Status | Evidência / notas |
|------|--------|-------------------|
| Domain sem refs proibidas | 🟢 OK | `TodoAI.Domain.csproj` sem ProjectReference |
| Application só referencia Domain | 🟢 OK | `TodoAI.Application.csproj` → Domain + MediatR |
| Sem lógica de negócio no handler além de orquestração | 🟢 OK | Handler delega criação a `TodoItem.Create` |
| DbContext fora de Infrastructure | 🟢 OK | Escopo não usa EF diretamente |

### Qualidade

| Item | Status | Evidência / notas |
|------|--------|-------------------|
| Magic numbers | 🟢 OK | `TitleMaxLength = 200` como constante pública |
| Magic strings | 🟡 AVISO | `"Title is required."` repetido em `Create` e `UpdateDetails` (L27, L75) |
| Exceptions para fluxo normal | 🟢 OK | `Complete`/`StartProgress`/`ChangeStatus` usam early return; `DomainException` só em validação |
| Duplicação óbvia | 🟡 AVISO | Validação de título duplicada entre `Create` e `UpdateDetails` |
| Responsabilidade única | 🟢 OK | Métodos focados em criar, completar, progresso, atualizar, mudar status |

### Testes

| Item | Status | Evidência / notas |
|------|--------|-------------------|
| Teste por método público relevante (TodoItem) | 🟢 OK | `Create`, `Complete`, `StartProgress`, `UpdateDetails`, `ChangeStatus` cobertos em `TodoItemTests.cs` (13 testes) |
| Teste do handler CreateTodo | 🟡 AVISO | Apenas happy path; sem cenário de título inválido |
| Nomenclatura `Método_Cenário_Resultado` | 🟡 AVISO | Domain OK; handler usa `Handle_ShouldPersistTodoAndReturnDto` (cenário implícito) |
| Asserts não relacionados no mesmo teste | 🟢 OK | Asserts agrupados por cenário |

## Achados detalhados

### 🔴 CRÍTICO

Nenhum.

### 🟡 AVISO

1. **Duplicação de validação de título** — blocos equivalentes em `Create` (L23–33) e `UpdateDetails` (L71–81); extrair método privado reduz risco de divergência.
2. **Magic string de mensagem de erro** — `"Title is required."` duplicada; considerar constante em `Domain`.
3. **Caminhos idempotentes não testados** — `Complete` quando já `Done` e `StartProgress` quando já `InProgress` sem teste de regressão (`ChangeStatus` idempotente agora coberto).
4. **CreateTodoCommandHandler — cenário de falha** — não há teste que verifique propagação de `DomainException` quando título é vazio/em branco.
5. **Nomenclatura de teste do handler** — renomear para padrão explícito, ex.: `Handle_WithValidTitle_ShouldPersistAndReturnDto`.

### 🟢 OK

- Entidade com factory `Create`, setters privados e construtor privado para EF.
- Validação de título vazio e tamanho máximo com `TitleMaxLength`.
- Status inicial `Pending` e timestamps UTC em mutações.
- `CreateTodoCommand` como `record` imutável com MediatR `IRequest<TodoItemDto>`.
- Handler injeta `ITodoItemRepository`, persiste e mapeia para `TodoItemDto`.
- Testes `UpdateDetails`: título válido, vazio, acima do máximo, descrição null e whitespace.
- Testes `ChangeStatus`: mudança efetiva, `UpdatedAt`, idempotência.
- Testes existentes: `Create` (válido, vazio, max length), `Complete`, `StartProgress`.
- Teste de handler verifica persistência (`Verify AddAsync`) e campos do DTO.
- Dependências de projeto alinhadas à Clean Architecture.
- `dotnet test TodoAI.sln`: 27 testes aprovados (2026-05-21).

## Correções necessárias (se BLOQUEADO)

N/A — gate aprovado.

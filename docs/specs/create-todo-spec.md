# Spec: Create Todo

**Feature:** `create-todo`  
**Comando CQRS:** `CreateTodoCommand`  
**Endpoint:** `POST /api/todos`

## Escopo

Criação de uma nova tarefa com título obrigatório, status inicial `Pending`, persistência em SQLite via EF Core e exposição REST na API.

## Requisitos

### REQ-001 — Comando CQRS

O caso de uso deve ser modelado como `CreateTodoCommand(string Title)` implementando `IRequest<TodoItemDto>` na camada Application.

**Artefatos esperados:** `src/TodoAI.Application/Todos/Commands/CreateTodo/CreateTodoCommand.cs`

---

### REQ-002 — Handler MediatR

Deve existir `CreateTodoCommandHandler` registrado via MediatR que orquestra criação e persistência, sem lógica de negócio além da coordenação.

**Artefatos esperados:** `CreateTodoCommandHandler.cs`

---

### REQ-003 — Criação via domínio

O handler deve instanciar a tarefa exclusivamente por `TodoItem.Create(title, description)` no Domain.

**Artefatos esperados:** `src/TodoAI.Domain/Entities/TodoItem.cs`, handler Application

---

### REQ-004 — Status inicial Pending

Toda tarefa criada deve ter `TodoItemStatus.Pending`.

**Artefatos esperados:** `TodoItem.Create`, enum `TodoItemStatus`

---

### REQ-005 — Persistência

Após criar a entidade, o handler deve chamar `ITodoItemRepository.AddAsync` e aguardar conclusão.

**Artefatos esperados:** `ITodoItemRepository`, implementação em Infrastructure

---

### REQ-006 — Retorno DTO

O handler deve retornar `TodoItemDto` contendo: `Id`, `Title`, `Description`, `Status`, `CreatedAt`, `UpdatedAt`.

**Artefatos esperados:** `TodoItemDto.cs`, mapeamento no handler

---

### REQ-007 — Título obrigatório

Título vazio, nulo ou apenas espaços deve ser rejeitado com `DomainException` e mensagem `"Title is required."`.

**Artefatos esperados:** `TodoItem.Create`

---

### REQ-008 — Limite de título

Título com mais de 200 caracteres deve ser rejeitado com `DomainException` indicando o limite `TitleMaxLength`.

**Artefatos esperados:** `TodoItem.TitleMaxLength`, `TodoItem.Create`

---

### REQ-009 — Normalização de título

O título informado deve ser persistido com `Trim()` aplicado.

**Artefatos esperados:** `TodoItem.Create`

---

### REQ-010 — Descrição nula na criação

O fluxo de criação via comando deve definir `Description` como `null` (sem descrição na criação).

**Artefatos esperados:** `CreateTodoCommandHandler` (`description: null`)

---

### REQ-011 — Endpoint REST POST

A API deve expor `POST /api/todos` com body `{ "title": "..." }`, delegando ao MediatR.

**Artefatos esperados:** `TodosController.Create`, `CreateTodoRequest`

---

### REQ-012 — Resposta HTTP 201

Sucesso na criação deve retornar `201 Created` com corpo da tarefa e cabeçalho de localização via `CreatedAtAction`.

**Artefatos esperados:** `TodosController.Create`

---

## Critérios de aceite globais

- `dotnet build` sem erros.
- Testes de Application cobrem persistência e DTO retornado.
- Testes de Domain cobrem validações de título.
- Testes de API (quando existirem) cobrem POST e status 201.

# AGENTS.md — Guia para agentes de IA (TodoAI)

Este documento orienta agentes de IA que trabalham no repositório **TodoAI** (`todolist-ia`): API REST de tarefas em .NET 10 com Clean Architecture, CQRS e EF Core.

## Objetivo do projeto

Construir e evoluir uma **Todo List API** com separação clara de responsabilidades, testes por camada e decisões arquiteturais documentadas em ADRs.

## Stack obrigatória

| Área | Tecnologia |
|------|------------|
| Runtime | .NET 10, C# |
| Arquitetura | Clean Architecture (4 projetos em `src/`) |
| Padrão de aplicação | CQRS com MediatR |
| Persistência | EF Core 10 + SQLite |
| Testes | xUnit, FluentAssertions, Moq |
| Layout | Monorepo: `src/`, `tests/`, `docs/` |

**Não introduzir** outro ORM, outro bus de comandos ou frameworks de teste alternativos sem ADR e aprovação explícita do usuário.

## Mapa do repositório

```
TodoAI.sln                 # Solução clássica (.sln); use `dotnet new sln -f sln` se recriar
.config/dotnet-tools.json # dotnet-ef (migrations)
src/
  TodoAI.Domain/           # Entidades, regras de domínio — sem dependências de outros projetos
  TodoAI.Application/      # Casos de uso, CQRS, interfaces (ports)
  TodoAI.Infrastructure/   # EF Core, repositórios, migrations, DI de infra
  TodoAI.Api/              # ASP.NET Core, controllers, composition root
tests/
  TodoAI.Domain.Tests/
  TodoAI.Application.Tests/
  TodoAI.Infrastructure.Tests/
  TodoAI.Api.Tests/
docs/
  adr/                     # ADR-001, ADR-002, ADR-003, …
ARCHITECTURE.md
Directory.Build.props
.cursor/rules/todoai-guardrails.mdc
```

## Regras de dependência (Clean Architecture)

```
Api → Application, Infrastructure
Infrastructure → Application, Domain
Application → Domain
Domain → (nenhum projeto interno)
```

- **Domain**: entidades, value objects, exceções de domínio. Sem MediatR, sem EF, sem ASP.NET.
- **Application**: requests/handlers MediatR, DTOs, interfaces de repositório. Sem EF, sem HTTP.
- **Infrastructure**: `DbContext`, configurações EF, implementações das interfaces da Application.
- **Api**: controllers finos; delegar para `IMediator.Send(...)`.

Violações de dependência devem ser corrigidas antes de merge.

## CQRS com MediatR

- **Commands**: alteram estado (`CreateTodoCommand`, etc.). Um handler por comando em `Application/Todos/Commands/...`.
- **Queries**: leitura sem efeitos colaterais (`GetTodosQuery`, etc.) em `Application/Todos/Queries/...`.
- Controllers **não** contêm lógica de negócio; apenas validam entrada HTTP e chamam MediatR.
- Registrar handlers via `AddApplication()` (`RegisterServicesFromAssembly`).

## Persistência (EF Core + SQLite)

- `TodoAiDbContext` em `Infrastructure/Persistence`.
- Configurações de entidade em `Persistence/Configurations/`.
- Migrations em `Infrastructure/Persistence/Migrations/`.
- Connection string: `appsettings.json` → `ConnectionStrings:DefaultConnection` (padrão `Data Source=todoai.db`).
- Startup para EF CLI: **TodoAI.Api**. Ferramenta local: `dotnet tool restore` → `dotnet ef`.
- Em Development, a API aplica migrations com `Database.MigrateAsync()` no pipeline de inicialização.

## Convenções de código

- `nullable` habilitado; evitar `null` silencioso em APIs públicas.
- Nomes em inglês para código (tipos, métodos, propriedades); documentação do repositório em português onde indicado.
- Entidades com comportamento: `Create`, métodos de domínio (`Complete`), setters privados quando possível.
- DTOs na Application como `record` quando forem imutáveis.
- Repositórios expõem apenas o necessário aos handlers; não vazar `DbContext` para Application.

## Testes

- **Domain.Tests**: regras puras, sem mocks.
- **Application.Tests**: handlers com **Moq** para ports (`ITodoItemRepository`).
- **Infrastructure.Tests**: EF com SQLite em memória.
- **Api.Tests**: smoke leve sobre o assembly da API (evoluir para `WebApplicationFactory` quando fizer sentido).
- Executar `dotnet test` antes de considerar a tarefa concluída quando houver mudança de comportamento.
- Usar FluentAssertions para asserções legíveis.

## Fluxo de trabalho recomendado para o agente

1. Ler `ARCHITECTURE.md` e ADRs relevantes em `docs/adr/`.
2. Respeitar `.cursor/rules/todoai-guardrails.mdc`.
3. Implementar na camada correta (domínio primeiro, depois application, infra, api).
4. Adicionar ou atualizar testes em `tests/` espelhando a feature.
5. `dotnet build` e `dotnet test` — reportar falhas com causa raiz.
6. Não commitar `.env`, secrets, `*.db` de desenvolvimento local (ver `.gitignore`).

## Comandos úteis

```bash
dotnet tool restore
dotnet build
dotnet test
dotnet run --project src/TodoAI.Api
dotnet ef migrations add <Name> --project src/TodoAI.Infrastructure --startup-project src/TodoAI.Api --output-dir Persistence/Migrations
dotnet ef database update --project src/TodoAI.Infrastructure --startup-project src/TodoAI.Api
```

## Endpoints atuais (referência)

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/todos` | Lista tarefas |
| POST | `/api/todos` | Cria tarefa (`{ "title": "..." }`) |
| PUT | `/api/todos/{id}` | Atualiza título e descrição |
| DELETE | `/api/todos/{id}` | Remove tarefa |
| PATCH | `/api/todos/{id}/status` | Altera status (`{ "status": "Pending" \| "InProgress" \| "Done" }`) |

## O que evitar

- Lógica de negócio em controllers ou em `Program.cs` além da composição DI e bootstrap (ex.: migrations em dev).
- Referência de Domain a Infrastructure ou Api.
- Novos pacotes NuGet sem justificativa alinhada à stack.
- Commits automáticos — só commitar quando o usuário pedir.
- Documentação genérica duplicando ADRs; preferir linkar `docs/adr/`.

## Links

- Repositório: https://github.com/juniorcesarrocha/todolist-ia
- [ARCHITECTURE.md](./ARCHITECTURE.md)
- [docs/adr/](./docs/adr/)

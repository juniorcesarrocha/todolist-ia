# TodoAI

API de lista de tarefas (Todo List) em **.NET 10** com **Clean Architecture**, **CQRS (MediatR)** e **EF Core 10 + SQLite**.

Repositório: [juniorcesarrocha/todolist-ia](https://github.com/juniorcesarrocha/todolist-ia)

## Estrutura

```
src/          # Domain, Application, Infrastructure, Api
tests/        # Testes por camada (xUnit, FluentAssertions, Moq)
docs/         # ADRs e documentação complementar
```

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `dotnet tool restore` (para `dotnet ef`)

## Comandos

```bash
dotnet tool restore
dotnet restore
dotnet build TodoAI.sln
dotnet test TodoAI.sln
dotnet run --project src/TodoAI.Api
```

## Documentação

- [AGENTS.md](./AGENTS.md) — guia para agentes de IA
- [ARCHITECTURE.md](./ARCHITECTURE.md) — visão da arquitetura
- [docs/adr/](./docs/adr/) — Architecture Decision Records

## Solução

O arquivo `TodoAI.sln` usa o formato clássico (`.sln`). No SDK 10 o modelo predefinido pode ser `.slnx`; para gerar `.sln` explicitamente: `dotnet new sln -n TodoAI -f sln`.

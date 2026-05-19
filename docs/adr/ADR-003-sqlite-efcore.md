# ADR-003: Persistência com EF Core 10 e SQLite

## Status

Aceito

## Contexto

O projeto precisa persistir tarefas com modelo relacional simples, setup local sem servidor de banco dedicado e alinhamento com a stack .NET 10 do repositório.

## Decisão

- Usar **Entity Framework Core 10** no projeto **Infrastructure**
- Provedor **Microsoft.EntityFrameworkCore.Sqlite**
- Arquivo local padrão: `Data Source=todoai.db` (configurável via `ConnectionStrings:DefaultConnection`)
- `TodoAiDbContext` com configurações Fluent API (`IEntityTypeConfiguration<T>`)
- Migrations EF em `TodoAI.Infrastructure/Persistence/Migrations/`; startup project para CLI: **TodoAI.Api**; ferramenta local **dotnet-ef** em `.config/dotnet-tools.json`
- Aplicação de schema na inicialização da API via `Database.MigrateAsync()` em ambiente **Development**

## Consequências

### Positivas

- Zero instalação de servidor de banco para desenvolvimento
- Migrations versionadas no Git
- Mesmo stack EF para testes de integração com SQLite in-memory

### Negativas

- SQLite não é ideal para todos os cenários de produção (concorrência intensa, recursos avançados)
- Migração futura para PostgreSQL/SQL Server exigirá novo ADR e ajuste de provider

## Convenções

- Tabelas em snake_case no banco (`todo_items`) quando configurado no mapping
- Repositórios implementam interfaces definidas na Application
- Entidades de domínio não herdam de classes EF

## Referências

- `src/TodoAI.Infrastructure/Persistence/`
- `src/TodoAI.Api/appsettings.json`

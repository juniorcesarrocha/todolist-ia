# ADR-001: Adotar Clean Architecture

## Status

Aceito

## Contexto

O TodoAI precisa de uma base que separe regras de negócio de detalhes de framework (ASP.NET, EF Core), facilitando testes e evolução da API sem acoplamento circular.

## Decisão

Organizar o código em quatro projetos em `src/`:

1. **TodoAI.Domain** — entidades e lógica de domínio
2. **TodoAI.Application** — casos de uso e contratos
3. **TodoAI.Infrastructure** — persistência e adaptadores
4. **TodoAI.Api** — exposição HTTP e composição de DI

As dependências apontam sempre para dentro: Api e Infrastructure dependem de Application; Application depende apenas de Domain. Infrastructure referencia também Domain para mapear entidades no EF sem expor `DbContext` à Application.

## Consequências

### Positivas

- Testes de domínio e application sem subir servidor ou banco real
- Substituição de SQLite ou de adaptadores sem reescrever regras de negócio
- Limites claros para revisão de código e para agentes de IA

### Negativas

- Mais projetos e pastas que uma API monolítica única
- Boilerplate inicial (interfaces, DTOs, handlers)

## Referências

- [ARCHITECTURE.md](../../ARCHITECTURE.md)
- [AGENTS.md](../../AGENTS.md)

# ADR-004: Subagentes de IA para validação e code review

## Status

Accepted

## Contexto

O projeto TodoAI evolui com apoio de agentes de IA que implementam features, specs e testes. Sem gates explícitos, é fácil avançar para pull request com requisitos não cobertos, violações de Clean Architecture ou achados críticos de revisão não tratados.

Há necessidade de subagentes de validação integrados ao fluxo de trabalho no Cursor, acionáveis por comandos previsíveis e alinhados a `AGENTS.md` e `todoai-guardrails.mdc`.

## Decisão

Criar dois subagentes como regras Cursor (`.mdc`) em `.cursor/rules/`:

1. **spec-validator** (`spec-validator.mdc`) — acionado com `validate spec: [feature]`; cruza requisitos da spec com código e testes; gera relatório em `docs/validation/` e bloqueia avanço se a cobertura for insuficiente.
2. **code-reviewer** (`code-reviewer.mdc`) — acionado com `review code: [feature ou arquivo]`; revisa escopo contra convenções C#, Clean Architecture e testes; gera relatório em `docs/reviews/` e bloqueia PR se houver itens críticos.

Ambas as rules são `alwaysApply: false` e documentadas como agent-requestable em `AGENTS.md`.

## Consequências

### Positivas

- Gates automáticos de qualidade antes de abrir ou mergear PR
- Comandos padronizados para humanos e agentes (`validate spec:`, `review code:`)
- Rastreabilidade via relatórios em `docs/validation/` e `docs/reviews/`

### Negativas

- Dependência do ecossistema Cursor (rules `.mdc`) para o fluxo completo
- Manutenção das rules quando convenções ou ADRs mudarem

## Referências

- [.cursor/rules/spec-validator.mdc](../../.cursor/rules/spec-validator.mdc)
- [.cursor/rules/code-reviewer.mdc](../../.cursor/rules/code-reviewer.mdc)
- [AGENTS.md](../../AGENTS.md)
- [docs/reviews/](../reviews/)

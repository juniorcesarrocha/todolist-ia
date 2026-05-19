# ADR-002: CQRS com MediatR na camada Application

## Status

Aceito

## Contexto

A API expõe operações de leitura e escrita sobre tarefas. Misturar ambas em serviços genéricos (“TodoService”) tende a crescer sem fronteiras claras e dificulta testar cada operação isoladamente.

## Decisão

Adotar **CQRS** na camada **Application** usando **MediatR**:

- Cada operação é um `IRequest` / `IRequest<TResponse>`
- Um handler dedicado por request (`IRequestHandler<,>`)
- Pastas `Commands/` e `Queries/` por feature (ex.: `Todos/Commands/CreateTodo/`)
- A API injeta `IMediator` e delega `Send` a partir dos controllers

Registro: `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...))` em `DependencyInjection.AddApplication()`.

## Consequências

### Positivas

- Um arquivo por caso de uso; fácil localizar e testar handlers
- Extensível com pipelines (validação, logging) sem alterar controllers
- Controllers permanecem finos

### Negativas

- Curva de aprendizado MediatR para quem não conhece o padrão
- Mais tipos (command/query + handler) por endpoint

## Alternativas consideradas

- Serviços de aplicação clássicos (`ITodoService`): rejeitados neste projeto para manter simetria explícita read/write e alinhar com o escopo educacional do repositório.

## Referências

- Pacote: `MediatR` no projeto `TodoAI.Application`
- Handlers em `src/TodoAI.Application/Todos/`

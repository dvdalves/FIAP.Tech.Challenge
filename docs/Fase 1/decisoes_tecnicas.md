# Decisões Técnicas e Tecnologias Atuais - SIAES

Este documento detalha a infraestrutura técnica de base e a pilha tecnológica adotada no MVP (Fase 1) do **SIAES (Sistema Integrado de Atendimento e Execução de Serviços)**.

---

## Tecnologias Utilizadas

- **.NET 10**: A plataforma e linguagem base (C#) adotada para o desenvolvimento do projeto (versão LTS).
- **ASP.NET Core**: Framework web para desenvolvimento da API REST e endpoints administrativos e públicos do projeto.
- **Entity Framework Core (EF Core)**: ORM (Object-Relational Mapping) utilizado para mapear objetos C# para tabelas do banco de dados e gerenciar a persistência.
- **EF Core InMemory Database**: Provedor em memória utilizado para isolar o contexto do banco de dados na execução dos testes de integração do repositório e controllers.
- **SQLite**: Banco de dados relacional leve utilizado em memória como fallback dinâmico nos testes integrados locais, facilitando a execução ágil via linha de comando.
- **PostgreSQL**: Banco de dados relacional utilizado para a persistência de dados em produção e no ambiente de contêineres Docker.
- **Swagger / OpenAPI**: Ferramenta para documentação de APIs REST, permitindo a visualização e testes interativos das rotas disponíveis.
- **xUnit**: Framework de testes automatizados adotado para execução e organização da suíte de testes de unidade e integração.
- **NSubstitute**: Biblioteca para criação de mocks e dublês de testes, facilitando o isolamento de dependências nos testes da camada de aplicação.
- **FluentAssertions**: Biblioteca que adiciona legibilidade e semântica fluida para asserções e validações na suíte de testes.
- **FluentValidation**: Biblioteca de validação voltada a regras de entrada de dados de payloads nas requisições da camada de aplicação.
- **Docker**: Ferramenta para criação e gerenciamento de contêineres, permitindo a execução da aplicação e banco de dados em ambientes isolados e idênticos.
- **Docker Compose**: Ferramenta para definir e executar múltiplos contêineres, facilitando a orquestração integrada dos serviços necessários para a inicialização do ecossistema.

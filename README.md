# Oficina Mecânica - Tech Challenge FIAP (Fase 1)

## SIAES - Sistema Integrado de Atendimento e Execução de Serviços

Este repositório contém a solução back-end para o **SIAES (Sistema Integrado de Atendimento e Execução de Serviços)**, um sistema projetado para otimizar e organizar os fluxos de trabalho de uma oficina mecânica de médio porte. O sistema abrange desde a recepção de veículos e abertura de ordens de serviço (OS) até o controle de estoque de peças, geração automática de orçamentos, aprovação do cliente e encerramento com registro de métricas.

O desenvolvimento foi estruturado seguindo os princípios de **Domain-Driven Design (DDD)**, **Clean Architecture**, testes automatizados e segurança de código.

---

## 🧭 Menu de Navegação

- [1. Como Executar a Aplicação](#1-como-executar-a-aplicação)
- [2. Arquitetura da Solução](#2-arquitetura-da-solução)
- [3. Requisitos do Sistema](#3-requisitos-do-sistema)
- [4. Decisões Técnicas Principais](#4-decisões-técnicas-principais)
- [5. Engenharia de Domínio & Documentação DDD](#5-engenharia-de-domínio--documentação-ddd)
  - [5.1. Linguagem Ubíqua](#51-linguagem-ubíqua)
  - [5.2. Documentação DDD & Diagramas](#52-documentação-ddd--diagramas)
- [6. APIs e Funcionalidades do MVP](#6-apis-e-funcionalidades-do-mvp)
- [7. Cobertura de Testes](#7-cobertura-de-testes)
- [8. Relatório de Análise de Vulnerabilidades](#8-relatório-de-análise-de-vulnerabilidades)
- [9. Plano de Evolução Arquitetural](#9-plano-de-evolução-arquitetural)

---

## 1. Como Executar a Aplicação

### 📋 Pré-requisitos

Para rodar a aplicação localmente, certifique-se de possuir instalado em sua máquina:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (com suporte ao comando `docker compose`)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Opcional, caso queira compilar/testar fora dos contêineres)

### 🚀 Inicialização via Docker Compose (Recomendado)

O ambiente completo é inicializado de forma orquestrada e resiliente. A partir da raiz do repositório, execute o comando:

```bash
docker compose up --build
```

#### 🛡️ Inicialização Resiliente (Healthcheck)

A orquestração do [docker-compose.yml](docker-compose.yml) possui um mecanismo de resiliência:

1. O banco de dados PostgreSQL 18 inicia primeiro.
2. O container da API (`api`) aguarda a sinalização de que o banco está pronto para receber conexões.
3. O healthcheck do PostgreSQL é validado via `pg_isready` a cada 5 segundos.
4. Somente após a validação bem-sucedida do banco (`service_healthy`), a compilação e inicialização do container da API são concluídas, evitando falhas de conexão de rede durante a subida.

### 🌐 Acessando as APIs e o Swagger

Assim que a inicialização for concluída, a API estará escutando na porta **`8080`**.

- Acesse no navegador: **`http://localhost:8080`**
- O sistema possui um redirecionamento inteligente no endpoint raiz (`/`). Ao acessar a URL acima, você será **automaticamente redirecionado** para o Swagger da aplicação (**`http://localhost:8080/swagger/index.html`**), onde poderá testar todos os fluxos de forma interativa.

### 🧪 Executando os Testes Localmente

Para executar a suíte completa de testes de unidade e de integração através da CLI do .NET:

```bash
dotnet test
```

---

## 2. Arquitetura da Solução

O sistema é estruturado como um **Monolito Coeso**, isolando rigorosamente a lógica de negócio dos detalhes de infraestrutura através da **Clean Architecture** e namespaces em projetos C#:

```text
FIAP.Tech.Challenge
├── src/
│   ├── FIAP.Tech.Challenge.API                 # Entry point HTTP, filtros globais, setup Swagger e JWT
│   ├── FIAP.Tech.Challenge.Application         # Casos de uso (Use Cases), DTOs e FluentValidation
│   ├── FIAP.Tech.Challenge.Domain              # Entidades Ricas, Objetos de Valor (VOs), interfaces e exceções
│   └── FIAP.Tech.Challenge.Infrastructure      # DbContext (EF Core), mapeamentos Fluent API, repositórios SQL
└── tests/
    ├── FIAP.Tech.Challenge.UnitTests           # Testes unitários focados nas regras e entidades de domínio
    └── FIAP.Tech.Challenge.IntegrationTests    # Testes integrados de endpoints e repositórios (SQLite in-memory)
```

### Isolamento de Camadas (DDD)

- **Domain**: Camada pura, sem dependência de frameworks ou bibliotecas externas. Contém entidades ricas que garantem integridade conceitual usando encapsulamento rígido (propriedades com modificadores `private set` e métodos internos de alteração de estado com validação).
- **Application**: Contém os Casos de Uso que expõem os fluxos do negócio, sanitizando os dados de entrada usando validadores e mapeando-os para DTOs.
- **Infrastructure**: Implementa os acessos ao PostgreSQL usando o Entity Framework Core (EF Core). Os mapeamentos de tabelas são definidos com a Fluent API fora das classes do Domínio para evitar acoplamento tecnológico.
- **API**: Controladores REST limpos encarregados unicamente do protocolo HTTP. Possui um filtro global de exceções para traduzir exceções de negócio em retornos HTTP apropriados (`400 Bad Request` ou `422 Unprocessable Entity`).

---

## 3. Requisitos do Sistema

O mapeamento completo dos requisitos do negócio e das especificações técnicas que orientam o desenvolvimento deste MVP está documentado na íntegra no seguinte plano detalhado:

- **[Requisitos Funcionais e Não Funcionais - requisitos.md](docs/Fase%201/requisitos.md)**

Este documento aborda:

- **Requisitos Funcionais (RF-01 a RF-12)**: Mapeamento de regras para a gestão de clientes, veículos, estoque de peças, catálogo de serviços, ciclo de vida das Ordens de Serviço (OS) e métricas operacionais.
- **Requisitos Não Funcionais (RNF-01 a RNF-12)**: Definições de arquitetura (Clean Architecture & DDD), segurança de acesso (JWT & UUID/Guid para mitigar IDOR), tolerância de falhas, integridade relacional com EF Core (PostgreSQL/SQLite) e estratégias de automação de testes.

---

## 4. Decisões Técnicas Principais

### Provedor de Banco de Dados Relacional (PostgreSQL)

Optou-se pelo **PostgreSQL 18** no ambiente de contêineres devido à sua robustez, maturidade industrial, suporte nativo a transações ACID concorrentes (essencial para o controle transacional de estoque e orçamentos da oficina) e ótima integração com o Entity Framework Core.

### 🔌 Flexibilidade nos Testes (SQLite Fallback)

Para agilizar o desenvolvimento local, o arquivo [DependencyInjectionSetup.cs](src/FIAP.Tech.Challenge.API/Configurations/DependencyInjectionSetup.cs) possui uma lógica de seleção dinâmica de banco de dados:

- Em execução padrão (Docker), a API conecta-se ao PostgreSQL.
- Caso executado localmente sem uma string de conexão PostgreSQL configurada, o sistema efetua um **fallback automático e transparente para o SQLite in-memory**. Isto permite executar a suíte de testes integrados instantaneamente com `dotnet test` sem a necessidade de subir contêineres externos de banco de dados.

### 🔒 Autenticação JWT e Segurança de Acesso

- **Segurança Administrativa**: Endpoints sob o prefixo `/api/admin/*` exigem autenticação do tipo JWT Bearer com a validação das Roles correspondentes (ex: `Admin`).
- **Endpoint de Token do MVP**: Para validação rápida das funcionalidades do MVP no Swagger, disponibilizamos a rota pública `/api/public/auth/token` para geração de tokens de testes.
- **Prevenção contra IDOR (Insecure Direct Object Reference)**: O sistema não expõe chaves primárias sequenciais inteiras (ex: `id = 1, 2, 3...`) nas URLs expostas. Todas as entidades de domínio expõem publicamente chaves baseadas em **`Guid` (UUID)**, inviabilizando tentativas de varreduras não autorizadas.

---

## 5. Engenharia de Domínio & Documentação DDD

### 5.1. Linguagem Ubíqua

Os termos utilizados no domínio da oficina mecânica e sua respectiva representação no código-fonte são:

| Termo do Negócio          | Significado Técnico                                                                                 | Classe/Código                        |
| :------------------------ | :-------------------------------------------------------------------------------------------------- | :----------------------------------- |
| **Cliente**               | Pessoa física ou jurídica que contrata a manutenção.                                                | `Cliente` (Entidade)                 |
| **Veículo**               | O carro ou utilitário do cliente que receberá a manutenção.                                         | `Veiculo` (Entidade)                 |
| **Ordem de Serviço (OS)** | O documento/agregado que rastreia o ciclo de vida do serviço.                                       | `OrdemServico` (Entidade / Agregado) |
| **Peça / Insumo**         | Componentes físicos estocados e adicionados à OS.                                                   | `Peca` (Entidade)                    |
| **Serviço / Mão de Obra** | Trabalho mecânico tabelado aplicado sobre o veículo.                                                | `Servico` (Entidade)                 |
| **Diagnóstico**           | Análise inicial do mecânico para listar danos e peças.                                              | `Diagnostico` (Objeto de Valor)      |
| **Orçamento**             | Proposta de preço calculada somando peças e mão de obra.                                            | `Orcamento` (Objeto de Valor)        |
| **Status da OS**          | Estados da OS: _Recebida, Em diagnóstico, Aguardando aprovação, Em execução, Finalizada, Entregue_. | `StatusOrdemServico` (Enum)          |

### 5.2. Documentação DDD & Diagramas

A dinâmica de negócios da oficina foi mapeada seguindo os padrões do DDD. Os diagramas em alta resolução estão disponíveis no repositório:

#### A. Domain Storytelling (Modelagem Narrativa)

Ilustra o fluxo de atendimento da oficina, representando os passos que o cliente e a equipe realizam desde a entrada do veículo até a entrega.

- **Imagem**:
  ![Domain Storytelling](docs/Fase%201/domain-storytelling.png)

#### B. Event Storming (Workshop de Eventos)

Define a linha do tempo com Comandos, Agregados, Eventos de Domínio e Políticas aplicadas ao ciclo de vida das ordens de serviço.

- **Imagem**:
  ![Event Storming](docs/Fase%201/event-storming.png)

---

## 6. APIs e Funcionalidades do MVP

O Swagger expõe os seguintes fluxos operacionais mapeados na solução:

### 👤 Fluxo do Cliente (Público - `/api/public/`)

- **Consulta de OS** (`GET /api/public/ordens-servico/{id}`): Permite ao cliente acompanhar o status de uma OS específica.
- **Descoberta de OSs Ativas** (`GET /api/public/ordens-servico`): Lista todas as ordens de serviço ativas vinculadas ao cliente autenticado (filtradas pelo token JWT).
- **Aprovação de Orçamento** (`POST /api/public/ordens-servico/{id}/aprovar`): Cliente autoriza o orçamento, alterando o status para `EmExecucao` e deduzindo as peças utilizadas do estoque de forma atômica.
- **Rejeição de Orçamento** (`POST /api/public/ordens-servico/{id}/rejeitar`): Cancela a OS (Status transiciona para `Cancelada`).
- **Token de Teste** (`POST /api/public/auth/token`): Emite JWT para testes das rotas administrativas.

### ⚙️ Fluxo Administrativo (Autenticado - `/api/admin/`)

- **Gestão de Clientes**:
  - `POST /api/admin/clientes` (Cadastra novo cliente)
  - `GET /api/admin/clientes` (Lista clientes cadastrados com suas frotas)
- **Gestão de Veículos**:
  - `POST /api/admin/clientes/{id}/veiculos` (Vincula veículo ao cliente)
- **Gestão de Ordens de Serviço**:
  - `POST /api/admin/ordens-servico` (Abre OS inicial - Status: `Recebida`)
  - `GET /api/admin/ordens-servico` (Lista todas as OSs do sistema com filtros opcionais de `status` e `clienteId`)
  - `POST /api/admin/ordens-servico/{id}/itens` (Adiciona peças e serviços a partir do catálogo, envia para `AguardandoAprovacao`)
  - `PUT /api/admin/ordens-servico/{id}/status` (Transiciona manualmente o status da OS e marca timestamps de controle de tempo)
  - `GET /api/admin/ordens-servico/metricas/tempo-medio` (Retorna o tempo médio de execução dos serviços finalizados)
- **Gestão de Serviços (Catálogo de Mão de Obra)**:
  - `GET /api/admin/servicos` (Lista o catálogo de serviços cadastrados)
  - `POST /api/admin/servicos` (Cadastra um novo serviço e seu preço padrão de mão de obra)
- **Gestão de Peças e Estoque**:
  - `GET /api/admin/pecas` (Lista catálogo e saldos em estoque)
  - `POST /api/admin/pecas` (Adiciona peça ao catálogo)
  - `PUT /api/admin/pecas/{id}/estoque` (Atualiza saldo em estoque)

---

## 7. Cobertura de Testes

Os testes foram desenvolvidos utilizando **xUnit**, **NSubstitute** (para isolamento/mocking) e **FluentAssertions** (asserções descritivas).

### Executando os Testes

Para rodar a suíte contendo **29 testes unitários** e **7 testes integrados**:

```bash
dotnet test
```

### 📈 Qualidade e Cobertura

- Os testes validam: transição correta de estados da OS, regras atômicas de cálculo de orçamentos, validação de inputs (CPF/CNPJ, placa Mercosul) e atualização transacional do estoque de peças.
- Visando focar as métricas na lógica crítica de negócio, o atributo `[ExcludeFromCodeCoverage]` foi aplicado a classes de transporte (DTOs), arquivos puramente de mapeamento ORM (Fluent API) e configurações estruturais da API.

### 📊 Integração com SonarQube

O SonarQube pode ser levantado localmente para validação de cobertura:

1. Suba o serviço: `docker compose up -d sonarqube`
2. Acesse `http://localhost:9000` (credenciais: `admin`/`admin`), crie um projeto com a chave `FIAP.Tech.Challenge` e obtenha o token de acesso.
3. Execute o script de análise na raiz:

   ```bash
   ./run-sonar.sh
   ```

---

## 8. Relatório de Análise de Vulnerabilidades

### 📝 Sumário Executivo

Como parte dos requisitos de segurança do código estático (SAST) e análise de composição de dependências (SCA), a solução foi submetida a varreduras profundas utilizando as ferramentas oficiais integradas do .NET SDK.

- **SCA (Software Composition Analysis)**: Mapeamento de dependências diretas e transitivas contra o _GitHub Advisory Database_, resultando em **zero vulnerabilidades** identificadas.
- **SAST (Static Application Security Testing)**: Compilação forçada em modo restrito de segurança utilizando as regras integradas dos _Roslyn Analyzers_ (`AnalysisLevel=latest-Security`), sem qualquer inconformidade de segurança detectada nos projetos principais de produção.

A análise detalhada dos scans de segurança executados (SCA e SAST), contendo evidências dos comandos executados e conformidade das dependências com o GitHub Advisory Database, está documentada na íntegra em:
**[Relatório de Vulnerabilidades - vulnerabilidade.md](docs/Fase%201/vulnerabilidade.md)**.

---

## 9. Plano de Evolução Arquitetural

Para detalhes sobre como o sistema evoluirá do monolito clássico atual para uma arquitetura modular de alta disponibilidade com resiliência, mensageria, observabilidade completa e segurança avançada, consulte o documento completo:

- **[Plano de Evolução Arquitetural - plano_evolucao_arquitetural.md](docs/Fase%201/plano_evolucao_arquitetural.md)**

Este plano detalha as futuras integrações e melhorias estratégicas da solução, incluindo Keycloak (IDP), .NET Aspire, Redis, RabbitMQ, Blazor WebApp, além de testes dinâmicos focados no OWASP Top 10 e o bloqueio automático de pipelines de deploy do GitHub Actions diante de vulnerabilidades críticas.

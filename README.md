# Oficina Mecânica - Tech Challenge FIAP Arquitetura de Software

## SIAES - Sistema Integrado de Atendimento e Execução de Serviços

Este repositório contém o projeto de software desenvolvido para o **Tech Challenge** do curso de Arquitetura de Software da **FIAP (Turma 15SOAT)**.

O objetivo do projeto é o desenvolvimento do back-end para o **SIAES (Sistema Integrado de Atendimento e Execução de Serviços)**, um sistema concebido para otimizar e organizar os fluxos de trabalho de uma oficina mecânica de médio porte. O sistema abrange desde a recepção de veículos e abertura de ordens de serviço (OS) até o controle de estoque de peças, geração automática de orçamentos, aprovação do cliente e encerramento com registro de métricas.

O desenvolvimento foi guiado pelos princípios de **Domain-Driven Design (DDD)**, **Clean Architecture**, segurança contra vulnerabilidades comuns (SAST) e testes automatizados.

---

## 🧭 Menu de Navegação

* [1. Como Executar a Aplicação](#1-como-executar-a-aplicação)
* [2. Arquitetura da Solução](#2-arquitetura-da-solução)
* [3. Decisões Técnicas Principais](#3-decisões-técnicas-principais)
* [4. Linguagem Ubíqua](#4-linguagem-ubíqua)
* [5. Documentação DDD & Diagramas](#5-documentação-ddd--diagramas)
* [6. APIs e Funcionalidades do MVP](#6-apis-e-funcionalidades-do-mvp)
* [7. Cobertura de Testes](#7-cobertura-de-testes)
* [8. Relatório de Análise de Vulnerabilidades](#8-relatório-de-análise-de-vulnerabilidades)

---

## 1. Como Executar a Aplicação

### 📋 Pré-requisitos

Para rodar a aplicação localmente, certifique-se de possuir instalado em sua máquina:

* [Docker Desktop](https://www.docker.com/products/docker-desktop/) (com suporte ao comando `docker compose`)
* (Opcional) [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) para compilação local fora do container

### 🚀 Inicialização via Docker Compose (Recomendado)

A inicialização do ambiente completo (API + Banco de Dados PostgreSQL) é automatizada. A partir da raiz do repositório, execute o seguinte comando no terminal:

```bash
docker compose up --build -d
```

Este comando irá:

1. Compilar os projetos .NET em múltiplos estágios otimizados (`multi-stage build` do [Dockerfile](file:///Users/david/Projects/FIAP.Tech.Challenge/src/FIAP.Tech.Challenge.API/Dockerfile)).
2. Levantar o container da API na porta **`8080`**.
3. Levantar o banco de dados PostgreSQL na porta padrão **`5432`** com as variáveis de ambiente pré-configuradas.

### 🌐 Acessando a API

Uma vez inicializada a aplicação:

* Abra o seu navegador e acesse **`http://localhost:8080`**
* O sistema possui uma rota inteligente de redirecionamento no [Program.cs](file:///Users/david/Projects/FIAP.Tech.Challenge/src/FIAP.Tech.Challenge.API/Program.cs). Ao acessar o endereço raiz (`/`), você será **automaticamente redirecionado** para a documentação interativa do **Swagger** (`/swagger/index.html`).

### 🧪 Executando os Testes Localmente

Para rodar os testes unitários e de integração fora do Docker, utilize a CLI do .NET:

```bash
dotnet test
```

---

## 2. Arquitetura da Solução

O sistema foi estruturado seguindo os princípios de **Clean Architecture**, dividindo a aplicação em camadas isoladas para garantir desacoplamento, testabilidade e agnosticismo de infraestrutura:

```text
FIAP.Tech.Challenge
├── src/
│   ├── FIAP.Tech.Challenge.API                 # Ponto de entrada REST, Filtros Globais, Setup JWT/Swagger
│   ├── FIAP.Tech.Challenge.Application         # Casos de uso (Orquestração), DTOs, Validadores (FluentValidation)
│   ├── FIAP.Tech.Challenge.Domain              # Entidades Ricas, Objetos de Valor (VOs), Interfaces, Exceções
│   └── FIAP.Tech.Challenge.Infrastructure      # Persistência (EF Core), Criptografia, Repositórios SQL
└── tests/
    ├── FIAP.Tech.Challenge.UnitTests           # Testes unitários com foco nas regras de negócio (Domain)
    └── FIAP.Tech.Challenge.IntegrationTests    # Testes integrados (Endpoints da API usando banco in-memory)
```

### Detalhamento das Camadas

1. **Domain (`Domain`)**: O coração da aplicação. Não possui dependência de frameworks externos ou bibliotecas de banco. Contém as regras cruciais de negócio do domínio de oficina mecânica. As entidades (como `OrdemServico`, `Veiculo` e `Cliente`) possuem estado com modificadores `private set` e métodos internos ricos que impedem modelos anêmicos.
2. **Application (`Application`)**: Contém os Casos de Uso (Use Cases) que coordenam o fluxo de dados entre o Domínio e a Infraestrutura. Utiliza FluentValidation para higienização dos payloads de entrada (ex: checagem de formato de placa Mercosul e estrutura de CPF/CNPJ) antes de chamar o Domínio.
3. **Infrastructure (`Infrastructure`)**: Implementa as abstrações do domínio. Contém o DbContext do Entity Framework Core, as configurações de mapeamento de tabela (Fluent API) que mantêm as classes de domínio limpas de decorações do ORM, e as implementações reais de repositórios.
4. **API (`API`)**: Controladores REST enxutos que recebem requisições HTTP, delegam aos Casos de Uso e retornam status HTTP adequados. Conta com um `Global Exception Filter` que captura automaticamente exceções do domínio (`DominioException`) e as traduz em respostas amigáveis (`400 Bad Request` ou `422 Unprocessable Entity`), sem vazar logs confidenciais da infraestrutura.

---

## 3. Decisões Técnicas Principais

### Provedor de Banco de Dados Relacional

Optamos pelo **PostgreSQL** (versão `18-alpine` mapeada no docker-compose) para produção e ambiente de contêineres devido à sua robustez operacional, conformidade total com ACID (garantindo consistência nas transações de estoque e orçamentos) e excelente integração de alta performance com o EF Core.

### 🔌 Seleção Dinâmica do Provedor de Banco (PostgreSQL / SQLite Fallback)

Desenvolvemos uma mecânica de registro de dados flexível no [DependencyInjectionSetup.cs](file:///Users/david/Projects/FIAP.Tech.Challenge/src/FIAP.Tech.Challenge.API/Configurations/DependencyInjectionSetup.cs):

* Quando a API detecta uma conexão contendo indicadores de PostgreSQL (ex: `Host=` ou `Server=`) ou a variável de ambiente `DbProvider=PostgreSQL`, ela carrega o provedor `Npgsql.EntityFrameworkCore.PostgreSQL`.
* Caso a aplicação seja executada fora do Docker ou em contextos de testes integrados nos quais nenhuma conexão é passada, o sistema faz um **fallback transparente e automático para SQLite**. Isso permite que o desenvolvedor execute a suíte de testes integrados instantaneamente com `dotnet test` usando SQLite in-memory, eliminando a dependência obrigatória de subir contêineres Docker para fins de teste local rápido.

### 🔒 Segurança de Acesso e APIs

* **Autenticação JWT Bearer**: Os endpoints sensíveis de administração (cadastro de peças, CRUDs de clientes e gerenciamento de faturamento) são protegidos por tokens JWT assinados digitalmente.
  * **Emissão no MVP**: Para fins de teste e validação do MVP do produto, disponibilizamos o endpoint público `POST /api/public/auth/token` para geração rápida de tokens temporários com perfis customizados (ex: `Admin`).
  * **Algoritmo e Chave de Assinatura**: O token é assinado localmente com chave simétrica usando o segredo de validação acadêmica `SuperSecretSecurityKeyOficinaMecanica2026!` via algoritmo `HMAC-SHA256`.
  * **Validação**: A API valida a integridade do token por meio do middleware oficial do ASP.NET Core `Microsoft.AspNetCore.Authentication.JwtBearer` (configurado em `JwtSetup.cs`).
  * **Rotas Protegidas**: Todas as rotas administrativas sob o prefixo `/api/admin/*` exigem o cabeçalho HTTP `Authorization: Bearer <seu_token>`.
  * **Roles e Perfis**: O middleware extrai as Claims de perfil (ex: `Admin`) mapeando as permissões de acesso de forma granular através do atributo `[Authorize]`.
  * **Evolução de Arquitetura**: O plano de evolução da infraestrutura de segurança (IdP/Keycloak), desacoplamento com mensageria e escalabilidade de banco de dados está detalhado em: [docs/Fase 1/arquitetura_futura.md](file:///Users/david/Projects/FIAP.Tech.Challenge/docs/Fase%201/arquitetura_futura.md).
* **Prevenção contra IDOR (Insecure Direct Object Reference)**: O sistema não expõe chaves primárias sequenciais do banco de dados (ex: `id = 1, 2, 3...`) nas URLs expostas publicamente. Em vez disso, utilizamos identificadores globais do tipo **`Guid` (UUID)** de forma nativa para todas as referências públicas das entidades de domínio.

---

## 4. Linguagem Ubíqua

A tabela abaixo mapeia os termos de negócio utilizados na oficina mecânica e suas respectivas implementações no código-fonte da aplicação:

| Termo do Negócio | Significado Técnico | Classe/Código Correspondente |
| :--- | :--- | :--- |
| **Cliente** | Pessoa física ou jurídica que contrata a manutenção do veículo. | `Cliente` (Entidade) |
| **Veículo** | O carro ou utilitário do cliente que receberá diagnóstico ou manutenção. | `Veiculo` (Entidade) |
| **Ordem de Serviço (OS)** | O documento que rastreia todo o ciclo de vida do veículo dentro da oficina. | `OrdemServico` (Entidade / Agregado) |
| **Peça / Insumo** | Componentes físicos estocados e adicionados à OS (ex: pastilha de freio, filtro). | `Peca` (Entidade de estoque) |
| **Serviço / Mão de Obra** | Trabalho mecânico tabelado aplicado sobre o veículo (ex: troca de óleo). | `Servico` (Entidade) |
| **Diagnóstico** | Análise técnica inicial feita pelo mecânico para listar danos e peças. | `Diagnostico` (Objeto de Valor) |
| **Orçamento** | Proposta de preço gerada automaticamente somando peças e mão de obra. | `Orcamento` (Objeto de Valor) |
| **SIAES** | Sistema Integrado de Atendimento e Execução de Serviços (Back-end/API). | `FIAP.Tech.Challenge.API` |
| **Status da OS** | Estados da OS: *Recebida, Em diagnóstico, Aguardando aprovação, Em execução, Finalizada, Entregue*. | `StatusOrdemServico` (Enum) |

---

## 5. Documentação DDD & Diagramas

Alinhado às melhores práticas de **DDD (Domain-Driven Design)**, a dinâmica de negócios da oficina foi mapeada por meio de duas ferramentas principais. Os diagramas oficiais estão disponíveis para consulta no repositório:

### A. Domain Storytelling (Modelagem Narrativa do Domínio)

O fluxo de atendimento da oficina foi desenhado usando a linguagem de atores e objetos de trabalho do Domain Storytelling. Ele ilustra os 12 passos da narrativa desde o contato inicial do cliente com o atendente até o encerramento do serviço.

* **Arquivo Gráfico SVG**: [docs/Fase 1/domain-storytelling.svg](file:///Users/david/Projects/FIAP.Tech.Challenge/docs/Fase%201/domain-storytelling.svg)
* **Visualização Completa**:
  
  ![Domain Storytelling](docs/Fase%201/domain-storytelling.png)

### B. Event Storming (Workshop de Eventos e Políticas)

A linha do tempo do ciclo de vida das ordens de serviço modelada horizontalmente em dominós de causa e efeito (Comandos, Agregados, Eventos de Domínio e Políticas):

* **Arquivo Gráfico SVG**: [docs/Fase 1/event-storming.svg](file:///Users/david/Projects/FIAP.Tech.Challenge/docs/Fase%201/event-storming.svg)
* **Visualização Completa**:
  
  ![Event Storming](docs/Fase%201/event-storming.png)

---

## 6. APIs e Funcionalidades do MVP

O Swagger fornece a documentação de todos os endpoints mapeados no back-end. A API expõe os seguintes fluxos operacionais:

### 👤 Fluxo do Cliente (Público - `api/public/`)

* **Consulta de OS** (`GET /api/public/ordens-servico/{id}`): Consulta o status de sua manutenção em tempo real.
* **Aprovação de Orçamento** (`POST /api/public/ordens-servico/{id}/aprovar`): Cliente autoriza o orçamento, alterando o status para `EmExecucao` e deduzindo automaticamente as quantidades de peças utilizadas do estoque.
* **Rejeição de Orçamento** (`POST /api/public/ordens-servico/{id}/rejeitar`): Cliente recusa o orçamento, cancelando a OS (Status transiciona para `Cancelada`).
* **Autenticação de Teste** (`POST /api/public/auth/token`): Emissão de token JWT para testar as rotas administrativas protegidas.

### ⚙️ Fluxo Administrativo (Autenticado via JWT Bearer - `api/admin/`)

* **Gestão de Clientes**:
  * `POST /api/admin/clientes` (Cadastra novo cliente).
  * `GET /api/admin/clientes` (Lista todos os clientes).
  * `GET /api/admin/clientes/{id}` (Consulta cliente por ID).
* **Gestão de Veículos**:
  * `POST /api/admin/clientes/{id}/veiculos` (Vicula um veículo à frota do cliente).
* **Gestão de Ordens de Serviço**:
  * `POST /api/admin/ordens-servico` (Abre nova OS inicial - Status: `Recebida`).
  * `PUT /api/admin/ordens-servico/{id}/status` (Transiciona status da OS manualmente).
  * `POST /api/admin/ordens-servico/{id}/itens` (Mecânico insere peças e serviços ao diagnóstico da OS, recalculando o orçamento total automaticamente e enviando para `AguardandoAprovacao`).
* **Gestão de Peças / Catálogo**:
  * `GET /api/admin/pecas` (Lista catálogo e quantidades em estoque).
  * `POST /api/admin/pecas` (Adiciona novas peças ao catálogo).
  * `HttpPut /api/admin/pecas/{id}/estoque` (Atualiza a quantidade de saldo em estoque da peça).

---

## 7. Cobertura de Testes

Os testes automatizados foram criados utilizando **xUnit**, **NSubstitute** (para isolamento e criação de mocks de repositórios e serviços) e **FluentAssertions** (para asserções fluidas).

A suíte de testes cobre os domínios críticos do sistema, com foco nas seguintes validações:

1. **Regras de Negócio de Ordem de Serviço**: Transição correta de status (ex: uma OS só pode ir para `EmExecucao` após aprovada; bloqueio de alteração de orçamentos se a OS já foi encerrada).
2. **Cálculos Matemáticos**: Somatório automático dos preços de peças do estoque somados ao custo das mãos de obra no momento em que o diagnóstico é fechado pelo mecânico.
3. **Consistência de Estoque**: Dedução correta de saldo de estoque apenas quando a OS é aprovada para execução.
4. **Validação de Inputs**: Checagem de formato de placa de veículos e estruturas de CPF/CNPJ.

Para rodar a suíte de testes (que automaticamente inicializa um banco de dados SQLite em memória para total isolamento e performance), execute o comando a partir do diretório raiz:

```bash
dotnet test
```

Isso executará os **29 testes unitários** de domínio e os **7 testes integrados** de ponta a ponta (verificando fluxos completos de criação de clientes, frotas de veículos, abertura de OS, cálculo automático de orçamento no diagnóstico e baixa transacional de estoque).

---

## 8. Relatório de Análise de Vulnerabilidades

Como parte das exigências de segurança de código estático (SAST) e alinhamento com mitigação de riscos do OWASP Top 10, a solução foi blindada nos seguintes pilares:

* **Remediação de Segurança de Dependências (Alerta NU1903)**: Corrigida a vulnerabilidade crítica de truncamento numérico (CVE-2025-6965 / GHSA-2m69-gcr7-jv3q) associada ao motor SQLite antigo (`SQLitePCLRaw.lib.e_sqlite3` v2.1.11). Adicionamos referências explícitas ao bundle seguro `SQLitePCLRaw.bundle_e_sqlite3` v3.0.3 em todos os projetos e suítes de teste, eliminando todos os alertas de dependências vulneráveis do compilador NuGet.
* **Mitigação de IDOR (Insecure Direct Object Reference)**: Todas as APIs públicas e administrativas utilizam identificadores globais aleatórios do tipo `Guid` (UUID) em vez de chaves primárias sequenciais inteiras, inviabilizando varreduras automatizadas e acessos cruzados não autorizados.
* **Prevenção a SQL Injection**: Todas as queries e persistências utilizam o Entity Framework Core (ORM) parametrizando as operações automaticamente tanto no PostgreSQL quanto no SQLite local.
* **Proteção contra Broken Object Level Authorization (BOLA)**: Implementada validação de escopo na abertura de OS (`AbrirOrdemServicoUseCase`) para garantir que o veículo pertence de fato ao cliente que está abrindo a OS, impedindo fraude de vínculo de frotas.
* **Segurança de APIs**: Proteção de rotas administrativas sob token criptográfico JWT Bearer com chaves simétricas de assinatura de 256 bits.

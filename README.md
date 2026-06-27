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
<!-- * [6. APIs e Funcionalidades do MVP](#6-apis-e-funcionalidades-do-mvp)
* [7. Cobertura de Testes](#7-cobertura-de-testes)
* [8. Relatório de Análise de Vulnerabilidades](#8-relatório-de-análise-de-vulnerabilidades) -->

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

```
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

<!-- ## 6. APIs e Funcionalidades do MVP

O Swagger fornece a documentação de todos os endpoints mapeados no back-end. A API do MVP expõe os seguintes fluxos operacionais:

### 👤 Fluxo do Cliente (Público)

* **Consulta de OS**: Permite ao cliente verificar o status de sua manutenção informando o Guid da OS e a placa do veículo (`GET /api/ordens-servico/consulta`).
* **Aprovação de Orçamento**: Permite ao cliente interagir diretamente com a proposta enviada, aprovando ou reprovando o orçamento (`POST /api/ordens-servico/{id}/aprovacao`).

### ⚙️ Fluxo Administrativo (Autenticado via JWT Bearer)

* **Autenticação**: Emissão de token JWT para usuários administrativos autorizados (`POST /api/auth/login`).
* **Abertura de OS**: Criação da Ordem de Serviço inicial pelo atendente (`POST /api/ordens-servico`).
* **Diagnóstico e Orçamento**: Lançamento das peças e serviços executados pelo mecânico (`PUT /api/ordens-servico/{id}/diagnostico`).
* **Atualização de Status**: Modificação manual ou de segurança dos status de execução, conclusão de reparos e entrega física (`PUT /api/ordens-servico/{id}/status`).
* **CRUDs Bases**: Gestão administrativa de Clientes, Veículos, Catálogo de Peças (Estoque) e Catálogo de Serviços.

---

## 7. Cobertura de Testes

Os testes automatizados foram criados utilizando **xUnit**, **NSubstitute** (para isolamento e criação de mocks rápidos de repositórios e serviços de infraestrutura) e **FluentAssertions** (para asserções fluidas em português e inglês).

A suíte de testes cobre mais de **80%** dos domínios críticos do sistema, com foco nas seguintes validações:

1. **Regras de Negócio de Ordem de Serviço**: Transição correta de status (ex: uma OS só pode ir para "Em execução" após aprovada; bloqueio de alteração de orçamentos se a OS já foi encerrada).
2. **Cálculos Matemáticos**: Somatório automático dos preços de peças do estoque somados ao custo das mãos de obra no momento em que o orçamento é fechado pelo mecânico.
3. **Validação de Inputs**: Checagem rigorosa de formato de placas e estruturas de identificadores nacionais.

---

## 8. Relatório de Análise de Vulnerabilidades

Como parte das exigências de segurança e qualidade do Tech Challenge da FIAP, a solução foi submetida a análises de segurança estática (**SAST**). O relatório completo contendo o diagnóstico de segurança da aplicação está contido no arquivo oficial de entrega:

* **Documento de Entrega PDF**: [docs/Fase 1/15SOAT - Fase 1 - Tech Challenge.pdf](file:///Users/david/Projects/FIAP.Tech.Challenge/docs/Fase%201/15SOAT%20-%20Fase%201%20-%20Tech%20Challenge.pdf)

Nesse relatório, detalham-se os scans efetuados nas dependências e os padrões aplicados para mitigar riscos de OWASP Top 10 (como injeção de SQL, quebras de autenticação e falhas de controle de acesso indireto/IDOR). -->

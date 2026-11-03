# Oficina Mecânica - Tech Challenge FIAP (Fase 3)

## SIAES - Sistema Integrado de Atendimento e Execução de Serviços

Este repositório contém a aplicação principal do **SIAES (Sistema Integrado de Atendimento e Execução de Serviços)**, uma API .NET para gestão de uma oficina mecânica. O código segue Clean Architecture e DDD, utiliza PostgreSQL e possui execução local orquestrada pelo Aspire.

## 🏛️ Estrutura de Repositórios da Fase 3

Atendendo aos requisitos obrigatórios da Fase 3, o projeto está estruturado em **quatro repositórios independentes**, cada um com sua esteira de **CI/CD** e automação:

| Repositório                                                                                                    | Descrição                                                                                                                | Tecnologias                                                                        | CI/CD          |
| :------------------------------------------------------------------------------------------------------------- | :----------------------------------------------------------------------------------------------------------------------- | :--------------------------------------------------------------------------------- | :------------- |
| [**`FIAP.Tech.Challenge.Lambda`**](https://github.com/dvdalves/FIAP.Tech.Challenge.Lambda)                     | Function Serverless para validação de CPF (Módulo 11), consulta ao banco e emissão de tokens JWT.                        | .NET 10, C# 13, Azure Container Apps, API Management v2, Npgsql, Docker, Terraform | GitHub Actions |
| [**`FIAP.Tech.Challenge.Infra.Kubernetes`**](https://github.com/dvdalves/FIAP.Tech.Challenge.Infra.Kubernetes) | IaC para provisionamento de VNet zone-redundant, Cluster Azure AKS v1.30, Node Groups, API Management central e Datadog. | Terraform >= 1.5.0, Azure AKS, Azure VNet, Helm, Datadog Agent, Metrics Server     | GitHub Actions |
| [**`FIAP.Tech.Challenge.Infra.Database`**](https://github.com/dvdalves/FIAP.Tech.Challenge.Infra.Database)     | IaC para provisionamento do Azure Database for PostgreSQL 16, Key Vault e scripts SQL versionados de DDL e Seed.         | Terraform >= 1.5.0, Azure Database for PostgreSQL 16, Azure Key Vault, SQL         | GitHub Actions |
| [**`FIAP.Tech.Challenge`**](https://github.com/dvdalves/FIAP.Tech.Challenge) _(este repositório)_              | Aplicação principal em contêineres executando no Kubernetes (Clean Architecture & DDD).                                  | .NET 10, C# 13, EF Core, Kubernetes, Kustomize, HPA, SonarQube                     | GitHub Actions |

---

## 🧭 Menu de Navegação

- [1. Documentação Arquitetural Completa (Fase 3)](#1-documentação-arquitetural-completa-fase-3)
- [2. Arquitetura da Aplicação (Clean Architecture & DDD)](#2-arquitetura-da-aplicação-clean-architecture--ddd)
- [3. Como Executar a Aplicação](#3-como-executar-a-aplicação)
  - [3.1. Execução Local via Aspire (Recomendado)](#31-execução-local-via-aspire-recomendado)
  - [3.2. Execução Local via Docker Compose (Alternativa)](#32-execução-local-via-docker-compose-alternativa)
  - [3.3. Deploy em Cluster Kubernetes (K8s)](#33-deploy-em-cluster-kubernetes-k8s)
  - [3.4. Execução dos Testes Automatizados](#34-execução-dos-testes-automatizados)
- [4. Orquestração e Escalabilidade (Kubernetes & HPA)](#4-orquestração-e-escalabilidade-kubernetes--hpa)
- [5. Monitoramento e Observabilidade (Datadog & Azure Monitor)](#5-monitoramento-e-observabilidade-datadog--azure-monitor)
- [6. Qualidade, Testes e SonarQube](#6-qualidade-testes-e-sonarqube)
- [7. Coleções de Teste de API (Postman & Swagger)](#7-coleções-de-teste-de-api-postman--swagger)

## 1. Documentação Arquitetural Completa (Fase 3)

A documentação arquitetural exigida pela Fase 3 está organizada no diretório [`docs/Fase 3/`](docs/Fase%203/):

- 📘 [**Documentação Arquitetural Completa (Master)**](docs/Fase%203/documentacao_arquitetural.md): Hub unificado contendo o diagrama de componentes na nuvem, diagramas de sequência de autenticação e abertura de OS, topologia de rede e checklist de conformidade.
- 🗄️ [**Modelagem do Banco de Dados Relacional & Justificativa Técnica**](docs/Fase%203/modelagem_banco_dados.md): Justificativa formal da escolha do PostgreSQL 16 no Azure Database for PostgreSQL, Diagrama Entidade-Relacionamento (ERD) completo, dicionário de dados e análise de transações ACID.
- 📄 **RFCs (Request for Comments) - Decisões Técnicas**:
  - [RFC 001: Escolha do Provedor de Nuvem Pública (Azure)](docs/Fase%203/rfcs/RFC-001-escolha-do-provedor-de-nuvem-azure.md)
  - [RFC 002: Escolha do Banco de Dados Gerenciado (PostgreSQL no PostgreSQL Flexível)](docs/Fase%203/rfcs/RFC-002-escolha-do-banco-de-dados-gerenciado-postgresql-azure.md)
  - [RFC 003: Estratégia de Autenticação Serverless por CPF e Emissão de JWT](docs/Fase%203/rfcs/RFC-003-estrategia-de-autenticacao-serverless-jwt.md)
- 📐 **ADRs (Architecture Decision Records) - Decisões Permanentes**:
  - [ADR 001: Segregação em 4 Repositórios Independentes com CI/CD Desacoplado](docs/Fase%203/adrs/ADR-001-segregacao-em-quatro-repositorios-e-cicd.md)
  - [ADR 002: Padrão de Comunicação e Roteamento via Azure API Management](docs/Fase%203/adrs/ADR-002-padrao-de-comunicacao-e-roteamento-api-management.md)
  - [ADR 003: Escalabilidade Dinâmica com HPA e Metrics Server](docs/Fase%203/adrs/ADR-003-escalabilidade-dinamica-com-hpa-e-metrics-server.md)
  - [ADR 004: Estratégia de Observabilidade, Tracing Distribuído e Logs Estruturados](docs/Fase%203/adrs/ADR-004-estrategia-de-observabilidade-e-monitoramento.md)

---

## 2. Arquitetura da Aplicação (Clean Architecture & DDD)

A aplicação principal é organizada como um monolito coeso, seguindo Clean
Architecture e Domain-Driven Design (DDD):

```text
src/
├── FIAP.Tech.Challenge.Domain/         # Entidades, agregados, VOs e regras de domínio
├── FIAP.Tech.Challenge.Application/    # Casos de uso, DTOs, mapeamentos e validadores
├── FIAP.Tech.Challenge.Infrastructure/ # EF Core, Npgsql, repositórios e persistência
└── FIAP.Tech.Challenge.API/            # Controllers REST, autenticação e Swagger
tests/
├── FIAP.Tech.Challenge.UnitTests/      # Testes unitários
└── FIAP.Tech.Challenge.IntegrationTests/# Testes de integração
```

## 3. Como Executar a Aplicação

### Pré-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) para executar o PostgreSQL gerenciado pelo Aspire.
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
- [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling).

### 3.1. Execução Local via Aspire (Recomendado)

O Aspire gerencia a API, o PostgreSQL, as migrations, os health checks e o
dashboard de observabilidade local.

Na primeira configuração da máquina, grave os secrets do AppHost:

```bash
aspire secret set Parameters:postgres-password "senha-local-do-postgres"
aspire secret set Parameters:jwt-secret "chave-local-do-aspire"
```

Esses comandos são executados uma única vez. Depois disso, basta iniciar o
ambiente:

```bash
aspire start --apphost src/FIAP.Tech.Challenge.AppHost
```

### 3.2. Execução Local via Docker Compose (Alternativa)

Se preferir não usar o Aspire, o Docker Compose também inicia a API e o
PostgreSQL:

```bash
docker compose up --build
```

- **API & Swagger**: `http://localhost:8080`.
- **Health Check**: `http://localhost:8080/health`.
- **SonarQube**: `http://localhost:9000`.

### 3.3. Deploy em Cluster Kubernetes (K8s)

Os manifestos declarativos estão localizados no diretório [`k8s/`](k8s/) e são orquestrados via Kustomize:

```bash
kubectl apply -k k8s/
kubectl get pods,services,hpa -n oficina
```

Para acessar a API exposta pelo NodePort do Kubernetes:

- **URL**: `http://localhost:30080` (ou IP do Node no NodePort `30080`).

### 3.4. Execução dos Testes Automatizados

```bash
dotnet test --verbosity normal
```

## 4. Orquestração e Escalabilidade (Kubernetes & HPA)

Os manifestos em [`k8s/`](k8s/) garantem alta disponibilidade e escalabilidade dinâmica:

- **Zero Downtime**: Estratégia `RollingUpdate` garantindo substituição progressiva de pods.
- **Probes de Confiabilidade**: `livenessProbe` e `readinessProbe` no endpoint `/health`.
- **Gerenciamento de Recursos**: Definição de `requests` (`cpu: 100m`, `memory: 128Mi`) e `limits` (`cpu: 500m`, `memory: 512Mi`).
- **Horizontal Pod Autoscaler (HPA)**:
  - Escala automaticamente de **2 réplicas mínimas** até **5 réplicas máximas**.
  - Gatilhos de autoescalabilidade: **70% de consumo médio de CPU** ou **80% de consumo médio de memória**.
- **Configuração Segura**: Segredos de banco e JWT isolados em `Secret` e parâmetros em `ConfigMap`.

---

## 5. Monitoramento e Observabilidade (Datadog & Azure Monitor)

Atendendo aos requisitos da **Fase 3**, o monitoramento contempla:

- **Latência das APIs**: Medição dos percentis p50, p95 e p99 para rotas de abertura de OS e autenticação.
- **Consumo de Recursos**: Alertas de saturação de CPU e memória em nós e pods.
- **Logs Estruturados (JSON)**: Ingestão de logs contendo correlação entre requisições via `TraceId` e `SpanId`.
- **Dashboards Operacionais no Datadog**:
  - Volume diário de ordens de serviço (abertas, aprovadas e finalizadas);
  - Tempo médio de execução por status (Diagnóstico, Execução e Finalização);
  - Erros e falhas nas integrações (taxas 4xx e 5xx).

---

## 6. Qualidade, Testes e SonarQube

- **Testes Automatizados**: 100% de cobertura nos fluxos críticos de negócio.
- **Análise Estática de Código (SAST & SCA)**: Zero vulnerabilidades e code smells.
- **Execução do SonarQube Local**:

  ```bash
  # 1. Subir o SonarQube
  docker compose up -d sonarqube

  # 2. Executar script de análise com envio de métricas de cobertura
  ./run-sonar.sh
  ```

---

## 7. Coleções de Teste de API (Postman & Swagger)

- **Swagger Interativo**: Disponível em `http://localhost:8080/swagger` com suporte completo a Bearer JWT.
- **Postman Collection**: Arquivo [`OficinaMecanica.postman_collection.json`](OficinaMecanica.postman_collection.json) contendo o fluxo completo de testes sequenciais.
- **API Reference**: Guia detalhado de endpoints em [`docs/api_reference.md`](docs/api_reference.md)..

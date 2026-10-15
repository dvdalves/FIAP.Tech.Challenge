# ADR 001: Segregação em 4 Repositórios Independentes com CI/CD Desacoplado

| Metadado | Detalhe |
| :--- | :--- |
| **Status** | **Aprovado** |
| **Data** | Outubro / 2026 |
| **Decisores** | Time de Engenharia e Arquitetura SIAES |
| **Contexto** | Requisito Obrigatório da Fase 3 - Estrutura de Repositórios e CI/CD |

---

## 1. Contexto

Na Fase 2 do projeto, o código-fonte da aplicação (.NET 10), os manifestos do Kubernetes (`/k8s`) e os scripts de Terraform (`/infra`) residiam em um único repositório (*Monorepo*). Essa estrutura apresentava desvantagens operacionais:
- Qualquer alteração em scripts de infraestrutura disparava a esteira completa de build e testes da aplicação .NET.
- A esteira de deploy misturava permissões de cluster, instâncias de banco e código de aplicação.
- Falta de governança e controle de versão independente para o ciclo de vida da infraestrutura vs código de negócio.

Os requisitos da **Fase 3** exigem a organização do projeto em **quatro repositórios separados**, cada um com sua esteira de **CI/CD independente** (GitHub Actions):
1. **Lambda (Function Serverless de Autenticação)**
2. **Infraestrutura Kubernetes (Terraform)**
3. **Infraestrutura do Banco de Dados Gerenciado (Terraform)**
4. **Aplicação Principal executando em Kubernetes**

---

## 2. Decisão

Decidimos segregar a arquitetura em 4 repositórios Git distintos e auto-contidos:

| Repositório | Propósito | Tecnologias Principais |
| :--- | :--- | :--- |
| **`FIAP.Tech.Challenge.Lambda`** | Function Serverless para validação de CPF e emissão de tokens JWT. | .NET 10, AWS Lambda, API Gateway HTTP v2, Npgsql, Docker, Terraform. |
| **`FIAP.Tech.Challenge.Infra.Kubernetes`** | Provisionamento da VPC, Subnets Multi-AZ, Cluster EKS, Node Groups, API Gateway central e stack de observabilidade. | Terraform, AWS EKS, AWS VPC, Helm, Datadog Agent, Metrics Server. |
| **`FIAP.Tech.Challenge.Infra.Database`** | Provisionamento do RDS PostgreSQL, DB Subnet Group, Security Groups, Secrets Manager e versionamento de DDL/Seeds SQL. | Terraform, AWS RDS PostgreSQL 16, AWS Secrets Manager, ANSI SQL. |
| **`FIAP.Tech.Challenge`** | Núcleo de regras de negócio, Clean Architecture, APIs REST, testes e manifestos de orquestração K8s/HPA. | .NET 10, C# 13, EF Core, Kubernetes, Kustomize, Docker Compose, SonarQube. |

### Regras de Governança Aplicadas a Todos os Repositórios:
- **Proteção de Branch**: As branches `main` e `master` são protegidas contra commits diretos, exigindo obrigatoriamente a abertura de **Pull Requests (PR)**.
- **Validação Automatizada Obrigatória**: O merge em `main` exige sucesso em todos os checks da pipeline CI (build, testes unitários, formatação e validação do Terraform).
- **Deploy Automático (CD)**: Ao realizar o merge na branch `main`, a esteira correspondente executa automaticamente o deploy na nuvem.
- **Acesso Docente**: Inclusão e confirmação do usuário `soat-architecture` como colaborador com permissão de leitura em todos os repositórios.

---

## 3. Consequências

### Consequências Positivas
- **Redução do Raio de Impacto (*Blast Radius*)**: Erros em scripts de Terraform não afetam a compilação ou deploy da aplicação principal.
- **Esteiras Mais Rápidas e Especializadas**: A pipeline da Lambda roda apenas testes da Lambda; a pipeline do banco valida apenas scripts SQL e manifests do RDS.
- **Separação de Privilégios (Least Privilege)**: As credenciais de CI/CD para deploy de banco de dados podem ser isoladas das credenciais de deploy de contêineres na aplicação.

### Consequências Negativas / Desafios Mitigados
- **Coordenação de Dependências**: O repositório de Banco e o repositório de Kubernetes devem ser provisionados antes do deploy da aplicação no cluster. Essa ordem está documentada no guia de deploy e é garantida via referências no SSM Parameter Store e variáveis de ambiente.

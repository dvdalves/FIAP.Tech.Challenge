# RFC 003: Estratégia de Autenticação Serverless por CPF e Emissão de JWT

| Metadado | Detalhe |
| :--- | :--- |
| **Status** | **Aprovado** |
| **Data** | Outubro / 2026 |
| **Autor** | Equipe de Arquitetura SIAES - Tech Challenge FIAP |
| **Contexto** | Fase 3 - Segurança, Autenticação Serverless e API Management |
| **Decisores** | Arquiteto de Segurança, Arquiteto de Software e Tech Lead |

---

## 1. Contexto e Problema

Na Fase 2, os endpoints da aplicação possuíam autenticação baseada em JWT gerado diretamente por um endpoint embutido na própria API (`/api/public/auth/token`).

Os novos requisitos da **Fase 3** demandam:
1. **Desacoplamento e Segurança na Borda**:
   - Rotas sensíveis da oficina (ex: abertura e consulta de ordens de serviço por clientes) devem ser protegidas por autenticação via CPF.
2. **Adoção de Arquitetura Serverless**:
   - Criação de uma **Function Serverless (Azure Container Apps)** dedicada para:
     - Validar o algoritmo matemático do CPF do cliente;
     - Consultar a existência e o status do cliente na base de dados gerenciada;
     - Gerar e devolver um token JWT assinado para consumo das APIs protegidas.
3. **Ponto Único de Entrada com API Management**:
   - Implementação de um **API Management** na nuvem para controlar rotas, segurança, CORS e vazão.

---

## 2. Alternativas Analisadas

| Abordagem | Descrição | Prós | Contras |
| :--- | :--- | :--- | :--- |
| **1. Manter Autenticação no Monolito K8s** | O próprio pod da API .NET valida o CPF e emite o token. | Menor número de componentes externos. | Violação do requisito obrigatório da Fase 3; consome recursos dos pods do Kubernetes com tráfego de autenticação. |
| **2. Azure Cognito User Pools** | Serviço gerenciado de identidade da Azure. | Gerencia usuários, MFA e recuperação de senha. | Custo desnecessário para o caso de uso simples de CPF; regras customizadas de validação de CPF exigiriam gatilhos Lambda de qualquer forma. |
| **3. Function Serverless Dedicada (Azure Container Apps) + API Management** *(Escolhida)* | Função leve em C# .NET 10 exposta via Azure API Management HTTP API v2. | Atende 100% aos requisitos da Fase 3; custo zero quando ociosa; escalabilidade sob demanda; isolamento total de responsabilidades. | Necessidade de gerenciar a chave simétrica de assinatura e conexão com o banco. |

---

## 3. Arquitetura da Solução Aprovada

A solução foi estruturada em torno de três componentes desacoplados:

1. **Azure API Management (HTTP API v2)**:
   - Exposição das rotas públicas `POST /auth/token` e `POST /auth/login`.
   - Gerenciamento de políticas de CORS, limites de requisições (*throttling*) e geração de logs estruturados de acesso no Azure Monitor.
2. **Azure Container Apps (Function Serverless)**:
   - Implementada em **.NET 10** no repositório dedicado [`FIAP.Tech.Challenge.Lambda`](file:///Users/david/Projects/FIAP/FIAP.Tech.Challenge.Lambda).
   - **Etapa 1 - Validação Matemática**: Valida o comprimento (11 dígitos) e os dois dígitos verificadores (Módulo 11) via [`CpfValidator.cs`](file:///Users/david/Projects/FIAP/FIAP.Tech.Challenge.Lambda/src/FIAP.Tech.Challenge.Lambda/Services/CpfValidator.cs). CPFs inválidos ou repetidos são rejeitados de imediato com `HTTP 400 Bad Request` antes de qualquer chamada ao banco.
   - **Etapa 2 - Consulta ao Banco**: Executa query parametrizada contra o PostgreSQL PostgreSQL Flexível (`SELECT "Id", "Nome", "Cpf", "Email" FROM "Clientes" WHERE "Cpf" = @cpf`). Se o cliente não existir, retorna `HTTP 404 Not Found`.
   - **Etapa 3 - Assinatura e Emissão de Token**: Gera um token JWT com algoritmo HMAC-SHA256, validade configurável (padrão: 2 horas) e as claims:
     - `sub` / `NameIdentifier`: `cliente.Id` (GUID)
     - `name` / `Name`: `cliente.Cpf` (utilizado pelos controllers da API principal para resolver o cliente autenticado)
     - `client_name`: `cliente.Nome`
     - `cpf`: `cliente.Cpf`
     - `email`: `cliente.Email`
     - `role`: `"Cliente"`
     - `status`: `"Ativo"`
3. **Consumo na Aplicação Principal (AKS)**:
   - A API principal recebe o cabeçalho `Authorization: Bearer <TOKEN>` nas rotas `/api/public/ordens-servico` e valida a assinatura criptográfica via middleware padrão `JwtBearer`.

---

## 4. Segurança e Governança

- **Segredo Compartilhado via Key Vault**: A chave de assinatura `JWT_SECRET_KEY` é parametrizada no Terraform e injetada nas variáveis de ambiente da Lambda e nas Secrets do Kubernetes, impedindo *hardcoding* em código.
- **Isolamento de VNet**: A Lambda pode ser alocada dentro das subnets privadas da VNet caso o PostgreSQL PostgreSQL Flexível não possua rota pública, garantindo comunicação restrita e segura.

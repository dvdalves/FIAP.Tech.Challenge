# Evolução Arquitetural Futura

Este documento descreve resumidamente os pilares planejados para a evolução do sistema Oficina Mecânica (SIAES) a fim de suportar escala de produção, alta disponibilidade e segurança corporativa.

---

## 1. Centralização de Identidade (IDP / Keycloak)

* **Evolução**: Substituir a geração interna e manual de tokens JWT da API por um **Identity Provider (IdP)** especializado de mercado (como **Keycloak** ou **Auth0**).
* **Motivo**: Centralizar o controle de acessos corporativo, permitir Single Sign-On (SSO), federação de usuários e delegar a segurança crítica (como recuperação de senhas, MFA e controle de sessões) para uma solução robusta que siga padrões de mercado (OAuth 2.1 / OIDC).

---

## 2. Desacoplamento Assíncrono (Mensageria)

* **Evolução**: Introduzir um message broker ou barramento de eventos (como **RabbitMQ** ou **Apache Kafka**).
* **Motivo**: Desacoplar tarefas de longa duração e integrações secundárias da transação síncrona HTTP. Por exemplo, o envio de avisos de orçamento (e-mail, SMS ou WhatsApp) ou atualizações de faturamento passam a rodar em background através de consumidores assíncronos de filas, reduzindo a latência da API.

---

## 3. Escalabilidade de Persistência (Leitura e Escrita / CQRS)

* **Evolução**: Separar o banco de dados principal transacional (PostgreSQL) de instâncias de leitura (Read Replicas) ou bancos analíticos (NoSQL / DW).
* **Motivo**: Evitar concorrência de recursos. À medida que o volume de dados e relatórios cresce, as consultas pesadas de acompanhamento e BI podem ser direcionadas para as réplicas de leitura, mantendo o banco principal leve e otimizado apenas para escrita e transações críticas de ordens de serviço.

---

## 4. Orquestração e Nuvem (Kubernetes / EKS)

* **Evolução**: Migrar a infraestrutura do Docker Compose local para um orquestrador de contêineres gerenciado (**Kubernetes / AWS EKS**).
* **Motivo**: Garantir resiliência, auto-healing (reinício automático de contêineres falhos), escalabilidade automática horizontal baseada em uso de CPU/memória e facilitação de atualizações sem indisponibilidade (Rolling Updates).

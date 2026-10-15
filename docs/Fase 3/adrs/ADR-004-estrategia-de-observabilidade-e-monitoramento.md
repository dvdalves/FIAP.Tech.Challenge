# ADR 004: Estratégia de Observabilidade, Tracing Distribuído e Logs Estruturados

| Metadado | Detalhe |
| :--- | :--- |
| **Status** | **Aprovado** |
| **Data** | Outubro / 2026 |
| **Decisores** | Arquiteto de Software, SRE e Tech Lead |
| **Contexto** | Requisito Obrigatório da Fase 3 - Monitoramento e Observabilidade |

---

## 1. Contexto

A migração para uma arquitetura distribuída (AWS API Gateway + Lambda Serverless + Cluster EKS + RDS PostgreSQL) introduz complexidades operacionais:
- Requisições transitam por múltiplos componentes antes de concluir uma operação;
- Falhas transitórias no banco ou na rede privada podem causar atrasos em diagnósticos e aberturas de OS;
- A operação da oficina necessita de visibilidade em tempo real sobre gargalos, SLAs de atendimento e saúde do cluster.

A **Fase 3** exige a implementação de integração com ferramentas como **Datadog** ou **New Relic**, contemplando:
- Monitoramento de latência das APIs;
- Consumo de recursos de nós e pods (CPU, memória);
- Healthchecks e uptime;
- Alertas para falhas no processamento de ordens de serviço;
- Logs estruturados em formato JSON com correlação entre requisições;
- Dashboards com volume diário de ordens de serviço, tempo médio de execução por status (Diagnóstico, Execução, Finalização) e taxas de erros.

---

## 2. Decisão

Adotamos uma abordagem híbrida de observabilidade unificada centralizada no **Datadog** com suporte complementar do **AWS CloudWatch**:

### 1. Plataforma Central de Observabilidade: Datadog
- Provisionado via Helm Chart oficial no namespace `monitoring` do cluster Kubernetes ([`FIAP.Tech.Challenge.Infra.Kubernetes/observability.tf`](file:///Users/david/Projects/FIAP/FIAP.Tech.Challenge.Infra.Kubernetes/observability.tf)).
- Módulos Ativados:
  - **Datadog APM & Distributed Tracing**: Instrumentação das requisições HTTP entre o API Gateway e a API .NET 10, injetando e propagando os cabeçalhos W3C `traceparent` e `tracestate` para correlação completa de spans.
  - **Datadog Log Ingestion**: Coleta automática de logs stdout/stderr dos contêineres formatados em JSON.
  - **Cluster Agent & Process Agent**: Coleta de métricas granulares de pods, nós, saturação de rede e I/O de disco.

### 2. Logs Estruturados em JSON
- A aplicação .NET 10 emite logs padronizados com campos estruturados:
  ```json
  {
    "Timestamp": "2026-10-14T22:00:00.000Z",
    "Level": "Information",
    "TraceId": "4bf92f3577b34da6a3ce929d0e0e4736",
    "SpanId": "00f067aa0ba902b7",
    "Message": "Ordem de servico aberta com sucesso",
    "OrdemServicoId": "99999999-9999-9999-9999-999999999999",
    "Status": "Recebida",
    "ClienteId": "11111111-1111-1111-1111-111111111111",
    "ValorTotal": 489.50
  }
  ```
- O `TraceId` e `SpanId` correlacionam os logs da API com a invocação da Function Serverless e a requisição no API Gateway.

### 3. Dashboards Operacionais (Métricas de Negócio e Infraestrutura)
São definidos três painéis de visualização contínua:
1. **Painel de Negócio - Fluxo de OS**:
   - Volume diário de ordens de serviço abertas, aprovadas e finalizadas.
   - Tempo médio de execução por status:
     - $\Delta t$ Médio em *Diagnóstico*;
     - $\Delta t$ Médio em *Aguardando Aprovação*;
     - $\Delta t$ Médio em *Em Execução*;
     - $\Delta t$ Total até a *Finalização*.
2. **Painel de Saúde das APIs**:
   - Latência p50, p95 e p99 dos endpoints de abertura e consulta de OS.
   - Taxa percentual de status codes (2xx, 4xx e 5xx).
   - Vazão de requisições por segundo (RPS).
3. **Painel de Infraestrutura Kubernetes**:
   - Percentual de utilização de CPU e memória em relação aos requests do HPA.
   - Quantidade de réplicas ativas por deployment.
   - Status de liveness e readiness probes dos pods.

### 4. Alarmes Automatizados
- **Alarme de Latência Excessiva**: Disparo automático no CloudWatch / Datadog se a latência p95 ultrapassar 2.000 ms por 2 períodos consecutivos de 1 minuto.
- **Alarme de Erros 5XX**: Disparo caso ocorram mais de 5 erros de servidor em 1 minuto.
- **Alarme de Gargalo de OS**: Notificação caso ordens de serviço fiquem retidas em *Diagnóstico* por mais de 48 horas úteis.

---

## 3. Consequências

### Consequências Positivas
- Resolução rápida de incidentes com correlação direta entre logs, traces e métricas na mesma interface do Datadog.
- Visibilidade em tempo real sobre os SLAs contratuais de atendimento da oficina.
- Conformidade integral com as exigências de observabilidade da Fase 3.

### Consequências Negativas / Mitigações
- **Custo de Ingestão de Telemetria**: Para conter o volume de dados faturados, aplicou-se amostragem adaptativa (*adaptive sampling*) no APM e retenção de logs de 14 dias no CloudWatch e Datadog.

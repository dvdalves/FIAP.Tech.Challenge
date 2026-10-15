# ADR 003: Escalabilidade Dinâmica com Horizontal Pod Autoscaler (HPA) e Metrics Server

| Metadado | Detalhe |
| :--- | :--- |
| **Status** | **Aprovado** |
| **Data** | Outubro / 2026 |
| **Decisores** | Arquiteto de Software, Engenheiro DevOps e Tech Lead |
| **Contexto** | Requisito Obrigatório da Fase 3 - Escalabilidade de Cluster Kubernetes |

---

## 1. Contexto

A oficina mecânica possui picos de demanda bem definidos ao longo do dia e em datas sazonais:
- Início da manhã (8h às 10h): Grande volume de abertura de Ordens de Serviço (recepção de veículos).
- Final da tarde (16h às 18h): Picos de consultas de status, aprovações de orçamento e entregas de veículos.
- Fins de semana ou períodos noturnos: Demanda residual mínima.

Manter uma quantidade fixa e superdimensionada de contêineres gera custos operacionais excessivos em nuvem. Por outro lado, manter poucos pods acarreta lentidão nas APIs, filas de requisições e degradação na experiência dos clientes.

---

## 2. Decisão

Adotamos uma estratégia de escalabilidade elástica em duas camadas complementares:

### Camada 1: Horizontal Pod Autoscaler (HPA) nos Contêineres da Aplicação
- Implementado nos manifestos do Kubernetes (`k8s/hpa.yaml`).
- Configuração de escalonamento:
  - **Mínimo de Réplicas**: 2 pods (garante alta disponibilidade mesmo durante falha súbita de uma réplica ou nó).
  - **Máximo de Réplicas**: 5 pods (suporta picos de alta demanda sem saturar recursos do cluster).
  - **Gatilhos de Escala (Métricas)**:
    - Utilização de **CPU média superior a 70%** dos requests configurados.
    - Utilização de **Memória média superior a 80%** dos requests configurados.
- **Período de Estabilização (Cooldown)**:
  - Scale Up agressivo (expansão rápida em caso de sobrecarga).
  - Scale Down com janela de estabilização de 300 segundos (5 minutos) para evitar oscilações bruscas (*flapping* de pods).

### Camada 2: Provedor de Métricas (Kubernetes Metrics Server)
- Provisionado via Terraform com Helm no repositório [`FIAP.Tech.Challenge.Infra.Kubernetes`](file:///Users/david/Projects/FIAP/FIAP.Tech.Challenge.Infra.Kubernetes/observability.tf).
- Coleta métricas de uso de CPU e memória diretamente do Kubelet em cada nó do cluster a cada 15 segundos e expõe na API `metrics.k8s.io`.

### Camada 3: EKS Managed Node Group Autoscaling
- Provisionado via Terraform (`eks_node_group.tf`).
- Limites de nós: Mínimo 1 nó, Desejado 2 nós, Máximo 5 nós EC2 `t3.medium`.
- Quando os pods em expansão pelo HPA não encontram capacidade de agendamento em nós existentes, novos nós são provisionados dinamicamente pelo grupo gerenciado da AWS.

---

## 3. Consequências

### Consequências Positivas
- **Resiliência e SLA Garantido**: Em caso de picos de abertura de ordens de serviço, a aplicação expande autonomamente sem intervenção manual.
- **Otimização de Custos de Cloud**: Em períodos de baixa utilização, a quantidade de pods é reduzida para o mínimo (2 réplicas), minimizando o consumo de vCPU e memória faturados.
- **Zero Downtime em Deployments**: Trabalhando em conjunto com `RollingUpdate`, novos deploys substituem pods sequencialmente mantendo o serviço disponível.

### Consequências Negativas / Mitigações
- **Definição Obrigatória de Requests e Limits**: O HPA só opera corretamente se os contêineres possuírem `resources.requests` e `resources.limits` explicitamente declarados. Esses limites foram padronizados no arquivo `k8s/api-deployment.yaml` (Requests: 250m CPU / 256Mi RAM; Limits: 500m CPU / 512Mi RAM).

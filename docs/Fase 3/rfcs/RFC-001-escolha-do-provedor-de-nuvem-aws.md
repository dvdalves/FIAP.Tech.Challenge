# RFC 001: Escolha do Provedor de Nuvem Pública (AWS)

| Metadado | Detalhe |
| :--- | :--- |
| **Status** | **Aprovado** |
| **Data** | Outubro / 2026 |
| **Autor** | Equipe de Arquitetura SIAES - Tech Challenge FIAP |
| **Contexto** | Fase 3 - Operação Corporativa em Nuvem |
| **Decisores** | Arquiteto de Software, Especialista em Cloud e Tech Lead |

---

## 1. Contexto e Problema

Com a expansão da rede de oficinas do **SIAES (Sistema Integrado de Atendimento e Execução de Serviços)** para múltiplas filiais e o crescimento contínuo da base de clientes e frotas atendidas, a infraestrutura on-premises / local tornou-se um gargalo operacional, apresentando riscos de indisponibilidade, sobrecarga de hardware e dificuldade de escalabilidade elástica.

A **Fase 3 do Tech Challenge** estabelece a livre escolha do provedor de nuvem pública para hospedar:
1. Um **API Gateway** central para roteamento, proteção e controle de tráfego;
2. Uma **Function Serverless** de autenticação com validação de CPF;
3. Um **Banco de Dados Gerenciado** relacional com alta disponibilidade e backup automatizado;
4. Um **Cluster Kubernetes** elástico e seguro com monitoramento de nível corporativo;
5. Suporte nativo a **Infraestrutura como Código (Terraform)** e integração com esteiras de **CI/CD**.

---

## 2. Provedores Avaliados

Foram comparados os três principais provedores de computação em nuvem do mercado:

| Critério de Avaliação | AWS (Amazon Web Services) | Microsoft Azure | Google Cloud Platform (GCP) |
| :--- | :--- | :--- | :--- |
| **Orquestração K8s** | **Amazon EKS**: Elevada maturidade, nós gerenciados com Amazon Linux, suporte amplo a CNI e addons de mercado. | **AKS**: Forte integração com Active Directory, boa gestão de nós. | **GKE**: Excelente automação de upgrades e autopilot. |
| **Serverless Functions** | **AWS Lambda**: Baixo cold start, suporte nativo a contêineres e zips .NET, integração com API Gateway HTTP v2. | **Azure Functions**: Boa integração .NET, consumo por app service plan ou consumo puro. | **Cloud Run / Functions**: Excelente para contêineres HTTP. |
| **API Gateway** | **Amazon API Gateway (HTTP API v2)**: Custo ultrabaixo, suporte a VPC Link privado para pods do EKS e CORS nativo. | **Azure API Management**: Muito completo, porém custo base elevado na camada dedicada. | **Apigee / Cloud Endpoints**: Complexidade de configuração ou custo enterprise. |
| **Banco Relacional** | **Amazon RDS PostgreSQL**: Multi-AZ com failover síncrono (< 60s), Storage Auto-scaling e Performance Insights. | **Azure Database for PostgreSQL**: Opção flexível, boa estabilidade. | **Cloud SQL**: Gerenciamento simples e boa performance. |
| **Maturidade Terraform** | **Provedor `hashicorp/aws`**: O mais maduro, estável e amplamente documentado do ecossistema IaC. | **Provedor `hashicorp/azurerm`**: Maduro, com atualizações frequentes de API. | **Provedor `hashicorp/google`**: Maduro e com cobertura abrangente. |

---

## 3. Justificativa da Escolha da AWS

A **Amazon Web Services (AWS)** foi selecionada como a plataforma de nuvem oficial do projeto pelos seguintes motivos:

1. **Eficiência no Acoplamento API Gateway + Lambda + EKS**:
   - A AWS oferece o **API Gateway HTTP API (v2)**, que combina latência mínima com custo até 70% inferior em relação a soluções REST tradicionais. O recurso de **VPC Link** permite rotear tráfego externo para pods alocados em subnets privadas do EKS sem a necessidade de expor os nós worker diretamente à Internet.
2. **Robustez do Amazon RDS PostgreSQL**:
   - Suporte nativo a **Storage Auto-scaling** (20 GiB a 50 GiB sem downtime), réplicas síncronas **Multi-AZ**, snapshots contínuos e integração direta com o **AWS Secrets Manager** para injeção segura de credenciais.
3. **Maturidade e Estabilidade do Provedor Terraform**:
   - O ecossistema Terraform para AWS possui o maior volume de módulos validados pela comunidade e ampla compatibilidade com as versões declaradas (`>= 1.5.0`), garantindo provisionamento determinístico e reprodutível.
4. **Isolamento de Segurança por Camadas (Defense in Depth)**:
   - A VPC com subnets públicas e privadas dedicadas, Security Groups granulares e IAM Roles baseadas no princípio do menor privilégio atendem integralmente aos requisitos corporativos da oficina.

---

## 4. Consequências e Trade-offs

### Pontos Positivos
- Ambiente modular e auto-recuperável em caso de falha de nós ou zonas.
- Baixo custo operacional para ambientes de teste e homologação através de instâncias da família `t3` e `t4g`.
- Totalmente desacoplado em 4 repositórios com pipelines independentes no GitHub Actions.

### Mitigação de Riscos
- **Risco de Custos Imprevistos**: Configuração de `skip_final_snapshot = true` para ambientes efêmeros, instâncias `t3.medium` e `t4g.micro`, além de scripts automatizados de `terraform destroy`.
- **Complexidade de Networking**: Padronização dos blocos CIDR (`10.0.0.0/16` para a VPC, `10.0.1.0/24` e `10.0.2.0/24` para subnets públicas, `10.0.10.0/24` e `10.0.20.0/24` para subnets privadas).

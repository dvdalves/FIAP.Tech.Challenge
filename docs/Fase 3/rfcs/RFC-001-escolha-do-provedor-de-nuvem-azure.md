# RFC 001: Escolha do Provedor de Nuvem Pública (Azure)

| Metadado | Detalhe |
| :--- | :--- |
| **Status** | **Aprovado** |
| **Data** | Novembro / 2026 |
| **Contexto** | Fase 3 - Operação Corporativa em Nuvem |
| **Decisão** | Microsoft Azure |

## Contexto

A Fase 3 exige API gateway, autenticação serverless, banco PostgreSQL
gerenciado, Kubernetes escalável, observabilidade e provisionamento via
Terraform.

## Mapeamento adotado

| Necessidade | Azure |
| :--- | :--- |
| API Gateway | Azure API Management |
| Serverless de autenticação | Azure Container Apps com escala para zero |
| Kubernetes | Azure Kubernetes Service (AKS) |
| PostgreSQL gerenciado | Azure Database for PostgreSQL Flexible Server |
| Segredos | Azure Key Vault |
| Rede | Azure Virtual Network, subnets e NAT Gateway |
| Monitoramento | Azure Monitor, Log Analytics e Datadog |
| Identidade | Managed Identity e Azure RBAC |
| IaC | Terraform com `hashicorp/azurerm` |

## Justificativa

O Azure foi escolhido por integrar nativamente AKS, API Management, Container
Apps, PostgreSQL Flexible Server, Key Vault, Monitor e Managed Identity. A
combinação mantém os requisitos de segurança, escalabilidade, alta
disponibilidade e automação da solução anterior sem dependência de serviços
proprietários do provedor anterior.

## Consequências

- Os quatro repositórios passam a autenticar no Azure com `AZURE_CREDENTIALS`.
- O cluster é acessado com `az aks get-credentials`.
- Segredos e connection strings são armazenados no Key Vault.
- A alta disponibilidade do banco usa configuração zone-redundant.
- Os workflows Terraform passam a usar o provedor `azurerm`.

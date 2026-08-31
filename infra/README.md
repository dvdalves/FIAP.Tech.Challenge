# Infraestrutura como Código (IaC) - Terraform | SIAES Oficina Mecânica

Este diretório contém os scripts **Terraform** responsáveis pelo provisionamento automatizado de toda a infraestrutura necessária para a execução do **SIAES (Fase 2)**.

---

## 🏛️ Recursos Provisionados

Os arquivos do Terraform provisionam uma infraestrutura robusta, segura e escalável:

1. **Rede (Networking / VPC)**:
   - **VPC Dedicada**: Bloco CIDR `10.0.0.0/16`.
   - **Subnets Públicas (x2)**: Distribuídas em zonas de disponibilidade distintas com tags para integração com Load Balancers externos.
   - **Subnets Privadas (x2)**: Distribuídas nas zonas de disponibilidade com tags para Load Balancers internos.
   - **Internet Gateway & Route Tables**: Roteamento público e isolamento da rede privada.

2. **Segurança e Controle de Acesso (Security Groups & IAM Roles)**:
   - **Security Group do Cluster**: Regras de controle de tráfego do control plane do Kubernetes.
   - **Security Group do Banco de Dados**: Acesso restrito na porta `5432` exclusivo a partir da rede do cluster Kubernetes.
   - **Políticas e Roles de Serviço**: Permissões granulares para os nós do cluster e integração de rede.

3. **Banco de Dados Relacional (PostgreSQL)**:
   - Instância **PostgreSQL** gerenciada.
   - Alocação elástica de armazenamento (`allocated_storage = 20Gi`, autoescalável até `50Gi`).
   - Grupo de subnets isolado na camada privada (sem exposição pública à Internet).

4. **Cluster Kubernetes**:
   - **Control Plane Kubernetes** versão `1.30`.
   - **Node Pool Gerenciado**: Instâncias distribuídas em subnets privadas com auto-recuperação e autoescalabilidade (mínimo de 1 nó, desejado 2 nós, máximo de 5 nós).

---

## 🚀 Como Aplicar a Infraestrutura

### Pré-requisitos
- [Terraform CLI](https://developer.hashicorp.com/terraform/downloads) instalado (versão >= 1.5.0).
- CLI do provedor de nuvem configurado com credenciais de provisionamento de infraestrutura.

### Passo a Passo de Execução

1. **Acessar o diretório de infraestrutura**:
   ```bash
   cd infra
   ```

2. **Configurar as variáveis**:
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   # Ajuste as variáveis no terraform.tfvars conforme seu ambiente
   ```

3. **Inicializar o Terraform**:
   ```bash
   terraform init
   ```

4. **Planejar a criação dos recursos**:
   ```bash
   terraform plan -out=tfplan
   ```

5. **Aplicar e provisionar o ambiente**:
   ```bash
   terraform apply tfplan
   ```

6. **Destruir o ambiente (quando necessário desprovisionar)**:
   ```bash
   terraform destroy -auto-approve
   ```

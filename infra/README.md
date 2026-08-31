# Infraestrutura como Código (IaC) - Terraform | SIAES Oficina Mecânica

Este diretório contém os scripts **Terraform** responsáveis pelo provisionamento automatizado de toda a infraestrutura de nuvem necessária para a execução do **SIAES (Fase 2)**.

---

## 🏛️ Recursos Provisionados

Os arquivos do Terraform provisionam uma infraestrutura robusta, segura e escalável na **AWS**:

1. **Rede (Networking / VPC)**:
   - **VPC Dedicada**: Bloco CIDR `10.0.0.0/16`.
   - **Subnets Públicas (x2)**: Distribuídas em zonas de disponibilidade distintas (`us-east-1a` e `us-east-1b`) com tags para integração com Elastic Load Balancers (`kubernetes.io/role/elb`).
   - **Subnets Privadas (x2)**: Distribuídas nas AZs com tags para Load Balancers internos (`kubernetes.io/role/internal-elb`).
   - **Internet Gateway & Route Tables**: Roteamento público e isolamento da rede privada.

2. **Segurança (Security Groups & IAM Roles)**:
   - **Security Group do EKS**: Regras de controle de tráfego do control plane.
   - **Security Group do RDS PostgreSQL**: Acesso restrito na porta `5432` exclusivo a partir da VPC do cluster EKS.
   - **IAM Roles e Políticas**:
     - `AmazonEKSClusterPolicy` para o cluster.
     - `AmazonEKSWorkerNodePolicy`, `AmazonEKS_CNI_Policy` e `AmazonEC2ContainerRegistryReadOnly` para os nós gerenciados.

3. **Banco de Dados Relacional (AWS RDS PostgreSQL)**:
   - Instância **PostgreSQL 16** gerenciada via RDS.
   - Alocação elástica de armazenamento (`allocated_storage = 20Gi`, autoescalável até `50Gi`).
   - `DB Subnet Group` isolado nas subnets privadas (sem acesso público).

4. **Cluster Kubernetes Gerenciado (AWS EKS)**:
   - **Control Plane EKS** versão `1.30`.
   - **EKS Managed Node Group**: Instâncias `t3.medium` distribuídas em subnets privadas com auto-recuperação e autoescalabilidade (mínimo de 1 nó, desejado 2 nós, máximo de 5 nós).

---

## 🚀 Como Aplicar a Infraestrutura

### Pré-requisitos
- [Terraform CLI](https://developer.hashicorp.com/terraform/downloads) instalado (versão >= 1.5.0).
- [AWS CLI](https://aws.amazon.com/cli/) instalado e autenticado com credenciais com permissão de administrador (`aws configure`).

### Passo a Passo de Execução

1. **Acessar o diretório de infraestrutura**:
   ```bash
   cd infra
   ```

2. **Configurar as variáveis**:
   ```bash
   cp terraform.tfvars.example terraform.tfvars
   # Ajuste as variáveis conforme sua preferência no terraform.tfvars
   ```

3. **Inicializar o Terraform (download dos providers AWS e Kubernetes)**:
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

6. **Conectar o `kubectl` ao cluster EKS provisionado**:
   ```bash
   aws eks update-kubeconfig --region us-east-1 --name $(terraform output -raw eks_cluster_name)
   ```

7. **Destruir o ambiente (quando necessário desprovisionar)**:
   ```bash
   terraform destroy -auto-approve
   ```

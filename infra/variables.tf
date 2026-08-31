variable "aws_region" {
  description = "Região da AWS para provisionamento da infraestrutura."
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Ambiente de execução (ex: dev, staging, prod)."
  type        = string
  default     = "production"
}

variable "cluster_name" {
  description = "Nome do Cluster Kubernetes (EKS)."
  type        = string
  default     = "oficina-eks-cluster"
}

variable "vpc_cidr" {
  description = "Bloco CIDR da VPC."
  type        = string
  default     = "10.0.0.0/16"
}

variable "db_username" {
  description = "Nome de usuário administrador do banco de dados PostgreSQL."
  type        = string
  default     = "postgres"
}

variable "db_password" {
  description = "Senha do usuário administrador do banco de dados PostgreSQL."
  type        = string
  sensitive   = true
  default     = "postgres_password"
}

variable "db_name" {
  description = "Nome do banco de dados inicial."
  type        = string
  default     = "oficina"
}

variable "db_instance_class" {
  description = "Tipo da instância RDS para o PostgreSQL."
  type        = string
  default     = "db.t3.micro"
}

variable "k8s_node_instance_type" {
  description = "Tipo de instância EC2 para os nós gerenciados do Kubernetes."
  type        = string
  default     = "t3.medium"
}

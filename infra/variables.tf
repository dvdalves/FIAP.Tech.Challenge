variable "azure_region" {
  description = "Região do Azure para provisionamento da infraestrutura."
  type        = string
  default     = "North Central US"
}

variable "resource_group_name" {
  description = "Resource Group da aplicação"
  type        = string
  default     = "fiap-tech-challenge-rg"
}

variable "environment" {
  description = "Ambiente de execução (ex: dev, staging, prod)."
  type        = string
  default     = "production"
}

variable "cluster_name" {
  description = "Nome do Cluster Kubernetes (AKS)."
  type        = string
  default     = "oficina-aks-cluster"
}

variable "vnet_address_space" {
  description = "Bloco CIDR da VNet."
  type        = string
  default     = "10.0.0.0/16"
}

variable "db_username" {
  description = "Nome de usuário administrador do banco de dados PostgreSQL."
  type        = string
  default     = "oficina_admin"
}

variable "db_password" {
  description = "Senha do usuário administrador do banco de dados PostgreSQL."
  type        = string
  sensitive   = true
  default     = "Oficina@Segura2025#Db"
}

variable "db_name" {
  description = "Nome do banco de dados inicial."
  type        = string
  default     = "oficina"
}

variable "db_instance_class" {
  description = "SKU do Azure Database for PostgreSQL."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "k8s_node_instance_type" {
  description = "SKU de VM para os nós gerenciados do Kubernetes."
  type        = string
  default     = "Standard_D2s_v5"
}

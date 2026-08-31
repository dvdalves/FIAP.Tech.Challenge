output "eks_cluster_name" {
  description = "Nome do cluster EKS criado."
  value       = aws_eks_cluster.main.name
}

output "eks_cluster_endpoint" {
  description = "Endpoint de acesso à API do Kubernetes EKS."
  value       = aws_eks_cluster.main.endpoint
}

output "eks_cluster_certificate_authority_data" {
  description = "Certificado de autoridade (CA) do cluster EKS."
  value       = aws_eks_cluster.main.certificate_authority[0].data
  sensitive   = true
}

output "rds_postgres_endpoint" {
  description = "Endpoint de conexão com a instância RDS PostgreSQL."
  value       = aws_db_instance.postgres.endpoint
}

output "rds_postgres_database_name" {
  description = "Nome do banco de dados provisionado no RDS."
  value       = aws_db_instance.postgres.db_name
}

output "vpc_id" {
  description = "ID da VPC provisionada."
  value       = aws_vpc.main.id
}

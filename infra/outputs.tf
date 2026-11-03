output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "aks_cluster_name" {
  value = azurerm_kubernetes_cluster.main.name
}

output "aks_fqdn" {
  value = azurerm_kubernetes_cluster.main.fqdn
}

output "postgres_fqdn" {
  value = azurerm_postgresql_flexible_server.postgres.fqdn
}

output "kubectl_configure_command" {
  value = "az aks get-credentials --resource-group ${azurerm_resource_group.main.name} --name ${azurerm_kubernetes_cluster.main.name}"
}

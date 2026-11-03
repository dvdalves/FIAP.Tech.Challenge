resource "azurerm_resource_group" "main" {
  name     = var.resource_group_name
  location = var.azure_region
}

resource "azurerm_virtual_network" "main" {
  name                = "${var.cluster_name}-vnet"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  address_space       = [var.vnet_address_space]
}

resource "azurerm_subnet" "aks" {
  name                 = "aks-nodes"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.1.0/24"]
}

resource "azurerm_subnet" "postgres" {
  name                 = "postgres"
  resource_group_name  = azurerm_resource_group.main.name
  virtual_network_name = azurerm_virtual_network.main.name
  address_prefixes     = ["10.0.2.0/24"]

  delegation {
    name = "postgres-flexible-server"
    service_delegation {
      name    = "Microsoft.DBforPostgreSQL/flexibleServers"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "${var.cluster_name}-logs"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
}

resource "azurerm_kubernetes_cluster" "main" {
  name                = var.cluster_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  dns_prefix          = var.cluster_name
  kubernetes_version  = "1.30.3"

  default_node_pool {
    name                 = "system"
    vm_size              = var.k8s_node_instance_type
    vnet_subnet_id       = azurerm_subnet.aks.id
    node_count           = 2
    auto_scaling_enabled = true
    min_count            = 1
    max_count            = 5
  }

  identity {
    type = "SystemAssigned"
  }

  network_profile {
    network_plugin      = "azure"
    network_plugin_mode = "overlay"
    load_balancer_sku   = "standard"
    service_cidr        = "10.2.0.0/16"
    dns_service_ip      = "10.2.0.10"
  }

  oms_agent {
    log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  }
}

resource "azurerm_postgresql_flexible_server" "postgres" {
  name                          = "oficina-postgres-db"
  resource_group_name           = azurerm_resource_group.main.name
  location                      = azurerm_resource_group.main.location
  version                       = "16"
  delegated_subnet_id           = azurerm_subnet.postgres.id
  administrator_login           = var.db_username
  administrator_password        = var.db_password
  sku_name                      = var.db_instance_class
  storage_mb                    = 32768
  backup_retention_days         = 7
  public_network_access_enabled = false
}

resource "azurerm_postgresql_flexible_server_database" "oficina" {
  name      = var.db_name
  server_id = azurerm_postgresql_flexible_server.postgres.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

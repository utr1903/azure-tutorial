### Kubernetes Cluster ###

# Kubernetes Cluster
resource "azurerm_kubernetes_cluster" "iot" {
  name                = local.project_kubernetes_cluster_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location

  kubernetes_version = "1.23.5"

  node_resource_group = local.project_kubernetes_cluster_nodepool_name

  default_node_pool {
    name    = "system-pool"
    vm_size = "Standard_D2_v2"

    enable_auto_scaling = true
    node_count          = 1
    min_count           = 1
    max_count           = 3
  }

  identity {
    type = "SystemAssigned"
  }
}

# Kubernetes Nodepool - Input Processor
resource "azurerm_kubernetes_cluster_node_pool" "input_processor" {
  name                  = "input-processor-pool"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.iot.id
  vm_size               = "Standard_DS2_v2"

  orchestrator_version = "1.23.5"

  enable_auto_scaling = true
  node_count          = 1
  min_count           = 1
  max_count           = 3
}

# Kubernetes Nodepool - Timeseries Processor
resource "azurerm_kubernetes_cluster_node_pool" "timeseries_processor" {
  name                  = "timeseries-processor-pool"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.iot.id
  vm_size               = "Standard_DS2_v2"

  orchestrator_version = "1.23.5"

  enable_auto_scaling = true
  node_count          = 1
  min_count           = 1
  max_count           = 3
}

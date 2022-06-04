### Kubernetes Cluster ###

# Kubernetes Cluster
resource "azurerm_kubernetes_cluster" "iot" {
  name                = var.project_kubernetes_cluster_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location

  dns_prefix         = var.platform
  kubernetes_version = "1.23.5"

  node_resource_group = var.project_kubernetes_cluster_nodepool_name

  default_node_pool {
    name    = "system"
    vm_size = "Standard_D2_v2"

    node_labels = {
      nodePoolName = "system"
    }

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
  name                  = "input"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.iot.id
  vm_size               = "Standard_DS2_v2"

  orchestrator_version = "1.23.5"

  node_labels = {
    nodePoolName = "input"
  }

  enable_auto_scaling = true
  node_count          = 1
  min_count           = 1
  max_count           = 3
}

# Kubernetes Nodepool - Timeseries Processor
resource "azurerm_kubernetes_cluster_node_pool" "timeseries_processor" {
  name                  = "timeseries"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.iot.id
  vm_size               = "Standard_DS2_v2"

  orchestrator_version = "1.23.5"

  node_labels = {
    nodePoolName = "timeseries"
  }

  enable_auto_scaling = true
  node_count          = 1
  min_count           = 1
  max_count           = 3
}

# Kubernetes Nodepool - Diagnostics Processor
resource "azurerm_kubernetes_cluster_node_pool" "diagnostics_processor" {
  name                  = "diagnostics"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.iot.id
  vm_size               = "Standard_DS2_v2"

  orchestrator_version = "1.23.5"

  node_labels = {
    nodePoolName = "diagnostics"
  }

  enable_auto_scaling = true
  node_count          = 1
  min_count           = 1
  max_count           = 3
}

# Kubernetes Nodepool - Applications
resource "azurerm_kubernetes_cluster_node_pool" "applications" {
  name                  = "applications"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.iot.id
  vm_size               = "Standard_DS2_v2"

  orchestrator_version = "1.23.5"

  node_labels = {
    nodePoolName = "applications"
  }

  enable_auto_scaling = true
  node_count          = 1
  min_count           = 1
  max_count           = 2
}
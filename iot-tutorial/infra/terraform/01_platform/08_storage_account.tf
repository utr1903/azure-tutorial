### Storage Account ###

# Storage Account
resource "azurerm_storage_account" "iot" {
  name                = var.project_storage_account_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location

  account_tier             = "Standard"
  account_replication_type = "LRS"
}

# Blob Container - Stats Processor
resource "azurerm_storage_container" "stats_processor" {
  name                  = var.project_blob_container_name_stats
  storage_account_name  = azurerm_storage_account.iot.name
  container_access_type = "private"
}

# Blob Container - Diagnostics Processor
resource "azurerm_storage_container" "diags_processor" {
  name                  = var.project_blob_container_name_diags
  storage_account_name  = azurerm_storage_account.iot.name
  container_access_type = "private"
}

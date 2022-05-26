### Storage Account ###

# Storage Account
resource "azurerm_storage_account" "iot" {
  name                = local.project_storage_account_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location

  account_tier             = "Standard"
  account_replication_type = "LRS"
}

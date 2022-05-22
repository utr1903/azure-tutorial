### Function App ###

# App Service Plan
resource "azurerm_app_service_plan" "iot" {
  name                = local.project_app_service_plan_name
  location            = azurerm_resource_group.iot.location
  resource_group_name = azurerm_resource_group.iot.name
  kind                = "FunctionApp"

  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

# Function App
resource "azurerm_function_app" "iot" {
  name                       = local.project_function_app_name
  location                   = azurerm_resource_group.iot.location
  resource_group_name        = azurerm_resource_group.iot.name

  app_service_plan_id        = azurerm_app_service_plan.iot.id
  storage_account_name       = azurerm_storage_account.iot.name
  storage_account_access_key = azurerm_storage_account.iot.primary_access_key

  identity {
    type = "SystemAssigned"
  }
}

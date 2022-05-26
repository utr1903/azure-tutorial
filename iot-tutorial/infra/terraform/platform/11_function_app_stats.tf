### Function App ###

# App Service Plan
resource "azurerm_service_plan" "stats" {
  name                = local.project_function_app_stats_plan_name
  location            = azurerm_resource_group.iot.location
  resource_group_name = azurerm_resource_group.iot.name

  os_type  = "Linux"
  sku_name = "Y1"
}

# Function App
resource "azurerm_linux_function_app" "stats" {
  name                = local.project_function_app_stats_name
  location            = azurerm_resource_group.iot.location
  resource_group_name = azurerm_resource_group.iot.name

  service_plan_id            = azurerm_service_plan.stats.id
  storage_account_name       = azurerm_storage_account.iot.name
  storage_account_access_key = azurerm_storage_account.iot.primary_access_key

  site_config {
    application_stack {
      dotnet_version = "3.1"
    }
  }

  identity {
    type = "SystemAssigned"
  }
}

# Azure Event Hubs Data Receiver
resource "azurerm_role_assignment" "event_hub_data_receiver_role_for_function_app" {
  scope                = azurerm_eventhub_consumer_group.stats_function.id
  role_definition_name = "Azure Event Hubs Data Receiver"
  principal_id         = azurerm_linux_function_app.stats.identity.0.principal_id
}

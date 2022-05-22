### Function App ###

# App Service Plan
resource "azurerm_service_plan" "iot" {
  name                = local.project_app_service_plan_name
  location            = azurerm_resource_group.iot.location
  resource_group_name = azurerm_resource_group.iot.name

  # kind     = "FunctionApp"
  os_type  = "Linux"
  sku_name = "Y1"
}

# Function App
resource "azurerm_linux_function_app" "iot" {
  name                = local.project_function_app_name
  location            = azurerm_resource_group.iot.location
  resource_group_name = azurerm_resource_group.iot.name

  service_plan_id            = azurerm_service_plan.iot.id
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

# Azure Service Bus Data Receiver Role
resource "azurerm_role_assignment" "service_bus_data_receiver_role_for_function_app" {
  scope                = azurerm_servicebus_queue.input.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_linux_function_app.iot.identity.0.principal_id
}

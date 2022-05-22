### Service Bus ###

# Service Bus Namespace
resource "azurerm_servicebus_namespace" "iot" {
  name                = local.project_service_bus_namespace_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location
  sku                 = "Standard"

  tags = {
    source = "terraform"
  }
}

# Service Bus Queue
resource "azurerm_servicebus_queue" "input" {
  name                = "input"
  namespace_id        = azurerm_servicebus_namespace.iot.id

  enable_partitioning = false
}

# Service Bus Queue Auth Rule for IoT Hub
resource "azurerm_servicebus_queue_authorization_rule" "iot_hub" {
  name     = local.project_iot_hub_name
  queue_id = azurerm_servicebus_queue.input.id

  listen = false
  send   = true
  manage = false
}

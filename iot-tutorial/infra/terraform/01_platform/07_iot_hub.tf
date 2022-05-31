### IoT Hub ###

resource "azurerm_iothub" "iot" {
  name                = var.project_iot_hub_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location

  sku {
    name     = "S1"
    capacity = "1"
  }

  endpoint {
    type              = "AzureIotHub.ServiceBusQueue"
    connection_string = azurerm_servicebus_queue_authorization_rule.iot_hub.primary_connection_string
    name              = var.project_iot_hub_name
  }

  route {
    name           = var.project_iot_hub_name
    source         = "DeviceMessages"
    condition      = "true"
    endpoint_names = [var.project_iot_hub_name]
    enabled        = true
  }
}

### IoT Hub ###

resource "azurerm_iothub" "iot" {
  name                = local.project_iot_hub_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location

  sku {
    name     = "S1"
    capacity = "1"
  }

  endpoint {
    type              = "AzureIotHub.ServiceBusQueue"
    connection_string = azurerm_servicebus_queue_authorization_rule.iot_hub.primary_connection_string
    name              = local.project_iot_hub_name
  }

  route {
    name           = local.project_iot_hub_name
    source         = "DeviceMessages"
    condition      = "true"
    endpoint_names = [local.project_iot_hub_name]
    enabled        = true
  }

  # enrichment {
  #   key            = "tenant"
  #   value          = "$twin.tags.Tenant"
  #   endpoint_names = ["export", "export2"]
  # }

  # cloud_to_device {
  #   max_delivery_count = 30
  #   default_ttl        = "PT1H"
  #   feedback {
  #     time_to_live       = "PT1H10M"
  #     max_delivery_count = 15
  #     lock_duration      = "PT30S"
  #   }
  # }

  # tags = {
  #   purpose = "testing"
  # }
}

### Event Hub ###

# Event Hub Namespace
resource "azurerm_eventhub_namespace" "iot" {
  name                = local.project_event_hub_namespace_name
  location            = azurerm_resource_group.iot.location
  resource_group_name = azurerm_resource_group.iot.name

  sku = "Standard"
}

# Event Hub
resource "azurerm_eventhub" "iot" {
  name                = local.project_event_hub_name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  resource_group_name = azurerm_resource_group.iot.name

  partition_count   = 2
  message_retention = 7
}

# Event Hub Consumer Group
resource "azurerm_eventhub_consumer_group" "iot" {
  name                = local.project_timeseries_insight_name
  resource_group_name = azurerm_resource_group.iot.name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  eventhub_name       = azurerm_eventhub.iot.name
}

# Event Hub Auth Rule for Timeseries Insight
resource "azurerm_eventhub_authorization_rule" "tsi" {
  name                = local.project_timeseries_insight_name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  eventhub_name       = azurerm_eventhub.iot.name
  resource_group_name = azurerm_resource_group.iot.name

  listen = true
  send   = false
  manage = false
}

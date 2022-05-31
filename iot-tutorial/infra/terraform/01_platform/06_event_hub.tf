### Event Hub ###

# Event Hub Namespace
resource "azurerm_eventhub_namespace" "iot" {
  name                = var.project_event_hub_namespace_name
  location            = azurerm_resource_group.iot.location
  resource_group_name = azurerm_resource_group.iot.name

  sku = "Standard"
}

###########
### IoT ###
###########

# Event Hub
resource "azurerm_eventhub" "iot" {
  name                = var.project_event_hub_name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  resource_group_name = azurerm_resource_group.iot.name

  partition_count   = 2
  message_retention = 7
}

# Event Hub Consumer Group - Timeseries Insight
resource "azurerm_eventhub_consumer_group" "tsi" {
  name                = var.project_event_hub_consumer_group_name_tsi
  resource_group_name = azurerm_resource_group.iot.name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  eventhub_name       = azurerm_eventhub.iot.name
}

# Event Hub Auth Rule - Timeseries Insight
resource "azurerm_eventhub_authorization_rule" "tsi" {
  name                = var.project_event_hub_consumer_group_name_tsi
  namespace_name      = azurerm_eventhub_namespace.iot.name
  eventhub_name       = azurerm_eventhub.iot.name
  resource_group_name = azurerm_resource_group.iot.name

  listen = true
  send   = false
  manage = false
}

# Event Hub Consumer Group - Stats Processor
resource "azurerm_eventhub_consumer_group" "stats_processor" {
  name                = var.project_event_hub_consumer_group_name_stats
  resource_group_name = azurerm_resource_group.iot.name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  eventhub_name       = azurerm_eventhub.iot.name
}

# Event Hub Auth Rule - Stats Processor
resource "azurerm_eventhub_authorization_rule" "stats_processor" {
  name                = var.project_event_hub_consumer_group_name_stats
  namespace_name      = azurerm_eventhub_namespace.iot.name
  eventhub_name       = azurerm_eventhub.iot.name
  resource_group_name = azurerm_resource_group.iot.name

  listen = true
  send   = false
  manage = false
}
###########



# Event Hub
resource "azurerm_eventhub" "diagnostics" {
  name                = var.diagnostics_event_hub_name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  resource_group_name = azurerm_resource_group.iot.name

  partition_count   = 2
  message_retention = 7
}

# Event Hub Consumer Group - Diagnostics
resource "azurerm_eventhub_consumer_group" "diagnostics" {
  name                = var.diagnostics_event_hub_consumer_group_name
  resource_group_name = azurerm_resource_group.iot.name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  eventhub_name       = azurerm_eventhub.diagnostics.name
}

# Event Hub Auth Rule - Diagnostics
resource "azurerm_eventhub_namespace_authorization_rule" "diagnostics" {
  name                = var.diagnostics_event_hub_consumer_group_name
  namespace_name      = azurerm_eventhub_namespace.iot.name
  resource_group_name = azurerm_resource_group.iot.name

  listen = true
  send   = true
  manage = true
}
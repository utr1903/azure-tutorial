### Timeseries Insight ###

# Timeseries Insight Environment
resource "azurerm_iot_time_series_insights_gen2_environment" "iot" {
  name                = local.project_timeseries_insight_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location

  sku_name      = "L1"
  id_properties = ["id"]

  storage {
    name = azurerm_storage_account.iot.name
    key  = azurerm_storage_account.iot.primary_access_key
  }
}

# Timeseries Insight Source - Event Hub
resource "azurerm_iot_time_series_insights_event_source_eventhub" "iot" {
  name           = local.project_event_hub_name
  location       = azurerm_resource_group.iot.location
  environment_id = azurerm_iot_time_series_insights_gen2_environment.iot.id

  namespace_name      = azurerm_eventhub_namespace.iot.name
  event_source_resource_id = azurerm_eventhub.iot.id
  eventhub_name       = azurerm_eventhub.iot.name
  consumer_group_name = azurerm_eventhub_consumer_group.tsi.name

  shared_access_key        = azurerm_eventhub_authorization_rule.tsi.primary_key
  shared_access_key_name   = azurerm_eventhub_authorization_rule.tsi.name
}

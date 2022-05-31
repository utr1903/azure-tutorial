###################
### Diagnostics ###
###################

### Service Bus ###

data "azurerm_monitor_diagnostic_categories" "service_bus" {
  resource_id = azurerm_servicebus_namespace.iot.id
}

resource "azurerm_monitor_diagnostic_setting" "service_bus" {
  name                           = var.project_service_bus_namespace_name
  target_resource_id             = azurerm_servicebus_namespace.iot.id
  eventhub_name                  = var.diagnostics_event_hub_name
  eventhub_authorization_rule_id = azurerm_eventhub_namespace_authorization_rule.diagnostics.id

  dynamic "log" {
    for_each = data.azurerm_monitor_diagnostic_categories.service_bus.logs

    content {
      category = log.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }

  dynamic "metric" {
    for_each = data.azurerm_monitor_diagnostic_categories.service_bus.metrics

    content {
      category = metric.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }
}
#########

### IoT Hub ###

data "azurerm_monitor_diagnostic_categories" "iot_hub" {
  resource_id = azurerm_iothub.iot.id
}

resource "azurerm_monitor_diagnostic_setting" "iot_hub" {
  name                           = var.project_iot_hub_name
  target_resource_id             = azurerm_iothub.iot.id
  eventhub_name                  = var.diagnostics_event_hub_name
  eventhub_authorization_rule_id = azurerm_eventhub_namespace_authorization_rule.diagnostics.id

  dynamic "log" {
    for_each = data.azurerm_monitor_diagnostic_categories.iot_hub.logs

    content {
      category = log.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }

  dynamic "metric" {
    for_each = data.azurerm_monitor_diagnostic_categories.iot_hub.metrics

    content {
      category = metric.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }
}
#########

### Event Hub ###

data "azurerm_monitor_diagnostic_categories" "event_hub" {
  resource_id = azurerm_eventhub_namespace.iot.id
}

resource "azurerm_monitor_diagnostic_setting" "event_hub" {
  name                           = var.project_event_hub_namespace_name
  target_resource_id             = azurerm_eventhub_namespace.iot.id
  eventhub_name                  = var.diagnostics_event_hub_name
  eventhub_authorization_rule_id = azurerm_eventhub_namespace_authorization_rule.diagnostics.id

  dynamic "log" {
    for_each = data.azurerm_monitor_diagnostic_categories.event_hub.logs

    content {
      category = log.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }

  dynamic "metric" {
    for_each = data.azurerm_monitor_diagnostic_categories.event_hub.metrics

    content {
      category = metric.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }
}
#########

### Storage Account ###

data "azurerm_monitor_diagnostic_categories" "storage_account" {
  resource_id = azurerm_storage_account.iot.id
}

resource "azurerm_monitor_diagnostic_setting" "storage_account" {
  name                           = var.project_storage_account_name
  target_resource_id             = azurerm_storage_account.iot.id
  eventhub_name                  = var.diagnostics_event_hub_name
  eventhub_authorization_rule_id = azurerm_eventhub_namespace_authorization_rule.diagnostics.id

  dynamic "log" {
    for_each = data.azurerm_monitor_diagnostic_categories.storage_account.logs

    content {
      category = log.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }

  dynamic "metric" {
    for_each = data.azurerm_monitor_diagnostic_categories.storage_account.metrics

    content {
      category = metric.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }
}
#########

### Timeseries Insights ###

data "azurerm_monitor_diagnostic_categories" "tsi" {
  resource_id = azurerm_iot_time_series_insights_gen2_environment.iot.id
}

resource "azurerm_monitor_diagnostic_setting" "tsi" {
  name                           = var.project_timeseries_insight_name
  target_resource_id             = azurerm_iot_time_series_insights_gen2_environment.iot.id
  eventhub_name                  = var.diagnostics_event_hub_name
  eventhub_authorization_rule_id = azurerm_eventhub_namespace_authorization_rule.diagnostics.id

  dynamic "log" {
    for_each = data.azurerm_monitor_diagnostic_categories.tsi.logs

    content {
      category = log.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }

  dynamic "metric" {
    for_each = data.azurerm_monitor_diagnostic_categories.tsi.metrics

    content {
      category = metric.value
      enabled  = true

      retention_policy {
        enabled = false
      }
    }
  }
}
#########

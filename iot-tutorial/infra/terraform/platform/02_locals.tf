locals {

  ### Shared ###

  # Resource Group
  shared_resource_group_name  = "rg${var.project}${var.location_short}${var.shared}x000"
  shared_storage_account_name = "st${var.project}${var.location_short}${var.shared}x000"

  ### Project

  # Resource Group
  project_resource_group_name = "rg${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"

  # Service Bus
  project_service_bus_namespace_name = "sb${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"

  # Event Hub
  project_event_hub_namespace_name = "ehn${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"
  project_event_hub_name = "eh${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"

  # IoT Hub
  project_iot_hub_name = "iot${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"

  # Storage Account
  project_storage_account_name = "st${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"

  # Timeseries Insight
  project_timeseries_insight_name = "tsi${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"

  # Kubernetes Cluster
  project_kubernetes_cluster_name = "aks${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"
  project_kubernetes_cluster_nodepool_name = "rgaksnp${var.project}${var.location_short}${var.platform}${var.stage_short}${var.instance}"

  # Function App
  project_function_app_name = "func${var.project}${var.location_short}${var.stage_short}${var.instance}"
  project_app_service_plan_name = "plan${var.project}${var.location_short}${var.stage_short}${var.instance}"
}

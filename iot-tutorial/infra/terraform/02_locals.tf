locals {

  ### Shared ###

  # Resource Group
  shared_resource_group_name  = "rg${var.project}${var.location_short}x000"
  shared_storage_account_name = "st${var.project}${var.location_short}x000"

  ### Project

  # Resource Group
  project_resource_group_name = "rg${var.project}${var.location_short}${var.stage_short}${var.instance}"

  # Service Bus
  project_service_bus_name = "sb${var.project}${var.location_short}${var.stage_short}${var.instance}"

  # IoT Hub
  project_iot_hub_name = "iot${var.project}${var.location_short}${var.stage_short}${var.instance}"
}

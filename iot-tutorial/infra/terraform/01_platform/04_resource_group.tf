### Resource Group ###

# Resource Group
resource "azurerm_resource_group" "iot" {
  name     = local.project_resource_group_name
  location = var.location_long
}

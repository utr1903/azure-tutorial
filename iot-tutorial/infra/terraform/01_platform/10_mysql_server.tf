### MySQL Server ###

# MySQL Server
resource "azurerm_mysql_server" "iot" {
  name                = var.project_mysql_server_name
  resource_group_name = azurerm_resource_group.iot.name
  location            = azurerm_resource_group.iot.location

  administrator_login          = var.project
  administrator_login_password = "Admin@1903!"

  sku_name   = "B_Gen5_1"
  storage_mb = 5120
  version    = "8.0"

  auto_grow_enabled                 = true
  backup_retention_days             = 7
  geo_redundant_backup_enabled      = false
  infrastructure_encryption_enabled = false
  public_network_access_enabled     = true
  ssl_enforcement_enabled           = true
  ssl_minimal_tls_version_enforced  = "TLS1_2"
}

# MySQL Firewall Rule - Azure Services
resource "azurerm_mysql_firewall_rule" "azure_services" {
  name                = "azure_services"
  resource_group_name = azurerm_resource_group.iot.name
  server_name         = azurerm_mysql_server.iot.name

  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# MySQL Database - Device
resource "azurerm_mysql_database" "device" {
  name                = "device"
  resource_group_name = azurerm_resource_group.iot.name
  server_name         = azurerm_mysql_server.iot.name

  charset   = "utf8"
  collation = "utf8_unicode_ci"
}

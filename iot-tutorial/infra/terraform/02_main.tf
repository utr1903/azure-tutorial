terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=2.46.0"
    }
  }

  backend "azurerm" {
    resource_group_name  = local.shared_resource_group_name
    storage_account_name = local.shared_storage_account_name
    container_name       = var.project
    key                  = "${var.stage_short}${var.instance}.tfstate"
  }

}

provider "azurerm" {
  features {}
}

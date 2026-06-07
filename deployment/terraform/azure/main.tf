provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "conference-rg"
  location = "westeurope"
}

resource "azurerm_postgresql_server" "db" {
  name                = "conference-postgres"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  version             = "11"
  administrator_login = "admin"
  administrator_password = var.db_password
  sku_name = "B_Gen5_1"
}

resource "azurerm_storage_account" "certificates" {
  name                     = "conferencecertificates"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

variable "db_password" {
  sensitive = true
}

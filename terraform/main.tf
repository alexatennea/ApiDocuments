terraform {
  required_version = ">= 1.7.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  backend "azurerm" {
    # Values supplied via -backend-config in the pipeline
  }
}

provider "azurerm" {
  features {}
}

# ── Resource Group ─────────────────────────────────────────────────────────────
resource "azurerm_resource_group" "main" {
  name     = "${var.project_name}-${var.environment}-rg"
  location = var.location
  tags     = local.common_tags
}

# ── Storage Account (Blob) ─────────────────────────────────────────────────────
resource "azurerm_storage_account" "documents" {
  name                     = "${var.storage_account_prefix}${var.environment}sa"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

  tags = local.common_tags
}

resource "azurerm_storage_container" "documents" {
  name                  = "documents"
  storage_account_id    = azurerm_storage_account.documents.id
  container_access_type = "private"
}

# ── Azure SQL Server ───────────────────────────────────────────────────────────
resource "azurerm_mssql_server" "main" {
  name                         = "${var.project_name}-${var.environment}-sqlsrv"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
  minimum_tls_version          = "1.2"

  tags = local.common_tags
}

# Basic tier – cheapest available SQL Database (5 DTUs)
resource "azurerm_mssql_database" "main" {
  name      = "${var.project_name}-${var.environment}-db"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "Basic"

  tags = local.common_tags
}

# Allow Azure services to reach the SQL server
resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# ── App Service Plan ───────────────────────────────────────────────────────────
resource "azurerm_service_plan" "main" {
  name                = "${var.project_name}-${var.environment}-asp"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku

  tags = local.common_tags
}

# ── App Service ────────────────────────────────────────────────────────────────
resource "azurerm_linux_web_app" "api" {
  name                = "${var.project_name}-${var.environment}-api"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }
    always_on = false
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"          = var.aspnetcore_environment
    "BlobStorage__ContainerName"      = azurerm_storage_container.documents.name
  }

  connection_string {
    name  = "DefaultConnection"
    type  = "SQLAzure"
    value = local.sql_connection_string
  }

  connection_string {
    name  = "BlobStorage"
    type  = "Custom"
    value = azurerm_storage_account.documents.primary_connection_string
  }

  tags = local.common_tags
}

# ── App Configuration ──────────────────────────────────────────────────────────
resource "azurerm_app_configuration" "main" {
  name                = "${var.project_name}-${var.environment}-appconfig"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "free"

  tags = local.common_tags
}

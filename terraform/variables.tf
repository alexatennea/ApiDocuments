variable "project_name" {
  description = "The base name used for all Azure resources."
  type        = string
  default     = "apidocs"
}

variable "environment" {
  description = "Deployment environment name (dev or prod)."
  type        = string
  validation {
    condition     = contains(["dev", "prod"], var.environment)
    error_message = "environment must be 'dev' or 'prod'."
  }
}

variable "location" {
  description = "Azure region in which resources will be deployed."
  type        = string
  default     = "uksouth"
}

variable "storage_account_prefix" {
  description = "Short prefix used to build the storage account name (max 9 chars, lowercase alphanumeric)."
  type        = string
  default     = "apidocs"
}

variable "sql_admin_username" {
  description = "Administrator login name for the SQL Server."
  type        = string
  sensitive   = true
}

variable "sql_admin_password" {
  description = "Administrator login password for the SQL Server."
  type        = string
  sensitive   = true
}

variable "app_service_sku" {
  description = "SKU for the App Service Plan (e.g. F1 for free, B1 for basic)."
  type        = string
  default     = "B1"
}

variable "aspnetcore_environment" {
  description = "Value of ASPNETCORE_ENVIRONMENT set on the App Service."
  type        = string
  default     = "Production"
}

locals {
  common_tags = {
    Project     = var.project_name
    Environment = var.environment
    ManagedBy   = "Terraform"
  }

  sql_connection_string = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};Persist Security Info=False;User ID=${var.sql_admin_username};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}

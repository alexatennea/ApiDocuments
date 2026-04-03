output "api_url" {
  description = "The default hostname of the deployed API App Service."
  value       = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "resource_group_name" {
  description = "Name of the resource group containing all resources."
  value       = azurerm_resource_group.main.name
}

output "sql_server_fqdn" {
  description = "Fully qualified domain name of the SQL Server."
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "storage_account_name" {
  description = "Name of the Azure Storage account."
  value       = azurerm_storage_account.documents.name
}

output "app_configuration_endpoint" {
  description = "Endpoint URL of the Azure App Configuration instance."
  value       = azurerm_app_configuration.main.endpoint
}

#!/usr/bin/env bash

func azure functionapp publish ita-datafactory-log-extractor
az functionapp config appsettings set \
  --name ita-datafactory-log-extractor \
  --resource-group vangos-resources \
  --settings AZURE_SUBSCRIPTION_ID="@Microsoft.KeyVault(SecretUri=https://vangos-key-vault.vault.azure.net/secrets/azure-subscription-id/ae8112e173984eb3b0c6d71f7ed19348)" \
  AZURE_TENANT_ID="@Microsoft.KeyVault(SecretUri=https://vangos-key-vault.vault.azure.net/secrets/azure-tenant-id/4cd6161571ae4e7f9f39c2b3c980a999)" \
  DATAFACTORY_APP_ID="@Microsoft.KeyVault(SecretUri=https://vangos-key-vault.vault.azure.net/secrets/data-factory-app-id/ffaf96f11f474ce29ea3b08e66d4230f)" \
  DATAFACTORY_AUTH_KEY="@Microsoft.KeyVault(SecretUri=https://vangos-key-vault.vault.azure.net/secrets/data-factory-auth-key/15bffe39d00047a9be7841b25bc8741c)" \
  RESOURCE_GROUP="vangos-resources" \
  DATAFACTORY_NAME="vangos-factory"
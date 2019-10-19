#!/usr/bin/env bash

func azure functionapp publish ita-datafactory-log-extractor
az functionapp config appsettings set \
  --name ita-datafactory-log-extractor \
  --resource-group vangos-resources \
  --settings AZURE_SUBSCRIPTION_ID=$AZURE_SUBSCRIPTION_ID \
  AZURE_TENANT_ID=$AZURE_TENANT_ID \
  DATAFACTORY_APP_ID=$DATAFACTORY_APP_ID \
  DATAFACTORY_AUTH_KEY=$DATAFACTORY_AUTH_KEY \
  RESOURCE_GROUP=$AZURE_RESOURCE_GROUP \
  DATAFACTORY_NAME=$DATAFACTORY_NAME
# ITA Data Factory Log Extractor
A tool that extracts the status of a pipline within an Azure Data Factory.

## Local Development
func start

**Prerequisites** 
 - .NET Core
 - Azure CLI

## Deploy
```./deploy.sh```

## Production Deployment Notes
 - An Azure Data Factory with one or more Pipelines is required
 - An Azure AD App Registration is required procure an OAuth Client ID and Client Secret
 - The App Registration must be part of the Contributor Role in AD
 - The following environment variables need to exist:
    - AZURE_SUBSCRIPTION_ID: Active Subscription ID
    - AZURE_TENANT_ID: Azure Tenant ID
    - DATAFACTORY_APP_ID: The Application ID created by the App Registration
    - DATAFACTORY_AUTH_KEY: The Authentication Key created by the App Registration
    - RESOURCE_GROUP: The resource group that the Data Factory is a part of
    - DATAFACTORY_NAME: The name of the Data Factory

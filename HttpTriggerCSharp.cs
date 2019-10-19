using System;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Rest;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Azure.Management.DataFactory.Models;

namespace ita.DataFactoryLogs
{
  public static class HttpTriggerCSharp
  {
    [FunctionName("HttpTriggerCSharp")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");

      string pipelineName = req.Query["pipelineName"];
      if (pipelineName == null)
        return new BadRequestObjectResult("Please pass a pipelineName on the query string");

      var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", EnvironmentVariableTarget.Process);
      var tenantID = Environment.GetEnvironmentVariable("AZURE_TENANT_ID", EnvironmentVariableTarget.Process);
      var applicationId = Environment.GetEnvironmentVariable("DATAFACTORY_APP_ID", EnvironmentVariableTarget.Process);
      var authenticationKey = Environment.GetEnvironmentVariable("DATAFACTORY_AUTH_KEY", EnvironmentVariableTarget.Process);
      var resourceGroup = Environment.GetEnvironmentVariable("RESOURCE_GROUP", EnvironmentVariableTarget.Process);
      var datafactoryName = Environment.GetEnvironmentVariable("DATAFACTORY_NAME", EnvironmentVariableTarget.Process);

      var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
      ClientCredential cc = new ClientCredential(applicationId, authenticationKey);
      AuthenticationResult result = context.AcquireTokenAsync(
          "https://management.azure.com/", cc).Result;
      ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
      var client = new DataFactoryManagementClient(cred)
      {
        SubscriptionId = subscriptionId
      };

      RunFilterParameters filterParameters = new RunFilterParameters();
      filterParameters.LastUpdatedAfter = DateTime.MinValue;
      filterParameters.LastUpdatedBefore = DateTime.MaxValue;
      PipelineRunsQueryResponse pipelineRuns = client.PipelineRuns.QueryByFactory(resourceGroup, datafactoryName, filterParameters);
      PipelineRun latestPipelineRun = null;

      foreach (var run in pipelineRuns.Value)
      {
        if ((latestPipelineRun == null) || (run.PipelineName == pipelineName && run.IsLatest == true && run.LastUpdated > latestPipelineRun.LastUpdated))
        {
          latestPipelineRun = run;
        }
      }

      return (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(latestPipelineRun));
    }
  }
}

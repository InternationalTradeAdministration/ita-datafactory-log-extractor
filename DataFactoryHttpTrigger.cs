using System;
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

namespace ita.DataFactory
{
  public static class DataFactoryHttpTrigger
  {
    
    [FunctionName("ExtractPipelineLog")]
    public static IActionResult ExtractPipelineLog(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      log.LogInformation("Extracting pipeline log.");
      string pipelineName = req.Query["pipelineName"];

      if (pipelineName == null)
        return new BadRequestObjectResult("Please pass a pipelineName on the query string");

      var client = buildClient();

      RunFilterParameters filterParameters = new RunFilterParameters();
      filterParameters.LastUpdatedAfter = DateTime.MinValue;
      filterParameters.LastUpdatedBefore = DateTime.MaxValue;
      PipelineRunsQueryResponse pipelineRuns = client.PipelineRuns.QueryByFactory(
        getResourceGroup(), getDataFactoryName(), filterParameters);
      PipelineRun latestPipelineRun = null;

      foreach (var run in pipelineRuns.Value)
      {
        if (run.PipelineName == pipelineName && 
          ((latestPipelineRun == null) || (run.IsLatest == true && run.LastUpdated > latestPipelineRun.LastUpdated)))
        {
          latestPipelineRun = run;
        }
      }

      return (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(latestPipelineRun));
    }

    [FunctionName("RunPipeline")]
    public static IActionResult RunPipeline(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      log.LogInformation("Triggering pipeline run.");
      string pipelineName = req.Query["pipelineName"];

      if (pipelineName == null)
        return new BadRequestObjectResult("Please pass a pipelineName on the query string");

      var client = buildClient();

      CreateRunResponse runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(
        getResourceGroup(), getDataFactoryName(), pipelineName, null).Result.Body;
      return (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(runResponse.RunId));
    }
  static DataFactoryManagementClient buildClient() {
      var subscriptionId = Environment.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID", EnvironmentVariableTarget.Process);
      var tenantID = Environment.GetEnvironmentVariable("AZURE_TENANT_ID", EnvironmentVariableTarget.Process);
      var applicationId = Environment.GetEnvironmentVariable("DATAFACTORY_APP_ID", EnvironmentVariableTarget.Process);
      var authenticationKey = Environment.GetEnvironmentVariable("DATAFACTORY_AUTH_KEY", EnvironmentVariableTarget.Process);
      var resourceGroup = getResourceGroup();
      var dataFactoryName = Environment.GetEnvironmentVariable("DATAFACTORY_NAME", EnvironmentVariableTarget.Process);

      var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
      ClientCredential cc = new ClientCredential(applicationId, authenticationKey);
      AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
      ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
      return new DataFactoryManagementClient(cred)
      {
        SubscriptionId = subscriptionId
      };
    }

    static string getResourceGroup() {
      return Environment.GetEnvironmentVariable("RESOURCE_GROUP", EnvironmentVariableTarget.Process);
    }

    static string getDataFactoryName() {
      return Environment.GetEnvironmentVariable("DATAFACTORY_NAME", EnvironmentVariableTarget.Process);
    }
  }

}

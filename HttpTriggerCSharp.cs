using System;
using System.Threading.Tasks;
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
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      log.LogInformation("C# HTTP trigger function processed a request.");

      string pipelineName = req.Query["pipelineName"];
      if (pipelineName == null)
        return new BadRequestObjectResult("Please pass a pipelineName on the query string");

      var subscriptionId = "61dbae51-cdd9-4380-ba89-78dd6c20b4d7";
      var tenantID = "787661a1-5142-41c0-bcc5-c2c9080781e5";
      var applicationId = "7b652d34-009e-4754-ac8e-960a6075368c";
      var authenticationKey = "INnoF[.6.xZk[z86Heoxy67Yde9TWG44";
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
      PipelineRunsQueryResponse pipelineRuns = client.PipelineRuns.QueryByFactory("vangos-resources", "vangos-factory", filterParameters);
      PipelineRun latestPipelineRun = null;
      foreach (var run in pipelineRuns.Value)
      {
        if ((run.IsLatest == true) && (run.PipelineName == pipelineName))
        {
          latestPipelineRun = run;
          break;
        }
      }

      return (ActionResult)new OkObjectResult(JsonConvert.SerializeObject(latestPipelineRun));
    }
  }
}

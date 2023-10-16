using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace RoomAvailabilityAutomationFunctionApp
{
    public static class RoomAvailabilityAutomation
	{
		[FunctionName("RoomAvailabilityAutomation")]
		public static async Task<IActionResult> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
			ILogger log)
		{
			var applicationname = GetEnvironmentVariable("applicationname") ?? "na.";
			var deviceid = GetEnvironmentVariable("deviceid") ?? "na.";
			var apiKey = GetEnvironmentVariable("apiKey") ?? "na.";
			var lightStrategy = GetEnvironmentVariable("lightStrategy") ?? "na.";
			var network = GetEnvironmentVariable("network") ?? "na.";

			string currentDate = req.Query["currentDate"];
			string startDate = req.Query["startDate"];
			string endDate = req.Query["endDate"];

			if (string.IsNullOrEmpty(startDate)
					|| string.IsNullOrEmpty(endDate)
					|| string.IsNullOrEmpty(currentDate))
			{
				return new BadRequestObjectResult($"Exception: no start date or end date in query");
			}

			log.LogInformation($"RoomAvailabilityAutomation function processed a request: {startDate} | {currentDate} | {endDate}");

			try
			{
				string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

				var freeSlots = OutlookMeetingProvider.GetMeetingTimeSuggestionsResult(requestBody, currentDate);

				var busyLightLoraProvider = new BusyLightLoraProvider(applicationname, deviceid, apiKey, network, lightStrategy);

				busyLightLoraProvider.ChangeLight(freeSlots.Current, freeSlots.Next, freeSlots.NumberOfMinutesTillFullHour, log);	

				string responseMessage = $"Triggered function executed successfully: Current slot is free:'{freeSlots.Current}'; Next slot is free:'{freeSlots.Next}'.";

				log.LogInformation($"request: {responseMessage}");

				return new OkObjectResult(responseMessage);
			}
			catch (Exception ex)
            {
				return new BadRequestObjectResult($"Exception: {ex.Message}");
			}
		}

		private static string GetEnvironmentVariable(string name)
		{
			return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
		}
	}
}
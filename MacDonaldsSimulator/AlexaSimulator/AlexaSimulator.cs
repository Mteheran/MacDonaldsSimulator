using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET;

namespace AlexaSimulator
{
    public static class AlexaSimulator
    {
        [FunctionName("Alexa")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            string json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);
            var requestType = skillRequest.GetRequestType();
            Session session = skillRequest.Session;

            SkillResponse response = null;

            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Welcome to MacdonalsApp Wich store do you want to consult!");
                response.Response.ShouldEndSession = false;

            }
            else if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;

                switch (intentRequest.Intent.Name.ToLower())
                {
                   
                    case "store":
                        if (intentRequest.Intent.Slots.Count > 0)
                        {
                            if (intentRequest.Intent.Slots["year"] != null &&
                                intentRequest.Intent.Slots["year"].Value != null &&
                                intentRequest.Intent.Slots["date"] != null &&
                                intentRequest.Intent.Slots["date"].Value != null)
                            {
                                DateTime dateValue = DateTime.Parse(intentRequest.Intent.Slots["date"].Value.ToString());

                                dateValue = dateValue.AddYears(int.Parse(intentRequest.Intent.Slots["year"].Value.ToString()) - dateValue.Year);

                                int result = (DateTime.Now - dateValue).Days / 365;

                                response = ResponseBuilder.Tell($"you are {result} years old");
                                response.Response.ShouldEndSession = true;

                            }
                            else
                            {
                                response = ResponseBuilder.Ask("Please tell me, when were you born?", null);
                            }
                        }
                        else
                        {
                            response = ResponseBuilder.Ask("Please tell me, when were you born?", null);
                        }
                        break;
                }

            }
            

            return new OkObjectResult(response);
        }

    }
}

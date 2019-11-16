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
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

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
            var client = new HttpClient();
            
            client.DefaultRequestHeaders.Add("X-Query-Key", "NRIQ-Do69S-UGnW9QMIEyErTygz5XXT73u--L");
            SkillResponse response = null;

            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Welcome to MacdonalsApp. Which store do you want to consult!");
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
                            if (intentRequest.Intent.Slots["storeName"] != null)
                            {
                                string store = intentRequest.Intent.Slots["storeName"].Value.ToString();
                                string query = "https://insights-api.newrelic.com/v1/accounts/1966971/query?nrql=SELECT%20id%2C%20amount%2Cname%2C%20city%20FROM%20StoreUpdate%20SINCE%201%20day%20ago%20%20LIMIT%2020";

                                var res = await client.GetStringAsync(query);
                                try
                                {
                                    var data = JsonConvert.DeserializeObject<Data>(res);
                                    var events = data.Results[0].Events;
                                    var eventSelected = events.First(p => p.Id == store);

                                   //response = ResponseBuilder.Tell($"The store {eventSelected.Name} located in {eventSelected.City} has sales for located in {eventSelected.Amount} dollars");

                                    var speechInvitation = new SsmlOutputSpeech();
                                    speechInvitation.Ssml = $"<speak><voice name=\"Enrique\"><prosody rate=\"medium\"><lang xml:lang=\"es-ES\">La tienda {eventSelected.Name} ubicada en {eventSelected.City} tiene ventas por {Math.Round(eventSelected.Amount,2)} dolares</lang></prosody></voice></speak>";
                                    response = ResponseBuilder.Tell(speechInvitation);


                                    response.Response.ShouldEndSession = true;
                                        
                                }
                                catch (Exception ex)
                                {

                                }
                               
                               
                            }
                            else
                            {
                                response = ResponseBuilder.Ask("Please, say the store number again?", null);
                            }
                        }
                        else
                        {
                            response = ResponseBuilder.Ask("Please, say the store number again?", null);
                        }
                        break;
                }

            }
            

            return new OkObjectResult(response);
        }

    }
}

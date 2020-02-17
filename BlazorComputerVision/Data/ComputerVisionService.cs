using BlazorComputerVision.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BlazorComputerVision.Data
{
    public class ComputerVisionService
    {
        static string subscriptionKey;
        static string endpoint;
        static string uriBase;

        public ComputerVisionService()
        {
            subscriptionKey = "b993f3afb4e04119bd8ed37171d4ec71";
            endpoint = "https://ankitocrdemo.cognitiveservices.azure.com/";
            uriBase = endpoint + "vision/v2.1/read/core/asyncBatchAnalyze";
        }

        public async Task<string> GetTextFromImage(byte[] imageFileBytes)
        {
            StringBuilder sb = new StringBuilder();
            string result = "No data detected";
            try
            {
                result = await ReadTextFromStream(imageFileBytes);

                ComputerVision computerVision = JsonConvert.DeserializeObject<ComputerVision>(result);
                if (computerVision.Status != null && computerVision.Status.Equals("Succeeded"))
                {
                    foreach (Line line in computerVision.RecognitionResults[0].Lines)
                    {
                        sb.Append(line.Text);
                        sb.AppendLine();
                    }
                    result = sb.ToString();
                }
                else
                {
                    dynamic errroMessage = JToken.Parse(result);
                    result = errroMessage.error.message;
                }
                return result;
            }
            catch
            {
                result = "Error occurred. Try again";
                return result;
            }
        }

        static async Task<string> ReadTextFromStream(byte[] byteData)
        {
            string result;
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
                string uri = uriBase;
                HttpResponseMessage response;
                string operationLocation;

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    response = await client.PostAsync(uri, content);
                }

                if (response.IsSuccessStatusCode)
                    operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                else
                {
                    string errorString = await response.Content.ReadAsStringAsync();
                    return errorString;
                }

                string contentString;
                int i = 0;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    response = await client.GetAsync(operationLocation);
                    contentString = await response.Content.ReadAsStringAsync();
                    ++i;
                }
                while (i < 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);

                if (i == 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1)
                {
                    result = "Timeout error.";
                    return result;
                }

                result = JToken.Parse(contentString).ToString();
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}

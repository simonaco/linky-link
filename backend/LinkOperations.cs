using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using System.Net;
using System.Security.Cryptography;

namespace LinkyLink
{
    public static partial class LinkOperations
    {
        [FunctionName("SaveLinks")]
        public static async Task<IActionResult> SaveLinks(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "links")] HttpRequest req,
            [CosmosDB(
                databaseName: "linkylinkdb",
                collectionName: "linkbundles",
                ConnectionStringSetting = "LinkLinkConnection"
            )] IAsyncCollector<LinkBundle> documents,
            ILogger log)
        {

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var linkDocument = JsonConvert.DeserializeObject<LinkBundle>(requestBody);

                if (!ValidatePayLoad(linkDocument, req, out ProblemDetails problems))
                {
                    return new BadRequestObjectResult(problems);
                }

                EnsureVanityUrl(linkDocument);

                await documents.AddAsync(linkDocument);

                return new CreatedResult($"/{linkDocument.VanityUrl}", linkDocument);
            }
            catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                log.LogError(ex, ex.Message);

                ProblemDetails exceptionDetail = new ProblemDetails
                {
                    Title = "Could not create link bundle",
                    Detail = "Vanity link already in use",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                    Instance = req.Path
                };
                return new BadRequestObjectResult(exceptionDetail);
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        private static void EnsureVanityUrl(LinkBundle linkDocument)
        {
            if (string.IsNullOrWhiteSpace(linkDocument.VanityUrl))
            {
                const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                var code = new char[7];
                var rng = new RNGCryptoServiceProvider();

                var bytes = new byte[8];
                for (int i = 0; i < code.Length; i++)
                {
                    rng.GetBytes(bytes);
                    code[i] =  characters[$"{BitConverter.ToUInt64(bytes) % (uint)characters.Length}"[0]];
                }

                linkDocument.VanityUrl = new String(code);
            }
        }

        private static bool ValidatePayLoad(LinkBundle linkDocument, HttpRequest req, out ProblemDetails problems)
        {
            bool isValid = linkDocument.Links?.Length > 0;
            problems = null;

            if (!isValid)
            {
                problems = new ProblemDetails()
                {
                    Title = "Payload is invalid",
                    Detail = "No links provided",
                    Status = StatusCodes.Status400BadRequest,
                    Type = "/linkylink/clientissue",
                    Instance = req.Path
                };
            }
            return isValid;
        }
    }
}
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace SpaWebApp.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarFactura(string destinatario, string asunto, string cuerpoHtml)
        {
            var apiKey = _configuration["EmailSettings:MailJetApiKey"];
            var apiSecret = _configuration["EmailSettings:MailJetSecretKey"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];
            var senderName = _configuration["EmailSettings:SenderName"];

            var client = new MailjetClient(apiKey, apiSecret);
            var request = new MailjetRequest
            {
                Resource = Send.Resource
            }
            .Property(Send.FromEmail, senderEmail)
            .Property(Send.FromName, senderName)
            .Property(Send.Subject, asunto)
            .Property(Send.HtmlPart, cuerpoHtml)
            .Property(Send.Recipients, new JArray {
                new JObject {
                    {"Email", destinatario}
                }
            });

            await client.PostAsync(request);
        }
    }
}

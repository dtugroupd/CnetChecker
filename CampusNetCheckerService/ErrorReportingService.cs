using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CampusNetCheckerService
{
    public class ErrorReportingService
    {
        private readonly ILogger<ErrorReportingService> _logger;
        private readonly IConfiguration _config;

        private readonly HttpClient _http;
        private readonly string _slackHookUrl;

        public ErrorReportingService(ILogger<ErrorReportingService> logger, IConfiguration config,
            IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _config = config;

            _http = httpFactory.CreateClient();

            var slackHookUrl = _config["Reporting:Slack:UrlHook"];
            if (slackHookUrl != null)
            {
                _slackHookUrl = slackHookUrl;
            }
        }

        public async Task ReportError(CampusNetException ex)
        {
            var status = await StatusSentTrackingService.GetStatus();
            if (status == SuccessStatus.Error) return;
            await StatusSentTrackingService.SetStatus(SuccessStatus.Error);

            _logger.LogInformation("Reporting error {ExceptionName}: {ExceptionMessage}", ex.GetType().Name,
                ex.Message);

            if (_slackHookUrl != null)
            {
                var message = new SlackMessage
                {
                    Text = "<!channel> CampusNet Login Failed",
                    LinkNames = 1,
                    Attachments = new List<SlackAttachment>
                    {
                        new SlackAttachment
                        {
                            AuthorName = ex.Title,
                            Text = ex.Message
                        }
                    }
                };

                await TryReport(() => PostToSlack(message), "Slack");
            }

            await Task.Run(() => { });
        }

        public async Task ReportSuccess()
        {
            var status = await StatusSentTrackingService.GetStatus();
            if (status == SuccessStatus.Success) return;
            await StatusSentTrackingService.SetStatus(SuccessStatus.Success);
            
            _logger.LogInformation("Reporting success");
            var slackMessage = new SlackMessage
            {
                Text = "<!channel> CampusNet Login Succeeded",
                LinkNames = 1,
                Attachments = new List<SlackAttachment>
                {
                    new SlackAttachment
                    {
                        AuthorName = "Login succeeded",
                        Text = "Automation service now succeeded at signing a user in on CampusNet.",
                        Color = "#13CE66"
                    }
                }
            };

            await TryReport(() => PostToSlack(slackMessage), "Slack");
        } 

        private async Task TryReport(Func<Task> callbackDelegate, string serviceName)
        {
            try
            {
                await callbackDelegate();
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to report to service {ServiceName}: {Message}",serviceName, e.Message);
            }
        }

        private async Task PostToSlack(SlackMessage message)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            var json = JsonConvert.SerializeObject(message, jsonSettings);

            var request = new HttpRequestMessage(HttpMethod.Post, _slackHookUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP Request failed with status {response.StatusCode}");
            }

            _logger.LogInformation("Message sent");
        }
    }

    internal class SlackMessage
    {
        public string Text { get; set; }
        public int LinkNames = 1;
        public List<SlackAttachment> Attachments { get; set; } = new List<SlackAttachment>
        {
            new SlackAttachment()
        };
    }

    internal class SlackAttachment
    {
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public string Color { get; set; } = "#FF4949";
    }
}

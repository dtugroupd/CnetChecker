using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace CampusNetCheckerService
{
    public class CampusNetService
    {
        private readonly ILogger<CampusNetService> _logger;
        private readonly HttpClient _http;

        public CampusNetService(ILogger<CampusNetService> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _http = clientFactory.CreateClient();
        }

        public async Task<string> FetchLimitedPassword(string username, string password)
        {
            const string authUrl = "https://auth.dtu.dk/dtu/mobilapp.jsp";

            var auth = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Username", username),
                new KeyValuePair<string, string>("Password", password)
            };

            var authenticationRequest = new HttpRequestMessage(HttpMethod.Post, authUrl)
            {
                Content = new FormUrlEncodedContent(auth)
            };

            _logger.LogInformation("Trying to sign in as {Username}", username);
            var response = await _http.SendAsync(authenticationRequest);

            _logger.LogInformation("Reading content from HTTP response");
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogTrace("Got response {Response}", response);
            _logger.LogInformation("Parsing XML from response");
            var xml = XElement.Parse(content);

            if (!xml.Elements("LimitedAccess").Any())
            {
                if (xml.Elements("BlockedAccess").Any())
                {
                    var reason = xml.Element("BlockedAccess")?.Attribute("Reason")?.Value;
                    var test = CampusNetException.InvalidCredentials(reason);
                    throw CampusNetException.InvalidCredentials(reason);
                }
                else
                {
                    var error = xml.Element("Reason")?.Element("Text")?.Value ??
                                "Something went completely wrong, and we don't know what.'";
                    throw CampusNetException.UnknownError(error);
                }
            }

            var limitedPassword = xml.Element("LimitedAccess")?.Attribute("Password")?.Value;

            if (string.IsNullOrEmpty(limitedPassword))
            {
                throw CampusNetException.UnknownError("Something went wrong. LimitedPassword exists in XML, but is empty.");
            }

            if (limitedPassword.ToLower().Trim() == "LimitedAccessPasswordFAILED".ToLower())
            {
                throw CampusNetException.UnknownError("CampusNet seems to be down at the moment. Received LimitedAccessPasswordFAILED");
            }

            return limitedPassword;
        }
    }
}

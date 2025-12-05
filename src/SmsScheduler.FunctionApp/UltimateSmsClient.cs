using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wavebreak.SmsScheduler.Integration.Pack;

namespace Wavebreak.SmsScheduler.Integration.Pack
{
    public class UltimateSmsClient : ISmsClient
    {
        private readonly HttpClient _httpClient;
        private readonly SmsSettings _settings;
        private readonly ILogger<UltimateSmsClient> _logger;

        public UltimateSmsClient(
            HttpClient httpClient,
            IOptions<SmsSettings> smsOptions,
            ILogger<UltimateSmsClient> logger)
        {
            _httpClient = httpClient;
            _settings = smsOptions.Value;
            _logger = logger;
        }

        public async Task<SmsSendResult> SendSmsAsync(Contact contact)
        {
            // If sending is disabled, just log and pretend success
            if (!_settings.EnableSending)
            {
                _logger.LogInformation("Dry run: would send SMS to {Phone} with message: {Message}",
                    contact.Phone,
                    contact.Message);


                return new SmsSendResult
                {
                    Success = true,
                    ProviderMessageId = null,
                    Error = null
                };
            }

            var result = new SmsSendResult();

            try
            {
                var baseUri = new Uri(_settings.ApiBaseUrl);
                var url = new Uri(baseUri, _settings.MessagesEndpoint);

                var body = new Dictionary<string, string>
                {
                    ["api_key"] = _settings.ApiKey,
                    ["senderid"] = _settings.SenderId,
                    ["type"] = _settings.Type,
                    ["contacts"] = contact.Phone,
                    ["message"] = contact.Message
                };

                using var content = new FormUrlEncodedContent(body);
                var response = await _httpClient.PostAsync(url, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Sent SMS to {Phone}. StatusCode={StatusCode}. Response={Response}",
                        contact.Phone,
                        (int)response.StatusCode,
                        responseText);


                    result.Success = true;
                    result.ProviderMessageId = null;
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to send SMS to {Phone}. StatusCode={StatusCode}. Response={Response}",
                        contact.Phone,
                        (int)response.StatusCode,
                        responseText);

                    result.Success = false;
                    result.Error = $"Http {(int)response.StatusCode}; {responseText}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while Sending SMS to {Phone}",
                    contact.Phone);

                result.Success = false;
                result.Error = ex.Message;
            }

            return result;


        }
    }
}

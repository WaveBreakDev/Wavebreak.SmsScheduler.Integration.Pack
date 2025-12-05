using System;

namespace Wavebreak.SmsScheduler.Integration.Pack
{
    /// <summary>
    /// Configuration settings for the SMS provider.
    /// These will be bound from local.settings.json / application settings.
    /// </summary>
    public class SmsSettings
    {
        // Base URL of the SMS API, e.g. http://<UltimateSMS.domain>
        public string ApiBaseUrl { get; set; } = string.Empty;

        // Endpoint path for sending messages, e.g. /api/http/sms/send
        public string MessagesEndpoint { get; set; } = string.Empty;

        // API key or token used to authenticate with the SMS provider
        public string ApiKey { get; set; } = string.Empty;

        // Phone number or sender ID used for "from" field
        public string SenderId { get; set; } = string.Empty;

        // Message type (often "plain" or "unicode")
        public string Type { get; set; } = "plain";

        // If false, we only log what would be sent (no real HTTP call)
        public bool EnableSending { get; set; } = false;
    }
}

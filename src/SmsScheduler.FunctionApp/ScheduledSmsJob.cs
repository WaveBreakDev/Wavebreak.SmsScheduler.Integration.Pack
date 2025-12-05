using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wavebreak.SmsScheduler.Integration.Pack
{
    public class ScheduledSmsJob
    {
        private readonly ILogger _logger;
        private readonly SmsSettings _smsSettings;

        private readonly CsvContactLoader _csvLoader;
        private readonly JsonContactLoader _jsonLoader;
        private readonly ISmsClient _smsClient;

        public ScheduledSmsJob(
            ILoggerFactory loggerFactory,
            IOptions<SmsSettings> smsOptions,
            CsvContactLoader csvLoader,
            JsonContactLoader jsonLoader,
            ISmsClient smsClient)
        {
            _logger = loggerFactory.CreateLogger<ScheduledSmsJob>();
            _smsSettings = smsOptions.Value;
            _csvLoader = csvLoader;
            _jsonLoader = jsonLoader;
            _smsClient = smsClient;
        }

        // Dev schedule: every 30 seconds
        [Function("ScheduledSmsJob")]
        public async Task RunAsync([TimerTrigger("*/30 * * * * *")] TimerInfo myTimer)
        {
            var now = DateTime.UtcNow;
            _logger.LogInformation("ScheduledSmsJob executed at: {ExecutionTimeUtc}", now);

            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation("Next schedule at: {NextScheduleUtc}", myTimer.ScheduleStatus.Next);
            }

            // Log configuration (without leaking the API key)
            _logger.LogInformation(
                "Using SMS API at {BaseUrl}{Endpoint} with SenderId {SenderId}, Type {Type}, EnableSending={EnableSending}",
                _smsSettings.ApiBaseUrl,
                _smsSettings.MessagesEndpoint,
                _smsSettings.SenderId,
                _smsSettings.Type,
                _smsSettings.EnableSending
            );

            var contactsFile = Path.Combine(AppContext.BaseDirectory, "Data", "contacts-sample.csv");
            var contacts = await _csvLoader.LoadContactsAsync(contactsFile);

            _logger.LogInformation("Loaded {Count} contacts.", contacts.Count);

            int successCount = 0;
            int failureCount = 0;

            foreach (var contact in contacts)
            {
                var result = await SendWithRetryAsync(contact);

                if (result.Success)
                {
                    successCount++;

                    _logger.LogInformation(
                        "SMS send success for {Phone}. ProviderMessageId={MessageId}",
                        contact.Phone,
                        result.ProviderMessageId ?? "(none)");
                }
                else
                {
                    failureCount++;

                    _logger.LogWarning(
                        "SMS send FAILED for {Phone}. Error={Error}",
                        contact.Phone,
                        result.Error ?? "(unknown error)");
                }
            }

            _logger.LogInformation(
                "Job complete. TotalContacts={Total}, Success={Success}, Failed={Failed}",
                contacts.Count,
                successCount,
                failureCount);
        }

        private async Task<SmsSendResult> SendWithRetryAsync(Contact contact, int maxAttempts = 3)
        {
            var attempt = 0;

            while (true)
            {
                attempt++;

                _logger.LogInformation(
                    "Sending SMS to {Phone}. Attempt {Attempt}/{MaxAttempts}",
                    contact.Phone,
                    attempt,
                    maxAttempts);

                var result = await _smsClient.SendSmsAsync(contact);

                if (result.Success)
                {
                    return result;
                }

                if (attempt >= maxAttempts)
                {
                    _logger.LogWarning(
                        "Giving up sending SMS to {Phone} after {Attempts} attempts. Last error: {Error}",
                        contact.Phone,
                        attempt,
                        result.Error ?? "(unknown error)");

                    return result;
                }

                // Simple backoff: 2s, 4s, 6s...
                var delaySeconds = 2 * attempt;
                _logger.LogInformation(
                    "Retrying SMS to {Phone} in {DelaySeconds} seconds...",
                    contact.Phone,
                    delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}

# Configuration Reference – Wavebreak SMS Scheduler Integration Pack

This reference explains every configuration value used by the SMS Scheduler, where each setting is stored, and how it's interpreted by the application.

------------------------------------------------------------
## 1. Azure Functions Core Settings
------------------------------------------------------------

These settings are required for Azure Functions to run.

------------------------------------------------------------
### FUNCTIONS_WORKER_RUNTIME
- **Location**
  - local.settings.json → Values
  - Azure → Application Settings
- **Type:** string
- **Value:** dotnet-isolated
- **Purpose:** Instructs Azure Functions to use the .NET Isolated Worker model.

------------------------------------------------------------
### AzureWebJobsStorage
- **Location:** Values
- **Type:** string
- **Purpose:** Connection string for Azure Storage used internally by Azure Functions.
- **Local Development:** UseDevelopmentStorage=true

Example (local):

"AzureWebJobsStorage": "UseDevelopmentStorage=true"

Example (Azure):
A full connection string pulled from your Storage Account → Access Keys.

------------------------------------------------------------
## 2. SMS Settings (SmsSettings)
------------------------------------------------------------

These map directly to the `SmsSettings` class via IOptions<SmsSettings>.

Class definition for reference:

public class SmsSettings
{
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string MessagesEndpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string Type { get; set; } = "plain";
    public bool EnableSending { get; set; } = false;
}

Each setting below corresponds to `SmsSettings:<key>`.

------------------------------------------------------------
### SmsSettings:ApiBaseUrl
- **Type:** string  
- **Example:** https://app.usapers.com  
- **Purpose:** Base URL of your SMS provider’s HTTP API.  

Used inside UltimateSmsClient to build the final endpoint URL.

------------------------------------------------------------
### SmsSettings:MessagesEndpoint
- **Type:** string  
- **Example:** /api/http/sms/send  
- **Purpose:** Path appended to ApiBaseUrl to form the send-SMS endpoint.

Final URL = ApiBaseUrl + MessagesEndpoint

------------------------------------------------------------
### SmsSettings:ApiKey
- **Type:** string  
- **Purpose:** API key/token for the SMS provider.  
- **Security warning:** Should ONLY be stored in Azure App Settings or Key Vault.  
Never store in source control.

------------------------------------------------------------
### SmsSettings:SenderId
- **Type:** string  
- **Examples:**  
  - 16562017190  
  - WAVEBREAK  
- **Purpose:** SMS sender or phone number registered with your provider.

------------------------------------------------------------
### SmsSettings:Type
- **Type:** string  
- **Default:** plain  
- **Examples:** plain, unicode  
- **Purpose:** Used by some SMS providers to specify encoding or message format.

------------------------------------------------------------
### SmsSettings:EnableSending
- **Type:** boolean  
- **Default:** false  
- **Purpose:** Controls whether real SMS messages are sent.

When false:
- All messages are logged as dry-run.
- No outbound requests are made.
- Safe for local development and first deployment.

When true:
- Real HTTP POST requests are performed.
- Messages are actually sent.

------------------------------------------------------------
## 3. Contact File Configuration
------------------------------------------------------------

The job loads contacts from a file deployed with the function.

Default path:

/Data/contacts-sample.csv

Properties required on this file:

- Build Action: None  
- Copy to Output Directory: Copy if newer  

CSV format:

Name,Phone,Message
John Doe,15551234567,Hello this is your reminder.
Mary Smith,18005557890,Your appointment is tomorrow.

JSON format also supported.

------------------------------------------------------------
## 4. Timer Trigger Schedule
------------------------------------------------------------

Defined in the ScheduledSmsJob attribute:

[TimerTrigger("*/30 * * * * *")]

Meaning:
- Every 30 seconds
- CRON format: second minute hour day month day-of-week

Production examples:

Every 2 hours on the hour:
[TimerTrigger("0 0 */2 * * *")]

Every day at 8am UTC:
[TimerTrigger("0 0 8 * * *")]

You can make this dynamic later via host.json or environment variables.

------------------------------------------------------------
## 5. Logging Behavior
------------------------------------------------------------

All logs flow through ILogger and can be viewed in:

- Local terminal (func start)
- Visual Studio output window
- Azure Portal → Log Stream

Extra logging includes:
- Send attempts
- Retry attempts
- Backoff timing
- Final summary: Total, Success, Failed

------------------------------------------------------------
## 6. Additional Future Configuration Options
------------------------------------------------------------

(These are not implemented yet but are designed for extensibility.)

- SmsSettings:RetryAttempts  
- SmsSettings:RetryBackoffSeconds  
- ContactSource:BlobContainerName  
- ContactSource:BlobFileName  
- ContactSource:FileMakerApiEndpoint  

These can be added as your integration pack evolves.

------------------------------------------------------------
## End of Configuration Reference
------------------------------------------------------------


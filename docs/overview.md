# Wavebreak SMS Scheduler Integration Pack

The Wavebreak SMS Scheduler Integration Pack is a modular, production-shaped example of how to run scheduled SMS jobs using Azure Functions (Isolated .NET) and an HTTP-based SMS provider such as Ultimate SMS.

It is designed to be:

- Senior-level reference code
- Easy to extend or adapt to multiple SMS vendors
- Safe to test (dry-run mode prevents accidental sending)
- Structured for real-world projects (DI, retry logic, logging, configuration)

------------------------------------------------------------
## Architecture Overview
------------------------------------------------------------

Components:

1. Scheduled Function (Timer Trigger)
   - Executes every X minutes/hours based on configuration
   - Loads contacts
   - Sends SMS messages with retry logic
   - Logs summaries

2. Contact Loading Layer
   - IContactLoader interface
   - CsvContactLoader and JsonContactLoader implementations
   - Allows easy switching to blob storage, FileMaker Data API, or databases

3. SMS Sending Layer
   - ISmsClient interface
   - UltimateSmsClient implementation using HttpClient
   - Supports dry-run vs real sending

4. Configuration Binding
   - SmsSettings class (ApiBaseUrl, ApiKey, SenderId, EnableSending)
   - Bound automatically through IOptions<SmsSettings>

5. Retry + Logging System
   - Configurable retry attempts
   - Simple exponential-ish backoff
   - Final send summary (total, success, failed)

------------------------------------------------------------
## How a Job Run Works
------------------------------------------------------------

1. Timer fires (local = every 30 seconds, production = every 2 hours)
2. ScheduledSmsJob logs the execution time and next scheduled time
3. SmsSettings loads from configuration (local.settings.json or Azure)
4. Contacts are loaded from a CSV file in /Data
5. For each contact:
   a. A retry loop attempts to send the SMS
   b. On success: logs ProviderMessageId
   c. On failure: logs error reason
6. Summary is logged:
   - TotalContacts: X
   - Success: Y
   - Failed: Z

------------------------------------------------------------
## Dry-Run Mode
------------------------------------------------------------

local.settings.json:

"EnableSending": false

When false:
- No HTTP calls are made
- SMS content is logged as "Dry run: would send SMS to …"
- System behaves as if messages are successfully sent

Switch EnableSending to true ONLY after verifying everything in Azure.

------------------------------------------------------------
## Contact File Format
------------------------------------------------------------

CSV format:

Name,Phone,Message
John Doe,15551234567,Hello this is your reminder.
Mary Smith,18005557890,Your appointment is tomorrow.

JSON format is also supported via JsonContactLoader.

------------------------------------------------------------
## Project Structure
------------------------------------------------------------

src/
  SmsScheduler.FunctionApp/
    Contact.cs
    CsvContactLoader.cs
    JsonContactLoader.cs
    IContactLoader.cs
    ISmsClient.cs
    SmsSendResult.cs
    SmsSettings.cs
    ScheduledSmsJob.cs
    UltimateSmsClient.cs
    Program.cs
    host.json
    Data/
      contacts-sample.csv

docs/
  overview.md
  setup-azure.md
  config-reference.md

examples/
  contacts-sample.csv
  contacts-sample.json
  local.settings.sample.json

------------------------------------------------------------
## Summary
------------------------------------------------------------

This integration pack demonstrates a complete, senior-level architecture for scheduled outbound SMS operations using Azure Functions. The project is structured to be maintainable, testable, and extensible, and serves as a strong example of clean .NET design and integration practices.


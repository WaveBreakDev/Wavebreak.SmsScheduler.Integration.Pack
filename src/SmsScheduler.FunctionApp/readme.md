# Wavebreak SMS Scheduler Integration Pack

The Wavebreak SMS Scheduler Integration Pack is a clean, modular, senior-level example of how to build a scheduled outbound SMS system using **Azure Functions (.NET Isolated)** and an HTTP-based SMS provider (Ultimate SMS, Telnyx, etc.).

This project is designed to demonstrate:
- Production-ready architecture
- Clean dependency injection
- Retry logic and fault handling
- Safe dry-run mode
- Configurable SMS provider integration
- Extensible contact loading (CSV, JSON, future: Blob, FileMaker, SQL)

It is ideal as a portfolio piece showing mastery of .NET backend workflows, Azure Functions, and integration design.

------------------------------------------------------------
## Features
------------------------------------------------------------

### ✔ Timer-Based SMS Scheduler
A scheduled Azure Function that executes on configurable CRON intervals (e.g., every 2 hours).

### ✔ CSV/JSON Contact Loading
Supports pluggable contact sources through the `IContactLoader` interface.

### ✔ SMS Provider Abstraction
`ISmsClient` interface with a concrete `UltimateSmsClient` implementation.

### ✔ Safe Dry-Run Mode
Set `EnableSending=false` to prevent accidental SMS sending—logs only.

### ✔ Retry Logic
Automatic retry (up to 3 attempts) with incremental backoff.

### ✔ Execution Summary Logging
Logs total contacts, successes, and failures at the end of each run.

### ✔ Azure-Ready Configuration
All settings (API keys, sender ID, provider URLs) loaded via App Settings or local.settings.json.

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
## How It Works
------------------------------------------------------------

### 1. Timer Trigger Starts the Job
The job runs every 30 seconds locally, or on a production schedule in Azure.

### 2. Contacts Are Loaded
Default: `/Data/contacts-sample.csv`

Example CSV:

Name,Phone,Message
John Doe,15551234567,Hello this is your reminder.

### 3. Messages Are Sent (or simulated)
If `EnableSending=false`, each message produces:

"Dry run: would send SMS to 15551234567 with message: Hello this is your reminder."

### 4. Retry Logic for Failures
Up to 3 attempts with backoff (2s, 4s, 6s).

### 5. Summary Output
At the end of each run:

"Job complete. TotalContacts=X, Success=Y, Failed=Z"

------------------------------------------------------------
## Configuration Overview
------------------------------------------------------------

Settings come from Azure App Settings or local.settings.json:

SmsSettings:
  ApiBaseUrl: Base URL for the SMS provider
  MessagesEndpoint: Path to send messages
  ApiKey: Provider API key (secure)
  SenderId: SMS sender or phone number
  Type: Message type (plain, unicode)
  EnableSending: false = dry run, true = real sending

Full configuration reference in `docs/config-reference.md`.

------------------------------------------------------------
## Running Locally
------------------------------------------------------------

1. Create or verify `local.settings.json`:

{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  },
  "SmsSettings": {
    "ApiBaseUrl": "https://app.your-sms-provider.com",
    "MessagesEndpoint": "/api/http/sms/send",
    "ApiKey": "YOUR_API_KEY",
    "SenderId": "YOUR_SENDER_ID",
    "Type": "plain",
    "EnableSending": false
  }
}

2. Press F5  
3. Watch the job execute every 30 seconds  
4. Check the logs for dry-run or real-send behavior depending on configuration

------------------------------------------------------------
## Deploying to Azure
------------------------------------------------------------

The included deployment guide is in:

docs/setup-azure.md

Summary:
- Publish the function from Visual Studio
- Configure App Settings in Azure
- Enable or disable real sending
- Monitor the timer-trigger logs in Azure Portal

------------------------------------------------------------
## Extensibility
------------------------------------------------------------

This pack is intentionally structured for extension:

- Add Telnyx or Twilio as another `ISmsClient`
- Load contacts from Blob Storage
- Replace CSV loader with a FileMaker Data API loader
- Add batching for large contact lists
- Add message templates
- Push metrics to Databox, Application Insights, Power BI, etc.
- Add Key Vault integration for secure secret storage

------------------------------------------------------------
## Purpose and Portfolio Positioning
------------------------------------------------------------

This project demonstrates senior-level capabilities:

- Clean separation of concerns
- Dependency injection
- Async workflows
- Integration with external providers
- Azure cloud deployment
- Config-driven architecture
- Resilient background job design

Use it as a strong portfolio item for:
- Backend engineering roles
- API integration roles
- Azure developer roles
- Senior .NET roles
- Freelance/consulting proposals

------------------------------------------------------------
## License
------------------------------------------------------------

This integration pack is provided "as-is" for educational and demonstration purposes. No SMS provider credentials are included.


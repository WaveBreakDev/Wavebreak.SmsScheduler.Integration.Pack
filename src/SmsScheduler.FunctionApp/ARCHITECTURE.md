# Architecture – Wavebreak SMS Scheduler Integration Pack

This document describes the high-level architecture of the Wavebreak SMS Scheduler Integration Pack, including its layers, runtime flow, deployment model, and extensibility points.

---

## 1. High-Level Overview

The Wavebreak SMS Scheduler Integration Pack is a scheduled outbound messaging system built on:

- **Azure Functions (.NET Isolated)**
- **Timer triggers**
- **Config-driven HTTP SMS provider integration**
- **Pluggable contact loaders (CSV, JSON, future: API/DB)**

The core goals of the architecture are:

- Clear separation of concerns
- Easy swapping of providers and data sources
- Production-shaped patterns (DI, logging, retry logic)
- Safe dry-run mode for development and initial deployments

---

## 2. Architectural Layers

The solution is organized into the following logical layers:

1. **Hosting & Runtime**
   - Azure Functions Host (.NET Isolated)
   - `Program.cs` bootstrapping
   - Timer trigger configuration

2. **Orchestration (Job Layer)**
   - `ScheduledSmsJob`
   - Job scheduling, logging, and summary reporting
   - Retry orchestration per contact

3. **Configuration Layer**
   - `SmsSettings` model
   - `local.settings.json` / Azure App Settings
   - Binding via `IOptions<SmsSettings>`

4. **Contact Loading Layer**
   - `IContactLoader` abstraction
   - `CsvContactLoader`
   - `JsonContactLoader`
   - Future: Blob/FileMaker/Database loaders

5. **SMS Provider Layer**
   - `ISmsClient` abstraction
   - `UltimateSmsClient` implementation
   - `SmsSendResult` result model

6. **Data & Examples**
   - `/Data/contacts-sample.csv`
   - `/examples/*.csv`, `/examples/*.json`
   - Sample `local.settings.sample.json`

Each layer has a strict responsibility and communicates with adjacent layers via interfaces and strongly-typed models.

---

## 3. Runtime Flow

### 3.1 Sequence of a Job Run

1. **Timer Trigger Fires**
   - Azure Functions runtime invokes `ScheduledSmsJob.RunAsync(TimerInfo myTimer)` based on a CRON expression.

2. **Execution Logging**
   - The job logs:
     - Current execution time
     - Next scheduled time
     - SmsSettings summary (without secrets)

3. **Contact Loading**
   - The job determines the data source (currently CSV by default).
   - Calls `CsvContactLoader.LoadContactsAsync(filePath)` to get a `List<Contact>`.

4. **Per-Contact Processing**
   - For each `Contact`:
     - Calls `SendWithRetryAsync(contact)` in `ScheduledSmsJob`.
     - `SendWithRetryAsync` calls `ISmsClient.SendSmsAsync(contact)` (UltimateSmsClient).
     - Retry logic is applied if the send fails.

5. **Provider Call**
   - `UltimateSmsClient`:
     - If `EnableSending == false`:
       - Logs a dry-run message and returns a successful `SmsSendResult`.
     - If `EnableSending == true`:
       - Builds an HTTP POST request using `SmsSettings` and `Contact`.
       - Sends the request via `HttpClient`.
       - Interprets the response and builds `SmsSendResult`.

6. **Summary Logging**
   - After all contacts are processed:
     - `ScheduledSmsJob` logs:
       - Total contacts processed
       - Total successes
       - Total failures

---

## 4. Key Components and Relationships

### 4.1 Program.cs (Host Bootstrap)

Responsibilities:

- Configure configuration sources:
  - `local.settings.json` (Development)
  - Environment variables / Azure App Settings
- Bind `SmsSettings` using `IOptions<SmsSettings>`.
- Register services in DI:
  - `CsvContactLoader`
  - `JsonContactLoader`
  - `ISmsClient` via `UltimateSmsClient` using `AddHttpClient`.

This composition root keeps all wiring in one place and makes components easy to test and replace.

### 4.2 ScheduledSmsJob (Orchestrator)

- Depends on:
  - `ILoggerFactory`
  - `IOptions<SmsSettings>`
  - `CsvContactLoader`
  - `JsonContactLoader`
  - `ISmsClient`
- Contains:
  - Job entrypoint (`RunAsync`)
  - Retry helper (`SendWithRetryAsync`)

Orchestrates the entire lifecycle of a scheduled run.

### 4.3 Contact Loaders

- All loaders implement `IContactLoader`.
- `CsvContactLoader` and `JsonContactLoader` are interchangeable, allowing the orchestration layer to remain agnostic of format.
- Future loaders (Blob, FileMaker, SQL, etc.) will plug into the same interface.

### 4.4 SMS Client

- `ISmsClient` defines the contract:
  - `Task<SmsSendResult> SendSmsAsync(Contact contact)`
- `UltimateSmsClient` implements the HTTP-specific behavior.
- Future providers (e.g., Telnyx, Twilio) can be added as new classes implementing `ISmsClient`.

---

## 5. Error Handling & Retry Strategy

### 5.1 Retry Strategy

- Implemented in `SendWithRetryAsync`.
- Attempts up to a configured number of times (currently 3).
- Backoff pattern: 2s, 4s, 6s, etc.
- On final failure:
  - Logs a warning including the last error.
  - Returns a failed `SmsSendResult`.

### 5.2 Dry-Run Mode

- Configured via `SmsSettings.EnableSending`.
- When `false`:
  - No HTTP calls are made.
  - Messages are logged as “Dry run: would send SMS to {Phone}…”.
- Reduces risk when:
  - First setting up a project.
  - Running in non-production environments.
  - Validating data and schedule behavior.

### 5.3 Logging

- All major operations log via `ILogger`:
  - Job start and end
  - Configuration overview (non-sensitive)
  - Contact load count
  - Each send attempt and result
  - Retry events
  - Final summary

This produces a clear operational trace for debugging and monitoring.

---

## 6. Deployment Architecture

### 6.1 Azure Function App

Typical deployment topology:

- **Azure Function App**
  - Hosts the timer-triggered SMS job
  - Uses Consumption or Premium plan
- **Azure Storage Account**
  - Required by Azure Functions runtime
  - Can also be used for future Blob-based contact sources
- **Configuration**
  - Azure App Settings hold:
    - `SmsSettings:*` values
    - `AzureWebJobsStorage`
    - `FUNCTIONS_WORKER_RUNTIME`

### 6.2 Environments

Recommended environment separation:

- **Local Development**
  - Uses `local.settings.json`
  - `EnableSending = false`
  - Contacts from local CSV/JSON

- **Staging / Test**
  - Azure Function App in a non-production resource group
  - `EnableSending = true` with test contacts/provider
  - Used for integration verification

- **Production**
  - Separate Function App and resource group
  - Strictly controlled configuration and secrets
  - Real contacts and real SMS provider credentials

---

## 7. Extensibility Points

The architecture is designed to grow in several directions:

### 7.1 New SMS Providers

- Add a new class implementing `ISmsClient`.
- Add any provider-specific configuration to `SmsSettings`.
- Register the new client in `Program.cs`.

Example:
- `TelnyxSmsClient`
- `TwilioSmsClient`

### 7.2 New Contact Sources

- Implement `IContactLoader` for:
  - Azure Blob Storage
  - FileMaker Data API
  - SQL or NoSQL databases
  - REST APIs

- Adjust `ScheduledSmsJob` to select the appropriate loader (config-driven or based on environment).

### 7.3 Message Templates

- Introduce templating (Razor, simple token replacement, etc.).
- Build messages using:
  - Template + data model → `Contact.Message`.

### 7.4 Metrics & Monitoring

- Push metrics to:
  - Application Insights
  - Databox
  - Power BI
- Examples:
  - SMS sent per run
  - Failure rate
  - Retry counts

---

## 8. Summary

The Wavebreak SMS Scheduler Integration Pack demonstrates a clean, modular architecture for scheduled messaging workloads on Azure Functions:

- Clear orchestration layer via `ScheduledSmsJob`
- Configurable, DI-based composition via `Program.cs`
- Pluggable contact loaders via `IContactLoader`
- Provider-agnostic SMS client interface via `ISmsClient`
- Production-appropriate patterns for logging, retry, and configuration

This structure makes it both a practical foundation for real projects and a strong demonstration of senior-level .NET and Azure architectural skills.

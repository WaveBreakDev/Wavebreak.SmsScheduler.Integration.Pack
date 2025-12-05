# Component Reference – Wavebreak SMS Scheduler Integration Pack

This document describes each core component of the Wavebreak SMS Scheduler Integration Pack: what it does, where it lives, and how it fits into the overall architecture.

------------------------------------------------------------
## 1. ScheduledSmsJob (Main Function)
------------------------------------------------------------

**File:** src/SmsScheduler.FunctionApp/ScheduledSmsJob.cs  
**Type:** Azure Function (Timer Trigger)

### Responsibility
- Acts as the main orchestrator for each scheduled run.
- Logs execution time and next schedule.
- Loads contacts via CsvContactLoader / JsonContactLoader.
- Calls ISmsClient for sending messages with retry logic.
- Logs total successes and failures.

### Key Members

**Constructor dependencies**
- ILoggerFactory loggerFactory  
- IOptions<SmsSettings> smsOptions  
- CsvContactLoader csvLoader  
- JsonContactLoader jsonLoader  
- ISmsClient smsClient  

**RunAsync(TimerInfo myTimer)**
- Entry point for the timer trigger.
- Loads contacts.
- Iterates and sends SMS messages via SendWithRetryAsync.

**SendWithRetryAsync(Contact contact, int maxAttempts = 3)**
- Implements retry logic.
- Logs each attempt.
- Simple incremental backoff (2s, 4s, 6s).
- Returns SmsSendResult.

------------------------------------------------------------
## 2. SmsSettings (Configuration Model)
------------------------------------------------------------

**File:** src/SmsScheduler.FunctionApp/SmsSettings.cs  
**Type:** Strongly-typed configuration model

### Properties
- ApiBaseUrl : Base URL of SMS provider  
- MessagesEndpoint : Endpoint path for message sending  
- ApiKey : Provider API token  
- SenderId : SMS sender name/number  
- Type : Message type (plain/unicode)  
- EnableSending : Dry-run toggle  

### Purpose
Loaded via IOptions<SmsSettings> and used by UltimateSmsClient and ScheduledSmsJob.

------------------------------------------------------------
## 3. Contact Model
------------------------------------------------------------

**File:** src/SmsScheduler.FunctionApp/Contact.cs  
**Type:** Simple POCO

### Properties
- Name  
- Phone  
- Message  

### Purpose
Represents a single outbound SMS operation.

------------------------------------------------------------
## 4. Contact Loading Layer
------------------------------------------------------------

### 4.1 IContactLoader (Interface)

**File:** src/SmsScheduler.FunctionApp/IContactLoader.cs  

Defines:

Task<List<Contact>> LoadContactsAsync(string filePath);

### Purpose
Allows the function to load contacts from:
- CSV files  
- JSON files  
- Blob storage (future)  
- Databases (future)  
- FileMaker Data API (future)  

---

### 4.2 CsvContactLoader

**File:** src/SmsScheduler.FunctionApp/CsvContactLoader.cs  

### Behavior
- Reads CSV text file.
- Ignores the header row.
- Maps columns → Contact:
  - Column 0 = Name  
  - Column 1 = Phone  
  - Column 2 = Message  

### Input Example
Name,Phone,Message
John Doe,15551234567,Hello
Mary Smith,18005557890,Reminder

---

### 4.3 JsonContactLoader

**File:** src/SmsScheduler.FunctionApp/JsonContactLoader.cs  

### Behavior
- Reads JSON array.
- Deserializes List<Contact>.
- Returns empty list if null.

### JSON Example
[
  { "name": "John", "phone": "1555", "message": "Hello" },
  { "name": "Mary", "phone": "1888", "message": "Reminder" }
]

------------------------------------------------------------
## 5. SMS Client Layer
------------------------------------------------------------

### 5.1 ISmsClient (Interface)

**File:** src/SmsScheduler.FunctionApp/ISmsClient.cs  

Method:

Task<SmsSendResult> SendSmsAsync(Contact contact);

### Purpose
Abstracts the SMS provider:
- Ultimate SMS  
- Telnyx  
- Twilio  
- Any API-based SMS backend  

---

### 5.2 SmsSendResult

**File:** src/SmsScheduler.FunctionApp/SmsSendResult.cs  

### Properties
- Success  
- ProviderMessageId  
- Error  

Used by ScheduledSmsJob and retry logic.

---

### 5.3 UltimateSmsClient

**File:** src/SmsScheduler.FunctionApp/UltimateSmsClient.cs  

### Responsibilities
- Implements SendSmsAsync(Contact contact).
- Builds HTTP POST request for SMS provider.
- Follows Ultimate SMS-style API.

### Behavior
- If EnableSending = false → dry run (no HTTP calls).
- If EnableSending = true → real POST via HttpClient.

### POST Body
api_key  
senderid  
type  
contacts  
message  

### Error Handling
- If HTTP status is not success → return failed result.
- On exception → log and return failed result.

------------------------------------------------------------
## 6. Program.cs (Host Bootstrap)
------------------------------------------------------------

**File:** src/SmsScheduler.FunctionApp/Program.cs  

### Responsibilities
- Builds Azure Functions Host.
- Binds SmsSettings into DI using IOptions<T>.
- Registers:
  - CsvContactLoader
  - JsonContactLoader
  - ISmsClient using AddHttpClient
- Sets up HttpClient.BaseAddress if configured.

------------------------------------------------------------
## 7. Data and Example Files
------------------------------------------------------------

### /Data/contacts-sample.csv
Used for local testing.

Build settings:
- Build Action: None  
- Copy to Output Directory: Copy if newer  

---

### /examples/
Contains example CSV/JSON files and sample local.settings.json.

------------------------------------------------------------
## 8. Architectural Summary
------------------------------------------------------------

1. Timer trigger activates ScheduledSmsJob.
2. Contacts are loaded via CsvContactLoader or JsonContactLoader.
3. Each contact is processed by SendWithRetryAsync.
4. ISmsClient.SendSmsAsync handles HTTP calls or dry-run logic.
5. Logs capture each event and summarize results.

This layered architecture makes the solution:
- Extensible  
- Testable  
- Provider-agnostic  
- Production-ready  

# Setup Guide – Deploying the Wavebreak SMS Scheduler to Azure

This guide explains how to deploy the SmsScheduler.FunctionApp to Azure Functions, configure it for scheduled execution, and manage environment settings such as SMS provider credentials.

------------------------------------------------------------
## 1. Prerequisites
------------------------------------------------------------

You will need:

- An Azure subscription
- Visual Studio 2022 (with Azure workload installed)
- .NET 8 SDK
- Storage account (created automatically during publish)
- SMS provider account (e.g., Ultimate SMS or Telnyx)
- The repository cloned locally

Before publishing, make sure the function runs locally.

------------------------------------------------------------
## 2. Verify Local Execution
------------------------------------------------------------

1. Ensure `src/SmsScheduler.FunctionApp/local.settings.json` exists and contains:

{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  },
  "SmsSettings": {
    "ApiBaseUrl": "https://app.your-sms-provider.com",
    "MessagesEndpoint": "/api/http/sms/send",
    "ApiKey": "YOUR_DEV_API_KEY",
    "SenderId": "YOUR_SENDER_ID",
    "Type": "plain",
    "EnableSending": false
  }
}

2. Run the project.
3. Confirm logs show:
   - Timer trigger firing
   - Contacts loading
   - Dry-run messages
   - Summary totals

Once local verification is successful, move on to deployment.

------------------------------------------------------------
## 3. Create the Azure Function App
------------------------------------------------------------

1. In Visual Studio:
   - Right-click **SmsScheduler.FunctionApp**
   - Choose **Publish…**

2. Select:
   - Azure
   - Azure Function App (Windows)

3. Choose your subscription.

4. Create a new Function App:
   - **Name:** wavebreak-sms-scheduler-dev (or similar)
   - **Runtime stack:** .NET
   - **Operating System:** Windows
   - **Plan type:** Consumption
   - **Storage account:** Select existing or create new

5. After creation, a publish profile is generated.

------------------------------------------------------------
## 4. Add App Settings in Azure
------------------------------------------------------------

In Azure Portal:

1. Open your Function App.
2. Go to:
   - Settings → Configuration → Application settings

3. Add the following keys:

FUNCTIONS_WORKER_RUNTIME = dotnet-isolated  
AzureWebJobsStorage = (your storage connection string)  
SmsSettings:ApiBaseUrl = https://app.your-sms-provider.com  
SmsSettings:MessagesEndpoint = /api/http/sms/send  
SmsSettings:ApiKey = (real API key)  
SmsSettings:SenderId = (your sender id or phone number)  
SmsSettings:Type = plain  
SmsSettings:EnableSending = false  

4. Click **Save**.

The Function App will restart.

------------------------------------------------------------
## 5. Publish from Visual Studio
------------------------------------------------------------

1. Right-click **SmsScheduler.FunctionApp** → **Publish…**
2. Select your publish profile
3. Click **Publish**

Once deployment finishes, Visual Studio will show the Function App URL.

------------------------------------------------------------
## 6. Verify the Deployment
------------------------------------------------------------

In Azure Portal:

1. Go to **Functions** inside your Function App.
2. Select **ScheduledSmsJob**.
3. Open **Log streaming**.

You should see:

- ScheduledSmsJob executed…
- Using SMS API at …
- Loaded X contacts…
- Dry run messages…

The timer trigger will run on the same schedule defined in code.

------------------------------------------------------------
## 7. Enable Real Sending
------------------------------------------------------------

After verifying dry-run behavior:

1. Go to:
   - Azure Portal → Function App → Configuration

2. Change:

SmsSettings:EnableSending = true

3. Save the configuration.

This will activate real outgoing SMS sending.

------------------------------------------------------------
## 8. Adjusting the Timer Schedule
------------------------------------------------------------

Development schedule (every 30 seconds):

[TimerTrigger("*/30 * * * * *")]

Production example (every 2 hours):

[TimerTrigger("0 0 */2 * * *")]

You can move CRON expressions into configuration later if you want dynamic schedules.

------------------------------------------------------------
## 9. Next Enhancements
------------------------------------------------------------

- Load contacts from Blob Storage instead of local CSV file
- Integrate with FileMaker via Data API
- Add metrics and push summary data to dashboards
- Add support for multiple SMS providers behind the ISmsClient interface
- Implement batching for very large contact lists

------------------------------------------------------------
## End of Setup Guide
------------------------------------------------------------


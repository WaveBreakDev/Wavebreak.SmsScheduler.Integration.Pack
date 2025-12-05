using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wavebreak.SmsScheduler.Integration.Pack;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, configBuilder) =>
    {
        // local.settings.json + environment variables are loaded automatically
        // so we usually don't need extra config here.
    })
    .ConfigureServices((context, services) =>
    {
        // Bind SmsSettings from configuration section "SmsSettings"
        services.Configure<SmsSettings>(
            context.Configuration.GetSection("SmsSettings"));

        // Register loaders
        services.AddSingleton<CsvContactLoader>();
        services.AddSingleton<JsonContactLoader>();

        services.AddHttpClient<ISmsClient, UltimateSmsClient>(client =>
        {
            //Base address is optional; we also build full URIs in the client.
            if (!string.IsNullOrWhiteSpace(context.Configuration["SmsSettings:ApiBaseUrl"]))
            {
                client.BaseAddress = new Uri(context.Configuration["SmsSettings:ApiBaseUrl"]!);
            }
        });
    })
    .Build();

host.Run();

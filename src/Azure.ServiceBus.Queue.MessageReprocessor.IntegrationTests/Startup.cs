using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.ServiceBus.Queue.MessageReprocessor.IntegrationTests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var uri = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI");
            var clientId = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_CLIENT_SECRET");

            var configurationBuilder = new ConfigurationBuilder();
            if (!string.IsNullOrEmpty(uri) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
            {
                configurationBuilder
                    .AddAzureKeyVault(uri, clientId, clientSecret);
            }

            var configuration = configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<ServiceBusConfiguration>()
                .AddEnvironmentVariables()
                .Build();

            var instance = new ServiceBusConfiguration();
            configuration.Bind("ServiceBusConfiguration", instance);

            services
                .AddSingleton(instance)
                .AddOptions();
        }
    }
}

using Console;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {

        var configurationBuilder = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfigurationRoot configuration = configurationBuilder.Build();

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddScoped<IApp, App>();
        builder.Services.AddSingleton(configuration);
        builder.Services.AddScoped<IProxy, Proxy>();
        
        builder.Services.AddHttpClient();

        IHost host = builder.Build();

        IApp app = host.Services.GetRequiredService<IApp>();

        app.Run(args);

    }
}
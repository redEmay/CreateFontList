using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CreateFontList
{
    public class AppSettings : IAppSettings
    {
        private const string CONFIGURATION_FILE = "appsettings.json";
        public Locations Location { get; }
        public AppSettings()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile(CONFIGURATION_FILE, false).Build();

            var serviceProvider = new ServiceCollection().
                AddOptions()
                .Configure<Locations>(o => configuration.GetSection(nameof(Locations)).Bind(o)).BuildServiceProvider();

            Location = serviceProvider.GetRequiredService<IOptions<Locations>>().Value;
        }
    }
}

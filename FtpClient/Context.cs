using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Client.Database;
using Client.Logger;

namespace Client
{
    public class Context
    {
        private IServiceCollection? _services;
        public IConfiguration? Configuration { get; set; }
        public IServiceProvider? ServiceProvider { get; set; }

        public void BuildServiceProvider(IServiceCollection services)
        {
            _services = services;
            ServiceProvider = _services.BuildServiceProvider();
        }
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddSingleton(typeof(ILoggerFactory), typeof(LoggerFactory));
			services.AddSingleton<ILoggerService, LoggerService>();
			services.AddSingleton<FtpClient>();
			services.AddSingleton<DbController>();
        }
    }
}

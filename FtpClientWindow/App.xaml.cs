using Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;

namespace FtpClientWindow
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{

		protected override async void OnStartup(StartupEventArgs e)
		{
			Context context = new Context();
			context.Configuration = new ConfigurationBuilder()
				.AddJsonFile("config.json")
				.Build();
			IServiceCollection services = new ServiceCollection();

			services.AddSingleton(context.Configuration);
			services.AddSingleton(context);
			Context.ConfigureServices(services);
			context.BuildServiceProvider(services);


			base.OnStartup(e);
			var ftpClient = context.ServiceProvider!.GetRequiredService<FtpClient>();
			MainWindow mainWindow = new MainWindow(ftpClient);
			ftpClient.OnCommandSent = mainWindow.PrintCommand;
			ftpClient.OnResponseReceived = mainWindow.PrintResponse;
			ftpClient.OnRespDataReceived = mainWindow.HandleResponseData;
			mainWindow.Show();
		}
	}

}

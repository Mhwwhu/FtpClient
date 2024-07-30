using Client;
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
			base.OnStartup(e);
			var _ftpClient = new FtpClient();
			MainWindow mainWindow = new MainWindow(_ftpClient);
			_ftpClient.OnCommandSent = mainWindow.PrintCommand;
			_ftpClient.OnResponseReceived = mainWindow.PrintResponse;
			_ftpClient.OnRespDataReceived = mainWindow.HandleResponseData;
			mainWindow.Show();
		}
	}

}

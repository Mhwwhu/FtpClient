using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Client;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FtpClientWindow
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private readonly FtpClient _ftpClient;
		private bool _showDetails;
		private ViewModel _viewModel;

		public event PropertyChangedEventHandler? PropertyChanged;

		public bool ShowDetails
		{
			get => _showDetails;
			set
			{
				if (_showDetails != value)
				{
					_showDetails = value;
					OnPropertyChanged();
				}
			}
		}

		public MainWindow(FtpClient ftpClient)
		{
			InitializeComponent();
			_viewModel = new ViewModel();
			DataContext = _viewModel;
			_ftpClient = ftpClient;
		}

		public void PrintResponse(string resp)
		{
			LogMessage("<<< " + resp);
		}

		public void PrintCommand(string cmd)
		{
			LogMessage(">>> " +  cmd);
		}

		public void HandleResponseData(CommandDataPair cdp)
		{
			switch(cdp.Command)
			{
				case FtpCommand.LIST:
					RefreshDirectory(cdp.Data);
					break;
			}
		}

		private void ConnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			ConnectWindow connectWindow = new ConnectWindow();
			if (connectWindow.ShowDialog() == true)
			{
				string host = connectWindow.Host;
				int port = connectWindow.Port;
				string username = connectWindow.Username;
				string password = connectWindow.Password;

				try
				{
					_ftpClient.ControllerConnect(host, port);
					_ftpClient.Login(username, password);
					_viewModel.CurrentDirectory = _ftpClient.PrintWokingDirectory();
					_ftpClient.ListDir();
				}
				catch (Exception ex)
				{
					LogMessage($"Error: {ex.Message}");
				}
			}
		}

		private void DisconnectButton_Clicked(object sender, RoutedEventArgs e)
		{
			try
			{
				_ftpClient.Disconnect();
			}
			catch (Exception ex)
			{
				LogMessage($"Error: {ex.Message}");
			}
		}

		private void UploadButton_Clicked(Object sender, RoutedEventArgs e)
		{
			TransferWindow transferWindow = new TransferWindow();
			transferWindow.ButtonName = "Upload";
			if (transferWindow.ShowDialog() == true)
			{
				string localFile = transferWindow.LocalPath;
				string remoteFile = transferWindow.RemotePath;

				try
				{
					_ftpClient.UploadFile(localFile, remoteFile);
				}
				catch (Exception ex)
				{
					LogMessage($"Error: {ex.Message}");
				}
			}
		}
		private void DownloadButton_Clicked(Object sender, RoutedEventArgs e)
		{
			TransferWindow transferWindow = new TransferWindow();
			transferWindow.ButtonName = "Download";
			if (transferWindow.ShowDialog() == true)
			{
				string localFile = transferWindow.LocalPath;
				string remoteFile = transferWindow.RemotePath;

				try
				{
					_ftpClient.DownloadFile(localFile, remoteFile);
				}
				catch (Exception ex)
				{
					LogMessage($"Error: {ex.Message}");
				}
			}
		}

		private void SendCommandButton_Click(object sender, RoutedEventArgs e)
		{
			string command = TerminalInput.Text.Trim();
			if (!string.IsNullOrEmpty(command))
			{
				try
				{
					_ftpClient.SendCmdAndReadResp(command);
				}
				catch (Exception ex)
				{
					LogMessage($"Error: {ex.Message}");
				}
			}
		}

		private void RefreshDirectory(byte[] directoryInfo)
		{
			string[] info = Encoding.UTF8.GetString(directoryInfo).Split(['\n', '\r']);
			var directoryItems = new List<DirectoryItem>();
			foreach(var line in info)
			{
				directoryItems.Add(new DirectoryItem(line));
			}
			FileList.ItemsSource = directoryItems;
		}
		

		private void LogMessage(string message)
		{
			LogBox.AppendText($"{message}\n");
			LogBox.ScrollToEnd();
		}

		private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (FileList.SelectedItem is DirectoryItem selectedItem && selectedItem.IsDirectory)
			{
				try
				{
					_ftpClient.ChangeDirectory(selectedItem.Name);
					_viewModel.CurrentDirectory = _ftpClient.PrintWokingDirectory();
					_ftpClient.ListDir();
				}
				catch (Exception ex)
				{
					LogMessage(ex.Message);
				}
			}
		}
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		private void CurrentDirectory_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				string path = CurrentDirectoryText.Text;
				_ftpClient.ChangeDirectory(path);
				_ftpClient.ListDir();
				_ftpClient.PrintWokingDirectory();
			}
		}
	}
}

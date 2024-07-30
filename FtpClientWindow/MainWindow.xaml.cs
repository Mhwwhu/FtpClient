using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Client;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Media;

namespace FtpClientWindow
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private readonly FtpClient _ftpClient;
		private bool _showDetails;
		private ViewModel _viewModel;
		private string _terminalInput = "";
		private int _terminalLastInputStart;

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
			AppendText("<<< " + resp, Brushes.Blue);
		}

		public void PrintCommand(string cmd)
		{
			LogMessage(">>> " +  cmd + "\n");
			AppendText(">>> " + cmd + "\n", Brushes.Black);
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

		public FileExistsNotifyChoice HandleFileExists(string filePath)
		{
			var notifyWindow = new FileExistsNotifyWindow();
			notifyWindow.ShowDialog();
			return notifyWindow.Choice;
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
					var caretPosition = TerminalTextBox.CaretPosition;
					var lineStart = caretPosition.GetLineStartPosition(0);
					string currentLine = new TextRange(lineStart, caretPosition).Text;
					if (!currentLine.StartsWith(">>>"))
					{
						AppendText(">>> ", Brushes.Black);
					}
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
					_ftpClient.DownloadFile(remoteFile, localFile);
				}
				catch (Exception ex)
				{
					LogMessage($"Error: {ex.Message}");
				}
			}
		}

		

		private void RefreshDirectory(byte[] directoryInfo)
		{
			string[] info = Encoding.UTF8
				.GetString(directoryInfo).Split(['\n', '\r'])
				.Where(line => !line.All(ch => char.IsWhiteSpace(ch) || char.IsControl(ch)))
				.ToArray();
			var directoryItems = new List<DirectoryItem>();
			foreach(var line in info)
			{
				directoryItems.Add(new DirectoryItem(line));
			}
			FileList.ItemsSource = directoryItems;
		}
		

		private void LogMessage(string message)
		{
			LogBox.AppendText($"{message}");
			LogBox.ScrollToEnd();
		}

		private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (FileList.SelectedItem is DirectoryItem selectedItem && selectedItem.IsDirectory)
			{
				try
				{
					AppendText("\n", Brushes.Black);
					_ftpClient.ChangeDirectory(selectedItem.Name);
					_viewModel.CurrentDirectory = _ftpClient.PrintWokingDirectory();
					_ftpClient.ListDir();
					var caretPosition = TerminalTextBox.CaretPosition;
					var lineStart = caretPosition.GetLineStartPosition(0);
					string currentLine = new TextRange(lineStart, caretPosition).Text;
					if(!currentLine.StartsWith(">>>"))
					{
						AppendText(">>> ", Brushes.Black);
					}
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
		private void TerminalTextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;

				var caretPosition = TerminalTextBox.CaretPosition;
				var lineStart = caretPosition.GetLineStartPosition(0);
				string currentLine = new TextRange(lineStart, caretPosition).Text;

				string command = currentLine.TrimStart('>', ' ');
				AppendText("\n", Brushes.Black);
				_ftpClient.SendCmdAndReadResp(command);
				_terminalInput = string.Empty;
				AppendText(">>> ", Brushes.Black);
			}
			else if (e.Key == Key.Back)
			{
				var caretPosition = TerminalTextBox.CaretPosition;
				var lineStart = caretPosition.GetLineStartPosition(0);
				if (caretPosition.CompareTo(lineStart) == 0)
				{
					e.Handled = true; // Prevent deletion of prompt
				}
			}
			else
			{
				_terminalInput += e.Key.ToString();
			}
		}
		private void AppendText(string text, Brush color)
		{
			var run = new Run(text) { Foreground = color };
			var paragraph = TerminalTextBox.Document.Blocks.LastBlock as Paragraph;
			if (paragraph == null)
			{
				paragraph = new Paragraph();
				TerminalTextBox.Document.Blocks.Add(paragraph);
			}
			paragraph.Inlines.Add(run);
			TerminalTextBox.CaretPosition = TerminalTextBox.Document.ContentEnd;
			TerminalTextBox.ScrollToEnd();
		}
	}
}

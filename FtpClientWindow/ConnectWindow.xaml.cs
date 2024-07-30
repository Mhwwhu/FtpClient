using System.Windows;
using System.Windows.Controls;
using Client;

namespace FtpClientWindow
{
	public partial class ConnectWindow : Window
	{
		public string Host => HostTextBox.Text;
		public int Port => int.Parse(PortTextBox.Text);
		public string Username => UsernameTextBox.Text;
		public string Password => PasswordBox.Password;
		//public string Host => "192.168.233.128";
		//public int Port => 21;
		//public string Username => "haowen";
		//public string Password => "Mhw20041001";

		public ConnectWindow()
		{
			InitializeComponent();
		}

		private void ConnectButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}

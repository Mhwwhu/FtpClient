using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FtpClientWindow
{
	/// <summary>
	/// TransferWindow.xaml 的交互逻辑
	/// </summary>
	public partial class TransferWindow : Window
	{
		private string _buttonName = "";
		public string LocalPath => LocalTextBox.Text;
		public string RemotePath => RemoteTextBox.Text;
		public string ButtonName
		{
			get => _buttonName;
			set
			{
				_buttonName = value;
				TransferButton.Content = value;
			}
		}
		public TransferWindow()
		{
			InitializeComponent();
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}
	}
}

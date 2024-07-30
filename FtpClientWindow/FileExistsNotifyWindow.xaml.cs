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
using Client;

namespace FtpClientWindow
{
	/// <summary>
	/// FileExistsNotifyWindow.xaml 的交互逻辑
	/// </summary>
	public partial class FileExistsNotifyWindow : Window
	{

		public FileExistsNotifyChoice Choice { get; private set; } = FileExistsNotifyChoice.Skip;

		public FileExistsNotifyWindow()
		{
			InitializeComponent();
		}

		private void OverwriteButton_Click(object sender, RoutedEventArgs e)
		{
			Choice = FileExistsNotifyChoice.Overwrite;
			this.DialogResult = true;
		}

		private void ResumeButton_Click(object sender, RoutedEventArgs e)
		{
			Choice = FileExistsNotifyChoice.Resume;
			this.DialogResult = true;
		}

		private void SkipButton_Click(object sender, RoutedEventArgs e)
		{
			Choice = FileExistsNotifyChoice.Skip;
			this.DialogResult = true;
		}
	}
}

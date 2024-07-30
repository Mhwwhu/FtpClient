using System.ComponentModel;

namespace FtpClientWindow;
public class ViewModel : INotifyPropertyChanged
{
	private string _currentDirectory;

	public string CurrentDirectory
	{
		get { return _currentDirectory; }
		set
		{
			if (_currentDirectory != value)
			{
				_currentDirectory = value;
				OnPropertyChanged(nameof(CurrentDirectory));
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;
	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}


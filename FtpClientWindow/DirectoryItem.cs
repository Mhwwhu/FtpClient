using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FtpClientWindow
{
	public class DirectoryItem
	{
		public string Permissions { get; set; }
		public int LinkCount { get; set; }
		public string Owner { get; set; }
		public string Group { get; set; }
		public long Size { get; set; }
		public DateTime LastModified { get; set; }
		public string Name { get; set; }
		public bool IsDirectory { get; set; } = false;

		public DirectoryItem(string info)
		{
			var parts = SplitLine(info);
			if (parts.Length >= 9)
			{
				Permissions = parts[0];
				LinkCount = int.Parse(parts[1]);
				Owner = parts[2];
				Group = parts[3];
				Size = long.Parse(parts[4]);
				LastModified = ParseDate(parts[5], parts[6], parts[7]);
				Name = parts[8];
				IsDirectory = parts[0][0] == 'd';
			}
		}
		private string[] SplitLine(string line)
		{
			var parts = new List<string>();
			var part = string.Empty;
			var inSpaces = false;

			foreach (var ch in line)
			{
				if (char.IsWhiteSpace(ch))
				{
					if (!inSpaces)
					{
						parts.Add(part);
						part = string.Empty;
					}
					inSpaces = true;
				}
				else
				{
					part += ch;
					inSpaces = false;
				}
			}

			if (!string.IsNullOrEmpty(part))
			{
				parts.Add(part);
			}

			return parts.ToArray();
		}
		private DateTime ParseDate(string month, string day, string timeOrYear)
		{
			var currentYear = DateTime.Now.Year;
			var dateString = $"{month} {day} {timeOrYear}";
			if (timeOrYear.Contains(":"))
			{
				dateString += $" {currentYear}";
			}

			DateTime parsedDate;
			if (DateTime.TryParseExact(dateString, "MMM dd HH:mm yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate) ||
				DateTime.TryParseExact(dateString, "MMM dd yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
			{
				return parsedDate;
			}

			return DateTime.MinValue;
		}
	}
	public class IsDirectoryToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool isDirectory)
			{
				return isDirectory ? "Directory" : "File";
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

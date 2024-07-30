using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Client
{
	public static class FtpClientExtension
	{
		private static int _defaultBufferSize = 1024;
		private static string _tag = "FtpClientExtension";

		public static void Login(this FtpClient client, string username, string password)
		{
			var resp = client.SendCmdAndReadResp("USER " + username);
			if (!resp.StartsWith(((int)FtpResponseCode.UserNameOK).ToString()))
			{
				throw new Exception(resp);
			}
			resp = client.SendCmdAndReadResp("PASS " + password);
			if(!resp.StartsWith(((int)FtpResponseCode.UserLoggedIn).ToString()))
			{
				throw new Exception(resp);
			}
			try
			{
				var server = client.Controller.GetServerByIp(client.HostIp, username);
				if (server == null)
				{
					client.Controller.AddServer(client.HostIp, username);
				}
				else client.HostId = server!.Id;
			}
			catch (Exception e)
			{
				client.Logger.Error(_tag, e.Message);
			}
		}
		public static void SetTransferType(this FtpClient client, string type)
		{
			var resp = client.SendCmdAndReadResp($"TYPE {type}");

		}
		public static void UploadFile(this FtpClient client, string localFilePath, string remoteFilePath)
		{
			FileInfo fileInfo = new FileInfo(localFilePath);
			long fileOffset = 0;

			if (client.FileExists(remoteFilePath))
			{
				switch (client.FileExistsHandler(remoteFilePath)) 
				{
					// 断点续传
					case FileExistsNotifyChoice.Resume:
						fileOffset = client.GetFileSize(remoteFilePath);
						client.SendCommand($"REST {fileOffset}");
						client.ReadResponse(_defaultBufferSize);
						break;
					// 覆盖
					case FileExistsNotifyChoice.Overwrite:
						break;
					// 跳过
					case FileExistsNotifyChoice.Skip:
						return;
				}
			}

			client.SetTransferType("I");
			client.OpenDataConnectionPassive();
			client.SendCommand($"STOR {remoteFilePath}");
			client.ReadResponse(_defaultBufferSize);

			using (FileStream fs = new FileStream(localFilePath, FileMode.Open, FileAccess.Read))
			{
				fs.Seek(fileOffset, SeekOrigin.Begin);
				byte[] fileBuffer = new byte[_defaultBufferSize];
				int bytesRead;

				while ((bytesRead = fs.Read(fileBuffer, 0, fileBuffer.Length)) > 0)
				{
					client.SendData(fileBuffer, bytesRead, SocketFlags.None);
				}
			}

			client.CloseDataConnection();
			client.ReadResponse(_defaultBufferSize);
		}

		public static void DownloadFile(this FtpClient client, string remoteFilePath, string localFilePath)
		{
			long fileOffset = 0;
			FileMode fileMode = FileMode.Append;
			if (File.Exists(localFilePath))
			{
				switch (client.FileExistsHandler(localFilePath))
				{
					// 断点续传
					case FileExistsNotifyChoice.Resume:
						fileOffset = new FileInfo(localFilePath).Length;
						client.SendCommand($"REST {fileOffset}");
						client.ReadResponse(_defaultBufferSize);
						break;
					// 覆盖
					case FileExistsNotifyChoice.Overwrite:
						fileMode = FileMode.Open;
						break;
					// 跳过
					case FileExistsNotifyChoice.Skip:
						return;
				}
			}

			client.SetTransferType("I");
			client.OpenDataConnectionPassive();
			client.SendCommand($"RETR {remoteFilePath}");
			client.ReadResponse(_defaultBufferSize);

			using (FileStream fs = new FileStream(localFilePath, fileMode, FileAccess.Write))
			{
				byte[] fileBuffer = new byte[_defaultBufferSize];
				int bytesRead;

				while ((bytesRead = client.ReceiveData(fileBuffer)) > 0)
				{
					fs.Write(fileBuffer, 0, bytesRead);
				}
			}

			client.CloseDataConnection();
		}

		public static void ChangeDirectory(this FtpClient client, string dir)
		{
			var resp = client.SendCmdAndReadResp("CWD " + dir);

		}

		public static void MakeDir(this FtpClient client, string dirName)
		{
			var resp = client.SendCmdAndReadResp("MKD " + dirName);
		}

		public static void RemoveDir(this FtpClient client, string dirName)
		{
			var resp = client.SendCmdAndReadResp("RMD " + dirName);
		}

		public static void RemoveFile(this FtpClient client, string fileName)
		{
			var resp = client.SendCmdAndReadResp("DELE " + fileName);
		}

		public static string ListDir(this FtpClient client, string dir = ".")
		{
			client.OpenDataConnectionPassive();
			var resp = client.SendCmdAndReadResp("LIST " + dir);
			var responseData = new List<byte>();
			var buffer = new byte[_defaultBufferSize];
			int recv;
			while((recv = client.ReceiveData(buffer)) > 0)
			{
				responseData.AddRange(buffer);
			}
			if (resp.Split(['\n', '\r']).Length == 1)
				client.ReadResponse();
			client.OnRespDataReceived(new CommandDataPair()
			{ 
				Command = FtpCommand.LIST, 
				Data = responseData .ToArray()
			});
			client.CloseDataConnection();
			return resp;
		}

		public static string PrintWokingDirectory(this FtpClient client)
		{
			var resp = client.SendCmdAndReadResp("PWD");
			string pattern = "(?<= \")(.*?)(?=\")";
			return Regex.Match(resp, pattern).ToString();
		}


		private static bool FileExists(this FtpClient client, string remoteFilePath)
		{
			client.SendCommand($"SIZE {remoteFilePath}");
			string response = client.ReadResponse();
			return response.StartsWith("213");
		}

		private static long GetFileSize(this FtpClient client, string remoteFilePath)
		{
			client.SendCommand($"SIZE {remoteFilePath}");
			string resp = client.ReadResponse();
			if (resp.StartsWith(((int)FtpResponseCode.FileStatus).ToString()))
			{
				return long.Parse(resp.Split(' ')[1]);
			}
			return 0;
		}
		private static void OpenDataConnectionPassive(this FtpClient client)
		{
			var resp = client.SendCmdAndReadResp("PASV");

			if (!resp.StartsWith(((int)FtpResponseCode.EnteringPassiveMode).ToString()))
			{
				throw new Exception("Failed to enter passive mode.");
			}

			string[] parts = resp.Split(new char[] { '(', ')' })[1].Split(',');
			string ipAddress = string.Join(".", parts, 0, 4);
			int port = (int.Parse(parts[4]) << 8) + int.Parse(parts[5]);

			client.CreateDataConnection(ipAddress, port);
		}
	}
}

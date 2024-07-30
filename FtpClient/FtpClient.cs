using Client.Database;
using Client.Logger;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;
public struct CommandDataPair
{
	public FtpCommand Command;
	public byte[] Data;
}
public class FtpClient
{
	private const int _defaultBufferSize = 1024;
	private Socket _controlSocket;
	private Socket _dataSocket;
	private const string _tag = "FtpClient";

	public ILoggerService Logger { get; private set; }
	public DbController Controller { get; private set; }
	public string HostIp { get; private set; }
	public int HostId;
	public Action<string> OnResponseReceived;
	public Action<string> OnCommandSent;
	public Action<CommandDataPair> OnRespDataReceived;

	public FtpClient(ILoggerService logger, DbController dbController)
	{
		_controlSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		Logger = logger;
		Controller = dbController;
	}

	public bool ControllerConnect(string hostIp, int port)
	{
		_controlSocket.Connect(new IPEndPoint(IPAddress.Parse(hostIp), port));
		HostIp = hostIp;
		return ReadResponse(_defaultBufferSize).StartsWith(FtpResponseCode.ServiceReadyForNewUser.ToString());
	}

	public void Disconnect()
	{
		SendCommand("QUIT");
		var resp = ReadResponse(_defaultBufferSize);
	}

	public int SendData(byte[] buffer, int size, SocketFlags socketFlags)
	{
		return _dataSocket.Send(buffer, size, socketFlags);
	}
	
	public int ReceiveData(byte[] buffer)
	{
		return _dataSocket.Receive(buffer);
	}

	public void CreateDataConnection(string ipAddress, int port)
	{
		_dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		_dataSocket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));
	}

	public void CloseDataConnection()
	{
		_dataSocket.Close();
	}

	public void SendCommand(string command)
	{
		byte[] cmdBytes = Encoding.ASCII.GetBytes((command + "\r\n").ToCharArray());
		_controlSocket.Send(cmdBytes, cmdBytes.Length, SocketFlags.None);
		OnCommandSent(command);
		Logger.Info(_tag + "-SendCommand", command);
		var server = Controller.GetServerById(HostId);
		int? id;
		if (server == null) id = null;
		else id = server.Id;
		try
		{
			Controller.AddRecord(id, command);
		}
		catch (Exception ex)
		{
			Logger.Fatal(_tag, ex.Message);
			throw;
		}
	}

	public string ReadResponse(int bufferSize = _defaultBufferSize)
	{
		var buffer = new byte[bufferSize];
		_controlSocket.ReceiveTimeout = 5000;
		int bytesRead;
		try
		{
			bytesRead = _controlSocket.Receive(buffer, bufferSize, SocketFlags.None);
		}
		catch (SocketException ex)
		{
			Logger.Error(_tag, ex.Message);
			throw;
		}
		string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
		OnResponseReceived(response);
		Logger.Info(_tag + "-Response", response);
		return response;
	}

	public string SendCmdAndReadResp(string command, int bufferSize = _defaultBufferSize)
	{
		SendCommand(command);
		if (string.IsNullOrEmpty(command) || command.All(char.IsWhiteSpace)) return string.Empty;
		return ReadResponse(bufferSize);
	}
}

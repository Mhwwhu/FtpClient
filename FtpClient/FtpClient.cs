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

	public Action<string> OnResponseReceived;
	public Action<string> OnCommandSent;
	public Action<CommandDataPair> OnRespDataReceived;

	public FtpClient()
	{
		_controlSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
	}

	public bool ControllerConnect(string hostname, int port)
	{
		_controlSocket.Connect(new IPEndPoint(IPAddress.Parse(hostname), port));
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
	}

	public string ReadResponse(int bufferSize = _defaultBufferSize)
	{
		var buffer = new byte[bufferSize];
		int bytesRead = _controlSocket.Receive(buffer, bufferSize, SocketFlags.None);
		string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
		OnResponseReceived(response);
		return response;
	}

	public string SendCmdAndReadResp(string command, int bufferSize = _defaultBufferSize)
	{
		SendCommand(command);
		return ReadResponse(bufferSize);
	}
}

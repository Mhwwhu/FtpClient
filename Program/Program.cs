using Client;

namespace Program
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var respQueue = new ResponseQueue();
			var client = new FtpClient(respQueue, "192.168.233.128", 21);
			client.ControllerConnect();
			client.Login("haowen", "Mhw20041001");
			client.UploadFile("./init.c", "Documents/init.c");
			client.Disconnect();
			while (respQueue.TryDequeue(out var resp))
			{
				Console.WriteLine(resp);
			}
			//while(true)
			//{
			//	client.SendCommand(Console.ReadLine()!);
			//	Console.WriteLine(client.ReadResponse());
			//}
		}
	}
}

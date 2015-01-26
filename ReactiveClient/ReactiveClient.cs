using System;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace ReactiveClient
{
	public class ReactiveClient
	{
		private RequestSocket reqSocket;
		private SubscriberSocket subSocket;

		public ReactiveClient()
		{
			var context = NetMQContext.Create();
			reqSocket = context.CreateRequestSocket();
			reqSocket.Connect("tcp://127.0.0.1:3334");

			reqSocket.Send("GetMarket");
			var data = reqSocket.Receive();
			var mess = Encoding.UTF8.GetString(data);
			Console.WriteLine(mess);

			subSocket = context.CreateSubscriberSocket();
			subSocket.Connect("tcp://127.0.0.1:3333");
			subSocket.Subscribe("");
			var thread = new Thread(ReceiveThread);
			thread.Start();
		}

		private void ReceiveThread()
		{
			while (true)
			{
				var data = subSocket.Receive();
				var mess = Encoding.UTF8.GetString(data);
				Console.WriteLine(mess);
			}	
		}
	}
}

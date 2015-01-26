using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;

namespace ReactiveServer
{
	public class ReactiveServer
	{
		public ReactiveServer()
		{
			var context = NetMQContext.Create();
			repSocket = context.CreateResponseSocket();
			repSocket.Bind("tcp://127.0.0.1:3334");
			repSocket.ReceiveReady += RepSocketOnReceiveReady;
			var repThread = new Thread(RepThread);
			repThread.Start();

			pubSocket = context.CreatePublisherSocket();
			pubSocket.Bind("tcp://127.0.0.1:3333");
		}

		private void RepThread()
		{
			while (true)
			{
				repSocket.Receive();
				if (market != null)
				{
					var t = market.Select(pair => string.Format("{0} = {1}", pair.Key, pair.Value));
					var mess = string.Join(";", t);
					repSocket.Send(mess);	
				}
				else
				{
					repSocket.Send("");
				}
			}
		}

		private void RepSocketOnReceiveReady(object sender, NetMQSocketEventArgs netMqSocketEventArgs)
		{
			netMqSocketEventArgs.Socket.Receive();
			var quotes = market.Select(pair => string.Format("{0}={1}", pair.Key, pair.Value));
			var data = string.Join(";", quotes);
			netMqSocketEventArgs.Socket.Send(data);
		}

		public void Start()
		{
			Quotes
				.ToObservable()
				.SubscribeOn(Scheduler.ThreadPool)	
				.Subscribe(tuple =>
			{
				market[tuple.Item1] = tuple.Item2;
			});

			Quotes.ToObservable()
				.SubscribeOn(Scheduler.ThreadPool)	
				.Buffer(TimeSpan.FromSeconds(2))
				.Subscribe(list => 
					list.ToObservable()
					.Subscribe(tuple => 
						pubSocket.Send(string.Format("{0}={1};", tuple.Item1, tuple.Item2)))
					);
		}

		private readonly Dictionary<string, decimal> market = new Dictionary<string, decimal>();

		private readonly List<string> symbols = new List<string>
		{
			"RUB_EUR", "EUR_USD", "USD_CHF", "USD_AUD", "USD_GBP"
		};

		private IEnumerable<Tuple<string, decimal>> Quotes
		{
			get
			{
				var rand = new Random();
				while (true)
				{
					yield return new Tuple<string, decimal>(
						symbols[rand.Next(symbols.Count)],
						(decimal)rand.NextDouble());
					Thread.Sleep(600);	
				}
			}
		}

		private readonly PublisherSocket pubSocket;
		private readonly ResponseSocket repSocket;

	}
}

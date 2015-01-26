using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace ReactiveServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var server = new ReactiveServer();
			server.Start();
			Console.ReadKey();
		}
	}
}

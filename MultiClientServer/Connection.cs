using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MultiClientServer
{
    class Connection
    {
        public StreamReader Read;
        public StreamWriter Write;
		private int clientPort;

        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port)
        {
			clientPort = port;
			Thread connectThread = new Thread(TryConnect);
			connectThread.Start();

			connectThread.Join();
            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write, int port)
        {
            Read = read; Write = write;
			clientPort = port;
            // Start het reader-loopje
            new Thread(ReaderThread).Start();
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            try
            {
				while (true)
				{
					string input = Read.ReadLine();
					if (input.StartsWith("UpdateRoute"))
						Program.UpdateDictionary(input.Substring(12));
					else if (input.StartsWith("Forward"))
					{
						string[] delen = input.Split(new char[] { ' ' }, 3);
						Program.SendMessage(int.Parse(delen[1]), delen[2], true);
					}
					else if (input.StartsWith("LostConnection"))
					{
						string[] delen = input.Split(' ');
						Program.LostConnection(int.Parse(delen[1]), int.Parse(delen[2]));
					}
					else
						Console.WriteLine(input);
				}
            }
            catch { Program.LostConnection(clientPort, clientPort); } // Verbinding is kennelijk verbroken
        }

		private void TryConnect(object mt)
		{
			bool TryAgain = true;
			TcpClient client = null;

			while (TryAgain)
			{
				try
				{
					client = new TcpClient("localhost", clientPort);
					TryAgain = false;
				}
				catch { Thread.Sleep(1000); }
				
			}

			Read = new StreamReader(client.GetStream());
			Write = new StreamWriter(client.GetStream());
			Write.AutoFlush = true;

			// De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
			Write.WriteLine("Poort: " + Program.MijnPoort);
		}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Program
    {
        static public int MijnPoort;

        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();

		static public RoutingTable table;

        static void Main(string[] args)
        {
			if(args.Length > 0)
				MijnPoort = int.Parse(args[0]);
            new Server(MijnPoort);

			Console.Title = "MultiClientServer " + MijnPoort;

			int[] buren = new int[args.Length - 1];
			table = new RoutingTable(MijnPoort);

			for (int i = 1; i < args.Length; i++)
			{
				int poort = int.Parse(args[i]);
				buren[i - 1] = poort;
				if (poort > MijnPoort)
					Buren.Add(poort, new Connection(poort));
			}

			foreach (KeyValuePair<int, Connection> buur in Buren)
				for (int j = 0; j < buren.Length; j++)
				{
					UpdateDictionary(buren[j] + " 0 " + buren[j] + " " + buren[j]);
				}

			lock (table.Table)
			{
				foreach (KeyValuePair<int, Entry> entry in table.Table)
					foreach (KeyValuePair<int, Connection> buur in Buren)
						SendMessage(buur.Key, "UpdateRoute " + entry.Value.ToString() + " " + MijnPoort);
			}

			while (true)
            {
                string input = Console.ReadLine();

				switch (input.Split()[0]) {
					case "C":
						int poort = int.Parse(input.Split()[1]);
						if (Buren.ContainsKey(poort))
							Console.WriteLine("Hier is al verbinding naar!");
						else
						{
							// Leg verbinding aan (als client)
							Buren.Add(poort, new Connection(poort));
							Console.WriteLine("Verbonden: " + poort);
							UpdateDictionary(poort + " 0 0 " + poort);

							lock (table.Table)
							{
								foreach (KeyValuePair<int, Entry> entry in table.Table)
									SendMessage(poort, "UpdateRoute " + entry.Value.ToString() + " " + MijnPoort);
							}

							foreach (KeyValuePair<int, Connection> buur in Buren)
								SendMessage(buur.Key, "UpdateRoute " + MijnPoort + " 0 " + MijnPoort + " " + poort);
						}
						break;
					case "R":
						Console.WriteLine(table.showTable());
						break;
					case "B":
						// Stuur berichtje
						string[] delen = input.Split(new char[]{' '}, 3);
						int poort2 = int.Parse(delen[1]);

						SendMessage(poort2, delen[2]);

						break;
				}
            }
		}

		public static void UpdateDictionary(string input)
		{
			string[] delen = input.Split();
			int destination = int.Parse(delen[0]);
			int afstand = int.Parse(delen[1]);
			int besteBuur = int.Parse(delen[3]); // Verzender van bericht

			lock (table.Table)
			{
				if (!table.Table.ContainsKey(destination))
				{
					table.Table.Add(destination, new Entry(destination, afstand + 1, besteBuur));
					SendUpdate(destination, afstand + 1, besteBuur);
				}
				else if (table.Table[destination].Afstand > (afstand + 1))
				{
					table.Table.Remove(destination);
					table.Table.Add(destination, new Entry(destination, afstand + 1, besteBuur));
					SendUpdate(destination, afstand + 1, besteBuur);
				}
				else
					return;
			}
		}

		public static void SendMessage(int destination, string message, bool forwarded = false)
		{
			int forwardDestination = destination;

			if (Buren.ContainsKey(destination))
				Buren[destination].Write.WriteLine(message);
			else
			{
				forwardDestination = table.Table[destination].BesteBuur;
				Buren[forwardDestination].Write.WriteLine("Forward " + destination + " " + message);
			}

			if (forwarded)
				Console.WriteLine("Bericht voor " + destination + " doorgestuurd naar " + forwardDestination);
		}

		private static void SendUpdate(int destination, int afstand, int besteBuur)
		{
			foreach (KeyValuePair<int, Connection> buur in Buren)
			{
				SendMessage(buur.Key, "UpdateRoute " + table.Table[destination].ToString() + " " + MijnPoort);
				SendMessage(buur.Key, "UpdateRoute " + MijnPoort + " " + afstand + " 0 " + besteBuur);
			}
		}
    }
}

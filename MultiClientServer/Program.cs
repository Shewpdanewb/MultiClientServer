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

            Console.WriteLine("Typ [verbind poortnummer] om verbinding te maken, bijvoorbeeld: verbind 1100");
            Console.WriteLine("Typ [poortnummer bericht] om een bericht te sturen, bijvoorbeeld: 1100 hoi hoi");

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
					UpdateDictionary(buren[j] + " 0 " + buren[j] + " " + MijnPoort);				

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
							UpdateDictionary(poort + " 0 0 " + MijnPoort);

							foreach (KeyValuePair<int, Connection> buur in Buren)
								Buren[buur.Key].Write.WriteLine("UpdateRoute " + MijnPoort + " 0 " + MijnPoort + " " + poort);
						}
						break;
					case "R":
						Console.WriteLine(table.showTable());
						break;
					case "B":
						// Stuur berichtje
						string[] delen = input.Split(new char[]{' '}, 3);
						int poort2 = int.Parse(delen[1]);

						if (Buren.ContainsKey(poort2))
						{
							Buren[poort2].Write.WriteLine(MijnPoort + ": " + delen[2]);
							Console.WriteLine("Test direct m");
						}
						else if (table.Table.ContainsKey(poort2))
						{
							int besteBuur = table.Table[poort2].BesteBuur;
							Buren[besteBuur].Write.WriteLine("Forward " + poort2 + " " + MijnPoort + ": " + delen[2]);
							Console.WriteLine("Test table forward");
						}
						else
							Console.WriteLine("Geen verbinding naar deze poort mogelijk");

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

			if (!table.Table.ContainsKey(destination))
				table.Table.Add(destination, new Entry(destination, afstand + 1, besteBuur));
			else if (table.Table[destination].Afstand > afstand + 1)
			{
				table.Table.Remove(destination);
				table.Table.Add(destination, new Entry(destination, afstand + 1, besteBuur));
			}
			else
				return;

			foreach (KeyValuePair<int, Connection> buur in Buren)
			{
				Buren[buur.Key].Write.WriteLine("UpdateRoute " + table.Table[destination].ToString() + " " + MijnPoort);
				Buren[buur.Key].Write.WriteLine("UpdateRoute " + MijnPoort + " " + afstand + " 0 " + besteBuur);
			}
		}

		public static void ForwardMessage(string message)
		{
			string[] delen = message.Split(new char[] { ' ' }, 3);
			int destination = int.Parse(delen[1]);

			if (Buren.ContainsKey(destination))
				Buren[destination].Write.WriteLine(delen[2]);
			else
			{
				int besteBuur = table.Table[destination].BesteBuur;
				Buren[besteBuur].Write.WriteLine("Forward " + destination + " " + delen[2]);
			}
		}
    }
}

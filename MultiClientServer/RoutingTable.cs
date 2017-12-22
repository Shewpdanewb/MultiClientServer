using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
	class RoutingTable
	{
		Dictionary<int, Entry> table = new Dictionary<int, Entry>();

		public RoutingTable(int mijnPoort)
		{
			table.Add(mijnPoort, new Entry(mijnPoort, 0, mijnPoort));
		}

		public Dictionary<int, Entry> Table { get { return table; } }

		public void updateTable() { }

		public string showTable() {
			string result = "";

			foreach(KeyValuePair<int, Entry> keyval in table)
				result += keyval.Value.ToString() + "\n";

			return result;
		}
	}

	class Entry
	{
		int destination, afstand, besteBuur;

		public int Destination { get { return destination; } }

		public int Afstand { get { return afstand; } }

		public int BesteBuur { get { return besteBuur; } }

		public Entry(int destination, int afstand, int besteBuur)
		{
			this.destination = destination;
			this.afstand = afstand;
			this.besteBuur = besteBuur;
		}

		public override string ToString()
		{
			if (afstand == 0)
				return destination + " " + 0 + " local";
			else
				return destination + " " + afstand + " " + besteBuur;
		}
	}
}

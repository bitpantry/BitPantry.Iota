using BitPantry.Iota.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Application
{
    internal class BookName
    {
        public string Name { get; private set; }
        public IReadOnlyList<string> Aliases { get; private set; }

        public BookName(string name, params string[] aliases)
        {
            Name = name;
            Aliases = aliases.AsReadOnly();
        }   

        public int CalculateShortestLevenshteinDistance(string str)
        {
            int minDistance = Name.CalculateLevenshteinDistance(str, true);

            foreach (var alias in Aliases)
            {
                int distance = str.CalculateLevenshteinDistance(alias, true);

                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }
    }
}

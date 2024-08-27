using Microsoft.IdentityModel.Protocols.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitPantry.Iota.Data.Entity
{
    public enum Divider : int
    {
        Queue = -1,

        Daily = 0,

        Sunday = 1,
        Monday = 2,
        Tuesday = 3,
        Wednesday = 4,
        Thursday = 5,
        Friday = 6,
        Saturday = 7,

        Odd = 8,
        Even = 9,

        Day1 = 10,
        Day2 = 11,
        Day3 = 12,
        Day4 = 13,
        Day5 = 14,
        Day6 = 15,
        Day7 = 16,
        Day8 = 17,
        Day9 = 18,
        Day10 = 19,
        Day11 = 20,
        Day12 = 21,
        Day13 = 22,
        Day14 = 23,
        Day15 = 24,
        Day16 = 25,
        Day17 = 26,
        Day18 = 27,
        Day19 = 28,
        Day20 = 29,
        Day21 = 30,
        Day22 = 31,
        Day23 = 32,
        Day24 = 33,
        Day25 = 34,
        Day26 = 35,
        Day27 = 36,
        Day28 = 37,
        Day29 = 38,
        Day30 = 39,
        Day31 = 40
    }


    public class Card : EntityBase<long>
    {
        public long UserId { get; set; }
        public User User { get; set; }
        public DateTime AddedOn { get; set; }
        public DateTime LastMovedOn { get; set; }
        public List<Verse> Verses { get; set; } 
        public Divider Divider { get; set; }
        public int Order { get; set; }
    }
}

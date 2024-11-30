using System;
using System.ComponentModel;

namespace BitPantry.Iota.Common;

public enum Tab : int
{
    Queue = -1,

    Daily = 0,

    Odd = 1,
    Even = 2,

    Sunday = 3,
    Monday = 4,
    Tuesday = 5,
    Wednesday = 6,
    Thursday = 7,
    Friday = 8,
    Saturday = 9,

    [Description("1st")]
    Day1 = 10,
    [Description("2nd")]
    Day2 = 11,
    [Description("3rd")]
    Day3 = 12,
    [Description("4th")]
    Day4 = 13,
    [Description("5th")]
    Day5 = 14,
    [Description("6th")]
    Day6 = 15,
    [Description("7th")]
    Day7 = 16,
    [Description("8th")]
    Day8 = 17,
    [Description("9th")]
    Day9 = 18,
    [Description("10th")]
    Day10 = 19,
    [Description("11th")]
    Day11 = 20,
    [Description("12th")]
    Day12 = 21,
    [Description("13th")]
    Day13 = 22,
    [Description("14th")]
    Day14 = 23,
    [Description("15th")]
    Day15 = 24,
    [Description("16th")]
    Day16 = 25,
    [Description("17th")]
    Day17 = 26,
    [Description("18th")]
    Day18 = 27,
    [Description("19th")]
    Day19 = 28,
    [Description("20th")]
    Day20 = 29,
    [Description("21st")]
    Day21 = 30,
    [Description("22nd")]
    Day22 = 31,
    [Description("23rd")]
    Day23 = 32,
    [Description("24th")]
    Day24 = 33,
    [Description("25th")]
    Day25 = 34,
    [Description("26th")]
    Day26 = 35,
    [Description("27th")]
    Day27 = 36,
    [Description("28th")]
    Day28 = 37,
    [Description("29th")]
    Day29 = 38,
    [Description("30th")]
    Day30 = 39,
    [Description("31st")]
    Day31 = 40
}

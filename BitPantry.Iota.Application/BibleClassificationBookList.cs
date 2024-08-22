using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using BitPantry.Iota.Data.Entity;

namespace BitPantry.Iota.Application
{
    internal static class BibleClassificationBookList
    {
        private static ReadOnlyDictionary<int, string> _protestant = new Dictionary<int, string>
        {
            { 1, "Genesis" },
            { 2, "Exodus" },
            { 3, "Leviticus" },
            { 4, "Numbers" },
            { 5, "Deuteronomy" },
            { 6, "Joshua" },
            { 7, "Judges" },
            { 8, "Ruth" },
            { 9, "1 Samuel" },
            { 10, "2 Samuel" },
            { 11, "1 Kings" },
            { 12, "2 Kings" },
            { 13, "1 Chronicles" },
            { 14, "2 Chronicles" },
            { 15, "Ezra" },
            { 16, "Nehemiah" },
            { 17, "Esther" },
            { 18, "Job" },
            { 19, "Psalms" },
            { 20, "Proverbs" },
            { 21, "Ecclesiastes" },
            { 22, "Song of Solomon" },
            { 23, "Isaiah" },
            { 24, "Jeremiah" },
            { 25, "Lamentations" },
            { 26, "Ezekiel" },
            { 27, "Daniel" },
            { 28, "Hosea" },
            { 29, "Joel" },
            { 30, "Amos" },
            { 31, "Obadiah" },
            { 32, "Jonah" },
            { 33, "Micah" },
            { 34, "Nahum" },
            { 35, "Habakkuk" },
            { 36, "Zephaniah" },
            { 37, "Haggai" },
            { 38, "Zechariah" },
            { 39, "Malachi" },
            { 40, "Matthew" },
            { 41, "Mark" },
            { 42, "Luke" },
            { 43, "John" },
            { 44, "Acts" },
            { 45, "Romans" },
            { 46, "1 Corinthians" },
            { 47, "2 Corinthians" },
            { 48, "Galatians" },
            { 49, "Ephesians" },
            { 50, "Philippians" },
            { 51, "Colossians" },
            { 52, "1 Thessalonians" },
            { 53, "2 Thessalonians" },
            { 54, "1 Timothy" },
            { 55, "2 Timothy" },
            { 56, "Titus" },
            { 57, "Philemon" },
            { 58, "Hebrews" },
            { 59, "James" },
            { 60, "1 Peter" },
            { 61, "2 Peter" },
            { 62, "1 John" },
            { 63, "2 John" },
            { 64, "3 John" },
            { 65, "Jude" },
            { 66, "Revelation" }
        }.AsReadOnly();

        private static ReadOnlyDictionary<int, string> _catholic = new Dictionary<int, string>
        {
            { 1, "Genesis" },
            { 2, "Exodus" },
            { 3, "Leviticus" },
            { 4, "Numbers" },
            { 5, "Deuteronomy" },
            { 6, "Joshua" },
            { 7, "Judges" },
            { 8, "Ruth" },
            { 9, "1 Samuel" },
            { 10, "2 Samuel" },
            { 11, "1 Kings" },
            { 12, "2 Kings" },
            { 13, "1 Chronicles" },
            { 14, "2 Chronicles" },
            { 15, "Ezra" },
            { 16, "Nehemiah" },
            { 17, "Tobit" },
            { 18, "Judith" },
            { 19, "Esther" },
            { 20, "1 Maccabees" },
            { 21, "2 Maccabees" },
            { 22, "Job" },
            { 23, "Psalms" },
            { 24, "Proverbs" },
            { 25, "Ecclesiastes" },
            { 26, "Song of Solomon" },
            { 27, "Wisdom" },
            { 28, "Sirach" },
            { 29, "Isaiah" },
            { 30, "Jeremiah" },
            { 31, "Lamentations" },
            { 32, "Baruch" },
            { 33, "Ezekiel" },
            { 34, "Daniel" },
            { 35, "Hosea" },
            { 36, "Joel" },
            { 37, "Amos" },
            { 38, "Obadiah" },
            { 39, "Jonah" },
            { 40, "Micah" },
            { 41, "Nahum" },
            { 42, "Habakkuk" },
            { 43, "Zephaniah" },
            { 44, "Haggai" },
            { 45, "Zechariah" },
            { 46, "Malachi" },
            { 47, "Matthew" },
            { 48, "Mark" },
            { 49, "Luke" },
            { 50, "John" },
            { 51, "Acts" },
            { 52, "Romans" },
            { 53, "1 Corinthians" },
            { 54, "2 Corinthians" },
            { 55, "Galatians" },
            { 56, "Ephesians" },
            { 57, "Philippians" },
            { 58, "Colossians" },
            { 59, "1 Thessalonians" },
            { 60, "2 Thessalonians" },
            { 61, "1 Timothy" },
            { 62, "2 Timothy" },
            { 63, "Titus" },
            { 64, "Philemon" },
            { 65, "Hebrews" },
            { 66, "James" },
            { 67, "1 Peter" },
            { 68, "2 Peter" },
            { 69, "1 John" },
            { 70, "2 John" },
            { 71, "3 John" },
            { 72, "Jude" },
            { 73, "Revelation" }
        }.AsReadOnly();

        public static ReadOnlyDictionary<int, string> GetBookList(BibleClassification classification)
        {
            switch (classification)
            {
                case BibleClassification.Protestant:
                    return _protestant;
                case BibleClassification.Catholic:
                    return _catholic;
                default:
                    throw new ArgumentException($"BibleClassification, {classification}, is undefined for this switch");
            }
        }
    }
}



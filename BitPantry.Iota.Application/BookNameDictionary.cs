using System.Collections.ObjectModel;
using BitPantry.Iota.Data.Entity;

namespace BitPantry.Iota.Application
{
    internal static class BookNameDictionary
    {


        private static ReadOnlyDictionary<int, BookName> _protestant = new Dictionary<int, BookName>
        {
            { 1, new BookName("Genesis", "Gen") },
            { 2, new BookName("Exodus", "Exo", "Ex") },
            { 3, new BookName("Leviticus", "Lev") },
            { 4, new BookName("Numbers", "Num", "Nm") },
            { 5, new BookName("Deuteronomy", "Deut", "Dt") },
            { 6, new BookName("Joshua", "Josh", "Jos") },
            { 7, new BookName("Judges", "Judg", "Jdg") },
            { 8, new BookName("Ruth", "Rth") },
            { 9, new BookName("1 Samuel", "1 Sam", "1 Sm") },
            { 10, new BookName("2 Samuel", "2 Sam", "2 Sm") },
            { 11, new BookName("1 Kings", "1 Kgs", "1 Ki") },
            { 12, new BookName("2 Kings", "2 Kgs", "2 Ki") },
            { 13, new BookName("1 Chronicles", "1 Chr", "1 Ch") },
            { 14, new BookName("2 Chronicles", "2 Chr", "2 Ch") },
            { 15, new BookName("Ezra", "Ezr") },
            { 16, new BookName("Nehemiah", "Neh", "Ne") },
            { 17, new BookName("Esther", "Est", "Es") },
            { 18, new BookName("Job", "Jb") },
            { 19, new BookName("Psalms", "Ps", "Pslm") },
            { 20, new BookName("Proverbs", "Prov", "Prv") },
            { 21, new BookName("Ecclesiastes", "Eccl", "Ecc") },
            { 22, new BookName("Song of Solomon", "Song", "SoS", "Canticles") },
            { 23, new BookName("Isaiah", "Isa", "Is") },
            { 24, new BookName("Jeremiah", "Jer", "Je") },
            { 25, new BookName("Lamentations", "Lam", "La") },
            { 26, new BookName("Ezekiel", "Ezek", "Eze") },
            { 27, new BookName("Daniel", "Dan", "Da") },
            { 28, new BookName("Hosea", "Hos", "Ho") },
            { 29, new BookName("Joel", "Jl") },
            { 30, new BookName("Amos", "Am") },
            { 31, new BookName("Obadiah", "Obad", "Ob") },
            { 32, new BookName("Jonah", "Jon") },
            { 33, new BookName("Micah", "Mic", "Mi") },
            { 34, new BookName("Nahum", "Nah", "Na") },
            { 35, new BookName("Habakkuk", "Hab", "Hb") },
            { 36, new BookName("Zephaniah", "Zeph", "Zep") },
            { 37, new BookName("Haggai", "Hag", "Hg") },
            { 38, new BookName("Zechariah", "Zech", "Zec") },
            { 39, new BookName("Malachi", "Mal", "Ml") },
            { 40, new BookName("Matthew", "Matt", "Mt") },
            { 41, new BookName("Mark", "Mk") },
            { 42, new BookName("Luke", "Lk") },
            { 43, new BookName("John", "Jn") },
            { 44, new BookName("Acts", "Acts of the Apostles") },
            { 45, new BookName("Romans", "Rom", "Ro") },
            { 46, new BookName("1 Corinthians", "1 Cor", "1 Co") },
            { 47, new BookName("2 Corinthians", "2 Cor", "2 Co") },
            { 48, new BookName("Galatians", "Gal", "Ga") },
            { 49, new BookName("Ephesians", "Eph", "Eph") },
            { 50, new BookName("Philippians", "Phil", "Php") },
            { 51, new BookName("Colossians", "Col", "Col") },
            { 52, new BookName("1 Thessalonians", "1 Thess", "1 Th") },
            { 53, new BookName("2 Thessalonians", "2 Thess", "2 Th") },
            { 54, new BookName("1 Timothy", "1 Tim", "1 Ti") },
            { 55, new BookName("2 Timothy", "2 Tim", "2 Ti") },
            { 56, new BookName("Titus", "Tit") },
            { 57, new BookName("Philemon", "Phlm", "Phm") },
            { 58, new BookName("Hebrews", "Heb") },
            { 59, new BookName("James", "Jas", "Ja") },
            { 60, new BookName("1 Peter", "1 Pet", "1 Pe") },
            { 61, new BookName("2 Peter", "2 Pet", "2 Pe") },
            { 62, new BookName("1 John", "1 Jn") },
            { 63, new BookName("2 John", "2 Jn") },
            { 64, new BookName("3 John", "3 Jn") },
            { 65, new BookName("Jude", "Jud") },
            { 66, new BookName("Revelation", "Rev", "Re") }
        }.AsReadOnly();

        private static ReadOnlyDictionary<int, BookName> _catholic = new Dictionary<int, BookName>
        {
            { 1, new BookName("Genesis", "Gen") },
            { 2, new BookName("Exodus", "Exo", "Ex") },
            { 3, new BookName("Leviticus", "Lev") },
            { 4, new BookName("Numbers", "Num", "Nm") },
            { 5, new BookName("Deuteronomy", "Deut", "Dt") },
            { 6, new BookName("Joshua", "Josh", "Jos") },
            { 7, new BookName("Judges", "Judg", "Jdg") },
            { 8, new BookName("Ruth", "Rth") },
            { 9, new BookName("1 Samuel", "1 Sam", "1 Sm", "I Samuel", "I Sam") },
            { 10, new BookName("2 Samuel", "2 Sam", "2 Sm", "II Samuel", "II Sam") },
            { 11, new BookName("1 Kings", "1 Kgs", "1 Ki", "I Kings", "I Kgs") },
            { 12, new BookName("2 Kings", "2 Kgs", "2 Ki", "II Kings", "II Kgs") },
            { 13, new BookName("1 Chronicles", "1 Chr", "1 Ch", "I Chronicles", "I Chr") },
            { 14, new BookName("2 Chronicles", "2 Chr", "2 Ch", "II Chronicles", "II Chr") },
            { 15, new BookName("Ezra", "Ezr") },
            { 16, new BookName("Nehemiah", "Neh", "Ne") },
            { 17, new BookName("Tobit", "Tob", "Tb") },
            { 18, new BookName("Judith", "Jdt") },
            { 19, new BookName("Esther", "Est", "Es") },
            { 20, new BookName("1 Maccabees", "1 Macc", "1 Mac", "I Maccabees", "I Macc") },
            { 21, new BookName("2 Maccabees", "2 Macc", "2 Mac", "II Maccabees", "II Macc") },
            { 22, new BookName("Job", "Jb") },
            { 23, new BookName("Psalms", "Ps", "Pslm") },
            { 24, new BookName("Proverbs", "Prov", "Prv") },
            { 25, new BookName("Ecclesiastes", "Eccl", "Ecc") },
            { 26, new BookName("Song of Solomon", "Song", "SoS", "Canticles") },
            { 27, new BookName("Wisdom", "Wis") },
            { 28, new BookName("Sirach", "Ecclesiasticus", "Sir") },
            { 29, new BookName("Isaiah", "Isa", "Is") },
            { 30, new BookName("Jeremiah", "Jer", "Je") },
            { 31, new BookName("Lamentations", "Lam", "La") },
            { 32, new BookName("Baruch", "Bar") },
            { 33, new BookName("Ezekiel", "Ezek", "Eze") },
            { 34, new BookName("Daniel", "Dan", "Da") },
            { 35, new BookName("Hosea", "Hos", "Ho") },
            { 36, new BookName("Joel", "Jl") },
            { 37, new BookName("Amos", "Am") },
            { 38, new BookName("Obadiah", "Obad", "Ob") },
            { 39, new BookName("Jonah", "Jon") },
            { 40, new BookName("Micah", "Mic", "Mi") },
            { 41, new BookName("Nahum", "Nah", "Na") },
            { 42, new BookName("Habakkuk", "Hab", "Hb") },
            { 43, new BookName("Zephaniah", "Zeph", "Zep") },
            { 44, new BookName("Haggai", "Hag", "Hg") },
            { 45, new BookName("Zechariah", "Zech", "Zec") },
            { 46, new BookName("Malachi", "Mal", "Ml") },
            { 47, new BookName("Matthew", "Matt", "Mt") },
            { 48, new BookName("Mark", "Mk") },
            { 49, new BookName("Luke", "Lk") },
            { 50, new BookName("John", "Jn") },
            { 51, new BookName("Acts", "Acts of the Apostles") },
            { 52, new BookName("Romans", "Rom", "Ro") },
            { 53, new BookName("1 Corinthians", "1 Cor", "1 Co", "I Corinthians", "I Cor") },
            { 54, new BookName("2 Corinthians", "2 Cor", "2 Co", "II Corinthians", "II Cor") },
            { 55, new BookName("Galatians", "Gal", "Ga") },
            { 56, new BookName("Ephesians", "Eph") },
            { 57, new BookName("Philippians", "Phil", "Php") },
            { 58, new BookName("Colossians", "Col") },
            { 59, new BookName("1 Thessalonians", "1 Thess", "1 Th", "I Thessalonians", "I Thess") },
            { 60, new BookName("2 Thessalonians", "2 Thess", "2 Th", "II Thessalonians", "II Thess") },
            { 61, new BookName("1 Timothy", "1 Tim", "1 Ti", "I Timothy", "I Tim") },
            { 62, new BookName("2 Timothy", "2 Tim", "2 Ti", "II Timothy", "II Tim") },
            { 63, new BookName("Titus", "Tit") },
            { 64, new BookName("Philemon", "Phlm", "Phm") },
            { 65, new BookName("Hebrews", "Heb") },
            { 66, new BookName("James", "Jas", "Ja") },
            { 67, new BookName("1 Peter", "1 Pet", "1 Pe", "I Peter", "I Pet") },
            { 68, new BookName("2 Peter", "2 Pet", "2 Pe", "II Peter", "II Pet") },
            { 69, new BookName("1 John", "1 Jn", "I John") },
            { 70, new BookName("2 John", "2 Jn", "II John") },
            { 71, new BookName("3 John", "3 Jn", "III John") },
            { 72, new BookName("Jude", "Jud") },
            { 73, new BookName("Revelation", "Rev", "Re") }
        }.AsReadOnly();

        public static ReadOnlyDictionary<int, BookName> Get(BibleClassification classification)
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

        public static KeyValuePair<int, BookName> Get(BibleClassification classification, string bookName)
        {
            var bookNameList = Get(classification);

            int minDistance = int.MaxValue;
            int matchingBookNumber = 0;

            foreach (var item in bookNameList)
            {
                int distance = item.Value.CalculateShortestLevenshteinDistance(bookName);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    matchingBookNumber = item.Key;
                }
            }

            if(matchingBookNumber == 0)
                return new KeyValuePair<int, BookName>(0, null);

            return new KeyValuePair<int, BookName>(matchingBookNumber, bookNameList[matchingBookNumber]);
        }

        public static KeyValuePair<int, BookName> Get(BibleClassification classification, int bookNumber)
            => new KeyValuePair<int, BookName>(bookNumber, Get(classification)[bookNumber]);

    }
}



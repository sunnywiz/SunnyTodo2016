using System;
using System.Globalization;

namespace SunnyTodo2016
{
    public static class HistoryFileHelper
    {
        public static Tuple<DateTime, string> PipeSeperatedLineToHistoryTuple(string hl)
        {
            Tuple<DateTime, string> tuple = null; 
            var index = hl.IndexOf('|');
            if (index < 0) return tuple;
            DateTime timestamp;
            var k1 = hl.Substring(0, index);
            var k2 = hl.Substring(index + 1);
            if (DateTime.TryParse(k1, null, DateTimeStyles.RoundtripKind, out timestamp))
            {
                tuple = new Tuple<DateTime, string>(timestamp, k2);
            }
            return tuple;
        }

        public static string TupleToPipeSeperatedLine(Tuple<DateTime, string> item)
        {
            return $"{item.Item1:o}|{item.Item2}";
        }
    }
}
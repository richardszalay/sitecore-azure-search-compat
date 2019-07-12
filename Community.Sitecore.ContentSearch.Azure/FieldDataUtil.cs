using System;
using System.Linq;
using System.Collections;
using System.Text;

namespace Community.Sitecore.ContentSearch.Azure
{
    public class FieldDataUtil
    {
        const int IndentOverhead = 20; // This is intentionally an overestimate
        const int JsonStringOverhead = 2;
        const int JsonArrayOverhead = 2;
        const int JsonArrayElementOverhead = 1 + IndentOverhead;
        const int JsonDictionaryOverhead = 2;
        const int JsonDictionaryElementOverhead = 2 + IndentOverhead;

        public static int EstimateSize(object value, bool asJson = false)
        {
            if (value is string stringValue)
                return EstimateSize(stringValue, asJson);

            if (value is DateTime || value is DateTimeOffset)
                return "yyyy-MM-ddTHH:mm:ss.fffZ".Length;

            if (value is IDictionary dictionaryValue)
                return EstimateSize(dictionaryValue, asJson);

            if (value is IEnumerable enumerableValue)
                return EstimateSize(enumerableValue, asJson);

            if (value == null)
                return 0;

            return EstimateSize(value.ToString(), asJson);
        }

        private static int EstimateSize(string value, bool asJson) =>
            Encoding.UTF8.GetByteCount(value) + (asJson ? JsonStringOverhead : 0);

        private static int EstimateSize(IEnumerable enumerable, bool asJson)
        {
            int size = asJson ? JsonArrayOverhead : 0;

            foreach (var value in enumerable)
            {
                size += EstimateSize(value);

                if (asJson)
                {
                    size += JsonArrayElementOverhead;
                }
            }

            return size;
        }

        private static int EstimateSize(IDictionary dictionary, bool asJson)
        {
            int size = asJson ? JsonDictionaryOverhead : 0;

            foreach (var key in dictionary.Keys)
            {
                size += EstimateSize(key);
                size += EstimateSize(dictionary[key]);

                if (asJson)
                {
                    size += JsonDictionaryElementOverhead;
                }
            }

            return size;
        }

        public static object Truncate(object value, int bytesToTruncate)
        {
            return Truncate(value, ref bytesToTruncate);
        }

        private static object Truncate(object value, ref int bytesToTruncate)
        {
            if (value is string stringValue)
            {
                return Truncate(stringValue, ref bytesToTruncate);
            }

            if (value is IDictionary dictionaryValue)
            {
                return Truncate(dictionaryValue, ref bytesToTruncate);
            }

            if (value is IEnumerable enumerableValue)
            {
                return Truncate(enumerableValue, ref bytesToTruncate);
            }

            return value;
        }


        private static string Truncate(string value, ref int bytesToTruncate)
        {
            int valueByteCount = Encoding.UTF8.GetByteCount(value);

            int charsToTrim = bytesToTruncate; // TODO: Handle multi-byte chars

            if (charsToTrim == 0)
                return value;

            if (charsToTrim >= valueByteCount)
            {
                bytesToTruncate -= Math.Min(bytesToTruncate, valueByteCount);
                return string.Empty;
            }

            var wordBreak = value.LastIndexOfAny(new[] { ' ', '.' }, value.Length - bytesToTruncate);

            if (wordBreak == -1)
            {
                bytesToTruncate -= Math.Min(bytesToTruncate, valueByteCount);
                return string.Empty;
            }

            bytesToTruncate = 0;
            return value.Substring(0, wordBreak);
        }

        private static IEnumerable Truncate(IEnumerable value, ref int bytesToTruncate)
        {
            var list = value.OfType<object>().ToList();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                list[i] = Truncate(list[i], ref bytesToTruncate);

                if (string.IsNullOrEmpty(list[i] as string))
                    list.RemoveAt(i);

                if (bytesToTruncate == 0)
                    break;
            }

            return list;
        }

        private static IDictionary Truncate(IDictionary value, ref int bytesToTruncate)
        {
            var keys = value.Keys.OfType<string>().ToList();

            foreach (var key in keys)
            {
                value[key] = Truncate(value[key], ref bytesToTruncate);

                if (bytesToTruncate == 0)
                    break;
            }

            return value;
        }
    }
}

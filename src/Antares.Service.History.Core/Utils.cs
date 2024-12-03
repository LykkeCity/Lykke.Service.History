using System;
using System.Text.RegularExpressions;

namespace Antares.Service.History.Core
{
    public class Utils
    {
        public static Regex GuidRegex = new Regex(@"[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?", RegexOptions.IgnoreCase);

        public static Guid IncrementGuid(Guid guid, int inc)
        {
            return new Guid(IncrementByteArray(guid.ToByteArray(), inc));
        }

        public static byte[] IncrementByteArray(byte[] array, int inc)
        {
            if (inc < 0)
                throw new ArgumentOutOfRangeException(nameof(inc));

            var index = array.Length - 1;
            while (index >= 0)
            {
                var diff = 255 - array[index];
                if (diff >= inc)
                {
                    array[index] += (byte)inc;
                    break;
                }
                inc -= diff;
                array[index--] = 0;
            }
            return array;
        }

        public static bool TryExtractGuid(string input, out Guid guid)
        {
            var match = GuidRegex.Match(input);

            guid = Guid.Empty;

            if (!match.Success || !Guid.TryParse(match.Value, out var id))
            {
                return false;
            }

            guid = id;
            return true;
        }
    }
}

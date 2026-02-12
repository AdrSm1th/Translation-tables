using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Translation_tables
{
    static class BinarySearch
    {
        public static bool Search(string elem, int key, List<Word> buff)
        {
            int low = 0, high = buff.Count - 1, middle = 0;
            while (low <= high)
            {
                middle = (low + high) / 2;
                Console.WriteLine(buff[middle].name);
                int comparison = string.Compare(elem, buff[middle].name, StringComparison.CurrentCulture);
                if (comparison < 0)
                {
                    high = middle - 1;
                }
                else if (comparison > 0)
                {
                    low = middle + 1;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }
}

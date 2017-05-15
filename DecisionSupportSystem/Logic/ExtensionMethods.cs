using DecisionSupportSystem.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionSupportSystem.Logic
{
    public static class ExtensionMethods
    {
        public static double toDouble(this object o)
        {
            return double.Parse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public static int toInt(this object o)
        {
            return int.Parse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public static int MaxResultIndex(this List<CountResult> list)
        {
            int maxIndex = 0;
            int maxValue = list[0].Count;

            for(int i = 1; i < list.Count; i++)
            {
                if (list[i].Count > maxValue)
                {
                    maxIndex = i;
                    maxValue = list[i].Count;
                }
            }
            return maxIndex;
        }
    }
}

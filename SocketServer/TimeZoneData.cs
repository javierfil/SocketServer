using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class TimeZoneData
    {
        public Dictionary<int, List<TZInterval>> TZDefinition;            // Asociacion de TZNumber y TZIntervals

        public TimeZoneData()
        {
            TZDefinition = new Dictionary<int, List<TZInterval>>();
        }

        public void addTZInterval(int v_TZnum, TZInterval v_Interval)
        {
            if (!TZDefinition.ContainsKey(v_TZnum))
            {
                TZDefinition.Add(v_TZnum, new List<TZInterval>());                
            }

            TZDefinition[v_TZnum].Add(v_Interval);
        }

        public void clearTimeZone(int v_TZnum)
        {
            if (TZDefinition.ContainsKey(v_TZnum))
            {
                TZDefinition.Remove(v_TZnum);
            }
        }
    }
}

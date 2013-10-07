using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class LENELTimeZones
    {
        private TimeZoneData timeZones;                // La definicion de TimeZones se comparte en toda la organizacion
        private int organizationID;

        public LENELTimeZones(int v_organization)
        {
            organizationID = v_organization;
            timeZones = new TimeZoneData();
        }


        public void createTimeZoneData(int v_TZNumber, string v_IntervalData)
        {
            string[] intervalData = v_IntervalData.Split('|');

            timeZones.clearTimeZone(v_TZNumber);                // Desde cero.
            foreach (string intervalo in intervalData)
            {
                if (intervalo.Trim().Length > 0)
                {
                    timeZones.addTZInterval(v_TZNumber, new TZInterval(intervalo));
                }
            }
        }

        public TimeZoneData getTimeZoneData()
        {
            return timeZones;
        }


        public int getOrganizationID()
        {
            return organizationID;
        }


    }
}

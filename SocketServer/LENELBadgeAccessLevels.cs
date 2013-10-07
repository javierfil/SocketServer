using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class LENELBadgeAccessLevels
    {
        Dictionary<string, string> badgesAccessLevels;                  // Asociacion de badge a accessLevels (1,2,3,4)
        int orgID;

        public LENELBadgeAccessLevels(int v_orgID)
        {
            badgesAccessLevels = new Dictionary<string, string>();
            orgID = v_orgID;
        }


        /// <summary>
        /// Se encarga de desasociar la tarjeta con los viejos access levels y de asociarlo a los access levels nuevos
        /// </summary>
        /// <param name="v_badge"></param>
        /// <param name="v_accessLevels"></param>
        /// <returns>True si cambiaron los access levels asociados a esta tarjeta</returns>
        public bool asociarAccessLevel(string v_badge, string v_accessLevels)
        {
            bool res = false;
            if (badgesAccessLevels.ContainsKey(v_badge))    // Si ya está en el diccionario me fijo si los access levels cambiaron
            {
                if (!badgesAccessLevels[v_badge].Equals(v_accessLevels))    // Si los access levels cambiaron los actualizo
                {
                    badgesAccessLevels.Remove(v_badge);
                    badgesAccessLevels.Add(v_badge, v_accessLevels);        // Aqui se guardan los accesslevels actuales de la Badge.
                    res = true;                                             // Informa del cambio
                }
            }
            else
            {
                badgesAccessLevels.Add(v_badge, v_accessLevels);            // Aqui se guardan los accesslevels actuales de la Badge.
                res = true;                                                 // Informa del cambio
            }

            return res;
        }


        public string getAccessLevels(string v_badge)
        {
            string res = "";

            if (badgesAccessLevels.ContainsKey(v_badge))
            {
                res = badgesAccessLevels[v_badge];
            }
            return res;

        }

    }
}

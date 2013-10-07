using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class ZoneAccess
    {
        public string HHID;
        public string ZoneID;
        public string GateID;
        public string Hora;
        public TiposAcceso tipoAcceso;  // Si fue permitido, denegado, etc.

        public ZoneAccess(string v_HHID, string v_zone, string v_Gate, string v_hora, TiposAcceso v_tAcceso)
        {
            HHID = v_HHID;
            ZoneID = v_zone;
            GateID = v_Gate;
            Hora = v_hora;
            tipoAcceso = v_tAcceso;
        }
    }
}

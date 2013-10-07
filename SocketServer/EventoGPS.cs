using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class EventoGPS
    {
        public string HHID;
        public string Latitud;
        public string Longitud;
        public string Hora;
    
        public EventoGPS(string v_HHID, string v_lat, string v_long, string v_hora)
        {
            HHID = v_HHID;
            Latitud = v_lat;
            Longitud = v_long;
            Hora = v_hora;
        }
    }
}

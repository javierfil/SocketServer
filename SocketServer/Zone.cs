using System;
using System.Collections.Generic;
using System.Text;

// Zone: Clase que representa una zona, sus puertas y sus puntos de definicion.
namespace ServerComunicacionesALUTEL
{
    public class Zone
    {

        public const int MAXVEL = 200;      // Velocidad maxima permitida: 200 Km por Hora.       
        public const string GateColorAccessGranted = @"#00FF00";
        public const string GateColorAccessDenied = @"#000000";
        public int LNLPanelID; 
       

        public struct GeoCoord
        {
            public string latitude;
            public string longitude;

            public GeoCoord(string v_lat, string v_lng)
            {
                latitude = v_lat;
                longitude = v_lng;
            }
        }

        public struct ZonePoint
        {
            public string ID;
            public GeoCoord position;

            public ZonePoint(string v_id, string v_lat, string v_lng)
            {
                ID = v_id;
                position = new GeoCoord(v_lat, v_lng);
            }
        }

        public class GateDefinition
        {
            public string ID;
            public GateAccessType type;
            public ZonePoint from;                     // Una puerta está definida por 2 puntos de la zona: desde y hasta.
            public ZonePoint to;
            public int LNLPanelID;                             // El PanelID Asociado en LENEL
            public int LNLReaderID;                            // El ReaderID asociado en LENEL
        }

        /// <summary>
        /// Atributos de la zona   
        /// </summary>
        public string IDZona;                          // Nombre de la zona.
        public Dictionary<string,GateDefinition> listaPuertas;      // Puertas de la zona

        public Zone(string v_id)
        {
            IDZona = v_id;
            listaPuertas = new Dictionary<string,GateDefinition>();
        }

        /// <summary>
        /// Agrega una puerta a la zona
        /// </summary>
        /// <param name="v_Gate"></param>
        public void addGate(string v_nombre, GateDefinition v_Gate)
        {
            listaPuertas.Add(v_nombre, v_Gate);
        }

        /// <summary>
        /// Obtiene todas las puertas de la zona
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,GateDefinition> getGates()
        {
            return listaPuertas;
        }

        /// <summary>
        /// Borra una puerta de la lista de puertas de la zona
        /// </summary>
        /// <param name="idGate"></param>
        /// <returns></returns>
        public bool deleteGate(string idGate)
        {
            bool res = false;

           if(listaPuertas.ContainsKey(idGate))
           {
               listaPuertas.Remove(idGate);
               res = true;
           }

            return res;
        }

       
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class AccessLevelData
    {
        public Dictionary<int, Dictionary<int,int>> ReaderTZ;            // Asociacion de AccessLevelID y su lista de (readerID, TZid): NOTA: HAY UN SOLO TIMEZONE DEFINIDO PARA UN READER, por eso ReaderID es CLAVE.
       

        public AccessLevelData()
        {
            ReaderTZ = new Dictionary<int, Dictionary<int,int>>();
        }

        public void createReaderTZData(int v_accessLevelID, string readerTZData)
        {
            string[] datosRTZ = readerTZData.Split('|');

            if (ReaderTZ.ContainsKey(v_accessLevelID))
            {
                ReaderTZ.Remove(v_accessLevelID);                           // Desde cero
            }

            Dictionary<int, int> listaRTZ = new Dictionary<int, int>();

            foreach (string RTZ in datosRTZ)
            {
                if (RTZ.Trim().Length > 0)
                {
                    string[] datos = RTZ.Split(',');

                    if (datos.Length == 2)
                    {
                        listaRTZ.Add(int.Parse(datos[0]), int.Parse(datos[1]));
                    }
                }
            }
            ReaderTZ.Add(v_accessLevelID, listaRTZ);
        }



        // Agrega una nueva asociacion accessID TZNumber: la crea o la modifica.
        public void addReaderTZData(int v_accessLevelID, string datosRTZ)
        {
            if (!ReaderTZ.ContainsKey(v_accessLevelID))
            {
               Dictionary<int, int> nuevaRTZ = new Dictionary<int, int>();

               ReaderTZ.Add(v_accessLevelID, nuevaRTZ);
            }
            Dictionary<int, int> RTZData = ReaderTZ[v_accessLevelID];

            if (datosRTZ.Trim().Length > 0)
            {
                string[] datos = datosRTZ.Split(',');

                if (datos.Length == 2)
                {

                    int readerID = int.Parse(datos[0]); 
                    int TZid = int.Parse(datos[1]);
                    if (RTZData.ContainsKey(readerID))
                    {
                        RTZData.Remove(readerID);
                    }
                    RTZData.Add(readerID, TZid);
                }
            }
        }

        public void clearData()
        {
            ReaderTZ.Clear();
        }
    }
}

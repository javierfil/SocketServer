using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class TZInterval
    {
        public int horaIni;
        public int minIni;
        public int horaFin;
        public int minFin;
        public byte DOW;            // Dias de la semana encodeados en los bits de un byte.
        public byte HOL;            // Tipos de holidays encodeados en los bits de un byte.

        public TZInterval( int v_hini, int v_minIni, int v_horaFin, int v_minFin, byte v_DOW, byte v_HOL)
        {
            horaIni = v_hini;
            minIni = v_minIni;
            horaFin = v_horaFin;
            minFin = v_minFin;
            DOW = v_DOW;
            HOL = v_HOL;
        }


        public TZInterval(string v_TzIntervalData)
        {
            string[] datos = v_TzIntervalData.Split(',');

            if (datos.Length == 6)
            {
                horaIni = int.Parse(datos[0]);
                minIni = int.Parse(datos[1]);
                horaFin = int.Parse(datos[2]);
                minFin = int.Parse(datos[3]);
                DOW = byte.Parse(datos[4]);
                HOL = byte.Parse(datos[5]);
            }
        }
        /// <summary>
        /// Obtiene la lista de dias a partir del analisis de los BITS del BYTE DOW
        /// Formato de salida: dia1,dia2,dia3
        /// </summary>
        /// <returns></returns>
        public string obtenerListaDias()
        {
            string binaryDays = Convert.ToString(DOW, 2).PadLeft(8, '0');

            string res = "";

            for (int i = 0; i < binaryDays.Length; i++)
            {
                if (binaryDays[i] == '1')
                {
                    int idDOW = i+1;            // Conversion de formato LENEL a DaysOfWeek
                    if (idDOW == 7)
                    {
                        idDOW = 0;              // Conversion de formato LENEL a DaysOfWeek        
                    }
                    res = res + idDOW.ToString() + ",";
                }
            }
            if (res.Length > 0)
            {
                return res.Substring(0, res.Length - 1);       // Le saco la coma
            }
            else
            {
                return res;
            }
        }

        /// <summary>
        /// Obtiene la lista de tipos de holidays a partir del analisis de los bits de BYTE HOL
        /// </summary>
        /// <returns></returns>
        public List<int> obtenerListaTiposHoilidays()
        {
            string binaryHol = Convert.ToString(HOL, 2).PadLeft(8, '0');            // Convierte el byte en Binairo


            List<int> res = new List<int>();

            for (int i = 0; i < binaryHol.Length; i++)
            {
                if (binaryHol[i] == '1')
                {
                    res.Add(i);
                }
            }
            return res;
        }
    }
}

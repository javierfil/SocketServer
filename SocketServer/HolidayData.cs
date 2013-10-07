using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class HolidayData
    {
        public int año;
        public int mes;
        public int dia;
        public byte tipo;
        public int cantDias;

        public HolidayData(string v_holidayDef)
        {
            string[] datos = v_holidayDef.Split(',');

            if (datos.Length == 5)
            {
                año = int.Parse(datos[0]) ;
                mes = int.Parse(datos[1]);
                dia = int.Parse(datos[2]);
                tipo = byte.Parse(datos[3]);
                cantDias = int.Parse(datos[4]);
            }
        }

        // Devuelve true si tipo contiene el bit
        public bool contieneBit(int bit)
        {
            bool res = false;

            string binaryTypes = Convert.ToString(tipo, 2).PadLeft(8, '0');

            for (int i = 0; i < binaryTypes.Length; i++)
            {
                if ((binaryTypes[i] == '1') && (i == bit))
                {
                    res = true;
                }
            }
            return res;
        }
    }
}

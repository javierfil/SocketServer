using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class Acceso
    {
        public string HHID;
        public string Tarjeta;
        public string Latitud;
        public string Longitud;
        public string Hora;
        public byte[] imagen;          // byteArray con la foto(datos jpg).
        public TiposAcceso tipoAcceso;  // Si fue permitido, denegado, etc.
        public int idEmpleado;

        public Acceso(int pidEmpleado, string v_HHID, string v_tarjeta, string v_lat, string v_long, string v_hora, byte[] v_imagen, TiposAcceso v_tAcceso)
        {
            HHID = v_HHID;
            Tarjeta = v_tarjeta;
            Latitud = v_lat;
            Longitud = v_long;
            Hora = v_hora;
            imagen = v_imagen;
            tipoAcceso = v_tAcceso;
            idEmpleado = pidEmpleado;
        }
        public void attachImagen(byte[] v_imagen)
        {
            imagen = v_imagen;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class Visita
    {
        public string HHID;
        public string Nombre;
        public string Apellido;
        public string Documento;
        public string Empresa;
        public string Tarjeta;
        public string Latitud;
        public string Longitud;
        public string Hora;
        public byte[] imagen;          // byteArray con la foto(datos jpg).
        public TiposAcceso tipoAcceso;  // Si fue permitido, denegado, etc.
        public int idOrg;

        public Visita(string v_HHID, string v_Nombre, string v_Apellido, string v_Documento, string v_Empresa,  string v_tarjeta, string v_lat, string v_long, string v_hora, byte[] v_imagen, TiposAcceso v_tAcceso, int v_idOrg)
        {
            HHID = v_HHID;
            Nombre = v_Nombre;
            Apellido = v_Apellido;
            Documento = v_Documento;
            Empresa = v_Empresa;
            Tarjeta = v_tarjeta;
            Latitud = v_lat;
            Longitud = v_long;
            Hora = v_hora;
            imagen = v_imagen;
            tipoAcceso = v_tAcceso;
            idOrg = v_idOrg;

        }

        public void attachImagen(byte[] v_imagen)
        {
            imagen = v_imagen;
        }

    }

}

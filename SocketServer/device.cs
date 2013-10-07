using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class device
    {
        public int identificador = 0;
        public string HHID;
        string Marca;
        string Modelo;
        public device(string pid, string pmarca, string pmodelo, int pidentificador)
        {
            HHID = pid;
            Marca = pmarca;
            Modelo = pmodelo;
            identificador = pidentificador;
        }
        public string ID
        {
            get
            {
                return HHID;
            }
            set
            {
                HHID = value;
            }
        }
        public string MARCA
        {
            get
            {
                return Marca;
            }
            set
            {
                Marca = value;
            }
        }
        public string MODELO
        {
            get
            {
                return Modelo;
            }
            set
            {
                Modelo = value;
            }
        }
        public int IDENTIFICADOR
        {
            get
            {
                return identificador;
            }
            set
            {
                identificador = value;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class JobToSendToClient
    {
        public int ID;                     // ID del trabajo: el que se desencolará cuando el envío sea exitoso
        public string header;              // header a enviar.
        public byte[] byteData;                // datos a enviar.
    }
}

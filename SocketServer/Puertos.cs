using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class Puertos
    {

        public int PORT_TO_SEND;
        public int PORT_TO_RECEIVE;
    
        public Puertos()
        {
            PORT_TO_SEND =SystemConfiguration.SendPort;
            PORT_TO_RECEIVE = SystemConfiguration.ReceivePort;

        }
    }
}

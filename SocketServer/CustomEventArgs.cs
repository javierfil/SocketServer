using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class stringEventArgs : EventArgs
    {
        public string text;
        public TiposLOG LOGTYPE;
        public StateObject stateObject;
        public Dictionary<string, string> textData = new Dictionary<string, string>(); // Datos recibidos

        public stringEventArgs(string v_value, StateObject v_stateObject)
        {
            text = v_value;
            stateObject = v_stateObject;
            LOGTYPE = TiposLOG.ALL;
        }
    }

    public class byteArrayEventArgs : EventArgs
    {
        public string header;                // El string del header del chunk
        public byte[] byteData;              // Los datos del chunk
        public StateObject stateObject;     // El stateObject asociado al evento
        public Dictionary<string, string> textData = new Dictionary<string, string>(); // Datos recibidos

        public byteArrayEventArgs(string v_header, byte[] v_data, StateObject v_state)
        {
            header = v_header;
            byteData = v_data;
            stateObject = v_state;
        }

    }
}

// LAYER HHGPS: Manda la informacion recibida desde los HH al servicio CapturaDatos ubicado en GPSControlIP y GPSControlPort(8999 por defecto)
// Su funcion principal es hacer que la poscion GPS de los HandhHelds se vea en la Web.
// Para ello, debe estar levantado en la Web un GPS con el nombre correspondiente al HH y debe estar asignado a un vehiculo
// Cuando el HH se conecta, arma el cabezal de conexion usando el nombre del HH y le envia informacion de posicion



using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Globalization;

namespace ServerComunicacionesALUTEL
{
    public class LAYERHHGPS
    {
        Dictionary<string, NetworkStream> conexiones;

        Aplicacion mainApp;

        public StringEventHandler actualizarLOG;
      
        /// <summary>
        /// Server general de comunicaciones con ALUTRACK. Aqui se centralizan todas las conexiones y
        /// desconexiones.
        /// </summary>
        public LAYERHHGPS(Aplicacion v_app)
        {
            conexiones = new Dictionary<string, NetworkStream>();
            mainApp = v_app;
            
        }

        public bool conectar(string v_HHID, out string v_message)
        {
            UInt32 alutrackID = translateFromString(v_HHID);        // long: 4 bytes: 2 exp 32
            Byte[] headerSync = makeHeader(alutrackID);
            v_message = "";
            //loguearString("AlutrackLayer.conectar. HHID: " + v_HHID);
            try
            {
                //TcpClient cliente = new TcpClient("192.168.1.6", 8999);
                TcpClient cliente = new TcpClient(SystemConfiguration.GPSControlIP,SystemConfiguration.GPSControlPort);

                NetworkStream stream = cliente.GetStream();
                stream.Write(headerSync, 0, headerSync.Length);
                stream.Flush();

                // Se conectó y mandó el sync.
                // Ahora espera el sync de respuesta.

                stream.ReadTimeout = 5000;
                Byte[] buffer = new Byte[8];
                stream.Read(buffer, 0, buffer.Length);

                // Leyó el sync. Conexion dada por ok, levanto el stream y el ID en la coleccion de control.
                if (conexiones.ContainsKey(v_HHID))
                {
                    conexiones.Remove(v_HHID);
                }
                
                conexiones.Add(v_HHID, stream);
                v_message = "OK";
                return true;

            }
            catch (Exception ex )
            {
                v_message = "ERROR al conectar: " + ex.Message;
                return false;
                // No se conectó, no lo doy de alta.
            }
        }
        /// <summary>
        /// ¿Está conectado?
        /// </summary>
        /// <param name="v_HHID"></param>
        /// <returns></returns>
        public bool isConnected(string v_HHID)
        {
            return conexiones.ContainsKey(v_HHID);
        }


        public void loguearString(string v_texto)
        {
            if (actualizarLOG != null)
            {
                stringEventArgs p = new  stringEventArgs(v_texto, null);
                p.LOGTYPE = TiposLOG.HH;
                actualizarLOG(this, p);
            }

        }



        /// <summary>
        /// Desconecta un HandHeld del ALUTRACK
        /// </summary>
        /// <param name="v_HHID"></param>
        public void desconectar(string v_HHID)
        {
            if (conexiones.ContainsKey(v_HHID))
            {
                NetworkStream stream = conexiones[v_HHID];

                conexiones.Remove(v_HHID);

                try
                {
                    // Cierro el socket y libero los recursos.
                    stream.Close();
                    stream.Dispose();
                }
                catch (Exception )
                {
                    // ok, error al desconectar... Salir
                }
            }
        }

        /// <summary>
        /// Envia datos GPS al servicio CapturaDatos usando el puerto de control, siguiendo el siguiente formato: 
        /// UnitID(Alutrack)   ,   FechaHora  ,Long , Lat, Vel., Heading, Altitud , Satelitaes, ReportID  , Odómetro, Inputs,  InputsAnalog_1, InputsAnalog_2, Outputs
        /// 2100000001 ,20070313170020,121.123456,12.654321,45   , 233     ,0       , 9         ,    0      ,  0.0    , 3     ,     0.00       ,     0.00      ,   5
        /// 
        /// Retorna TRUE si pudo enviar sin error y FALSE en caso contrario
        /// </summary>
        /// <param name="v_HHID"></param>
        /// <param name="v_Latitud"></param>
        /// <param name="v_longitud"></param>
        public bool sendGPSData(string v_HHID, string v_Latitud, string v_longitud, string v_hora, string v_heading,string v_speed, out string v_message)
        {
            v_message = "";

            //mainApp.DataManager.actualizarUltimaPosGPS(v_Latitud, v_longitud,v_hora, v_HHID);

            if (isConnected(v_HHID))
            {
                long alutrackID = translateFromString(v_HHID);

                string alutrackIDStr = alutrackID.ToString();

                DateTime fechaHora;
                fechaHora = new DateTime();
                fechaHora = DateTime.ParseExact(v_hora, "yyyy-MM-dd HH:mm:ss",null);

                string año = fechaHora.Year.ToString();
                string mes = (fechaHora.Month.ToString()).PadLeft(2, '0');
                string dia = (fechaHora.Day.ToString()).PadLeft(2, '0');
                string hora = (fechaHora.Hour.ToString()).PadLeft(2, '0');
                string minutos = (fechaHora.Minute.ToString()).PadLeft(2, '0');
                string segundos = (fechaHora.Second.ToString()).PadLeft(2, '0');

                string strFechaHora = año + mes + dia + hora + minutos + segundos;

                // Convertir a entero
                float dbl_Speed = float.Parse(v_speed, CultureInfo.InvariantCulture.NumberFormat);
                string velocidad = ((int)dbl_Speed).ToString();

                // Convertir a entero
                float dbl_rumbo = float.Parse(v_heading, CultureInfo.InvariantCulture.NumberFormat);
                string rumbo = ((int)dbl_rumbo).ToString();


                // Por completar desde el HH: (ver que son cada uno...)
                string altitud = "0";
                string satelites = "1";
                string reportID = "0";
                string odometro = "0.0";
                string Inputs = "3";
                string InputsAnalog_1 = "0";
                string InputsAnalog_2 = "0";
                string Outputs = "0";


                string strDataToSend = alutrackID + "," + strFechaHora + "," + v_longitud + "," + v_Latitud + "," + velocidad + "," + rumbo + "," + altitud + "," + satelites + "," + reportID + "," + odometro + "," + Inputs + "," + InputsAnalog_1 + "," + InputsAnalog_2 + "," + Outputs;

                try
                {
                    NetworkStream stream = conexiones[v_HHID];
                    stream.Write(Encoding.Default.GetBytes(strDataToSend), 0, strDataToSend.Length);
                    stream.Flush();
                    v_message = "OK: " + strDataToSend;
                }
                catch (Exception ex)
                {
                    v_message = "ERROR: " + ex.Message;
                    return false;           // Network exception: no envió.

                }

                return true;
            }
            else
            {
                v_message = "ERROR: trató de enviar y no estaba conectado";
                return false;   // No estaba conectado....
            }
        }

/// <summary>
/// Convierte al LONG (entero de 4 bytes), un string de 4 letras. Toma las primeras 4 letras del 
/// string y arma el long concatenando su valor ASCII -32
/// </summary>
/// <param name="v_HHID"></param>
/// <returns></returns>
        private UInt32 translateFromString(string v_HHID)
        {
            string resStr = "30";
            if (v_HHID.Length< 4 ) 
            {
                v_HHID = v_HHID.PadRight(4,' ');
            }


            for (int i = 0; (i < v_HHID.Length) && (i<4); i++)
            {
                int t = (int)v_HHID[i] -32;

                resStr = resStr + t.ToString().PadLeft(2, '0'); 
            }
            return Convert.ToUInt32(resStr);
        }

        private Byte[] makeHeader(UInt32 alutrackID)
        {
            Byte[] result = new Byte[8];

            result[0] = 200;            // Byte 0 = 200 para los HH
            result[1] = 201;            // Byte 1 = 200 
            result[2] = 0;
            result[3] = 0;

            byte[] byteArray = BitConverter.GetBytes(alutrackID);
            result[4] = byteArray[0];
            result[5] = byteArray[1];
            result[6] = byteArray[2];
            result[7] = byteArray[3];
            return result;
        }

    }
}

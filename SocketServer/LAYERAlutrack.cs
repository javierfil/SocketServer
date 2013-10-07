#define SERVER
//#define VERSIONEMP
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ServerComunicacionesALUTEL
{
   public class LAYERAlutrack
    {
       #if SERVER        
        
#else
        int puertoLenel1 = 10998;
        int puertoLenel2 = 10999;
#endif

        private Dictionary<string,byte[]> repositorioBuffer;
        private Aplicacion mainApp;
        private LAYERCommunication ComunicationLayer;
        
        public TcpListener servidorHeaders;
        public TcpListener servidorDatos;

        StringEventHandler m_LOGHandler;
        public bool isListening = false;
                    

        //Expresiones regulares para el reconocimiento de mensajes

        //ACCESSLEVELS
        //string MENSAJE = @"TYPE:ALT_DELETEACCESSLEVELS:IDORGANIZACION:" + idOrganizacion.ToString() + "," + "IDACCESSLEVELS:"  + idAccess.ToString();
        Regex ALT_DELETEACCESSLEVELS = new Regex(@"TYPE:ALT_DELETEACCESSLEVELS:IDORGANIZACION:(.*),IDACCESSLEVELS:(.*)");
        //string MENSAJE = @"TYPE:ALT_DELETEINTERVALACCESSLEVELS:IDORGANIZACION:" + idOrganizacion.ToString() + "," + "IDACCESSLEVELS:"  + idintervalo.ToString();
        Regex ALT_DELETEINTERVALACCESSLEVELS = new Regex(@"TYPE:ALT_DELETEINTERVALACCESSLEVELS:IDORGANIZATION:(.*),IDACCESSLEVELS:(.*)");
        //string MENSAJE = @"TYPE:ALT_ADDACCESSLEVELS:IDORGANIZACION:" + idOrganizacion.ToString() + "," + "IDACCESSLEVELS:"  + retornoagregar.ToString();
        Regex ALT_ADDACCESSLEVELS = new Regex(@"TYPE:ALT_ADDACCESSLEVELS:ORGANIZATION:(.*),IDACCESSLEVELS:(.*)");
        //string MENSAJE = @"TYPE:ALT_MODIFYACCESSLEVELS:IDORGANIZACION:" + idOrganizacion.ToString() + "," + "IDACCESSLEVELS:"  + id;
        Regex ALT_MODIFYACCESSLEVELS = new Regex(@"TYPE:ALT_MODIFYACCESSLEVELS:ORGANIZATION:(.*),IDACCESSLEVELS:(.*)");

        //HOLIDAYS
        //string MENSAJE = @"TYPE:ALT_DELETEHOLIDAYS,IDORGANIZACION:" +idOrganizacion.ToString()+","+"IDHOLIDAY:"+idHoliday.ToString();
        Regex ALT_DELETEHOLIDAYS = new Regex(@"TYPE:ALT_DELETEHOLIDAYS,IDORGANIZATION:(.*),IDHOLIDAY:(.*)");
        //string MENSAJE = @"TYPE:ALT_MODIFYHOLIDAYS:IDORGANIZACION:" + idOrganizacion.ToString() + "," + "IDHOLIDAY:"+ id.ToString(); 
        Regex ALT_MODIFYHOLIDAYS = new Regex(@"TYPE:ALT_MODIFYHOLIDAYS:IDORGANIZATION:(.*),IDHOLIDAY:(.*)");
        //string MENSAJE=@"TYPE:ALT_ADDHOLIDAYS,ORGANIZATION:"+idOrganizacion +","+"IDHOLIDAYS:"+retorno1.ToString();
        Regex ALT_ADDHOLIDAYS = new Regex(@"TYPE:ALT_ADDHOLIDAYS,ORGANIZATION:(.*),IDHOLIDAYS:(.*)");


        //EMPLOYEE
        //string mensaje = "TYPE:ALT_ADDCARDEMPLOYEE,ORGANIZATION:" + idOrganizacion.ToString() +","+ "IDEMPLEADO:" + dt.IDEMPLEADO  +","+ "IDTARJETA:" + idTarjetaAgregar;
        //Regex ALT_ADDCARDEMPLOYEE = new Regex(@"TYPE:ALT_ADDCARDEMPLOYEE,ORGANIZATION:(.*),IDEMPLEADO:(.*),IDTARJETA:(.*)");
        Regex ALT_ADDCARDEMPLOYEE = new Regex(@"TYPE:ALT_ADDCARDEMPLOYEE,ORGANIZATION:(.*),IDTARJETA:(.*)");
        //string mensaje = "TYPE:ALT_DELETEEMPLOYEE,IDEMPLEADO:" + dt.IDEMPLEADO.ToString();
        Regex ALT_DELETEEMPLOYEE = new Regex(@"TYPE:ALT_DELETEEMPLOYEE,IDTARJETA:(.*),IDEMPLEADO:(.*)");
        //string mensaje = "TYPE:ALT_MODIFYCARDEMPLOYEE,ORGANIZATION:" + idOrganizacion.ToString() +","+ "IDEMPLEADO:" + dt.IDEMPLEADO.ToString()   + "IDTARJETA:" + idTarjetaAgregar.ToString() ;
        Regex ALT_MODIFYCARDEMPLOYEE = new Regex(@"TYPE:ALT_MODIFYCARDEMPLOYEE,ORGANIZATION:(.*),IDEMPLEADO:(.*)IDTARJETA:(.*)");


        //TARJETAS
        Regex ALT_DELETEBADGE = new Regex(@"TYPE:ALT_DELETEBADGE,NUMEROTARJETA:(.*),IDORGANIZACION:(.*)");

        
        //TIMEZONES
        //string mensaje = "TYPE:ALT_DELETETIMEZONE,ORGANIZATION:" + idOrganizacion +","+ "TZNUM:" + TZNUM.ToString();
        Regex ALT_DELETETIMEZONE = new Regex(@"TYPE:ALT_DELETETIMEZONE,ORGANIZATION:(.*),TZNUM:(.*)");
        //string mensaje = "TYPE:ALT_DELETETIMEZONEINTERVAL,ORGANIZATION:" + idOrganizacion  +","+ "TZNUM:" + tznum.ToString() +","+ "IDTIMEZONE:"+idTimeZone.ToString();   
        Regex ALT_DELETETIMEZONEINTERVAL = new Regex(@"TYPE:ALT_DELETETIMEZONEINTERVAL,ORGANIZATION:(.*),TZNUM:(.*),IDTIMEZONE:(.*)");
        //string mensaje = "TYPE:ALT_ADDTIMEZONE,ORGANIZATION:" + idOrganizacion.ToString() +","+ "TZNUM:" + tznum.ToString() +","+"IDTIMEZONE:"+retorno2.ToString();
        Regex ALT_ADDTIMEZONE = new Regex(@"TYPE:ALT_ADDTIMEZONE,ORGANIZATION:(.*),TZNUM:(.*),IDTIMEZONE:(.*)");
        //string mensaje = "TYPE:ALT_MODIFYTIMEZONE,ORGANIZATION:" + idOrganizacion.ToString() +","+ "TZNUM:" + TZnum.ToString() +","+ "IDTIMEZONE:"+retorno2;  
        Regex ALT_MODIFYTIMEZONE = new Regex(@"TYPE:ALT_MODIFYTIMEZONE,ORGANIZATION:(.*),TZNUM:(.*),IDTIMEZONE:(.*)");

        //ZONES
        //string mensaje = "TYPE:ALT_ADDZONE,ORGANIZATION:" + idOrganizacion.ToString()+ "," + "IDZONE:" + ID;
        Regex ALT_ADDZONE = new Regex(@"TYPE:ALT_ADDZONE,ORGANIZATION:(.*),IDZONE:(.*)");
        //string mensaje = "TYPE:ALT_DELETEZONE,ORGANIZATION:"+idOrganizacion.ToString()  + "," + "IDZONE:" + this.Datos_Pantalla.ZonaEliminar.ID;
        Regex ALT_DELETEZONE = new Regex(@"TYPE:ALT_DELETEZONE,ORGANIZATION:(.*),IDZONE:(.*)");
        //string mensaje = "TYPE:ALT_MODIFYZONE,ORGANIZATION:" + idOrganizacion.ToString() + "," + "IDZONE:" + id.ToString()  ;    
        Regex ALT_MODIFYZONE = new Regex(@"TYPE:ALT_MODIFYZONE,ORGANIZATION:(.*),IDZONE:(.*)");

        //TARJETASYACCESSLEVELS
        //string mensajea = "TYPE:ALT_ADDACCESLEVELSATARJETA,ORGANIZATION:" + idOrganizacion.ToString() + "," + "IDTARJETA:" + idTarjeta.ToString();
        Regex ALT_ADDACCESLEVELSATARJETA = new Regex(@"TYPE:ALT_ADDACCESLEVELSATARJETA,ORGANIZATION:(.*),IDTARJETA:(.*),IDACCESS:(.*)");
        //string mensajea = "TYPE:ALT_MODIFYACCESLEVELSATARJETA,ORGANIZATION:" + idOrganizacion.ToString() + "," + "IDTARJETA:" + id.ToString();
        Regex ALT_DELETEACCESSLEVELSATARJETA = new Regex(@"TYPE:ALT_DELETEACCESLEVELSATARJETA,ORGANIZATION:(.*),IDTARJETA:(.*),IDACCESS:(.*)");

        //MENSAJES DE GPS
        //string comando = "TYPE:GPSREPORT,DEVICEID:" + medicion.UnitID + "DATE:" + medicion.FechaHora.ToString("yyyy-MM-dd hh:mm:ss") + "LAT:" + medicion.Latitud + "LONG:" + medicion.Longitud + "ORGANIZATION:" + this.idOrganizacion;
        Regex ALT_GPSREPORT = new Regex(@"TYPE:GPSREPORT,DEVICEID:(.*),DATE:(.*),LAT:(.*),LONG:(.*),ORGANIZATION:(.*)");

        //string comando = "TYPE:GPSACCESS,DEVICEID:" + medicion.UnitID + "DATE:" + medicion.FechaHora.ToString("yyyy-MM-dd hh:mm:ss") + "LAT:" + medicion.Latitud + "LONG:" + medicion.Longitud + "ORGANIZATION:" + this.idOrganizacion + "IDTARJETA:" + medicion.idTag;
        Regex ALT_GPSACCESS = new Regex(@"TYPE:GPSACCESS,DEVICEID:(.*),DATE:(.*),LAT:(.*),LONG:(.*),ORGANIZATION:(.*),IDTARJETA(.*)");
       
        /// <summary>
        /// Server general de comunicaciones con ALUTRACK. Aqui se centralizan todas las conexiones y
        /// desconexiones.
        /// </summary>
        public LAYERAlutrack(Aplicacion v_app,StringEventHandler v_logHandler)
        {
            mainApp = v_app;
            m_LOGHandler = v_logHandler;
            repositorioBuffer = new Dictionary<string,byte[]>();
            ComunicationLayer = new LAYERCommunication(v_app,v_logHandler);
        }    

        /// <summary>
        /// Thread lanzado por el serverComunicaciones para atender conexiones y pedidos desde LENEL.
        /// </summary>
        /// <returns></returns>
        public void startListening()
        {
            int puertoAlutrack1 = SystemConfiguration.AlutrackPort1;
            int puertoAlutrack2 = SystemConfiguration.AlutrackPort2;

            isListening = true;

            try
            {
                servidorHeaders = new TcpListener(IPAddress.Parse(StaticTools.obtenerLocalIPAddress()), puertoAlutrack1);//7981);

                servidorHeaders.Start();

                servidorDatos = new TcpListener(IPAddress.Parse(StaticTools.obtenerLocalIPAddress()), puertoAlutrack2);//7987);
               
                servidorDatos.Start();
                loguearString("Aceptando conexiones en el ALUTRACK Layer");
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en ALUTRACKLayer.startListening(): " + ex.Message);
                isListening = false;
            }

            while (isListening)
            {
                try
                {
                    byte [] buffer = new byte[1024];  // Longitud maxima del header
                    
                    TcpClient cliente = servidorHeaders.AcceptTcpClient();
                    // NOTA: Aqui podria crear un thread para leer los datos y procesar los pedidos sin bloquear al 
                    //       resto de los pedidos de conexiones. 

                    NetworkStream netStream = cliente.GetStream();
                    netStream.ReadTimeout = 3000;
                    netStream.Read(buffer, 0, buffer.Length);

//                  loguearString("Buffer leido: "  +buffer.Length.ToString());

                    ProcesarPedidoAlutrack(netStream,buffer);     // Acá se hacen los send, etc y el FLUSH final

                    // Cierro la conexion.
                    cliente.Client.Shutdown(SocketShutdown.Both);
                }

                catch (Exception ex)
                {
                    if (isListening)
                    {
                        loguearString("Excepcion en ALUTRACKLayer: " + ex.Message);
                    }
                }
            }
        }

        public void stopListening()
        {
            isListening = false;
            if (servidorHeaders != null)
            {
                servidorHeaders.Stop();
            }
            if(servidorDatos !=null)
            {
            servidorDatos.Stop();
            }
            loguearString("Servidor ALUTRACK detenido");
        }
        /// <summary>
        /// Procesamiento de un pedido desde LENEL. En todos los casos, el pedido es reconocido, luego se envia una
        /// respuesta o muchasy se hace FLUSH.
        /// Inmediatamente despues de esta salida se DESCONECTARA al cliente.
        /// </summary>
        /// <param name="netStream"></param>
        /// <param name="v_buffer"></param>
        private void ProcesarPedidoAlutrack(NetworkStream netStream, byte[] v_buffer)
        {
            Match matchHeader;
            string texto = Encoding.Default.GetString(v_buffer).Replace("\0", "");
            #region BorrarAccesslevels
            // Regex ALT_DELETEACCESSLEVELS = new Regex(@"TYPE:ALT_DELETEACCESSLEVELS:IDORGANIZACION:(.*),IDACCESSLEVELS:(.*)");
            matchHeader = ALT_DELETEACCESSLEVELS.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    //Extraigo los datos que vienen en el mensaje.
                    loguearString("Comando reconocido: " + texto);
                    int organizationID = Convert.ToInt32(getMatchData(matchHeader, 1));
                    string accessLevelID = getMatchData(matchHeader, 2);

                    //Llamo la operacion que distribuye la nueva modificacion de ACCESSLEVELS  a los HH involucrados.
                    EnviarACCCESLEVESALosHH(organizationID, accessLevelID, "DELETEACCESSLEVELS");
                    loguearString("ALT_DELETEACCESSLEVELS: Organization: " + organizationID + " AccessLevel: " + accessLevelID);
                }
                catch (Exception e) { loguearString("Excepcion en ALT_DELETEACCESSLEVELS: " + e.Message); }
                return;
            }
            #endregion
            #region AgregarAccesslevels
            matchHeader = ALT_ADDACCESSLEVELS.Match(texto);
            //Regex ALT_ADDACCESSLEVELS = new Regex(@"TYPE:ALT_ADDACCESSLEVELS:IDORGANIZACION:(.*),IDACCESSLEVELS:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);     
                    //Extraigo los datos que vienen en el mensaje
                    int organizationID = Convert.ToInt32(getMatchData(matchHeader, 1));                    
                    string accesslevelID = getMatchData(matchHeader, 2);
                    //Llamo la operacion que distribuye la nueva modificacion de ACCESSLEVELS  a los HH involucrados.
                    EnviarACCCESLEVESALosHH(organizationID, accesslevelID, "ADDACCESSLEVELS");
                                       

                }
                catch (Exception e) { loguearString("Excepcion en ALT_ADDACCESSLEVELS: " + e.Message); }
                return;
            }
            #endregion
            #region ModificarAccesslevels
            matchHeader = ALT_MODIFYACCESSLEVELS.Match(texto);
            //Regex ALT_MODIFYACCESSLEVELS = new Regex(@"TYPE:ALT_MODIFYACCESSLEVELS:IDORGANIZACION:(.*),IDACCESSLEVELS:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    //Estraigo los datos que vienen en el mensaje
                    loguearString("Comando reconocido: " + texto);   
                    int organizationID = Convert.ToInt32(getMatchData(matchHeader, 1));
                    string accesslevelID = getMatchData(matchHeader, 2);
                    //Llamo la operacion que distribuye la modificacion de Accesslevels a los HH involucrados.
                    EnviarACCCESLEVESALosHH(organizationID,accesslevelID, "MODIFYACCESSLEVELS");
                    
                }
                catch (Exception e) { loguearString("Excepcion en ALT_MODIFYACCESSLEVELS: " + e.Message); }

                return;
            }
            #endregion
            #region BorrarHolidays.
            matchHeader = ALT_DELETEHOLIDAYS.Match(texto);
            //Regex ALT_DELETEHOLIDAYS = new Regex(@"TYPE:ALT_DELETEHOLIDAYS,IDORGANIZACION:(.*),IDHOLIDAY:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto); 
                    //Extraigo del mensaje los datos necesarios.
                    int organizationID = int.Parse(getMatchData(matchHeader, 1));
                    string holidayID = getMatchData(matchHeader, 2);

                    //BORRAR LOS HOLIDAYS DE LOS HH DE LA ORGANIZACION. 
                    EnviarHolidaysALosHH(organizationID, holidayID, "ALT_DELETEHOLIDAYS");
                    
                }
                catch (Exception e) { loguearString("Excepcion en ALT_DELETEHOLIDAYS: " + e.Message); } 
                return;
            }
            #endregion
            #region ModificarHolidays
            matchHeader = ALT_MODIFYHOLIDAYS.Match(texto);
            //Regex ALT_MODIFYHOLIDAYS = new Regex(@"TYPE:ALT_MODIFYHOLIDAYS:IDORGANIZACION:(.*),IDHOLIDAY:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    //Extraigo del mensaje los datos necesarios
                    int organizationID = int.Parse(getMatchData(matchHeader, 1));
                    string holidayID = getMatchData(matchHeader, 2);

                    //Enviar a los HH la modificacion.
                    EnviarHolidaysALosHH(organizationID,holidayID,"ALT_MODIFYHOLIDAYS");
                    
                }
                catch (Exception e) { loguearString("Excepcion en ALT_MODIFYHOLIDAYS: " + e.Message); }
                return;
            }
            #endregion
            #region AgregarHolidays
            matchHeader = ALT_ADDHOLIDAYS.Match(texto);
            //Regex ALT_ADDHOLIDAYS = new Regex(@"TYPE:ALT_ADDHOLIDAYS,ORGANIZATION:(.*),IDHOLIDAYS:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    //Extraigo del mensaje los datos necesarios.
                    int organizationID = int.Parse(getMatchData(matchHeader, 1));
                    string holidayID = getMatchData(matchHeader, 2);

                    //Enviar  HOLIDAYS A LOS HH DE LA ORGANIZACION. 
                    EnviarHolidaysALosHH(organizationID, holidayID, "ALT_ADDHOLIDAYS");
                }
                catch (Exception e) { loguearString("Excepcion en ALT_ADDHOLIDAYS: " + e.Message); }
                return;
            }
            #endregion


            matchHeader = ALT_DELETEINTERVALACCESSLEVELS.Match(texto);
            //Regex ALT_DELETEINTERVALACCESSLEVELS = new Regex(@"TYPE:ALT_DELETEINTERVALACCESSLEVELS:IDORGANIZACION:(.*),IDACCESSLEVELS:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    string organizationID = getMatchData(matchHeader, 1);
                    string accesslevelID = getMatchData(matchHeader, 2);


                    loguearString("ALT_DELETEINTERVALACCESSLEVELS: Organization: " + organizationID + " Accesslevel: " + accesslevelID);
                    //MANDAR BORRAR LOS INTERVALACCESSLEVELS DE LOS HH DE LA ORGANIZACION.
                }

                catch (Exception e) { loguearString("Excepcion en ALT_DELETEINTERVALACCESSLEVELS: " + e.Message); }
                return;
            }

            #region AgregarEmpleado
            matchHeader = ALT_ADDCARDEMPLOYEE.Match(texto);
            //Regex ALT_ADDCARDEMPLOYEE = new Regex(@"TYPE:ALT_ADDCARDEMPLOYEE,ORGANIZATION:(.*),IDEMPLEADO:(.*),IDTARJETA:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    string organizationID = getMatchData(matchHeader, 1);
                    string tarjetaID = getMatchData(matchHeader, 2);
                    
                    mainApp.DataManager.actualizarMemoriaTarjetasyEmpleados(tarjetaID);

                    // Conseguir tarjeta 
                    Tarjeta T = mainApp.DataManager.obtenerTarjetaPorID(tarjetaID);
                    
                    // Obtener su lista de accesslevels
                    string accessLevels = T.accessLevels;                    
                    
                    // Para cada uno obtener la lista de HH
                    if (!accessLevels.Equals(string.Empty))
                    {
                        string[] als = accessLevels.Split(',');
                        List<string> aux;
                        List<string> lstHH = new List<string>();
                        foreach(string al in als)
                        {
                            aux = mainApp.DataManager.ObtenerListaHHIdPorAccessLevel(al, Convert.ToInt32(organizationID));
                            foreach (string alInAux in aux)
                            {
                                if (lstHH.Contains(alInAux)) lstHH.Add(alInAux);
                            }
                        }
                        
                        // Para cada HH, enviarAgregarUnEmpleado()
                        foreach (string HH in lstHH)
                        {
                            ComunicationLayer.enviarAgregarUnEmpleado(T.numerodetarjeta, HH);
                            loguearString("ALT_ADDCARDEMPLOYEE enviado a "+ HH +". Organization: " + organizationID + " Tarjeta: " + tarjetaID);
                        }
                    }
                }
                catch (Exception e) { loguearString("Excepcion en ALT_ADDCARDEMPLOYEE: " + e.Message); }
                return;
            } 
            #endregion
            #region BorrarEmpleado
            matchHeader = ALT_DELETEEMPLOYEE.Match(texto);
            //Regex ALT_DELETEEMPLOYEE = new Regex(@"TYPE:ALT_DELETEEMPLOYEE,IDTARJETA:(.*),IDEMPLEADO:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    string idTarjeta = getMatchData(matchHeader, 1);
                    string empleadoID = getMatchData(matchHeader, 2);

                    //BORRAR EMPLEADOS EN LOS HH DE LA ORGANIZACION. 
                    List<string> lstHH = mainApp.DataManager.ObtenerHHxIdTarjeta(idTarjeta);
                    long tarjeta = mainApp.DataManager.ObtenerTarjeta(idTarjeta);                    
                    foreach (string HH in lstHH) {
                        ComunicationLayer.enviarBorrarEmpleado(tarjeta.ToString(), HH);
                    }

                    //Actualizamos los empleados en memoria
                    mainApp.DataManager.deleteEmpFromMemory(empleadoID);

                    loguearString("ALT_DELETEEMPLOYEE. IdTarjeta:" + idTarjeta);
                   
                }
                catch (Exception e) { loguearString("Excepcion en ALT_DELETEEMPLOYEE: " + e.Message); }
                return;
            }
            #endregion
            #region ModificarEmpleado
            matchHeader = ALT_MODIFYCARDEMPLOYEE.Match(texto);            
            //Regex ALT_MODIFYCARDEMPLOYEE = new Regex(@"TYPE:ALT_MODIFYCARDEMPLOYEE,ORGANIZATION:(.*),IDEMPLEADO:(.*)IDTARJETA:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    string organizationID = getMatchData(matchHeader, 1);
                    string empleadoID = getMatchData(matchHeader, 2);
                    string tarjetaID = getMatchData(matchHeader, 3);
                    //MODIFICAR EMPLEADO EN LOS HH DE LA ORGANIZACION. La llamada en LAYERCommunication para agregar empleado incluye tb la logica para modificar

                    ComunicationLayer.enviarAgregarUnEmpleado(tarjetaID, organizationID);
                    loguearString("ALT_MODIFYCARDEMPLOYEE. Organization: " + organizationID + " Empleado: " + empleadoID + " Tarjeta: " + tarjetaID);
                    
                                                   }
                catch (Exception e) { loguearString("Excepcion en ALT_MODIFYCARDEMPLOYEE: " + e.Message); }
                return;
            }
            #endregion
            #region Tarjetas
            matchHeader = ALT_DELETEBADGE.Match(texto);
            // Regex ALT_DELETEBADGE = new Regex(@"TYPE:ALT_DELETEBADGE,NUMEROTARJETA:(.*),IDORGANIZACION:(.*)");
            

            if (matchHeader.Success)
            {
                try
                {
                    string idTarjeta = "";
                    loguearString("Comando reconocido: " + texto);
                    string numeroTarjeta = getMatchData(matchHeader, 1);
                    string idOrganizacion = getMatchData(matchHeader, 2);

                    // Averiguar en que HH está el empleado: utilizar tarjetas en memoria para obtener los access levels y 
                    // con los access levels averiguar los panelsID

                    List<string> lstHH = mainApp.DataManager.ObtenerHHxNumeroTarjeta(numeroTarjeta);
                    foreach (string HH in lstHH)
                    {
                        ComunicationLayer.enviarBorrarEmpleado(numeroTarjeta.ToString(), HH);
                    }

                    // Borrar la tarjeta de la memoria
                    idTarjeta = mainApp.DataManager.obtenerIdTarjeta(numeroTarjeta).ToString();
                    mainApp.DataManager.deleteTarjFromMemory(idTarjeta);
                   

                    // Dar de baja el empleado en los HH que lo tengan
                    loguearString("ALT_DELETEBADGE. Numero Tarjeta: " + numeroTarjeta + " IdOrganizacion: " + idOrganizacion);

                }
                catch (Exception e) { loguearString("Excepcion en ALT_DELETEBADGE: " + e.Message); }
                return;
            }




            #endregion
            #region BorrarTimeZone
            matchHeader = ALT_DELETETIMEZONE.Match(texto);
            //Regex ALT_DELETETIMEZONE = new Regex(@"TYPE:ALT_DELETETIMEZONE,ORGANIZATION:(.*),TZNUM:(.*)");
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    //Extraigo del mensaje los datos necesarios.
                    int organizationId =int.Parse(getMatchData(matchHeader, 1));
                    string timezoneNum = getMatchData(matchHeader, 2);
                    //envio  TIMEZONES EN LOS HH DE LA ORGANIZACION. 
                    EnviarTimeZonesALosHH(organizationId, timezoneNum, "ALT_DELETETIMEZONE");

                }
                catch (Exception e)
                {
                    loguearString("Excepcion en ALT_DELETETIMEZONE: " + e.Message);
                   
                }
                return;
            }
            #endregion
            #region AgregarTimeZone
             matchHeader = ALT_ADDTIMEZONE.Match(texto);
             //Regex ALT_ADDTIMEZONE = new Regex(@"TYPE:ALT_ADDTIMEZONE,ORGANIZATION:(.*),TZNUM:(.*),IDTIMEZONE:(.*)");
             if (matchHeader.Success)
             {
                 try
                 {
                     loguearString("Comando reconocido: " + texto);
                     //Extraigo del mensaje los datos necesarios.
                     int organizationID = int.Parse(getMatchData(matchHeader, 1));
                     string timezoneNum = getMatchData(matchHeader, 2);
                     string timezoneID = getMatchData(matchHeader, 3);
                     //Envio a los HH de la organizacion los timezones.
                     EnviarTimeZonesALosHH(organizationID, timezoneID, "ALT_ADDTIMEZONE");
                     
                 }
                 catch (Exception e) { loguearString("Excepcion en ALT_ADDTIMEZONE: " + e.Message); }
                 return;
             }
            #endregion
            #region ModificarTimeZone
             matchHeader = ALT_MODIFYTIMEZONE.Match(texto);
             //Regex ALT_MODIFYTIMEZONE = new Regex(@"TYPE:ALT_MODIFYTIMEZONE,ORGANIZATION:(.*),TZNUM:(.*),IDTIMEZONE:(.*)");
             if (matchHeader.Success)
             {
                 try
                 {
                     loguearString("Comando reconocido: " + texto);
                     int organizationID =int.Parse(getMatchData(matchHeader, 1));
                     string timezoneNum = getMatchData(matchHeader, 2);
                     string timezoneID = getMatchData(matchHeader, 3);
                     //MODIFICAR TIMEZONES EN LOS HH DE LA ORGANIZACION.
                     EnviarTimeZonesALosHH(organizationID, timezoneID, "ALT_MODIFYTIMEZONE");

                 }
                 catch (Exception e) { loguearString("Excepcion en ALT_MODIFYTIMEZONE: " + e.Message); }
                 return;
             }
             #endregion


             matchHeader = ALT_DELETETIMEZONEINTERVAL.Match(texto);
             //Regex ALT_DELETETIMEZONEINTERVAL = new Regex(@"TYPE:ALT_DELETETIMEZONEINTERVAL,ORGANIZATION:(.*),TZNUM:(.*),IDTIMEZONE:(.*)");
             if (matchHeader.Success)
             {
                 try
                 {
                     loguearString("Comando reconocido: " + texto);
                     string organizationID = getMatchData(matchHeader, 1);
                     string timezoneNum = getMatchData(matchHeader, 2);
                     string timezoneID = getMatchData(matchHeader, 3);
                     //DELETE TIMEZONES INTERVAL DE LOS HH DE LA ORGANIZACION. 
                     loguearString("ALT_DELETETIMEZONE: ORGANIZATION " + organizationID + "TIMEZONENUM: " + timezoneNum + "TIMEZONEID" + timezoneID);

                 }
                 catch (Exception e) { loguearString("Excepcion en ALT_DELETETIMEZONEINTERVAL: " + e.Message); }
                 return;
             }
                        
            #region Agregar AccessLevelATarjeta
            //Regex ALT_ADDACCESLEVELSATARJETA = new Regex(@"TYPE:ALT_ADDACCESLEVELSATARJETA,ORGANIZATION:(.*),IDTARJETA:(.*)");
            matchHeader = ALT_ADDACCESLEVELSATARJETA.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);

                    AccessLevelLogic.LoadAccessLevels();    // Actualizamos los access levels en memoria.

                    int organizationID = int.Parse(getMatchData(matchHeader, 1));
                    string IdTarjeta = getMatchData(matchHeader, 2);
                    
                    string idAccess = getMatchData(matchHeader, 3);
                    idAccess = idAccess.Trim(',');  //Quitamos las comas del ppio y/o final

                    // Obtenemos lista lstAL access levels y le quitamos el nuevo
                    string[] aux = mainApp.DataManager.obtenerListaAccessLevels(IdTarjeta, organizationID).Split(',');
                    List<string> lstAL = new List<string>();
                    foreach (string al in aux)
                        if (!al.Equals(idAccess)) lstAL.Add(al);
                    
                    // Obtenemos lista lstNuevaHH de hh que estan vinculados con el nuevo AL
                    List<string> lstNuevaHH = mainApp.DataManager.ObtenerListaHHIdPorAccessLevel(idAccess,organizationID);

                    // Obtenemos lista de hh que estan vinculados con los access levels de lstAL
                    List<string> lstHH = new List<string>();
                    
                    foreach (string al in lstAL)
                    {
                        lstHH = mainApp.DataManager.ObtenerListaHHIdPorAccessLevel(al, organizationID);
                        foreach (string auxAL in lstHH)
                        {
                            if (lstNuevaHH.Contains(auxAL)) lstNuevaHH.Remove(auxAL);
                        }
                    }

                    // ¿Hay algun hh que esté contenido en lstNuevaHH y no esté en lstAL? Si -> enviar empleado a aquellos hh
                    foreach (string HH in lstNuevaHH)
                    {
                        Tarjeta T = mainApp.DataManager.obtenerTarjetaPorID(IdTarjeta);
                        ComunicationLayer.enviarAgregarUnEmpleado(T.numerodetarjeta, HH);
                        loguearString("ALT_ADDACCESLEVELSATARJETA enviado a " + HH + ": ORGANIZATION " + organizationID + "Tarjeta: " + T.numerodetarjeta);
                        
                    }
                }
                catch (Exception e) { loguearString("Excepcion en ALT_ADDACCESLEVELSATARJETA: " + e.Message); }
                return;
            }
#endregion
            #region BorrarAccessLevel
            //Regex ALT_DELETEACCESSLEVELSATARJETA = new Regex(@"TYPE:ALT_DELETEACCESLEVELSATARJETA,ORGANIZATION:(.*),IDTARJETA:(.*),IDACCESS(.*)");
            matchHeader = ALT_DELETEACCESSLEVELSATARJETA.Match(texto); 
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    int organizationID =int.Parse(getMatchData(matchHeader, 1));
                    string IdTarjeta = getMatchData(matchHeader, 2);
                    string IDAccessLevel = getMatchData(matchHeader, 3);
                    //MODIFICAR ACCESSLEVEL DE UNA TARJETA EN TODOS LO HH DE LA ORGANIZACION.
                    EnviarEliminarAccessLevelaTarjeta(organizationID, IdTarjeta, IDAccessLevel);
                    loguearString("ALT_DELETEACCESSLEVELSATARJETA: ORGANIZATION " + organizationID + "Tarjeta: " + IdTarjeta+"AccessLvl: "+IDAccessLevel);
                }
                catch (Exception e) { loguearString("Excepcion en ALT_DELETEACCESSLEVELSATARJETA: " + e.Message); }
                return;
            }
            #endregion


            //Ver como resolver que hacer con las notificaciones de posicion y marcas de Empleados atraves de GPS.

            //Regex ALT_GPSREPORT = new Regex(@"TYPE:GPSREPORT,DEVICEID:(.*),DATE:(.*)LAT:(.*)LONG:(.*)ORGANIZATION:(.*)");
            matchHeader = ALT_GPSREPORT.Match(texto);
            if (matchHeader.Success)
            {
                try
                {                    
                    loguearString("Comando reconocido: " + texto);
                    string Device = getMatchData(matchHeader, 1);
                    string Fecha = getMatchData(matchHeader, 2);
                    string Latitud = getMatchData(matchHeader, 3);
                    string Longitud = getMatchData(matchHeader, 4);
                    string Organizacion = getMatchData(matchHeader, 5);
                    //Enviar Reporte de posicionamiento del GPS a Lenel para Livetracking.
                    loguearString("ALT_GPSREPORT: ORGANIZATION " + Organizacion + "  Device: " + Device+"  Fecha: "+Fecha+"  Latitud  "+Latitud+"  Longitud  "+Longitud);
                }
                catch (Exception e) { loguearString("Excepcion en ALT_GPSREPORT: " + e.Message); }
                return;
            }

            //Regex ALT_GPSACCESS = new Regex(@"TYPE:GPSACCESS,DEVICEID:(.*),DATE:(.*)LAT:(.*)LONG:(.*)ORGANIZATION:(.*)IDTARJETA(.*)");
            matchHeader = ALT_GPSACCESS.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    string Device = getMatchData(matchHeader, 1);
                    string Fecha = getMatchData(matchHeader, 2);
                    string Latitud = getMatchData(matchHeader, 3);
                    string Longitud = getMatchData(matchHeader, 4);
                    string Organizacion = getMatchData(matchHeader, 5);
                    string IdTarjeta = getMatchData(matchHeader, 6);
                    //Enviar dato de acceso de una tarjeta desde un dispositivo de GPS a LENEL.
                    loguearString("ALT_GPSACCESS: ORGANIZATION " + Organizacion + "Tarjeta: " + IdTarjeta + "  Device: " + Device + "  Fecha: " + Fecha + "  Latitud  " + Latitud + "  Longitud  " + Longitud);
                }
                catch (Exception e) { loguearString("Excepcion en ALT_GPSACCESS: " + e.Message); }
                return;
            }
            // Si llega aca es porque no pudo machear ninguna expresion regular             
                loguearString("ATENCION: COMANDO NO RECONOCIDO: " + texto);
            
        }

        /// <summary>
        /// Extrae un campo de una expresion regular reconocida
        /// </summary>
        /// <param name="resultMatch"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string getMatchData(Match resultMatch, int index)
        {
            return resultMatch.Groups[index].Value;
        }

        private void loguearString(string v_texto)
        {
            if (m_LOGHandler != null)
            {
                stringEventArgs p = new stringEventArgs(v_texto, null);
                p.LOGTYPE = TiposLOG.ALUTRACK;
                this.m_LOGHandler(this, p);
            }
            else
            {
                StaticTools.loguearString(v_texto);
            }
        }
       /// <summary>
       /// Envia a los HH que estan relacionados con un accesslevels, modificaciones, agregaciones o eliminaciones que se
       /// realicen desde la WEB.
       /// </summary>
       /// <param name="Organizacion">ID de la organizacion.</param>
       /// <param name="ID">ID del Accesslevels modificado, eliminado o agregado</param>
       /// <param name="TipoMensaje">Tipo de operacion que se realizo en la WEB.(ADD, DELETE O MODIFY)</param>
       private void EnviarACCCESLEVESALosHH(int Organizacion, string IDAccessLevel, string TipoMensaje)
       {
           List<string> ListaHHConAccessLevels = mainApp.DataManager.ObtenerListaHHIdPorAccessLevel(IDAccessLevel, Organizacion);
               //Vuelvo a cargar los accesslevels desde la BD. 
               AccessLevelLogic.LoadAccessLevels();
               //Mando actualizar los AccessLevels a cada HH. 
                foreach (string HH in ListaHHConAccessLevels)
                {
                   ComunicationLayer.enviarAccessLevelsDefinitions(HH, Organizacion, "0");
                }           
           loguearString("ALT_"+TipoMensaje + ":  organizationID:" + Organizacion + " accesslevel: " + IDAccessLevel);
       }

     
       /// <summary>
       /// Envia a HH de la organizacion la nueva definicion de Accesslevels para el empleado 
       /// Envia a los HH que tengan los ACCESSLEVELS QUE TIENE LA TARJETA  *NO A TODOS LOS DE LA ORGANIZACION*
       /// </summary>
       /// <param name="Organizacion"></param>
       /// <param name="BadgeId"></param>
       private void EnviarAgregarAccessLevelaTarjeta(int Organizacion, string BadgeId)
       {
           //Listo los Accesslevels de la Tarjeta (Como viene de BD está ya con el cambio que se hizo en WEB)
           string accessLevels = mainApp.DataManager.obtenerListaAccessLevels(BadgeId, Organizacion);
           string[] listaAccessLevels = accessLevels.Split(',');
           List<string> ListaHHConAccessLevels= new List<string>();
           List<string> ListaHHConAccessLevelsFiltrada = new List<string>();
           foreach (string AccessLevelID in listaAccessLevels)//Por cada accesslevel de la tarjeta listo los HH
           {
               ListaHHConAccessLevels=mainApp.DataManager.ObtenerListaHHIdPorAccessLevel(AccessLevelID,Organizacion);
               foreach (string HH in ListaHHConAccessLevels)//Filtro los HH para evitar repeticiones.
               {
                   if (!ListaHHConAccessLevelsFiltrada.Contains(HH))
                       ListaHHConAccessLevelsFiltrada.Add(HH);//Lista con los HH(sin repetir) que tienen  los accesslevels.
               }
           }           
           //le envio el empleado a los HH que tienen el accesslevel.
           foreach (string HH in ListaHHConAccessLevelsFiltrada)
           {
               ComunicationLayer.enviarAgregarUnEmpleado(BadgeId, HH);
           }
       }

       private void EnviarEliminarAccessLevelaTarjeta(int Organizacion, string BadgeId, string IdAccLvl)
       {
           //Listo los Accesslevels de la Tarjeta (Como viene de BD está ya con el cambio que se hizo en WEB)
           string accessLevels = mainApp.DataManager.obtenerListaAccessLevels(BadgeId, Organizacion);
           string[] listaAccessLevels = accessLevels.Split(',');
           //Listo los HH que tengan los accesslevels de la tarjeta actualizados
           List<string> ListaHHConAccessLevels = new List<string>();
           List<string> ListaHHConAccessLevelsFiltrada = new List<string>();
           foreach (string AccessLevelID in listaAccessLevels)//Por cada accesslevel de la tarjeta listo los HH
           {
               ListaHHConAccessLevels = mainApp.DataManager.ObtenerListaHHIdPorAccessLevel(AccessLevelID, Organizacion);
               foreach (string HH in ListaHHConAccessLevels)//Filtro los HH para evitar repeticiones.
               {
                   if (!ListaHHConAccessLevelsFiltrada.Contains(HH))
                       ListaHHConAccessLevelsFiltrada.Add(HH);//Lista con los HH(sin repetir) que tienen  los accesslevels.
               }
           }     
           //Listo los HH que tengan el Accesslevel del parametro el accesslevel que se saco
           ListaHHConAccessLevels = mainApp.DataManager.ObtenerListaHHIdPorAccessLevel(IdAccLvl, Organizacion);
           //Si HH en lista ListaHHConAccessLevels NO ESTA EN ListaHHConAccessLevelsFiltrada
           //ENTONCES DE ESE LO MANDO BORRAR.
           foreach (string HH in ListaHHConAccessLevelsFiltrada)
           {
               foreach (string HH1 in ListaHHConAccessLevels)
               {
                   if (HH == HH1)
                   {
                       //ListaHHConAccessLevels esta en ListaHHConAccessLevelsFiltrada
                       //lo borro de la lista ListaHHConAccessLevels
                       ListaHHConAccessLevels.Remove(HH1);
                       //En este HH lo tengo que actualizar.
                       ComunicationLayer.enviarAgregarUnEmpleado(BadgeId, HH1);
                   }
               }              
               //Si queda alguna en ListaHHConAccessLevels lo Mando borrar.
           }
           if (ListaHHConAccessLevels.Count > 0)
           {
               foreach (string HH in ListaHHConAccessLevels)
               {
                   //Mando borrar el empleado de este HH.
                   ComunicationLayer.enviarBorrarEmpleado(BadgeId, HH);
               }
           }

          
       }


       /// <summary>
       /// Envia a todos los HH de la organizacion modificaciones, eliminaciones oagregacions que se hagan desde la WEB
       /// </summary>
       /// <param name="Organizacion">Organizacion</param>
       /// <param name="ID">ID del Holiday  agregado, modificado o eliminado</param>
       /// <param name="TipoMensaje">Tipo de operacion que se realizo en la WEB.(ADD, DELETE O MODIFY) </param>
       private void EnviarHolidaysALosHH(int Organizacion, string ID, string TipoMensaje)
       {
           //Refresco la carga en memoria de holidays desde la BD
           AccessLevelLogic.LoadAccessLevels();
           //Esta llamada envia a todos los HH de la organizacion la nueva carga de Acceslevels(con holidays) 
           ComunicationLayer.enviarAccessLevelsDefinitions(Organizacion, "0");

           loguearString( TipoMensaje + ":  organizationID:" + Organizacion + " HolidayId: " + ID);
       }


       /// <summary>
       /// Envia a todos los HH de la organizacion modificaciones, eliminaciones o agregacions que se hagan desde la WEB
       /// </summary>
       /// <param name="Organizacion">Organizacion</param>
       /// <param name="ID">ID del TimeZone  agregado, modificado o eliminado</param>
       /// <param name="TipoMensaje">Tipo de operacion que se realizo en la WEB.(ADD, DELETE O MODIFY) </param>
       private void EnviarTimeZonesALosHH(int Organizacion, string ID, string TipoMensaje)
       {
           //Refresco la carga en memoria de holidays desde la BD
          AccessLevelLogic.LoadAccessLevels();

           //Esta llamada envia a todos los HH de la organizacion la nueva carga de Acceslevels(con holidays) 
           ComunicationLayer.enviarAccessLevelsDefinitions(Organizacion, "0");

           loguearString(TipoMensaje + ":  organizationID:" + Organizacion + " TimeZone: " + ID);
       }
    }
}

    

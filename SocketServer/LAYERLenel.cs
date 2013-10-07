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
    public class LAYERLenel
    {


        private Dictionary<string,byte[]> repositorioBuffer;
        private Aplicacion mainApp;
        
        public TcpListener servidorHeaders;
        public TcpListener servidorDatos;
        private LAYERCommunication ComunicationLayer;
        StringEventHandler m_LOGHandler;
        public bool isListening = false;

        // Caminos a archivos de Mapas, etc
        string MapLENELCompletePath = "";
        string MapZONADEFCompletePath = "";

        
        //Expresiones regulares para el reconocimiento de mensajes
        Regex LNL_AddDevice = new Regex(@"TYPE:LNL_ADDDEVICE,DEVICEID:(.*),DEVICENAME:(.*),ORGANIZATION:(.*)");
        Regex LNL_GetAcceso = new Regex(@"TYPE:LNL_GETACCESO,DEVICEID:(.*),DEVICENAME:(.*),ORGANIZATION:(.*),SERIALNUM:(.*)");
        Regex LNL_GetEventData = new Regex(@"TYPE:LNL_GETEVENTDATA,DEVICEID:(.*),SERIALNUM:(.*)");
        Regex LNL_AddReaderToPanel = new Regex(@"TYPE:LNL_ADDREADERTOPANEL,DEVICEID:(.*),DEVICENAME:(.*),READERID:(.*),READERNAME:(.*),READERENTRANCETYPE:(.*),ORGANIZATION:(.*)");
       
        Regex LNL_AddEmployee = new Regex(@"TYPE:LNL_ADDEMPLOYEE,DEVICEID:(.*),NAME:(.*),LASTNAME:(.*),BADGE:(.*),COMPANY:(.*),SSNO:(.*),ACCESSLEVELS:(.*),IMAGE:(.*),ORGANIZATION:(.*),ISDOWNLOADINGDB:(.*),PERSONID:(.*),IDEVENT:(.*),LASTCHANGED:(.*)");


        Regex LNL_DatosEvento = new Regex(@"BADGE:(.*),NAME:(.*),SURNAME:(.*),SSNO:(.*),COMPANY:(.*),HHID:(.*),ACCESSTYPE:(.*),DATETIME:(.*),LATITUDE:(.*),LONGITUDE:(.*),IMAGENEMPLEADO:(.*),IMAGENACCESO:(.*),READERNAME:(.*)");

        Regex LNL_DelEmployee = new Regex(@"TYPE:LNL_DELEMPLOYEE,BADGE:(.*),DEVICEID:(.*),ORGANIZATION:(.*)");

        Regex LNL_GetPosition = new Regex(@"TYPE:LNL_GETPOS,DEVICEID:(.*),ORGANIZATION:(.*),INCLUDEMAP:(.*)");

        Regex LNL_AddHolidays = new Regex(@"TYPE:LNL_ADDHOLIDAYS,DEVICEID:(.*),ORGANIZATION:(.*),HOLIDAYSDATA:(.*),ISDOWNLOADINGDB:(.*)");
        Regex LNL_AddTimeZone = new Regex(@"TYPE:LNL_ADDTIMEZONE,DEVICEID:(.*),ORGANIZATION:(.*),TZNUMBER:(.*),TIMEZONEDATA:(.*),ISDOWNLOADINGDB:(.*)");

        Regex LNL_AddAccessLevel = new Regex(@"TYPE:LNL_ADDACCESSLEVEL,DEVICEID:(.*),ORGANIZATION:(.*),ACCESSLEVELID:(.*),TZREADERDATA:(.*),ISDOWNLOADINGDB:(.*)");
        Regex LNL_SendEmployeeList = new Regex(@"TYPE:LNL_SENDEMPLOYEELIST,DEVICEID:(.*),ORGANIZATION:(.*)");

        Regex HeaderFOTO = new Regex(@"IMG:(.*),IDEVENT:(.*)");

        Regex LNL_GetZoneMap = new Regex("TYPE:LNL_GETZONEMAP,DEVICEID:(.*),ORGANIZATION:(.*)");

        Regex LNL_GetOnlyZone = new Regex("TYPE:LNL_GETONLYZONE,DEVICEID:(.*),ORGANIZATION:(.*)");

        Regex LNL_AddVirtualZoneFromLenel = new Regex("TYPE:LNL_ADDZONE,DEVICEID:(.*),DEVICENAME:(.*),ORGANIZATION:(.*)");

        Regex LNL_AddVirtualGateFromLenel = new Regex(@"TYPE:LNL_ADDVIRTUALGATE,DEVICEID:(.*),DEVICENAME:(.*),READERID:(.*),READERNAME:(.*),READERENTRANCETYPE:(.*),ORGANIZATION:(.*)");

        Regex LNL_DefineZone = new Regex ("TYPE:LNL_DEFINEZONE,DEVICEID:(.*),ORGANIZATION:(.*),POINTS:(.*)");

        Regex LNL_DefineGate = new Regex ("TYPE:LNL_DEFINEGATE,DEVICEID:(.*),READERID:(.*),ORGANIZATION:(.*),ACCESSTYPE:(.*),ORD1:(.*),ORD2:(.*)");

        Regex LNL_CleanupDB = new Regex ("TYPE:LNL_CLEANUPDB,ORGANIZATION:(.*),FINISHPANELS:(.*),FIRST:(.*)");

        Regex LNL_GetConnectionStatus = new Regex("TYPE:LNL_GETCONNSTATUS,DEVICEID:(.*),ORGANIZATION:(.*)");

        /// <summary>
        /// Server general de comunicaciones con ALUTRACK. Aqui se centralizan todas las conexiones y
        /// desconexiones.
        /// </summary>
        public LAYERLenel(Aplicacion v_app,StringEventHandler v_logHandler)
        {
            mainApp = v_app;
            m_LOGHandler = v_logHandler;
            //MapZONADEFCompletePath  = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + @"\MapZONEDEF.html";

            MapZONADEFCompletePath = Path.GetDirectoryName(Application.ExecutablePath) + @"\MapZONEDEF.html";
            MapLENELCompletePath = Path.GetDirectoryName(Application.ExecutablePath) + @"\MapLENEL.html";
            ComunicationLayer = new LAYERCommunication(v_app,v_logHandler);
            repositorioBuffer = new Dictionary<string,byte[]>();
        }
       

        /// <summary>
        /// Thread lanzado por el serverComunicaciones para atender conexiones y pedidos desde LENEL.
        /// </summary>
        /// <returns></returns>
        public void startListening()
        {


            int puertoLenel1 = SystemConfiguration.OnGuardPort1;
            int puertoLenel2 = SystemConfiguration.OnGuardPort2;

            isListening = true;

            try
            {
                servidorHeaders = new TcpListener(IPAddress.Parse(StaticTools.obtenerLocalIPAddress()), puertoLenel1);//11008);

                servidorHeaders.Start();

                servidorDatos = new TcpListener(IPAddress.Parse(StaticTools.obtenerLocalIPAddress()), puertoLenel2);//11009);
                //servidorDatos = new TcpListener(IPAddress.Parse(SystemConfiguration.AlutrackIP), puertoLenel2);//11009);
                servidorDatos.Start();


                loguearString("Aceptando conexiones en el LENEL Layer");
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en LENELLayer.startListening(): " + ex.Message);
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

                    ProcesarPedidoLenel(netStream,buffer);     // Acá se hacen los send, etc y el FLUSH final

                    // Cierro la conexion.
                    cliente.Client.Shutdown(SocketShutdown.Both);
                }

                catch (Exception ex)
                {
                    if (isListening)
                    {
                        loguearString("Excepcion en LENELLayer: " + ex.Message);
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
            loguearString("Servidor Lenel detenido");
        }


        /// <summary>
        /// Procesamiento de un pedido desde LENEL. En todos los casos, el pedido es reconocido, luego se envia una
        /// respuesta o muchasy se hace FLUSH.
        /// Inmediatamente despues de esta salida se DESCONECTARA al cliente.
        /// </summary>
        /// <param name="netStream"></param>
        /// <param name="v_buffer"></param>
        private void ProcesarPedidoLenel(NetworkStream netStream, byte[] v_buffer)
        {
            Match matchHeader;
            string texto = Encoding.Default.GetString(v_buffer).Replace("\0", "");

            matchHeader = LNL_AddDevice.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);

                    string deviceID = getMatchData(matchHeader, 1);
                    string deviceName = getMatchData(matchHeader, 2);
                    string organizationID = getMatchData(matchHeader, 3);

                    loguearString("LNL_AddDevice: DeviceID: " + deviceID + " DeviceName: " + deviceName);

                    AgregarDeviceDesdeLenel(deviceID, deviceName, organizationID);

                    //string respuesta = "Agregado satisfactoriamente";
                    //respuesta = respuesta + "\n";
                    //netStream.Write(Encoding.Default.GetBytes(respuesta), 0, respuesta.Length);
                    //netStream.Flush();
                }
                catch (Exception e) { loguearString("Excepcion en LNL_AddDevice: " + e.Message); }
                return;
            }

            matchHeader = LNL_GetEventData.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    string LENELPanelID = getMatchData(matchHeader, 1);
                    string serialNum = getMatchData(matchHeader, 2);

                    loguearString("LNL_GetEventData: PanelID: " + LENELPanelID + " Serial: " + serialNum);

                    string EventData = ObtenerDatosEvento(LENELPanelID, serialNum);

                    byte[] datosFotoEmp = null;
                    byte[] datosFotoEv = null;
                    byte[] datosMapa = null;

                    bool esValido = true;

                    if (EventData != "FALSE")
                    {
                        //Expresion regular para extraer datos del EventData obtenido de la base
                        matchHeader = LNL_DatosEvento.Match(EventData);
                        if (matchHeader.Success)
                        {
                            string ImagenEmpleado = getMatchData(matchHeader, 11);
                            string ImagenAcceso = getMatchData(matchHeader, 12);
                            string Latitud = getMatchData(matchHeader, 9);
                            string Longitud = getMatchData(matchHeader, 10);
                            string Nombre = getMatchData(matchHeader, 2);

                            datosFotoEmp = obtenerFotoEmp(ImagenEmpleado);
                            datosFotoEv = obtenerFotoEv(ImagenAcceso);
                            datosMapa = obtenerMapaEv(Latitud, Longitud, "16");

                            if (Nombre == "INVALIDO")
                            {
                                esValido = false;
                            }

                            
                        }
                    }

                   enviarBulkData(Encoding.Default.GetBytes(EventData), datosFotoEmp, datosFotoEv, datosMapa,esValido);

                }
                catch (Exception e) { loguearString("Excepcion en LNL_GetEventData: " + e.Message); }
                return;
            }

            matchHeader = LNL_GetPosition.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    string LNLPanelID = getMatchData(matchHeader, 1);
                    string organizationID = getMatchData(matchHeader, 2);
                    string includeMap = getMatchData(matchHeader, 3);

                    loguearString("LNL_GetPosition:  PanelID:" + LNLPanelID + " ConMapa: " + includeMap);

                    string encodedGPSPosition = mainApp.DataManager.obtenerUltimaPosicionGPS(LNLPanelID, organizationID);

                    
                    enviarPosicionGPS(encodedGPSPosition);
                    if (encodedGPSPosition != dataManager.NO_LOCATION_DATA)     // Solo envia el mapa si envio una posicion valida...
                    {
                        if (includeMap == "T")
                        {
                            enviarMapaAcceso(@"[LAT]", @"[LONG]", "16");        // El reemplazo de las coordenadas se realiza en el LENEL
                        }
                    }
                }
                catch (Exception e) { loguearString("Excepcion en LNL_GetPosition: " + e.Message); }
                return;
            }

            matchHeader = LNL_GetAcceso.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    string deviceID = getMatchData(matchHeader, 1);
                    string deviceName = getMatchData(matchHeader, 2);
                    string organizationID = getMatchData(matchHeader, 3);
                    string serialNum = getMatchData(matchHeader, 4);

                    //loguearString("LNL_GetAcceso:  DeviceName:" + deviceName + " serial: " + serialNum);

                    int idAcceso = 0;
                    string fuente = "";
                    string accessToSend = "";

                    if (mainApp.ComunicationSystem.socketServerGeneral.isHandHeldOnLine(deviceName))
                    {
                        accessToSend = ObtenerAccesoSinSerialNum(deviceName, organizationID, serialNum, ref idAcceso, ref fuente);
                    }
                    else
                    {
                        accessToSend = "FAIL";
                    }

                   
                    if (accessToSend != "")
                    {
                        if (accessToSend != "FAIL")
                        {
                            loguearString("Asignado SERIALNUM: " + serialNum + " a acceso " + idAcceso.ToString() + " de " + deviceName + ".Enviando a LENEL");
                        }
                        ///Envia al Lenel Translator, el string con los datos para que se agregue el acceso en el AlarmMonitoring 
                        netStream.Write(Encoding.Default.GetBytes(accessToSend), 0, accessToSend.Length);
                        netStream.Flush();

                        if (esperarConfirmacion(netStream))  // Not implementado: always true
                        {
                            if (fuente == "A")
                            {
                                ActualizarSerialNumEnAccesos(idAcceso, serialNum);
                            }
                            if (fuente == "V")
                            {
                                ActualizarSerialNumEnVisitas(idAcceso, serialNum);
                            }
                        }
                    }
                    else
                    {
                        string noData = "";
                        netStream.Write(Encoding.Default.GetBytes(noData), 0, noData.Length);
                    }
                }
                catch (Exception e) { loguearString("Excepcion en LNL_GetAcceso: " + e.Message); }

                return;
            }

            matchHeader = LNL_AddReaderToPanel.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    loguearString("Comando reconocido: " + texto);
                    string deviceID = getMatchData(matchHeader, 1);
                    string deviceName = getMatchData(matchHeader, 2);
                    string readerID = getMatchData(matchHeader, 3);
                    string readerName = getMatchData(matchHeader, 4);
                    string readerEntranceType = getMatchData(matchHeader, 5);       //0: Indefinido, 1: Entrada, 2: Salida
                    string organizationID = getMatchData(matchHeader, 6);

                    loguearString("LNL_AddReaderToPanel. Panel: " + deviceID + " ReaderID: " + readerID + " Tipo: " + readerEntranceType);

                    AgregarReaderLenel(deviceID, deviceName, readerID, readerName, readerEntranceType, organizationID);
                }
                catch (Exception e) { loguearString("Excepcion en LNL_AddReaderToPanel: " + e.Message); } 
                return;
            }

            matchHeader = LNL_DelEmployee.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    string badge = getMatchData(matchHeader, 1);
                    string LNLpanelID = getMatchData(matchHeader, 2);
                    string LNLOrgID = getMatchData(matchHeader, 3);

                    loguearString("LNL_DelEmployee. Panel: " + LNLpanelID + " badge: " + badge);

                    int orgID = int.Parse(LNLOrgID);

                    // Desasocia el empleado de la tarjeta de manera de sacarlo de la lista
                    // de empleados que se enviara al HH
                    mainApp.DataManager.borrarEmpleado(badge);

                    string HHID = mainApp.DataManager.ObtenerHHID(LNLpanelID);

                    if (HHID != "")
                    {
                        // DESCOMENTAR LA SIGUIENTE LINEA PARA BORRAR EMPLEADOS EN VEZ DE MANDAR LA LISTA
                       ComunicationLayer.enviarBorrarEmpleado(badge, HHID);

                        //enviarPedidoListaEmpleados(HHID);       // Ok, enviar la nueva lista de empleados. INCLUYE el envio de AccessLevels
                        //enviarAccessLevelsDefinitions(HHID, orgID);
                    }
                }
                catch (Exception e) { loguearString("Excepcion en LNL_DelEmployee: " + e.Message); }
                return;
            }

            matchHeader = LNL_AddAccessLevel.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                   
                    int LNLpanelID = int.Parse(getMatchData(matchHeader, 1));
                    int orgID = int.Parse(getMatchData(matchHeader, 2));
                    int accessLevelID = int.Parse(getMatchData(matchHeader, 3));
                    string TZReaderData = getMatchData(matchHeader, 4);
                    string isDownloadingDB = getMatchData(matchHeader, 5);

                    loguearString("LNL_AddAccessLevel. Panel: " + LNLpanelID + " AccessLevelID:" + accessLevelID.ToString() + " ReadersTZ: " + TZReaderData);
                    
                    // Actualizar la info en el panel
                    string HHID = mainApp.DataManager.ObtenerHHID(LNLpanelID.ToString());

                    string oldAllPersonIDs = mainApp.DataManager.loadAllPersonID(HHID);

                    AccessLevelLogic.addAccessLevel(orgID, LNLpanelID, accessLevelID, TZReaderData);
                                     
                    if (HHID != "")                     // Envío final de la nueva lista de empleados y las definiciones de los accessLevels
                    {
                        if (isDownloadingDB != "1")     // Si no esta haciendo DownloadDB
                        {

                            string newAllPersonIDs = mainApp.DataManager.loadAllPersonID(HHID);
                            ComunicationLayer.enviarListaEmpleadosDiferencia(HHID, oldAllPersonIDs, newAllPersonIDs);
                            ComunicationLayer.enviarAccessLevelsDefinitions(HHID, orgID, isDownloadingDB);      // Incluye un envio del ALLPersonIDs al final                                                   
                        }
                    }
                }
                catch (Exception e) { loguearString("Excepcion en LNL_AddAccessLevel: " + e.Message); }
                return;
            }

            matchHeader = LNL_SendEmployeeList.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    int LNLpanelID = int.Parse(getMatchData(matchHeader, 1));
                    int orgID = int.Parse(getMatchData(matchHeader, 2));

                    // Actualizar la info en el panel
                    string HHID = mainApp.DataManager.ObtenerHHID(LNLpanelID.ToString());

                    if (HHID != "")                 // Evia efectivamente la lista de empleados.
                    {
                        ComunicationLayer.enviarPedidoListaEmpleados(HHID);           // Actualizo la lista de empleados y el accesslevel del HH,INCLUYE el envio de AccessLevels
                        //enviarAccessLevelsDefinitions(HHID, orgID);
                    }
                }
                catch (Exception e) { loguearString("Excepcion en LNL_SendEmployeeList: " + e.Message); }
                return;
            }
            matchHeader = LNL_AddTimeZone.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    int LNL_PanelID = int.Parse(getMatchData(matchHeader, 1));
                    int organizationID = int.Parse(getMatchData(matchHeader, 2));
                    int TZNumber = int.Parse(getMatchData(matchHeader, 3));
                    string intervalData = getMatchData(matchHeader, 4);
                    string isDownloadingDB = getMatchData(matchHeader, 5);

                    loguearString("LNL_AddTimeZone. Panel: " + LNL_PanelID + " TZNumber:" + TZNumber.ToString() + " Interval: " + intervalData);

                    //Agrego la nueva Time zone a Estructura en memoria y a la BD.
                    AccessLevelLogic.addTimezone(organizationID, TZNumber, intervalData);                   

                    // Actualizar la info en el panel
                    string HHID = mainApp.DataManager.ObtenerHHID(LNL_PanelID.ToString());

                    if (HHID != "")                 // Envío final de la nueva lista de empleados y las definiciones de los accessLevels
                    {
                        if (isDownloadingDB != "1")
                        {
                            ComunicationLayer.enviarAccessLevelsDefinitions(HHID, organizationID,isDownloadingDB);
                        }
                    }
                }
                catch (Exception e) { loguearString("Excepcion en LNL_AddTimeZone: " + e.Message); }
                return;
            }

            matchHeader = LNL_AddHolidays.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    string LNLpanelID = getMatchData(matchHeader, 1);
                    string idOrganization = getMatchData(matchHeader, 2);
                    string holidaysData = getMatchData(matchHeader, 3);
                    string isDownloadingDB = getMatchData(matchHeader, 4);

                    int orgID = int.Parse(idOrganization);

                    loguearString("LNL_AddHolidays. Panel: " + LNLpanelID + " datos:" + holidaysData);

                     AccessLevelLogic.addLenelHolidays(idOrganization, holidaysData);        // NOTA: En una sola llamada se definen TODAS las holidays para cada HH
                    // Actualizar la info en el panel
                    string HHID = mainApp.DataManager.ObtenerHHID(LNLpanelID);

                    if (HHID != "")                                      // Envío final de la nueva lista de empleados y las definiciones de los accessLevels
                    {
                        if (isDownloadingDB != "1")
                        {
                            ComunicationLayer.enviarAccessLevelsDefinitions(HHID, orgID,isDownloadingDB);
                        }
                    }
                }
                catch (Exception e) { loguearString("Excepcion en LNL_AddHolidays: " + e.Message); }
                return;
            }

            // Cambio en una badge de LENEL
            matchHeader = LNL_AddEmployee.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    int LNL_PanelID = int.Parse(getMatchData(matchHeader, 1));
                    string name = getMatchData(matchHeader, 2);
                    string lastName = getMatchData(matchHeader, 3);
                    string badge = getMatchData(matchHeader, 4);
                    string empresa = getMatchData(matchHeader, 5);
                    string SSNO = getMatchData(matchHeader, 6);
                    string accessLevels = getMatchData(matchHeader, 7);         // Lista de accessLevels asociados a la badge.
                    string tieneFoto = getMatchData(matchHeader, 8);
                    string LNLOrgID = getMatchData(matchHeader, 9);
                    string isDownloadingDB = getMatchData(matchHeader, 10);
  
                    string personID = getMatchData(matchHeader, 11);

                    string idEventoEmp = getMatchData(matchHeader, 12);
                    string lastChangedLNL = getMatchData(matchHeader, 13);

                    string lastChanged = convertirFormatoHoraLNL(lastChangedLNL);

                    string HHID = mainApp.DataManager.ObtenerHHID(LNL_PanelID.ToString());

     
                    int OrgID = int.Parse(LNLOrgID);
                    string imageFileName = "";
                    empresa = "";

                    loguearString("LNL_AddEmployee: " + name + " " + lastName + " PANELID: " + LNL_PanelID.ToString());
                    bool aumentarVersion = false;

                    if (tieneFoto == "T")                   // Recibe la foto del empleado
                    {
                        try
                        {

                            byte[] imageBytes = obtenerBufferconIDEvento(idEventoEmp, servidorDatos);

                            if (imageBytes!=null)
                            {
                                int bytesFoto = imageBytes.Length;

                                string pathImagenDB = mainApp.DataManager.cargarPathImagen(personID);
                                // Si no hay imagen en la DB y ahora llegó una imagen, aumento la version del empleado 
                                if (pathImagenDB.Equals(string.Empty)) aumentarVersion = true;// mainApp.DataManager.aumentarIdImagen(personID);//aumentarVersionEmpleado(personID);
                                else  // Si hay imagen verifico que tenga el mismo largo
                                {
                                    byte[] imgDB = obtenerFotoEmp(pathImagenDB);
                                    if (imgDB != null)
                                    {
                                        if (bytesFoto != imgDB.Length) aumentarVersion = true; // mainApp.DataManager.aumentarIdImagen(personID);//mainApp.DataManager.aumentarVersionEmpleado(personID);
                                    }
                                }
                                imageFileName = LNL_PanelID.ToString() + "_" + personID + ".jpg";

                                //imageFileName = LNL_PanelID.ToString() + "_" + badge + "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + ".jpg";
                                string imagePath = SystemConfiguration.ImagesPath;
                                File.WriteAllBytes(imagePath + @"\" + imageFileName, imageBytes);

                            }
                        }
                        catch (Exception)
                        {
                            imageFileName = "";     // Excepcion: no recibe foto.
                        }
                    }

                    int idNuevoEmpleado = mainApp.DataManager.addEmpleadoDesdeLenel(name, lastName, empresa, SSNO, @"~/Imagenes/" + imageFileName, OrgID, personID, isDownloadingDB, lastChanged);
                    mainApp.DataManager.asociarEmpleadoATarjeta(badge, idNuevoEmpleado, OrgID);

                    if (aumentarVersion)
                    {
                        mainApp.DataManager.aumentarIdImagen(personID);
                    }

                    if (!AccessLevelLogic.ListaBadgeAccessLevels.ContainsKey(OrgID))
                    {
                        LENELBadgeAccessLevels nuevoLBACC = new LENELBadgeAccessLevels(OrgID);
                        AccessLevelLogic.ListaBadgeAccessLevels.Add(OrgID, nuevoLBACC);
                    }

                    LENELBadgeAccessLevels LBACC = AccessLevelLogic.ListaBadgeAccessLevels[OrgID];

                    if (LBACC.asociarAccessLevel(badge, accessLevels))          // Si cambiaron los access levels asociados a la tarjeta
                        mainApp.DataManager.aumentarVersionEmpleado(personID);  // aumento la versión del empleado

                    // Alta en la BD
                    mainApp.DataManager.updateLenelBadgeAccessLevels(OrgID, badge, accessLevels);
              
                    if (HHID != "")                 // Envio final del add o delete Empleado al HH. Puede ser un DELETE porque Lenel manda un addEmployee al QUITAR un accesslevel de una tarjeta
                    {
                        if (isDownloadingDB != "1")             // Si esta llamada no es debida al DownloadDatabase, entonces enviar la lista al HH
                        {
                            try
                            {
                                string accesslevel = AccessLevelLogic.getAccessLevelsByBadgeInHH(badge, HHID);
                                if (accesslevel.Trim().Length > 0)
                                {
                                    ComunicationLayer.enviarAgregarUnEmpleado(badge, HHID);
                                }
                                else
                                {
                                    ComunicationLayer.enviarBorrarEmpleado(badge, HHID);
                                }
                            }
                            catch (Exception e) { loguearString("ERROR!! No se pudo agregar el empleado: " + e.Message); }

                            //enviarPedidoListaEmpleados(HHID);                           // Actualizo la lista de empleados y el accesslevel del HH, INCLUYE el envio de AccessLevels
                            //enviarAccessLevelsDefinitions(HHID, OrgID);
                        }
                    }
                   
                }
                catch (Exception e) { loguearString("Excepcion en LNL_AddEmployee: " + e.Message); }
                return;
            }

             matchHeader = LNL_GetZoneMap.Match(texto);
             if (matchHeader.Success)
             {
                 try
                 {
                     int LNL_PanelID = int.Parse(getMatchData(matchHeader, 1));
                     string idOrganization = getMatchData(matchHeader, 2);
                     loguearString("Enviando el mapa de definicion de ZONAS para el panel " + LNL_PanelID.ToString());

                     string nombreZona = mainApp.DataManager.ObtenerHHID(LNL_PanelID.ToString());
                     // Envia el HTML con el mapa que contiene el javascript de definicion de zonas y ademas envia los datos de la zona actual
                     enviarDatosZona(nombreZona,ObtenerMapaZonas(), obtenerDatosZona(LNL_PanelID, int.Parse(idOrganization)));
                 }
                 catch (Exception e) { loguearString("Excepcion en LNL_GetZoneMap: " + e.Message); } 
                 return;
             }


             matchHeader = LNL_GetOnlyZone.Match(texto);
             if (matchHeader.Success)
             {
                 try
                 {
                     int LNL_PanelID = int.Parse(getMatchData(matchHeader, 1));
                     string idOrganization = getMatchData(matchHeader, 2);
                     loguearString("Enviando datos de la zona: " + LNL_PanelID.ToString());

                     string nombreZona = mainApp.DataManager.ObtenerHHID(LNL_PanelID.ToString());
                     // Envia el HTML con el mapa que contiene el javascript de definicion de zonas y ademas envia los datos de la zona actual
                     enviarDatosSoloZona(nombreZona,obtenerDatosZona(LNL_PanelID, int.Parse(idOrganization)));
                 }
                 catch (Exception e) { loguearString("Excepcion en LNL_GetOnlyZone: " + e.Message); }
                 return;
             }



             matchHeader = LNL_DefineZone.Match(texto);
             if (matchHeader.Success)
             {
                 try
                 {
                     int LNL_PanelID = int.Parse(getMatchData(matchHeader, 1));
                     int idOrganization = int.Parse(getMatchData(matchHeader, 2));
                     string listaPuntos = getMatchData(matchHeader, 3);

                     loguearString("Actualizando definicion de zona de panelID: " + LNL_PanelID.ToString() + "con: " + listaPuntos);
                     // Le saco la ultima coma del final
                     listaPuntos = listaPuntos.Substring(0, listaPuntos.Length - 1);

                     mainApp.DataManager.DefineNewZone(LNL_PanelID, listaPuntos, idOrganization);

                 }
                 catch (Exception e) { loguearString("Excepcion en LNL_DefineZone: " + e.Message); }
                 return;
             }
             matchHeader = LNL_DefineGate.Match(texto);
             if (matchHeader.Success)
             {
                 try
                 {
                        //("TYPE:LNL_DEFINEGATE,DEVICEID:(.*),READERID:(.*),ORGANIZATION:(.*),ACCESSTYPE:(.*),ORD1:(.*),ORD2:(.*)");
                     int LNL_PanelID = int.Parse(getMatchData(matchHeader, 1));
                     int LNL_ReaderID = int.Parse(getMatchData(matchHeader, 2));
                     int idOrganization = int.Parse(getMatchData(matchHeader, 3));
                     string accessType = getMatchData(matchHeader, 4);
                     int Punto1=int.Parse(getMatchData(matchHeader, 5));
                     int Punto2= int.Parse(getMatchData(matchHeader, 6));
                     loguearString("Actualizando la Virtual Gate del LNLPanelID: " + LNL_PanelID.ToString() + ", ReaderID: " + LNL_ReaderID.ToString());

                     mainApp.DataManager.defineGate(LNL_PanelID,LNL_ReaderID,idOrganization,accessType,Punto1,Punto2);

                 }
                 catch (Exception e) { loguearString("Excepcion en LNL_DefineGate: " + e.Message); }


                 return;
             }


            matchHeader = LNL_AddVirtualZoneFromLenel.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    int LNL_PanelID = int.Parse(getMatchData(matchHeader, 1));
                    string LNL_Panelname = getMatchData(matchHeader, 2);
                    int idOrganization = int.Parse(getMatchData(matchHeader, 3));

                    loguearString("LNL_AddZONE: PanelID: " + LNL_PanelID.ToString() + " ZoneName: " + LNL_Panelname + "OrgID: " + idOrganization.ToString());

                    mainApp.DataManager.addZoneFromLenel(LNL_PanelID, LNL_Panelname, idOrganization);
                }
                catch (Exception e) { loguearString("Excepcion en LNL_AddVirtualZoneFromLenel: " + e.Message); }
                return;
            }

            //Regex(@"TYPE:LNL_ADDVIRTUALGATE,DEVICEID:(.*),DEVICENAME:(.*),READERID:(.*),READERNAME:(.*),READERENTRANCETYPE:(.*),ORGANIZATION:(.*)");
            matchHeader = LNL_AddVirtualGateFromLenel.Match(texto);
            if (matchHeader.Success)
            {
                try
                {
                    int LNL_PanelID = int.Parse(getMatchData(matchHeader, 1));
                    string LNL_Panelname = getMatchData(matchHeader, 2);            // No se usa
                    int LNL_ReaderID = int.Parse(getMatchData(matchHeader, 3));
                    string LNL_ReaderName = getMatchData(matchHeader, 4);
                    string LNL_ReaderEntranceType = getMatchData(matchHeader, 5);   // No se usa
                    int idOrganization = int.Parse(getMatchData(matchHeader, 3));

                    mainApp.DataManager.addVirtualGateFromLenel(LNL_PanelID, LNL_ReaderID, LNL_ReaderName, idOrganization);
                }
                catch (Exception e) { loguearString("Excepcion en LNL_AddVirtualGateFomLenel: " + e.Message); }
                return;
            }

            matchHeader = LNL_CleanupDB.Match(texto);
            if (matchHeader.Success)
            {
                int idOrganization = int.Parse(getMatchData(matchHeader, 1));
                string finishPanels = getMatchData(matchHeader, 2);             // Lista de panels a enviar la lista de empelados nueva.
                string first = getMatchData(matchHeader, 3);                    // Si vale 1, entonces es la primera vez que se manda desde el constructor del translator
                                                                                // SOLO entonces proceder al borrado de empleados de la base.

                if (first == "1")
                {
                    loguearString("Borrando lista de empleados no actualizados de la organizacion: " + idOrganization.ToString());
                    mainApp.DataManager.borrarListaPreviaEmpleados(idOrganization);
                }
                
                string [] idPanels = finishPanels.Split(',');
                for (int i =0 ; i<idPanels.Length;i++)
                {

                    string HHID = mainApp.DataManager.ObtenerHHID(idPanels[i]);
                    if (HHID != "")                 // Evia efectivamente la lista de empleados.
                    {
                       ComunicationLayer.enviarPedidoListaEmpleados(HHID);           // Actualizo la lista de empleados y el accesslevel del HH,INCLUYE el envio de AccessLevels
                    }

                }
       
                return;
            }

            matchHeader = LNL_GetConnectionStatus.Match(texto); // Devuelve true o False si el HH correspondiente al PANELID esta conectado
            if (matchHeader.Success)
            {

                string LNL_PanelID = getMatchData(matchHeader, 1);
                int idOrganization = int.Parse(getMatchData(matchHeader, 2));
                try
                {
                    string HHID = mainApp.DataManager.ObtenerHHID(LNL_PanelID);
                    bool res = mainApp.ComunicationSystem.socketServerGeneral.isHandHeldOnLine(HHID);

                    string resToSend = (res) ? "YES":"NO";

                    //resToSend = "YES";

                    netStream.Write(Encoding.Default.GetBytes(resToSend), 0, resToSend.Length);
                    netStream.Flush();

                    //loguearString("Connection Status de " + HHID + ": " + resToSend);
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en LNL_GetConnectionStatus: " + ex.Message);
                }
                return;
            }

            // Si llega aca es porque no pudo machear ninguna expresion regular 
            loguearString("ATENCION: COMANDO NO RECONOCIDO: " + texto);
        }

        private string convertirFormatoHoraLNL(string v_HoraLNL)
        {
            string res = "";
            Regex rHora = new Regex(@"([0-9]{4})([0-9]{2})([0-9]{2})([0-9]{2})([0-9]{2})([0-9]{2})");
            Match mHora = rHora.Match(v_HoraLNL);

            if (mHora.Success)
            {
                res = mHora.Groups[1].Value + "-" + mHora.Groups[2].Value + "-" + mHora.Groups[3].Value + " " + mHora.Groups[4].Value + ":" + mHora.Groups[5].Value + ":" + mHora.Groups[6].Value;
               
            }
            else { loguearString("No paso la expresion regular de la hora en convertirFormatoHoraLNL"); }
            return res;
        }

        /// <summary>
        /// Lee desde el TCPListener, un bloque de bytes con Header IMG:size,IDEVENT:IDEVENTO
        /// Guarda en el diccionario de repositorio IDEVENTO,Buffer
        /// Pide del repositiorio un buffer con el idEvento pasado por parametro.
        /// Si no esta, vuelve a leer. 
        /// Si esta, lo saca del repositorio y vuelve.
        /// SALIDA: Timeout, 5000 ms
        /// </summary>
        /// <param name="idEvento"></param>
        /// <param name="servidor"></param>
        /// <returns></returns>
        private byte[] obtenerBufferconIDEvento(string idEvento, TcpListener servidor)
        {
            byte[] res = null;

            byte[] headerBytes = new byte[100]; // El header del bulk
            bool sal = false;

            try
            {
                while (!sal)
                {
                    res = obtenerBufferRepositorio(idEvento);
                    if (res != null)
                    {
                        sal = true;
                    }
                    else
                    {
                        TcpClient clienteFoto = servidorDatos.AcceptTcpClient();
                        NetworkStream netStreamFoto = clienteFoto.GetStream();
                        netStreamFoto.ReadTimeout = 5000;
                        netStreamFoto.Read(headerBytes, 0, headerBytes.Length); // Lectura del header

                        string textoHeader = Encoding.Default.GetString(headerBytes).Replace("\0", "");

                        //@"IMG:(.*), IDEVENT:(.*)");
                        Match matchHeader = HeaderFOTO.Match(textoHeader);

                        if (matchHeader.Success)
                        {
                            int bytesFoto = int.Parse(getMatchData(matchHeader, 1));
                            string idEventoFoto = getMatchData(matchHeader, 2).Trim();

                            byte[] imageBytes = cargarBytes(netStreamFoto, bytesFoto);
                            agregarBufferAlRepositorio(idEventoFoto, imageBytes);

                            res = obtenerBufferRepositorio(idEvento);
                            if (res != null)
                            {
                                sal = true;
                            }
                            else
                            {
                                loguearString("ATENCION: leyo datos del evento incorrecto");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerBufferconIDEvento: " + ex.Message); 
            }
            return res;
        }

        private void agregarBufferAlRepositorio(string idEvento,byte[] buffer)
        {

            if (!repositorioBuffer.ContainsKey(idEvento))
            {
                repositorioBuffer.Add(idEvento, buffer);
            }
            else
            {
                loguearString("ATENCION: Clave duplicada en el repositorio de buffers:" + idEvento);
            }
        }


        private byte[] obtenerBufferRepositorio(string idEvento)
        {
            byte[] res = null;

             if (repositorioBuffer.ContainsKey(idEvento))
            {
                res = repositorioBuffer[idEvento];
                repositorioBuffer.Remove(idEvento);
            }
             return res;

        }

        /// <summary>
        /// Arma el string de descripcion de la zona para enviar al driver LENEL
        /// </summary>
        /// <param name="LNL_PanelID"></param>
        /// <param name="orgID"></param>
        /// <returns></returns>
        private byte[] obtenerDatosZona(int LNL_PanelID, int orgID)
        {
            string res = "";
            res = mainApp.DataManager.obtenerDatosZona(LNL_PanelID,orgID);
            return Encoding.Default.GetBytes(res);    // Devuelve el byte Array de res.
        }
        private byte[] cargarBytes(NetworkStream stream, int cant)
        {
            byte[] res = new byte[cant];

            int cantLeida = 0;
            int aux = 0;

            while (cantLeida < cant)
            {
                aux = stream.Read(res, cantLeida, cant - cantLeida);
                cantLeida += aux;
            }

            return res;
        }

        private void enviarDatosSoloZona(string nombreZona, byte[] v_datosZona)
        {
            int largoZoneDef = 0;

            if (v_datosZona != null)
            {
                largoZoneDef = v_datosZona.Length;
            }
            string header;

            header = "ZONEDEF:" + nombreZona + ",POINTS:" + largoZoneDef.ToString();

            header = header.PadRight(100);

            int totalBufferLength = header.Length + largoZoneDef;

            byte[] totalBuffer = new byte[totalBufferLength];

            Buffer.BlockCopy(Encoding.Default.GetBytes(header), 0, totalBuffer, 0, header.Length);


            if (v_datosZona != null)
            {
                Buffer.BlockCopy(v_datosZona, 0, totalBuffer, header.Length , largoZoneDef);
            }

            // Ahora si, envio...

            servidorDatos.Start();
            TcpClient cliente = servidorDatos.AcceptTcpClient();
            loguearString("Conexión aceptada para enviar BULK DATA");
            NetworkStream netStream = cliente.GetStream();
            try
            {
                netStream.WriteTimeout = 3000;
                netStream.Write(totalBuffer, 0, totalBuffer.Length);
                netStream.Flush();
                cliente.Close();

                loguearString("Enviado BULK DATA: " + totalBuffer.Length.ToString());
            }
            catch (Exception ex)
            {
                cliente.Close();

                loguearString("EXCEPCION al enviar bulk data: " + ex.Message);
            }
        }





        /// <summary>
        /// Envia el MAPA y la definicion de la ZONA de una zola vez.
        /// </summary>
        /// <param name="v_mapaZonas"></param>
        /// <param name="v_datosZona"></param>
        private void enviarDatosZona(string nombreZona, byte[] v_mapaZonas, byte[] v_datosZona)
        {
            int largoMapa = 0;
            int largoZoneDef = 0;
            if (v_mapaZonas != null)
            {
                largoMapa = v_mapaZonas.Length;
            }

                
            if (v_datosZona != null)
            {
                largoZoneDef = v_datosZona.Length;
            }
            string header;

            header = "ZONEDEF:" +nombreZona + ",MAP:"+ largoMapa.ToString() + ",POINTS:" + largoZoneDef.ToString();

            header = header.PadRight(100);

            int totalBufferLength = header.Length + largoMapa + largoZoneDef;

            byte[] totalBuffer = new byte[totalBufferLength];

            Buffer.BlockCopy(Encoding.Default.GetBytes(header), 0, totalBuffer, 0, header.Length);

            if (v_mapaZonas != null)
            {
                Buffer.BlockCopy(v_mapaZonas, 0, totalBuffer, header.Length, largoMapa);
            }

            if (v_datosZona != null)
            {
                Buffer.BlockCopy(v_datosZona, 0, totalBuffer, header.Length + largoMapa, largoZoneDef);
            }

            // Ahora si, envio...

            servidorDatos.Start();
            TcpClient cliente = servidorDatos.AcceptTcpClient();
            loguearString("Conexión aceptada para enviar BULK DATA");
            NetworkStream netStream = cliente.GetStream();
            try
            {
                netStream.WriteTimeout = 3000;
                netStream.Write(totalBuffer, 0, totalBuffer.Length);
                netStream.Flush();
                cliente.Close();

                loguearString("Enviado BULK DATA: " + totalBuffer.Length.ToString());
            }
            catch (Exception ex)
            {
                cliente.Close();

                loguearString("EXCEPCION al enviar bulk data: " + ex.Message);
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="v_eventData"></param>
        /// <param name="v_datosFotoEmp"></param>
        /// <param name="v_datosFotoEv"></param>
        /// <param name="v_datosMapa"></param>
        private void  enviarBulkData(byte [] v_eventData, byte [] v_datosFotoEmp, byte []v_datosFotoEv,byte[] v_datosMapa, bool v_esValido)
        {

            int largoEventData = 0;
            int largoFotoEmp = 0;
            int largoFotoEv = 0;
            int largoMapa = 0;
          
            if (v_eventData != null)
            {
                largoEventData = v_eventData.Length;
            }
            if (v_datosFotoEmp != null)
            {
                largoFotoEmp = v_datosFotoEmp.Length;
            }

            if (v_datosFotoEv != null)
            {
                largoFotoEv = v_datosFotoEv.Length;
            }

            if (v_datosMapa != null)
            {
                largoMapa = v_datosMapa.Length;
            }
            
            string header;

            if (v_esValido)
            {
                header = "VALIDO:" + largoEventData.ToString() + "," + largoFotoEmp.ToString() + "," + largoFotoEv.ToString() + "," + largoMapa.ToString();
            }
            else
            {
                header = "INVALIDO:" + largoEventData.ToString() + "," + largoMapa.ToString();
            }

            header = header.PadRight(100);

            int totalBufferLength = header.Length + largoEventData + largoFotoEmp + largoFotoEv + largoMapa ;

            byte[] totalBuffer = new byte[totalBufferLength];

            Buffer.BlockCopy(Encoding.Default.GetBytes(header), 0, totalBuffer, 0, header.Length);

            if (v_eventData != null)
            {
                Buffer.BlockCopy(v_eventData, 0, totalBuffer, header.Length, largoEventData);
            }

            if (v_datosFotoEmp != null)
            {
                Buffer.BlockCopy(v_datosFotoEmp, 0, totalBuffer, header.Length + largoEventData, largoFotoEmp);
            }

            if (v_datosFotoEv != null)
            {
                Buffer.BlockCopy(v_datosFotoEv, 0, totalBuffer, header.Length + largoEventData + largoFotoEmp, largoFotoEv);
            }

            if (v_datosMapa != null)
            {
                Buffer.BlockCopy(v_datosMapa, 0, totalBuffer, header.Length + largoEventData + largoFotoEmp + largoFotoEv, largoMapa);
            }
           


            // Ahora si, envio...

            servidorDatos.Start();
            TcpClient cliente = servidorDatos.AcceptTcpClient();
            loguearString("Conexión aceptada para enviar BULK DATA");
            NetworkStream netStream = cliente.GetStream();
            try
            {
                netStream.WriteTimeout = 3000;
                netStream.Write(totalBuffer, 0, totalBuffer.Length);
                netStream.Flush();
                cliente.Close();

                loguearString("Enviado BULK DATA: " + totalBuffer.Length.ToString());
            }
            catch (Exception ex)
            {
                cliente.Close();
               
                loguearString("EXCEPCION al enviar bulk data: " + ex.Message);
            }
        }


        //**********************************--------------------**************************************
        //Operacion esta ahora en LayerCommunication, si funciona ok, borrarla de aca. 
        //**********************************--------------------**************************************
        //private void enviarAgregarUnEmpleado(string tarjeta, string v_HHID)
        //{
        //    string header = "TYPE:ADDEMP,HHID:" + v_HHID + ",SIZE:" + communicationSystem.FIXED_HEADER_LENGTH + ",BADGE:" + tarjeta;            
          
        //        try
        //        {
        //            StaticTools.obtenerMutex_StateObjectClients();
        //            Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;                    
        //            bool enc = false;
        //            StateObject ClienteHH = null;
        //            foreach (KeyValuePair<string, StateObject> pair in listaClientes)
        //            {
        //                if (pair.Value.HHID == v_HHID)
        //                {
        //                    enc = true;
        //                    ClienteHH = pair.Value;
        //                    break;
        //                }
        //            }

        //            if (enc)
        //            {
        //                loguearString("VA A AGREGAR EL EMPLEADO: " + tarjeta);
        //                mainApp.ComunicationSystem.socketServerGeneral.AddJobs(header, null, ClienteHH);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            loguearString(v_HHID + " - excepcion en enviarAgrearUnEmpleado(): " + ex.Message);
        //        }
        //        finally
        //        {
        //            StaticTools.liberarMutex_StateObjectClients();
        //        }           
        //}
        //**********************************--------------------**************************************
        //Operacion esta ahora en LayerCommunication, si funciona ok, borrarla de aca. 
        //**********************************--------------------**************************************
        //private void enviarBorrarEmpleado(string tarjeta, string v_HHID)
        //{
        //    string header = "TYPE:DELEMP,HHID:" + v_HHID + ",SIZE:" + communicationSystem.FIXED_HEADER_LENGTH + ",BADGE:" + tarjeta;
           
        //    try
        //    {
        //        StaticTools.obtenerMutex_StateObjectClients();
        //        Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;
                
        //        bool enc = false;
        //        StateObject ClienteHH = null;

        //        foreach (KeyValuePair<string, StateObject> pair in listaClientes)
        //        {
        //            if (pair.Key == v_HHID)
        //            {
        //                enc = true;
        //                ClienteHH = pair.Value;
        //                break;
        //            }
        //        }

        //        if (enc)
        //        {
        //            loguearString("VA A BORRAR EL EMPLEADO: " + tarjeta);
        //            mainApp.ComunicationSystem.socketServerGeneral.AddJobs(header, null, ClienteHH);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        loguearString(v_HHID +" - excepcion en enviarBorrarEmpleado() " + ex.Message);
        //    }
        //    finally
        //    {
        //        StaticTools.liberarMutex_StateObjectClients();
        //    }
        //}        

        //**********************************--------------------**************************************
        //Operacion esta ahora en LayerCommunication, si funciona ok, borrarla de aca. 
        //**********************************--------------------**************************************

        // Encola en la lista jobs del HH el trabajos GETEMPLIST, que simula un pedido desde el HH un pedido de lista de empleados
        // por lo que el server respondera enviando la lista de empelados.


        //private void enviarPedidoListaEmpleados(string v_HHID)
        //{
        //    string header = "TYPE:GETEMPLIST,HHID:" + v_HHID + ",SIZE:512";

        //    try
        //    {
        //        StaticTools.obtenerMutex_StateObjectClients();

        //        Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

        //        bool enc = false;
        //        StateObject ClienteHH = null;

        //        foreach (KeyValuePair<string, StateObject> pair in listaClientes)
        //        {
        //            if (pair.Key == v_HHID)
        //            {
        //                enc = true;
        //                ClienteHH = pair.Value;
        //                break;
        //            }
        //        }

        //        if (enc)
        //        {
        //            loguearString("ADDJOB GETEMPLIST :" + v_HHID);
        //            mainApp.ComunicationSystem.socketServerGeneral.AddJobs(header, null, ClienteHH);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        loguearString(v_HHID +" - excepcion en enviarPedidoListaEmpleados(): " + ex.Message);
        //    }
        //    finally
        //    {
        //        StaticTools.liberarMutex_StateObjectClients();
        //    }
        //}


        //**********************************--------------------**************************************
        //Operacion esta ahora en LayerCommunication, si funciona ok, borrarla de aca. 
        //**********************************--------------------**************************************
        // Agrega el trabajo de enviar una lista de empleados especifica al HH 
        //public void enviarListaEmpleadosDiferencia(string v_HHID, string oldAllPersonIDs, string newAllPersonIDs)
        //{
        //    try
        //    {
               
        //        StaticTools.obtenerMutex_StateObjectClients();
        //        Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

        //        bool enc = false;
        //        StateObject ClienteHH = null;


        //        foreach (KeyValuePair<string, StateObject> pair in listaClientes)
        //            {
        //                if (pair.Key == v_HHID)
        //                {
        //                    enc = true;
        //                    ClienteHH = pair.Value;
        //                    break;
        //                }
        //            }

        //            if (enc)
        //            {
        //                string personIDsDiff = restarConjuntoPersonIDs(newAllPersonIDs, oldAllPersonIDs);  // le saca a la nueva todos los de la vieja
        //                // Hay algo nuevo que mandar?
        //                if (personIDsDiff.Trim().Length > 0)
        //                {
        //                    mainApp.ComunicationSystem.agregarTrabajoListaEmpleadosEspecifica(ClienteHH, personIDsDiff);
        //                }
        //            }
               

        //    }
        //    catch (Exception ex)
        //    {

        //        loguearString("Excepcion en enviarListaEmpleadosDiferencia: " + ex.Message);
        //    }
        //    finally
        //    {
        //        StaticTools.liberarMutex_StateObjectClients();
        //    }

        //}
        //**********************************--------------------**************************************
        //Operacion esta ahora en LayerCommunication, si funciona ok, borrarla de aca. 
        //**********************************--------------------**************************************

        // devuelve un string con todos los del minuendo que NO estan en el sustraendo
        //private string restarConjuntoPersonIDs(string v_minuendo, string v_sustraendo)
        //{
        //    string res = "";
        //    string[] minuendos = v_minuendo.Split(',');
        //    string sustraendos=","+v_sustraendo+",";        // Me aseguro que comience y  termine en , para que el contains pueda encontrar el primero y el ultimo


        //    foreach (string s in minuendos)
        //    {
        //        if (s.Trim().Length > 0)
        //        {
        //            if (!sustraendos.Contains("," + s + ","))
        //            {
        //                res = res + s + ",";
        //            }
        //        }
        //    }

        //    return res;

        //}

        //**********************************--------------------**************************************
        //Operacion esta ahora en LayerCommunication, si funciona ok, borrarla de aca. 
        //**********************************--------------------**************************************

        ///// <summary>
        ///// Envia, en una sola llamada, todas las definiciones de los accesslevels asociados al panel HHID
        ///// </summary>
        ///// <param name="v_HHID"></param>
        //public void enviarAccessLevelsDefinitions(string v_HHID, int organizationID, string isDownloadingDB)
        //{

            
        //    try
        //    {
        //        StaticTools.obtenerMutex_StateObjectClients();
        //        Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

        //        bool enc = false;
        //        StateObject ClienteHH = null;
        //        foreach (KeyValuePair<string, StateObject> pair in listaClientes)
        //        {
        //            if (pair.Key == v_HHID)
        //            {
        //                enc = true;
        //                ClienteHH = pair.Value;
        //                break;
        //            }
        //        }

        //        if (enc)
        //        {
        //            //  int organizationID = mainApp.DataManager.obtenerOrganizationIDFromHHID(v_HHID);

        //            string v_accessLevelsIDs = obtenerAccessLevelsFromPanel(v_HHID, organizationID);

        //            loguearString("Envio AccessLevels al HandHeld:" + v_HHID);

        //            string[] idACCs = v_accessLevelsIDs.Split(',');
        //            string LNLPanelIDstr = mainApp.DataManager.ObtenerLenelPanelID(v_HHID);


        //            string totalAccessLevelInfo = "";
        //            if ((LNLPanelIDstr != "") && organizationID > 0)
        //            {
        //                int LNLPanelID = int.Parse(LNLPanelIDstr);

        //                foreach (string ACCID in idACCs)
        //                {
        //                    if (ACCID.Trim().Length > 0)
        //                    {
        //                        string accessLevelinfo = getAccessLevelDefinition(organizationID, LNLPanelID, int.Parse(ACCID));
        //                        if (accessLevelinfo != "")
        //                        {
        //                            totalAccessLevelInfo = totalAccessLevelInfo + accessLevelinfo + "+";

        //                            //mainApp.ComunicationSystem.agregarTrabajoAccessLevel(ClienteHH, accessLevelinfo);       // Encola el trabajo para enviar el accessLevel
        //                        }
        //                    }
        //                }

        //                if (totalAccessLevelInfo.Length > 0)
        //                {
        //                    totalAccessLevelInfo = totalAccessLevelInfo.Substring(0, totalAccessLevelInfo.Length - 1);      // Saca el ultimo |
        //                }
        //                // Encola el trabajo para enviar todos los accessLevels juntos.
        //                mainApp.ComunicationSystem.agregarTrabajoAccessLevel(ClienteHH, totalAccessLevelInfo);

        //                // Si el envio de accesslevels se genero por un cambio manual desde Lenel, se envian los PersonIDs del HH para borrar los que no deban figurar mas.
        //                if (isDownloadingDB != "1")
        //                {
        //                    mainApp.ComunicationSystem.enviarAllPersonIDJob(ClienteHH.HHID, ClienteHH);
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        loguearString(v_HHID + " - excepcion en enviarAccessLevelsDefinitions(): " + ex.Message);
        //    }
        //    finally
        //    {
        //        StaticTools.liberarMutex_StateObjectClients();
        //    }
        //}



        //private string obtenerAccessLevelsFromPanel(string v_HHID,int v_orgID)
        //{
        //    string res = "";
        //    string LNLPanelIDstr = mainApp.DataManager.ObtenerLenelPanelID(v_HHID);

        //    if (LNLPanelIDstr != "")
        //    {
        //        int LNLPanelID = int.Parse(LNLPanelIDstr);

        //        LENELAccessLevels LNACC = ListaAccessLevels[v_orgID];

        //        res = LNACC.obtenerAccessLevelsStringFromPanel(LNLPanelID);
        //    }

        //    return res;
        //}

        ///// <summary>
        ///// Devuelve un string con la definicion del accessLevel indicado en v_ACCID
        ///// </summary>
        ///// <param name="v_HHID"></param>
        ///// <param name="v_ACCID"></param>
        ///// <returns></returns>
        //private string getAccessLevelDefinition(int v_organizationID, int v_LNLPanelID, int v_ACCID)
        //{
        //    string strAccess = "";

        //    LENELAccessLevels LNACC = ListaAccessLevels[v_organizationID];

        //    AccessLevelData accessLevel = LNACC.getAccessLevel(v_LNLPanelID);

        //    if (accessLevel!= null)
        //    {
        //        if (accessLevel.ReaderTZ.ContainsKey(v_ACCID))
        //        {
        //            Dictionary<int,int> ReaderTZList = accessLevel.ReaderTZ[v_ACCID];

        //            int entrada = mainApp.DataManager.ObtenerEntranceReaderID(v_LNLPanelID);      // OJO: Zero based ReaderIDs
        //            int TZEntrada = -1;
        //            int salida = mainApp.DataManager.ObtenerExitReaderID(v_LNLPanelID);        // OJO: Zero based ReaderIDs
        //            int TZSalida = -1;

        //            foreach(KeyValuePair<int,int> pair in ReaderTZList)
        //            {
        //                if (pair.Key == entrada)
        //                {
        //                    TZEntrada = pair.Value;
        //                }
        //                if (pair.Key == salida)
        //                {
        //                    TZSalida = pair.Value;
        //                }
        //            }

        //            if ( (TZSalida == TZEntrada) && (TZSalida >0))
        //            {
        //                strAccess = getStrAccess(v_organizationID,v_ACCID, v_LNLPanelID, TZEntrada, true, true);
        //            }
        //            else
        //            {
        //                if (TZEntrada >= 0)
        //                {
        //                    strAccess = getStrAccess(v_organizationID,v_ACCID, v_LNLPanelID, TZEntrada, true, false);      // Pone el bit de entrada en true
        //                }
        //                if (TZSalida >= 0)
        //                {
        //                    strAccess = strAccess + getStrAccess(v_organizationID,v_ACCID, v_LNLPanelID, TZSalida, false, true);    // Pone el bit de salida en true
        //                }
        //            }
        //        }
        //        else
        //        {
        //            loguearString("AccessLevel: " + v_ACCID.ToString() + " no definido para el panelID: " + v_LNLPanelID.ToString());
        //        }
        //    }
        //    else
        //    {
        //        loguearString("Pedido de un accessLevel no definido para el panelID: " + v_LNLPanelID.ToString());
        //    }
        //    return strAccess;
        //}

       

        /// <summary>
        /// Envia la posicion GPS encodeada en datosAEnviar como:
        /// LATITUDE:xxx,LONGITUDE:xxx,DATETIME:xxx,HHID:xxx
        /// </summary>
        /// <param name="datosAEnviar"></param>
        private void enviarPosicionGPS(string datosAEnviar)
        {

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            byte[] arrayBytesComando = encoding.GetBytes(datosAEnviar);

            TcpClient cliente = servidorDatos.AcceptTcpClient();
            loguearString("Conexión aceptada para enviar la Posicion GPS");
            NetworkStream netStream = cliente.GetStream();

            netStream.Write(arrayBytesComando, 0, arrayBytesComando.Length);
            netStream.Flush();
            cliente.Close();

            loguearString("Enviada la posicion GPS");
        }

        private byte[] obtenerFotoEmp(string v_imagePath)
        {
            byte[] dataBytesFoto = null;

            string fileName = Path.GetFileName(v_imagePath);
            string imagePath = SystemConfiguration.ImagesPath;
            try
            {
                dataBytesFoto = File.ReadAllBytes(imagePath + @"/" + fileName);
            }
            catch (Exception)
            {
                loguearString("FOTO Del empleado no encontrada");
                dataBytesFoto = null;
            }

            return dataBytesFoto;
        }



        ///// <summary>
        ///// Envia al LENEL la foto del empleado especificada en v_imagePath
        ///// </summary>
        ///// <param name="v_imagePath"></param>
        //private void enviarFotoEmpleado(string v_imagePath)
        //{
        //    byte[] dataBytesFoto = null;

        //    string fileName = Path.GetFileName(v_imagePath);
        //    string imagePath = SystemConfiguration.ImagesPath;
        //    try
        //    {
        //        dataBytesFoto = File.ReadAllBytes(imagePath + @"/" + fileName);
        //    }
        //    catch (Exception)
        //    {
        //        loguearString("FOTO NO ENCONTRADA. Enviando 1 byte");
        //        dataBytesFoto = new byte[1];        // Manda 1 byte si no puede encontrar la foto ( o no tiene)
        //    }

           
        //    servidorDatos.Start();
        //    TcpClient cliente = servidorDatos.AcceptTcpClient();
        //    loguearString("Conexión aceptada para enviar Foto Empleado");
        //    NetworkStream netStream = cliente.GetStream();
        //    try
        //    {
        //        netStream.WriteTimeout = 3000;
        //        netStream.Write(dataBytesFoto, 0, dataBytesFoto.Length);
        //        netStream.Flush();
        //        cliente.Close();
        //        servidorHeaders.Stop();

        //        loguearString("Enviada foto del empleado");
        //    }
        //    catch (Exception ex)
        //    {
        //        cliente.Close();
        //        servidorHeaders.Stop();
        //        loguearString("EXCEPCION al enviar foto del empleado: " + ex.Message);
        //    }
        //}


        private byte[] obtenerFotoEv(string v_imagePath)
        {
            byte[] dataBytesFoto = null;

            string fileName = Path.GetFileName(v_imagePath);
            string imagePath = SystemConfiguration.ImagesPath;
            try
            {
                dataBytesFoto = File.ReadAllBytes(imagePath + @"/" + fileName);
            }
            catch (Exception)
            {
                dataBytesFoto = null;
            }

            return dataBytesFoto;

        }


        //private void enviarFotoEvento(string v_imagePath)
        //{
        //    byte[] dataBytesFoto = null;

        //    string fileName = Path.GetFileName(v_imagePath);
        //    string imagePath = SystemConfiguration.ImagesPath;
        //    try
        //    {
        //        dataBytesFoto = File.ReadAllBytes(imagePath + @"/" + fileName);
        //    }
        //    catch (Exception)
        //    {
        //        dataBytesFoto = new byte[1];        // Manda 1 byte si no puede encontrar la foto ( o no tiene)
        //    }

            
        //    servidorHeaders.Start();
        //    TcpClient cliente = servidorHeaders.AcceptTcpClient();
        //    loguearString("Conexión aceptada para enviar Foto Acceso");
        //    NetworkStream netStream = cliente.GetStream();
        //    try
        //        {
        //        netStream.WriteTimeout = 3000;
        //        netStream.Write(dataBytesFoto, 0, dataBytesFoto.Length);
        //        netStream.Flush();
        //        cliente.Close();
        //        servidorHeaders.Stop();

        //        loguearString("Enviada foto del acceso");
        //    }
        //    catch (Exception ex)
        //    {
        //        cliente.Close();
        //        servidorHeaders.Stop();
        //    }
        //}


        /// <summary>
        /// Obtiene el mapa. Es null si no lo encuentra o si no hay datos validos de lat y long
        /// </summary>
        /// <param name="latitud"></param>
        /// <param name="longitud"></param>
        /// <param name="zoom"></param>
        /// <returns></returns>
        private byte[] obtenerMapaEv(string latitud, string longitud, string zoom)
        {
            byte[] dataBytesMapa = null;

            if ((latitud != "") && (longitud != ""))
            {
                string fileName = MapLENELCompletePath; 

                try
                {
                    StreamReader streamReader = new StreamReader(@fileName);
                    string mapText = streamReader.ReadToEnd();
                    streamReader.Close();

                    mapText = mapText.Replace(@"[ZOOM]", zoom);
                    mapText = mapText.Replace(@"[LAT]", latitud);
                    mapText = mapText.Replace(@"[LONG]", longitud);

                    System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                    dataBytesMapa = encoding.GetBytes(mapText);

                }
                catch (Exception)
                {
                    dataBytesMapa = null;
                    loguearString("Error al tratar de cargar el mapa: " + fileName);
                }
            }
            return dataBytesMapa;
        }

        /// <summary>
        /// Carga el buffer con el archivo de mapa que permite definir zonas.
        /// </summary>
        /// <returns></returns>
        private byte[] ObtenerMapaZonas()
        {
            byte[] dataBytesMapa = null;
            string fileName = MapZONADEFCompletePath;

            try
            {
                StreamReader streamReader = new StreamReader(@fileName);
                string mapText = streamReader.ReadToEnd();
                streamReader.Close();
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                dataBytesMapa = encoding.GetBytes(mapText);

            }
            catch (Exception)
            {
                dataBytesMapa = new byte[1];        // Devuelve 1 byte si no puede cargar el mapa.
                loguearString("Error al tratar de cargar el mapa: " + fileName);
            }
          
            return dataBytesMapa;

        }

   

        private void enviarMapaAcceso(string latitud, string longitud, string zoom)
        {
        
            byte[] dataBytesMapa = null;

            //string fileName = @"c:\Temp\MapLENEL.html";

            string fileName = MapLENELCompletePath;
            try
            {
                StreamReader lectorMapa = new StreamReader(fileName);
                string mapText = lectorMapa.ReadToEnd();
                lectorMapa.Close();

                mapText = mapText.Replace(@"[ZOOM]", zoom);
                mapText = mapText.Replace(@"[LAT]", latitud);
                mapText = mapText.Replace(@"[LONG]", longitud);

                System.Text.ASCIIEncoding  encoding=new System.Text.ASCIIEncoding();
                dataBytesMapa = encoding.GetBytes(mapText);

            }
            catch (Exception)
            {
                dataBytesMapa = new byte[1];        // Manda 1 byte si no puede encontrar el mapa
                loguearString("Error al tratar de cargar el mapa desde: "+ fileName);
            }

            servidorDatos.Start();
            TcpClient cliente = servidorDatos.AcceptTcpClient();

            loguearString("Conexión aceptada para enviar el MAPA del acceso");
            NetworkStream netStream = cliente.GetStream();
            netStream.WriteTimeout = 3000;
            try
            {
                netStream.Write(dataBytesMapa, 0, dataBytesMapa.Length);
                netStream.Flush();
                cliente.Close();
                loguearString("Enviado el mapa del acceso de tamaño: " + dataBytesMapa.Length.ToString());
            }
            catch (Exception)
            {
                loguearString("Excepcion enviando el mapa LENEL");
            }
        }

        /// <summary>
        /// Espera TRUE desde el stream por 3 segundos, sino devuelve FALSE
        /// </summary>
        /// <param name="v_netStream"></param>
        /// <returns></returns>
        private bool esperarConfirmacion(NetworkStream v_netStream)
        {
            //bool res = false;
            //loguearString("Esperando Confirmacion");
            //try
            //{

            //    byte[] buffer = new byte[1024];
            //    v_netStream.ReadTimeout = 3000;
            //    v_netStream.Read(buffer, 0, buffer.Length);

            //    string texto = Encoding.Default.GetString(buffer).Replace("\0", "");

            //    loguearString("Confirmacion recibida: " + texto);
            //    res = (texto == "TRUE");

            //}
            //catch (Exception)
            //{
            //    res = false;
            //}
            bool res = true;

            return res;
        }

        /// <summary>
        /// Agraga un device a la base de datos de ALUTRACK. 
        /// Se da de alta en la base de datos de ALUTRACK de manera que soporte tracking GPS.
        /// </summary>
        /// <param name="v_deviceID"></param>
        /// <param name="v_deviceName"></param>
        /// <param name="v_organizationName"></param>
        private void AgregarDeviceDesdeLenel(string v_deviceID, string v_deviceName, string v_organizationName)
        {
            mainApp.DataManager.agregarDeviceDesdeLENEL(v_deviceID, v_deviceName, v_organizationName);
        }

        /// <summary>
        /// Devuelve el string de acceso y el id del acceso para asociarlo al serialnum pasado desde Lenel
        /// Primero busca en la tabla de accesos y luego en la de visitas.
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="organizationID"></param>
        /// <param name="serialNum"></param>
        /// <param name="idAcceso"></param>
        /// <returns></returns>
        private string ObtenerAccesoSinSerialNum(string deviceName,string organizationID,string serialNum, ref int idAcceso, ref string fuente)
        {
            string res = "";

            fuente = "A";               // Fuente de la tabla Accesos
            
            res = mainApp.DataManager.obtenerAccesoSinSerialNum(deviceName, ref idAcceso);


           // loguearString("ObtenerAccesoSinSerialNum. DeviceID=" + deviceName + " Organizacion: " + organizationID + " SerialNum: " + serialNum);

            if (res == "")
            {
                fuente = "V";           // Fuente de la tabla de visitas
                res = mainApp.DataManager.obtenerVisitaSinSerialNum(deviceName, ref idAcceso);
            }
            return res;
        }

        /// <summary>
        /// El usuario ha hecho click con el boton derecho sobre un evento de LENEL. 
        /// Viene el serial Number del evento y el device ID.
        /// ObtenerDatosEvento hace una consulta a la base de ALUTRACK para ver si existen datos de ese evento
        /// y mandar, de existir, los datos al LENEL.
        /// Si no existen datos, se envia la palabra FALSE, que hará que LENEL no se quede esperando Fotos, mapas, etc.
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="serialNum"></param>
        /// <returns></returns>
        private string ObtenerDatosEvento(string LenelPanelID, string serialNum)
        {
            string res = "";

            loguearString("ObtenerDatosEvento. DeviceID=" + LenelPanelID + " SerialNum: " + serialNum);

            res = mainApp.DataManager.obtenerDatosEventoDesdeAccesos(LenelPanelID, serialNum);

            if (res == "FALSE")
            {
                res = mainApp.DataManager.obtenerDatosEventoDesdeVisitas(LenelPanelID, serialNum);
            }

            return res;
        }

        public void AgregarReaderLenel(string deviceID, string deviceName, string readerID, string readerName, string readerEntranceType, string organizationID)
        {
            loguearString("ActualizarReaderIDLenel para el panel: " + deviceName + "con ID: " + deviceID + "readerID: " + readerID + "es de EntranceType: " + readerEntranceType);
            mainApp.DataManager.agregarReaderDesdeLenel(deviceID, deviceName, readerID, readerName, readerEntranceType, organizationID);
        }

        public void ActualizarSerialNumEnAccesos(int idAcceso, string serialNum)
        {
            loguearString("ActualizarSerialNumEnAccesos. idAcceso: " + idAcceso.ToString() + " SerialNum: " + serialNum);
            mainApp.DataManager.actualizarSerialNumEnAccesos(idAcceso, serialNum);
        }

        public void ActualizarSerialNumEnVisitas(int idAcceso, string serialNum)
        {
            loguearString("ActualizarSerialNumEnVisitas. idAcceso: " + idAcceso.ToString() + " SerialNum: " + serialNum);
            mainApp.DataManager.actualizarSerialNumEnVisitas(idAcceso, serialNum);
        }


        ///// <summary>
        ///// Devuelve la lista de AccessLevelsID separada por coma. Esto se envia con cada empleado.
        ///// </summary>
        ///// <param name="v_badge"></param>
        ///// <returns></returns>
        //public string getAccessLevelsByBadge(string v_badge)
        //{
        //    string res = "";
        //    if (LENELBadgeAccessLevels.ContainsKey(v_badge))
        //    {
        //        res = LENELBadgeAccessLevels[v_badge];
        //    }
        //    return res;
        //}



      

    

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
                p.LOGTYPE = TiposLOG.LENEL;
                this.m_LOGHandler(this, p);
            }
            else
            {
                StaticTools.loguearString(v_texto);
            }
        }
    }
}

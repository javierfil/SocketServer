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
    //Esta Clase contiene todas las operaciones de Envio y distribucion de mensajes hacia los HandHeld. 
    public class LAYERCommunication
    {

        // Estructuras de datos para los AccessLevels
        // La organizacion a la que pertenece esta info viene como parámetro externo a cada llamada desde LENEL.
        // Coleccion de LENELBadgeAccessLevels indexada por idOrganizacion
        private Aplicacion mainApp;
        StringEventHandler m_LOGHandler;

        public LAYERCommunication(Aplicacion v_app, StringEventHandler v_logHandler)
        {
            mainApp = v_app;
            m_LOGHandler = v_logHandler;

        }
        /// <summary>
        /// Devuelve true si el HH esta conectado al ALUTRACKLAYER
        /// NOTA: si tiene info sobre sus accesslevels
        /// </summary>
        /// <param name="v_HHID"></param>
        /// <returns></returns>
        public bool isPannelConnected(string v_HHID)
        {
            bool res = false;

            try
            {
                string panelID = mainApp.DataManager.ObtenerLenelPanelID(v_HHID);
                int orgID = mainApp.DataManager.obtenerOrganizationIDFromHHID(v_HHID);

                if ((panelID.Length > 0) && (orgID > 0))
                {
                    LENELAccessLevels LNACC = AccessLevelLogic.ListaAccessLevels[orgID];
                    int LNLPanelID = int.Parse(panelID);
                    AccessLevelData accessLevel = LNACC.getAccessLevel(LNLPanelID);
                    if (accessLevel != null)
                    {
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en isPannelConnected: " + ex.Message);
            }

            return res;
        }


        ///// <summary>
        ///// Envia en una sola llamada borrar un empleado a todos los HH de la Organizacion
        ///// </summary>
        ///// <param name="tarjeta"></param>
        ///// <param name="v_HHID"></param>
        //public void enviarBorrarEmpleado(string tarjeta, int OrgID)
        //{
        //    try
        //    {
        //        StaticTools.obtenerMutex_StateObjectClients();
        //        Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;
                                
        //        List<StateObject> ClienteHH = new List<StateObject>() ;

        //        //Obtengo la lista de HH de la org.
        //        foreach (KeyValuePair<string, StateObject> pair in listaClientes)
        //        {
        //            if (pair.Value.orgID == OrgID)
        //            {
                        
        //                ClienteHH.Add(pair.Value);
        //                break;
        //            }
        //        }
        //        //Envio borrar el Empleado a todos los HH de la org.
        //        foreach (StateObject StObj in ClienteHH)
        //        {
        //            string header = "TYPE:DELEMP,HHID:" + StObj.HHID + ",SIZE:" + communicationSystem.FIXED_HEADER_LENGTH + ",BADGE:" + tarjeta;
        //            loguearString("VA A BORRAR EL EMPLEADO: " + tarjeta);
        //            try
        //            {
        //                mainApp.ComunicationSystem.socketServerGeneral.AddJobs(header, null, StObj);
        //            }
        //            catch (Exception ex) { loguearString(StObj.HHID + " - excepcion en enviarBorrarEmpleado() " + ex.Message); }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        loguearString(tarjeta+ " - excepcion en enviarBorrarEmpleado() " + ex.Message);
        //    }
        //    finally
        //    {
        //        StaticTools.liberarMutex_StateObjectClients();
        //    }

        //}

        /// <summary>
        /// Envia en una sola llamada borrar un empleado a al HHID pasado como parametro.
        /// </summary>
        /// <param name="tarjeta"></param>
        /// <param name="v_HHID"></param>
        public void enviarBorrarEmpleado(string tarjeta, string v_HHID)
        {
            string header = "TYPE:DELEMP,HHID:" + v_HHID + ",SIZE:" + communicationSystem.FIXED_HEADER_LENGTH + ",BADGE:" + tarjeta;
            try
            {
               
                StaticTools.obtenerMutex_StateObjectClients();
                Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

                bool enc = false;
                StateObject ClienteHH = null;

                foreach (KeyValuePair<string, StateObject> pair in listaClientes)
                {
                    if (pair.Value.HHID == v_HHID)
                    {
                        enc = true;
                        ClienteHH = pair.Value;
                        break;
                    }
                }

                if (enc)
                {
                    loguearString("VA A BORRAR EL EMPLEADO: " + tarjeta);
                    mainApp.ComunicationSystem.socketServerGeneral.AddJobs(header, null, ClienteHH);
                }
            }
            catch (Exception ex)
            {
                loguearString(v_HHID + " - excepcion en enviarBorrarEmpleado() " + ex.Message);
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }

        }
        /// <summary>
        /// Envia en una sola llamada un empleado a todos los HH de la organizacion.
        /// </summary>
        /// <param name="tarjeta"></param>
        /// <param name="OrgId"></param>
        public void enviarAgregarUnEmpleado(string tarjeta, int OrgId)
        {
            try
            {
                StaticTools.obtenerMutex_StateObjectClients();
                Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;                
                List<StateObject> ClienteHH = null;
                //Obtengo en ClienteHH la lista de Handhelds de la organizacion
                foreach (KeyValuePair<string, StateObject> pair in listaClientes)
                {
                    if (pair.Value.orgID == OrgId)
                    {                        
                        ClienteHH.Add(pair.Value);                       
                    }
                }
                //Recorro los HH de la organizacion y a cada uno le envio el empleado. 
                if (ClienteHH.Count>0)
                {
                    foreach (StateObject stobj in ClienteHH)
                    {
                        string header = "TYPE:ADDEMP,HHID:" + stobj.HHID + ",SIZE:" + communicationSystem.FIXED_HEADER_LENGTH + ",BADGE:" + tarjeta;
                        loguearString("VA A AGREGAR EL EMPLEADO: " + tarjeta);
                        try
                        {
                            mainApp.ComunicationSystem.socketServerGeneral.AddJobs(header, null, stobj);
                        }
                        catch(Exception ex)
                        {
                            loguearString(stobj.HHID + " - excepcion en enviarAgrearUnEmpleado(): " + ex.Message);
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
                loguearString( tarjeta+ " - excepcion en enviarAgrearUnEmpleado(): " + ex.Message);
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }
        }

        public void enviarAgregarUnEmpleado(string tarjeta, string v_HHID)
        {
            if (mainApp.ComunicationSystem.socketServerGeneral.isHandHeldOnLine(v_HHID))
            {
                StateObject cliente = mainApp.ComunicationSystem.socketServerGeneral.getCliente(v_HHID);

                if (cliente != null)
                {
                    stringEventArgs e = new stringEventArgs("", cliente);
                    e.textData.Add("HHID", v_HHID);
                    e.textData.Add("TARJETA", tarjeta);

                    mainApp.ComunicationSystem.agregarEmp(this, e);
                }
            }
            else loguearString("enviarAgregarUnEmpleado(): " + v_HHID + " no está online");
           
            //string header = "TYPE:ADDEMP,HHID:" + v_HHID + ",SIZE:" + communicationSystem.FIXED_HEADER_LENGTH + ",BADGE:" + tarjeta;

            //try
            //{
            //    StaticTools.obtenerMutex_StateObjectClients();
            //    Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;
            //    bool enc = false;
            //    StateObject ClienteHH = null;
            //    foreach (KeyValuePair<string, StateObject> pair in listaClientes)
            //    {
            //        if (pair.Value.HHID == v_HHID)
            //        {
            //            enc = true;
            //            ClienteHH = pair.Value;
            //            break;
            //        }
            //    }

            //    if (enc)
            //    {
            //        loguearString("VA A AGREGAR EL EMPLEADO: " + tarjeta);
            //        mainApp.ComunicationSystem.socketServerGeneral.AddJobs(header, null, ClienteHH);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    loguearString(v_HHID + " - excepcion en enviarAgrearUnEmpleado(): " + ex.Message);
            //}
            //finally
            //{
            //    StaticTools.liberarMutex_StateObjectClients();
            //}
        }
        public void enviarPedidoListaEmpleados(string v_HHID)
        {
            string header = "TYPE:GETEMPLIST,HHID:" + v_HHID + ",SIZE:512";

            try
            {

                StaticTools.obtenerMutex_StateObjectClients();

                Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

                bool enc = false;
                StateObject ClienteHH = null;

                foreach (KeyValuePair<string, StateObject> pair in listaClientes)
                {
                    if (pair.Key == v_HHID)
                    {
                        enc = true;
                        ClienteHH = pair.Value;
                        break;
                    }
                }

                if (enc)
                {
                    loguearString("ADDJOB GETEMPLIST :" + v_HHID);
                    mainApp.ComunicationSystem.socketServerGeneral.AddJobs(header, null, ClienteHH);
                }
            }
            catch (Exception ex)
            {
                loguearString(v_HHID + " - excepcion en enviarPedidoListaEmpleados(): " + ex.Message);
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }
        }
        public void enviarListaEmpleadosDiferencia(string v_HHID, string oldAllPersonIDs, string newAllPersonIDs)
        {
            try
            {
                StaticTools.obtenerMutex_StateObjectClients();
                Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

                bool enc = false;
                StateObject ClienteHH = null;

                foreach (KeyValuePair<string, StateObject> pair in listaClientes)
                {
                    if (pair.Key == v_HHID)
                    {
                        enc = true;
                        ClienteHH = pair.Value;
                        break;
                    }
                }

                if (enc)
                {
                    string personIDsDiff = restarConjuntoPersonIDs(newAllPersonIDs, oldAllPersonIDs);  // le saca a la nueva todos los de la vieja
                    // Hay algo nuevo que mandar?
                    if (personIDsDiff.Trim().Length > 0)
                    {
                        mainApp.ComunicationSystem.agregarTrabajoListaEmpleadosEspecifica(ClienteHH, personIDsDiff);
                    }
                }


            }
            catch (Exception ex)
            {

                loguearString("Excepcion en enviarListaEmpleadosDiferencia: " + ex.Message);
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }

        }


        /// <summary>
        /// Metodo auxiliar a enviarListaEmpleadosDiferencia
        /// </summary>
        /// <param name="v_minuendo"></param>
        /// <param name="v_sustraendo"></param>
        /// <returns></returns>
        private string restarConjuntoPersonIDs(string v_minuendo, string v_sustraendo)
        {
            string res = "";
            string[] minuendos = v_minuendo.Split(',');
            string sustraendos = "," + v_sustraendo + ",";        // Me aseguro que comience y  termine en , para que el contains pueda encontrar el primero y el ultimo


            foreach (string s in minuendos)
            {
                if (s.Trim().Length > 0)
                {
                    if (!sustraendos.Contains("," + s + ","))
                    {
                        res = res + s + ",";
                    }
                }
            }

            return res;

        }

        /// <summary>
        /// Envia, en una sola llamada, todas las definiciones de los accesslevels a Todos los HH de la Organizacion.
        /// </summary>
        /// <param name="v_HHID"></param>
        public void enviarAccessLevelsDefinitions(int organizationID, string isDownloadingDB)
        {
            try
            {
                StaticTools.obtenerMutex_StateObjectClients();
                Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

                bool enc = false;
                List<StateObject> ClienteHH = null;
                foreach (KeyValuePair<string, StateObject> pair in listaClientes)
                {                    
                    if (pair.Value.orgID == organizationID)
                    {
                        enc = true;
                        ClienteHH.Add(pair.Value);
                        break;
                    }
                }

                if (enc)
                {
                    foreach (StateObject StObj in ClienteHH)
                    {
                        string v_HHID = StObj.HHID;

                        string v_accessLevelsIDs = obtenerAccessLevelsFromPanel(v_HHID, organizationID);

                        loguearString("Envio AccessLevels al HandHeld:" + v_HHID);

                        string[] idACCs = v_accessLevelsIDs.Split(',');
                        string LNLPanelIDstr = mainApp.DataManager.ObtenerLenelPanelID(v_HHID);


                        string totalAccessLevelInfo = "";
                        if ((LNLPanelIDstr != "") && organizationID > 0)
                        {
                            int LNLPanelID = int.Parse(LNLPanelIDstr);

                            foreach (string ACCID in idACCs)
                            {
                                if (ACCID.Trim().Length > 0)
                                {
                                    string accessLevelinfo = getAccessLevelDefinition(organizationID, LNLPanelID, int.Parse(ACCID));
                                    if (accessLevelinfo != "")
                                    {
                                        totalAccessLevelInfo = totalAccessLevelInfo + accessLevelinfo + "+";

                                        //mainApp.ComunicationSystem.agregarTrabajoAccessLevel(ClienteHH, accessLevelinfo);       // Encola el trabajo para enviar el accessLevel
                                    }
                                }
                            }

                            if (totalAccessLevelInfo.Length > 0)
                            {
                                totalAccessLevelInfo = totalAccessLevelInfo.Substring(0, totalAccessLevelInfo.Length - 1);      // Saca el ultimo |
                            }
                            // Encola el trabajo para enviar todos los accessLevels juntos.
                            mainApp.ComunicationSystem.agregarTrabajoAccessLevel(StObj, totalAccessLevelInfo);

                            // Si el envio de accesslevels se genero por un cambio manual desde Lenel, se envian los PersonIDs del HH para borrar los que no deban figurar mas.
                            if (isDownloadingDB != "1")
                            {
                                mainApp.ComunicationSystem.enviarAllPersonIDJob(StObj.HHID, StObj);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en enviarAccessLevelsDefinitions() a todos los HH de la org.: " + ex.Message);
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }
        }


        /// <summary>
        /// Envia, en una sola llamada, todas las definiciones de los accesslevels asociados al panel HHID
        /// </summary>
        /// <param name="v_HHID"></param>
        public void enviarAccessLevelsDefinitions(string v_HHID, int organizationID, string isDownloadingDB)
        {
            try
            {

                StaticTools.obtenerMutex_StateObjectClients();
                Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

                bool enc = false;
                StateObject ClienteHH = null;
                foreach (KeyValuePair<string, StateObject> pair in listaClientes)
                {
                    if (pair.Key == v_HHID)
                    {
                        enc = true;
                        ClienteHH = pair.Value;
                        break;
                    }
                }

                if (enc)
                {
                    //  int organizationID = mainApp.DataManager.obtenerOrganizationIDFromHHID(v_HHID);

                    string v_accessLevelsIDs = obtenerAccessLevelsFromPanel(v_HHID, organizationID);

                    loguearString("Envio AccessLevels al HandHeld:" + v_HHID);

                    string[] idACCs = v_accessLevelsIDs.Split(',');
                    string LNLPanelIDstr = mainApp.DataManager.ObtenerLenelPanelID(v_HHID);


                    string totalAccessLevelInfo = "";
                    if ((LNLPanelIDstr != "") && organizationID > 0)
                    {
                        int LNLPanelID = int.Parse(LNLPanelIDstr);

                        foreach (string ACCID in idACCs)
                        {
                            if (ACCID.Trim().Length > 0)
                            {
                                string accessLevelinfo = getAccessLevelDefinition(organizationID, LNLPanelID, int.Parse(ACCID));
                                if (accessLevelinfo != "")
                                {
                                    totalAccessLevelInfo = totalAccessLevelInfo + accessLevelinfo + "+";

                                    //mainApp.ComunicationSystem.agregarTrabajoAccessLevel(ClienteHH, accessLevelinfo);       // Encola el trabajo para enviar el accessLevel
                                }
                            }
                        }

                        if (totalAccessLevelInfo.Length > 0)
                        {
                            totalAccessLevelInfo = totalAccessLevelInfo.Substring(0, totalAccessLevelInfo.Length - 1);      // Saca el ultimo |
                        }
                        // Encola el trabajo para enviar todos los accessLevels juntos.
                        mainApp.ComunicationSystem.agregarTrabajoAccessLevel(ClienteHH, totalAccessLevelInfo);

                        // Si el envio de accesslevels se genero por un cambio manual desde Lenel, se envian los PersonIDs del HH para borrar los que no deban figurar mas.
                        if (isDownloadingDB != "1")
                        {
                            mainApp.ComunicationSystem.enviarAllPersonIDJob(ClienteHH.HHID, ClienteHH);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                loguearString(v_HHID + " - excepcion en enviarAccessLevelsDefinitions(): " + ex.Message);
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }
        }
         
        public string obtenerAccessLevelsFromPanel(string v_HHID, int v_orgID)
        {

            string res = "";

            try
            {

                string LNLPanelIDstr = mainApp.DataManager.ObtenerLenelPanelID(v_HHID);

                if (LNLPanelIDstr != "")
                {
                    int LNLPanelID = int.Parse(LNLPanelIDstr);
                    if (AccessLevelLogic.ListaAccessLevels.ContainsKey(v_orgID))
                    {
                        LENELAccessLevels LNACC = AccessLevelLogic.ListaAccessLevels[v_orgID];

                        res = LNACC.obtenerAccessLevelsStringFromPanel(LNLPanelID);
                    }
                }
            }
            catch (Exception ex)
            {
                loguearString(v_HHID + " - excepcion en obtenerAccessLevelsFromPanel(): " + ex.Message);
            }


            return res;
        }

        /// <summary>
        /// Devuelve un string con la definicion del accessLevel indicado en v_ACCID
        /// </summary>
        /// <param name="v_HHID"></param>
        /// <param name="v_ACCID"></param>
        /// <returns></returns>
        public string getAccessLevelDefinition(int v_organizationID, int v_LNLPanelID, int v_ACCID)
        {
            string strAccess = "";

            LENELAccessLevels LNACC = AccessLevelLogic.ListaAccessLevels[v_organizationID];

            AccessLevelData accessLevel = LNACC.getAccessLevel(v_LNLPanelID);

            if (accessLevel != null)
            {
                if (accessLevel.ReaderTZ.ContainsKey(v_ACCID))
                {
                    Dictionary<int, int> ReaderTZList = accessLevel.ReaderTZ[v_ACCID];

                    int entrada = mainApp.DataManager.ObtenerEntranceReaderID(v_LNLPanelID);      // OJO: Zero based ReaderIDs
                    int TZEntrada = -1;
                    int salida = mainApp.DataManager.ObtenerExitReaderID(v_LNLPanelID);        // OJO: Zero based ReaderIDs
                    int TZSalida = -1;

                    foreach (KeyValuePair<int, int> pair in ReaderTZList)
                    {
                        if (pair.Key == entrada)
                        {
                            TZEntrada = pair.Value;
                        }
                        if (pair.Key == salida)
                        {
                            TZSalida = pair.Value;
                        }
                    }

                    if ((TZSalida == TZEntrada) && (TZSalida > 0))
                    {
                        
                        strAccess = AccessLevelLogic.getStrAccess(v_organizationID, v_ACCID, v_LNLPanelID, TZEntrada, true, true);
                    }
                    else
                    {
                        if (TZEntrada >= 0)
                        {
                            strAccess =AccessLevelLogic.getStrAccess(v_organizationID, v_ACCID, v_LNLPanelID, TZEntrada, true, false);      // Pone el bit de entrada en true
                        }
                        if (TZSalida >= 0)
                        {
                            strAccess = strAccess + AccessLevelLogic.getStrAccess(v_organizationID, v_ACCID, v_LNLPanelID, TZSalida, false, true);    // Pone el bit de salida en true
                        }
                    }
                }
                else
                {
                    loguearString("AccessLevel: " + v_ACCID.ToString() + " no definido para el panelID: " + v_LNLPanelID.ToString());
                }
            }
            else
            {
                loguearString("Pedido de un accessLevel no definido para el panelID: " + v_LNLPanelID.ToString());
            }
            return strAccess;
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

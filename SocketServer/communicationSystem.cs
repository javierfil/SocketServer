﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace ServerComunicacionesALUTEL
{
    // Delaro los tipos de handlers para los eventos del socket.
    public delegate void StringEventHandler(object sender, stringEventArgs e);
    public delegate void byteArrayEventHandler(object sender, byteArrayEventArgs e);

    public delegate void callControlHandler();
    public delegate void addItemLOGHandler();
    public delegate void clearLOGHandler();
    
    /// <summary>
    /// 
    /// </summary>
    public class communicationSystem
    {
        #region ENCRIPTAR
        public const string ClaveEncriptar = "clave_encript:_-#$==%n#%$/()@-_:";
        #endregion

   
        public Puertos puertos = new Puertos();

        Aplicacion mainApp;
        public bool actualizeListView = false;
        public bool actualizeListViewVisitas = false;
        public bool actualizeListViewAccesosVirtualGates = false;
      
        public const int FIXED_HEADER_LENGTH = 512;
    
        private int jobCounter = 0;
        public string stringToAdd = "";

        public SocketServer socketServerGeneral;
        public LAYERLenel layerLENEL;
        public LAYERAlutrack layerALUTRACK;
        public LAYERCommunication communicationLAYER;
        

        public Queue<stringEventArgs> LOGInformation;

        static frmInicial mainForm;
        static frmInicialCliente mainFormCliente;

        public communicationSystem(Aplicacion v_app, frmInicial v_MainForm , frmInicialCliente v_MainFormCliente)
        {
            mainApp = v_app;
            mainForm = v_MainForm;
            mainFormCliente = v_MainFormCliente;

            LOGInformation = new Queue<stringEventArgs>();

            socketServerGeneral = new SocketServer(puertos.PORT_TO_SEND, puertos.PORT_TO_RECEIVE, SystemConfiguration.GPSControlIP, SystemConfiguration.GPSControlPort, mainApp);

            socketServerGeneral.getEmployeeList += new StringEventHandler(addSendEmployeeListJob);
            socketServerGeneral.getImage += new StringEventHandler(addSendImageJob);
            socketServerGeneral.actualizeAccess += new byteArrayEventHandler(actualizeAcccess);
            socketServerGeneral.actualizeVisit += new byteArrayEventHandler(actualizeVisit);

            socketServerGeneral.actualizeGPSData += new StringEventHandler(actualizeGPS);

            socketServerGeneral.deleteEmp += new StringEventHandler(borrarEmp);
            socketServerGeneral.addEmp += new StringEventHandler(agregarEmp);
            socketServerGeneral.sendEmpxVersion += new byteArrayEventHandler(enviarEmpxVersion);
            socketServerGeneral.enviarListaImagenes += new byteArrayEventHandler(enviarListaImagenes);
            socketServerGeneral.enviarVersionImagenes += new StringEventHandler(enviarVersionImagenes);
            socketServerGeneral.enviarMasImagenes += new StringEventHandler(enviarMasImagenes);
            socketServerGeneral.enviarDummy+=new StringEventHandler(enviarDummy);

            socketServerGeneral.actualizarLOG += new StringEventHandler(agregarItemLOG);
            socketServerGeneral.HHGPSLayer.actualizarLOG += new StringEventHandler(agregarItemLOG);
            socketServerGeneral.confirmarLOG += new StringEventHandler(confirmarLOG);

            layerLENEL = new LAYERLenel(mainApp,agregarItemLOG);
            layerALUTRACK = new LAYERAlutrack(mainApp, agregarItemLOG);
            communicationLAYER = new LAYERCommunication(mainApp, agregarItemLOG);           
           
        }


        void enviarDummy(object sender, stringEventArgs e)
        {

            int chunkSize = FIXED_HEADER_LENGTH;
            string header = "TYPE:DUMMY";

            // Nuevo: Encriptado de datos.
            string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
            headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;
            string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

            byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

            byte[] dataToSend = new byte[finalHeader.Length];
            System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);


            JobToSendToClient j = new JobToSendToClient();

            jobCounter++;
            j.ID = jobCounter;
            j.header = headerEncriptado;                // Usa el encriptado.
            j.byteData = dataToSend;

            // Nuevo: un pendingJobs por cada stateobject de cada cliente.
            e.stateObject.pendingJobs.Enqueue(j);
            e.stateObject.readyToSend.Set();


        }


        void enviarMasImagenes(object sender, stringEventArgs e)
        {
            // SETEAR FLAG PARA ENVIAR NUEVO BLOQUE DE IMAGENES
            e.stateObject.masImagenes = true;
        }

        void enviarVersionImagenes(object sender, stringEventArgs e)
        {
            loguearString(e.stateObject.HHID + " - Recibio pedido de LISTA DE VERSIONES DE IMAGENES", TiposLOG.LENEL);
            Thread t = new Thread(() => ThreadEnviarVersionImagenes(e));
            t.Name = "ThreadEnviarVersionImagenes";
            t.Start();
        }

        void ThreadEnviarVersionImagenes(stringEventArgs e)
        {
            StateObject clientState = e.stateObject;

            try
            {
                string dataString = "";
                Dictionary<int, Employee> listaEmpleados = mainApp.DataManager.getListaEmpleados();

                //int i = 50; // Para debug

                foreach (KeyValuePair<int, Employee> emp in listaEmpleados)
                {
                    if (clientState.abortFlag)
                    {
                        break;
                    }

                    
                    //if (i == 0) break;  // debug
                    //i--;    // debug

                    // VERIFICO QUE EL EMPLEADO TENGA IMAGEN
                    string pathImagenDB = mainApp.DataManager.cargarPathImagen(emp.Value.PersonID.ToString());
                    
                    if (HayImagen(pathImagenDB))
                    {
                        Tarjeta tarjeta = mainApp.DataManager.buscarTarjetadeEmpleado(emp.Value.Id);
                        // Solo se envia la info de un empleado si tiene tarjeta asociada y algun accesslevel asociado
                        if (tarjeta != null)
                        {
                            string accessLevels = AccessLevelLogic.getAccessLevelsByBadgeInHH(tarjeta.NUMERODETARJETA, e.stateObject.HHID);

                            if (accessLevels.Trim().Length > 0)
                            {
                                // Envío las versiones de las imagenes para que el hh pida los datos SOLO de aquellos que cambiaron
                                dataString += emp.Value.PersonID.ToString() + ":" + emp.Value.imageVersion.ToString() + ",";
                            }
                        }
                    }
                }

                if (dataString.Length > 0)   // Si ninguna de las tarjetas a enviar al HH tienen foto, terminar la secuencia enviando FinSYNC
                {
                    dataString = dataString.Substring(0, dataString.Length - 1);    // Le saco el | de más al final

                    // Solo encola el trabajo si se dispone efectivamente informacion sobre el HH
                    if (mainApp.ComunicationSystem.communicationLAYER.isPannelConnected(e.stateObject.HHID) && (!clientState.abortFlag))
                    {
                        string dataStringEncriptado = Encriptar_Datos.Encriptar.encriptar(dataString, communicationSystem.ClaveEncriptar);
                        byte[] dataBytes = Encoding.ASCII.GetBytes(dataStringEncriptado);

                        int chunkSize = FIXED_HEADER_LENGTH + dataBytes.Length;
                        string header = "TYPE:VERIMGLIST,SIZE:" + chunkSize.ToString();

                        // Nuevo: Encriptado de datos.
                        string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
                        headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;
                        string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

                        byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

                        byte[] dataToSend = new byte[finalHeader.Length + dataBytes.Length];
                        System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
                        System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

                        JobToSendToClient j = new JobToSendToClient();

                        jobCounter++;
                        j.ID = jobCounter;
                        j.header = headerEncriptado;                // Usa el encriptado.
                        j.byteData = dataToSend;

                        // Nuevo: un pendingJobs por cada stateobject de cada cliente.
                        e.stateObject.pendingJobs.Enqueue(j);
                        e.stateObject.readyToSend.Set();        // Le aviso al semaforo que hay cosas encoladas para mandar.

                        loguearString(e.stateObject.HHID + "- TRABAJO VERIMGLIST", TiposLOG.HH);
                        loguearString(e.stateObject.HHID + "- Cantidad de trabajos: " + e.stateObject.pendingJobs.Count.ToString(), TiposLOG.HH);

                    }
                    else
                    {
                        loguearString(e.stateObject.HHID + "- No conectado. NO SE AGREGA EL TRABAJO " + dataString, TiposLOG.HH);
                    }
                }
                else       // Si ninguna de las tarjetas a enviar al HH tienen foto, terminar la secuencia enviando FinSYNC
                {
                    EnviarJobFinSync(e.stateObject.HHID, e.stateObject);
                }
            }
            catch (Exception ex) { loguearString("Excepcion en ThreadEnviarVersionImagenes(): " + ex.Message, TiposLOG.HH); }
        }



        private bool HayImagen(string pathImagenDB)
        {
            bool res = false;

            try
            {
                string fileName = Path.GetFileName(pathImagenDB);
                string imagePath = SystemConfiguration.ImagesPath;
                
                res = File.Exists(imagePath + @"/" + fileName);

            }
            catch (Exception) { res = false; }

            return res;
        }

        void enviarListaImagenes(object sender, byteArrayEventArgs e)
        {
            loguearString(e.stateObject.HHID + " RECIBIO PEDIDO DE LISTA DE IMAGENES", TiposLOG.LENEL);
            Thread t = new Thread(() => ThreadLoopBloqueImagenes(e));
            t.Name = "ThreadLoopBloqueImagenes";
            t.Start();

            //try
            //{
            //    loguearString("RECIBIO PEDIDO DE LISTA DE IMAGENES", TiposLOG.ALUTRACK);
            //    if (e.byteData == null) return;

            //    socketServerGeneral.masImagenes = true; // Seteo indicador para enviar primer bloque de imagenes

            //    // Identifico los ID de las imagenes a enviar
            //    string dataID = Encriptar_Datos.Encriptar.desencriptar(Encoding.ASCII.GetString(e.byteData, 0, e.byteData.Length), communicationSystem.ClaveEncriptar);
            //    string[] IDs = dataID.Split(',');
            //    string HHID = e.textData["HHID"];

            //    // Se envían paquetes de 'cantImagenes' imagenes
            //    int cantImagenes = 100;
            //    int indice = 1; // Representa la cantidad de imagenes que se agregaron en el buffer

            //    byte[] dataBytes = new byte[0];
            //    byte[] aux = new byte[0];
            //    string dataLength = string.Empty;   // Va a contener el largo de cada imagen con el formato: " PersonID:VersionImagen:largoImagen "

            //    byte[] imageBytes;
                
            //    string imgver;
            //    string Imagen;

            //    for (int i = 0; i < IDs.Length; i++)
            //    {
            //        // Obtener los datos de la imagen 
            //        Imagen = mainApp.DataManager.cargarPathImagen(IDs[i]);
            //        imgver = mainApp.DataManager.cargarVersionImagen(IDs[i]);
            //        string fileName = Path.GetFileName(Imagen);
            //        string imagePath = SystemConfiguration.ImagesPath;
            //        try
            //        {
            //            imageBytes = File.ReadAllBytes(imagePath + @"/" + fileName);

            //            // Agrego el largo al dataLength
            //            dataLength += IDs[i] + ':' + imgver + ':' + imageBytes.Length + ',';

            //            // Agrego los datos bytes nuevos a dataBytes
            //            aux = dataBytes;
            //            dataBytes = new byte[aux.Length + imageBytes.Length];
            //            System.Buffer.BlockCopy(aux, 0, dataBytes, 0, aux.Length);
            //            System.Buffer.BlockCopy(imageBytes, 0, dataBytes, aux.Length, imageBytes.Length);

            //            indice++;
            //        }
            //        catch { /* Entra acá cuando el empleado no tiene imagen */ }


            //        // Si el tamaño del buffer a enviar supera los 3 Mb (aprox), enviar el bloque y limpiar para el proximo envio
            //        if (dataBytes.Length > 2000000)
            //        {
            //            EnviarJobBloqueImagenes(HHID, dataLength, dataBytes, e);
            //            // Limpio los datos
            //            dataBytes = new byte[0];
            //            aux = new byte[0];
            //            dataLength = string.Empty;

            //            socketServerGeneral.masImagenes = false;
            //        }

            //        // Si es multiplo de 10 ó si alcanzó el final de IDs debe mandar el trabajo y limpiar los datos para armar el header nuevo
            //        //if (i == IDs.Length - 1 || (indice % cantImagenes) == 0)
            //        //{
            //        //    EnviarJobBloqueImagenes(HHID, dataLength, dataBytes, e);

            //        //    // Limpio los datos
            //        //    dataBytes = new byte[0];
            //        //    aux = new byte[0];
            //        //    dataLength = string.Empty;
            //        //}
            //    }

            //    if (!dataLength.Equals(string.Empty)) EnviarJobBloqueImagenes(HHID, dataLength, dataBytes, e);
            //}
            //catch (Exception ex) { loguearString("Problema en enviarListaImagenes: " + ex.Message, TiposLOG.ALUTRACK); }
        }

        private void ThreadLoopBloqueImagenes(byteArrayEventArgs e)
        {
            try
            {
                
                if (e.byteData == null) return;

                e.stateObject.masImagenes = true; // Seteo indicador para enviar primer bloque de imagenes

                // Identifico los ID de las imagenes a enviar
                string dataID = Encriptar_Datos.Encriptar.desencriptar(Encoding.ASCII.GetString(e.byteData, 0, e.byteData.Length), communicationSystem.ClaveEncriptar);
                string[] IDs = dataID.Split(',');
                string HHID = e.textData["HHID"];

                int indice = 1; // Representa la cantidad de imagenes que se agregaron en el buffer

                byte[] dataBytes = new byte[0];
                byte[] aux = new byte[0];
                string dataLength = string.Empty;   // Va a contener el largo de cada imagen con el formato: " PersonID:VersionImagen:largoImagen "

                byte[] imageBytes;

                string imgver;
                string Imagen;
                int cantImagenesEnviadas = 0;
                int cantImagenesEnBloque=0;        // Contador de la cantidad de imagenes en cada bloque: dato para loguear solamente

                for (int i = 0; i < IDs.Length; i++)
                {
                    if (e.stateObject.abortFlag)
                    {
                        break;
                    }
                    // Obtener los datos de la imagen 
                    Imagen = mainApp.DataManager.cargarPathImagen(IDs[i]);
                    imgver = mainApp.DataManager.cargarVersionImagen(IDs[i]);
                    string fileName = Path.GetFileName(Imagen);
                    string imagePath = SystemConfiguration.ImagesPath;
                    try
                    {
                        imageBytes = File.ReadAllBytes(imagePath + @"/" + fileName);

                        // Agrego el largo al dataLength
                        dataLength += IDs[i] + ':' + imgver + ':' + imageBytes.Length + ',';

                        // Agrego los datos bytes nuevos a dataBytes
                        aux = dataBytes;
                        dataBytes = new byte[aux.Length + imageBytes.Length];
                        System.Buffer.BlockCopy(aux, 0, dataBytes, 0, aux.Length);
                        System.Buffer.BlockCopy(imageBytes, 0, dataBytes, aux.Length, imageBytes.Length);

                        indice++;
                    }
                    catch { /* Entra acá cuando el empleado no tiene imagen */ }

                    cantImagenesEnviadas++;
                    cantImagenesEnBloque++;
                    // Si el tamaño del buffer a enviar supera los 2 Mb (aprox), enviar el bloque y limpiar para el proximo envio
                    if (dataBytes.Length > 2000000)
                    {
                        loguearString("Va a enviar " + cantImagenesEnBloque + " imagenes. Total enviadas: " + cantImagenesEnviadas + " de " + IDs.Length, TiposLOG.HH);

                        EnviarJobBloqueImagenes(HHID, dataLength, dataBytes, e);
                        // Limpio los datos
                        dataBytes = new byte[0];
                        aux = new byte[0];
                        dataLength = string.Empty;

                        e.stateObject.masImagenes = false;
                        cantImagenesEnBloque = 0;
                    }

                }

                if (!dataLength.Equals(string.Empty) && !e.stateObject.abortFlag)
                {
                    EnviarJobBloqueImagenes(HHID, dataLength, dataBytes, e);
                    e.stateObject.masImagenes=false;
                }

                if (!e.stateObject.abortFlag)
                {
                    // Espero señal para enviar el ultimo FinSync
                    if (!e.stateObject.masImagenes) EsperaProximoPedidoImagenes(e.stateObject);

                    EnviarJobFinSync(HHID, e.stateObject);
                }
            }
            catch (Exception ex) { loguearString("Problema en enviarListaImagenes: " + ex.Message, TiposLOG.HH); }
        }

        /// <summary>
        /// Envía un paquete para avisarle al hh que la sincronización finalizó. Ya sea por fin de imagenes o porque ya está sincronizado.
        /// Se manda tambien la lista de personID definitiva que tiene que tener el HH
        /// </summary>
        /// <param name="HHID"></param>
        /// <param name="e"></param>
        public void EnviarJobFinSync(string HHID, StateObject e)
        {
            try
            {
                // Armo el trabajo a enviar si el HH está conectado
                if (mainApp.ComunicationSystem.socketServerGeneral.isHandHeldOnLine(HHID))
                {
                    enviarAllPersonIDJob(HHID, e);

                    enviarFinSyncJob(HHID, e);
 
                    mainApp.DataManager.actualizarFechaSync(HHID);
                }
            }

            catch (Exception ex) { loguearString("Problema en EnviarFinSYNC: " + ex.Message, TiposLOG.HH); }
        }

        private void enviarFinSyncJob(string HHID, StateObject e)
        {
            int chunkSize = FIXED_HEADER_LENGTH;

            string header = "TYPE:FINSYNC";
            // Nuevo: Encriptado de datos.
            string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
            headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

            string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);
            byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

            byte[] dataToSend = new byte[finalHeader.Length];

            System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);


            JobToSendToClient j = new JobToSendToClient();

            jobCounter++;
            j.ID = jobCounter;
            j.header = headerEncriptado;
            j.byteData = dataToSend;

            e.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
            e.readyToSend.Set();            // Activar envio

            loguearString(HHID + "- TRABAJO FINSYNC", TiposLOG.LENEL);
        }

        // Envia el trabajo ALLPERSONID al HH para indicarle los personID validos en funcion de los actuales AccessLevels
        public void enviarAllPersonIDJob(string HHID, StateObject e)
        {
            string allPersonID = mainApp.DataManager.loadAllPersonID(HHID); // Carga los PersonID con los IDs de las personas que pertenecen al HHID

            string allPersonIDEnc = Encriptar_Datos.Encriptar.encriptar(allPersonID, communicationSystem.ClaveEncriptar);
            byte[] dataBytes = Encoding.ASCII.GetBytes(allPersonIDEnc);

            int chunkSize = FIXED_HEADER_LENGTH + dataBytes.Length;

            string header = "TYPE:ALLPERSONID,SIZE:" + chunkSize.ToString();

            // Nuevo: Encriptado de datos.
            string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
            headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

            string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);
            byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

            byte[] dataToSend = new byte[finalHeader.Length + dataBytes.Length];
            System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
            System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

            JobToSendToClient j = new JobToSendToClient();

            jobCounter++;
            j.ID = jobCounter;
            j.header = headerEncriptado;
            j.byteData = dataToSend;

            e.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
            e.readyToSend.Set();

            int cantEnviados = 0;
            if (allPersonID.Trim().Length > 0)
            {
                cantEnviados=allPersonID.Split(',').Length;
            }
            loguearString(HHID + "- TRABAJO ALLPERSONID. Cant de PersonIDs enviados:" + cantEnviados.ToString(), TiposLOG.LENEL);

        }

        private void EsperaProximoPedidoImagenes(StateObject e)
        {
            while (!e.masImagenes && !e.abortFlag) System.Threading.Thread.Sleep(1000);
        }

        private void EnviarJobBloqueImagenes(string HHID, string dataLength, byte[] dataBytes, byteArrayEventArgs e)
        {

            // Espero señal para enviar imagenes
            if (!e.stateObject.masImagenes) EsperaProximoPedidoImagenes(e.stateObject);

            if (!e.stateObject.abortFlag)
            {
                try
                {
                    dataLength = dataLength.Trim(',');  // Quito la última coma

                    // Armo el trabajo a enviar si el HH está conectado
                    if (mainApp.ComunicationSystem.communicationLAYER.isPannelConnected(HHID))
                    {
                        dataLength = Encriptar_Datos.Encriptar.encriptar(dataLength, communicationSystem.ClaveEncriptar);
                        byte[] dataLengthBytes = Encoding.ASCII.GetBytes(dataLength);

                        int chunkSize = FIXED_HEADER_LENGTH + dataLengthBytes.Length + dataBytes.Length;

                        string header = "TYPE:IMGLIST,SIZE:" + chunkSize.ToString() + ",IDLENGTH:" + dataLengthBytes.Length.ToString();

                        // Nuevo: Encriptado de datos.
                        string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
                        headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

                        string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);
                        byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

                        byte[] dataToSend = new byte[finalHeader.Length + dataLengthBytes.Length + dataBytes.Length];
                        System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
                        System.Buffer.BlockCopy(dataLengthBytes, 0, dataToSend, headerBytedata.Length, dataLengthBytes.Length);
                        System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length + dataLengthBytes.Length, dataBytes.Length);

                        JobToSendToClient j = new JobToSendToClient();

                        jobCounter++;
                        j.ID = jobCounter;
                        j.header = headerEncriptado;
                        j.byteData = dataToSend;

                        e.stateObject.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
                        e.stateObject.readyToSend.Set();
                        loguearString(HHID + "- ENVIADO TRABAJO Bloque de imágenes, largo: " + chunkSize.ToString(), TiposLOG.HH);
                    }
                }

                catch (Exception ex) { loguearString("Problema en EnviarJobBloqueImagenes: " + ex.Message, TiposLOG.HH); }
            }
        }

        void enviarEmpxVersion(object sender, byteArrayEventArgs e)
        {
           
            Thread t = new Thread(() => ThreadEnviarEmpxVersion(e));
            t.Name = "ThreadEnviarEmpxVersion";
            t.Start();
        }

        void ThreadEnviarEmpxVersion(byteArrayEventArgs e)
        {

            try
            {
                if (e.byteData == null) return;

                // Identifico los ID a enviar
                string dataID = Encriptar_Datos.Encriptar.desencriptar(Encoding.ASCII.GetString(e.byteData, 0, e.byteData.Length), communicationSystem.ClaveEncriptar);
                string[] IDs = dataID.Split(',');
                string HHID = e.textData["HHID"];

                loguearString(e.stateObject.HHID + " Pide "+IDs.Length.ToString() +" empleados x Version", TiposLOG.LENEL);

                // Obtengo los datos de los empleados con el ID y los agrego a la lista para enviar
                string aux = string.Empty;
                string dataEmp = " ";       // Obligatorio, Comenzar con espacio
                string isAdmin_BETA = "true";
                string passWord_BETA = "alutel";
                int i; 
                for (i = 0; i < IDs.Length; i++)
                {
                    if (e.stateObject.abortFlag)
                    {
                        break;
                    }

                    // Cargo los datos del empleado
                    Employee emp = mainApp.DataManager.buscarEmpleadoxPersonIDenBD(IDs[i]);

                    Tarjeta t = mainApp.DataManager.buscarTarjetadeEmpleado(emp.Id);
                    // Solo se envia la info de un empleado si tiene tarjeta asociada y algun accesslevel asociado
                    if ((t == null)) loguearString("El empleado con personID = " + IDs[i] + " no tiene tarjeta asociada", TiposLOG.HH);
                    else
                    {
                        string accessLevels = AccessLevelLogic.getAccessLevelsByBadgeInHH(t.NUMERODETARJETA, HHID);

                        // Agrego los datos a dataEmp
                        if (accessLevels.Trim().Length > 0)
                        {
                            aux += "TARJETA:" + t.NUMERODETARJETA + ",NOMBRE:" + emp.Nombre + ",APELLIDO:" + emp.Apellido + ",DOCUMENTO:" + emp.NumeroDocumento + ",EMPRESA:" + emp.Empresa + ",ACCESO:" + t.ESTADO.ToString() + ",ADMIN:" + isAdmin_BETA + ",PASS:" + passWord_BETA + ",ACCESSID:" + accessLevels + ",VERSION:" + emp.VersionEmpleado.ToString() + ",PERSONID:" + emp.PersonID.ToString() + '|';
                            if ( (i!=0) && (i%5000 == 0) )
                            {
                                dataEmp += Encriptar_Datos.Encriptar.encriptar(aux, communicationSystem.ClaveEncriptar) + ' ';
                                aux = string.Empty;
                            }
                        }
                    }
                }
                if (!e.stateObject.abortFlag)
                {
                    if (!aux.Equals(string.Empty))
                    {
                        dataEmp += Encriptar_Datos.Encriptar.encriptar(aux, communicationSystem.ClaveEncriptar);
                    }

                    loguearString( "Server va a enviar BloqueEmpleados con " + i.ToString() + " empleados a: "+e.stateObject.HHID, TiposLOG.LENEL);

                    enviarBloqueEmpleados(dataEmp, HHID, e.stateObject);
                }
            }
            catch (Exception ex) { loguearString("Excepción en ThreadEnviarEmpxVersion: " + ex.Message, TiposLOG.HH); }
        }


        private void enviarBloqueEmpleados(string dataEmp, string HHID, StateObject clientState)
        {

            try
            {

                //dataEmp = dataEmp.Trim('|');    // Saco último '|'

                // Armo el trabajo a enviar si el HH está conectado
                if (mainApp.ComunicationSystem.communicationLAYER.isPannelConnected(HHID))
                {

                    string dataStringEncriptado = dataEmp;//Encriptar_Datos.Encriptar.encriptar(dataEmp, communicationSystem.ClaveEncriptar);
                    byte[] dataBytes = Encoding.ASCII.GetBytes(dataStringEncriptado);

                    int chunkSize = FIXED_HEADER_LENGTH + dataBytes.Length;
                    //TYPE:EMPLISTXVER,SIZE:(.*)
                    string header = "TYPE:EMPLISTXVER,SIZE:" + chunkSize.ToString();

                    // Nuevo: Encriptado de datos.
                    string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
                    headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

                    string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);
                    byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

                    byte[] dataToSend = new byte[finalHeader.Length + dataBytes.Length];
                    System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
                    System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

                    JobToSendToClient j = new JobToSendToClient();

                    jobCounter++;
                    j.ID = jobCounter;
                    j.header = headerEncriptado;
                    j.byteData = dataToSend;

                    clientState.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
                    clientState.readyToSend.Set();
                    loguearString(HHID + "- TRABAJO Bloque de empleados por versión", TiposLOG.HH);
                }
            }
            catch (Exception ex) { loguearString("Excepción en enviarBloqueEmpleados: " + ex.Message, TiposLOG.HH); }
        }



        public void agregarEmp(object sender, stringEventArgs e)
        {
            try
            {
                string dataEmp = string.Empty;
                string isAdmin_BETA = "true";
                string passWord_BETA = "alutel";

                string tarjeta = e.textData["TARJETA"];         // La tarjeta del empleado que se va a agregar
                string id = e.textData["HHID"];

                Employee emp = mainApp.DataManager.buscarEmpleadoxTarjeta(tarjeta);

                Tarjeta t = mainApp.DataManager.buscarTarjetadeEmpleado(emp.Id);
                // Solo se envia la info de un empleado si tiene tarjeta asociada y algun accesslevel asociado
                if ((tarjeta == null) || (t == null))
                {
                    loguearString("NO SE PUDO ENVIAR LA INFORMACIÓN DEL EMPLEADO CON TARJETA " + tarjeta, TiposLOG.HH);
                    return;
                }

                string accessLevels = AccessLevelLogic.getAccessLevelsByBadgeInHH(t.NUMERODETARJETA, e.stateObject.HHID);

                if (accessLevels.Trim().Length > 0)
                {

                    dataEmp = "TARJETA:" + t.NUMERODETARJETA + ",NOMBRE:" + emp.Nombre + ",APELLIDO:" + emp.Apellido + ",DOCUMENTO:" + emp.NumeroDocumento + ",EMPRESA:" + emp.Empresa + ",ACCESO:" + t.ESTADO.ToString() + ",IMGVER:" + emp.imageVersion.ToString().Trim() + ",ADMIN:" + isAdmin_BETA + ",PASS:" + passWord_BETA + ",ACCESSID:" + accessLevels + ",VERSION:" + emp.VersionEmpleado.ToString() +",PERSONID:" + emp.PersonID.ToString();

                    //dataEmp = "TARJETA:" + t.NUMERODETARJETA + ",NOMBRE:" + emp.Nombre + ",APELLIDO:" + emp.Apellido + ",DOCUMENTO:" + emp.NumeroDocumento + ",EMPRESA:" + emp.Empresa + ",ACCESO:" + t.ESTADO.ToString() + ",IMGVER:" + emp.imageVersion.ToString().Trim() + ",ADMIN:" + isAdmin_BETA + ",PASS:" + passWord_BETA + ",ACCESSID:" + accessLevels;

                }

                // Solo encola el trabajo si se dispone efectivamente informacion sobre el HH
                if (mainApp.ComunicationSystem.communicationLAYER.isPannelConnected(id))
                {

                    string dataStringEncriptado = Encriptar_Datos.Encriptar.encriptar(dataEmp, communicationSystem.ClaveEncriptar);
                    byte[] dataBytes = Encoding.ASCII.GetBytes(dataStringEncriptado);

                    int chunkSize = FIXED_HEADER_LENGTH + dataBytes.Length;
                    //TYPE:ADDEMP,SIZE:(.*)
                    string header = "TYPE:ADDEMP,SIZE:" + chunkSize.ToString();

                    // Nuevo: Encriptado de datos.
                    string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
                    headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

                    string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);
                    byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

                    byte[] dataToSend = new byte[finalHeader.Length + dataBytes.Length];
                    System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
                    System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

                    JobToSendToClient j = new JobToSendToClient();

                    jobCounter++;
                    j.ID = jobCounter;
                    j.header = headerEncriptado;
                    j.byteData = dataToSend;

                    e.stateObject.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
                    e.stateObject.readyToSend.Set();
                    loguearString(e.stateObject.HHID + "- TRABAJO Agregar empleado: " + header, TiposLOG.HH);
                }
            }
            catch (Exception ex) { loguearString("Excepción agregando empleado: "+ex.Message, TiposLOG.HH); }
        }

        void borrarEmp(object sender, stringEventArgs e)
        {
            try
            {
                string tarjeta = e.textData["TARJETA"];         // La tarjeta del empleado que se va a eliminar
                string id = e.textData["HHID"];

                // Solo encola el trabajo si se dispone efectivamente informacion sobre el HH
                if (mainApp.ComunicationSystem.communicationLAYER.isPannelConnected(id))
                {
                    int chunkSize = FIXED_HEADER_LENGTH;

                    string header = "TYPE:DELEMP,SIZE:" + chunkSize.ToString() + ",BADGE:" + tarjeta;

                    // Nuevo: Encriptado de datos.
                    string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
                    headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

                    string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);
                    byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

                    byte[] dataToSend = new byte[finalHeader.Length];
                    System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);

                    JobToSendToClient j = new JobToSendToClient();

                    jobCounter++;
                    j.ID = jobCounter;
                    j.header = headerEncriptado;
                    j.byteData = dataToSend;

                    e.stateObject.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
                    e.stateObject.readyToSend.Set();
                    loguearString(e.stateObject.HHID + "- TRABAJO Borrar empleado: " + header, TiposLOG.HH);
                }
            }
            catch (Exception ex) { loguearString("Excepción borrando empleado: " + ex.Message, TiposLOG.HH); }

        }
        /// <summary>
        /// Actualiza localmente el ultimo dato GPS recibido y lanza el evento que verifica los accesos a las Virtual gates.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="v_eventArgs"></param>
        void actualizeGPS(object sender, stringEventArgs v_eventArgs)
        {
            string HHID = v_eventArgs.textData["HHID"];
            string latitud = v_eventArgs.textData["LATITUD"];
            string longitud = v_eventArgs.textData["LONGITUD"];
            string hora = v_eventArgs.textData["HORA"];

            EventoGPS nuevoEvento = new EventoGPS(HHID, mainApp.InterpretarLatitud(latitud), mainApp.InterpretarLongitud(longitud), hora);

            EventoGPS ultimoEvento = mainApp.DataManager.getLastEventoGPS(HHID);
            
            mainApp.DataManager.addEventoGPS(HHID, nuevoEvento);

            mainApp.DataManager.updateVirtualGateEvents(ultimoEvento, nuevoEvento);
        }
        void addSendEmployeeListJob(object sender, stringEventArgs e)
        {
            loguearString(e.stateObject.HHID + "-Pidio la lista de Versiones de Empleados",TiposLOG.LENEL);
            Thread t = new Thread(() => ThreadAddSendEmployeeListJob(e));
            t.Name = "ThreadAddSendEmployeeListJob";
            t.Start();
        }

        void ThreadAddSendEmployeeListJob(stringEventArgs e)
        {
            //Se envian los AccessLevels UNA sola vez por conexion, 
            if (!e.stateObject.envioInicialAccessLevels)
            {
                string HHID = e.stateObject.HHID;
                int orgID = mainApp.DataManager.obtenerOrganizationIDFromHHID(HHID);
                communicationLAYER.enviarAccessLevelsDefinitions(HHID, orgID,"1");          // El "1"es para que no se envion los allPersonIDs en esta llamada.
                e.stateObject.envioInicialAccessLevels= true;
            }

            agregarTrabajoListaEmpleados(e);
        }

        //void addBeginEmpListJob(object sender, stringEventArgs v_eventArgs)
        //{
        //    string header = "TYPE:BEGINEMPLIST";

        //    // Nuevo: Encriptado de datos.
        //    string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
        //    headerEncriptado = "SIZE:"+FIXED_HEADER_LENGTH.ToString() + ",DATA:" + headerEncriptado;
        //    string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

        //    byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

        //    byte[] dataToSend = new byte[finalHeader.Length];

        //    System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
        //    //System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

        //    JobToSendToClient j = new JobToSendToClient();

        //    jobCounter++;
        //    j.ID = jobCounter;
        //    j.header = headerEncriptado;                // Usa el encriptado.
        //    j.byteData = dataToSend;

        //    // Nuevo: un pendingJobs por cada stateobject de cada cliente.
        //    v_eventArgs.stateObject.pendingJobs.Enqueue(j);
        //    v_eventArgs.stateObject.readyToSend.Set();        // Le aviso al semaforo que hay cosas encoladas para mandar.

        //    loguearString(v_eventArgs.stateObject.HHID + "- TRABAJO BEGINEMPLIST", TiposLOG.ALUTRACK);
        //}

        // Envia el mensaje de fin del envio de los bloques de empleadosXVersion
        //void addEndEmpListJob(StateObject clientState)
        //{
        //    string header = "TYPE:ENDEMPLIST";

        //    // Nuevo: Encriptado de datos.
        //    string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
        //    headerEncriptado = "SIZE:"+FIXED_HEADER_LENGTH.ToString() + ",DATA:" + headerEncriptado;
        //    string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

        //    byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

        //    byte[] dataToSend = new byte[finalHeader.Length];

        //    System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
        //    //System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

        //    JobToSendToClient j = new JobToSendToClient();

        //    jobCounter++;
        //    j.ID = jobCounter;
        //    j.header = headerEncriptado;                // Usa el encriptado.
        //    j.byteData = dataToSend;

        //    // Nuevo: un pendingJobs por cada stateobject de cada cliente.
        //    clientState.pendingJobs.Enqueue(j);
        //    clientState.readyToSend.Set();        // Le aviso al semaforo que hay cosas encoladas para mandar.

        //    loguearString(clientState.HHID + "- TRABAJO ENDEMPLIST", TiposLOG.ALUTRACK);

        //}


        public void agregarTrabajoListaEmpleadosEspecifica(StateObject ClienteHH, string v_personIDs)
        {
            
            string personIDs ="'"+ v_personIDs+",";         // Para poder encontrar con contains 

            string dataString = "";
            Dictionary<int, Employee> listaEmpleados = mainApp.DataManager.getListaEmpleados();

            
            try
            {
                StaticTools.obtenerMutex_ListaEmpleados();

                foreach (KeyValuePair<int, Employee> emp in listaEmpleados)
                {
                    if (personIDs.Contains(","+emp.Value.PersonID.ToString()))
                    {
                         dataString += emp.Value.PersonID.ToString() + "," + emp.Value.VersionEmpleado.ToString() + "|";
                    }
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en agregarTrabajoListaEmpleadosEspecifica: " + ex.Message, TiposLOG.LENEL);
            }
            finally
            {
                StaticTools.liberarMutex_ListaEmpleados();
            }

            if (dataString.Length > 0)
                    dataString = dataString.Substring(0, dataString.Length - 1);    // Le saco el | de más al final
           


    // Solo encola el trabajo si se dispone efectivamente informacion sobre el HH y no esta en proceso de abort...
            if (mainApp.ComunicationSystem.communicationLAYER.isPannelConnected(ClienteHH.HHID) && !ClienteHH.abortFlag)
                {
                    string dataStringEncriptado = Encriptar_Datos.Encriptar.encriptar(dataString, communicationSystem.ClaveEncriptar);
                    byte[] dataBytes = Encoding.ASCII.GetBytes(dataStringEncriptado);

                    int chunkSize = FIXED_HEADER_LENGTH + dataBytes.Length;
                    string header = "TYPE:EMPLIST,SIZE:" + chunkSize.ToString();

                    // Nuevo: Encriptado de datos.
                    string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
                    headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;
                    string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

                    byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

                    byte[] dataToSend = new byte[finalHeader.Length + dataBytes.Length];
                    System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
                    System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

                    JobToSendToClient j = new JobToSendToClient();

                    jobCounter++;
                    j.ID = jobCounter;
                    j.header = headerEncriptado;                // Usa el encriptado.
                    j.byteData = dataToSend;

                    // Nuevo: un pendingJobs por cada stateobject de cada cliente.
                    ClienteHH.pendingJobs.Enqueue(j);
                    ClienteHH.readyToSend.Set();        // Le aviso al semaforo que hay cosas encoladas para mandar.

                    loguearString(ClienteHH.HHID + "- Encolado TRABAJO EMPLIST", TiposLOG.LENEL);
                    loguearString(ClienteHH.HHID + "- Cantidad de trabajos: " + ClienteHH.pendingJobs.Count.ToString(), TiposLOG.HH);

                   // int orgID = mainApp.DataManager.obtenerOrganizationIDFromHHID(ClienteHH.HHID);

                    //layerLENEL.enviarAccessLevelsDefinitions(v_eventArgs.stateObject.HHID, orgID);

                }
                else
                {
                    loguearString(ClienteHH.HHID + "- no conectado. NO SE AGREGA EL TRABAJO " + dataString, TiposLOG.HH);
                }

  


        }


        /// <summary>
        /// Agrega a la cola de trabajos el trabajo de enviar la lista completa de versiones de empleados
        /// BETA: Todos son ADMIN: true y todos tienen Password: alutel
        /// NUEVO: EMPLIST envia ahora SOLAMENTE la lista de versiones de empleados, para que el HH pida solamente los que le faltan
        /// </summary>
        /// <param name="v_eventArgs"></param>
        public void agregarTrabajoListaEmpleados(stringEventArgs v_eventArgs)
        {
            try
            {
                string dataString = "";
                Dictionary<int, Employee> listaEmpleados = mainApp.DataManager.getListaEmpleados();

               
                DateTime ultimaActualizacionHandHeld = mainApp.DataManager.obtenerUltimaFechaSync(v_eventArgs.stateObject.HHID);
                
                //MUTEX
                try
                {
                    StaticTools.obtenerMutex_ListaEmpleados();
                    loguearString("Entro a agregarTrabajoListaEmpleados(EMPLIST) con : " + listaEmpleados.Count.ToString(),TiposLOG.LENEL);

                    foreach (KeyValuePair<int, Employee> emp in listaEmpleados)
                    {
                        if (v_eventArgs.stateObject.abortFlag)
                        {
                            loguearString("Hizo ABORTFLAG=TRUE ",TiposLOG.LENEL);
                            break;
                        }
                        //if (i == 0) break;  // debug
                        //i--;    // debug
                        DateTime ultimaActualizacionEmpleado = emp.Value.ultimaActualizacion;

                        Tarjeta tarjeta = mainApp.DataManager.buscarTarjetadeEmpleado(emp.Value.Id);


                        // Solo se envia la info de un empleado si tiene tarjeta asociada y algun accesslevel asociado
                        if ((tarjeta != null))
                        {
                            string accessLevels =AccessLevelLogic.getAccessLevelsByBadgeInHH(tarjeta.NUMERODETARJETA, v_eventArgs.stateObject.HHID);
                            DateTime ultimaActualizacionTarjeta = tarjeta.ultimaActualizacion;
                            if (accessLevels.Trim().Length>0)
                            {
                                if ((ultimaActualizacionEmpleado > ultimaActualizacionHandHeld)||(ultimaActualizacionTarjeta > ultimaActualizacionHandHeld ))
                                {

                                    // Envío los empleados con sus versiones para que el hh pida los datos SOLO de aquellos que cambiaron
                                    dataString += emp.Value.PersonID.ToString() + "," + emp.Value.VersionEmpleado.ToString() + "|";

                                }
                            }
                        }
                    }

                   // loguearString("dataString: " + dataString,TiposLOG.LENEL);

                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en agregarTrabajoListaEmpleados-MUTEX " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    //MUTEX
                    StaticTools.liberarMutex_ListaEmpleados();
                }

                if (dataString.Length > 0)
                    dataString = dataString.Substring(0, dataString.Length - 1);    // Le saco el | de más al final
                else
                {
                    EnviarJobFinSync(v_eventArgs.stateObject.HHID, v_eventArgs.stateObject);
                    return;
                }
                // Solo encola el trabajo si se dispone efectivamente informacion sobre el HH y no esta en proceso de abort...
                if (mainApp.ComunicationSystem.communicationLAYER.isPannelConnected(v_eventArgs.stateObject.HHID) && !v_eventArgs.stateObject.abortFlag)
                {
                    string dataStringEncriptado = Encriptar_Datos.Encriptar.encriptar(dataString, communicationSystem.ClaveEncriptar);
                    byte[] dataBytes = Encoding.ASCII.GetBytes(dataStringEncriptado);

                    int chunkSize = FIXED_HEADER_LENGTH + dataBytes.Length;
                    string header = "TYPE:EMPLIST,SIZE:" + chunkSize.ToString();

                    // Nuevo: Encriptado de datos.
                    string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
                    headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;
                    string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

                    byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

                    byte[] dataToSend = new byte[finalHeader.Length + dataBytes.Length];
                    System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
                    System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

                    JobToSendToClient j = new JobToSendToClient();

                    jobCounter++;
                    j.ID = jobCounter;
                    j.header = headerEncriptado;                // Usa el encriptado.
                    j.byteData = dataToSend;

                    // Nuevo: un pendingJobs por cada stateobject de cada cliente.
                    v_eventArgs.stateObject.pendingJobs.Enqueue(j);
                    v_eventArgs.stateObject.readyToSend.Set();        // Le aviso al semaforo que hay cosas encoladas para mandar.

                    loguearString(v_eventArgs.stateObject.HHID + "- Encolado TRABAJO EMPLIST", TiposLOG.LENEL);
                   // loguearString(v_eventArgs.stateObject.HHID + "- Cantidad de trabajos: " + v_eventArgs.stateObject.pendingJobs.Count.ToString(), TiposLOG.ALUTRACK);

                    int orgID = mainApp.DataManager.obtenerOrganizationIDFromHHID(v_eventArgs.stateObject.HHID);

                    //layerLENEL.enviarAccessLevelsDefinitions(v_eventArgs.stateObject.HHID, orgID);

                }
                else
                {
                    loguearString(v_eventArgs.stateObject.HHID + "- no conectado. NO SE AGREGA EL TRABAJO " + dataString, TiposLOG.HH);
                }
            }
            catch (Exception ex) { loguearString("Excepcion en agregarTrabajoListaEmpleados(): " + ex.Message, TiposLOG.HH); }
        }

        public void agregarTrabajoAccessLevel(StateObject v_stateobject, string v_accessLevel)
        {
            string dataStringEncriptado = Encriptar_Datos.Encriptar.encriptar(v_accessLevel, communicationSystem.ClaveEncriptar);
            byte[] dataBytes = Encoding.ASCII.GetBytes(dataStringEncriptado);

            int chunkSize = FIXED_HEADER_LENGTH + dataBytes.Length;

            string header = "ACCESSLVL:SIZE:" + chunkSize.ToString();
            // Nuevo: Encriptado de datos.
            string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
            headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;
            string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

            byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

            byte[] dataToSend = new byte[finalHeader.Length + dataBytes.Length];
            System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
            System.Buffer.BlockCopy(dataBytes, 0, dataToSend, headerBytedata.Length, dataBytes.Length);

            JobToSendToClient j = new JobToSendToClient();

            jobCounter++;
            j.ID = jobCounter;
            j.header = headerEncriptado;                // Usa el encriptado.
            j.byteData = dataToSend;
            
            v_stateobject.pendingJobs.Enqueue(j);
            v_stateobject.readyToSend.Set();        // Le aviso al semaforo que hay cosas encoladas para mandar.

            loguearString(v_stateobject.HHID + "-ACCESSLEVEL: " + v_accessLevel,TiposLOG.HH);
            
        }

        /// <summary>
        /// Evento lanzado por el socket que agrega un JOB de enviar una Imagen a la cola de pendingJobs.
        /// En el parametro TARJETA viene el numero de tarjeta cuya imagen hay que enviar.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="v_eventArgs"></param>
        void addSendImageJob(object sender, stringEventArgs v_eventArgs)
        {
            try
            {
                string tarjeta = v_eventArgs.textData["TARJETA"];         // La tarjeta cuya imagen hay que enviar

                int idEmp = mainApp.DataManager.buscarEmpleadoxTarjeta(tarjeta).Id;

                Dictionary<int, Employee> listaEmpleados = mainApp.DataManager.getListaEmpleados();

                if (listaEmpleados.ContainsKey(idEmp))
                {
                    if (listaEmpleados[idEmp].hasImage())
                    {
                        byte[] imageBytes = listaEmpleados[idEmp].getImage();
                        int chunkSize = FIXED_HEADER_LENGTH + imageBytes.Length;
                        int imageVersion = listaEmpleados[idEmp].imageVersion;

                        //string header = "TYPE:IMAGE,SIZE:" + chunkSize.ToString() + ",TARJETA:" + tarjeta + ",IMGVER:" + imageVersion.ToString().Trim();
                        string header = "TYPE:IMAGE,SIZE:" + chunkSize.ToString() + ",TARJETA:" + tarjeta + ",IMGVER:" + imageVersion.ToString().Trim() + ",PERSONID:" +listaEmpleados[idEmp].PersonID;

                        // Nuevo: Encriptado de datos.
                        string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
                        headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

                        string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);
                        byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

                        byte[] dataToSend = new byte[finalHeader.Length + imageBytes.Length];
                        System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
                        System.Buffer.BlockCopy(imageBytes, 0, dataToSend, headerBytedata.Length, imageBytes.Length);

                        JobToSendToClient j = new JobToSendToClient();

                        jobCounter++;
                        j.ID = jobCounter;
                        j.header = headerEncriptado;
                        j.byteData = dataToSend;

                        v_eventArgs.stateObject.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
                        v_eventArgs.stateObject.readyToSend.Set();
                        loguearString(v_eventArgs.stateObject.HHID + "- TRABAJO SEND IMAGE: " + header, TiposLOG.HH);

                    }
                    else loguearString("No hay imagen asociada a la tarjeta " + tarjeta, TiposLOG.HH);
                }
                else
                {
                    //lstStatus.Items.Add("ERROR: la tarjeta NO EXISTE");
                }
            }
            catch (Exception e) { loguearString("Excepcion enviando imagen: " + e.Message, TiposLOG.HH); }
        }

        // Acualiza los datos en funcion de un evento de visita
        void actualizeVisit(object sneder, byteArrayEventArgs e)
        {
         
            Visita nuevaVisita = new Visita(e.textData["HHID"], e.textData["NOMBRE"], e.textData["APELLIDO"], e.textData["DOCUMENTO"], e.textData["EMPRESA"], e.textData["TARJETA"], e.textData["LATITUD"], e.textData["LONGITUD"], e.textData["HORA"], e.byteData, (TiposAcceso)Enum.Parse(typeof(TiposAcceso), e.textData["TIPOACCESO"]),int.Parse(e.textData["ORGID"]));
            if (!mainApp.DataManager.existeVisita(nuevaVisita))
            {
               

                mainApp.DataManager.addVisita(nuevaVisita);

                actualizeListViewVisitas = true;
            }
            addConfirmVisitJob(e);
        }

        void actualizeAcccess(object sender, byteArrayEventArgs e)
        {
            // Dar de alta el acceso en la lista de accesos
            Dictionary<int,Acceso> listaAccesos = mainApp.DataManager.getListaAccesos();

            string tarjeta = e.textData["TARJETA"];

            Employee emp = mainApp.DataManager.buscarEmpleadoxTarjeta(tarjeta);

            if (emp != null)
            {
                int idEmpleado = emp.Id;

                Acceso nuevoAcceso = new Acceso(idEmpleado, e.textData["HHID"], e.textData["TARJETA"], e.textData["LATITUD"], e.textData["LONGITUD"], e.textData["HORA"], e.byteData, (TiposAcceso)Enum.Parse(typeof(TiposAcceso), e.textData["TIPOACCESO"]));
                if (!mainApp.DataManager.existeAcceso(nuevoAcceso))
                {
                    mainApp.DataManager.addAcceso(nuevoAcceso);
                    actualizeListView = true;
                }
            }
            addConfirmJob(e);
        }

        private void addConfirmJob(byteArrayEventArgs e)
        {
            int chunkSize = FIXED_HEADER_LENGTH;
            string header = "TYPE:CONFIRM,SIZE:" + chunkSize.ToString() + ",ID:" + e.textData["ID"];
            
            // Nuevo: Encriptado de datos.
            string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
            headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

            string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

            byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

            byte[] dataToSend = new byte[finalHeader.Length];
            System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);
            
            JobToSendToClient j = new JobToSendToClient();

            jobCounter++;
            j.ID = jobCounter;
            j.header = headerEncriptado;
            j.byteData = dataToSend;

            e.stateObject.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
            e.stateObject.readyToSend.Set();

            loguearString(e.stateObject.HHID + "- Agregado trabajo de CONFIRMACION con ID: " + e.textData["ID"],TiposLOG.HH);
        }

        private void addConfirmVisitJob(byteArrayEventArgs e)
        {
            int chunkSize = FIXED_HEADER_LENGTH;
            string header = "TYPE:CONFIRMVISIT,SIZE:" + chunkSize.ToString() + ",ID:" + e.textData["ID"];

            // Nuevo: Encriptado de datos.
            string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
            headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;

            string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);
            byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

            byte[] dataToSend = new byte[finalHeader.Length];
            System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);

            JobToSendToClient j = new JobToSendToClient();

            jobCounter++;
            j.ID = jobCounter;
            j.header = headerEncriptado;
            j.byteData = dataToSend;

            e.stateObject.pendingJobs.Enqueue(j);     // Nuevo: un pendingjobs por cada cliente.
            e.stateObject.readyToSend.Set();
            loguearString(e.stateObject.HHID + "- Agregado trabajo de CONFIRMACION de VISITA con ID: " + e.textData["ID"], TiposLOG.HH);

        }

        //void agregarLENELEvent(Acceso v_acceso)
        //{
        //    string IDLenelDevice = "";

        //    string accessString = @"//" + SystemConfiguration.LENELServer + "/root/OnGuard";

        //    ManagementClass baseClass = new ManagementClass(accessString, "Lnl_IncomingEvent", null);
        //    ManagementBaseObject inParams = baseClass.GetMethodParameters("SendIncomingEvent");
        //    inParams["Source"] = "MicroHH";
           
        //    inParams["BadgeID"] = traducirLENEL(v_acceso.Tarjeta);
        //    inParams["Description"] = "Acceso desde device";

        //    IDLenelDevice = v_acceso.HHID;

        //    if (v_acceso.tipoAcceso != TiposAcceso.INVALIDO)
        //    {
        //        inParams["isAccessGrant"] = true;
        //    }
        //    else
        //    {
        //        inParams["isAccessDeny"] = true;
        //        IDLenelDevice = IDLenelDevice + "Entrada";
        //    }

          

        //    if (v_acceso.tipoAcceso == TiposAcceso.Entrada)
        //    {
        //        IDLenelDevice = IDLenelDevice + "Entrada";
        //    }
        //    if (v_acceso.tipoAcceso == TiposAcceso.Salida)
        //    {
        //        IDLenelDevice = IDLenelDevice + "Salida";
        //    }

        //    inParams["Device"] = IDLenelDevice;

        //    try
        //    {
        //        ManagementBaseObject outParams = baseClass.InvokeMethod("SendIncomingEvent", inParams, null);
        //    }
        //    catch (Exception e)
        //    {
        //        loguearString("ERROR al Enviar Evento al LENEL: " + e.Message);
        //        LOGToFile.doLOGExcepciones("ERROR al Enviar Evento al LENEL: " + e.Message);
        //    }
        //}

        /// <summary>
        /// Traduccion de una badge con hexadecimales compatible con LENEL
        /// </summary>
        /// <param name="v_tarjeta"></param>
        /// <returns></returns>
        public string traducirLENEL(string v_tarjeta)
        {
            string res = "";

            if (v_tarjeta.Length > 10)
            {
                v_tarjeta = v_tarjeta.Substring(v_tarjeta.Length - 10, 10);
            }

            for (int i = 0; i < v_tarjeta.Length; i++)
            {
                char r = v_tarjeta[i];

                if ((r >= '0') && (r <= '9'))
                {
                    res = res + r;
                }
            }
            return res;

        }

        /// <summary>
        /// Handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void agregarItemLOG(object sender, stringEventArgs e)
        {
            loguearString(e.text, e.LOGTYPE);
            //try
            //{
            //    LOGInformation.Enqueue(e);     // MUTEX ACA??
            //}
            //catch (Exception ex)
            //{
            //}

        }


        public void confirmarLOG(object sender, stringEventArgs e)
        {
            int chunkSize = FIXED_HEADER_LENGTH;
            string header = "TYPE:BORRARLOG,NAME:" + e.textData["LOGNAME"];

            // Nuevo: Encriptado de datos.
            string headerEncriptado = Encriptar_Datos.Encriptar.encriptar(header, communicationSystem.ClaveEncriptar);
            headerEncriptado = "SIZE:" + chunkSize.ToString() + ",DATA:" + headerEncriptado;
            string finalHeader = headerEncriptado.PadRight(FIXED_HEADER_LENGTH);

            byte[] headerBytedata = Encoding.ASCII.GetBytes(finalHeader);

            byte[] dataToSend = new byte[finalHeader.Length];
            System.Buffer.BlockCopy(headerBytedata, 0, dataToSend, 0, headerBytedata.Length);

            JobToSendToClient j = new JobToSendToClient();

            jobCounter++;
            j.ID = jobCounter;
            j.header = headerEncriptado;                // Usa el encriptado.
            j.byteData = dataToSend;

            // Nuevo: un pendingJobs por cada stateobject de cada cliente.
            e.stateObject.pendingJobs.Enqueue(j);
            e.stateObject.readyToSend.Set();

        }

        /// <summary>
        /// Loguea el dato al primer archivo de LOG
        /// </summary>
        /// <param name="texto"></param>
        private void loguearString(string texto, TiposLOG v_tipoLOG)
        {
            if (mainForm != null)
            {
                mainForm.actualizarLOG(texto, v_tipoLOG);
            }
            else
            {
                StaticTools.loguearString(v_tipoLOG.ToString() + " - " + texto);
            }


            
        }

        public static void actualizarListaDevicesInForm()
        {
            if (mainFormCliente != null)
            {
                mainFormCliente.actualizarConnectedDevices();
            }
        }

       

    }
}
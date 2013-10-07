using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using Encriptar_Datos;


namespace ServerComunicacionesALUTEL
{
    /// <summary>
    /// La clase SocketServer representa al SERVER COMPLETO. 
    /// Implementa toda la logica de atnecion y proceso de datos de recepcion y envio 
    /// para TODOS LOS CLIENTES.
    /// </summary>
    public class SocketServer
    {
        public const int MAXPENDIGCONNECTIONS = 100;    // Maxima Cantidad de conexiones simultáneas.
        public const int FIXED_HEADER_LENGTH = 512;
        public byte[] ACK = { 65 };           // 65 = A
        
        //public bool reBind = false;          // Indica si se deben reBindear los sockets de escucha. Se setea internamente y es consumido por un timer externo que hacer un rebind y reconecta el server.

        public static ManualResetEvent allDone = new ManualResetEvent(false);  // Semaforo para permitir una nueva conexion

        // Diccionario con los stateObjects de los clientes, numerado a medida que se conectan los clientes.
        public Dictionary<string, StateObject> stateObjectsClients = new Dictionary<string, StateObject>();

        public bool finishListen = false;                                      // al hacer el set se genera otra conexion, solo si finishListen no esta en false

        public static int numClient =0;                                        // Contador de numero de cliente. 

        public int puertoSend = 0;         // Puerto usado por socketSend
        public int puertoReceive = 0;      // Puerto usado por socketReceive
        //public int puertoAliveSend = 0;     // Puerto usado por socketAliveSend
        //public int puertoAliveReceive = 0;  // Puerto usado por socketAliveReceive

        public bool finishSending = false;  // Flag para generar un fin del thread de envio.
        public bool finishReceiving = false;   // Flag para generar un fin del thread de recepcion.

        // EndPoint y Socket de escucha por el puerto puertoReceive. Atiende todas las conexiones
        IPEndPoint localEndPointReceive;
        Socket listenerPuertoReceive;

        // EndPoint y Socket de escucha por el puerto puertoSend. Atiende todas las conexiones
        IPEndPoint localEndPointSend;
        Socket listenerPuertoSend;

        // EndPoint y Socket de escucha por el puerto puertoAliveSend. Atiende todas las conexiones
        //IPEndPoint localEndPointAliveSend;
        //Socket listenerPuertoAliveSend;

        // EndPoint y Socket de escucha por el puerto puertoAliveReceive. Atiende todas las conexiones
        //IPEndPoint localEndPointAliveReceive;
        //Socket listenerPuertoAliveReceive;

        public Socket socketToReceive=null;          // Socket usado para recibir datos desde el HandHeld. Esta bindeado al puerto 11000
        public Socket socketToSend=null;             // Socket usado para enviar datos. Esta bindeado al puerto 11001

        //public Socket socketAliveSend=null;          // Socket usado para enviar el ACK al cliente.
        //public Socket socketAliveReceive=null;       // Socket usado para recibir ACK desde el cliente.

        StateObject sessionState;               // Stateobject del cliente. Se crea uno nuevo en cada cliente y se transfiere a los threads asociados

        // Eventos para que el socket haga un rise externo.
//        public event StringEventHandler beginEmpList;
        public event StringEventHandler getEmployeeList;
        public event StringEventHandler getImage;
        public event byteArrayEventHandler actualizeAccess;
        public event byteArrayEventHandler actualizeVisit;
        public event StringEventHandler actualizeGPSData;
        //public event StringEventHandler responderDUMMY;

        public event StringEventHandler deleteEmp;
        public event StringEventHandler addEmp;
        public event byteArrayEventHandler sendEmpxVersion;
        public event byteArrayEventHandler enviarListaImagenes;
        public event StringEventHandler enviarVersionImagenes;
        public event StringEventHandler enviarMasImagenes;
        public event StringEventHandler enviarDummy;

        public event StringEventHandler actualizarLOG;
        public event StringEventHandler confirmarLOG;


        //Expresiones regulares para el reconocimiento de mensajes
        Regex GPSdata = new Regex(@"TYPE:GPS,HHID:(.*),SIZE:(.*),LAT:(.*),LONG:(.*),HORA:(.*),HEADING:(.*),SPEED:(.*)");
        Regex getImageHeader = new Regex(@"TYPE:GETIMAGE,HHID:(.*),SIZE:(.*),TARJETA:(.*)");
        Regex getEmpListHeader = new Regex(@"TYPE:GETEMPLIST,HHID:(.*),SIZE:(.*)");
        Regex accessHeader = new Regex(@"TYPE:ACCESS,HHID:(.*),SIZE:(.*),TARJETA:(.*),LAT:(.*),LONG:(.*),HORA:(.*),IMAGE:(.*),TIPOACCESO:(.*),ID:(.*)");
        //Regex enddata = new Regex(@"TYPE:ENDDATA,HHID:(.*),SIZE:(.*)");
        Regex visitHeader = new Regex("TYPE:VISITA,HHID:(.*),SIZE:(.*),NOMBRE:(.*),APELLIDO:(.*),DOCUMENTO:(.*),EMPRESA:(.*),TARJETA:(.*),LAT:(.*),LONG:(.*),HORA:(.*),IMAGE:(.*),TIPOACCESO:(.*),ID:(.*)");
        Regex dummyHeader = new Regex(@"TYPE:DUMMY,HHID:(.*)");

        Regex EmpxVersionHeader = new Regex(@"TYPE:EMPXVER,HHID:(.*),SIZE:(.*)");//,ID:(.*)");
        Regex DeleteEmpHeader = new Regex(@"TYPE:DELEMP,HHID:(.*),SIZE:(.*),BADGE:(.*)");
        Regex AddEmpHeader = new Regex(@"TYPE:ADDEMP,HHID:(.*),SIZE:(.*),BADGE:(.*)");
        Regex ImgListHeader = new Regex(@"TYPE:IMGLIST,HHID:(.*),SIZE:(.*)");
        Regex ImgVersionHeader = new Regex(@"TYPE:VERIMGS,HHID:(.*),SIZE:(.*)");
        Regex MasImgsHeader = new Regex(@"TYPE:MASIMGS,HHID:(.*)");
        Regex FinSyncHeader = new Regex(@"TYPE:FINSYNC");

        Regex DatosEncripHeader = new Regex(@"SIZE:(.*),DATA:(.*)");

        Regex LOGFile = new Regex(@"TYPE:LOG,HHID:(.*),SIZE:(.*),LOGNAME:(.*)");



       // Regex beginEmpListHeader = new Regex(@"TYPE:BEGINEMPLIST");


        public LAYERHHGPS HHGPSLayer;
        Aplicacion mainApp;


        public SocketServer(int v_puerto_Send, int v_puerto_Receive,  string v_AlutrackIP, int v_AlutrackPort, Aplicacion v_app)
        {
            puertoSend = v_puerto_Send;
            puertoReceive = v_puerto_Receive;
            //puertoAliveSend = v_puerto_AliveSend;
            //puertoAliveReceive = v_puerto_AliveReceive;
            mainApp = v_app;
            HHGPSLayer = new LAYERHHGPS(mainApp);

        }


        private void CreateListenSockets()
        {
            // EndPoint y Socket de escucha por el puerto puertoReceive
            localEndPointReceive = new IPEndPoint(IPAddress.Any, puertoReceive);
            listenerPuertoReceive = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // EndPoint y Socket de escucha por el puerto puertoSend
            localEndPointSend = new IPEndPoint(IPAddress.Any, puertoSend);
            listenerPuertoSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // EndPoint y Socket de escucha por el puerto puertoAliveSend
            //localEndPointAliveSend = new IPEndPoint(IPAddress.Any, puertoAliveSend);
            //listenerPuertoAliveSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // EndPoint y Socket de escucha por el puerto puertoAliveReceive
            //localEndPointAliveReceive = new IPEndPoint(IPAddress.Any, puertoAliveReceive);
           // listenerPuertoAliveReceive = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void BindListenSockets()
        {
            listenerPuertoReceive.Bind(localEndPointReceive);
            listenerPuertoReceive.Listen(MAXPENDIGCONNECTIONS);

            listenerPuertoSend.Bind(localEndPointSend);
            listenerPuertoSend.Listen(MAXPENDIGCONNECTIONS);

            //listenerPuertoAliveReceive.Bind(localEndPointAliveReceive);
           // listenerPuertoAliveReceive.Listen(MAXPENDIGCONNECTIONS);

            //listenerPuertoAliveSend.Bind(localEndPointAliveSend);
            //listenerPuertoAliveSend.Listen(MAXPENDIGCONNECTIONS);
            loguearString("BindListenSockets() - Todos los sockets de LISTEN han sido BINDEADOS");
        }

        /// <summary>
        /// 
        /// </summary>
        private void shutDownAndCloseListenSockets()
        {

            try
            {
                listenerPuertoReceive.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // No hacer nada... solo quiero cerrar todo lo que pueda...
            }
            
            try
            {
                listenerPuertoReceive.Close();
            }
            catch (Exception)
            {
                // No hacer nada... solo quiero cerrar todo lo que pueda...
            }

            try
            {
                listenerPuertoSend.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // No hacer nada... solo quiero cerrar todo lo que pueda...
            }
            try
            {
                listenerPuertoSend.Close();
            }
            catch (Exception)
            {
               // No hacer nada... solo quiero cerrar todo lo que pueda...
            }

            try
            {
                //listenerPuertoAliveReceive.Shutdown(SocketShutdown.Both);
            }

            catch (Exception)
            {
                // No hacer nada... solo quiero cerrar todo lo que pueda...
            }

            try
            {
                //listenerPuertoAliveReceive.Close();
            }
            catch (Exception)
            {
                // No hacer nada... solo quiero cerrar todo lo que pueda...
            }

            try
            {
                //listenerPuertoAliveSend.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // No hacer nada... solo quiero cerrar todo lo que pueda...
            }

            try
            {
                //listenerPuertoAliveSend.Close();
            }
            catch (Exception)
            {
                // No hacer nada... solo quiero cerrar todo lo que pueda...
            }

            loguearString("Todos los sockets de LISTEN han sido cerrados");
        }

        // Devuelve true si el HH identificado tiene un stateObject asociado.
        public bool isHandHeldOnLine(string v_hhID)
        {
            bool res = false;
            try
            {
                StaticTools.obtenerMutex_StateObjectClients();
                foreach (StateObject s in stateObjectsClients.Values)
                {
                    if (s.HHID == v_hhID)
                    {
                        res = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en ishandHeldOnLine():" + ex.Message); 
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }
            return res;
        }


        /// <summary>
        /// Se ejecutará en un thread propio.
        /// El ORDEN de conexiones desde el device es:
        ///  - puertoAliveSend
        ///  - puertoAliveReceive
        ///  - puertoSend
        ///  - puertoReceive
        /// </summary>
        public void StartListening()
        {
            finishListen = false;           // Flag para indicar la finalizacion de la escucha y por lo tanto del server.

            CreateListenSockets();          // Crea los sockets que van a hacer BeginAccept();

            try
            {
                BindListenSockets();        // Hace Bind de los sockets que aceptan conexiones.
            }
            catch (Exception e)
            {
                loguearString2("Excepción en BIND inicial de los sockets. Server NO escuchando " + e.Message);
                finishListen = true;
                StaticTools.reBind = true;
            }
            try
            {
                while (!finishListen)
                {
                    
                    loguearString("Esperando por una nueva conexión");
                    Thread.Sleep(2000);  // 2 segundos antes de aceptar efectivamente otra conexion.
                    // Reseteo el semaforo
                    allDone.Reset();

                    //*****DEBUG

                    //listenerPuertoAliveSend.BeginAccept(new AsyncCallback(AcceptCallbackAliveSend), listenerPuertoAliveSend);

                    listenerPuertoReceive.BeginAccept(new AsyncCallback(AcceptCallbackReceive), listenerPuertoReceive);
                    //****DEBUG

                    // Bloqueante. Espera a que la segunda conexion sea hecha para que acepte otra...
                    allDone.WaitOne();
                }

                loguearString("Server Desconectado. Salida normal del Thread");

                // TODO: Liberar sockets, abortar Threads, etc.
               // closeAndShutDownListenSockets();
            }
            catch (Exception e)
            {
                if (finishListen)
                {
                    loguearString("Server Desconectado manualmente. ");
                    allDone.Set();

                }
                else   // salir del thread y forzar un REBIND de los sockets externamente.
                {
                    loguearString2("Excepcion en THREAD STARTLISTENING. Salida del thread. " + e.Message);
                    finishListen = true;
                    allDone.Set();
                    StaticTools.reBind = true;
                }
            }
        }

        // El cliente se acaba de conectar al puerto puertoAliveSend
        //public void AcceptCallbackAliveSend(IAsyncResult ar)
        //{

        //    // Get the socket that handles the client request.
        //    Socket listener = (Socket)ar.AsyncState;
        //    if (!finishListen)
        //    {
        //        try
        //        {
        //            socketAliveSend = listener.EndAccept(ar);        // Crea un nuevo socket comunicarse con el cliente

        //           // loguearString("Conectado al puerto ACK de ALIVESEND: " + puertoAliveSend.ToString());
        //            // El cliente acaba de conectarse por el puerto PORT_ALIVE_SEND.
        //            // Ahora sigo con la secuencia de conexion, pero antes desbloqueo para permitir aceptar nuevas conexiones.
        //            //allDone.Set();

        //            listenerPuertoAliveReceive.BeginAccept(new AsyncCallback(AcceptCallbackAliveReceive), listenerPuertoAliveReceive);
        //        }
        //        catch (Exception e)
        //        {
        //            loguearString2(" Excepción en AcceptCallbackAliveSend: " + e.Message);
        //            reBind = true;// fuerzo un rebind de los sockets de escucha
        //            finishListen = true;
        //            allDone.Set();  // desbloqueo para que salga del thread y luego sea reactivado con el rebind.
        //        }
        //    }
        //}

        //// El cliente se acaba de conectar al puerto puertoAliveReceive
        //public void AcceptCallbackAliveReceive(IAsyncResult ar)
        //{
        //    // Get the socket that handles the client request.
        //    Socket listener = (Socket)ar.AsyncState;
        //    if (!finishListen)
        //    {
        //        try
        //        {
        //            socketAliveReceive = listener.EndAccept(ar);        // Crea un nuevo socket comunicarse con el cliente

        //           // loguearString("Conectado a puerto ACK de ALIVERECEIVE: " + puertoAliveReceive.ToString());
        //            // El cliente acaba de conectarse por el puerto PORT_ALIVE_RECEIVE.
        //            listenerPuertoReceive.BeginAccept(new AsyncCallback(AcceptCallbackReceive), listenerPuertoReceive);
        //        }
        //        catch (Exception e)
        //        {
        //            loguearString2(" ERROR en AcceptCallbackAliveReceive con ACCEPT: " + e.Message);
        //            reBind = true;
        //            finishListen = true;
        //            allDone.Set();  // desbloqueo para que salga del thread y luego sea reactivado con el rebind.
        //        }
        //    }
        //}

        // El Client se acaba de conectar al socket por el puerto 11000
        public void AcceptCallbackReceive(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            if (!finishListen)
            {
                try
                {
                    socketToReceive = listener.EndAccept(ar);        // Crea un nuevo socket comunicarse con el cliente que acaba de conectarse. 

                   // loguearString("Conectado a puerto de RECEIVE: " + puertoReceive.ToString());

                    listenerPuertoSend.BeginAccept(new AsyncCallback(AcceptCallbackSend), listenerPuertoSend);
                }
                catch (Exception e)
                {
                    loguearString2("Error en AcceptCallbackReceive: " + e.Message);
                    StaticTools.reBind = true;
                    finishListen = true;
                    allDone.Set();  // desbloqueo para que salga del thread y luego sea reactivado con el rebind.
                }
            }
        }

        /// <summary>
        /// El cliente se acaba de conectar a todos los puertos.
        /// OJO!!! ACA: ¿Puede pasar que UN CLIENTE DIFERENTE AL QUE SE CONECTO AL PRIMER PUERTO SE CONECTE AQUI??? 
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptCallbackSend(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
             
            if (!finishListen)
            {
                try
                {
                    socketToSend = listener.EndAccept(ar);        // Crea un nuevo socket comunicarse con el cliente que acaba de conectarse. 

                   // loguearString("Contectado al puerto SEND: " + puertoSend.ToString());

                    loguearString("EXITO: Conectado a todos los puertos desde: " + socketToSend.RemoteEndPoint.ToString());

                    // Lanza los threads de esta sesion, creando un StateObject propio
                    StartSendAndReceiveThreads(socketToReceive, socketToSend);
                    allDone.Set();  // Aceptar nuevas conexiones
                }
                catch (Exception e)
                {
                    loguearString2(" Error en AcceptCallbackSend: " +  e.Message);
                    StaticTools.reBind = true;
                    finishListen = true;
                    allDone.Set();  // desbloqueo para que salga del thread y luego sea reactivado con el rebind.
                }
            }
        }

        /// <summary>
        /// Crea un nuevo StateObject correspondiente a esta nueva sesion.
        /// Lanza los Threads de send y receive asociados al Cliente que se conecto.
        /// </summary>
        public void StartSendAndReceiveThreads(Socket v_socketToReceive, Socket v_socketToSend)
        {
        //    numClient++;
              sessionState = new StateObject();
        //    loguearString("Comenzando THREADS del nuevo cliente");

        //    // Agrega el stateObject a la lista de stateobjects del sistema.
        //    try
        //    {
        //        StaticTools.obtenerMutex_StateObjectClients();
        //        stateObjectsClients.Add(numClient, sessionState);
        //    }
        //    catch (Exception ex)
        //    {
        //        loguearString("Excepcion en StartSendAndReceiveThreads-MUTEX:" + ex.Message);
        //    }
        //    finally
        //    {
        //        StaticTools.liberarMutex_StateObjectClients();
        //    }

            communicationSystem.actualizarListaDevicesInForm();   // Metodo estatico para actualizar derecho el form...

            socketToReceive.ReceiveBufferSize = 300000;
            socketToReceive.SendBufferSize = 300000;
            sessionState.socketToReceive = v_socketToReceive;

            socketToSend.ReceiveBufferSize = 300000;
            socketToSend.SendBufferSize = 300000;
            sessionState.socketToSend = v_socketToSend;

           
            sessionState.numeroCliente = numClient;

//            loguearString("Lanzando THREAD ThreadToReceive");
            sessionState.t_receive = new Thread(ThreadToReceive);
            sessionState.t_receive.Name = "Thread Receive" + sessionState.t_receive.ManagedThreadId.ToString();
            sessionState.t_receive.Start();
  //          loguearString("ThreadToReceive lanzado, esperando IsAlive");
            //while (!sessionState.t_receive.IsAlive);
            Thread.Sleep(100);
    //        loguearString("ThreadToReceive IsAlive!!!");

//            loguearString("Lanzando THREAD ThreadToSend");
            sessionState.t_send = new Thread(ThreadToSend);
            sessionState.t_send.Name = "Thread Send" + sessionState.t_send.ManagedThreadId.ToString();
            sessionState.t_send.Start();
//            loguearString("ThreadToSend lanzado, esperando IsAlive");
            //while (!sessionState.t_send.IsAlive) ;
            Thread.Sleep(100);
  //          loguearString("ThreadToSend IsAlive!!!");

            sessionState.t_dummySend = new Thread(ThreadSendDummy);
            sessionState.t_dummySend.Name = "Thread SendDUMMY" + sessionState.t_dummySend.ManagedThreadId.ToString();
            sessionState.t_dummySend.Start();
            Thread.Sleep(100);


            //**********************************DEBUG//
//            loguearString("Lanzando THREAD ThreadAliveSend");
 //           sessionState.t_aliveSend = new Thread(ThreadAliveSend);               // Para chequear el mantenimiento de la conexion
 //           sessionState.t_aliveSend.Name = "Thread Alive Send" + sessionState.t_aliveSend.ManagedThreadId.ToString(); ;
 //           sessionState.t_aliveSend.Start();
  //          loguearString("ThreadAliveSend lanzado, esperando IsAlive");
            //while (!sessionState.t_aliveSend.IsAlive) ;
            Thread.Sleep(100);
  //          loguearString("ThreadAliveSend IsAlive!!!");
            //***************************FIN DEBUG //

//            loguearString("Lanzando THREAD ThreadAliveReceive");
  //          sessionState.t_aliveReceive = new Thread(ThreadAliveReceive);         // Para chequear el mantenimiento de la conexion
  //          sessionState.t_aliveReceive.Name = "Thread Alive Receive" + sessionState.t_aliveReceive.ManagedThreadId.ToString();
  //          sessionState.t_aliveReceive.Start();
            //loguearString("ThreadAliveReceive lanzado, esperando IsAlive");
            //while (!sessionState.t_aliveReceive.IsAlive) ;
            Thread.Sleep(100);
//            loguearString("ThreadAliveReceive IsAlive!!!");


            loguearString("TODOS LOS THREADS HAN SIDO LANZADOS y estan vivos");
            // Ya lanzó los threads asociados. Listo.
        }

        /// <summary>
        /// Thread asociado al socket aliveSend para enviar ACK hacia el cliente.
        /// </summary>
        //void ThreadAliveSend()
        //{
        //    StateObject StateClient = sessionState;  // Copio la referencia al StateObject del cliente.
        //    try
        //    {
        //        sessionState.sendACK.Reset();
        //        loguearString("COMENZANDO Thread ALIVE SEND");
        //        while (!StateClient.abortFlag)
        //        {
        //            sessionState.sendACK.WaitOne();
        //            sessionState.sendACK.Reset();
        //            if (!StateClient.abortFlag)            // Por si el ThreadAliveReceive mandó terminar todo
        //            {
        //                StateClient.socketAliveSend.SendTimeout = 30000;  // 30 segundos antes de lanzar una excepcion.
        //                try
        //                {
        //                    StateClient.socketAliveSend.Send(ACK, 1, SocketFlags.None);
        //                    LOGToFile.doLOG("ACK enviado nº" + StateClient.contadorACKSend.ToString() + " de " + StateClient.HHID+" " + StateClient.numeroCliente.ToString());
        //                    StateClient.contadorACKSend++;
        //                    //loguearString("ACK enviado");
        //                }
        //                catch (TimeoutException e)
        //                {
        //                    //loguearString2("Timeout en ThreadAliveSEND: " + e.Message + " - AbortFLAG = true");
        //                    setAbortFlag("Timeout en ThreadAliveSEND" + e.Message, StateClient);
        //                    //StateClient.abortFlag = true;

        //                }
        //                catch (Exception e)
        //                {
        //                   // loguearString2("Excepcion en ThreadAliveSEND: " + e.Message + " - AbortFLAG = true");
        //                    setAbortFlag("Excepcion en ThreadAliveSEND: " + e.Message, StateClient);
        //                   // StateClient.abortFlag = true;
        //                }
        //            }
        //        }
        //        loguearString("ABORTFLAG: Saliendo de ThreadAliveSEND. Abortando todos los threads");

        //        EliminarThreadsySockets(StateClient, StateClient.t_aliveSend);
        //    }
        //    catch (Exception)
        //    {
        //        loguearString2("Excepcion en THREAD ThreadAliveSend. Abortando todos los threads");
        //        EliminarThreadsySockets(StateClient, StateClient.t_aliveSend);
        //    }
        //}

        ///// <summary>
        ///// Thread asociado al socket aliveReceive para recibir ACK desde el cliente
        ///// </summary>
        //void ThreadAliveReceive()
        //{
        //    StateObject StateClient = sessionState;  // Copio la referencia al StateObject del cliente.
        //   // try
        //   // {
        //        byte[] ACKBuffer = new byte[1];     // El ACK es un solo byte.
        //        loguearString("COMENZANDO Thread ALIVE RECEIVE");

        //        while (!StateClient.abortFlag)
        //        {
        //            StateClient.socketAliveReceive.ReceiveTimeout = 50000;  // 50 segundos antes de lanzar una excepcion.
        //            try
        //            {
        //                ACKBuffer[0] = 0;
        //                StateClient.socketAliveReceive.Receive(ACKBuffer, 1, SocketFlags.None);
        //                LOGToFile.doLOG("ACK recibido nº" + StateClient.contadorACKReceive.ToString() + " de " + StateClient.HHID + " " + StateClient.numeroCliente.ToString());
        //                // Si hay un cero en la recepcion quiere decir que receive volvio sin datos. 
        //                if (ACKBuffer[0] != 0)
        //                {
        //                    StateClient.sendACK.Set();         // Activo el thread de SendAlive para indicar que se recibio el ACK
        //                    Thread.Sleep(200);                  // Una pausita para no saturar...
        //                    StateClient.contadorACKReceive++;
        //                }
        //                else Thread.Sleep(200);
        //                //{
        //                //    if (StateClient.socketAliveReceive.Available == 0)
        //                //    {
        //                //        //loguearString2("ThreadAliveReceive recibió 0 Bytes y no hay Available en la Network. Abortando TODO");
        //                //        //StateClient.abortFlag = true;
        //                //        //// Va a eliminar Threads y sockets al salir de este bloque de ejecucion
        //                //        //StateClient.sendACK.Set(); // Libero al SendAlive para que salga.
        //                //    }
        //                //}
        //            }
        //            catch (TimeoutException e)
        //            {
        //               //loguearString2("Timeout en ThreadAliveRECEIVE: " + e.Message + " - AbortFLAG = true");
        //                setAbortFlag("TIMEOUT en ThreadAliveRECEIVE: " + e.Message, StateClient);
        //                //StateClient.abortFlag = true;
        //            }
        //            catch (Exception e)
        //            {
        //                //loguearString2("Excepcion en ThreadAliveReceive: " + e.Message + " ABORTFLAG = true");
        //                setAbortFlag("Excepcion en ThreadAliveReceive:  " + e.Message, StateClient);
        //                //StateClient.abortFlag = true;

        //                // Va a eliminar Threads y sockets al salir de este bloque de ejecucion
        //                StateClient.sendACK.Set(); // Libero al SendAlive para que salga.
        //            }
        //        }

        //        loguearString("ABORTFLAG: Saliendo de ThreadAliveReceive. Abortando todos los threads");
        //        EliminarThreadsySockets(StateClient, StateClient.t_aliveReceive);

        //    //}
        //    //catch (Exception e)
        //    //{
        //    //    //StateClient.abortFlag = true;
        //    //    setAbortFlag("Excepcion en ThreadAliveReceive:  " + e.Message, StateClient);



        //    //    StateClient.sendACK.Set(); // Libero al SendAlive para que salga.
        //    //    loguearString2("Excepcion en THREAD ThreadAliveReceive. Abortando todos los threads" + e.Message);
        //    //    EliminarThreadsySockets(StateClient, StateClient.t_aliveReceive);
        //    //}
        //}

        /// <summary>
        /// Thread asociado al socket para recibir datos desde el cliente.
        /// Como Beginreceive es no bloqueante y genera su propio thread, esta llamada es directa.
        /// </summary>
        public void ThreadToReceive()
        {
            StateObject StateClient = sessionState;  // Copio la referencia al StateObject del cliente.
            try
            {
                StateClient.socketToReceive.BeginReceive(StateClient.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(receiveCallback), StateClient);
            }
            catch (Exception e)
            {
                //loguearString2("Excepcion en ThreadToReceive de: " + StateClient.HHID + " Abortando todos los threads" + e.Message);
                //StateClient.abortFlag = true;
                setAbortFlag("Excepcion en ThreadToReceive de: " + StateClient.HHID + e.Message, StateClient);
                
                EliminarThreadsySockets(StateClient, sessionState.t_receive);
            }
        }


        public void ThreadSendDummy()
        {
            StateObject StateClient = sessionState;
            try
            {
                while (!StateClient.abortFlag)
                {

                    stringEventArgs retorno = new stringEventArgs("", StateClient);

                    this.enviarDummy(this, retorno);
                    
                    Thread.Sleep(30000);
                }
                loguearString("ABORT: Saliendo de ThreadSendDummy()");
                // Finalizo el thread: Libero los sockets, sus recursos y sus puertos.
                EliminarThreadsySockets(StateClient, sessionState.t_dummySend);
            }
            catch (Exception ex)
            {

                setAbortFlag("Excepcion en ThreadSendDummy. Abortando todos los threads. " + ex.Message, StateClient);

                EliminarThreadsySockets(StateClient, sessionState.t_dummySend);
            }

        }



        /// <summary>
        /// Thread asociado al socket para enviar datos. Utilizan un semaforo readyToSend para sincronizar los envios...
        /// </summary>
        public void ThreadToSend()
        {
            StateObject StateClient = sessionState;
            try
            {

                while (!StateClient.abortFlag)
                {
                    StateClient.readyToSend.WaitOne();                      // Bloqueo hasta que me avisen...
                   
                    if (!StateClient.abortFlag)
                    {
                        StateClient.readyToSend.Reset();
                        if (!StateClient.sendingData)  // solo si no esta enviando datos me pongo a enviar. Si ya esta enviando entonces no mando startSend nuevamente.
                        {
                            startSendSequence(StateClient);
                        }
                    }
                }
                loguearString("ABORT: Saliendo de ThreadToSend()");
                // Finalizo el thread: Libero los sockets, sus recursos y sus puertos.
                EliminarThreadsySockets(StateClient,sessionState.t_send);
            }
            catch (Exception ex)
            {
                //loguearString2("Excepcion en Thread ThreadToSend. Abortando todos los threads. " + ex.Message);
                //StateClient.abortFlag = true;

                setAbortFlag("Excepcion en ThreadToSend. Abortando todos los threads. " + ex.Message, StateClient);

                EliminarThreadsySockets(StateClient, sessionState.t_send);
            }
        }

        /// <summary>
        /// Thread asociado a la recepción de datos en el socket.
        /// Cada evento de recepción se encarga de lanzar nuevamente el beginReceive o continueReceive si no se recibieron todos los datos.
        /// Por ahora se asume que en una recepcion se reciben por lo menos FIXED_HEADER_LENGTH datos.
        /// </summary>
        /// <param name="ar"></param>
        public void receiveCallback(IAsyncResult ar)
        {
        
            // Obtiene el stateObject y, de él,  el socket asociado a la conexión que recibe el dato.
            StateObject StateClient = (StateObject)ar.AsyncState;
            Socket socketToClient = StateClient.socketToReceive;

            //loguearString(StateClient.HHID + " - Entro a receiveCallback");

            try
            {
                // Finaliza la lectura de datos desde el cliente
                int bytesRead = socketToClient.EndReceive(ar);

                if (bytesRead > 0)
                {
                    StateClient.memStream.Write(StateClient.buffer, 0, bytesRead);
                    //try
                    //{
                        ProcesarBuffer(StateClient);
                    //}
                    //catch (Exception e)
                    //{
                       // loguearString("Excepcion en ProcesarBuffer: " + e.Message + " Continuando...");
                    //}
                    if (!StateClient.abortFlag)
                    {
                        socketToClient.BeginReceive(StateClient.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(receiveCallback), StateClient); // Ojo: lanza un thread propio.
                    }
                }
                else
                {
                    //if (!StateClient.abortFlag)
                    //{
                    //    socketToClient.BeginReceive(StateClient.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(receiveCallback), StateClient); // Ojo: lanza un thread propio.
                    //}
                    //loguearString2("Recibio CERO bytes desde: " + StateClient.HHID + " Abortando todos los threads");
                    //StateClient.abortFlag = true;

                    setAbortFlag("Recibio CERO bytes desde: " + StateClient.HHID + " Abortando todos los threads", StateClient);

                    StateClient.readyToSend.Set();
                    EliminarThreadsySockets(StateClient, StateClient.t_receive);
                }
            }
            catch (Exception e)
            {
                //loguearString2("Excepcion en  ReceiveCallback de: " + StateClient.HHID + " Abortando todos los threads" + e.Message);
                //StateClient.abortFlag = true;

                setAbortFlag("Excepcion en  ReceiveCallback de: " + StateClient.HHID + " Abortando todos los threads" + e.Message,StateClient);
                
                StateClient.readyToSend.Set();
                StaticTools.reBind = true;
                EliminarThreadsySockets(StateClient, StateClient.t_receive);
            }
        }

        /// <summary>
        /// Procesa toda la informacion posible dentro de los datos contenidos en el MemoryStream.
        /// NOTA: Para sacar los datos del MemoryStream crea otro y copia los datos.
        /// </summary>
        /// <param name="v_state"></param>
        /// <returns></returns>
        private void ProcesarBuffer(StateObject v_state)
        {
            if (!v_state.hasHeader)  // No tiene el header
            {
                if (v_state.memStream.Length >= FIXED_HEADER_LENGTH)
                {
                    // Leo el Header y lo saco del MemoryStream
                    byte[] bytesHeader = new byte[FIXED_HEADER_LENGTH];

                    bytesHeader = ObtenerDatos(v_state, FIXED_HEADER_LENGTH,false);  // Obtiene el header pero no lo borra del memoryStream

                    int chunkSize = obtenerChunkSize(bytesHeader);

                    if (chunkSize > 0)
                    {
                        //v_state.chunkSizeTarget = chunkSize;  // Ojo: incluye el size del header.
                        //string header = Encoding.ASCII.GetString(bytesHeader, 0, FIXED_HEADER_LENGTH);
                        //v_state.hasHeader = true;
                        //v_state.actualHeader = header;
                        //ProcesarBuffer(v_state);              // OJO: RECURSION ACA...

                        v_state.chunkSizeTarget = chunkSize;  // Ojo: incluye el size del header.
                        string header = Encoding.ASCII.GetString(bytesHeader, 0, FIXED_HEADER_LENGTH);

                        // ACA DESENCRIPTAR LOS DATOS Y OBTENER EL HEADER ENCRIPTADO
                        header = DesencriptarHeader(header);
                        if (!header.Equals(string.Empty))
                        {
                            v_state.hasHeader = true;
                            v_state.actualHeader = header;
                            ProcesarBuffer(v_state);              // OJO: RECURSION ACA
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        string header = Encoding.ASCII.GetString(bytesHeader, 0, FIXED_HEADER_LENGTH);
                        //loguearString2("DATA CORRUPTION, HEADER NO RECONOCIDO: " + header + ". ABORTING...");
                        //v_state.abortFlag = true;
                        setAbortFlag("DATA CORRUPTION, HEADER NO RECONOCIDO: " + header + ". ABORTING...", v_state);

                        return;
                    }
                }
            }
            else                 // Procesamiento con el header ya reconocido
            {
                if (v_state.memStream.Length >=v_state.chunkSizeTarget)
                {
                    byte[] finalBuffer = new byte[v_state.chunkSizeTarget];
                    v_state.memStream.Position = 0;
                    finalBuffer = ObtenerDatos(v_state, v_state.chunkSizeTarget, true);     // saca TODO el chunk 
                    string header = v_state.actualHeader;
                    AddJobs(header, finalBuffer, v_state);
                    v_state.hasHeader = false;
                    ProcesarBuffer(v_state);            // OJO: Recursion ACA...
                }
            }
        }
        //
        /// <summary>
        /// AddJobs:  Reconoce de Headers y responde en consecuencia
        /// </summary>
        /// <param name="v_header"></param>
        /// <param name="v_byteData"></param>
        /// <param name="v_state"></param>
        public void AddJobs(string v_header, byte[] v_byteData, StateObject v_state)
        {
            Match matchHeader;
            byte[] finalBuffer = v_byteData;
            string retMessage = "";
            string resConnectMessage = "";

            // RECIBI un registro GPS
            // NUEVO EN VERSION 1.3: Se conecta al alutrack via ALUTRACK LAYER
            matchHeader = GPSdata.Match(v_header);
            if (matchHeader.Success)
            {
                loguearString(v_header);        // Loguea el GPS

                string HHID = getMatchData(matchHeader, 1).Trim();
                string latitud = getMatchData(matchHeader, 3).Trim();
                string longitud = getMatchData(matchHeader, 4).Trim();
                string hora = getMatchData(matchHeader, 5).Trim();
                string heading = getMatchData(matchHeader, 6).Trim();
                string speed =getMatchData(matchHeader, 7).Trim();

                stringEventArgs retorno = new stringEventArgs(v_header, v_state);
                retorno.textData.Add("HHID", HHID);
                retorno.textData.Add("LATITUD", latitud);
                retorno.textData.Add("LONGITUD", longitud);
                retorno.textData.Add("HORA", hora);

                // Actualiza la base LOCAL con los GPS, chequea los accesos a zonas y da de alta eventos de acceso en caso de Entrada/Salida.
                this.actualizeGPSData(this, retorno);       

                mainApp.DataManager.actualizarUltimaPosGPS(latitud, longitud, hora, HHID);

                //********************************************
                // Conexion al layer ALUTRACK: Alutracklayer
                //********************************************

                if (!HHGPSLayer.isConnected(HHID))
                {
                    if (HHGPSLayer.conectar(HHID, out resConnectMessage))
                    {
                       // loguearString("HandHeld: " + HHID + " conectado al ALUTRACK LAYER");
                        if (HHGPSLayer.sendGPSData(HHID, latitud, longitud, hora, heading, speed, out retMessage))
                        {
                         //   loguearString(HHID + " envio GPS(" + latitud + "," + longitud + ") al ALUTRACK LAYER");
                        }
                        else
                        {
                           // loguearString("Error al enviar GPS desde el HH: " + HHID + ", error: " + retMessage + ". Desconectando...");
                            HHGPSLayer.desconectar(HHID);
                           // loguearString("HH: " + HHID + "desconectado");
                        }
                    }
                    else // No se pudo conectar...
                    {
                       // loguearString("Error al conectar HHID: " + HHID + "- " + resConnectMessage);
                    }
                }
                else
                {
                    if (HHGPSLayer.sendGPSData(HHID, latitud, longitud, hora, heading, speed, out retMessage))
                    {
                        loguearString(HHID + " envio GPS(" + latitud + "," + longitud + ") al ALUTRACK LAYER");
                    }
                    else
                    {
                        //loguearString("Error al enviar GPS desde el HH: " + HHID + " Error: " + retMessage);
                    }
                }

                return;
            }

            // RECIBI DUMMY
            //
            matchHeader = dummyHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();
                loguearString(HHID + "- DUMMY" );
                //stringEventArgs retorno = new stringEventArgs(v_header, v_state);
                //this.responderDUMMY(this, retorno);
                return;
            }

            // RECIBI el pedido de actualización de la lista de empleados.
            // Es el lugar donde se asocia el StateObject al HHID y a la Organizacion.
            // Si el dispositivo esta registrado (agregado previamente desde LENEL o desde la WEB), entonces
            // se da de alta en la lista de stateObjectClients, con clave String HHID
            matchHeader = getEmpListHeader.Match(v_header);
            if (matchHeader.Success)
            {
                if (getEmployeeList != null)
                {
                    string HHID = getMatchData(matchHeader, 1).Trim();
                    v_state.HHID = HHID;                                // registra el nombre del Dispositivo en el State del cliente.
                    
                    stringEventArgs retorno = new stringEventArgs(v_header, v_state);

                    int orgID = mainApp.DataManager.obtenerOrganizationIDFromHHID(HHID);
                    if (orgID > 0)
                    {
                        v_state.orgID=orgID;

                        try
                        {
                            StaticTools.obtenerMutex_StateObjectClients();
                            if (!stateObjectsClients.ContainsKey(HHID))
                            {
                                stateObjectsClients.Add(HHID, v_state);
                            }
                        }
                        catch (Exception ex)
                        {
                            loguearString(HHID + " - excepcion en matchHeader= getEmpListHeader creando Stateobject: " + ex.Message);
                        }
                        finally
                        {
                            StaticTools.liberarMutex_StateObjectClients();
                        }

                        this.getEmployeeList(this, retorno);                 // hace un rise para avisar que va a comenzar el envio de lista de empleados.
                    }
                    else
                    {
                        loguearString("El HH: " + HHID + " no tiene organizacion asociada.");
                        v_state.abortAllThreads(null);
                        v_state.closeAllSockets();                        
                    }
                    
                }
                return;
            }

            // Desde Lenel se eliminó un empleado
            matchHeader = DeleteEmpHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();
                string tarjeta = getMatchData(matchHeader, 3).Trim();

                stringEventArgs retorno = new stringEventArgs(v_header, v_state);
                retorno.textData.Add("HHID", HHID);
                retorno.textData.Add("TARJETA", tarjeta);

                this.deleteEmp(this, retorno);
                return;
            }

            // Desde Lenel se agregó un empleado
            matchHeader = AddEmpHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();
                string tarjeta = getMatchData(matchHeader, 3).Trim();

                stringEventArgs retorno = new stringEventArgs(v_header, v_state);
                retorno.textData.Add("HHID", HHID);
                retorno.textData.Add("TARJETA", tarjeta);

                this.addEmp(this, retorno);
                return;
            }

            // Desde el HH recibí pedido de empleados por versión
            matchHeader = EmpxVersionHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();
                int chunkSize = Convert.ToInt32(getMatchData(matchHeader, 2).Trim());
                byte[] byteData = getDataBytes(finalBuffer, chunkSize);

                byteArrayEventArgs retorno = new byteArrayEventArgs(v_header, byteData, v_state);
                retorno.textData.Add("HHID", HHID);

                this.sendEmpxVersion(this, retorno);
                return;
            }

            // Desde el HH recibí pedido de siguiente bloque de imagenes
            matchHeader = MasImgsHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();

                stringEventArgs retorno = new stringEventArgs(v_header, v_state);
                retorno.textData.Add("HHID", HHID);

                this.enviarMasImagenes(this, retorno);
                return;
            }

            // Desde el HH recibí pedido de lista de imagenes
            matchHeader = ImgVersionHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();

                stringEventArgs retorno = new stringEventArgs(v_header, v_state);
                retorno.textData.Add("HHID", HHID);

                this.enviarVersionImagenes(this, retorno);
                return;
            }

            // Desde el HH recibí pedido de lista de imagenes
            matchHeader = ImgListHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();
                int chunkSize = Convert.ToInt32(getMatchData(matchHeader, 2).Trim());
                byte[] byteData = getDataBytes(finalBuffer, chunkSize);

                byteArrayEventArgs retorno = new byteArrayEventArgs(v_header, byteData, v_state);
                retorno.textData.Add("HHID", HHID);

                this.enviarListaImagenes(this, retorno);
                return;
            }

            // RECIBI un pedido de actualización de una imagen de un usuario.
            matchHeader = getImageHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();
                string tarjeta = getMatchData(matchHeader, 3).Trim();

                stringEventArgs retorno = new stringEventArgs(v_header,v_state);
                retorno.textData.Add("HHID", HHID);
                retorno.textData.Add("TARJETA", tarjeta);

                this.getImage(this, retorno);                  // Hace un rise del evento registrado, el cual va a agregar el job EMPIMAGE a la lista de jobs del socket.
                return;
            }

            // RECIBI un ACCESS de una tarjeta
            matchHeader =accessHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();
                int chunkSize = int.Parse(getMatchData(matchHeader, 2));
                string tarjeta = getMatchData(matchHeader, 3).Trim();
                string Latitud = getMatchData(matchHeader, 4).Trim();
                string Longitud = getMatchData(matchHeader, 5).Trim();
                string hora = getMatchData(matchHeader, 6).Trim();
                string hasImage = getMatchData(matchHeader, 7).Trim();
                string tipoAcceso = getMatchData(matchHeader, 8).Trim();
                string id = getMatchData(matchHeader, 9).Trim();
                byte[] dataBytes;
                if (bool.Parse(hasImage))
                {
                    dataBytes = getDataBytes(finalBuffer, chunkSize);
                }
                else
                {
                    dataBytes = new byte[0];
                }

                byteArrayEventArgs retorno = new byteArrayEventArgs(v_header, dataBytes,v_state);
                retorno.textData.Add("HHID", HHID);
                retorno.textData.Add("TARJETA", tarjeta);
                retorno.textData.Add("LATITUD", Latitud);
                retorno.textData.Add("LONGITUD", Longitud);
                retorno.textData.Add("HORA", hora);
                retorno.textData.Add("IMAGEN", hasImage);
                retorno.textData.Add("TIPOACCESO", tipoAcceso);
                retorno.textData.Add("ID", id);

                loguearString("Reconocido ACCESO CON ID: " + id);
                this.actualizeAccess(this, retorno);                   // Llama al evento registrado para actualizar el movimiento de acceso
                return;
            }

            // RECIBI VISITA
            matchHeader = visitHeader.Match(v_header);
            if (matchHeader.Success)
            {
                string HHID = getMatchData(matchHeader, 1).Trim();
                int chunkSize = int.Parse(getMatchData(matchHeader, 2));
                string nombre = getMatchData(matchHeader, 3).Trim();
                string apellido = getMatchData(matchHeader, 4).Trim();
                string documento = getMatchData(matchHeader, 5).Trim();
                string empresa = getMatchData(matchHeader, 6).Trim();
                string tarjeta = getMatchData(matchHeader, 7).Trim();
                string latitud = getMatchData(matchHeader, 8).Trim();
                string longitud = getMatchData(matchHeader, 9).Trim();
                string hora = getMatchData(matchHeader, 10).Trim();
                string hasImage = getMatchData(matchHeader, 11).Trim();
                string tipoAcceso = getMatchData(matchHeader, 12).Trim();
                string id = getMatchData(matchHeader, 13).Trim();

            
                byte[] dataBytes;
                if (bool.Parse(hasImage))   // Hay imagen asociada. Viene el databytes.
                {
                    dataBytes = getDataBytes(finalBuffer, chunkSize);
                }
                else
                {
                    dataBytes = new byte[0];
                }

                byteArrayEventArgs retorno = new byteArrayEventArgs(v_header, dataBytes, v_state);
                retorno.textData.Add("HHID", HHID);
                retorno.textData.Add("TARJETA", tarjeta);
                retorno.textData.Add("NOMBRE", nombre);
                retorno.textData.Add("APELLIDO", apellido);
                retorno.textData.Add("DOCUMENTO", documento);
                retorno.textData.Add("EMPRESA", empresa);
                retorno.textData.Add("LATITUD", latitud);
                retorno.textData.Add("LONGITUD", longitud);
                retorno.textData.Add("HORA", hora);
                retorno.textData.Add("IMAGEN", hasImage);
                retorno.textData.Add("TIPOACCESO", tipoAcceso);
                retorno.textData.Add("ID", id);

                int orgID = mainApp.DataManager.obtenerOrganizationIDFromHHID(HHID);
                if (orgID > 0)
                {
                    retorno.textData.Add("ORGID", orgID.ToString());
                    this.actualizeVisit(this, retorno);

                }

                return;
            }

            // Recibi un mensaje de Fin de sincronizacion
            matchHeader = FinSyncHeader.Match(v_header);
            if (matchHeader.Success)
            {

                mainApp.DataManager.actualizarFechaSync(v_state.HHID);
                mainApp.ComunicationSystem.EnviarJobFinSync(v_state.HHID, v_state);
                return;
            }

            // RECIBI ARCHIVO DE LOG
            matchHeader = LOGFile.Match(v_header);
            if (matchHeader.Success)
            {
                
                string HHID = getMatchData(matchHeader, 1).Trim();
                int fileSize = int.Parse(getMatchData(matchHeader, 2));
                string logName = getMatchData(matchHeader, 3).Trim();
                try
                {
                    byte[] dataBytes;
                    dataBytes = getDataBytes(finalBuffer, fileSize);
                    if (dataBytes.Length > 0 && logName != "")
                    {
                        DateTime ahora = DateTime.Now;
                        string finalLogName = HHID + "_" + ahora.Year.ToString() + ahora.Month.ToString() + ahora.Day.ToString() + "_" + ahora.Hour.ToString() + "-" + ahora.Minute.ToString() + "-" + ahora.Second.ToString() + "_" + logName;
                        File.WriteAllBytes(SystemConfiguration.ImagesPath + @"\" + finalLogName, dataBytes);
                    }
                    loguearString(HHID + " - Server recibió log: " + logName);

                    stringEventArgs retorno = new stringEventArgs(logName, v_state);

                    retorno.textData.Add("LOGNAME", logName);
                    this.confirmarLOG(this, retorno);                   //TYPE:BORRARLOG Manda la confirmacion de recepcion del archivo de LOG para que el HH lo borre de su disco: BORRARLOG
                }
                catch (Exception ex)
                {
                    loguearString(HHID +"- Excepcion en LOGFile: " + ex.Message);
                }
                return;
            }
            /* NO RECONOCIO HEADER */
            else loguearString("ATENCION! Header no reconocido: " + v_header);
        }
        //public void  PedirListaEmpleados(string HHID)
        //{
            
        //    try
        //    {
        //        StaticTools.obtenerMutex_StateObjectClients();
        //        StateObject cliente = null;
        //        foreach (KeyValuePair<int, StateObject> pair in stateObjectsClients)
        //        {
        //            if (pair.Value.HHID == HHID)
        //            {
        //                cliente = pair.Value;
        //                break;
        //            }
        //        }
        //        if (cliente != null)
        //        {
        //            stringEventArgs ev = new stringEventArgs("", cliente);
        //            this.getEmployeeList(this, ev);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        loguearString(HHID+" - xcepcion en PedirListaEmpleados()" + ex.Message);
        //    }
        //    finally
        //    {
        //        StaticTools.liberarMutex_StateObjectClients();
        //    }
        //}

        /// <summary>
        /// ObtenerDatos()
        /// Obtiene los bytes especificados del comienzo del MemoryStream asociado al StateObject.
        /// Devuelve un byteArray con los datos extraidos
        /// El parametro extract hace que se saquen los datos devueltos.
        /// Para ello lee los datos a un buffer y crea un nuevo MemoryStream con los datos sobrantes.
        /// </summary>
        /// <param name="v_state"> El StateObject que contiene el MemoryStream</param>
        /// <param name="cant_datos">La cantidad de bytes a extraer desde el comienzo</param>
        /// <returns></returns>
        private byte[] ObtenerDatos(StateObject v_state, int cant_datos, bool extractDatos)
        {
            try
            {
                byte[] bytesToreturn = new byte[cant_datos];
                v_state.memStream.Position = 0;
                v_state.memStream.Read(bytesToreturn, 0, cant_datos);

                if (extractDatos)
                {
                    byte[] bytesResto = new byte[v_state.memStream.Length - cant_datos];
                    v_state.memStream.Read(bytesResto, 0, bytesResto.Length);

                    // Grabo los datos del resto en el nuevo MemoryStream.
                    MemoryStream newStream = new MemoryStream();
                    newStream.Write(bytesResto, 0, bytesResto.Length);

                    v_state.memStream = newStream;  // En el StateObject queda el MemoryStream SIN cant_datos bytes 
                }

                v_state.memStream.Position = v_state.memStream.Length;  // Lo posiciona para continuar leyendo al final.
                return bytesToreturn;
            }
            catch (Exception ex)
            {
                loguearString(v_state.HHID + "- Excepcion en ObtenerDatos: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// obtenerChunkSize()
        /// Parsea los datos del header pasado como parametro y obtiene el tamaño del chunk definido en su interior.
        /// Si el header no está en ningún formato reconocible devuelve -1
        /// </summary>
        /// <param name="v_headerData"> Los datos del Header. Por lo menos de FIXED_HEADER_LENGTH</param>
        /// <returns></returns>
        public int obtenerChunkSize(byte[] v_headerData)
        {
            int chunkSize = -1;
            try
            {
                Match matchHeader;

                string header = Encoding.ASCII.GetString(v_headerData, 0, FIXED_HEADER_LENGTH);


                matchHeader = DatosEncripHeader.Match(header);
                if (matchHeader.Success)
                {
                    chunkSize = int.Parse(getMatchData(matchHeader, 1));
                }

                matchHeader = getEmpListHeader.Match(header);
                if (matchHeader.Success)
                {
                    chunkSize = int.Parse(getMatchData(matchHeader, 2));
                }

                matchHeader = getImageHeader.Match(header);
                if (matchHeader.Success)
                {
                    chunkSize = int.Parse(getMatchData(matchHeader, 2));
                }

                matchHeader = accessHeader.Match(header);
                if (matchHeader.Success)
                {
                    chunkSize = int.Parse(getMatchData(matchHeader, 2));
                }

                matchHeader = GPSdata.Match(header);
                if (matchHeader.Success)
                {
                    chunkSize = int.Parse(getMatchData(matchHeader, 2));
                }

                matchHeader = visitHeader.Match(header);
                if (matchHeader.Success)
                {
                    chunkSize = int.Parse(getMatchData(matchHeader, 2));
                }

                //matchHeader = dummyHeader.Match(header);
                //if (matchHeader.Success)
                //{
                //    chunkSize = int.Parse(getMatchData(matchHeader, 2));
                //}
            }
            catch (Exception ex)
            {
                loguearString("EXCEPCION EN OBTENERCHUNKSIZE: " +ex.Message);
            }
            return chunkSize;
        }

        public void startSendSequence(StateObject stateClient)
        {
            Socket socketToClient = stateClient.socketToSend;
            JobToSendToClient trabajo;
            try
            {
                loguearString("START SEND SEQUENCE. CANTIDAD DE TRABAJOS: " + stateClient.pendingJobs.Count);

                if (stateClient.pendingJobs.Count > 0)
                {
                    stateClient.sendingData = true;
                    trabajo = stateClient.pendingJobs.Dequeue();           // OJO: Aca: MUTEX...

                    loguearString("Trabajo a enviar: " + trabajo.byteData.Length.ToString());
                   
                    socketToClient.BeginSend(trabajo.byteData, 0, trabajo.byteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), stateClient);
                }
                else
                {
                    // Avisa que ya no hay mas datos que enviar y que el socket esta libre para enviar datos nuevamente.
                    stateClient.sendingData = false;
                }
            }
            catch (Exception ex)
            {
                //loguearString2("Excepcion en StartSendSequence: " + ex.Message);
                //stateClient.abortFlag = true;
                setAbortFlag(" StartSendSequence, " + ex.Message, stateClient);

                EliminarThreadsySockets(stateClient, stateClient.t_send);
            }
        }

        private void setAbortFlag(string texto, StateObject cliente)
        {
            if (!cliente.abortFlag)
            {
                loguearString2(cliente.HHID +" - EXCEPCION: " + texto);
                cliente.abortFlag = true;
            }


        }

        public string getMatchData(Match resultMatch, int index)
        {
            return resultMatch.Groups[index].Value;
        }

        private byte[] getDataBytes(byte[] buffer, int chunksize)
        {
            byte[] result = new byte[chunksize - FIXED_HEADER_LENGTH];
            System.Buffer.BlockCopy(buffer, FIXED_HEADER_LENGTH, result, 0, result.Length);
            return result;
        }

        /// <summary>
        /// Interpreta los datos recibidos y los muestra según su tipo. 
        /// </summary>
        /// <param name="datos"></param>
        private void interpretarDatos(byte[] datos, int longitudTotal)
        {
            string header = Encoding.ASCII.GetString(datos, 0, FIXED_HEADER_LENGTH);
            
            char[] separadorDatos = { '|' };
            char[] separadorCampos = {':'};
            string [] datosHeader = header.Split(separadorDatos);
            bool cargarImagen = false;

            foreach (string s in datosHeader)
            {
                string[] camposheader = s.Split(separadorCampos);
                if (camposheader.Length > 1)
                {
                    string tipo = camposheader[0];
                    string valor = camposheader[1];
                    switch (tipo)
                    {
                        case "TYPE":
                            if (valor == "DATA")
                            {
                                string dataASCII = Encoding.ASCII.GetString(datos, FIXED_HEADER_LENGTH, longitudTotal - FIXED_HEADER_LENGTH);
                                string[] datosEmployee = dataASCII.Split(separadorDatos);
                                foreach (string s2 in datosEmployee)
                                {
                                    string[] camposEmployee = s2.Split(separadorCampos);
                                    string campo = camposEmployee[0];
                                    string dato = camposEmployee[1];
                                    
                                    //LOGQueue.Enqueue(campo + ":" + dato);
                                    loguearString(campo + ":" + dato);

                                }
                            }                            
                            if (valor == "IMAGE")
                            {
                                cargarImagen = true;
                            }
                            break;
                        case "FILENAME":
                            if (cargarImagen)
                            {
                                string nombrearchivo = valor;

                                FileStream stream = new FileStream(nombrearchivo, FileMode.Create, FileAccess.Write);
                                BinaryWriter writer = new BinaryWriter(stream);

                                writer.Write(datos,FIXED_HEADER_LENGTH,longitudTotal-FIXED_HEADER_LENGTH);

                                writer.Close();
                                stream.Close();
                                //LOGQueue.Enqueue("IMAGEN:" + nombrearchivo);
                                loguearString("IMAGEN:" + nombrearchivo);
                            }
                            break;

                    }
                }            
            }

        }

        public void stopListening()
        {
            finishListen = true;

            shutDownAndCloseListenSockets();
        }

        private void loguearString(string texto)
        {
            //LOGToFile.doLOG(texto);
            StaticTools.loguearString(texto);
            if (actualizarLOG != null)
            {
                
                stringEventArgs p = new stringEventArgs(texto,null);
                p.LOGTYPE = TiposLOG.HH;
                this.actualizarLOG(this, p);
            }
        }

        private void loguearString2(string texto)
        {
            //LOGToFile.doLOGExcepciones(texto);
            loguearString(texto);
            if (actualizarLOG != null)
            {
                stringEventArgs p = new stringEventArgs(texto, null);
                p.LOGTYPE = TiposLOG.HH;
                this.actualizarLOG(this, p);
            }
        }
        
        public void SendBytes(byte[] byteData)
        {
            // Begin sending the data to the remote device.
            
            socketToReceive.BeginSend(byteData, 0, byteData.Length,SocketFlags.None, new AsyncCallback(SendCallback), socketToReceive);
            
            //socketToClient.Send(byteData);
        }

        // Nueva estrategia de SEND: Se envían todos los trabajos encolados uno tras otro.
        private void SendCallback(IAsyncResult ar)
        {
            loguearString("Entro a SendCallback");
           
            try
            {
                StateObject stateClient = (StateObject)ar.AsyncState;
                Socket SocketClient = stateClient.socketToSend;

                // Complete sending the data to the remote device.
                int bytesSent = SocketClient.EndSend(ar);
                loguearString(stateClient.HHID + " - Enviados: " + bytesSent);

                if ((stateClient.pendingJobs.Count > 0) && (!stateClient.abortFlag))
                {
                    JobToSendToClient trabajo = stateClient.pendingJobs.Dequeue();           // OJO: Aca: MUTEX...
                    loguearString("trabajo a enviar: " + trabajo.byteData.Length.ToString());
                    Thread.Sleep(500);
                    SocketClient.BeginSend(trabajo.byteData, 0, trabajo.byteData.Length, SocketFlags.None, new AsyncCallback(SendCallback), stateClient);
                }
                else
                {
                    // Avisa al thread que el socket no esta enviando mas datos y por lo tanto puede ser llamado nuevamente BeginSend()
                    stateClient.sendingData = false;
                    loguearString("No hay mas datos que enviar");
                }

            }
            catch (Exception e)
            {
                loguearString("Excepcion en SendCallback: " + e.Message);
                StaticTools.reBind = true;
            }
        }

        /// <summary>
        /// Elimina todos los threads asociados a un cliente.
        /// </summary>
        /// <param name="sessionState"></param>
        public void EliminarThreadsySockets(StateObject sessionState, Thread v_callingThread)
        {
            loguearString("EliminarThreads y sockets: ABORTANDO TODOS LOS THREADS del cliente: " + sessionState.HHID);
            EliminarThreads(sessionState, v_callingThread);

            EliminarSockets(sessionState);

            loguearString("Activando REBIND");

            string keyStateObject ="";
            try
            {
                StaticTools.obtenerMutex_StateObjectClients();

                // Elimina el Stateobject de la lista de stateobjects
                foreach (KeyValuePair<string, StateObject> pair in stateObjectsClients)
                {
                    if (pair.Value == sessionState)
                    {
                        keyStateObject = pair.Key;
                        break;
                    }
                }

                if (keyStateObject !="")
                {
                    stateObjectsClients.Remove(keyStateObject);
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en EliminarThreadsySockets: "+ex.Message);
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }
            finishListen = true;
            allDone.Set();
            StaticTools.reBind = true;

            communicationSystem.actualizarListaDevicesInForm();
        }

        /// <summary>
        /// Elimina todos los threads de sessionState haciendoles Abort() y, por lo tanto, generando una excepcion para salir
        /// el parametro v_actualThread es el thread que esta invocando a esta llamada, el cual, NO debe ser abortado, ya que saldra
        /// por el hilo actualmente en ejecucion.
        /// </summary>
        /// <param name="sessionState"></param>
        /// <param name="v_actualThread"></param>
        public void EliminarThreads(StateObject sessionState, Thread v_actualThread)
        {
            sessionState.abortAllThreads(v_actualThread);
            loguearString(sessionState.HHID + " - TODOS LOS THREADS han sido eliminados");
        }
        public void EliminarSockets(StateObject sessionState)
        {

            sessionState.closeAllSockets();
            loguearString(sessionState.HHID  + " - TODOS LOS SOCKETS han sido cerrados");
        }


        /// <summary>
        /// Devuelve el header desencriptado.
        /// </summary>
        /// <param name="header">Header encriptado</param>
        /// <returns></returns>
        private string DesencriptarHeader(string header)
        {
            string res = string.Empty;

            Match DatosOK = DatosEncripHeader.Match(header);

            if (DatosOK.Success)
            {
                res = DatosOK.Groups[2].Value;
            }
            else
            {
                loguearString("Excepción DesencriptarHeader(), header incorrecto: " + header);
            }
            string dataToDecrypt = res.Trim();
            res = Encriptar.desencriptar(dataToDecrypt, communicationSystem.ClaveEncriptar);

            return res;
        }


        internal StateObject getCliente(string v_HHID)
        {
            StateObject res = null;
            try
            {
                StaticTools.obtenerMutex_StateObjectClients();
                res = stateObjectsClients[v_HHID];
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en getCliente():" + ex.Message);
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }

            return res;

            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using log4net;

namespace ServerComunicacionesALUTEL
{
    
    static class StaticTools
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	
        static Aplicacion mainApp;
        static Mutex mutx_UltimasPosGPS = new Mutex();
        static Mutex mutx_StateObjectClients = new Mutex();
        static Mutex mutx_ListaEmpleados = new Mutex();
        static Mutex mutx_ListaTarjetas = new Mutex();
        static Mutex mutx_ListaEventosGPS = new Mutex();
        static Mutex mutx_Rebind = new Mutex();

        public static bool reBind = false;


        public static void setearAplicacion(Aplicacion v_app)
        {
            mainApp = v_app;
        }


        public static void obtenerMutex_Rebind()
        {
            mutx_Rebind.WaitOne();
        }

        public static void liberarMutex_Rebind()
        {
            mutx_Rebind.ReleaseMutex();
        }
        
        public static void obtenerMutex_ListaEventosGPS()
        {
            mutx_ListaEventosGPS.WaitOne();
        }

        public static void liberarMutex_ListaEventosGPS()
        {
            mutx_ListaEventosGPS.ReleaseMutex();
        }

        public static void obtenerMutex_ListaTarjetas()
        {
            mutx_ListaTarjetas.WaitOne();
        }

        public static void liberarMutex_ListaTarjetas()
        {
            mutx_ListaTarjetas.ReleaseMutex();
        }


        public static void obtenerMutex_ListaEmpleados()
        {
            mutx_ListaEmpleados.WaitOne();
        }

        public static void liberarMutex_ListaEmpleados()
        {
            mutx_ListaEmpleados.ReleaseMutex();
        }

        public static void obtenerMutex_UltimasPosGPS()
        {
            mutx_UltimasPosGPS.WaitOne();
        }

        public static void liberarMutex_UltimasPosGPS()
        {
            mutx_UltimasPosGPS.ReleaseMutex();
        }

        public static void obtenerMutex_StateObjectClients()
        {
            mutx_StateObjectClients.WaitOne();
        }

        public static void liberarMutex_StateObjectClients()
        {
            mutx_StateObjectClients.ReleaseMutex();
        }

        public static string obtenerLocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        public static void doRebind()
        {
            try
            {
                obtenerMutex_Rebind();

                if (reBind)
                {
                    // LOGToFile.doLOG("Activado el ReBIND de los sockets");

                    loguearString("doRebind() -Haciendo el ReBIND de los sockets");
                    //Thread.Sleep(1000);
                    StaticTools.reBind = false;
                    mainApp.ComunicationSystem.socketServerGeneral.stopListening();
                    mainApp.alutrackServerThread.Abort();

                    //LOGToFile.doLOG("Sockets de escucha Cerrados y thread de escucha Detenido.");
                    loguearString("doRebind() - Sockets de escucha Cerrados y thread de escucha Detenido");

                    Thread.Sleep(1000);

                    mainApp.alutrackServerThread = new Thread(new ThreadStart(mainApp.ComunicationSystem.socketServerGeneral.StartListening));
                    mainApp.alutrackServerThread.Name = "ALUTRACK SERVER";

                    // Lanza el thread
                    mainApp.alutrackServerThread.Start();

                    //Espera para que el thread efectivamente comience.
                    //while (!mainApp.mainThread.IsAlive) ;

                    Thread.Sleep(100);

                    //LOGToFile.doLOG("THREAD de escucha vuelto a levantar. Server escuchando");
                    StaticTools.loguearString("doRebind() - THREAD de escucha vuelto a levantar. Server escuchando");

                }
            }
            catch (Exception ex)
            {
                StaticTools.loguearString("Excepcion en StaticTools.doRebind(): " + ex.Message);
            }
            finally
            {
                liberarMutex_Rebind();
            }

        }
        public static void loguearString(string v_texto)
        {
            log.Debug(v_texto);
        }
    }
}

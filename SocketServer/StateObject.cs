using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ServerComunicacionesALUTEL
{
    public class StateObject
    {
        // Client  socket.
        //public Socket workSocket = null;
        public Socket socketToSend = null;
        public Socket socketToReceive = null;
        public ManualResetEvent sendACK = new ManualResetEvent(false);   // Semaforo para activar el send del ACK
        public bool abortFlag = false;

        public Socket socketAliveSend = null;
        public Socket socketAliveReceive = null;

        public string HHID = "";  // EL ID del handheld asociado al StateObject 

        public ManualResetEvent readyToSend = new ManualResetEvent(false);  // Semaforo para indicar que se ha recibido un dato y que se puede enviar info
        public bool sendingData = false;                                        // Flag para indicar que se estan enviando datos.

        public Queue<JobToSendToClient> pendingJobs = new Queue<JobToSendToClient>();     // cola de trabajos asociados al cliente.

        // Size of receive buffer.
        public const int BufferSize = 400 * 1024;                     // Maximo: 400 kbytes por recepcion.
        public byte[] buffer;
        public MemoryStream memStream;
        public int chunkSizeTarget;     // Largo objetivo del bloque de datos a recibir.
        public int offset;              // Offset dentro del bloque para continuar recibiendo datos dentro del mismo chunk

        public bool hasHeader;          // Indica si en el buffer ya se ha leido el HEADER
        public string actualHeader;    // Ultimo header reconocido
        public int numeroCliente;
        public int orgID; //id de la organizacion a la que pertenece el dispositivo.

        public bool masImagenes = false;   // Indica cuando se deben enviar mas imágenes

        // Variables de THREAD
        public Thread t_receive = null;
        public Thread t_send = null;
        public Thread t_aliveSend = null;
        public Thread t_aliveReceive = null;
        public Thread t_dummySend = null;
        public const int TIME_BETWEEN_DUMMY = 30000;    // Un DUMMY cada 30 segundos


        public int contadorACKSend = 0;
        public int contadorACKReceive = 0;

        public bool envioInicialAccessLevels = false;



        public StateObject()
        {
            memStream = new MemoryStream();
            buffer = new byte[BufferSize];
            offset = 0;
            contadorACKSend = 0;
            contadorACKReceive = 0;
        }
        
        public void abortAllThreads(Thread v_actualThread)
        {
            try
            {
                if (v_actualThread != t_aliveReceive)
                    t_aliveReceive.Abort();
            }
            catch (Exception)
            {
                // No hago nada. Quiero eliminar bajar todo lo que tenga bajado...
            }

            try
            {
                if (v_actualThread != t_aliveSend)
                    t_aliveSend.Abort();
            }
            catch (Exception)
            {
                // No hago nada. Quiero eliminar bajar todo lo que tenga bajado...
            }

            try
            {
                if (v_actualThread != t_receive)
                    t_receive.Abort();
            }
            catch (Exception)
            {
                // No hago nada. Quiero eliminar bajar todo lo que tenga bajado...
            }

            try
            {
                if (v_actualThread != t_send)
                    t_send.Abort();
            }
            catch (Exception)
            {
                // No hago nada. Quiero eliminar bajar todo lo que tenga bajado...
            }

            try
            {
                if (v_actualThread != t_dummySend)
                    t_dummySend.Abort();
            }
            catch (Exception)
            {
                // No hago nada. Quiero eliminar bajar todo lo que tenga bajado...
            }

        }

        public void closeAllSockets()
        {
            try
            {
                socketAliveReceive.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // NO hacer nada. Continuar: quiero dar de baja todo lo que este abierto.
            }

            try
            {
                socketAliveReceive.Close();
            }
            catch (Exception)
            {
                // NO hacer nada. Continuar: quiero dar de baja todo lo que este abierto.
            }

            try
            {
                socketAliveSend.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // NO hacer nada. Continuar: quiero dar de baja todo lo que este abierto.
            }

            // Ahora cerrar y liberar todos los SOKCETS involucrados.
            // Están dentro de try/catch por si salta una excepcion. Sigue hasta que se libere todo lo posible.
            try
            {
                socketAliveSend.Close();
            }
            catch (Exception)
            {
                // NO hacer nada. Continuar: quiero dar de baja todo lo que este abierto.
            }

            try
            {
                socketToSend.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // NO hacer nada. Continuar: quiero dar de baja todo lo que este abierto.
            }

            try
            {
                socketToSend.Close();
            }
            catch (Exception)
            {
                // NO hacer nada. Continuar: quiero dar de baja todo lo que este abierto.
            }

            try
            {
                socketToReceive.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
                // NO hacer nada. Continuar: quiero dar de baja todo lo que este abierto.
            }

            try
            {
                socketToReceive.Close();
            }
            catch (Exception)
            {
                // NO hacer nada. Continuar: quiero dar de baja todo lo que este abierto.
            }

        }


    }
}

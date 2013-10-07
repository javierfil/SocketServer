﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;
using Microsoft.Win32;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace ServerComunicacionesALUTEL
{
    public partial class frmInicial : Form
    {
        public delegate void ActualizarALUTRACKLOGDelegate(string msg);
        public delegate void ActualizarLENELLOGDelegate(string msg);
        public delegate void ActualizarAlutrackServerLOGDelegate(string msg);
        public string generalLat = "0";
        public string generalLong = "0";

        public string generalZoom = "16";

        //StreamWriter LOGFile;

        private Aplicacion mainApp;



        //frmUsuarios frmUsuarios = new frmUsuarios();
        public frmInicial(Aplicacion v_aplicacion)
        {
            mainApp = v_aplicacion;

            InitializeComponent();
        }

        private void frmConfiguration_Load(object sender, EventArgs e)
        {
//            MessageBox.Show("ATENCION: NUEVO PROTOCOLO. NO USAR CON DEMOS HASTA NO ACTUALIZAR VERSION EN CLIENTES");
            mainApp.Init2();
           
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!mainApp.formUsuarios.IsDisposed)
            {
                mainApp.formUsuarios.Show();
            }
            else
            {
                mainApp.formUsuarios = new frmUsuarios(mainApp);
                mainApp.formUsuarios.Show();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!mainApp.formDevices.IsDisposed)
            {
                mainApp.formDevices.Show();
            }
            else
            {
                mainApp.formDevices = new frmDevices(mainApp);
                mainApp.formDevices.Show();
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (!mainApp.formGrupos.IsDisposed)
            {
                mainApp.formGrupos.Show();
            }
            else
            {
                mainApp.formGrupos = new frmGrupos(mainApp);
                mainApp.formGrupos.Show();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

            if (!mainApp.formConfigAcceso.IsDisposed)
            {
                mainApp.formConfigAcceso.Show();
            }
            else
            {
                mainApp.formConfigAcceso = new frmConfigAcceso(mainApp);
                mainApp.formConfigAcceso.Show();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
 

        }

        /// <summary>
        /// Timer que actualiza los listView con los datos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrSend_Tick(object sender, EventArgs e)
        {
            //int cantLog = mainApp.ComunicationSystem.LOGInformation.Count;
            //if (cantLog > 0)
            //{
            //    for (int i = 0; i < cantLog; i++)
            //    {
            //        DateTime hora = DateTime.Now;

            //        stringEventArgs textToLOG = mainApp.ComunicationSystem.LOGInformation.Dequeue();
            //        if (textToLOG != null)
            //        {
            //            if (textToLOG.LOGTYPE == TiposLOG.ALL || textToLOG.LOGTYPE == TiposLOG.ALUTRACK)
            //            {
            //                if (!chkVerboseLOGAlutrack.Checked)
            //                {
            //                    lstAlutrackLOG.Items.Insert(0, hora.ToLongTimeString() + ":" + hora.Millisecond + " - " + textToLOG.text);
            //                }
            //            }

            //            if (textToLOG.LOGTYPE == TiposLOG.ALL || textToLOG.LOGTYPE == TiposLOG.LENEL)
            //            {
            //                if (!chkVerboseLOGLenel.Checked)
            //                {
            //                    lstLenelLOG.Items.Insert(0, hora.ToLongTimeString() + ":" + hora.Millisecond + " - " + textToLOG.text);
            //                }
            //            }
            //        }

            //    }
            //}

            //try
            //{
            //    int cantACKsRecibidos = 0;
            //    int cantACKsEnviados = 0;

            //    Dictionary<int, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;
            //    foreach (KeyValuePair<int, StateObject> pair in listaClientes)
            //    {
            //        cantACKsRecibidos = cantACKsRecibidos + pair.Value.contadorACKReceive;
            //        cantACKsEnviados = cantACKsEnviados + pair.Value.contadorACKSend;
            //    }

            //    lblACKRecv.Text = cantACKsRecibidos.ToString();
            //    lblACKSnd.Text =  cantACKsEnviados.ToString();
            //}
            //catch (Exception)
            //{
                
            //}
        }


        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }


        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tarjeta"></param>
       
        private void button3_Click_1(object sender, EventArgs e)
        {

        }

        private void btnConfig_Click(object sender, EventArgs e)
        {


            if (!mainApp.formConfiguracion.IsDisposed)
            {
                mainApp.formConfiguracion.Show();
            }
            else
            {
                mainApp.formConfiguracion = new frmConfiguracion(mainApp);
                mainApp.formConfiguracion.Show();
            }
        }

        private void chkActualizarLENEL_CheckedChanged(object sender, EventArgs e)
        {
           // mainApp.ComunicationSystem.actualizarLENEL = chkActualizarLENEL.Checked;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click_2(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }


        private void chkActualizarLENEL_CheckedChanged_1(object sender, EventArgs e)
        {
           // mainApp.ComunicationSystem.actualizarLENEL = chkActualizarLENEL.Checked;
        }

        private void btnVirtualGate_Click(object sender, EventArgs e)
        {
            if (!mainApp.formVirtualGate.IsDisposed)
            {
                mainApp.formVirtualGate.Show();
            }
            else
            {
                mainApp.formVirtualGate = new frmVirtualGate(mainApp);
                mainApp.formVirtualGate.Show();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
           lstAlutrackLOG.Items.Clear();
        }

        private void frmInicial_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void tmrRebind_Tick(object sender, EventArgs e)
        {
            StaticTools.doRebind();
        }

        private void button3_Click_3(object sender, EventArgs e)
        {
            double p = 1.234f;

            string d = p.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }
        private void button3_Click_4(object sender, EventArgs e)
        {
            string allPersonID = mainApp.DataManager.loadAllPersonID("P001");
            string accessLevels = AccessLevelLogic.getAccessLevelsByBadgeInHH("8565", "P001");

            string dato2 = Encoding.ASCII.GetString(new byte[0]);


           string privALUTELKey = "clave_encript:_-#$==%n#%$/()@-_:";


            string dato = "";
           // string clave = "Ery1234567nypoIu09MPOEry1234567m";
            string clave = getMAC();


            if (clave.Length < 32)
            {
                clave = clave + privALUTELKey.Substring(1, 32 - clave.Length);
            }

            string javierEnc = encriptar(dato, clave);

            string javierDec = desencriptar(javierEnc, clave);

        }

            //PhysicalAddress MAC = null;
            //foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            //{
            //    // Only consider Ethernet network interfaces
            //    if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
            //    {
            //        MAC =  nic.GetPhysicalAddress();
            //    }
            //}
            //string strMAC = MAC.ToString();
            
            //long ticks = DateTime.Now.Ticks;
            
            //string InstallPath = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\ALUTEL\TRANSLATOR", "ServerIP", null);

            //string personalFolder = @Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);


        // Obtiene la Physical MAC de la maquina....
        private string getMAC()
        {
            PhysicalAddress MAC = null;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider Ethernet network interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet && nic.OperationalStatus == OperationalStatus.Up)
                {
                    MAC = nic.GetPhysicalAddress();
                }
            }

            return MAC.ToString();
        }

        private string encriptar(string cadena, string clave)
        {
            // Convierto la cadena y la clave en arreglos de bytes
            // para poder usarlas en las funciones de encriptacion
            byte[] cadenaBytes = Encoding.UTF8.GetBytes(cadena);
            byte[] claveBytes = Encoding.UTF8.GetBytes(clave);

            // Creo un objeto de la clase Rijndael
            RijndaelManaged rij = new RijndaelManaged();

            // Configuro para que utilice el modo ECB
            rij.Mode = CipherMode.ECB;

            // Configuro para que use encriptacion de 256 bits.
            rij.BlockSize = 256;

            // Declaro que si necesitara mas bytes agregue ceros.
            rij.Padding = PaddingMode.Zeros;

            // Declaro un encriptador que use mi clave secreta y un vector
            // de inicializacion aleatorio
            ICryptoTransform encriptador;
            encriptador = rij.CreateEncryptor(claveBytes, rij.IV);

            // Declaro un stream de memoria para que guarde los datos
            // encriptados a medida que se van calculando
            MemoryStream memStream = new MemoryStream();

            // Declaro un stream de cifrado para que pueda escribir aqui
            // la cadena a encriptar. Esta clase utiliza el encriptador
            // y el stream de memoria para realizar la encriptacion
            // y para almacenarla
            CryptoStream cifradoStream;
            cifradoStream = new CryptoStream(memStream, encriptador, CryptoStreamMode.Write);

            // Escribo los bytes a encriptar. A medida que se va escribiendo
            // se va encriptando la cadena
            cifradoStream.Write(cadenaBytes, 0, cadenaBytes.Length);

            // Aviso que la encriptación se terminó
            cifradoStream.FlushFinalBlock();

            // Convierto los datos encriptados en el memory stream a un byte array
            byte[] cipherTextBytes = memStream.ToArray();

            // Cierro los dos streams creados
            memStream.Close();
            cifradoStream.Close();

            // Convierto el resultado en base 64 para que sea legible
            // y devuelvo el resultado
            return Convert.ToBase64String(cipherTextBytes);
        }

        private string desencriptar(string cadena, string clave)
        {
            // Convierto la cadena y la clave en arreglos de bytes
            // para poder usarlas en las funciones de encriptacion
            // En este caso la cadena la convierta usando base 64
            // que es la codificacion usada en el metodo encriptar
            byte[] cadenaBytes = Convert.FromBase64String(cadena);
            byte[] claveBytes = Encoding.UTF8.GetBytes(clave);

            // Creo un objeto de la clase Rijndael
            RijndaelManaged rij = new RijndaelManaged();

            // Configuro para que utilice el modo ECB
            rij.Mode = CipherMode.ECB;

            // Configuro para que use encriptacion de 256 bits.
            rij.BlockSize = 256;

            // Declaro que si necesitara mas bytes agregue ceros.
            rij.Padding = PaddingMode.Zeros;

            // Declaro un desencriptador que use mi clave secreta y un vector
            // de inicializacion aleatorio
            ICryptoTransform desencriptador;
            desencriptador = rij.CreateDecryptor(claveBytes, rij.IV);

            // Declaro un stream de memoria para que guarde los datos
            // encriptados
            MemoryStream memStream = new MemoryStream(cadenaBytes);

            // Declaro un stream de cifrado para que pueda leer de aqui
            // la cadena a desencriptar. Esta clase utiliza el desencriptador
            // y el stream de memoria para realizar la desencriptacion
            CryptoStream cifradoStream;
            cifradoStream = new CryptoStream(memStream, desencriptador, CryptoStreamMode.Read);

            // Declaro un lector para que lea desde el stream de cifrado.
            // A medida que vaya leyendo se ira desencriptando.
            StreamReader lectorStream = new StreamReader(cifradoStream);

            // Leo todos los bytes y lo almaceno en una cadena
            string resultado = lectorStream.ReadToEnd();

            // Cierro los dos streams creados
            memStream.Close();
            cifradoStream.Close();

            // Quito los '\0' que completan la cadena
            //if (resultado.Contains(@"\0")) resultado = resultado.Substring(0, resultado.IndexOf(@"\0"));


            // Devuelvo la cadena
            return resultado.Replace("\0", "");
        }





























        private int obtenerByteDias(string listaDias)
        {
            string byteResStr = "00000000";

            string[] listaBits = listaDias.Split(',');

            StringBuilder strB = new StringBuilder(byteResStr);

            foreach (string bit in listaBits)
            {
                if (bit.Length > 0)
                {
                    int posBit = 8 - int.Parse(bit) ;     // numeracion basada en cero.
                    if (posBit == 8)
                    {
                        posBit = 1;
                    }
                    strB[ 7- posBit] = '1';
                }
            }
            return Convert.ToInt32(strB.ToString(), 2);
        }



        /// <summary>
        /// Obtiene la lista de dias a partir del analisis de los BITS del BYTE DOW
        /// Formato de salida: dia1,dia2,dia3
        /// </summary>
        /// <returns></returns>
        public string obtenerListaDias(byte DOW)
        {
            string binaryDays = Convert.ToString(DOW, 2).PadLeft(8, '0');

            string res = "";

            for (int i = 0; i < binaryDays.Length-1; i++)
            {
                if (binaryDays[i] == '1')
                {
                    int idDOW = i + 1;            // Conversion de formato LENEL a DaysOfWeek
                    if (idDOW == 7)
                    {
                        idDOW = 0;              // Conversion de formato LENEL a DaysOfWeek        
                    }
                    res = res + idDOW.ToString() + ",";
                }
            }
            if (res.Length > 0)
            {
                return res.Substring(0, res.Length - 1);       // Le saco la coma
            }
            else
            {
                return res;
            }
        }

        


        // Devuelve la lista de tipos separados por coma
        private string obtenerTipos(int tipoHoliday)
        {
            string res = "";

            string binaryHol = Convert.ToString(tipoHoliday, 2).PadLeft(8, '0');            // Convierte el byte en Binairo

            for (int i =  binaryHol.Length-1; i >=0; i--)
            {
                if (binaryHol[i] == '1')
                {
                    res = res + (8 - i).ToString() + ",";
                }
            }
            if (res.Length>0)
                res = res.Substring(0, res.Length - 1);

            return res;

        }

        // Construye el byte a partir de la lista de tipos separados por coma
        private int obtenerByte(string listaTipos)
        {
            string byteResStr = "00000000";

            string[] listaBits = listaTipos.Split(',');

            StringBuilder strB = new StringBuilder(byteResStr);

            foreach (string bit in listaBits)
            {
                if (bit.Length > 0)
                {
                    int posBit = int.Parse(bit) - 1;     // numeracion basada en cero.
                    strB[7 - posBit] = '1';
                }
            }

            return Convert.ToInt32(strB.ToString(), 2);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            
            Dictionary <int,Acceso> dic = mainApp.DataManager.LoadAccesos();

        }

        private void btnSTARTAlutrackServer_Click(object sender, EventArgs e)
        {
            try
            {

                btnStartAlutrackServer.Visible = false;
                btnStopAlutrackServer.Visible = true;

                mainApp.alutrackServerThread = new Thread(new ThreadStart(mainApp.ComunicationSystem.socketServerGeneral.StartListening));
                mainApp.alutrackServerThread.Name = "SOCKET SERVER - " + mainApp.alutrackServerThread.ManagedThreadId.ToString();
                // Lanza el thread
                
                mainApp.alutrackServerThread.Start();

                //Espera para que el thread efectivamente comience.
                //          while (!mainApp.mainThread.IsAlive) ;
                Thread.Sleep(500);
                //tmrSend.Enabled = true;
                tmrRebind.Enabled = true;

                // mainApp.ComunicationSystem.socket.readyToDequeueJob = true;
            }
            catch (Exception exc)
            {
                //LOGToFile.doLOGExcepciones("Excepcion no controlada: " + exc.Message + " SALIENDO del programa");

                StaticTools.loguearString("btnSTARTAlutrackServer_Click() - Excepcion no controlada: " + exc.Message + " SALIENDO del programa");

            }

        }

        private void btnSTOPAlutrackServer_Click(object sender, EventArgs e)
        {
            tmrRebind.Enabled = false;
            StaticTools.reBind = false;
            mainApp.ComunicationSystem.socketServerGeneral.stopListening();

          

            Thread.Sleep(500);

           
            try
            {
                StaticTools.obtenerMutex_StateObjectClients();

                Dictionary<string, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;

                List<StateObject> listaClientesBackup = new List<StateObject>();

                foreach (KeyValuePair<string, StateObject> Cliente in listaClientes)
                {
                    StateObject p = new StateObject();
                    p = Cliente.Value;

                    listaClientesBackup.Add(p);
                }

                foreach (StateObject estado in listaClientesBackup)
                {
                    estado.abortAllThreads(null);
                    estado.closeAllSockets();
                }

                //foreach (KeyValuePair<int, StateObject> Cliente in listaClientes)
                //{
                //    Cliente.Value.abortAllThreads(null);
                //    Cliente.Value.closeAllSockets();
                //}


                mainApp.alutrackServerThread.Abort();


                btnStopAlutrackServer.Visible = false;
                btnStartAlutrackServer.Visible = true;
            }
            catch (Exception)
            {
            }
            finally
            {
                StaticTools.liberarMutex_StateObjectClients();
            }

        }

        private void btnStartLenelServer_Click(object sender, EventArgs e)
        {
            btnStartLENELServer.Visible = false;
            btnStopLENELServer.Visible = true;
            

            mainApp.LENELServerThread = new Thread(mainApp.ComunicationSystem.layerLENEL.startListening);
            mainApp.LENELServerThread.Name = "LENEL SERVER - " + mainApp.LENELServerThread.ManagedThreadId.ToString();
            // Lanza el thread del server LENEL
            mainApp.LENELServerThread.Start();

            //Espera para que el thread efectivamente comience.
            Thread.Sleep(500);
            //tmrSend.Enabled = true;
            tmrRebind.Enabled = true;



        }

        private void button6_Click(object sender, EventArgs e)
        {
            lstLenelLOG.Items.Clear();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            lstAlutrackLOG.Items.Clear();
        }

        private void btnStopLENELServer_Click(object sender, EventArgs e)
        {

           
            mainApp.ComunicationSystem.layerLENEL.stopListening();
            Thread.Sleep(500);

            mainApp.LENELServerThread.Abort();

            //tmrSend.Enabled = false;
            //tmrRebind.Enabled = false;
            btnStopLENELServer.Visible = false;
            btnStartLENELServer.Visible = true;

        }

        //Nuevo 25/9
        private void btnStopAlutrackServerWeb_Click(object sender, EventArgs e)
        {

            mainApp.ComunicationSystem.layerLENEL.stopListening();
            Thread.Sleep(500);

            mainApp.LENELServerThread.Abort();

            //tmrSend.Enabled = false;
            //tmrRebind.Enabled = false;
            btnStopLENELServer.Visible = false;
            btnStartLENELServer.Visible = true;

        }
        //Termina lo nuevo 25/9
        private void frmInicial_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                btnSTOPAlutrackServer_Click(sender, e);
                mainApp.ComunicationSystem.layerLENEL.stopListening();

                mainApp.LENELServerThread.Abort();
                while (mainApp.LENELServerThread.IsAlive)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception)
            {
            }
            log4net.LogManager.Shutdown();
        }

        private void button8_Click(object sender, EventArgs e)
        {

            this.Dispose();


            int cant = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients.Count;

            //mainApp.ComunicationSystem.socketServerGeneral.AddJobs();


            SqlConnection cnn = new SqlConnection(dataManager.conexion);
            cnn.Open();
            SqlCommand cmd = cnn.CreateCommand();

            cmd.CommandText = "select max(idEmpleado) from empleado";
            cmd.CommandType = CommandType.Text;

            SqlDataReader lector = cmd.ExecuteReader();

            if (lector.HasRows)
            {
                lector.Read();
                int maximo = int.Parse(lector[0].ToString());
            }



        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            try
            {
                mainApp.ComunicationSystem.layerLENEL.servidorDatos.Stop();
                mainApp.ComunicationSystem.layerLENEL.servidorDatos.Start();
                MessageBox.Show("Servidor de datos de LENEL reseteado con exito");

            }
            catch (Exception )
            {
                MessageBox.Show("Error al resetear el servidor de datos de LENEL");
            }


        }

        private void lstAlutrackLOG_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lstLenelLOG_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        // Version de logueo con Delegates e Invoke
        // ALUTRACK LOG:
        public void actualizarALUTRACKLOG(string texto)
        {
            try
            {
                if (lstAlutrackLOG.InvokeRequired)
                {
                    lstAlutrackLOG.Invoke(new ActualizarALUTRACKLOGDelegate(UpdateALUTRACKLOG), texto);
                }
                else
                {
                    UpdateALUTRACKLOG(texto);
                }
            }
            catch (Exception )
            {
            }

        }

        private void UpdateALUTRACKLOG(string texto)
        {
            lstAlutrackLOG.Items.Insert(0,texto);
            StaticTools.loguearString(texto);
        }

        //Nuevo Matias 25/9
        private void UpdateAlutrackServerLOG(String texto)
        {
            lstAlutrackServerLOG.Items.Insert(0, texto);
            StaticTools.loguearString(texto);

        }

        public void actualizarAlutrackServerLOG(string texto)
        {
            if (lstAlutrackServerLOG.InvokeRequired)
            {
                lstAlutrackServerLOG.Invoke(new ActualizarAlutrackServerLOGDelegate(UpdateAlutrackServerLOG), texto);
            }
            else
            {
                UpdateAlutrackServerLOG(texto);
            }
        }


        //Termina lo nuevo

        // Version de logueo con Delegates e Invoke
        // LENEL LOG:
        public void actualizarLENELLOG(string texto)
        {
            if (lstAlutrackLOG.InvokeRequired)
            {
                lstAlutrackLOG.Invoke(new ActualizarLENELLOGDelegate(UpdateLENELLOG), texto);
            }
            else
            {
                UpdateLENELLOG(texto);
            }
        }

        private void UpdateLENELLOG(string texto)
        {
            
            lstLenelLOG.Items.Insert(0, texto);

            StaticTools.loguearString(texto);
        }

        public void actualizarLOG(string texto, TiposLOG v_tipoLOG)
        {
            DateTime hora = DateTime.Now;

            if ((v_tipoLOG == TiposLOG.HH) && !chkVerboseLOGAlutrack.Checked)
            {
                actualizarALUTRACKLOG(hora.ToLongTimeString() + ":" + hora.Millisecond + " - " + texto);
            }

            if ((v_tipoLOG == TiposLOG.LENEL) && !chkVerboseLOGLenel.Checked)
            {
                actualizarLENELLOG(hora.ToLongTimeString() + ":" + hora.Millisecond + " - " + texto);
            }

            if ((v_tipoLOG == TiposLOG.ALUTRACK) && !chkVerboseLOGAlutrackServerWeb.Checked)
            {
                actualizarAlutrackServerLOG(hora.ToLongTimeString() + ":" + hora.Millisecond + " - " + texto);
            }

        }

        private void button4_Click_2(object sender, EventArgs e)
        {
           

        }

        private void button9_Click(object sender, EventArgs e)
        {
            string res = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");   // POr si falla, devolver esto...

            string conexion = "Data Source=LOCALHOST; Initial Catalog=GPS; User ID=alutel;password=alutel";
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                // Borro el dato actual
                cmd.CommandText = "select top 1 cast(ultimaactualizacion as DateTime) from empleados";
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    DateTime fecha = (DateTime)lector[0];
                    res = fecha.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                }
            }
            catch (Exception )
            {
                //loguearString("Excepcion en obtenerUltimaFechaActualizacionEmpleados -" + ex.Message, TiposLOG.ALUTRACK);
            }
            finally
            {
                cnn.Close();
            }

        }

        private void btnStopAlutrackServerWeb_Click_1(object sender, EventArgs e)
        {
            mainApp.ComunicationSystem.layerALUTRACK.stopListening();
            Thread.Sleep(500);

            mainApp.alutrackServerThread.Abort();
            //tmrSend.Enabled = false;
            //tmrRebind.Enabled = false;
            btnStopAlutrackServerWeb.Visible = false;
            btnStartAlutrackServerWeb.Visible = true;
        }

        private void btnStartAlutrackServerWeb_Click(object sender, EventArgs e)
        {
            btnStartAlutrackServerWeb.Visible = false;
            btnStopAlutrackServerWeb.Visible = true;
    
            mainApp.alutrackServerThread = new Thread(mainApp.ComunicationSystem.layerALUTRACK.startListening);
            mainApp.alutrackServerThread.Name = "ALUTRACK SERVER WEB - " + mainApp.alutrackServerThread.ManagedThreadId.ToString();

            // Lanza el thread del server Alutrack WEB
            
            mainApp.alutrackServerThread.Start();

            //Espera para que el thread efectivamente comience.
            Thread.Sleep(500);
            //tmrSend.Enabled = true;
            tmrRebind.Enabled = true;

        }

        private void btnClearAlutrack_Click(object sender, EventArgs e)
        {
            lstAlutrackServerLOG.Items.Clear();
        }

        
    }
}

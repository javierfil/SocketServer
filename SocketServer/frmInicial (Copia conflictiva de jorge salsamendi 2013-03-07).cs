using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Globalization;

namespace ServerComunicacionesALUTEL
{
    public partial class frmInicial : Form
    {
        AlutrackLayer alu; // Debug 
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
            if (mainApp.ComunicationSystem.puertos.tipoPuertos)
            {
                lblPuertos.Text = "SERVER";
            }
            else
            {
                lblPuertos.Text = "LABORATORIO";
            }


            actualizarListViewAccesos();
            actualizarlistViewVisitas();
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
            try
            {

                btnStart.Visible = false;
                btnStop.Visible = true;

                mainApp.mainThread = new Thread(new ThreadStart(mainApp.ComunicationSystem.socketServerGeneral.StartListening));

                // Lanza el thread
                mainApp.mainThread.Start();

                //Espera para que el thread efectivamente comience.
                //          while (!mainApp.mainThread.IsAlive) ;
                Thread.Sleep(500);
                tmrSend.Enabled = true;
                tmrRebind.Enabled = true;

                // mainApp.ComunicationSystem.socket.readyToDequeueJob = true;
            }
            catch (Exception exc)
            {
                LOGToFile.doLOGExcepciones("Excepcion no controlada: " + exc.Message + " SALIENDO del programa");

            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            tmrRebind.Enabled = false;
            mainApp.ComunicationSystem.socketServerGeneral.reBind = false;
            mainApp.ComunicationSystem.socketServerGeneral.stopListening();

            mainApp.mainThread.Abort();

            Thread.Sleep(500);

//            while (mainApp.mainThread.IsAlive)
            {
            }

            btnStop.Visible = false;
            btnStart.Visible = true;


        }

        /// <summary>
        /// Timer que actualiza los listView con los datos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrSend_Tick(object sender, EventArgs e)
        {
            if (mainApp.ComunicationSystem.actualizeListView)
            {
                if (actualizarListViewAccesos())
                {
                    mainApp.ComunicationSystem.actualizeListView = false;
                }
            }

            if (mainApp.ComunicationSystem.actualizeListViewVisitas)
            {
                

                if (actualizarlistViewVisitas())
                {
                    mainApp.ComunicationSystem.actualizeListViewVisitas = false;
                }
            }

            int cantLog = mainApp.ComunicationSystem.LOGInformation.Count;
            if (cantLog > 0)
            {
                for (int i = 0; i < cantLog; i++)
                {
                    string textToLOG = mainApp.ComunicationSystem.LOGInformation.Dequeue();
                    lstLOG.Items.Insert(0, textToLOG);
                }
            }

            try
            {
                int cantACKsRecibidos = 0;
                int cantACKsEnviados = 0;

                Dictionary<int, StateObject> listaClientes = mainApp.ComunicationSystem.socketServerGeneral.stateObjectsClients;
                foreach (KeyValuePair<int, StateObject> pair in listaClientes)
                {
                    cantACKsRecibidos = cantACKsRecibidos + pair.Value.contadorACKReceive;
                    cantACKsEnviados = cantACKsEnviados + pair.Value.contadorACKSend;
                }

                lblACKRecv.Text = cantACKsRecibidos.ToString();
                lblACKSnd.Text =  cantACKsEnviados.ToString();
            }
            catch (Exception)
            {
                
            }
        }

        private bool actualizarlistViewVisitas()
        {
            bool success = false;

            listViewVisitas.View = View.Details;
            listViewVisitas.GridLines = true;
            listViewVisitas.Columns.Clear();
            listViewVisitas.Columns.Add("Nro.", 50, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Hand Held", 100, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Tarjeta", 100, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Nombre", 100, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Apellido", 100, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Documento", 100, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Empresa", 100, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Hora", 150, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Latitud", 50, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Longitud", 70, HorizontalAlignment.Left);
            listViewVisitas.Columns.Add("Tipo", 60, HorizontalAlignment.Left);
            listViewVisitas.Items.Clear();

            try
            {
                Dictionary<string, Visita> listaVisitas = mainApp.DataManager.getListaVisitas();

                foreach (KeyValuePair<string, Visita> pair in listaVisitas)
                {
                    ListViewItem list;
                    list = listViewVisitas.Items.Add(pair.Key);
                    list.SubItems.Add(pair.Value.HHID);
                    list.SubItems.Add(pair.Value.Tarjeta);
                    list.SubItems.Add(pair.Value.Nombre);
                    list.SubItems.Add(pair.Value.Apellido);
                    list.SubItems.Add(pair.Value.Documento);
                    list.SubItems.Add(pair.Value.Empresa);

                    list.SubItems.Add(pair.Value.Hora);
                    list.SubItems.Add(pair.Value.Latitud);
                    list.SubItems.Add(pair.Value.Longitud);
                    list.SubItems.Add(pair.Value.tipoAcceso.ToString());

                }
                this.listViewVisitas.MultiSelect = true;
                this.listViewVisitas.HideSelection = false;
                this.listViewVisitas.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                success = true;
            }
            catch (Exception )
            {
            }

            return success; 
        }

        private bool actualizarListViewAccesos()
        {
            bool success = false;

            listViewAccesos.View = View.Details;
            listViewAccesos.GridLines = true;
            listViewAccesos.Columns.Clear();
            listViewAccesos.Columns.Add("Nro.", 50, HorizontalAlignment.Left);
            listViewAccesos.Columns.Add("HandHeld Id", 100, HorizontalAlignment.Left);
            listViewAccesos.Columns.Add("Badge", 100, HorizontalAlignment.Left);
            listViewAccesos.Columns.Add("DateTime", 150, HorizontalAlignment.Left);
            listViewAccesos.Columns.Add("Latitude", 50, HorizontalAlignment.Left);
            listViewAccesos.Columns.Add("Longitude", 70, HorizontalAlignment.Left);
            listViewAccesos.Columns.Add("Type", 60, HorizontalAlignment.Left);
            listViewAccesos.Items.Clear();

            try
            {
                Dictionary<int, Acceso> listaAccesos = mainApp.DataManager.LoadAccesos();

                foreach (KeyValuePair<int, Acceso> pair in listaAccesos)
                {
                    ListViewItem list;
                    list = listViewAccesos.Items.Add(pair.Key.ToString().PadLeft(4, '0'));
                    list.SubItems.Add(pair.Value.HHID);
                    list.SubItems.Add(pair.Value.Tarjeta);
                    list.SubItems.Add(pair.Value.Hora);
                    list.SubItems.Add(pair.Value.Latitud);
                    list.SubItems.Add(pair.Value.Longitud);
                    list.SubItems.Add(pair.Value.tipoAcceso.ToString());

                }
                this.listViewAccesos.MultiSelect = true;
                this.listViewAccesos.HideSelection = false;
                this.listViewAccesos.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                success = true;
            }
            catch (Exception )
            {
                // Do nothing... :)
            }

            return success;
     
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void listViewAccesos_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dictionary<int, Acceso> listaAccesos = mainApp.DataManager.getListaAccesos();

            ListView.SelectedListViewItemCollection itemCollection = listViewAccesos.SelectedItems;
            int idAcceso = -1;
            string HH = "";
            string tarjeta = "";
            string hora = "";
            string latitud = "";
            string longitud = "";
            string datosUsuario = "";
            if (itemCollection.Count > 0)
            {
                ListViewItem item = itemCollection[0];
                idAcceso = (int.Parse((item.SubItems[0].Text)));
                HH = (item.SubItems[1].Text);
                tarjeta = (item.SubItems[2].Text);
                hora = (item.SubItems[3].Text);
                latitud = mainApp.InterpretarLatitud(item.SubItems[4].Text);
                longitud = mainApp.InterpretarLongitud(item.SubItems[5].Text);

                int idEmpleado;
                Employee emp = mainApp.DataManager.buscarEmpleadoxTarjeta(tarjeta);

                if (emp != null)
                {
                    idEmpleado = emp.Id;

                    datosUsuario = mostrarUsuario(idEmpleado);
                    lblbadge.Text = tarjeta;
                    lbltype.Text = (item.SubItems[6].Text);
                }
            }

            if (idAcceso != -1)
            {
                if (latitud != "" && longitud != "")
                {
                    // OJO: globales aca....
                    generalLat = latitud;
                    generalLong = longitud;

                    ActualizarMapa();
                    webBrowser1.Visible = true;
                }
                else
                {
                    webBrowser1.Visible = false;
                }

                if (listaAccesos[idAcceso].imagen != null)
                {
                    if (listaAccesos[idAcceso].imagen.Length > 0)
                    {
                        File.WriteAllBytes( "temp.jpg", listaAccesos[idAcceso].imagen);
                        picAcceso.Load("temp.jpg");
                    }
                    else
                        picAcceso.Image = null;
                }
                else
                {
                    picAcceso.Image = null;
                }
            }
        }

        private void ActualizarMapa()
        {
            StreamReader streamReader = new StreamReader("MapOriginal.html");
            string text = streamReader.ReadToEnd();
            streamReader.Close();

            text = text.Replace(@"[ZOOM]", generalZoom);
            text = text.Replace(@"[LAT]", generalLat);
            text = text.Replace(@"[LONG]", generalLong);

            String PersonalFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

           

            StreamWriter writer = new StreamWriter(@PersonalFolder + @"\\Map.html");

            writer.Write(text);
            writer.Close();
            webBrowser1.Navigate(@PersonalFolder + @"/Map.html");
        }


        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tarjeta"></param>
        private string mostrarUsuario(int idEmpleado)
        {
            Dictionary<int, Employee> listaUsuarios = mainApp.DataManager.getListaEmpleados();

            string result = "";


            if (listaUsuarios.ContainsKey(idEmpleado))
            {
                Employee emp = listaUsuarios[idEmpleado];

                lblName.Text = emp.Apellido+", " + emp.Nombre;

                result = emp.Apellido + ", " + emp.Nombre;

                if (emp.Imagen != null)
                {
                    if (emp.Imagen.Length > 0)
                    {
                        //File.WriteAllBytes("temp.jpg", listaAccesos[numero].imagen);
                        string imageFilname = Path.GetFileName(emp.Imagen);
                        string imageFullPath = SystemConfiguration.ImagesPath + @"/" + imageFilname;

                        picEmpleado.Load(imageFullPath);
                    }
                    else
                        picEmpleado.Image = null;
                }
                else
                {
                    picEmpleado.Image = null;
                }

            }

            return result;
        }

       
        private void button3_Click_1(object sender, EventArgs e)
        {
            Employee emp = new Employee();

            //emp.Tarjeta = "1234";
            emp.Nombre = "Javier";
            emp.Apellido = "Filippini";
            //emp.Acceso = true;
            mainApp.DataManager.addUser("1234", emp);
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
            mainApp.ComunicationSystem.actualizarLENEL = chkActualizarLENEL.Checked;
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

        private void listViewVisitas_SelectedIndexChanged(object sender, EventArgs e)
        {
            Dictionary<string, Visita> listaVisitas = mainApp.DataManager.getListaVisitas();

            ListView.SelectedListViewItemCollection itemCollection = listViewVisitas.SelectedItems;
            string numero = "";

            if (itemCollection.Count > 0)
            {
                numero = itemCollection[0].SubItems[0].Text;

                if (listaVisitas[numero].imagen != null)
                {
                    if (listaVisitas[numero].imagen.Length > 0)
                    {
                        File.WriteAllBytes("temp.jpg", listaVisitas[numero].imagen);
                        picAcceso.Load("temp.jpg");
                    }
                    else
                        picAcceso.Image = null;
                }
                else
                {
                    picAcceso.Image = null;
                }
            }
        }

        private void chkActualizarLENEL_CheckedChanged_1(object sender, EventArgs e)
        {
            mainApp.ComunicationSystem.actualizarLENEL = chkActualizarLENEL.Checked;
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
           lstLOG.Items.Clear();
        }

        private void frmInicial_FormClosed(object sender, FormClosedEventArgs e)
        {
         
        }

        private void tmrRebind_Tick(object sender, EventArgs e)
        {
            if (mainApp.ComunicationSystem.socketServerGeneral.reBind)
            {
                LOGToFile.doLOG("Activado el ReBIND de los sockets");

                //Thread.Sleep(1000);
                mainApp.ComunicationSystem.socketServerGeneral.reBind = false;
                mainApp.ComunicationSystem.socketServerGeneral.stopListening();
                mainApp.mainThread.Abort();
                
                LOGToFile.doLOG("Sockets de escucha Cerrados y thread de escucha Detenido.");

                Thread.Sleep(1000);

                mainApp.mainThread = new Thread(new ThreadStart(mainApp.ComunicationSystem.socketServerGeneral.StartListening));

                // Lanza el thread
                mainApp.mainThread.Start();

                //Espera para que el thread efectivamente comience.
                //while (!mainApp.mainThread.IsAlive) ;

                Thread.Sleep(100);

                LOGToFile.doLOG("THREAD de escucha vuelto a levantar. Server escuchando");

            }
        }

        private void button3_Click_3(object sender, EventArgs e)
        {
            double p = 1.234f;

            string d = p.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }
        private void button3_Click_4(object sender, EventArgs e)
        {
            string message;

            bool res = mainApp.ComunicationSystem.socketServerGeneral.layerAlutrack.conectar(txtIDDevice.Text, out message);
            if (!res)
            {
                MessageBox.Show(message);
            }


        }
        private void button4_Click_1(object sender, EventArgs e)
        {
            string message = "";
            bool res = mainApp.ComunicationSystem.socketServerGeneral.layerAlutrack.sendGPSData("P001",txtLat.Text, txtLong.Text,"90.344","17.334",out message);

            if (!res)
            {
                MessageBox.Show(message);

            }
        }
    }
}

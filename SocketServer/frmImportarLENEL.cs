using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Management;


namespace ServerComunicacionesALUTEL
{
    public partial class frmImportarLENEL : Form
    {

        private Aplicacion mainApp;

        ManagementClass BadgeClass;
        ManagementClass CardHolderClass;

        ManagementObjectCollection coleccionBadges;
        ManagementObjectCollection coleccionCardHolders;


        public frmImportarLENEL(Aplicacion v_app)
        {
            mainApp = v_app;
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void frmImportarLENEL_Load(object sender, EventArgs e)
        {
            BadgeClass = new ManagementClass("root\\OnGuard", "Lnl_Badge", null);
            CardHolderClass = new ManagementClass("root\\OnGuard", "Lnl_CardHolder", null);

            if (BadgeClass != null)
            {
                
                coleccionBadges = BadgeClass.GetInstances();
            }

            if (CardHolderClass != null)
            {
                
                coleccionCardHolders = CardHolderClass.GetInstances();
            }
            
            updateListaUsuarios();
        }

        private void updateListaUsuarios()
        {

            listViewUsuarios.View = View.Details;
            listViewUsuarios.GridLines = true;
            listViewUsuarios.Columns.Clear();
            listViewUsuarios.Columns.Add("Tarjeta", 100, HorizontalAlignment.Left);
            listViewUsuarios.Columns.Add("Nombre", 100, HorizontalAlignment.Left);
            listViewUsuarios.Columns.Add("Apellido", 100, HorizontalAlignment.Left);
            listViewUsuarios.Columns.Add("Acceso", 100, HorizontalAlignment.Left);
            listViewUsuarios.Items.Clear();

            foreach (ManagementBaseObject cardHolder in coleccionCardHolders)
            {
                if (cardHolder.Properties["FIRSTNAME"].Value != null)
                {
                    string nombre = (string)cardHolder.Properties["FIRSTNAME"].Value;
                    string apellido = (string)cardHolder.Properties["LASTNAME"].Value;

                    int empID = (int)cardHolder.Properties["ID"].Value;
                    long tarjeta=0;
                    int status =0;
                    //MessageBox.Show("vA A BUSCAR UNA TARJETA Y STATUS DE: " + empID);
                    BuscarTarjetayStatus(empID, out tarjeta, out status);
                    //MessageBox.Show("vOLVIO DE LA BUSQUEDA. tarjeta: " + tarjeta.ToString() + " STATUS: "+ status.ToString());
                    string txStatus = "";
                    if (status == 1)
                    {
                        txStatus = "Permitido";
                    }

                    if (status  ==2)
                    {
                        txStatus = "Denegado";
                    }

                    ListViewItem list;
                    list = listViewUsuarios.Items.Add(tarjeta.ToString());
                    list.SubItems.Add(nombre);
                    list.SubItems.Add(apellido);
                    list.SubItems.Add(txStatus);
                }
            }
            
            
            this.listViewUsuarios.MultiSelect = true;
            this.listViewUsuarios.HideSelection = false;
            this.listViewUsuarios.HeaderStyle = ColumnHeaderStyle.Nonclickable;

        }


        private void BuscarTarjetayStatus(int empID, out long tarjeta, out int status)
        {
            tarjeta = 0;
            status = 0;

            foreach (ManagementBaseObject badge in coleccionBadges)
            {
                if ((int)badge.Properties["PERSONID"].Value == empID)
                {
                    tarjeta = (long)badge.Properties["ID"].Value;
                    status = (int)badge.Properties["STATUS"].Value;
                    break;
                }
            }
        }

        private void btnImportar_click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection  listaElegidos= listViewUsuarios.SelectedItems;

            foreach (ListViewItem item in listaElegidos)
            {
                string tarjeta = item.SubItems[0].Text;
                string Nombre = item.SubItems[1].Text;
                string Apellido = item.SubItems[2].Text;
                string Acceso = item.SubItems[3].Text;

                Employee nuevoEmpleado = new Employee();
                //nuevoEmpleado.Tarjeta = tarjeta;
                nuevoEmpleado.Nombre = Nombre;
                nuevoEmpleado.Apellido = Apellido;
                //bool realAccess = true ;
                //if (Acceso == "Permitido")
                //{
                //    realAccess = true;
                //}
                //else
                //{
                //    realAccess = false;
                //}

                //nuevoEmpleado.Acceso = realAccess;

                

            }
            MessageBox.Show("Agregados " + listaElegidos.Count + " empleados al sistema");
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            //this.Dispose();

            listViewUsuarios.View = View.Details;
            listViewUsuarios.GridLines = true;
            listViewUsuarios.Columns.Clear();
            listViewUsuarios.Columns.Add("Badge", 100, HorizontalAlignment.Left);
            listViewUsuarios.Columns.Add("Name", 100, HorizontalAlignment.Left);
            listViewUsuarios.Columns.Add("Lastname", 100, HorizontalAlignment.Left);
            listViewUsuarios.Columns.Add("Access", 100, HorizontalAlignment.Left);
            listViewUsuarios.Items.Clear();






            ListViewItem list;
            list = listViewUsuarios.Items.Add("879893");
            list.SubItems.Add("Maria");
            list.SubItems.Add("González");
            list.SubItems.Add("Granted");

            list = listViewUsuarios.Items.Add("767885");
            list.SubItems.Add("Juan Raúl");
            list.SubItems.Add("Berttoni");
            list.SubItems.Add("Granted");

            list = listViewUsuarios.Items.Add("889384");
            list.SubItems.Add("José");
            list.SubItems.Add("Grudzien");
            list.SubItems.Add("Granted");

            list = listViewUsuarios.Items.Add("66745");
            list.SubItems.Add("Louis");
            list.SubItems.Add("Jordan");
            list.SubItems.Add("Granted");

            list = listViewUsuarios.Items.Add("887341");
            list.SubItems.Add("Michel");
            list.SubItems.Add("Grudzien");
            list.SubItems.Add("Granted");

            list = listViewUsuarios.Items.Add("663573");
            list.SubItems.Add("Javier");
            list.SubItems.Add("Filippini");
            list.SubItems.Add("Denied");







        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ServerComunicacionesALUTEL
{
    public partial class frmGrupos : Form
    {
        private Aplicacion mainApp;
        public frmGrupos(Aplicacion v_app)
        {
            mainApp = v_app;
            InitializeComponent();
        }

        private void frmGrupos_Load(object sender, EventArgs e)
        {
            updateListaGrupos();
        }

        private void btnCrearGrupoUsuarios_Click(object sender, EventArgs e)
        {

            if (!mainApp.formCrearGrupoUsuarios.IsDisposed)
            {
                mainApp.formCrearGrupoUsuarios.ShowDialog();
               
                updateListaGrupos();
                
            }
            else
            {
                mainApp.formCrearGrupoUsuarios = new frmCrearGrupoUsuarios(mainApp);
                mainApp.formCrearGrupoUsuarios.ShowDialog();
               
                updateListaGrupos();
            }
        }

        public void updateListaGrupos()
        {
            lstGruposUsuarios.Items.Clear();

            listViewUsuarios.View = View.Details;
            listViewUsuarios.GridLines = true;
            listViewUsuarios.Columns.Clear();
            listViewUsuarios.Columns.Add("Tarjeta", 70, HorizontalAlignment.Left);
            listViewUsuarios.Columns.Add("Nombre", 100, HorizontalAlignment.Left);
            listViewUsuarios.Columns.Add("Apellido", 100, HorizontalAlignment.Left);
            listViewUsuarios.Items.Clear();

            
            this.listViewUsuarios.MultiSelect = true;
            this.listViewUsuarios.HideSelection = false;
            this.listViewUsuarios.HeaderStyle = ColumnHeaderStyle.Nonclickable;


            lstGruposDevices.Items.Clear();
            lstDevicesDelGrupo.Items.Clear();

            Dictionary<string, List<int>> gruposUsuarios = mainApp.DataManager.getGruposUsuarios();
            Dictionary<string, List<string>> gruposDevices = mainApp.DataManager.getGruposDevices();

            foreach (KeyValuePair<string, List<int>> pair in gruposUsuarios)
            {
                lstGruposUsuarios.Items.Add(pair.Key);
            }

            foreach (KeyValuePair<string, List<string>> pair in gruposDevices)
            {
                lstGruposDevices.Items.Add(pair.Key);
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnCrearGrupoDevices_Click(object sender, EventArgs e)
        {

            if (!mainApp.formCrearGrupoDevices.IsDisposed)
            {
                mainApp.formCrearGrupoDevices.ShowDialog();

                updateListaGrupos();

            }
            else
            {
                mainApp.formCrearGrupoDevices = new frmCrearGrupoDevices(mainApp);
                mainApp.formCrearGrupoDevices.ShowDialog();

                updateListaGrupos();
            }

        }

        private void lstGruposUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (lstGruposUsuarios.SelectedIndex >= 0)
            {
                string idGrupo = (string)lstGruposUsuarios.SelectedItem;

                Dictionary<string, List<int>> gruposUsuarios = mainApp.DataManager.getGruposUsuarios();

                Dictionary<int, Employee> listaUsuarios = mainApp.DataManager.getListaEmpleados();

                List<int> idUsuarios = gruposUsuarios[idGrupo];

                listViewUsuarios.Items.Clear();

                foreach (int s in idUsuarios)
                {
                    if (listaUsuarios.ContainsKey(s))
                    {
                        ListViewItem list;
                        list = listViewUsuarios.Items.Add(listaUsuarios[s].Id.ToString());
                        list.SubItems.Add(listaUsuarios[s].Nombre);
                        list.SubItems.Add(listaUsuarios[s].Apellido);
                    }
                }
            }
        }

        private void lstGruposDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstGruposDevices.SelectedIndex >= 0)
            {
                string idGrupo = (string)lstGruposDevices.SelectedItem;

                Dictionary<string, List<string>> gruposDevices = mainApp.DataManager.getGruposDevices();

                Dictionary<string, device> listaDevices = mainApp.DataManager.getListaDevices();

                List<string> idDevices = gruposDevices[idGrupo];

                lstDevicesDelGrupo.Items.Clear();

                foreach (string s in idDevices)
                {
                    if (listaDevices.ContainsKey(s))
                    {
                        lstDevicesDelGrupo.Items.Add(listaDevices[s].ID);
                    }
                }
            }












        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}

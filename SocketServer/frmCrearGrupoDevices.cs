using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ServerComunicacionesALUTEL
{
    public partial class frmCrearGrupoDevices : Form
    {

        private Aplicacion mainApp;

        private Dictionary<string, device> listaDevices;

        public frmCrearGrupoDevices(Aplicacion v_aplicacion)
        {
            mainApp = v_aplicacion;
            listaDevices = mainApp.DataManager.getListaDevices();

            InitializeComponent();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void frmCrearGrupoDevices_Load(object sender, EventArgs e)
        {
            updateListadevices();
        }

        private void updateListadevices()
        {
            lstDevices.Items.Clear();
            foreach (KeyValuePair<string, device> pair in listaDevices)
            {
                lstDevices.Items.Add(pair.Key);
            }
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {

            string nombreGrupo = txtNombreGrupo.Text.Trim();
            if (nombreGrupo != "")
            {
                //ListView.SelectedListViewItemCollection listaElegidos = listViewUsuarios.SelectedItems;
                ListBox.SelectedObjectCollection listaElegidos = lstDevices.SelectedItems;

                Dictionary<string, List<string>> gruposDevices = mainApp.DataManager.getGruposDevices();

                if (!gruposDevices.ContainsKey(nombreGrupo) && listaElegidos.Count > 0)
                {
                    List<string> listaDevices = new List<string>();

                    foreach (Object item in listaElegidos)
                    {
                        string id = (string)item;
                        listaDevices.Add(id);
                    }
                    mainApp.DataManager.addGrupoDevices(nombreGrupo, listaDevices);

                    MessageBox.Show("Grupo " + nombreGrupo + " creado", "Grupo creado correctamente");
                    txtNombreGrupo.Text = "";
                }
            }





        }
    }
}

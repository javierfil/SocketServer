using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ServerComunicacionesALUTEL
{
    public partial class frmCrearGrupoUsuarios : Form
    {
        private Aplicacion mainApp;

        private Dictionary<int, Employee> listaUsuarios;

        public frmCrearGrupoUsuarios(Aplicacion v_aplicacion)
        {
            mainApp = v_aplicacion;

            listaUsuarios = mainApp.DataManager.getListaEmpleados();

            InitializeComponent();
        }

        private void frmCrearGrupo_Load(object sender, EventArgs e)
        {
          
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
            listViewUsuarios.Items.Clear();

            foreach (KeyValuePair<int, Employee> pair in listaUsuarios)
            {
                ListViewItem list;
                list = listViewUsuarios.Items.Add(pair.Key.ToString());
                list.SubItems.Add(pair.Value.Nombre);
                list.SubItems.Add(pair.Value.Apellido);

            }
            this.listViewUsuarios.MultiSelect = true;
            this.listViewUsuarios.HideSelection = false;
            this.listViewUsuarios.HeaderStyle = ColumnHeaderStyle.Nonclickable;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string nombreGrupo = txtNombreGrupo.Text.Trim();
            if (nombreGrupo != "")
            {
                ListView.SelectedListViewItemCollection  listaElegidos= listViewUsuarios.SelectedItems;

                Dictionary<string, List<int>> gruposUsuarios = mainApp.DataManager.getGruposUsuarios();

                if (!gruposUsuarios.ContainsKey(nombreGrupo) && listaElegidos.Count>0)
                {
                    List<int> listaUsuarios = new List<int>();

                    foreach (ListViewItem item in listaElegidos)
                    {
                        int id = Convert.ToInt32(item.SubItems[0].Text);
                        listaUsuarios.Add(id);
                    }
                    mainApp.DataManager.addGrupoUsuarios(nombreGrupo, listaUsuarios);

                    MessageBox.Show("Grupo " + nombreGrupo + " creado", "Grupo creado correctamente");
                    txtNombreGrupo.Text = "";
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void listViewUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}

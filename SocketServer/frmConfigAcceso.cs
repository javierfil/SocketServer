using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ServerComunicacionesALUTEL
{
    public partial class frmConfigAcceso : Form
    {
        private Aplicacion mainApp;

        private Dictionary<string, List<int>> gruposUsuarios;
        private Dictionary<string, List<string>> gruposDevices;
        private Dictionary<int, Employee> listaUsuarios;
        private Dictionary<string, device> listaDevices;
   

        private Dictionary<string, KeyValuePair<string, string>> reglasAcceso;


        public frmConfigAcceso(Aplicacion v_app)
        {
            mainApp = v_app;

            gruposUsuarios = mainApp.DataManager.getGruposUsuarios();
            gruposDevices = mainApp.DataManager.getGruposDevices();
            reglasAcceso = mainApp.DataManager.getReglasAcceso();
            listaUsuarios = mainApp.DataManager.getListaEmpleados();
            listaDevices = mainApp.DataManager.getListaDevices();

            InitializeComponent();
        }

        private void frmConfigAcceso_Load(object sender, EventArgs e)
        {
            updateControls();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void updateControls()
        {
            lstReglas.Items.Clear();
            lstUsuarios.Items.Clear();
            lstDevices.Items.Clear();
            cmbGrupoDispositivos.Items.Clear();
            cmbGrupoUsuarios.Items.Clear();

            foreach (KeyValuePair<string, KeyValuePair<string, string>> pair in reglasAcceso)
            {
                lstReglas.Items.Add(pair.Key);
            }

            foreach (KeyValuePair<string, List<int>> pair in gruposUsuarios)
            {
                cmbGrupoUsuarios.Items.Add(pair.Key);
            }

            foreach (KeyValuePair<string, List<string>> pair in gruposDevices)
            {
                cmbGrupoDispositivos.Items.Add(pair.Key);
            }
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            string grupoUsuario = cmbGrupoUsuarios.Text;
            string grupoDevice = cmbGrupoDispositivos.Text;

            if (grupoUsuario != "" && grupoDevice !="")
            {
                string idRegla = grupoUsuario + "---->" + grupoDevice;
                if (!reglasAcceso.ContainsKey(idRegla))
                {
                    KeyValuePair<string, string> asociacion = new KeyValuePair<string, string>(grupoUsuario, grupoDevice);
                    mainApp.DataManager.addReglaAcceso(idRegla, asociacion);
                    updateControls();
                }
            }
        }

        private void lstReglas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstReglas.SelectedIndex >= 0)
            {
                lstUsuarios.Items.Clear();
                lstDevices.Items.Clear();
                string idRegla = (string)lstReglas.SelectedItem;
                if (reglasAcceso.ContainsKey(idRegla))
                {
                    KeyValuePair<string, string> asociacion = reglasAcceso[idRegla];

                    string grupoUsuarios = asociacion.Key;
                    string grupoDevices = asociacion.Value;
                    if (gruposUsuarios.ContainsKey(grupoUsuarios))
                    {
                        List<int> lUsuarios = gruposUsuarios[grupoUsuarios];
                        
                        foreach (int idUsuario in lUsuarios)
                        {
                            if (listaUsuarios.ContainsKey(idUsuario))
                            {
                                lstUsuarios.Items.Add(listaUsuarios[idUsuario].Nombre);
                            }
                        }
                    }

                    if (gruposDevices.ContainsKey(grupoDevices))
                    {
                        List<string> lDevices = gruposDevices[grupoDevices];

                        foreach (string idDevice in lDevices)
                        {
                            if (listaDevices.ContainsKey(idDevice))
                            {
                                lstDevices.Items.Add(listaDevices[idDevice].ID);
                            }
                        }
                    }
                }
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (lstReglas.SelectedIndex >= 0)
            {
                string idRegla = (string)lstReglas.SelectedItem;
                mainApp.DataManager.removeReglaAcceso(idRegla);
                updateControls();
            }
        }



    }
}

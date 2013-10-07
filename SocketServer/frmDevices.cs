using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ServerComunicacionesALUTEL
{
    public partial class frmDevices : Form
    {
        private Aplicacion mainApp;
        private Dictionary<string, device> listaDevices;
        
        public frmDevices(Aplicacion v_app)
        {

            mainApp = v_app;

            listaDevices = mainApp.DataManager.getListaDevices();
            
            InitializeComponent();
        }

        private void frmDevices_Load(object sender, EventArgs e)
        {
            updateListaDevices();
         
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void btnAddDevice_Click(object sender, EventArgs e)
        {
            string idHandHeld = "";
            string marca = "";
            string modelo = "";
            if (txtIDDevice.Text.Trim() != "")
            {
                idHandHeld = txtIDDevice.Text;
                marca = cbdbrand.SelectedValue.ToString();
                modelo = cbdmodel.Text;
                device newDevice = new device(idHandHeld, marca, modelo, 0);

                mainApp.DataManager.addDevice(newDevice);
                updateListaDevices();
                txtIDDevice.Text = "";
            }
            else
            {
                MessageBox.Show("Debe ingresarle un nombre al dispositivo");
            }
        }


        private void updateListaDevices()
        {
            lstDevices.Items.Clear();

            listaDevices = mainApp.DataManager.getListaDevices();

            foreach (KeyValuePair<string, device> pair in listaDevices)
            {
                lstDevices.Items.Add(pair.Value.ID);            // POr ahora solo el ID
            }

        }

        private void btnRemoveDevice_Click(object sender, EventArgs e)
        {

            int seleccionado = lstDevices.SelectedIndex;
            if (seleccionado >= 0)
            {
                string idDevice = (string)lstDevices.Items[seleccionado];

                mainApp.DataManager.removeDevice(idDevice);
                updateListaDevices();
            }
        }
    }
}

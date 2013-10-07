using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ServerComunicacionesALUTEL
{
    public partial class frmConfiguracion : Form
    {

        private Aplicacion mainApp;

        private Dictionary<string, string> camposLenelCamposSistema;

        public frmConfiguracion(Aplicacion v_app)
        {
            mainApp = v_app;

            camposLenelCamposSistema = mainApp.DataManager.getCamposLenelCamposSistema();
            InitializeComponent();
        }

        private void frmConfiguracion_Load(object sender, EventArgs e)
        {
            updateControls();
        }

        private void updateControls()
        {
            txtOnGuardPort1.Text = SystemConfiguration.OnGuardPort1.ToString();
            txtOnGuardPort2.Text = SystemConfiguration.OnGuardPort2.ToString();
            txtSendPort.Text = SystemConfiguration.SendPort.ToString();
            txtReceivePort.Text = SystemConfiguration.ReceivePort.ToString();
            txtImagesPath.Text = SystemConfiguration.ImagesPath;
            txtSystemDataSource.Text = SystemConfiguration.DataSource;
            txtDatabaseName.Text = SystemConfiguration.DataBaseName;
            txtUserName.Text = SystemConfiguration.DBUserName;
            txtPassword.Text = SystemConfiguration.DBPassword;
          
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            try
            {
                SystemConfiguration.OnGuardPort1 = int.Parse(txtOnGuardPort1.Text);
                SystemConfiguration.OnGuardPort2 = int.Parse(txtOnGuardPort2.Text);

                SystemConfiguration.SendPort = int.Parse(txtSendPort.Text);
                SystemConfiguration.ReceivePort = int.Parse(txtReceivePort.Text);
                
                if (grpGPSControl.Enabled)
                {
                    SystemConfiguration.GPSControlIP = txtGPSControlIP.Text;
                    SystemConfiguration.GPSControlPort = int.Parse(txtGPSControlPort.Text);
                }

                SystemConfiguration.ImagesPath = txtImagesPath.Text;
                SystemConfiguration.DataSource = txtSystemDataSource.Text;
            
                SystemConfiguration.DataBaseName = txtDatabaseName.Text;
  
                SystemConfiguration.SaveConfiguration();
                this.Tag = true;
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in data configuration. Please update the data and retry." + ex.Message);
            }
        }

        private void btnSelPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtImagesPath.Text = dialog.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Tag = false;
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void txtDatabaseName_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmConfiguracion_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void frmConfiguracion_Activated(object sender, EventArgs e)
        {
            this.Tag = false;
        }

        private void btnResetDB_Click(object sender, EventArgs e)
        {
            string dataSource = txtSystemDataSource.Text;
            string databaseName = txtDatabaseName.Text;
            string userName = txtUserName.Text;
            string password = txtPassword.Text;

            if (dataSource.Length > 0 && databaseName.Length > 0)
            {
                DialogResult choice = MessageBox.Show("This will install a new and clean version of " + databaseName + " on " + dataSource + ". Warning!: This CAN NOT be undone. Do you really want to continue?", "Confirmation Required", MessageBoxButtons.YesNo);

                if (choice == DialogResult.Yes)
                {
                    string conexion = "Data Source=LOCALHOST; Initial Catalog=master; User ID=" + userName + ";password=" + password;

                    SqlConnection cnn = new SqlConnection(conexion);
                    try
                    {
                        cnn.Open();
                        SqlCommand cmd = cnn.CreateCommand();
                        string appPath = SystemConfiguration.applicationPath;
                        string destPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                        string comando = "restore database " + databaseName + " from disk='" + appPath + @"\GPSClean2.bak' WITH REPLACE, MOVE 'GPS' to '" + destPath + @"\GPS.mdf', MOVE 'GPS_log' to'" + destPath + @"\GPS_1.ldf'";
                        // Borro el dato actual
                        cmd.CommandText = comando;
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Database: " + dataSource + @"\" + databaseName + " successfully created");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception resetting DB to: " + dataSource + @"\" + databaseName + ": " + ex.Message + ". You should first dettach the existing GPS database");
                    }
                    finally
                    {
                        cnn.Close();
                    }

                }
            }
    
        }
    }
}

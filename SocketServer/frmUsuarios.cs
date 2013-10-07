using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ServerComunicacionesALUTEL
{
    public partial class frmUsuarios : Form
    {
        private Aplicacion mainApp;
        private Dictionary<int, Employee> listaUsuarios;

        List<int> keyList;

        int actualKey;

        public frmUsuarios(Aplicacion v_app)
        {
            mainApp = v_app;

            listaUsuarios = mainApp.DataManager.getListaEmpleados();
            actualKey = 0;

            InitializeComponent();
        }


        private void frmUsuarios_Load(object sender, EventArgs e)
        {
            keyList = new List<int>(this.listaUsuarios.Keys);

            // Llena las categorias del Combo Box
            String[] names = Enum.GetNames(typeof(CategoríasLibretasConducir));

            foreach (string s in names)
            {
                cmbCatLibCond.Items.Add(s);
            }

            verUsuario();
        }

        // usa actualKey para ver un usuario.
        private void verUsuario()
        {
            int key = keyList[actualKey];

            if (listaUsuarios.ContainsKey(key))
            {
                Employee emp = listaUsuarios[key];


                //txtTarjeta.Text = emp.Tarjeta;
                txtNombre.Text = emp.Nombre;
                txtApellido.Text = emp.Apellido;
                if (emp.Sexo)
                {
                    rdbSexoM.Checked = true;
                    rdbSexoF.Checked = false;
                }
                else
                {
                    rdbSexoM.Checked = false;
                    rdbSexoF.Checked = true;
                }

                dtFechaNac.Value = emp.FechaNacimiento;
              //  txtFuncion.Text = emp.Funcion;
                //if (emp.Acceso)
                //{
                //    rdbAccessOK.Checked = true;
                //    rdbAccessDny.Checked = false;
                //}
                //else
                //{
                //    rdbAccessOK.Checked = false;
                //    rdbAccessDny.Checked = true;
                //}

                txtDireccion.Text = emp.Direccion;
                txtdepto.Text = emp.Departamento;
                txtNacionalidad.Text = emp.Nacionalidad;
                txtTelefono.Text = emp.Telefono;
                //switch (emp.EstadoCivil)
                //{
                //    case EstadosCiviles.Casado:
                //        rdbCasado.Checked = true;
                //        rdbSoltero.Checked = false;
                //        rdbViudo.Checked = false;
                //        break;
                //    case EstadosCiviles.Soltero:
                //        rdbCasado.Checked = false;
                //        rdbSoltero.Checked = true;
                //        rdbViudo.Checked = false;
                //        break;
                //    case EstadosCiviles.Viudo:
                //        rdbCasado.Checked = false;
                //        rdbSoltero.Checked = false;
                //        rdbViudo.Checked = true;
                //        break;
                //}
                //txtNombConyugue.Text = emp.NombreConyugue;
                //txtNomPadre.Text = emp.NombrePadre;
                //txtNomMadre.Text = emp.NombreMadre;
                //txtEscolaridad.Text = emp.NivelEscolaridad;
                dtValCarSal.Value = emp.FechaVencimientoCarnetSalud;

                txtSSNO.Text = emp.NumeroDocumento;
                dtExpSSNO.Value = emp.FechaExpedicionDocumento;
                dtVenSSNO.Value = emp.FechaVencimientoDocumento;
                //cmbCatLibCond.Text = emp.CategoriaLibretaConducir.ToString();
                //dtVenLibCond.Value = emp.VencimientoLibretaConducir;
                dtIngBPS.Value = emp.FechaIngresoBPS;

                //txtTipoAport.Text = emp.TipoAportación;
                //dtVigenciaBSE.Value = emp.VigenciaBSE;

                if (emp.hasImage())
                {
                    File.WriteAllBytes("temp.jpg", emp.getImage());
                    picUsuario.Load("temp.jpg");
                }
                else
                {
                    picUsuario.Image = null;
                }

                Tarjeta t = mainApp.DataManager.buscarTarjetadeEmpleado(emp.Id);
                if (t != null)
                {
                    txtTarjeta.Text = t.IDTARJETA.ToString();
                }
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (actualKey < listaUsuarios.Count-1)
            {
                actualKey++;
            }
            verUsuario();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (actualKey > 0)
            {
                actualKey--;
            }

            verUsuario();
        }

        private void rdbSexoM_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rdbSexoF_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rdbAccessOK_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rdbAccessDny_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {

        }

        private void btnImportarLENEL_Click(object sender, EventArgs e)
        {
            if (!mainApp.formImportarLenel.IsDisposed)
            {
                mainApp.formImportarLenel.ShowDialog();
            }
            else
            {
                mainApp.formImportarLenel = new frmImportarLENEL(mainApp);
                mainApp.formImportarLenel.ShowDialog();
            }
            keyList = new List<int>(this.listaUsuarios.Keys);
        }

        
    }
}

//#define VERSIONCLIENTE
// Cambio Javier
//Cambio2
//Cambio3
//Cambio4
//Cambio5
//Cambio6




using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace ServerComunicacionesALUTEL
{
    public class Aplicacion
    {
       
        public frmInicial formInicial;
        public frmInicialCliente formInicialCliente;

        //public frmAccesos formAccesos;
        public frmUsuarios formUsuarios;
        public frmDevices formDevices;
        public frmConfigAcceso formConfigAcceso;

        public frmGrupos formGrupos;
        public frmCrearGrupoUsuarios formCrearGrupoUsuarios;
        public frmCrearGrupoDevices formCrearGrupoDevices;


        public dataManager DataManager;
        public communicationSystem ComunicationSystem;
        public frmConfiguracion formConfiguracion;
        public frmImportarLENEL formImportarLenel;
        public frmVirtualGate formVirtualGate;
        public AccessLevelLogic RecursosMemoria;
        
        public Thread alutrackServerThread;
        public Thread LENELServerThread;


        public void Init()
        {
            StaticTools.setearAplicacion(this);


        //    formInicial = null;
            formInicialCliente = new frmInicialCliente(this);


            formInicial = new frmInicial(this);
          //  formInicialCliente=null;

        }



        public bool Init2()
        {
            bool res = false;
            try
            {

                ComunicationSystem = new communicationSystem(this, formInicial,formInicialCliente);

                DataManager = new dataManager(this, ComunicationSystem.agregarItemLOG);
                formConfiguracion = new frmConfiguracion(this);
                DataManager.loadData();
                formUsuarios = new frmUsuarios(this);
                formDevices = new frmDevices(this);
                formConfigAcceso = new frmConfigAcceso(this);
                formGrupos = new frmGrupos(this);
                formCrearGrupoUsuarios = new frmCrearGrupoUsuarios(this);
                AccessLevelLogic.LoadAccessLevels(this);
                formCrearGrupoDevices = new frmCrearGrupoDevices(this);
            
                formImportarLenel = new frmImportarLENEL(this);
                res = true;
               
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error loading initial data: " + ex.Message + " Please reconfigure the Server and restart");
               
            }

            return res;

        }

        public void Start()
        {

            string ServerType = SystemConfiguration.serverType;  // TINY para la version chica(Cliente)
            if (ServerType.ToUpper() == "TINY")
            {
                Application.Run(formInicialCliente);
            }
            else
            {
                Application.Run(formInicial);
            }

        }

        public string InterpretarLatitud(string v_latitud)
        {
            int posGrados = v_latitud.IndexOf("°");
            int posMinutos = v_latitud.IndexOf("'");
            if (posGrados >= 0 && posMinutos >= 0)
            {
                string grados = v_latitud.Substring(0, posGrados);
                string minutos = v_latitud.Substring(posGrados + 1, posMinutos - posGrados - 1);
                string orientacion = v_latitud.Substring(v_latitud.Length - 1);
                string signo = "";
                if (orientacion == "S")
                    signo = "-";
                return signo + grados + "." + minutos;
            }
            else  // Segundo formato
            {
                int posdiv = v_latitud.IndexOf(":");
                if (posdiv >= 0)
                {
                    string orientacion = v_latitud.Substring(0, 1);
                    string posicion = v_latitud.Substring(2);
                    string signo = "";
                    if (orientacion == "S")
                        signo = "-";

                    return signo + posicion;
                }
                else
                    return v_latitud;
            }
        }


        public string InterpretarLongitud(string v_longitud)
        {
            int posGrados = v_longitud.IndexOf("°");
            int posMinutos = v_longitud.IndexOf("'");
            if (posGrados >= 0 && posMinutos >= 0)
            {
                string grados = v_longitud.Substring(0, posGrados);
                string minutos = v_longitud.Substring(posGrados + 1, posMinutos - posGrados - 1);
                string orientacion = v_longitud.Substring(v_longitud.Length - 1);
                string signo = "";
                if (orientacion == "W")
                    signo = "-";


                return signo + grados + "." + minutos;
            }
            else  // Segundo formato
            {
                int posdiv = v_longitud.IndexOf(":");
                if (posdiv >= 0)
                {
                    string orientacion = v_longitud.Substring(0, 1);
                    string posicion = v_longitud.Substring(2);
                    string signo = "";
                    if (orientacion == "W")
                        signo = "-";

                    return signo + posicion;
                }
                else
                    return v_longitud;
            }
        }




    }
}

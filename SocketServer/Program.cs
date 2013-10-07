using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4NetConfig.cnf", ConfigFileExtension = "cnf", Watch = true)]
// This will cause log4net to look for a configuration file


namespace ServerComunicacionesALUTEL
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// 
        /// 
        /// </summary>
        /// 

       // private  frmServer frmPrincipal;
       // private  dataManager DataManager;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //frmPrincipal = new frmServer();
            //DataManager = new dataManager();

            Aplicacion laAplicacion = new Aplicacion();

            laAplicacion.Init();

            //Application.Run(frmPrincipal);
            //Application.Run(new frmInicial(laAplicacion));
            //try
            //{
                laAplicacion.Start();
            //}
            //catch (Exception ex)
            //{

            //    string archivo = @"c:\Temporal\SocketServerLOG.txt";
            //    StreamWriter sw = !File.Exists(archivo) ? File.CreateText(archivo) : File.AppendText(archivo);
            //    sw.WriteLine(ex.Message);
            //    sw.Close();
            //}
        }
    }
}

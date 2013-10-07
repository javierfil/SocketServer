using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using System.Globalization;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;
namespace ServerComunicacionesALUTEL
{
    public class dataManager
    {


        private const int DEFAULT_TIPO_FEATURE = 3;          // Usada al dar de alta una Zona (Feature)
        private const string LENEL_LAYER = "LENEL";          // Usado al dar de alta una Zona (Layers)
        public static string NO_LOCATION_DATA = "NODATA";    // Usado para indicar que no hay ultima posicion disponible del HH
        public struct Punto
        {
            public float X;
            public float Y;

            public Punto(float v_x, float v_y)
            {
                X = v_x;
                Y = v_y;
            }
        }

        public static string conexion;

        ///  Constantes de tipos de acceso
        const string ENTRADA = "Entrada";
        const string SALIDA = "Salida";
        const string SINVALIDO = "SINVALIDO";
        const string EINVALIDO = "EINVALIDO";
        const string FC_INVALIDO = "FC-INVALIDO";
        const string ACCESS_GRANTED = "1";
        const string ACCESS_DENIED = "0";

        private Dictionary<string, List<int>> gruposUsuarios;
        private Dictionary<string, List<string>> gruposDevices;

        private Dictionary<string, Tarjeta> listaTarjetas;
        private Dictionary<int, Employee> listaEmpleados;

        private Dictionary<string, KeyValuePair<string, string>> reglasAcceso;

        public  Dictionary<string, List<EventoGPS>> listaEventosGPS = new Dictionary<string, List<EventoGPS>>();         // La clave de cada Lista es el HHID

        //private Dictionary<string, Zone> listaZonas;                        // Lista de Zonas definidas.

        private Dictionary<string, string> camposLenelCamposSistema;

        //private static string XMLDataPath;          // Lugar desde donde se leen y cargan los XMLs.       
        //private static string imagePath;            // Lugar desde donde se leen y cargan las imagenes de empleados, accesos, etc.
      
        private Aplicacion mainApp;

        private StringEventHandler m_LOGHandler;

        private Dictionary<string, string> ultimasPosGPS;

        public LAYERLenel layerLENEL;

        public LAYERAlutrack layerALUTRACK;

        /// <summary>
        /// Inicializo las estructuras de datos que se llenarán en RAM con el loadData
        /// Y las variables de configuracion usando la clase SystemConfiguration
        /// </summary>
        public dataManager(Aplicacion v_app, StringEventHandler v_logHandler)
        {
            conexion = "Data Source=" + SystemConfiguration.DataSource + "; Initial Catalog=" + SystemConfiguration.DataBaseName + "; User ID=" + SystemConfiguration.DBUserName+";password="+SystemConfiguration.DBPassword;

            mainApp = v_app;
            m_LOGHandler = v_logHandler;
            //XMLDataPath = v_XMLPath;
            //imagePath = v_imagePath;

            reglasAcceso = new Dictionary<string, KeyValuePair<string, string>>();

            camposLenelCamposSistema = new Dictionary<string, string>();

            listaTarjetas = null;
            listaEmpleados = null;

            ultimasPosGPS = new Dictionary<string, string>();

        }

        /// <summary>
        /// Cargo desde el dataSource todas las estructuras de datos en RAM
        /// </summary>
        /// <param name="dataSource"></param>
        public void loadData()
        {

                //MessageBox.Show("Entra a loadData()");

                gruposUsuarios = LoadGruposUsuarios();

                //MessageBox.Show("1");
                gruposDevices = LoadGruposDevices();

                //MessageBox.Show("2");
                LoadReglasAcceso();

                //MessageBox.Show("3");
                LoadVisitas();

                //MessageBox.Show("4");
                LoadEventosGPS();

                //MessageBox.Show("5");
                LoadZonas();

                //MessageBox.Show("6");
                LoadAccesosAZonas();

                //MessageBox.Show("7");
                LoadTarjetas();
                //MessageBox.Show("Sale de loadData()");

           
        }

        /// <summary>
        /// Guarda el string de acceso para mandar, en formato: 
        /// res = "LATITUDE:" + latitud + ",LONGITUDE:" + longitud + ",DATETIME:" + dateTime + ",HHID:"+HHID;
        /// </summary>
        /// <param name="v_lat"></param>
        /// <param name="v_long"></param>
        /// <param name="v_fechaHora"></param>
        public void actualizarUltimaPosGPS(string v_lat, string v_long, string v_fechaHora, string HHID)
        {
            string GPSPosString = "LATITUDE:" + v_lat + ",LONGITUDE:" + v_long + ",DATETIME:" + v_fechaHora + ",HHID:" + HHID;
            
           
            try
            {
                StaticTools.obtenerMutex_UltimasPosGPS();

                if (ultimasPosGPS.ContainsKey(HHID))
                {
                    ultimasPosGPS.Remove(HHID);
                }

                ultimasPosGPS.Add(HHID, GPSPosString);
            }
            catch (Exception ex)
            {
                loguearString(HHID + "Excepcion en actualizarUltimaPosGPS: " + ex.Message,TiposLOG.HH);
            }
            finally
            {
                StaticTools.liberarMutex_UltimasPosGPS();
            }


        }

        public string obtenerUltimaPosGPS(string HHID)
        {
            string res = string.Empty;
           
            try
            {
                StaticTools.obtenerMutex_UltimasPosGPS();
                if (ultimasPosGPS.ContainsKey(HHID))
                {
                    res= ultimasPosGPS[HHID];
                }
            }
            catch (Exception )
            {
                loguearString(HHID + "- Excepcion en obtenerUltimaPosGPS",TiposLOG.HH);

            }
            finally
            {
                StaticTools.liberarMutex_UltimasPosGPS();
            }

            return res;

        }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, List<EventoGPS>> LoadEventosGPS()
        {
            //Aca cargo todos los eventos gps desde base de datos Sql Server.

            int id = 0;
            string hhid = "";
            string latitud = "";
            string longitud = "";
            string hora = "";
            
            SqlConnection cnn = new SqlConnection(conexion);

            try
            {
                StaticTools.obtenerMutex_ListaEventosGPS();

                SqlCommand cmd = new SqlCommand("ListarGpsEventos", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cnn.Open();
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    id = Convert.ToInt32(lector["id"].ToString());
                    hhid = lector["hhid"].ToString();
                    latitud = lector["latitud"].ToString();
                    longitud = lector["longitud"].ToString();
                    hora = lector["hora"].ToString();
                    if (!listaEventosGPS.ContainsKey(hhid))
                    {
                        List<EventoGPS> listaEvento = new List<EventoGPS>();
                        listaEventosGPS.Add(hhid, listaEvento);
                    }

                    EventoGPS nuevoEvento = new EventoGPS(hhid, latitud, longitud, hora);

                    listaEventosGPS[hhid].Add(nuevoEvento);
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en LoadEventosGPS() - " + ex.Message,TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
                StaticTools.liberarMutex_ListaEventosGPS();
            }



            return listaEventosGPS;
        }


        public void addDevice(device v_device)
        {

            SqlConnection cnn = new SqlConnection(conexion);
            SqlCommand cmd = new SqlCommand("AgregarDispositivo", cnn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@idHandHeld", v_device.IDENTIFICADOR);
            cmd.Parameters.AddWithValue("@marca", v_device.MARCA);
            cmd.Parameters.AddWithValue("@modelo", v_device.MODELO);
            cnn.Open();
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar el producto,consulte con el administrador. Error: " + ex.Message);
            }
            finally
            {
                cnn.Close();
            }
        }


        public List<string> ListarNombreReglas()
        {
            string nombreregla = "";
            List<string> listado = new List<string>();
            SqlConnection cnn = new SqlConnection(conexion);
            SqlCommand cmd = new SqlCommand("ListarNombresReglas", cnn);
            cmd.CommandType = CommandType.StoredProcedure;
            cnn.Open();
            SqlDataReader lector;
            try
            {
                lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    nombreregla = lector["NombreRegla"].ToString();
                    listado.Add(nombreregla);
                }
            }
            catch
            {
            }
            finally
            {
                cnn.Close();
            }
            return listado;
        }

        public Dictionary<string, Visita> LoadVisitas()
        {

            //Carga las visitas a partir de Base de Datos Sql Server

            Dictionary<string, Visita> listaVisitas = new Dictionary<string, Visita>();
            byte[] imageBytes = null;
            string id = "";
            string hhid = "";
            string tarjeta = "";
            string latitud = "";
            string longitud = "";
            string hora = "";
            string documento = "";
            string apellido = "";
            string nombre = "";
            string image = "";
            string empresa = "";
            string TipoAcceso = "";
            int orgID = -1;
            TiposAcceso tipoAcceso = new TiposAcceso();
            List<Visita> lista = new List<Visita>();

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "select * from Visita";
                cmd.CommandType = CommandType.Text;
               
                SqlDataReader lector = cmd.ExecuteReader();
           
                while (lector.Read())
                {
                    id = lector["Id"].ToString();
                    hhid = lector["IdDevice"].ToString();
                    nombre = lector["Nombre"].ToString();
                    apellido = lector["Apellido"].ToString();
                    empresa = lector["Empresa"].ToString();
                    tarjeta = lector["Tarjeta"].ToString();
                    latitud = lector["Latitud"].ToString();
                    longitud = lector["Longitud"].ToString();
                    hora = lector["Fecha"].ToString();
                    TipoAcceso = lector["Tipoacceso"].ToString();
                    documento = lector["SSNO"].ToString();
                    orgID = int.Parse(lector["IdOrganizacion"].ToString());

                    if (lector["Imagen"].ToString() == "")
                    {
                        image = "";
                        imageBytes = null;
                    }
                    else
                    {
                        image = lector["Imagen"].ToString();
                    }

                    if (image != "")
                    {
                        try
                        {

                            string fileName = Path.GetFileName(image);

                            string imagePath = SystemConfiguration.ImagesPath;

                            imageBytes = File.ReadAllBytes(imagePath + @"/" + fileName);
                        }
                        catch (Exception )
                        {
                            image = "";
                            imageBytes = null;

                        }
                    }
                    tipoAcceso = (TiposAcceso)Enum.Parse(typeof(TiposAcceso), TipoAcceso);

                    Visita nuevaVisita = new Visita(hhid, nombre, apellido, documento, empresa, tarjeta, latitud, longitud, hora, imageBytes, tipoAcceso, orgID);

                    listaVisitas.Add(id, nuevaVisita);

                }
            }
            catch (Exception )
            {

            }
            finally
            {
                cnn.Close();
            }
            return listaVisitas;
        }

        public Dictionary<int, Acceso> LoadAccesos()
        {
            Dictionary<int, Acceso> listaAccesos = new Dictionary<int, Acceso>();
                byte[] imageBytes = null;
                int id;
                string hhid;
                string tarjeta;
                string latitud;
                string longitud;
                string hora;
                string TipoAccesoStr;
                string imagen;
                int idempleado;
                TiposAcceso tipoAcceso = new TiposAcceso();
                List<Acceso> lista = new List<Acceso>();
                SqlConnection cn = new SqlConnection(conexion);
                cn.Open();
                SqlCommand cmd = cn.CreateCommand();
                cmd.CommandText = "select * from Accesos";
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();
                
               while (lector.Read())
                {
                    imagen = lector["imagen"].ToString();
                    id = Convert.ToInt16(lector["idAcceso"]);
                    hhid = lector["IdHandHeld"].ToString();
                    tarjeta = lector["Tarjeta"].ToString();
                    latitud = lector["Latitud"].ToString();
                    longitud = lector["Longitud"].ToString();
                    try
                    {
                        hora = lector["Fecha"].ToString();
                    }
                    catch (Exception )
                    {
                        hora = "01-01-20000 01:01:00";
                    }
                   
                    TipoAccesoStr = lector["Tipoacceso"].ToString();
                    tipoAcceso = (TiposAcceso)Enum.Parse(typeof(TiposAcceso), TipoAccesoStr);
                    idempleado = Convert.ToInt32(lector["idEmpleado"].ToString());

                    if (imagen != "")
                    {
                        try
                        {
                            string fileName = Path.GetFileName(imagen);
                            string imagePath = SystemConfiguration.ImagesPath;
                            imageBytes = File.ReadAllBytes(imagePath + @"/" + fileName);
                        }
                        catch (Exception )
                        {
                            imageBytes = null;
                        }
                    }
                    else
                    {
                        imageBytes = null;
                    }
                    
                    Acceso nuevoAcceso = new Acceso(idempleado, hhid, tarjeta, latitud, longitud, hora, imageBytes, tipoAcceso);
                    listaAccesos.Add(id, nuevoAcceso);

                }
                cn.Close();
                return listaAccesos;
            }

        private List<ZoneAccess> LoadAccesosAZonas()
        {
            List<ZoneAccess> listaAccesosZonas = new List<ZoneAccess>();

            //string idhandheld;
            //string idZona;
            //string idGate;
            //string hora;
            //string tipo;
            
            //List<Acceso> lista = new List<Acceso>();
            //SqlConnection cn = new SqlConnection(conexion);
            //SqlCommand cmd = new SqlCommand("ListarAccesoaZonas", cn);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cn.Open();
            //SqlDataReader lector = cmd.ExecuteReader();
            //try
            //{
            //    while (lector.Read())
            //    {
            //        idhandheld = lector["idHandHeld"].ToString();
            //        idZona = lector["idZona"].ToString();
            //        idGate = lector["IdGate"].ToString();
            //        hora = lector["Hora"].ToString();
            //        tipo = lector["Tipo"].ToString();
            //        ZoneAccess nuevoAcceso = new ZoneAccess(idhandheld, idZona, idGate, hora, (TiposAcceso)Enum.Parse(typeof(TiposAcceso), tipo));
            //        listaAccesosZonas.Add(nuevoAcceso);
            //    }
            //}
            //catch (Exception )
            //{

            //}
            //finally
            //{
            //    cn.Close();
            //}

            return listaAccesosZonas;
        }

        private  Dictionary<string, device> LoadDevices()
        {
            Dictionary<string, device> listaDevices = new Dictionary<string, device>();
            string idDevice = "";
            string marca = "";
            string modelo = "";
            int identificador = 0;

            SqlConnection cnn = new SqlConnection(conexion);
            SqlCommand cmd = new SqlCommand("ListarDispositivos", cnn);
            cmd.CommandType = CommandType.StoredProcedure;
            cnn.Open();
            SqlDataReader lector = cmd.ExecuteReader();
            try
            {

                while (lector.Read())
                {
                    idDevice = lector["hhid"].ToString();
                    marca = lector["idMarca"].ToString();
                    modelo = lector["idModelo"].ToString();
                    identificador = Convert.ToInt32(lector["id"].ToString());
                    device d = new device(idDevice, marca, modelo, identificador);

                    listaDevices.Add(idDevice, d);

                }
            }
            catch (Exception)
            {
               
            }
            finally
            {
                cnn.Close();
            }

            return listaDevices;
        }

        public void LoadReglasAcceso()
        {
            //if (File.Exists(XMLDataPath + @"\\reglasAcceso.xml"))
            //{
            //    XmlDocument xDoc = new XmlDocument();
            //    xDoc.Load(XMLDataPath + @"\\reglasAcceso.xml");

            //    reglasAcceso.Clear();
            //    foreach (XmlElement elem in xDoc.SelectNodes(@"/ReglasAcceso/ReglaAcceso"))
            //    {
            //        string idRegla = elem.Attributes["id"].Value;
            //        string grupoUsuario = elem.Attributes["grupoUsuario"].Value;
            //        string grupoDevice = elem.Attributes["grupoDevice"].Value;
            //        KeyValuePair<string, string> asociacion = new KeyValuePair<string, string>(grupoUsuario, grupoDevice);
            //        if (!reglasAcceso.ContainsKey(idRegla))
            //        {
            //            reglasAcceso.Add(idRegla, asociacion);
            //        }
            //    }
            //}
        }

        private Dictionary<string, List<string>> LoadGruposDevices()
        {
            string IdDevice = "";

            string grupoDevice = "";
            Dictionary<string, List<string>> lstGruposDevices = new Dictionary<string, List<string>>();
            
            SqlConnection cnn = new SqlConnection(conexion);
            
            try
            {
               
                SqlCommand cmd = new SqlCommand("ListarGrupoDevices", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cnn.Open();
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    IdDevice = lector["idDevice"].ToString();
                    grupoDevice = lector["NombreGrupoDevice"].ToString();

                    if (lstGruposDevices.ContainsKey(grupoDevice))
                    {
                        List<string> listadevices = lstGruposDevices[grupoDevice];

                        listadevices.Add(IdDevice);
                    }
                    else
                    {
                        List<string> nuevoGrupo = new List<string>();
                        nuevoGrupo.Add(IdDevice);
                        lstGruposDevices.Add(grupoDevice, nuevoGrupo);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                cnn.Close();
            }

            return lstGruposDevices;
        }

        public Dictionary<string, List<int>> LoadGruposUsuarios()
        {
            Dictionary<string, List<int>> grp_Usuarios = new Dictionary<string,List<int>>();

            int idEmpleado;
            string idGrupo;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
               
                SqlCommand cmd = new SqlCommand("ListarGrupoEmpleados", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cnn.Open();
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    idEmpleado = Convert.ToInt32(lector["idEmpleado"]);
                    idGrupo = lector["NombreGrupo"].ToString();

                    if (grp_Usuarios.ContainsKey(idGrupo))
                    {
                        List<int> listaUsuarios = grp_Usuarios[idGrupo];

                        listaUsuarios.Add(idEmpleado);
                    }
                    else
                    {
                        List<int> nuevoGrupo = new List<int>();
                        nuevoGrupo.Add(idEmpleado);
                        grp_Usuarios.Add(idGrupo, nuevoGrupo);
                    }

                }
            }
            catch (Exception)
            {
            }
            finally
            {
                cnn.Close();
            }

            return grp_Usuarios;
        }
        private Dictionary<int, Employee> LoadEmpleados()
        {
            if (listaEmpleados != null)
            {
                return listaEmpleados;
            }
            else
            {
                listaEmpleados = new Dictionary<int, Employee>();

                int idEmpleado;
                string Nombre;
                string Apellido;
                bool Sexo = true;
                string eMail;
                string Direccion = "";
                //string Nacionalidad;
                //string Ciudad;
                DateTime FechaNacimiento;
                string Telefono;
                string Celular;
                string TipoDocumento;
                string NumeroDocumento;
                DateTime FechaExpedicionDocumento;
                DateTime FechaVencimientoDocumento;
                string Empresa;
                string Imagen;          // El path + nombre del archivo
                int IdImagen;
                byte[] imageBytes = null;
                string ultimaActualizacion;

                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    //SqlCommand cmd = new SqlCommand("ListarEmpleados", cnn);
                    //cmd.CommandType = CommandType.StoredProcedure;

                    //cnn.Open();

                    //SqlDataReader lector = cmd.ExecuteReader();
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = "select * from Empleados";
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader lector = cmd.ExecuteReader();

                    int cant = 0;
                    Regex rHora = new Regex(@"(.*)-(.*)-(.*) (.*):(.*):(.*)");
                    while (lector.Read())
                    {
                        idEmpleado = Convert.ToInt32(lector["idEmpleado"]);
                        Nombre = lector["Nombre"].ToString();
                        Apellido = lector["Apellido"].ToString();

                        if (lector["Sexo"] != DBNull.Value)
                        {
                            Sexo = Convert.ToBoolean(lector["Sexo"]);
                        }
                        eMail = lector["eMail"].ToString();

                        if (lector["Direccion"] != DBNull.Value)
                        {
                            Direccion = lector["Direccion"].ToString();
                        }
                        FechaNacimiento = new DateTime(2013, 01, 01);

                        if (lector["FechaNacimiento"] != null)
                        {
                            if (lector["FechaNacimiento"].ToString().Length > 6)
                            {
                                FechaNacimiento = Convert.ToDateTime(lector["FechaNacimiento"].ToString());
                            }
                        }

                        Telefono = lector["Telefono"].ToString();
                        Celular = lector["Celular"].ToString();
                        TipoDocumento = lector["TipoDocumento"].ToString();

                        NumeroDocumento = lector["NumeroDocumento"].ToString();

                        FechaExpedicionDocumento = new DateTime(2013, 1, 1);
                        if (lector["FechaExpedicionDocumento"] != null)
                        {
                            if (lector["FechaExpedicionDocumento"].ToString().Length > 6)
                            {
                                FechaExpedicionDocumento = Convert.ToDateTime(lector["FechaExpedicionDocumento"]);
                            }

                        }
                        FechaVencimientoDocumento = new DateTime(2013, 12, 12);
                        if (lector["FechaVencimientoDocumento"] != null)
                        {
                            if (lector["FechaVencimientoDocumento"].ToString().Length > 6)
                            {
                                FechaVencimientoDocumento = Convert.ToDateTime(lector["FechaVencimientoDocumento"]);
                            }
                        }

                        Empresa = lector["Empresa"].ToString();

                        Imagen = lector["Imagen"].ToString();
                        IdImagen = Convert.ToInt32(lector["IdImagen"]);

                        ultimaActualizacion = lector["ultimaActualizacion"].ToString();

                        Employee emp = new Employee();


                        
                        Match mHora = rHora.Match(ultimaActualizacion);

                        if (mHora.Success)
                        {
                            DateTime d = new DateTime(Convert.ToInt32(mHora.Groups[1].Value), Convert.ToInt32(mHora.Groups[2].Value), Convert.ToInt32(mHora.Groups[3].Value), Convert.ToInt32(mHora.Groups[4].Value), Convert.ToInt32(mHora.Groups[5].Value), Convert.ToInt32(mHora.Groups[6].Value));
                            emp.ultimaActualizacion = d;
                        }
                        else { loguearString("No paso la expresion regular de la hora. LoadEmpleados", TiposLOG.HH); }
                    

                        bool valido = true;
                        int version = -1;
                        try { version = Convert.ToInt32(lector["Version"].ToString()); }
                        catch { valido = false; }

                        int personID = -1;
                        try { personID = Convert.ToInt32(lector["PersonID"].ToString()); }
                        catch { valido = false; }

                        emp.VersionEmpleado = version;
                        emp.PersonID = personID;

                        emp.Id = idEmpleado;
                        emp.Nombre = Nombre;
                        emp.Apellido = Apellido;
                        emp.Sexo = Sexo;
                        emp.EMail = eMail;
                        emp.Direccion = Direccion;
                        // emp.Nacionalidad = Nacionalidad;
                        // emp.Ciudad = Ciudad;
                        emp.FechaNacimiento = FechaNacimiento;
                        emp.Telefono = Telefono;
                        emp.Celular = Celular;
                        emp.TipoDocumento = TipoDocumento;
                        emp.NumeroDocumento = NumeroDocumento;
                        emp.FechaExpedicionDocumento = FechaExpedicionDocumento;
                        emp.FechaVencimientoDocumento = FechaVencimientoDocumento;

                        //DEBUG - ACA POR AHORA ES ""
                        emp.Empresa = Empresa;
                        emp.Empresa = "";

                        emp.Imagen = Imagen;
                        emp.imageVersion = IdImagen;

                        if (Imagen != "")
                        {
                            try
                            {
                                string fileName = Path.GetFileName(Imagen);
                                if (fileName != "")
                                {
                                    string imagePath = SystemConfiguration.ImagesPath;
                                    imageBytes = File.ReadAllBytes(imagePath + @"/" + fileName);
                                    cant++;
                                }
                                else
                                {
                                    imageBytes = null;
                                    emp.Imagen = "";
                                }

                            }
                            catch (Exception)
                            {
                                imageBytes = null;
                                emp.Imagen = "";
                            }
                        }
                        else
                        {
                            imageBytes = null;
                            emp.Imagen = "";
                        }

                        emp.attachImage(imageBytes);

                        if (valido) listaEmpleados.Add(idEmpleado, emp);

                    }

                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en LoadEmpleados: " + ex.Message, TiposLOG.HH);

                }
                finally
                {
                    cnn.Close();
                }
            }

            return listaEmpleados;
        }

        private Dictionary<string,Tarjeta> LoadTarjetas()
        {
            if (listaTarjetas != null)
            {
                return listaTarjetas;
            }
            else
            {
                listaTarjetas = new Dictionary<string, Tarjeta>();
                int idTarjeta = 0;
                int idOrg = 0;
                string Tarjeta = "";
                bool Estado = false;
                int idEmpleado = 0;
                string accessLevels = "";
                string strUltimaAct = "";
                DateTime ultimaActualizacion = DateTime.Now;

                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    SqlCommand cmd = new SqlCommand("ListarTarjetas", cnn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cnn.Open();
                    SqlDataReader lector = cmd.ExecuteReader();

                    Regex rHora = new Regex(@"(.*)-(.*)-(.*) (.*):(.*):(.*)");
                    while (lector.Read())
                    {
                        
                        idTarjeta = (int)lector["idTarjeta"];
                        Tarjeta = lector["Tarjeta"].ToString();

                        Estado = Convert.ToBoolean(lector["Estado"]);
                        accessLevels = lector["accessLevels"].ToString();
                        idOrg = (int)lector["idOrganizacion"];
                        strUltimaAct = lector["ultimaActualizacion"].ToString();

                        if (lector["idEmpleado"] != DBNull.Value)
                        {
                            idEmpleado = Convert.ToInt32(lector["idEmpleado"]);
                        }
                        else
                        {
                            idEmpleado = Employee.NODEFINIDO;
                        }

                        Match mHora = rHora.Match(strUltimaAct);

                        if (mHora.Success)
                        {
                            ultimaActualizacion = new DateTime(Convert.ToInt32(mHora.Groups[1].Value), Convert.ToInt32(mHora.Groups[2].Value), Convert.ToInt32(mHora.Groups[3].Value), Convert.ToInt32(mHora.Groups[4].Value), Convert.ToInt32(mHora.Groups[5].Value), Convert.ToInt32(mHora.Groups[6].Value));

                        }
                        else
                        {
                            loguearString("No reconocio la expresion regular de la hora: LoadTarjetas(). Tarjeta: " + Tarjeta, TiposLOG.HH);
                        }

                        Tarjeta t = new Tarjeta(idOrg,idTarjeta, Tarjeta, idEmpleado, Estado, accessLevels, ultimaActualizacion);

                        if (!listaTarjetas.ContainsKey(Tarjeta))
                        {
                            listaTarjetas.Add(Tarjeta, t);
                        }
                    }
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en LoadTarjetas: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
            return listaTarjetas;
        }

   
        public Dictionary<string, string> getCamposLenelCamposSistema()
        {
            return camposLenelCamposSistema;
        }

        public Dictionary<string, KeyValuePair<string, string>> getReglasAcceso()
        {
            return reglasAcceso;
        }

        public Dictionary<string, List<int>> getGruposUsuarios()
        {
            return gruposUsuarios;
        }

        public Dictionary<string, List<string>> getGruposDevices()
        {
            return LoadGruposDevices();
        }

        public Dictionary<int, Employee> getListaEmpleados()
        {
            return LoadEmpleados();
        }

        public Dictionary<string, device> getListaDevices()
        {
            return LoadDevices();
        }

        public Dictionary<int, Acceso> getListaAccesos()
        {
            return LoadAccesos();
        }

        public Dictionary<string, Tarjeta> getListaTarjetas()
        {
            return LoadTarjetas();
            //return listaTarjetas;
        }

        public Dictionary<string, Visita> getListaVisitas()
        {
            return mainApp.DataManager.LoadVisitas();
        }

        public Dictionary<string, List<EventoGPS>> getEventosGPS()
        {
            return listaEventosGPS;
        }

        public Dictionary<string, Zone> getListaZonas()
        {
            return mainApp.DataManager.LoadZonas();
        }
        
      
        public void addReglaAcceso(string id_regla, KeyValuePair<string, string> regla)
        {
            if (reglasAcceso.ContainsKey(id_regla))
            {
                reglasAcceso.Remove(id_regla);
                reglasAcceso.Add(id_regla, regla);
            }
            else
            {
                reglasAcceso.Add(id_regla, regla);
            }
            SaveReglasAccesos();
        }

        public void addGrupoUsuarios(string id_grupo, List<int> lstUsuarios)
        {
            if (gruposUsuarios.ContainsKey(id_grupo))
            {
                gruposUsuarios.Remove(id_grupo);
                gruposUsuarios.Add(id_grupo, lstUsuarios);
            }
            else
            {
                gruposUsuarios.Add(id_grupo, lstUsuarios);
            }

            SaveGruposUsuarios();
        }

        public void addGrupoDevices(string id_grupo, List<string> lstdevices)
        {
            if (gruposDevices.ContainsKey(id_grupo))
            {
                gruposDevices.Remove(id_grupo);
                gruposDevices.Add(id_grupo, lstdevices);
            }
            else
            {
                gruposDevices.Add(id_grupo, lstdevices);
            }

            SaveGruposDevices();
        }

        public void SaveReglasAccesos()
        {
            try
            {
                XmlDocument xdoc = new XmlDocument();
                XmlElement elementRoot = xdoc.CreateElement("ReglasAcceso");
                xdoc.AppendChild(elementRoot);
                foreach (KeyValuePair<string, KeyValuePair<string, string>> pair in reglasAcceso)
                {
                    XmlElement reglaAcceso = xdoc.CreateElement("ReglaAcceso");
                    XmlAttribute att_idRegla = xdoc.CreateAttribute("id");
                    att_idRegla.Value = pair.Key;
                    reglaAcceso.Attributes.Append(att_idRegla);

                    XmlAttribute att_grupoUsuario = xdoc.CreateAttribute("grupoUsuario");
                    att_grupoUsuario.Value = pair.Value.Key;
                    reglaAcceso.Attributes.Append(att_grupoUsuario);

                    XmlAttribute att_grupoDevice = xdoc.CreateAttribute("grupoDevice");
                    att_grupoDevice.Value = pair.Value.Value;
                    reglaAcceso.Attributes.Append(att_grupoDevice);
                    elementRoot.AppendChild(reglaAcceso);
                }


                string XMLDataPath = SystemConfiguration.applicationPath;

                xdoc.Save(XMLDataPath + @"\\reglasAcceso.xml");
            }
            catch (Exception)
            {
                mainApp.ComunicationSystem.LOGInformation.Enqueue(new stringEventArgs("Excepcion al grabar REGLASACCESO. Continuando",null));
            }
        }

        public void SaveZonas(Dictionary <string, Zone> listaZonas)
        {


            //if (USERDB == true)
            //{
            //    foreach (KeyValuePair<string, Zone> valuePair in listaZonas)
            //    {
            //        foreach (KeyValuePair<string, Zone.GateDefinition> gate in valuePair.Value.listaPuertas)
            //        {

            //            //preguntar no se como agregar.
            //        }
            //    }
            //}
            //else
            //{
            try
            {
                XmlDocument xdoc = new XmlDocument();
                XmlElement elementRoot = xdoc.CreateElement("Zonas");
                xdoc.AppendChild(elementRoot);

                foreach (KeyValuePair<string, Zone> valuePair in listaZonas)
                {
                    XmlElement Zona = xdoc.CreateElement("Zona");
                    XmlAttribute att_idZona = xdoc.CreateAttribute("id");
                    att_idZona.Value = valuePair.Value.IDZona;
                    Zona.Attributes.Append(att_idZona);

                    foreach (KeyValuePair<string, Zone.GateDefinition> gate in valuePair.Value.listaPuertas)
                    {
                        XmlElement puerta = xdoc.CreateElement("Puerta");
                        XmlAttribute att_id = xdoc.CreateAttribute("id");
                        att_id.Value = gate.Key;
                        puerta.Attributes.Append(att_id);

                        XmlAttribute att_type = xdoc.CreateAttribute("type");
                        att_type.Value = gate.Value.type.ToString();
                        puerta.Attributes.Append(att_type);

                        XmlAttribute att_LatFrom = xdoc.CreateAttribute("latfrom");
                        att_LatFrom.Value = gate.Value.from.position.latitude.ToString();
                        puerta.Attributes.Append(att_LatFrom);

                        XmlAttribute att_LngFrom = xdoc.CreateAttribute("lngfrom");
                        att_LngFrom.Value = gate.Value.from.position.longitude.ToString();
                        puerta.Attributes.Append(att_LngFrom);

                        XmlAttribute att_LatTo = xdoc.CreateAttribute("latto");
                        att_LatTo.Value = gate.Value.to.position.latitude.ToString();
                        puerta.Attributes.Append(att_LatTo);

                        XmlAttribute att_LngTo = xdoc.CreateAttribute("lngto");
                        att_LngTo.Value = gate.Value.to.position.longitude.ToString();
                        puerta.Attributes.Append(att_LngTo);

                        Zona.AppendChild(puerta);

                    }
                    elementRoot.AppendChild(Zona);
                }
                string XMLDataPath = SystemConfiguration.applicationPath;

                xdoc.Save(XMLDataPath + @"\\Zonas.xml");
            }
            catch (Exception)
            {
                mainApp.ComunicationSystem.LOGInformation.Enqueue(new stringEventArgs("Excepcion al grabar ZONAS. Continuando",null));
            }
            //}
        }

        public Dictionary<string, Zone> LoadZonas()
        {

            Dictionary<string, Zone> listaZonas = new Dictionary<string,Zone>();                        // Lista de Zonas definidas. 

            //if (File.Exists(XMLDataPath + @"\\Zonas.xml"))
            //{
            //    XmlDocument xDoc = new XmlDocument();
            //    xDoc.Load(XMLDataPath + @"\\Zonas.xml");
            //    listaZonas.Clear();
            //    foreach (XmlElement zona in xDoc.SelectNodes(@"/Zonas/Zona"))
            //    {
            //        string idZona = zona.Attributes["id"].Value;
            //        Zone nuevaZona = new Zone(idZona);
            //        nuevaZona.listaPuertas = new Dictionary<string, Zone.GateDefinition>();

            //        int cuentaPuntos = 0;
            //        foreach (XmlElement puerta in zona.SelectNodes("Puerta"))
            //        {
            //            string idPuerta = puerta.Attributes["id"].Value;
            //            string typePuerta = puerta.Attributes["type"].Value;
            //            string latFrom = puerta.Attributes["latfrom"].Value;
            //            string lngFrom = puerta.Attributes["lngfrom"].Value;
            //            string latTo = puerta.Attributes["latto"].Value;
            //            string lngTo = puerta.Attributes["lngto"].Value;

            //            Zone.GateDefinition nuevaPuerta = new Zone.GateDefinition();
            //            nuevaPuerta.ID = idPuerta;
            //            nuevaPuerta.type = (GateAccessType)Enum.Parse(typeof(GateAccessType), typePuerta);

            //            nuevaPuerta.from = new Zone.ZonePoint("P" + cuentaPuntos.ToString(), latFrom, lngFrom);
            //            nuevaPuerta.to = new Zone.ZonePoint("P" + cuentaPuntos.ToString(), latTo, lngTo);

            //            nuevaZona.listaPuertas.Add(idPuerta, nuevaPuerta);
            //        }
            //        listaZonas.Add(idZona, nuevaZona);
            //    }
            //}

            return listaZonas;
        }


        public void SaveGruposDevices()
        {
            try
            {

                XmlDocument xdoc = new XmlDocument();
                XmlElement elementRoot = xdoc.CreateElement("GruposDevices");
                xdoc.AppendChild(elementRoot);

                foreach (KeyValuePair<string, List<string>> keyValue in gruposDevices)
                {
                    XmlElement grupoDevice = xdoc.CreateElement("GrupoDevice");
                    XmlAttribute att_idGrupo = xdoc.CreateAttribute("id");
                    att_idGrupo.Value = keyValue.Key;
                    grupoDevice.Attributes.Append(att_idGrupo);

                    foreach (string s in keyValue.Value)
                    {
                        XmlElement device = xdoc.CreateElement("Device");
                        XmlAttribute att_id = xdoc.CreateAttribute("id");
                        att_id.Value = s;
                        device.Attributes.Append(att_id);
                        grupoDevice.AppendChild(device);
                    }
                    elementRoot.AppendChild(grupoDevice);
                }
                string XMLDataPath = SystemConfiguration.applicationPath;

                xdoc.Save(XMLDataPath + @"\\gruposDevices.xml");
            }
            catch (Exception)
            {
                mainApp.ComunicationSystem.LOGInformation.Enqueue(new stringEventArgs("Excepcion al grabar GRUPOSDEVICES. Continuando",null));
            }
        }

        public void SaveGruposUsuarios()
        {
            //try
            //{
            //    XmlDocument xdoc = new XmlDocument();
            //    XmlElement elementRoot = xdoc.CreateElement("GruposUsuarios");
            //    xdoc.AppendChild(elementRoot);

            //    foreach (KeyValuePair<string, List<string>> keyValue in gruposUsuarios)
            //    {
            //        XmlElement grupoUsuario = xdoc.CreateElement("GrupoUsuario");
            //        XmlAttribute att_idGrupo = xdoc.CreateAttribute("id");
            //        att_idGrupo.Value = keyValue.Key;
            //        grupoUsuario.Attributes.Append(att_idGrupo);

            //        foreach (string s in keyValue.Value)
            //        {
            //            XmlElement usuario = xdoc.CreateElement("Usuario");
            //            XmlAttribute att_id = xdoc.CreateAttribute("id");
            //            att_id.Value = s;
            //            usuario.Attributes.Append(att_id);
            //            grupoUsuario.AppendChild(usuario);
            //        }
            //        elementRoot.AppendChild(grupoUsuario);
            //    }
            //    xdoc.Save(dataSource + @"\\gruposUsuarios.xml");
            //}
            //catch (Exception)
            //{
            //    mainApp.ComunicationSystem.LOGInformation.Enqueue("Excepcion al grabar GruposUSUARIOS. Continuando");
            //}
        }

        /// <summary>
        /// Agrega o actualiza la informacion de un reader de LENEL
        /// </summary>
        /// <param name="v_deviceID"></param>
        /// <param name="v_deviceName"></param>
        /// <param name="v_readerID"></param>
        /// <param name="v_readerEntranceType"></param>
        /// <param name="v_organizationID"></param>
        public void agregarReaderDesdeLenel(string v_PanelID, string v_deviceName, string v_readerID, string v_readerName, string v_readerEntranceType, string v_organizationID)
        {

            bool yaExisteReader = false;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from LENELReaders where LNLPanelID = " + v_PanelID + " and LNLReaderID=" + v_readerID + " and OrgID=" + v_organizationID;
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();

                if (lector.HasRows)
                {
                    yaExisteReader = true;
                }

                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en agregarReaderDesdeLenel 1: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            // Obtiene el tipo de gate en funcion del tipo de entranceReader con el que se llama: 0: Granted, 1: Entrance, 2: Exit.
            GateAccessType tipoReader = obtenerTipoReader(v_readerEntranceType);

            if (yaExisteReader)            // Ya existe: hago Update de la zona por si cambio su nombre
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "update LENELReaders set Name = '" + v_readerName + "',Type = '" + tipoReader.ToString() + "' where LNLPanelID = " + v_PanelID + " and LNLReaderID=" + v_readerID + " and OrgID=" + v_organizationID;
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en agregarReaderDesdeLenel 2: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
            else
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "insert into LENELReaders(LNLPanelID,LNLReaderID,Type,Name,OrgID) values (" + v_PanelID + "," + v_readerID + ",'" + tipoReader.ToString() + "','" + v_readerName + "'," + v_organizationID + ")";
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en agregarReaderDesdeLenel 3: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
        }

        /// <summary>
        /// Arma el string que se enviara a LENEL con los datos del evento correspondiente a (LeneldeviceID,serialNum)
        /// Busca en la tabla de ACCESOS
        /// El formato de respuesta es:
        /// BADGE:xxxx,NAME:xxxx,SURNAME:xxx,SSNO:xxxx,COMPANY:xxxx,HHID:xxxx,ACCESSTYPE:Granted|Denied,DATETIME:xxxx,LATITUDE:xxxx,LONGITUDE:xxxxx,IMAGENEMPLEADO:xxxx,IMAGENACCESO:xxxx";
        /// FALSE si no hay registro de acceso en la base con ese serialNum
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="serialNum"></param>
        /// <returns></returns>
        public string obtenerDatosEventoDesdeAccesos(string LenelPanelID, string serialNum)
        {
            string res="";
            string Badge = "";
            string Name = "";
            string Surname = "";
            string SSNO = "";
            string company = "";
            string AccessType = "";
            string datetime = "";
            string Latitude = "";
            string Longitude = "";
            string imagenEmpleado = "";
            string imagenAcceso = "";
            string readerName = "";

            // Primero obtengo el HHID asociado al LenelPanelID
            string HHID = ObtenerHHID(LenelPanelID);

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select Tarjetas.tarjeta,Nombre,Apellido,NumeroDocumento,Empresa,TipoAcceso,Fecha,Latitud,Longitud,Empleados.imagen as imagenEmpleado,accesos.imagen as imagenAcceso from Accesos,Empleados,Tarjetas where Accesos.SerialNum=" + serialNum + " and Accesos.idHandHeld ='" + HHID + "' and Accesos.idEmpleado = Empleados.IdEmpleado and Tarjetas.idEmpleado = Empleados.IdEmpleado";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                // Es un while, pero en realidad si hay, solo hay uno con ese SerialNum
                while (lector.Read())
                {
                    Badge = lector["tarjeta"].ToString();
                    Name = lector["Nombre"].ToString();
                    Surname = lector["Apellido"].ToString();
                    SSNO = lector["NumeroDocumento"].ToString();
                    company = lector["Empresa"].ToString();
                    AccessType = lector["TipoAcceso"].ToString();
                    datetime = lector["Fecha"].ToString();
                    Latitude = lector["Latitud"].ToString();
                    Longitude = lector["Longitud"].ToString();
                    imagenEmpleado = lector["imagenEmpleado"].ToString();
                    imagenAcceso = lector["imagenAcceso"].ToString();
                }
                lector.Close();
                cnn.Close();


                if (Badge != "")
                {

                    TiposAcceso tipoAcceso = (TiposAcceso)Enum.Parse(typeof(TiposAcceso), AccessType);
                    GateAccessType tipoReader = obtenerTipoReader(tipoAcceso);

                    readerName = obtenerReaderName(tipoReader, int.Parse(LenelPanelID));

                    res = @"BADGE:" + Badge + ",NAME:" + Name + ",SURNAME:" + Surname + ",SSNO:" + SSNO + ",COMPANY:" + company + ",HHID:" + HHID + ",ACCESSTYPE:" + AccessType + ",DATETIME:" + datetime + ",LATITUDE:" + Latitude + ",LONGITUDE:" + Longitude + ",IMAGENEMPLEADO:" + imagenEmpleado + ",IMAGENACCESO:" + imagenAcceso + ",READERNAME:" + readerName;
                }
                else
                {
                    res = "FALSE";
                }
            }
            catch (Exception )
            {
            }
            finally
            {
                cnn.Close();
            }

            return res;
        }

        public Tarjeta obtenerTarjetaPorID(string idTarjeta)
        {
            Tarjeta res = null;

            res = listaTarjetas.FirstOrDefault(x => x.Value.idTarjeta.ToString() == idTarjeta).Value;

            return res;
        }



        /// <summary>
        /// Devuelve el tipo de reader asociado a un tipo de acceso: Un acceso de entrada se realiza exclusivamente con un reader de tipo Entrance, etc-
        /// NOTA: Devuelve Granted por defecto
        /// </summary>
        /// <param name="v_tipoAcceso"></param>
        /// <returns></returns>
        GateAccessType obtenerTipoReader(TiposAcceso v_tipoAcceso)
        {
            GateAccessType res = GateAccessType.Granted;

            switch (v_tipoAcceso)
            {
                case TiposAcceso.Entrada:
                case TiposAcceso.EINVALIDO:
                    res = GateAccessType.Entrance;
                    break;
                case TiposAcceso.Salida:
                case TiposAcceso.SINVALIDO:
                    res = GateAccessType.Exit;
                    break;
                case TiposAcceso.INVALIDO:
                    res = GateAccessType.Forbidden;
                    break;
            }

            return res;
        }
        /// <summary>
        /// Devuelve el tipo de reader asociado a un tipo de reader definido desde LENEL como entranceType: 0: Granted, 1: Entrance, 2: Exit.
        /// NOTA: Devuelve Granted por defecto
        /// </summary>
        /// <param name="v_tipoAcceso"></param>
        /// <returns></returns>
        public GateAccessType obtenerTipoReader(string v_EntranceType)
        {
            GateAccessType res = GateAccessType.Granted;
            switch (v_EntranceType)
            {
                case "0":
                    res = GateAccessType.Granted;
                    break;
                case "1":
                    res = GateAccessType.Entrance;
                    break;
                case "2":
                    res = GateAccessType.Exit;
                    break;
            }
            return res;
        }

        /// <summary>
        /// Busca en la tabla LENELReaders, el nombre del PRIMER reader del tipoGate indicado
        /// </summary>
        /// <param name="v_tipoGate"></param>
        /// <param name="v_LNLPanelID"></param>
        /// <returns></returns>
        private string obtenerReaderName(GateAccessType v_tipoGate, int v_LNLPanelID)
        {
            string readerName = "";

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "Select * from LENELReaders where LNLPanelID=" + v_LNLPanelID.ToString() + " and Type ='" + v_tipoGate.ToString() + "'";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
              
                while (lector.Read())
                {
                    if (lector["Name"] != null)
                    {
                        readerName = lector["Name"].ToString();
                    }
                    break;  // Solo el primero o nada.
                }
                lector.Close();
                cnn.Close();
            }
            catch (Exception ex)
            {
                loguearString("ERROR en obtenerReaderName: " + ex.Message, TiposLOG.ALL);
            }
            finally
            {
                cnn.Close();
            }

            return readerName;
        }


        /// <summary>
        /// Arma el string que se enviara a LENEL con los datos del evento correspondiente a (LeneldeviceID,serialNum)
        /// Buscando en la tabla de VISITAS.
        /// El formato de respuesta es:
        /// BADGE:xxxx,NAME:xxxx,SURNAME:xxx,SSNO:xxxx,COMPANY:xxxx,HHID:xxxx,ACCESSTYPE:Granted|Denied,DATETIME:xxxx,LATITUDE:xxxx,LONGITUDE:xxxxx,IMAGENEMPLEADO:xxxx,IMAGENACCESO:xxxx";
        /// FALSE si no hay registro de acceso en la base
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="serialNum"></param>
        /// <returns></returns>
        public string obtenerDatosEventoDesdeVisitas(string LenelPanelID, string serialNum)
        {
            string res = "";
            string Badge = "";
            string Name = "";
            string Surname = "";
            string SSNO = "";
            string company = "";
            string AccessType = "";
            string datetime = "";
            string Latitude = "";
            string Longitude = "";
            string imagenVisita = "";

            // Primero obtengo el HHID asociado al LenelPanelID
            string HHID = ObtenerHHID(LenelPanelID);

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                string readerName = "";
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select Tarjeta,Nombre,Apellido,SSNO,Empresa,TipoAcceso,Fecha,Latitud,Longitud,visita.imagen as imagenVisita from Visita where Visita.SerialNum=" + serialNum + " and Visita.idDevice ='" + HHID + "'";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    Badge = lector["Tarjeta"].ToString();
                    Name = lector["Nombre"].ToString();
                    Surname = lector["Apellido"].ToString();
                    SSNO = lector["SSNO"].ToString();
                    company = lector["Empresa"].ToString();
                    AccessType = lector["TipoAcceso"].ToString();
                    datetime = lector["Fecha"].ToString();
                    Latitude = lector["Latitud"].ToString();
                    Longitude = lector["Longitud"].ToString();
                    imagenVisita = lector["imagenVisita"].ToString();
                }
                lector.Close();
                cnn.Close();

                TiposAcceso tipoAcceso = (TiposAcceso)Enum.Parse(typeof(TiposAcceso), AccessType);
                GateAccessType tipoReader = obtenerTipoReader(tipoAcceso);          // Obtengo el tipo de gate que fue usado para el tipo de acceso especificado

                readerName = obtenerReaderName(tipoReader, int.Parse(LenelPanelID));

                if (Badge != "")
                {
                    res = @"BADGE:" + Badge + ",NAME:" + Name + ",SURNAME:" + Surname + ",SSNO:" + SSNO + ",COMPANY:" + company + ",HHID:" + HHID + ",ACCESSTYPE:" + AccessType + ",DATETIME:" + datetime + ",LATITUDE:" + Latitude + ",LONGITUDE:" + Longitude + ",IMAGENEMPLEADO: ,IMAGENACCESO:" + imagenVisita+",READERNAME:" + readerName;
                }
                else
                {
                    res = "FALSE";
                }
                
            }
            catch (Exception )
            {
            }
            finally
            {
                cnn.Close();
            }
            return res;
        }


        /// <summary>
        /// busca la ultima actualizacion de la posicion GPS en la tabla de accesos del device especificado.
        /// La organizacion no se utuliza por ahora, pero será escencial en la version final)
        /// La devolucion se encodea como: 
        /// LATITUDE:xxx,LONGITUDE:xxx,DATETIME:xxx
        /// </summary>
        /// <param name="v_LNLPanelID"></param>
        /// <param name="v_organizationID"></param>
        /// <returns></returns>
        public string obtenerUltimaPosicionGPS(string v_LNLPanelID, string v_organizationID)
        {
            string res = NO_LOCATION_DATA;
            string HHID = ObtenerHHID(v_LNLPanelID);
            string latitud="";
            string longitud ="";
            string dateTime="";

            if (HHID != "")
            {
                res = obtenerUltimaPosGPS(HHID);
                if (res == string.Empty)
                {
                    SqlConnection cnn = new SqlConnection(conexion);
                    try
                    {
                        cnn.Open();
                        SqlCommand cmd = cnn.CreateCommand();
                       
                        cmd.CommandText = "select top 1 * from eventosGPS where hhid = '" + HHID + "' and longitud !='' and latitud !='' order by hora desc";
                        cmd.CommandType = CommandType.Text;
                        SqlDataReader lector = cmd.ExecuteReader();
                        while (lector.Read())
                        {
                            latitud = lector["Latitud"].ToString();
                            longitud = lector["Longitud"].ToString();
                            dateTime = lector["hora"].ToString();
                        }

                        res = "LATITUDE:" + latitud + ",LONGITUDE:" + longitud + ",DATETIME:" + dateTime + ",HHID:" + HHID;
                    }
                    catch (Exception ex)
                    {
                        loguearString("Excepcion en obtenerUltimaPosicionGPS: " + ex.Message,TiposLOG.HH);
                    }
                    finally
                    {
                        cnn.Close();
                    }


                }
            }
            return res;
        }

        /// <summary>
        /// Arma el string que se enviará al LENEL para que éste genere un acceso en el Alarm Monitoring
        /// Para ello busca en la tabla de accesos el primero que no tenga serialNum asociado, 
        /// devuelve en idAcceso el id de ese acceso y arma el string de devolucion:
        /// Si tipoAcceso == "Entrada", entonces ACCESSTYPE = 1 (Granted) y ReaderID es el ID de la gate tipo GateAccesType.Entrance
        /// Si tipoAcceso == "Salida", entonces ACCESSTYPE = 1 (Granted) y ReaderID es el ID del GateAccesType.Exit
        /// Si tipoAcces0 =="INVALID", entonces ACCESSTYPE = 0 (Denied) y ReaderID es el ID de GateAccesType.Entrance
        /// El formato de devolucion es: ACCESSTYPE,BADGE,PANELID,READERID";
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="idAcceso"></param>
        /// <returns></returns>
        public string  obtenerAccesoSinSerialNum(string deviceName, ref int  idAcceso) 
        {
            string res = "";
            string tipoAcceso = "";
            string badge = "";
            string HHID = "";
            string readerID = "";
            string accessType = "";

            idAcceso = -1;

            SqlConnection cnn = new SqlConnection(conexion);

            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select top 1 * from Accesos where SerialNum is null and idHandHeld = '" + deviceName + "' order by Fecha asc";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    idAcceso = Convert.ToInt32(lector["idAcceso"].ToString());
                    tipoAcceso = lector["TipoAcceso"].ToString();
                    badge = lector["Tarjeta"].ToString();
                    HHID = lector["idHandHeld"].ToString();
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerAccesoSinSerialNum: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }


            if (tipoAcceso.ToLower() == ENTRADA.ToLower())
            {
                readerID = obtenerLenelReaderIDEntrada(HHID);
                accessType = ACCESS_GRANTED;
            }
            if (tipoAcceso.ToLower() == SALIDA.ToLower())
            {
                readerID = obtenerLenelReaderIDSalida(HHID);
                accessType = ACCESS_GRANTED;
            }

            if (tipoAcceso.ToLower() == SINVALIDO.ToLower())     // Tarjeta registrada pero inactiva
            {
                readerID = obtenerLenelReaderIDSalida(HHID);
                accessType = ACCESS_DENIED;
            }

            if (tipoAcceso.ToLower() == EINVALIDO.ToLower())     // Tarjeta registrada pero inactiva
            {
                readerID = obtenerLenelReaderIDEntrada(HHID);
                accessType = ACCESS_DENIED;
            }

            if (idAcceso > 0) // Encontro un acceso para enviar
            {
                string panelID = ObtenerLenelPanelID(HHID);
                res = accessType + "," + badge + "," + panelID + "," + readerID;
            }
            return res;
        }


        /// <summary>
        /// Arma el string que se enviará al LENEL para que éste genere un acceso en el Alarm Monitoring
        /// Para ello busca en la tabla de VISITAS el primero que no tenga serialNum asociado, 
        /// devuelve en idAcceso el id de ese acceso y arma el string de devolucion:
        /// Si tipoAcceso == "Entrada", entonces ACCESSTYPE = 1 (Granted) y ReaderID es el ID de la gate con Type = GateAccessType.Entrance
        /// Si tipoAcceso == "Salida", entonces ACCESSTYPE = 1 (Granted) y ReaderID es el ID de la gate con Type = GateAccessType.Exit
        /// Si tipoAcces0 =="INVALID", entonces ACCESSTYPE = 0 (Denied) y ReaderID es el ID de la gate con Type = GateAccessType.Entrance
        /// El formato de devolucion es: ACCESSTYPE,BADGE,PANELID,READERID";
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="idAcceso"></param>
        /// <returns></returns>
        public string obtenerVisitaSinSerialNum(string deviceName, ref int idAcceso)
        {
            string res = "";
            string tipoAcceso = "";
            string badge = "";
            string HHID = "";
            string readerID = "";
            string accessType = "";

            idAcceso = -1;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select top 1 * from Visita where SerialNum is null and idDevice = '" + deviceName + "' order by Fecha asc";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    idAcceso = Convert.ToInt32(lector["id"].ToString());
                    tipoAcceso = lector["TipoAcceso"].ToString();
                    badge = lector["Tarjeta"].ToString();
                    HHID = lector["idDevice"].ToString();
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerVisitaSinSerialNum: " + ex.Message,TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            if (tipoAcceso.ToLower() == ENTRADA.ToLower())
            {
                readerID = obtenerLenelReaderIDEntrada(HHID);
                accessType = ACCESS_GRANTED;
            }
            if (tipoAcceso.ToLower() == SALIDA.ToLower())
            {
                readerID = obtenerLenelReaderIDSalida(HHID);
                accessType = ACCESS_GRANTED;
            }

            if (tipoAcceso.ToLower() == EINVALIDO.ToLower())     // Tarjeta registrada pero inactiva
            {
                readerID = obtenerLenelReaderIDEntrada(HHID);
                accessType = ACCESS_DENIED;
            }

            if (tipoAcceso.ToLower() == SINVALIDO.ToLower())     // Tarjeta registrada pero inactiva
            {
                readerID = obtenerLenelReaderIDSalida(HHID);
                accessType = ACCESS_DENIED;
            }

            if (idAcceso > 0) // Encontro un acceso para enviar
            {
                string panelID = ObtenerLenelPanelID(HHID);
                res = accessType + "," + badge + "," + panelID + "," + readerID;
            }
            return res;
        }


        private string obtenerLenelReaderIDSalida(string v_HHID)
        {
            string strLNLPanelID = ObtenerLenelPanelID(v_HHID);


            string entranceReaderID = "";

            if (strLNLPanelID != "")
            {
                int LNLPanelID = int.Parse(strLNLPanelID);
                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "select * from LenelReaders where LNLPanelID =" + LNLPanelID + " and Type='" + GateAccessType.Exit.ToString() + "'";
                    cmd.CommandType = CommandType.Text;
                    SqlDataReader lector = cmd.ExecuteReader();
                    while (lector.Read())
                    {
                        entranceReaderID = lector["LNLReaderID"].ToString();
                    }
                    lector.Close();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en obtenerLenelReaderIDSalida: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }

            }
            return entranceReaderID;
        }

        private string obtenerLenelReaderIDEntrada(string v_HHID)
        {

            string  strLNLPanelID = ObtenerLenelPanelID(v_HHID);

            string entranceReaderID = "";

            if (strLNLPanelID != "")
            {
                int LNLPanelID = int.Parse(strLNLPanelID);
                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "select * from LenelReaders where LNLPanelID =" + LNLPanelID + " and Type='" + GateAccessType.Entrance.ToString() + "'";
                    cmd.CommandType = CommandType.Text;
                    SqlDataReader lector = cmd.ExecuteReader();
                    while (lector.Read())
                    {
                        entranceReaderID = lector["LNLReaderID"].ToString();
                    }
                    lector.Close();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en obtenerLenelReaderIDEntrada: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }

            }
            return entranceReaderID;
        }


        /// <summary>
        /// Obtiene el ID de la organizacion asociado a un HHID, el cual es UNICO: clave primaria de la tabla Devices.
        /// </summary>
        /// <param name="v_HHID"></param>
        /// <returns></returns>
        public int obtenerOrganizationIDFromHHID(string v_HHID)
        {
            int res = -1;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Devices where hhid ='" + v_HHID + "'";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    res = int.Parse(lector["idOrganizacion"].ToString());
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerOrganizationIDFromHHID: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            return res;
        }

        /// <summary>
        /// Dado el HHID del handHeld devuelve el LNLPanelID asociado en LENEL
        /// Fundamental: TODOS LOS HHID son DIFERENTES. Es una PRIMARY KEY de la tabla Devices.
        /// </summary>
        /// <param name="v_HHID"></param>
        /// <returns></returns>
        public string ObtenerLenelPanelID(string v_HHID)
        {
            string lnlPanelIDstr = "";
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Devices where hhid ='" + v_HHID + "'";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    lnlPanelIDstr = lector["LNLPanelID"].ToString();
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en ObtenerLenelPanelID: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            return lnlPanelIDstr;
        }


        /// <summary>
        /// Dado el LenelPanelID devuelve el HHID asociado.
        /// </summary>
        /// <param name="LENELPanelID"></param>
        /// <returns></returns>
        public string ObtenerHHID(string LENELPanelID)
        {
            string HHIDstr="";

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Devices where LNLPanelID =" + LENELPanelID;
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    HHIDstr = lector["hhid"].ToString();
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en ObtenerHHID: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            return HHIDstr;
        }

        /// <summary>
        /// Dado un AccessLevels de una organizacion retorna la lista de HH que lo tienen asociado.
        /// </summary>
        /// <param name="LENELPanelID"></param>
        /// <returns></returns>
        public List<string> ObtenerListaHHIdPorAccessLevel(string idAccess, int Organizacion)
        {           
            List<string> ListaHH = new List<string>();
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                //cmd.CommandText = "select distinct LnLPanelID  from AccessLevels where idAccess =" + AccessLevels + " and idOrganizacion=" + Organizacion.ToString() ;
                cmd.CommandText = "select distinct devices.HHID from accesslevels,Devices where AccessLevels.LNLPanelID = devices.LNLPanelID and idAccess = " + idAccess + " and Accesslevels.idOrganizacion =" + Organizacion;
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    if (lector["HHID"] != null)
                    {
                        string resultado = lector["HHID"].ToString();
                        ListaHH.Add(resultado);
                    }                   
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en ObtenerListaHHIdPorAccessLevel: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            return ListaHH;
        }   



        /// <summary>
        /// Le pone el serialnum al acceso, como para que la proxima vez el pedido de un acceso SIN serialnum, no lo considere.
        /// </summary>
        /// <param name="v_idAcceso"></param>
        /// <param name="v_serialNum"></param>
        public void actualizarSerialNumEnAccesos(int v_idAcceso, string v_serialNum)
        {
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "update Accesos set SerialNum=" + v_serialNum + " where idAcceso = " + v_idAcceso.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en actualizarSerialNumEnAccesos: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
        }

        /// <summary>
        /// Le pone el serialnum a la visita, como para que la proxima vez el pedido de una visita SIN serialnum, no lo considere.
        /// </summary>
        /// <param name="v_idAcceso"></param>
        /// <param name="v_serialNum"></param>
        public void actualizarSerialNumEnVisitas(int v_idAcceso, string v_serialNum)
        {
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "update Visita set SerialNum=" + v_serialNum + " where id = " + v_idAcceso.ToString();
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en actualizarSerialNumEnVisitas: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

        }
        
        // Borra todos los empleados de la organizacion cuyo isDownloadingDatabase =0.
        // Luego actualiza isDownloadingDatabase con cero, para que quede registrada para la proxima bajada de empleados.
        public void  borrarListaPreviaEmpleados(int orgID)
        {

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "delete from empleados where isDownloadingDB= 0 and idOrganizacion = " + orgID;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en borrarListaPreviaEmpleados, fase 1: borrar los no actualizados " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "update empleados set isDownloadingDB=0 where idOrganizacion = " + orgID;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en borrarListaPreviaEmpleados, fase 2: resetear flag de actualizacion " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
        }


        /// <summary>
        /// Agrega un device desde un pedido de LENEL. Si ya está entonces NO lo agrega. 
        /// Potencialmente puede actualizar su nombre a partir del ID
        /// NUEVO: Setea en 1 el flag de isDownloadingDB, para marcar que va a comenzar el DownloadDatabase.
        /// </summary>
        /// <param name="v_LENELPanelID"></param>
        /// <param name="v_LENELPanelName"></param>
        /// <param name="v_organizacion"></param>
        public void agregarDeviceDesdeLENEL(string v_LENELPanelID, string v_LENELPanelName, string v_organizacion )
        {

            bool doAdd = false;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "select * from Devices where hhid='" + v_LENELPanelName + "' and idOrganizacion =" + v_organizacion;
                cmd.CommandType = CommandType.Text;
                SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    doAdd = true;         // Si no hay nada para leer, lo modifico.
                }
                reader.Close();


                if (doAdd)
                {
                    cmd = cnn.CreateCommand();
                    cmd.CommandText = "insert into Devices(hhid,idMarca,idModelo,Activo,IdGrupoDevice,LENEL,idOrganizacion,LNLPanelID,isDownloadingDB)values('" + v_LENELPanelName + "',6,2,1,null,1," + v_organizacion + "," + v_LENELPanelID +",1)";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    cmd = cnn.CreateCommand();
                    cmd.CommandText = "update Devices set LNLPanelID ='" + v_LENELPanelID + "', isDownloadingDB=1 where hhid = '" + v_LENELPanelName + "'and idOrganizacion =" + v_organizacion;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en agregarDeviceDesdeLENEL: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
        }

        public void addEventoGPS(string id_hh, EventoGPS v_evento)
        {
            try
            {
                StaticTools.obtenerMutex_ListaEventosGPS();
                if (!listaEventosGPS.ContainsKey(id_hh))
                {
                    listaEventosGPS.Add(id_hh, new List<EventoGPS>());
                }

                if (listaEventosGPS.ContainsKey(id_hh))
                {
                    List<EventoGPS> eventos = listaEventosGPS[id_hh];
                    eventos.Add(v_evento);

                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addEventoGPS: " + ex.Message,TiposLOG.HH);
            }
            finally
            {
                StaticTools.liberarMutex_ListaEventosGPS();
            }


            SqlConnection cnn = new SqlConnection(conexion);
            SqlCommand cmd = new SqlCommand("AgregarEventosGps", cnn);
        
            try
            {
                cnn.Open();

                cmd = cnn.CreateCommand();
                cmd.CommandText = "insert into EventosGps(hhid,latitud,longitud,Hora)values('" + v_evento.HHID+ "','" + v_evento.Latitud + "','"+v_evento.Longitud+"','" + v_evento.Hora+"')";
                
                cmd.CommandType = CommandType.Text;
                
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addEventoGPS del HH: " + id_hh + " - " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

        }

        public EventoGPS getLastEventoGPS(string HHID)
        {
            EventoGPS retEvento = null;

            try
            {
                StaticTools.obtenerMutex_ListaEventosGPS();

                if (listaEventosGPS.ContainsKey(HHID))
                {
                    List<EventoGPS> lstEventos = listaEventosGPS[HHID];

                    retEvento = lstEventos[lstEventos.Count - 1]; // Zero based array...
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en getLastEventoGPS:" + ex.Message,TiposLOG.HH);
            }
            finally
            {
                StaticTools.liberarMutex_ListaEventosGPS();
            }

            return retEvento;
        }


        /// <summary>
        /// Nueva version: Utiliza las tablas originales del ALUTRACK para chequear los accesos a puertas virtuales.
        /// </summary>
        /// <param name="GPSDesde"></param>
        /// <param name="GPSHacia"></param>
        public void updateVirtualGateEvents(EventoGPS GPSDesde, EventoGPS GPSHacia)
        {
            
            // El movimiento se obtuvo desde A hacia B en coordenadas cartesianas
            Punto A = new Punto(float.Parse(GPSDesde.Latitud, CultureInfo.InvariantCulture.NumberFormat), float.Parse(GPSDesde.Longitud, CultureInfo.InvariantCulture.NumberFormat));
            Punto B = new Punto(float.Parse(GPSHacia.Latitud, CultureInfo.InvariantCulture.NumberFormat), float.Parse(GPSHacia.Longitud, CultureInfo.InvariantCulture.NumberFormat));


            string HHID = GPSDesde.HHID;


            int orgID = obtenerOrganizationIDFromHHID(HHID);

            if (orgID > 0)      // Tengo la Organizacion y el Nombre del Device GPS. 
            {
                // Busco la tarjeta asociada a ese empleadoGPS, de esa organizacion: Busco por Nombre y Organizacion... weak...
               
                int IDEmpleadoGPS = obtenerEmpleadoGPS(HHID,orgID);         // El ID del empleado correspondiente al GPS
                
                if (IDEmpleadoGPS >0)
                {
                    string TarjetaEmpleadoGPS = obtenerTarjetaGPS(IDEmpleadoGPS, orgID);        // La tarjeta de ese empleado


                    if (TarjetaEmpleadoGPS != "")

                    {
                       
                        // Ahora obtengo la lista de idFeatures de la organizacion.

                        Dictionary<int, string> listaIDFeatures = obtenerListaFeatures(orgID);

                        // Ahora cargo las zonas correspondientes a las features

                        Dictionary<string, Zone> listaZonas = LoadZonasFromFeatures(listaIDFeatures);

                        // Ahora chequeo la interseccion de las Zonas....

                        foreach (KeyValuePair<string, Zone> zone in listaZonas)
                        {
                            foreach (KeyValuePair<string, Zone.GateDefinition> gate in zone.Value.listaPuertas)
                            {
                                Punto C = new Punto(float.Parse(gate.Value.from.position.latitude, CultureInfo.InvariantCulture.NumberFormat), float.Parse(gate.Value.from.position.longitude, CultureInfo.InvariantCulture.NumberFormat));
                                Punto D = new Punto(float.Parse(gate.Value.to.position.latitude, CultureInfo.InvariantCulture.NumberFormat), float.Parse(gate.Value.to.position.longitude, CultureInfo.InvariantCulture.NumberFormat));

                                if (intersecta(A, B, C, D))
                                {
                                    TiposAcceso tipoAcceso;

                                    // El punto intersecto un segmento.
                                    // Uso el algoritmo Ray Casting para saber si quedo dentro o fuera de la zona.
                                    //Algoritmo RAYCAST: devuelve la cantidad de intersecciones al  poligono: Si es un numero impar, es entrada. Si es un numero par, es salida
                                    int numInter = RayCast(zone.Value, B);

                                    if (numInter % 2 != 0)      // Chequea si es impar, entonces es entrada
                                    {
                                        tipoAcceso = TiposAcceso.Entrada;
                                    }
                                    else                                // Si es par es salida.
                                    {
                                        tipoAcceso = TiposAcceso.Salida;
                                    }

                                    TiposAcceso tipoEvento = TiposAcceso.INVALIDO;      // Por defecto...

                                    switch (gate.Value.type)
                                    {
                                        case GateAccessType.Granted:
                                            tipoEvento = tipoAcceso;
                                            break;
                                        case GateAccessType.Forbidden:
                                            tipoEvento = TiposAcceso.INVALIDO;
                                            break;
                                        case GateAccessType.Entrance:
                                            if (tipoAcceso == TiposAcceso.Entrada)
                                            {
                                                tipoEvento = TiposAcceso.Entrada;
                                            }
                                            else
                                            {
                                                tipoEvento = TiposAcceso.EINVALIDO;
                                            }
                                            break;
                                        case GateAccessType.Exit:
                                            if (tipoAcceso == TiposAcceso.Salida)
                                            {
                                                tipoEvento = TiposAcceso.Salida;
                                            }
                                            else
                                            {
                                                tipoEvento = TiposAcceso.SINVALIDO;
                                            }
                                            break;
                                    }

                                    //ZoneAccess nuevoAcceso = new ZoneAccess(GPSDesde.HHID, zone.Key, gate.Value.ID, GPSHacia.Hora, tipoEvento);

                                    agregarAccesoDesdeGPS(IDEmpleadoGPS, GPSHacia.Latitud, GPSHacia.Longitud, HHID, GPSHacia.Hora, tipoEvento, TarjetaEmpleadoGPS, orgID,gate.Value.LNLPanelID,gate.Value.LNLReaderID);

                                    //addAccesoZona(nuevoAcceso);
                                   
                                }

                            }

                        }
                    }
                }
            }
        }

        // Obtiene de la tabla de Empleados, aquel idEmpleado cuyo Nombre es v_HHID y pertenece ala organizacion v_orgID
        public int obtenerEmpleadoGPS(string v_HHID, int v_orgID)
        {

            int res = -1;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "Select * from Empleados where Nombre='" + v_HHID + "' and idOrganizacion =" + v_orgID.ToString();
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    if (lector["idEmpleado"] != null)
                    {
                        string resStr = lector["idEmpleado"].ToString();

                        res = int.Parse(resStr);
                    }
                    break;  // Solo el primero o nada.
                }
                lector.Close();
                cnn.Close();
            }
            catch (Exception ex)
            {
                loguearString("ERROR en obtenerEmpleadoGPS: " + ex.Message, TiposLOG.ALL);
            }
            finally
            {
                cnn.Close();
            }
            return res;
        }


        // Obtiene la tarjeta correspondiente al idEmpleadoGPS, de la organizacion
        public string obtenerTarjetaGPS(int v_IDEmpleadoGPS, int v_orgID)
        {
            string res = "";

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "Select * from Tarjetas where idEmpleado=" + v_IDEmpleadoGPS + " and idOrganizacion =" + v_orgID.ToString();
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    if (lector["Tarjeta"] != null)
                    {
                        res = lector["Tarjeta"].ToString();
                    }
                    break;  // Solo el primero o nada.
                }
                lector.Close();
                cnn.Close();
            }
            catch (Exception ex)
            {
                loguearString("ERROR en obtenerTarjetaGPS: " + ex.Message, TiposLOG.ALL);
            }
            finally
            {
                cnn.Close();
            }

            return res;
        }

        // Obtiene la lista de AccessLevels asociados a los de la tarjeta de la organizacion. 
        // NUEVA VERSION: lo saca de la tabla Tarjetas
        public string obtenerListaAccessLevels(string v_IDTarjetaEmpleado, int v_orgID)
        {
            string res = "";

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "Select * from Tarjetas where idTarjeta=" + v_IDTarjetaEmpleado + " and idOrganizacion =" + v_orgID.ToString();
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    if (lector["AccessLevels"] != null)
                    {
                        res = lector["AccessLevels"].ToString();
                    }
                    break;  // Solo el primero o nada.
                }
                lector.Close();
                cnn.Close();
            }
            catch (Exception ex)
            {
                loguearString("ERROR en obtenerListaAccessLevels: " + ex.Message, TiposLOG.ALL);
            }
            finally
            {
                cnn.Close();
            }
            return res;
        }

        // Saca de la tabla de Features, que ahora tiene LNLPanerlID, OrgID, la lista de ID de la organizacion
        public Dictionary<int, string> obtenerListaFeatures(int v_orgID)
        {
            Dictionary<int, string> res = new Dictionary<int, string>();

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "Select * from Features where OrgID =" + v_orgID.ToString();
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    if (lector["id"] != null)
                    {
                        string resStr = lector["id"].ToString();
                        res.Add(int.Parse(resStr), lector["Nombre"].ToString());
                    }
                    break;  // Solo el primero o nada.
                }
                lector.Close();
                cnn.Close();
            }
            catch (Exception ex)
            {
                loguearString("ERROR en obtenerListaFeatures: " + ex.Message, TiposLOG.ALL);
            }
            finally
            {
                cnn.Close();
            }

            return res;
        }


        public Dictionary<string, Zone> LoadZonasFromFeatures(Dictionary<int, string> listaIDFeatures)
        {
            // La lista desde cero..
            Dictionary<string, Zone> resList = new Dictionary<string, Zone>();

            foreach (KeyValuePair<int, string> Pair in listaIDFeatures)
            {
                string nombreZona = Pair.Value;
                int idFeature = Pair.Key;

                // Crea la nueva  zona.
                Zone nuevaZona = new Zone(nombreZona);

                // Recorre la base para agregarle Gates...
                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "Select * from gates where idFeature =" + idFeature.ToString();
                    cmd.CommandType = CommandType.Text;
                    SqlDataReader lector = cmd.ExecuteReader();

                    while (lector.Read())
                    {
                        string NombreGate = lector["Nombre"].ToString();
                        Zone.ZonePoint PuntoFrom = ConstruirZonePoint(int.Parse(lector["Punto1"].ToString()), idFeature);
                        Zone.ZonePoint PuntoTo = ConstruirZonePoint(int.Parse(lector["Punto2"].ToString()), idFeature);
                        
                        int LNLPanelID=-1;
                        int LNLReaderID=-1;

                        if (!string.IsNullOrEmpty(lector["LNLPanelID"].ToString()))
                        {
                            LNLPanelID = int.Parse(lector["LNLPanelID"].ToString());
                        }
                        if (!string.IsNullOrEmpty(lector["LNLReaderID"].ToString()))
                        {
                            LNLReaderID = int.Parse(lector["LNLReaderID"].ToString());
                        }

                        if ((LNLPanelID > 0) && (LNLReaderID > 0))
                        {
                            GateAccessType tipoAcceso;

                            try
                            {
                                tipoAcceso = (GateAccessType)Enum.Parse(typeof(GateAccessType), lector["Acceso"].ToString());
                            }
                            catch (Exception )
                            {
                                tipoAcceso = GateAccessType.Forbidden;
                            }

                            // Crea la nueva Gate
                            Zone.GateDefinition nuevaGate = new Zone.GateDefinition();

                            nuevaGate.ID = NombreGate;
                            nuevaGate.from = PuntoFrom;
                            nuevaGate.to = PuntoTo;
                            nuevaGate.type = tipoAcceso;
                            nuevaGate.LNLReaderID = LNLReaderID;
                            nuevaGate.LNLPanelID = LNLPanelID;

                            // Agrega la gate a la Zona.
                            nuevaZona.addGate(NombreGate, nuevaGate);
                        }

                    }
                    lector.Close();
                    cnn.Close();
                }
                catch (Exception ex)
                {
                    loguearString("ERROR en obtenerListaFeatures: " + ex.Message, TiposLOG.ALL);
                }
                finally
                {
                    cnn.Close();
                }
            
                // Da de alta la zona con sus gates en el diccionario de zonas.
                resList.Add(nombreZona, nuevaZona);
            }

            return resList;

        }


        private Zone.ZonePoint ConstruirZonePoint(int v_ordinalPunto, int v_idFeature)
        {
           Zone.ZonePoint nuevoPunto = new Zone.ZonePoint();

            
           //SqlConnection cnn = new SqlConnection(conexion);

           string latLong = obtenerLatLong(v_ordinalPunto, v_idFeature);
           if (latLong != "")
           {
               string[] coord = latLong.Split(',');
               nuevoPunto.position.latitude = coord[0];
               nuevoPunto.position.longitude = coord[1];
           }

           return nuevoPunto;

        }
    
        /// <summary>
        /// Paso final para agregar un acceso desde un GPS. Ya se reviso el tema geometrico y se tiene el Empleado, su tarjeta, la latitud, longitud, hora, tipode evento y organizacion
        /// Falta revisar que tenga accesslevel y actualizar la tabla de Accesos.
        /// </summary>
        /// <param name="v_IDEmpleadoGPS"></param>
        /// <param name="v_Latitud"></param>
        /// <param name="v_Longitud"></param>
        /// <param name="v_HHID"></param>
        /// <param name="v_Hora"></param>
        /// <param name="v_tipoEvento"></param>
        /// <param name="v_TarjetaEmpleadoGPS"></param>
        /// <param name="v_orgID"></param>
        public void agregarAccesoDesdeGPS(int v_IDEmpleadoGPS,string v_Latitud,string v_Longitud,string v_HHID,string v_Hora,TiposAcceso v_tipoEvento,string v_TarjetaEmpleadoGPS,int v_orgID, int v_LNLPanelID, int v_LNLReaderID)
        {
            // Tengo la tarjeta correspondiente al empleadoGPS, obtengo ahora su lista de accesslevels, para esa organizacion, claro.
            string accessLevels = obtenerListaAccessLevels(v_TarjetaEmpleadoGPS, v_orgID);

            string[] listaAccessLevels = accessLevels.Split(',');

            bool conAccessLevel = false;

            // Loop Principal de verificacion del accessLevel del empleado.
            foreach (string idAccess in listaAccessLevels)
            {
                if (TieneAcceso(int.Parse(idAccess), v_Hora,v_LNLPanelID, v_LNLReaderID, v_orgID))
                {
                    conAccessLevel = true;
                    break;
                }
            }

            
            TiposAcceso tipoAccesoFinal = TiposAcceso.INVALIDO;

            if (!conAccessLevel)
            {
                if ( v_tipoEvento == TiposAcceso.Entrada)
                {
                    tipoAccesoFinal =  TiposAcceso.EINVALIDO;
                }

                if ( v_tipoEvento == TiposAcceso.Salida)
                {
                    tipoAccesoFinal =  TiposAcceso.SINVALIDO;
                }
            }
            else
            {
                tipoAccesoFinal = v_tipoEvento;
            }

            // Por fin dar de alta en la tabla de Accesos para que lo agarre el PollPanelForEvents y se vea en el Alarm Monitoring.
            Acceso nuevoAcceso = new Acceso(v_IDEmpleadoGPS, v_HHID, v_TarjetaEmpleadoGPS, v_Latitud, v_Longitud, v_Hora, null, tipoAccesoFinal);
            addAcceso(nuevoAcceso);
        }

        // Devuelve TRUE si el empleado tiene accessLevel para realizar un acceso Valido.
        public bool TieneAcceso(int idAcceso, string v_hora, int LNLPanelID, int LNLReaderID, int OrgID)
        {
            bool res = false;
            // Obtengo la lista de todo desde AccesslevelLogic.

            Dictionary<int, LENELTimeZones> ListaTimeZones = AccessLevelLogic.ListaTimeZones; ;
            Dictionary<int, LENELAccessLevels>  ListaAccessLevels = AccessLevelLogic.ListaAccessLevels;
            Dictionary<int, LENELBadgeAccessLevels> ListaBadgeAccessLevels = AccessLevelLogic.ListaBadgeAccessLevels;

            DateTime fechaHoraAcceso = generarDateTime(v_hora);

            if (ListaAccessLevels.ContainsKey(OrgID))
            {
                LENELAccessLevels accessLevels = ListaAccessLevels[OrgID];      // Obtengo la lista de accessLevels definidos para la organizacion

                AccessLevelData defAccessLevel = accessLevels.getAccessLevel(LNLPanelID);       // Obtengo los accessLevels del LNLPanel
                if ( defAccessLevel!= null)
                {
                    if (defAccessLevel.ReaderTZ.ContainsKey(idAcceso))
                    {
                        Dictionary<int,int> readerTZ = defAccessLevel.ReaderTZ[idAcceso];       // Obtengo la lista de asociaciones ReaderID, Timezone del accesslevel


                        if (readerTZ.ContainsKey(LNLReaderID))      // La key es el readerID
                        {
                            int timeZoneNumber = readerTZ[LNLReaderID];     // Tengo el numero de TimeZone asociado al LNLReaderID

                            // Cada Timezone se define como un conjunto de TZData.

                            if (ListaTimeZones.ContainsKey(OrgID))
                            {
                                LENELTimeZones timeZones = ListaTimeZones[OrgID];

                                TimeZoneData timeZoneData= timeZones.getTimeZoneData();

                                if (timeZoneData.TZDefinition.ContainsKey(timeZoneNumber))
                                {
                                    List<TZInterval> listaIntervalos = timeZoneData.TZDefinition[timeZoneNumber];

                                    foreach (TZInterval intervalo in listaIntervalos)
                                    {
                                        if (incluidoEnIntervalo(intervalo, fechaHoraAcceso, OrgID))
                                        {
                                            res = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return res;
        }

        // Chequea si el acceso esta incluido en las Holidays.
        private bool incluidoEnHolidays(DateTime fechaHoraAcceso, int v_OrgID, TZInterval intervalo)
        {
            Dictionary<int, List<HolidayData>> ListaHolidays = AccessLevelLogic.ListaHolidays;
            bool res = false;

            if (ListaHolidays.ContainsKey(v_OrgID))
            {
                List<int> tiposHol = intervalo.obtenerListaTiposHoilidays();

                // Obtengo las holidays de la organizacion
                List<HolidayData> LENELHolidays = ListaHolidays[v_OrgID];

                foreach (int tipoToAdd in tiposHol)
                {
                    foreach (HolidayData holi in LENELHolidays)
                    {
                        if (holi.contieneBit(tipoToAdd))
                        {
                            DateTime FechaIni = new DateTime(holi.año,holi.mes,holi.dia);
                            DateTime FechaFin = FechaIni.AddDays(holi.cantDias);

                            if ((FechaIni.CompareTo(fechaHoraAcceso) <= 0) && (FechaFin.CompareTo(fechaHoraAcceso) >= 0))
                            {
                                res = true;
                            }
                        }
                    }
                }
            }
            return res;
        }

        // Chequea si el acceso esta incluido en el intervalo especificado
        private bool incluidoEnIntervalo(TZInterval v_intervalo, DateTime fechaHora, int v_OrgID)
        {

            bool res = false;

            if (!incluidoEnHolidays(fechaHora, v_OrgID, v_intervalo))                 // Si no estamos en una de las vacaciones definidas para la organizacion
            {

                DateTime RangoInicio = GenerarRango(v_intervalo.horaIni, v_intervalo.minIni);
                DateTime RangoFin = GenerarRango(v_intervalo.horaFin, v_intervalo.minFin);


                string listadias = v_intervalo.obtenerListaDias();      // Devuelve la lista de dias del intervalo, separados por coma.


                if (listadias.Contains(((int)fechaHora.DayOfWeek).ToString()) )      // Estamos en uno de los dias de la semana del intervalo?
                {
                    DateTime fechaAComparar = new DateTime(1, 1, 1, fechaHora.Hour, fechaHora.Minute, 0);
                    if ((RangoInicio.CompareTo(fechaAComparar) <= 0) && (RangoFin.CompareTo(fechaAComparar) >= 0))
                    {
                        res = true;
                    }
                }
            }
            return res;
        }

        private DateTime GenerarRango(int _horaInicio, int _minInicio)
        {
            return new DateTime(1, 1, 1, _horaInicio, _minInicio, 0);
        }


        // Genera un objeto DateTime a partir de una fecha/Hora especificada en un string con el fomato: 
        // YYYY-MM-DD HH:MM:SS
        // OJO: Esto es estricto!!!!

        private DateTime generarDateTime(string v_Fechahora)
        {
            DateTime res= new DateTime();

            Regex FECHAHORA = new Regex(@"(.*)-(.*)-(.*) (.*):(.*):(.*)");
            

            Match MatchFecha = FECHAHORA.Match(v_Fechahora);
            if (MatchFecha.Success)
            {

                string Año = getMatchData(MatchFecha, 1);
                string Mes = getMatchData(MatchFecha, 2);
                string Dia = getMatchData(MatchFecha, 3);

                string Hora = getMatchData(MatchFecha, 4);
                string Minuto = getMatchData(MatchFecha, 5);
                string Segundo = getMatchData(MatchFecha, 6);

                res = new DateTime(int.Parse(Año), int.Parse(Mes), int.Parse(Dia), int.Parse(Hora), int.Parse(Minuto), int.Parse(Segundo));

            }
            return res;
        }

        /// <summary>
        /// Extrae un campo de una expresion regular reconocida
        /// </summary>
        /// <param name="resultMatch"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public string getMatchData(Match resultMatch, int index)
        {
            return resultMatch.Groups[index].Value;
        }

        /// <summary>
        /// Algoritmo de Ray Casting para saber si un punto esta o no dentro de un poligono (Convexo o Concavo!!)
        /// Devuelve la cantidad de intersecciones que un rayo horizontal le hace al poligono. 
        /// </summary>
        /// <param name="v_zona"></param>
        /// <param name="v_punto"></param>
        /// <returns></returns>
        public int RayCast(Zone v_zona, Punto v_punto)
        {
            int cantIntersecciones = 0;
            Punto puntoLejano = new Punto(-100.0f, v_punto.Y);  // Punto horizontal lejano...

            foreach (KeyValuePair<string, Zone.GateDefinition> gate in v_zona.listaPuertas)
            {

                Punto C = new Punto(float.Parse(gate.Value.from.position.latitude, CultureInfo.InvariantCulture.NumberFormat), float.Parse(gate.Value.from.position.longitude, CultureInfo.InvariantCulture.NumberFormat));
                Punto D = new Punto(float.Parse(gate.Value.to.position.latitude, CultureInfo.InvariantCulture.NumberFormat), float.Parse(gate.Value.to.position.longitude, CultureInfo.InvariantCulture.NumberFormat));
                if (intersecta(puntoLejano, v_punto, C, D))
                {
                    cantIntersecciones++;
                }
            }
            return cantIntersecciones;
        }

        /// <summary>
        /// Algoritmo de interseccion de segmentos.
        /// </summary>
        private bool intersecta(Punto A, Punto B, Punto C, Punto D)
        {
            return ccw(A, C, D) != ccw(B, C, D) && ccw(A, B, C) != ccw(A, B, D);
        }

        /// <summary>
        /// Para el algoritmo de interseccion de segmentos.
        /// </summary>
        /// <param name="v_A"></param>
        /// <param name="v_B"></param>
        /// <param name="v_C"></param>
        /// <returns></returns>
        private bool ccw(Punto v_A, Punto v_B, Punto v_C)
        {
            return (v_C.Y - v_A.Y) * (v_B.X - v_A.X) > (v_B.Y - v_A.Y) * (v_C.X - v_A.X);
        }


        /// <summary>
        /// insert into  Visita(IdDevice,nombre,Apellido,SSNO,Empresa,Tarjeta,Latitud,Longitud,Fecha,TipoAcceso,Imagen)values(@idDevice,@nombre,@apellido,@ssno,@Empresa,@tarjeta,@Latitud,@Longitud,@fecha,@TipoAcceso,@Imagen)        
        /// </summary>
        /// <param name="id_visita"></param>
        /// <param name="v_visita"></param>
        public void addVisita(Visita v_visita)
        {
            //SqlConnection cnn = new SqlConnection(conexion);
            //SqlCommand cmd = new SqlCommand("AgregarVisita", cnn);
            //cmd.CommandType = CommandType.StoredProcedure;
            //cmd.Parameters.AddWithValue("@idDevice", v_visita.HHID);
            //cmd.Parameters.AddWithValue("@Nombre", v_visita.Nombre);
            //cmd.Parameters.AddWithValue("@Apellido", v_visita.Apellido);
            //cmd.Parameters.AddWithValue("@SSNO", v_visita.Documento);
            //cmd.Parameters.AddWithValue("@Empresa", v_visita.Empresa);
            //cmd.Parameters.AddWithValue("@Tarjeta", v_visita.Tarjeta);
            //cmd.Parameters.AddWithValue("@Latitud", v_visita.Latitud);
            //cmd.Parameters.AddWithValue("@Longitud", v_visita.Longitud);
            //cmd.Parameters.AddWithValue("@Fecha", v_visita.Hora);
            //cmd.Parameters.AddWithValue("@TipoAcceso", v_visita.tipoAcceso.ToString());
            //cmd.Parameters.AddWithValue("@idOrganizacion", v_visita.tipoAcceso.ToString());

            string imageFileName = "";
            if (v_visita.imagen != null)
            {
                if (v_visita.imagen.Length > 0)
                {
                    DateTime fechaActual = DateTime.Now;

                    // crea un identificador unico para la imagen a guardar.
                    string idImagen = fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString();

                    imageFileName = v_visita.HHID + "_V_" + idImagen + ".jpg";
                    Thread.Sleep(1000);         // Espera un segundo para que el proximo identificador de imagenes sea DIFERENTE
                }
            }
            string imageParameter = "";

            if (imageFileName != "")
            {
                string imagePath = SystemConfiguration.ImagesPath;
                File.WriteAllBytes(imagePath + @"\" + imageFileName, v_visita.imagen);
                imageParameter = @"~/Imagenes/" + imageFileName;
                //cmd.Parameters.AddWithValue("@Imagen", @"~/Imagenes/" + imageFileName);
            }

            SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = @"insert into  Visita(IdDevice,nombre,Apellido,SSNO,Empresa,Tarjeta,Latitud,Longitud,Fecha,TipoAcceso,Imagen,idOrganizacion)values('" + v_visita.HHID + "','" +v_visita.Nombre+"','"+ v_visita.Apellido+"','"+v_visita.Documento + "','" + v_visita.Empresa +"','"+ v_visita.Tarjeta+"','" + v_visita.Latitud + "','"+v_visita.Longitud+"','"+v_visita.Hora + "','"+ v_visita.tipoAcceso.ToString() +"','" + imageParameter+"'," + v_visita.idOrg+")";
                    
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en DataManager.addVisita(): " + ex.Message, TiposLOG.ALUTRACK);
                }
                finally
                {
                    cnn.Close();
                }

        }

        /// <summary>
        /// Agrega un acceso a la lista de accesos en memoria.
        /// </summary>
        /// <param name="v_acceso"></param>
        public void addAcceso(Acceso v_acceso)
        {
            // Agrega el acceso a la lista en memoria

            Dictionary<int, Acceso> listaAccesos = mainApp.DataManager.LoadAccesos();
            try
            {
                List<int> keyList = new List<int>(listaAccesos.Keys);

                int id_acceso;
                if (keyList.Count > 0)
                {
                    id_acceso = keyList[0] + 1;
                }
                else
                {
                    id_acceso = 1;
                }

                // Agrega el acceso a la BD
                SqlConnection cnn = new SqlConnection(conexion);
                SqlCommand cmd = new SqlCommand("AgregarAcceso", cnn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@idHandHeld", v_acceso.HHID);
                cmd.Parameters.AddWithValue("@idEmpleado", v_acceso.idEmpleado);
                cmd.Parameters.AddWithValue("@Latitud", v_acceso.Latitud);
                cmd.Parameters.AddWithValue("@Longitud", v_acceso.Longitud);
                cmd.Parameters.AddWithValue("@Fecha", v_acceso.Hora);
                cmd.Parameters.AddWithValue("@Tarjeta", v_acceso.Tarjeta);
                cmd.Parameters.AddWithValue("@TipoAcceso", v_acceso.tipoAcceso.ToString());

                string imageFileName = "";
                if (v_acceso.imagen != null)
                {
                    if (v_acceso.imagen.Length > 0)
                    {

                        DateTime fechaActual = DateTime.Now;
                        // crea un identificador unico para la imagen a guardar.
                        string idImagen = fechaActual.Year.ToString() + fechaActual.Month.ToString() + fechaActual.Day.ToString() + fechaActual.Hour.ToString() + fechaActual.Minute.ToString() + fechaActual.Second.ToString();
                        imageFileName = v_acceso.HHID + "_" + idImagen + ".jpg";
                        Thread.Sleep(1000);         // Espera un segundo para que el proximo identificador de imagenes sea DIFERENTE

                    }
                }
                if (imageFileName != "")
                {
                    string imagePath = SystemConfiguration.ImagesPath;
                    File.WriteAllBytes(imagePath + @"\" + imageFileName, v_acceso.imagen);
                    cmd.Parameters.AddWithValue("@Imagen", @"~/Imagenes/" + imageFileName);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Imagen", "");
                }


                try
                {
                    cnn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("No se pudo agregar el acceso,consulte con el administrador. Error: " + ex.Message);
                }
                finally
                {
                    cnn.Close();
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addAcceso: " + ex.Message, TiposLOG.LENEL);
            }

        }

        /// <summary>
        /// Verifica si ya hay un acceso con la misma hora.
        /// </summary>
        /// <param name="v_acceso"></param>
        /// <returns></returns>
        public bool existeAcceso(Acceso v_acceso)
        {
            Dictionary<int,Acceso> lstAccesos = mainApp.DataManager.LoadAccesos();
            foreach (Acceso _acceso in lstAccesos.Values)
            {
                if (_acceso.Hora.Trim() == v_acceso.Hora.Trim())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Verifica si ya hay un acceso con la misma hora.
        /// </summary>
        /// <param name="v_acceso"></param>
        /// <returns></returns>
        public bool existeVisita(Visita v_visit)
        {

            Dictionary<string, Visita> listaVisitas = mainApp.DataManager.getListaVisitas();

            foreach (Visita _visit in listaVisitas.Values)
            {
                if (_visit.Hora.Trim() == v_visit.Hora.Trim())
                {
                    return true;
                }
            }

            return false;
        }

        public void addDevice(string id_Device, device v_device)
        {
            // Agrega el acceso a la BD
            SqlConnection cnn = new SqlConnection(conexion);
            SqlCommand cmd = new SqlCommand("AgregarDispositivo", cnn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@idHandHeld", v_device.ID);
            cmd.Parameters.AddWithValue("@marca", v_device.MARCA);
            cmd.Parameters.AddWithValue("@modelo", v_device.MODELO);

            try
            {
                cnn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar el acceso,consulte con el administrador. Error: " + ex.Message);
            }
            finally
            {
                cnn.Close();
            }

        }

        public void removeDevice(string id_Device)
        {
            // Agrega el acceso a la BD
            SqlConnection cnn = new SqlConnection(conexion);
            SqlCommand cmd = new SqlCommand("BorrarDevices", cnn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@idHandHeld", id_Device);

            try
            {
                cnn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo borrar el dispositivo. Error: " + ex.Message);
            }
            finally
            {
                cnn.Close();
            }

        }

        public void removeReglaAcceso(string idRegla)
        {
            if (reglasAcceso.ContainsKey(idRegla))
            {
                reglasAcceso.Remove(idRegla);
                SaveReglasAccesos();
            }
        }


       
        public void addZone(string v_idZone, Zone v_Zone)
        {
            Dictionary<string, Zone> listaZonas = LoadZonas();

            if (listaZonas.ContainsKey(v_idZone))
            {
                listaZonas.Remove(v_idZone);
            }
            listaZonas.Add(v_idZone, v_Zone);

            SaveZonas(listaZonas);
        }

        public void deleteZone(string v_zone)
        {
            Dictionary<string, Zone> listaZonas = LoadZonas();
            if (listaZonas.ContainsKey(v_zone))
            {
                listaZonas.Remove(v_zone);
                SaveZonas(listaZonas);
            }
        }

        /// <summary>
        /// Agregado de una zona desde el driver LENEL. Se usa la tabla LNLVirtualZones para almacenar su ID, organizacion y nombre
        /// Esta tabla se usara cuando venga la definicion de los puntos de la zona.
        /// </summary>
        /// <param name="v_LNL_PanelID"></param>
        /// <param name="v_LNL_Panelname"></param>
        /// <param name="v_idOrganization"></param>
        public void addZoneFromLenel(int v_LNL_PanelID, string v_LNL_Panelname, int v_idOrganization)
        {

            bool yaExisteZona = false;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from LNLVirtualZones where LNLPanelID = " + v_LNL_PanelID.ToString() + " and OrgID=" + v_idOrganization.ToString();
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();


                if (lector.HasRows)
                {
                    yaExisteZona = true;
                }

                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addZoneFromLenel 1: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            if (yaExisteZona)            // Ya existe: hago Update de la zona por si cambio su nombre
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "update LNLVirtualZones set Name = '" + v_LNL_Panelname + "' where LNLPanelID = " + v_LNL_PanelID.ToString() + " and OrgID=" + v_idOrganization.ToString();
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en addZoneFromLenel 2: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
            else
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "insert into LNLVirtualZones(LNLPanelID,OrgID,Name) values (" + v_LNL_PanelID + "," + v_idOrganization + ",'" + v_LNL_Panelname + "')";
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en addZoneFromLenel 3: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
        }

        /// <summary>
        /// Da de alta en todas las tablas la informacion de la definicion de una Virtual Zone.
        /// Hace de todo: Define en Layers LENEL, si no esta. Se lo asigna a la Organizacion en Organizacionlayers
        /// crea la Feature asociandoel el layer LENEL y el LNLPanelID y por ultimo, la info geometrica en PuntorOrd y Gates.
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        public void  DefineNewZone(int v_LNL_PanelID, string v_listaPuntos,int  v_idOrganization)
        {

            string VName = obtenerVirtualZoneName(v_LNL_PanelID);
            if (VName == "")
            {
                loguearString("Virtual Zone del panel: " + v_LNL_PanelID + " NO DADA DE ALTA debido a que no tiene NOMBRE", TiposLOG.LENEL);
            }
            else
            {
                int IDLENELayer = obtenerLENELLayer();

                if (IDLENELayer < 0)
                {
                    loguearString("Virtual Zone del panel: " + v_LNL_PanelID + " NO DADA DE ALTA debido a que no existe LENELLAYER", TiposLOG.LENEL);

                }
                else
                {
                    AsignarLENELLayerAOrganizacion(IDLENELayer, v_idOrganization);

                    // Alta de la zona en el sistema ALUTRACK. Alli una Feature es una Zona....
                    bool yaExisteFeature = false;
                    int lastIDFeature = -1;

                    SqlConnection cnn = new SqlConnection(conexion);
                    try
                    {
                        cnn.Open();
                        SqlCommand cmd = cnn.CreateCommand();

                        cmd.CommandText = "select * from Features where LNLPanelID = " + v_LNL_PanelID.ToString() + " and OrgID = " + v_idOrganization.ToString();
                        cmd.CommandType = CommandType.Text;

                        SqlDataReader lector = cmd.ExecuteReader();

                        while (lector.Read())
                        {
                            lastIDFeature = int.Parse(lector["id"].ToString());
                            yaExisteFeature = true;
                            break;

                        }

                        lector.Close();
                    }
                    catch (Exception ex)
                    {
                        loguearString("Excepcion en DefineNewZone 1: " + ex.Message, TiposLOG.HH);
                    }
                    finally
                    {
                        cnn.Close();
                    }

                 
                    string FeaturesUpdateSQL = "";

                    if (yaExisteFeature)            // Ya existe: hago Update de la gate por si cambio su nombre
                    {
                        FeaturesUpdateSQL = "update Features set Nombre = '" + VName + "', idLayer = " + IDLENELayer + ", idTipoFeature=" + DEFAULT_TIPO_FEATURE.ToString() + ",Descripcion='Virtual Zone' where LNLPanelID = " + v_LNL_PanelID.ToString() + " and OrgID=" + v_idOrganization.ToString();
                    }
                    else
                    {
                        FeaturesUpdateSQL = "insert into Features(idLayer,idTipoFeature,Nombre,Descripcion,LNLPanelID,OrgID) output INSERTED.ID values (" + IDLENELayer.ToString() + "," + DEFAULT_TIPO_FEATURE.ToString() + ",'" + VName + "','Virtual Zone'," + v_LNL_PanelID.ToString() + "," + v_idOrganization.ToString() + ")";
                    }
                    try
                    {
                        cnn = new SqlConnection(conexion);

                        cnn.Open();
                        SqlCommand cmd = cnn.CreateCommand();

                        cmd.CommandText = FeaturesUpdateSQL;

                        SqlDataReader myReader = cmd.ExecuteReader(); // last inserted ID is recieved as any resultset on the first column of the first row
                        if (lastIDFeature == -1)
                        {
                            while (myReader.Read())
                            {
                                lastIDFeature = int.Parse(myReader[0].ToString());
                                break;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        loguearString("Excepcion en DefineNewZone 2: " + ex.Message, TiposLOG.HH);
                    }
                    finally
                    {
                        cnn.Close();
                    }
                    if (lastIDFeature > 0)
                    {

                        AddGeometricData(v_LNL_PanelID, v_listaPuntos, v_idOrganization, lastIDFeature);
                    }
   
                }
            }

        }

        private void AddGeometricData(int v_LNL_PanelID, string v_listaPuntos, int v_idOrganization, int v_idFeature)
        {

            BorrarDatosGeometricosAnteriores(v_idFeature);

            int ordinal = 1;
            

            string[] puntos = v_listaPuntos.Split(',');

            for (int index = 0; index < puntos.Length; index = index + 2)
            {
                string v_lat = puntos[index];
                string v_long = puntos[index+1];

                AgregarPuntoAPuntosORD(v_idFeature, ordinal, float.Parse(v_lat, CultureInfo.InvariantCulture.NumberFormat), float.Parse(v_long, CultureInfo.InvariantCulture.NumberFormat));

                if (ordinal > 1)
                {
                    AgregarGate(ordinal-1, ordinal, v_LNL_PanelID, v_idFeature);
                }
                ordinal++;
            }

            AgregarGate(ordinal - 1, 1, v_LNL_PanelID, v_idFeature);        // La ultima puerta conecta el ultimo punto con el primero.
        }

        /// <summary>
        /// Borra de las tablas de definicion de puntos y gates todos los datos correspondientes a la idFeature indicada. 
        /// </summary>
        /// <param name="v_idFeature"></param>
        private void BorrarDatosGeometricosAnteriores(int v_idFeature)
        {
            // Primero borro los puntos
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "delete from PuntosOrd where idFeature=" + v_idFeature;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en BorrarDatosGeometricosAnteriores 1 : " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            // Luego borro las Gates

            cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "delete from Gates where idFeature=" + v_idFeature;
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en BorrarDatosGeometricosAnteriores 2: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
        }

        private void AgregarGate(int ordinalDesde, int ordinalHacia, int v_LNL_PanelID, int v_idFeature)
        {
            string nombreZona = obtenerVirtualZoneName(v_LNL_PanelID);


            string NombreGate = nombreZona + "_" + ordinalDesde.ToString();
            string tipoAcceso = GateAccessType.Forbidden.ToString();

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "insert into Gates(Nombre,Punto1,Punto2,idFeature,Acceso) values('" + NombreGate + "'," + ordinalDesde.ToString() + "," + ordinalHacia.ToString() + "," + v_idFeature.ToString() + ",'" + tipoAcceso +"')";
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en AgregarGate: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

        }
        /// <summary>
        /// Agrega un punto a la tabla de Puntos por ordinal. 
        /// Si ya existe un punto con ese ordinal para esa Zona, hace un update de los datos geometricos.
        /// NOTA: No se realzia la conversion a UTM.(TODO...). Se les pone 1,1 a estos dos valores
        /// </summary>
        /// <param name="v_Feature"></param>
        /// <param name="v_ordinal"></param>
        /// <param name="v_lat"></param>
        /// <param name="v_long"></param>
        private void AgregarPuntoAPuntosORD(int v_Feature, int v_ordinal, float v_lat, float v_long)
        {

            bool yaExistePunto = false;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from PuntosOrd where idFeature = " + v_Feature + " and ordinal=" + v_ordinal;
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();

                if (lector.HasRows)
                {
                    yaExistePunto = true;
                }

                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en AgregarPuntoAPuntosORD 1: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            if (yaExistePunto)            // Ya existe: hago Update de la zona por si cambio su nombre
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = "update PuntosOrd set Latitud=" + v_lat.ToString(CultureInfo.InvariantCulture.NumberFormat) + ",Longitud=" + v_long.ToString(CultureInfo.InvariantCulture.NumberFormat) + " where idFeature = " + v_Feature + " and ordinal=" + v_ordinal; 
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en AgregarPuntoAPuntosORD:2 " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }

            }
            else
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();
                    cmd.CommandText = "insert into PuntosOrd(idFeature,Ordinal,Latitud,Longitud,XUtm,YUtm) values(" + v_Feature.ToString() + "," + v_ordinal.ToString() + "," + v_lat.ToString(CultureInfo.InvariantCulture.NumberFormat) + "," + v_long.ToString(CultureInfo.InvariantCulture.NumberFormat) + ",1,1)";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en AgregarPuntoAPuntosORD: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
        }

        private string obtenerVirtualZoneName(int v_LNL_PanelID)
        {
            string res = "";

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from lnlvirtualzones where LNLPanelID =" + v_LNL_PanelID.ToString();
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    res = lector["Name"].ToString();
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerVirtualZoneName: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            return res;
        }

        private int obtenerLENELLayer()
        {
            int res = -1;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Layers where Nombre ='" + LENEL_LAYER+"'";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    res = int.Parse(lector["id"].ToString());
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerLENELLayer: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            if ( res <0)
            {
                res = addLENELLayer();
            }

            return res;
        }


        private int addLENELLayer()
        {
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "insert into Layers(Nombre,Descripcion,Modificable) values ('" + LENEL_LAYER + "','VirtualZoneLayer',0)";
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addLENELLayer: " + ex.Message, TiposLOG.HH);
                return -1;
            }
            finally
            {
                cnn.Close();
            }

            return obtenerLENELLayer();

        }

        private void AsignarLENELLayerAOrganizacion(int v_IDLenelayer, int v_idOrganization)
        {
            if (!ExisteLenelLayerEnOrganizacion(v_IDLenelayer, v_idOrganization))
            {
                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "insert into OrganizacionLayer(idOrganizacion,idLayer) values (" + v_idOrganization + "," + v_IDLenelayer + ")";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en AsignarLENELLayerAOrganizacion: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
        }

        private bool ExisteLenelLayerEnOrganizacion(int v_IDLenelayer, int v_idOrganization)
        {
            bool yaExiste = false;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from organizacionlayer where idOrganizacion= " + v_idOrganization.ToString() + " and idLayer = " + v_IDLenelayer.ToString();
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();

                if (lector.HasRows)
                {
                    yaExiste = true;
                }

                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en ExisteLenelLayerEnOrganizacion 1: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            return yaExiste;
        }
        
        /// <summary>
        /// Definicion de una gate desde AlarmMonitoring. Paso final en la asignacion de una VirtualGate a un Reader de LENEL
        /// 
        /// </summary>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        /// <param name="?"></param>
        public void defineGate(int v_LNL_PanelID,int v_LNL_ReaderID,int v_idOrganization,string v_accessType,int v_Punto1,int v_Punto2)
        {
            string actualLenelReaderName = obtenerLENELVirtualGateName(v_LNL_PanelID, v_LNL_ReaderID);

            if (actualLenelReaderName != "")
            {
                int idFeature = obtenerFeature(v_LNL_PanelID, v_idOrganization);

                if (idFeature > 0)
                {
                    // Tengo el Punto1, Punto2, la Feature, el Nombre de la gate en LENEL y el tipo de acceso.

                    actualizarGate(v_LNL_PanelID, v_LNL_ReaderID, idFeature, v_Punto1, v_Punto2, actualLenelReaderName, v_accessType);
                }
                else
                {
                    loguearString("ERROR en defineGate(): No se pudo encontrar la Feature asociada al LENEL Panel de la Zona que contiene la puerta a modificar", TiposLOG.LENEL);
                }
            }
            else
            {
                loguearString("ERROR en defineGate(): NO Se pudo definir la Gate por no encontrarse en la base de puertas Virtuales creadas desde SysAdm", TiposLOG.LENEL);
            }

        }

        // Obtiene el nombre REAL de la Virtual Gate definido en LENEL
        private string obtenerLENELVirtualGateName(int v_LNL_PanelID, int v_LNL_ReaderID)
        {
            string res = "";

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from LNLVirtualGates where LNLPanelID= " + v_LNL_PanelID.ToString() + " and LNLReaderID = " + v_LNL_ReaderID.ToString();
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();
                while(lector.Read())
                {
                    res = lector["Name"].ToString();
                }

                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerLENELVirtualGateName: " + ex.Message, TiposLOG.LENEL);
            }
            finally
            {
                cnn.Close();
            }
            return res;
        }


        // Paso final: Actualizar los datos de la Gate en la tabla de Gates del sistema.
        // Se actualiza el AccessType y el NOMBRE para que quede como en el LENEL
        private void actualizarGate(int v_LNL_PanelID,int  v_LNL_ReaderID, int idFeature, int v_Punto1, int v_Punto2, string actualLenelReaderName, string v_accessType)
        {

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "update Gates set  Nombre = '" + actualLenelReaderName + "', LNLPanelID=" +  v_LNL_PanelID.ToString() + ",LNLReaderID=" +v_LNL_ReaderID + ", Acceso='" + v_accessType+"'" + " where idFeature = " + idFeature.ToString() +" and Punto1=" + v_Punto1.ToString() + " and Punto2=" + v_Punto2.ToString();
                cmd.CommandType = CommandType.Text;

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en actualizarGate: " + ex.Message, TiposLOG.LENEL);
            }
            finally
            {
                cnn.Close();
            }
        }
        /// <summary>
        /// Agregado de una VirtualGate desde el driver LENEL. Se usa la tabla LNLVirtualGates para almacenar su PanelID, ReaderID, organizacion y nombre
        /// Esta tabla se usara cuando venga la definicion de la puerta en la Zona.
        /// </summary>
        /// <param name="v_LNL_PanelID"></param>
        /// <param name="v_LNL_ReaderID"></param>
        /// <param name="v_LNL_ReaderName"></param>
        /// <param name="v_idOrganization"></param>
        public void addVirtualGateFromLenel(int v_LNL_PanelID,  int v_LNL_ReaderID, string v_LNL_ReaderName, int v_idOrganization)
        {

            bool yaExisteGate = false;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from LNLVirtualGates where LNLPanelID = " + v_LNL_PanelID.ToString() + " and LNLReaderID = " + v_LNL_ReaderID.ToString() + " and OrgID=" + v_idOrganization.ToString();
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();


                if (lector.HasRows)
                {
                    yaExisteGate = true;
                }

                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addVirtualGateFromLenel 1: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            if (yaExisteGate)            // Ya existe: hago Update de la gate por si cambio su nombre
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "update LNLVirtualGates set Name = '" + v_LNL_ReaderName + "' where LNLPanelID = " + v_LNL_PanelID.ToString() + " and LNLReaderID = " + v_LNL_ReaderID.ToString() + " and OrgID=" + v_idOrganization.ToString();
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en addVirtualGateFromLenel 2: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
            else
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "insert into LNLVirtualGates(LNLPanelID,LNLReaderID,Name,OrgID) values (" + v_LNL_PanelID.ToString() +"," + v_LNL_ReaderID.ToString()+ ",'"+ v_LNL_ReaderName + "',"+ v_idOrganization.ToString()+")";
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en addVirtualGateFromLenel 3: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
        }



        public string obtenerDatosZona(int LNL_PanelID,int orgID)
        {
            string res = "";
            int idFeature = obtenerFeature(LNL_PanelID, orgID);
            if (idFeature > 0)
            {
                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "select * from Gates where idFeature = " + idFeature + " order by Punto1";
                    cmd.CommandType = CommandType.Text;

                    SqlDataReader lector = cmd.ExecuteReader();

                    while (lector.Read())
                    {
                        string strP1 = lector["Punto1"].ToString();
                        string strP2 = lector["Punto2"].ToString();
                        string tipoAcceso = lector["Acceso"].ToString();
                        string LNLReaderID= lector["LNLReaderID"].ToString();

                        string datosPuerta = ArmarDatosPuerta(strP1, strP2, tipoAcceso, idFeature, LNLReaderID);
                        res = res + datosPuerta;

                    }

                    lector.Close();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en agregarReaderDesdeLenel 1: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
            else
            {
                loguearString("Feature correspondiente al panel: " + LNL_PanelID + " no encontrada",TiposLOG.HH);
            }


            if (res.Length > 0)
            {
                res = res.Substring(0, res.Length - 1);            // Le saco la coma final
            }
            return res;
        }

        private int obtenerFeature(int LNLPanelID, int orgID)
        {
            int res = -1;
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Features where LNLPanelID = " + LNLPanelID.ToString() + " and OrgID=" + orgID.ToString();
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    res = int.Parse(lector["id"].ToString());
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en agregarReaderDesdeLenel 1: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            return res;
        }

        /// <summary>
        /// Datos de una puerta: Lat1,Long1,ORD1,Lat2,Long2,ORD2,TIPOACCESO
        /// </summary>
        /// <param name="v_strP1"></param>
        /// <param name="v_strP2"></param>
        /// <param name="v_tipoAcceso"></param>
        /// <param name="idFeature"></param>
        /// <returns></returns>
        private string ArmarDatosPuerta(string v_strP1,string v_strP2,string v_tipoAcceso, int idFeature, string v_LNLReaderID)
        {

            string res = "";
            int ordinalP1 = int.Parse(v_strP1);
            int ordinalP2 = int.Parse(v_strP2);

            string posP1 = obtenerLatLong(ordinalP1,idFeature);
            string posP2 = obtenerLatLong(ordinalP2, idFeature);



            res = posP1 + "," + v_strP1 + "," + posP2 + "," + v_strP2 + "," + v_tipoAcceso + "," + v_LNLReaderID + ",";


            return res;
        }


        private string obtenerLatLong(int ordinalPunto, int idFeature)
        {
            string res = "";
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from PuntosOrd where idFeature = " + idFeature.ToString() + " and Ordinal=" + ordinalPunto.ToString();
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    double flLat = (double)lector["Latitud"];
                    double flLong = (double)lector["Longitud"];
                    
                    res = flLat.ToString(CultureInfo.InvariantCulture.NumberFormat) + "," + flLong.ToString(CultureInfo.InvariantCulture.NumberFormat);
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerLatLong: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
            return res;
        }

        /// <summary>
        /// Agrega un empleado nuevo y devuelve su id. si ya existe, devuelve el id que ya tiene.
        /// 
        /// Versión nueva: la DB tiene que tener dos campos nuevos (PersonID y Version)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="lastName"></param>
        /// <param name="empresa"></param>
        /// <param name="SSNO"></param>
        /// <param name="imageFileName"></param>
        /// <param name="personID">ID único que identifica a cada empleado</param>
        /// <returns></returns>
        public int addEmpleadoDesdeLenel(string name, string lastName, string empresa, string SSNO, string imageFileName, int v_orgID, string personID, string v_isDownloadingDB, string lastChanged)
        {
            int idEmpleado = -1;

            int versionEmp = 0;

            SqlConnection cnn = new SqlConnection(conexion);

            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Empleados where PersonID='" + personID + "'";
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();

                if (lector.Read())
                {
                    idEmpleado = Convert.ToInt32(lector["idEmpleado"].ToString());
                    versionEmp = Convert.ToInt32(lector["Version"].ToString());

                    // Verifico que no haya cambiado la version del empleado
                    if (!lector["nombre"].ToString().Equals(name) ||
                        !lector["apellido"].ToString().Equals(lastName) ||
                        !lector["Empresa"].ToString().Equals(empresa) ||
                        !lector["NumeroDocumento"].ToString().Equals(SSNO) ||
                        !lector["idOrganizacion"].ToString().Equals(v_orgID.ToString())                        
                        )
                    {
                        versionEmp++;
                        lastChanged = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                   
                }

                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addEmpleadoDesdeLenel: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            if (idEmpleado != -1)            // El empleado existe. Hacer Update del Empleado.
            {

                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();
                    cmd = cnn.CreateCommand();

                    cmd.CommandText = "update  Empleados set nombre = '" + name + "', apellido='" + lastName + "',Empresa='" + empresa + "',NumeroDocumento='" + SSNO + "',Imagen='" + imageFileName + "',idOrganizacion=" + v_orgID + ",Version='" + versionEmp.ToString() + "',isDownloadingDB = " + v_isDownloadingDB + ",ultimaActualizacion='"+lastChanged+"' where idEmpleado=" + idEmpleado;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en el update de addEmpleadoDesdeLenel: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }

                try
                {
                    idEmpleado = buscarEmpleadoxPersonIDenBD(personID).Id;
                }
                catch (Exception )
                {
                    idEmpleado = -1;
                }
            }
            else                               // Si no existe el empleado, lo doy de alta y devuelvo el id que se le asigna.
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();
                    cmd = cnn.CreateCommand();

                    cmd.CommandText = "insert into Empleados(IdImagen,Nombre,Apellido,Empresa, NumeroDocumento,imagen,Email,idPais,idCiudad,FechaNacimiento,Telefono,Celular,TipoDocumento,FechaExpedicionDocumento,FechaVencimientoDocumento,idOrganizacion,PersonID,Version,isDownloadingDB,ultimaActualizacion)values(1,'" + name + "','" + lastName + "','" + empresa + "','" + SSNO + "','" + imageFileName + "','','','','','','','','',''" + ",'" + v_orgID.ToString() + "','" + personID + "','" + versionEmp.ToString() + "','" + v_isDownloadingDB + "','" + lastChanged + "')";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();


                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en insert de addEmpleadoDesdeLenel: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }

                idEmpleado = buscarEmpleadoxPersonIDenBD(personID).Id;

            }

            // PASO FINAL: Actualiza la coleccion en RAM
            agregarOActualizarEmpleadoEnRAM(idEmpleado, name, lastName, empresa, SSNO,int.Parse(personID), imageFileName, versionEmp, lastChanged);

            return idEmpleado;
        }


        private void agregarOActualizarEmpleadoEnRAM(int idEmpleado, string name, string lastName, string empresa, string SSNO, int personID,string imageFileName, int versionEmp, string lastChanged)
        {

            try
            {
                //MUTEX
                StaticTools.obtenerMutex_ListaEmpleados();

                if (!listaEmpleados.ContainsKey(idEmpleado))
                {
                    listaEmpleados.Add(idEmpleado, new Employee());
                }
                listaEmpleados[idEmpleado].Id = idEmpleado;
                listaEmpleados[idEmpleado].Nombre = name;
                listaEmpleados[idEmpleado].Apellido = lastName;
                listaEmpleados[idEmpleado].Empresa = empresa;
                listaEmpleados[idEmpleado].NumeroDocumento = SSNO;
                listaEmpleados[idEmpleado].PersonID = personID;
                listaEmpleados[idEmpleado].Imagen = imageFileName;
                listaEmpleados[idEmpleado].VersionEmpleado = versionEmp;

                Regex rFechaHora = new Regex(@"(.*)-(.*)-(.*) (.*):(.*):(.*)");
                Match mFechaHora = rFechaHora.Match(lastChanged);

                if (mFechaHora.Success)
                {
                    DateTime d = new DateTime(Convert.ToInt32(mFechaHora.Groups[1].Value), Convert.ToInt32(mFechaHora.Groups[2].Value), Convert.ToInt32(mFechaHora.Groups[3].Value), Convert.ToInt32(mFechaHora.Groups[4].Value), Convert.ToInt32(mFechaHora.Groups[5].Value), Convert.ToInt32(mFechaHora.Groups[6].Value));
                    listaEmpleados[idEmpleado].ultimaActualizacion = d;
                }
                else { loguearString("No paso la expresion regular de la hora. agregarOActualizarEmpleadoEnRAM", TiposLOG.HH); }
                
                // Actualizacion de la imagen
                byte[] imageBytes = null;
                string fileName = Path.GetFileName(imageFileName);
                if ((fileName != "") && (fileName != "sin_img.jpg"))
                {
                    string imagePath = SystemConfiguration.ImagesPath;
                    imageBytes = File.ReadAllBytes(imagePath + @"/" + fileName);
                    
                }
                else
                {
                    imageBytes = null;
                    listaEmpleados[idEmpleado].Imagen="";
                }
                listaEmpleados[idEmpleado].imageDataBytes = imageBytes;

            }
            catch (Exception ex)
            {
                loguearString("Excepcion en agregarOActualizarEmpleadoEnRAM: " + ex.Message,TiposLOG.HH);
            }
            finally
            {
                StaticTools.liberarMutex_ListaEmpleados();
            }
        }

        // Borra completamente todos los datos de un empleado, incluida su tarjeta.
        public void borrarEmpleado(string v_tarjeta)
        {

            int idEmpleado = -1;

            try
            {
                idEmpleado = buscarEmpleadoxTarjeta(v_tarjeta).Id;
            }
            catch (Exception )
            {
                idEmpleado = -1;
            }

            if (idEmpleado > 0)
            {

                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    // Borro el Empleado
                    cmd.CommandText = "delete from Empleados where idEmpleado =" + idEmpleado.ToString();
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();

                    // Borro em Empleado de la lista en RAM
                    //MUTEX
                    try
                    {
                        StaticTools.obtenerMutex_ListaEmpleados();
                        if (listaEmpleados.ContainsKey(idEmpleado))
                        {
                            listaEmpleados.Remove(idEmpleado);
                        }
                    }
                    catch (Exception ex)
                    {
                        loguearString("Excepcion en  borrarEmpleado-MUTEX " + ex.Message, TiposLOG.HH);
                    }
                    finally
                    {
                        StaticTools.liberarMutex_ListaEmpleados();
                    }
                    
                    // Ahora borro su tarjeta
                    cmd.CommandText = "delete from Tarjetas where Tarjeta = '" + v_tarjeta + "'";
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();

                    // Borro la tarjeta de la lista en RAM
                    try
                    {
                        StaticTools.obtenerMutex_ListaTarjetas();
                        if (listaTarjetas.ContainsKey(v_tarjeta))
                        {
                            listaTarjetas.Remove(v_tarjeta);
                        }
                    }
                    catch (Exception ex)
                    {
                        loguearString("Excepcion en borrarEmpleado-MutexListaTarjetas" + ex.Message, TiposLOG.HH);
                    }
                    finally
                    {
                        StaticTools.liberarMutex_ListaTarjetas();
                    }

                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en borrarEmpleado: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }

            }

        }


        /// <summary>
        /// Actualiza la fecha de actualizacion del HH en la base
        /// </summary>
        /// <param name="v_HHID"></param>
        public void actualizarFechaSync(string v_HHID)
        {
            int orgID = obtenerOrganizationIDFromHHID(v_HHID);
            string fechaActualizacion = obtenerUltimaFechaActualizacionEmpleados();
            //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (orgID > 0)
            {
                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                  cnn.Open();
                  SqlCommand cmd = cnn.CreateCommand();

                  // Borro el dato actual
                  cmd.CommandText = "delete from HandHeldSync where idOrg = " + orgID.ToString() + " and HandHeld = '" + v_HHID + "'";
                  cmd.CommandType = CommandType.Text;

                  cmd.ExecuteNonQuery();

                  // Agrego el dato nuevo
                  cmd.CommandText = "insert into HandHeldSync(idOrg,HandHeld,FechaSync) values(" + orgID.ToString() + ",'" + v_HHID + "','" + fechaActualizacion + "')";
                  cmd.CommandType = CommandType.Text;

                  cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                  loguearString("Excepcion en actualizarFechaSync del HHID: " + v_HHID + "-"+ex.Message, TiposLOG.HH);
                }
                finally
                {
                  cnn.Close();
                }
            }
        }

        private string obtenerUltimaFechaActualizacionEmpleados()
        {
            string res = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");   

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                // Borro el dato actual
                cmd.CommandText = "select MAX(cast(ultimaactualizacion as DateTime)) from empleados";
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    DateTime fecha = (DateTime)lector[0];
                    res = fecha.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en obtenerUltimaFechaActualizacionEmpleados -" + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            return res;

        }

        public DateTime obtenerUltimaFechaSync(string v_HHID)
        {
            DateTime res = new DateTime(2010,1,1,0,0,0);        // Fecha a devolver si no hay o null

            int orgID = obtenerOrganizationIDFromHHID(v_HHID);
            if (orgID > 0)
            {
                SqlConnection cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "select * from HandHeldSync where idOrg = " + orgID + " and HandHeld='" + v_HHID + "'";
                    cmd.CommandType = CommandType.Text;
                    SqlDataReader lector = cmd.ExecuteReader();
                    string ultimaActualizacion;
                    Regex rHora = new Regex(@"(.*)-(.*)-(.*) (.*):(.*):(.*)");
                    while (lector.Read())
                    {
                        ultimaActualizacion = lector["FechaSync"].ToString();

                        Match mHora = rHora.Match(ultimaActualizacion);

                        if (mHora.Success)
                        {
                            DateTime d = new DateTime(Convert.ToInt32(mHora.Groups[1].Value), Convert.ToInt32(mHora.Groups[2].Value), Convert.ToInt32(mHora.Groups[3].Value), Convert.ToInt32(mHora.Groups[4].Value), Convert.ToInt32(mHora.Groups[5].Value), Convert.ToInt32(mHora.Groups[6].Value));
                            res = d;
                        }
                        else { loguearString("No paso la expresion regular de la hora. obtenerUltimaFechaSync", TiposLOG.HH); }
                    }
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en obtnerUltimaFechaSync: " + ex.Message,TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }

            }


            return res;
        }
        /// <summary>
        /// Obtiene el ID del lector de Entrada de un Panel, o -1 si no esta definido
        /// </summary>
        /// <param name="v_LNLPanelID"></param>
        /// <returns></returns>
        public int ObtenerEntranceReaderID(int v_LNLPanelID)
        {
            
            int res = -1;

            try
            {
                string HHID = ObtenerHHID(v_LNLPanelID.ToString());

                string strLNLReaderID = obtenerLenelReaderIDEntrada(HHID);

                if (strLNLReaderID != "")
                {
                    res = int.Parse(strLNLReaderID);
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en ObtenerEntranceReaderID(" + v_LNLPanelID.ToString() + ") -" + ex.Message, TiposLOG.LENEL);
            }

            return res;

        }

        /// <summary>
        /// Obtiene el ID del lector de Salida de un Panel, o -1 si no esta definido
        /// </summary>
        /// <param name="v_LNLPanelID"></param>
        /// <returns></returns>
        public int ObtenerExitReaderID(int v_LNLPanelID)
        {
            int res = -1;
            try
            {
                string HHID = ObtenerHHID(v_LNLPanelID.ToString());

                string strLNLReaderID = obtenerLenelReaderIDSalida(HHID);

                if (strLNLReaderID != "")
                {
                    res = int.Parse(strLNLReaderID);
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en ObtenerExitReaderID(" + v_LNLPanelID.ToString() + ") -" + ex.Message, TiposLOG.LENEL);
            }

            return res;
        }

        public void asociarEmpleadoATarjeta(string v_tarjeta, int v_empID, int v_orgID)
        {
            bool yaExisteTarjeta = false;
            
            bool cambioVersion = false;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Tarjetas where Tarjeta = '" + v_tarjeta + "'";
                cmd.CommandType = CommandType.Text;

                SqlDataReader lector = cmd.ExecuteReader();
                
                if (lector.Read())
                {
                    if (!lector["idEmpleado"].ToString().Equals(v_empID)) cambioVersion = true;
                    yaExisteTarjeta = true;
                }

                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en asociarEmpleadoATarjeta 1: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            if (yaExisteTarjeta)            // Ya existe: hago Update del empleado
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "update Tarjetas set idEmpleado = " + v_empID + " where Tarjeta ='" + v_tarjeta + "'";
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();

                    if (cambioVersion)
                    {
                        string version = "0";
                        // Obtengo la version y la aumento
                        cmd.CommandText = "select Version from Empleados where idEmpleado = '" + v_empID + "'";
                        cmd.CommandType = CommandType.Text;
                        
                        SqlDataReader rdr = cmd.ExecuteReader();
                        
                        if (rdr.Read()) version = rdr[0].ToString();
                        
                        rdr.Close();

                        cmd.CommandText = "update Empleados set Version = " + version.ToString() + " where idEmpleado ='" + v_empID + "'";
                        cmd.CommandType = CommandType.Text;

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en asociarEmpleadoATarjeta 2: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }
            else                           // No existe la Tarjeta: Creo una nueva en la base y en RAM
            {
                cnn = new SqlConnection(conexion);
                try
                {
                    cnn.Open();
                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "insert into Tarjetas(Tarjeta,Estado,idEmpleado,idOrganizacion) values ('" + v_tarjeta + "',1," + v_empID + "," + v_orgID +")";
                    cmd.CommandType = CommandType.Text;

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    loguearString("Excepcion en asociarEmpleadoATarjeta 2: " + ex.Message, TiposLOG.HH);
                }
                finally
                {
                    cnn.Close();
                }
            }

            // Carga el idTarjeta generado 
            string idTarjeta = string.Empty;
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select idTarjeta from Tarjetas where idEmpleado='" + v_empID + "'";
                cmd.CommandType = CommandType.Text;


                SqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                idTarjeta = rdr["idTarjeta"].ToString();
            }
            catch (Exception ex) { loguearString("Excepción en asociarEmpleadoATarjeta: " + ex.Message, TiposLOG.HH); }
            finally { cnn.Close(); }

            // PASO FINAL: Actualizar la ListaTarjetas en RAM
            agregarOActualizarTarjetaEnRAM(v_tarjeta, v_empID, idTarjeta);

        }


        private void agregarOActualizarTarjetaEnRAM(string v_tarjeta, int v_empID, string idTarjeta)
        {

            try
            {
                StaticTools.obtenerMutex_ListaTarjetas();

                if (!listaTarjetas.ContainsKey(v_tarjeta))
                {
                    listaTarjetas.Add(v_tarjeta, new Tarjeta());
                }
                listaTarjetas[v_tarjeta].numerodetarjeta = v_tarjeta;
                listaTarjetas[v_tarjeta].idEmpleado = v_empID;
                listaTarjetas[v_tarjeta].estado = true;
                listaTarjetas[v_tarjeta].idTarjeta = int.Parse(idTarjeta);
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en agregarOActualizarTarjetaEnRAM: " + ex.Message,TiposLOG.HH);
            }
            finally
            {
                StaticTools.liberarMutex_ListaTarjetas();
            }
        }





        public void addAccesoZona(ZoneAccess v_acceso)
        {
            SqlConnection cnn = new SqlConnection(conexion);
            SqlCommand cmd = new SqlCommand("AgregarAccesoaZonas", cnn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@idHandheld", v_acceso.HHID);
            cmd.Parameters.AddWithValue("@idZona", v_acceso.ZoneID);
            cmd.Parameters.AddWithValue("@idGate", v_acceso.GateID);
            cmd.Parameters.AddWithValue("@Hora", v_acceso.Hora);
            cmd.Parameters.AddWithValue("@Tipo", v_acceso.tipoAcceso);

            try
            {
                cnn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo agregar el acceso a zona: " + ex.Message);
            }
            finally
            {
                cnn.Close();
            }
        }

        public List<ZoneAccess> getAccesosZonas()
        {
            return mainApp.DataManager.LoadAccesosAZonas();
        }
        public Employee buscarEmpleadoxPersonIDenRAM(string strPID)
        {
            Employee res = null;

            try
            {
                int pID = int.Parse(strPID);

                Dictionary<int, Employee> listaEmp = getListaEmpleados();

                foreach (KeyValuePair<int, Employee> emp in listaEmp)
                {
                    if (emp.Value.PersonID == pID)
                    {
                        res = emp.Value;
                        break;
                    }

                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en buscarEmpleadoxPersonIDenRAM: " + ex.Message, TiposLOG.ALUTRACK);

            }

            return res;

        }

        internal Employee buscarEmpleadoxPersonIDenBD(string p)
        {
            Employee res = null;

            SqlConnection cnn = new SqlConnection(conexion);
            cnn.Open();
            SqlCommand cmd = cnn.CreateCommand();

            cmd.CommandText = "select * from Empleados where PersonID = '" + p + "'";
            cmd.CommandType = CommandType.Text;
            try
            {
                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    res = new Employee();

                    res.Id = Convert.ToInt32(rdr["idEmpleado"].ToString());
                    res.Nombre = rdr["Nombre"].ToString();
                    res.Apellido = rdr["Apellido"].ToString();

                    res.NumeroDocumento = rdr["NumeroDocumento"].ToString();

                    res.Empresa = rdr["Empresa"].ToString();

                    res.Imagen = rdr["Imagen"].ToString();
                    res.imageVersion = Convert.ToInt32(rdr["IdImagen"].ToString());

                    res.VersionEmpleado = Convert.ToInt32(rdr["Version"].ToString());
                    res.PersonID = Convert.ToInt32(rdr["PersonID"].ToString());                    

                }
                else loguearString("Error cargando empleado x PersonID: No existe empleado con el PersonID = " + p, TiposLOG.HH);
            }
            catch (Exception e) 
            { 
                loguearString("Error cargando empleado x PersonID(" + p + "): " + e.Message, TiposLOG.HH);
                res = null; 
            }
            finally
            {
                    cnn.Close();
            }

            return res;
        }

        public Employee buscarEmpleadoxTarjeta(string tarjeta)
        {
            Employee emp = null;
            int idEmp = 0;

            Dictionary<int, Employee> listaEmpleados = mainApp.DataManager.getListaEmpleados();
            Dictionary<string, Tarjeta> listaTarjetas = mainApp.DataManager.getListaTarjetas();

            foreach (KeyValuePair<string, Tarjeta> pair in listaTarjetas)
            {
                if (pair.Value.NUMERODETARJETA == tarjeta)
                {
                    idEmp = pair.Value.IDEMPLEADO;
                    break;
                }
            }

            if (listaEmpleados.ContainsKey(idEmp))
            {
                emp = listaEmpleados[idEmp];
            }
            return emp;
        }


        public Tarjeta buscarTarjetadeEmpleado(int v_empID)
        {

            Tarjeta res = null;

            try
            {
                //MUTEX
                StaticTools.obtenerMutex_ListaTarjetas();

                Dictionary<string, Tarjeta> listaTarjetas = mainApp.DataManager.getListaTarjetas();

                foreach (KeyValuePair<string, Tarjeta> pair in listaTarjetas)
                {
                    if (pair.Value.IDEMPLEADO == v_empID)
                    {
                        res = pair.Value;
                        break;
                    }
                }
            }
            catch (Exception es)
            {
                loguearString("Excepcion en buscarTarjetadeEmpleado: " + es.Message,TiposLOG.HH);
            }
            finally
            {
                //MUTEX
                StaticTools.liberarMutex_ListaTarjetas();
            }

           

            return res;
        }

        /// <summary>
        /// addLenelBadgeAccessLevels: Actualiza desde cero la lista de idAccessLevels asociados a una tarjeta.
        /// </summary>
        /// <param name="v_OrgID"></param>
        /// <param name="v_badge"></param>
        /// <param name="v_accessLevels"></param>
        public void updateLenelBadgeAccessLevels(int v_OrgID, string v_badge, string v_accessLevels)
        {
         

            SqlConnection cnn = new SqlConnection(conexion);
            string horaActualizacion = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                StaticTools.obtenerMutex_ListaTarjetas();

                if (listaTarjetas.ContainsKey(v_badge))
                {
                    // Solo actualiza la ultima actulizacion si hubo un cambio en los accesslevels de la tarjeta.
                    if (listaTarjetas[v_badge].accessLevels.Trim() == v_accessLevels.Trim())
                    {
                        horaActualizacion = listaTarjetas[v_badge].ultimaActualizacion.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                    {
                        listaTarjetas[v_badge].ultimaActualizacion = DateTime.Now;// Actualiza la RAM
                    }

                    listaTarjetas[v_badge].accessLevels = v_accessLevels;
                  
                    cnn.Open();

                    SqlCommand cmd = cnn.CreateCommand();

                    cmd.CommandText = "update Tarjetas set accesslevels='" + v_accessLevels + "',ultimaActualizacion='" + horaActualizacion + "' where Tarjeta ='" + v_badge + "' and idOrganizacion =" + v_OrgID.ToString();
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    loguearString("ATENCION: Se trato de actualizar un accessLevel de una tarjeta no dada de alta: " + v_badge, TiposLOG.HH);
                }

            }
            catch (Exception ex)
            {
                loguearString("Excepcion en updateLenelBadgeAccessLevels: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                StaticTools.liberarMutex_ListaTarjetas();
                cnn.Close();
            }
        }

        // Arma un string con todos los PersonID de la lista de empleados que tienen que estar en el HH
        // Usado en el chequeo de SYN final
        public string loadAllPersonID(string HHID)
        {
            string res = "";

            try
            {
                StaticTools.obtenerMutex_ListaEmpleados();

                Dictionary<int, Employee> listaEmpleados = mainApp.DataManager.LoadEmpleados();

                foreach (KeyValuePair<int,Employee> emp in listaEmpleados)
                {

                    Tarjeta tarjeta = mainApp.DataManager.buscarTarjetadeEmpleado(emp.Value.Id);


                    // Solo se envia la info de un empleado si tiene tarjeta asociada y algun accesslevel asociado
                    if ((tarjeta != null))
                    {
                        string accessLevels = AccessLevelLogic.getAccessLevelsByBadgeInHH(tarjeta.NUMERODETARJETA, HHID);
                        DateTime ultimaActualizacionTarjeta = tarjeta.ultimaActualizacion;

                        if ((accessLevels.Trim().Length > 0)) 
                        {

                            // Envío los empleados con sus versiones para que el hh pida los datos SOLO de aquellos que cambiaron
                            res += emp.Value.PersonID.ToString() + ",";

                        }
                    }
                }
                if (res.Length > 0)
                {
                    res = res.Substring(0,res.Length-1);
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en loadAllPersonID del panel: " + HHID + " - " + ex.Message,TiposLOG.HH);
            }
            finally
            {
                StaticTools.liberarMutex_ListaEmpleados();
            }
            return res;
        }

        
        /// <summary>
        /// addLenelTimeZone: Graba la definicion completa de una TimeZone(TZNum) a la base, para lo cual borra previamente la definicion anterior
        /// </summary>
        /// <param name="v_LNLTimeZones"></param>
        /// <param name="v_OrgID"></param>
        /// <param name="v_TZNum"></param>
        public void addLenelTimeZone(LENELTimeZones v_LNLTimeZones, int v_OrgID, int v_TZNum)
        {
          
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "delete from TimeZones where idOrganizacion =" + v_OrgID.ToString() + " and TZNum = " + v_TZNum.ToString();

                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();

                TimeZoneData TZData = v_LNLTimeZones.getTimeZoneData();

                if (TZData != null)
                {
                    if (TZData.TZDefinition.ContainsKey(v_TZNum))
                    {
                        List<TZInterval> ReaderTZDef = TZData.TZDefinition[v_TZNum];
                        foreach (TZInterval intervalo in ReaderTZDef)
                        {
                            string rangoini = intervalo.horaIni.ToString() + ":" + intervalo.minIni.ToString();
                            string rangoFin = intervalo.horaFin.ToString() + ":" + intervalo.minFin.ToString();

                            cmd = cnn.CreateCommand();

                            cmd.CommandText = "insert into TimeZones (idOrganizacion,TZNum,rangoIni,rangoFin,DOW,HOL) values (" + v_OrgID.ToString() + "," + v_TZNum.ToString() + ",'" + rangoini + "','" + rangoFin + "'," + intervalo.DOW.ToString() + "," + intervalo.HOL.ToString() + ")";
                            cmd.CommandType = CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addLenelTimeZone: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
        }

        /// <summary>
        /// Holidays de la organizacion. En LENELHolidays viene la definicion completa de las holidays, por lo que 
        /// lo primero que se hace es borrar todas las holidays para luego darlas nuevamente de alta.
        /// </summary>
        /// <param name="v_idOrganization"></param>
        /// <param name="v_LENELHolidays"></param>
        public void addLenelHoliday(int v_idOrganization, List<HolidayData> v_LENELHolidays)
        {
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "delete from Holidays where idOrganizacion =" + v_idOrganization.ToString();

                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();

                int contador = 1;

                foreach (HolidayData holi in v_LENELHolidays)
                {
                    string fechaIni = holi.año.ToString() + "-" + holi.mes.ToString() + "-" + holi.dia.ToString();
                    int cantDias = holi.cantDias;

                    int tipo = holi.tipo;
                    string Nombre = v_idOrganization.ToString() + "_Holiday_" + contador.ToString();

                    cmd = cnn.CreateCommand();

                    cmd.CommandText = "insert into Holidays (idOrganizacion,FechaIni,CantDias,Tipo,Nombre) values (" + v_idOrganization + ",'" +fechaIni + "'," + cantDias + "," + tipo + ",'" + Nombre + "')";
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    contador++;
                }

            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addLenelHoliday: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

        }

        /// <summary>
        /// Da de alta la definicion del accessLevel  v_accessLevelID correspondiente al PanelID
        /// Es una definicion completa, por lo que se da de baja antes de hacer el add.
        /// </summary>
        /// <param name="v_LNLACC"></param>
        /// <param name="v_PanelID"></param>
        /// <param name="v_accessLevelID"></param>
        public void addLenelAccesLevel(LENELAccessLevels v_LNLACC, int v_PanelID, int v_accessLevelID, int v_idOrganizacion)
        {

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "delete from AccessLevels where idOrganizacion =" + v_idOrganizacion.ToString() + " and LNLPanelID = " + v_PanelID.ToString() + " and idAccess = " + v_accessLevelID.ToString();

                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();

                AccessLevelData ACCData = v_LNLACC.getAccessLevel(v_PanelID);

                if (ACCData != null)
                {
                    
                    Dictionary<int,int> ReaderTZDef = ACCData.ReaderTZ[v_accessLevelID];


                    foreach (KeyValuePair<int, int> pair in ReaderTZDef)
                    {
                        cmd = cnn.CreateCommand();

                        cmd.CommandText = "insert into AccessLevels (idOrganizacion,LNLPanelID,idAccess,ReaderID,TZNum) values (" + v_idOrganizacion.ToString() + "," + v_PanelID.ToString() + "," + v_accessLevelID + "," + pair.Key.ToString() + "," + pair.Value.ToString() + ")";
                        cmd.CommandType = CommandType.Text;
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception ex)
            {
                loguearString("Excepcion en addLenelAccessLevel: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
        }

        /// <summary>
        /// Hace un update del campo ultimaActualizacion a todas las tarjetas que contienen el accesslevel indicado
        /// y a sus empleados. Tambien aumenta en +1 la version de los Empleados
        /// </summary>
        /// <param name="v_accessLevelID"></param>
        public void updateUltimasActualizacionesTarjetasyEmpleados(int v_orgID,int v_accessLevelID)
        {
            //select * from Tarjetas where accessLevels like '%16%'
            DateTime fechaActual = DateTime.Now;
            string fechaActualStr = fechaActual.ToString("yyyy-MM-dd HH:mm:ss");

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();

                SqlCommand cmd = cnn.CreateCommand();
                cmd.CommandText = "update Tarjetas set ultimaActualizacion='" + fechaActualStr + "' where idOrganizacion = " + v_orgID.ToString() + " and accessLevels like '%" + v_accessLevelID.ToString() + "'";

                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();

                cmd.CommandText = "update Empleados set ultimaActualizacion='" + fechaActualStr + "', Version = Version+1 where idEmpleado in (Select Tarjetas.idEmpleado from Tarjetas,Empleados where Tarjetas.idEmpleado = Empleados.idEmpleado)";
                
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                loguearString("Excepcion en updateUltimasActualizacionesTarjetasyEmpleados: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            updateUltimasActualizacionesTarjetasyEmpleadosRAM(v_orgID, v_accessLevelID, fechaActual);
        }

        /// <summary>
        /// Actualiza las fechas de ultima actualizacion de las tarjetas que contienen en AccessLevel indicado
        /// y tambien las fechas de ultima actualizacion de sus empleados y sus numeros de versiones.
        /// </summary>
        /// <param name="v_orgid"></param>
        /// <param name="v_accesslevelID"></param>
        /// <param name="actualDate"></param>
        private void updateUltimasActualizacionesTarjetasyEmpleadosRAM(int v_orgid, int v_accesslevelID, DateTime actualDate)
        {
            string strACCLvl = v_accesslevelID.ToString();
            try
            {
                StaticTools.obtenerMutex_ListaTarjetas();
                StaticTools.obtenerMutex_ListaEmpleados();

                foreach (KeyValuePair<string, Tarjeta> valuePair in listaTarjetas)
                {
                    Tarjeta t = valuePair.Value;
                    if ((t.OrgID == v_orgid) && t.accessLevels.Contains(strACCLvl))
                    {
                        t.ultimaActualizacion = actualDate;
                        if (listaEmpleados.ContainsKey(t.idEmpleado))
                        {
                            listaEmpleados[t.idEmpleado].ultimaActualizacion = actualDate;
                            listaEmpleados[t.idEmpleado].VersionEmpleado++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en updateUltimasActualizacionesTarjetasyEmpleadosRAM: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                StaticTools.liberarMutex_ListaTarjetas();
                StaticTools.liberarMutex_ListaEmpleados();
            }

        }





        // Carga todas las listas de Holidays de todas las organizaciones.
        public Dictionary<int, List<HolidayData>> LoadListaHolidays()
        {
            Dictionary<int, List<HolidayData>> listaResultado = new Dictionary<int, List<HolidayData>>();

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Holidays";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    int idOrg = int.Parse(lector["idOrganizacion"].ToString());

                    if (!listaResultado.ContainsKey(idOrg))
                    {
                        List<HolidayData> nuevaLista = new List<HolidayData>();
                        listaResultado.Add(idOrg, nuevaLista);
                    }

                    List<HolidayData> listaHoli = listaResultado[idOrg];

                    string fechaIni = lector["FechaIni"].ToString();
                    int cantDias = int.Parse(lector["CantDias"].ToString());
                    int tipo = int.Parse(lector["Tipo"].ToString());
                    string holidayDef = fechaIni.Replace('-',',') + ","+tipo.ToString() + "," + cantDias.ToString();

                    HolidayData nuevaHoli = new HolidayData(holidayDef);

                    listaHoli.Add(nuevaHoli);

                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en LoadListaHolidays: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            return listaResultado;
        }

        // Carga todas las listas de Timezones definidas para todas las organizaciones.
        public Dictionary<int, LENELTimeZones> LoadListaTimeZones()
        {
            Dictionary<int, LENELTimeZones> listaResultado = new Dictionary<int, LENELTimeZones>();

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from TimeZones";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    int idOrg = int.Parse(lector["idOrganizacion"].ToString());

                    if (!listaResultado.ContainsKey(idOrg))
                    {
                        LENELTimeZones nuevaLista = new LENELTimeZones(idOrg);
                        listaResultado.Add(idOrg, nuevaLista);
                    }

                    LENELTimeZones timeZones = listaResultado[idOrg];

                    string rangoIni = lector["rangoIni"].ToString();
                    string rangoFin = lector["rangoFin"].ToString();
                    int TZNum = int.Parse(lector["TZNum"].ToString());
                    int DOW = int.Parse(lector["DOW"].ToString());
                    int HOL = int.Parse(lector["HOL"].ToString());

                    string[] datosRangoIni = rangoIni.Split(':');
                    string[] datosRangoFin = rangoFin.Split(':');

                    string horaIni = datosRangoIni[0];
                    string minIni = datosRangoIni[1];
                    string horaFin = datosRangoFin[0];
                    string minFin = datosRangoFin[1];

                    string timeZoneDef = horaIni +"," + minIni + "," + horaFin +"," + minFin + "," + DOW.ToString() + "," + HOL.ToString();

                    TimeZoneData TZData = timeZones.getTimeZoneData();
                    TZData.addTZInterval(TZNum, new TZInterval(timeZoneDef));
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en LoadListaTimeZones: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            return listaResultado;
        }


        // Carga todos los datos de los AccessLevels de todas las organizaciones
        public Dictionary<int, LENELAccessLevels> LoadListaAccessLevels()
        {
            Dictionary<int, LENELAccessLevels> listaResultado = new Dictionary<int, LENELAccessLevels>();

            SqlConnection cnn = new SqlConnection(conexion);
            //try
            //{
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from AccessLevels";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    int idOrg = int.Parse(lector["idOrganizacion"].ToString());

                    if (!listaResultado.ContainsKey(idOrg))
                    {
                        LENELAccessLevels nuevoAccessLevels = new LENELAccessLevels(idOrg);
                        listaResultado.Add(idOrg, nuevoAccessLevels);
                    }

                    LENELAccessLevels accessLevels = listaResultado[idOrg];

                    int LNLPanelID = int.Parse(lector["LNLPanelID"].ToString());
                    int idAccessLevel = int.Parse(lector["idAccess"].ToString());
                    int readerID = int.Parse(lector["ReaderID"].ToString());
                    int TZNum = int.Parse(lector["TZNum"].ToString());

                    AccessLevelData ALD = accessLevels.obtenerAccessLevelsFromPanel(LNLPanelID);

                    string datosAccessLevel = readerID.ToString() + "," + TZNum.ToString();

                    ALD.addReaderTZData(idAccessLevel, datosAccessLevel);
                }
                lector.Close();
            //}
            //catch (Exception ex)
            //{
            //    loguearString("Excepcion en LoadListaAccessLevels: " + ex.Message, TiposLOG.ALUTRACK);
            //}
            //finally
            //{
                cnn.Close();
            //}
            return listaResultado;
        }

        // Lista de asociaciones de tarjetas a accesslevels para todas las organizaciones
        public Dictionary<int, LENELBadgeAccessLevels> LoadListaBadgeAccessLevels()
        {
            Dictionary<int, LENELBadgeAccessLevels> listaResultado = new Dictionary<int, LENELBadgeAccessLevels>();


            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Tarjetas";
                cmd.CommandType = CommandType.Text;
                SqlDataReader lector = cmd.ExecuteReader();
                while (lector.Read())
                {
                    int idOrg = int.Parse(lector["idOrganizacion"].ToString());

                    if (!listaResultado.ContainsKey(idOrg))
                    {
                        LENELBadgeAccessLevels nuevoAccessLevels = new LENELBadgeAccessLevels(idOrg);
                        listaResultado.Add(idOrg, nuevoAccessLevels);
                    }

                    LENELBadgeAccessLevels accessLevels = listaResultado[idOrg];

                    long idTarjeta = long.Parse(lector["Tarjeta"].ToString());
                    string listaAccessLevels = lector["accessLevels"].ToString();

                    accessLevels.asociarAccessLevel(idTarjeta.ToString(), listaAccessLevels);
                }
                lector.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en LoadListaBadgeAccessLevels: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }


            return listaResultado;

        }

        internal void aumentarIdImagen(string strPersonID)
        {
            aumentarVersionEmpleado(strPersonID);

            int verImg = 0;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select IdImagen from Empleados where PersonID = '" + strPersonID + "'";
                cmd.CommandType = CommandType.Text;

                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read()) verImg = Convert.ToInt32(rdr[0].ToString());

                rdr.Close();

                verImg++;
                cmd.CommandText = "update Empleados set IdImagen = '" + verImg.ToString() + "' from Empleados where PersonID = '" + strPersonID + "'";
                cmd.CommandType = CommandType.Text;

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en aumentarIdImagen: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }

            // Aumenta la version de la Imagen del empleado en la lista de RAM    
            Employee emp = mainApp.DataManager.buscarEmpleadoxPersonIDenRAM(strPersonID);
            if (emp != null)
            {
                emp.imageVersion++;
            }

        }

        internal void aumentarVersionEmpleado(string strPersonID)
        {
            int version = 0;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select Version from Empleados where PersonID = '"+strPersonID+"'";
                cmd.CommandType = CommandType.Text;
                
                SqlDataReader rdr = cmd.ExecuteReader();
                
                if (rdr.Read()) version = Convert.ToInt32(rdr[0].ToString());
                
                rdr.Close();
                version++;
                cmd.CommandText = "update Empleados set Version = '" + version.ToString() + "' from Empleados where PersonID = '" + strPersonID + "'";
                cmd.CommandType = CommandType.Text;

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en aumentarVersionEmpleado: " + ex.Message, TiposLOG.ALUTRACK);
            }
            finally
            {
                cnn.Close();
            }

            // Aumenta la version del empleado en la lista de RAM    
            Employee emp = mainApp.DataManager.buscarEmpleadoxPersonIDenRAM(strPersonID);
            if (emp != null)
            {
                emp.VersionEmpleado++;
            }

        }
        /// <summary>
        /// Devuelve la versión de la imagen del empleado según su PersonID
        /// </summary>
        /// <param name="personID">PersonID del empleado del que se desea obtener la versión de imagen</param>
        /// <returns></returns>
        internal string cargarVersionImagen(string personID)
        {
            string verimg = string.Empty;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select IdImagen from Empleados where PersonID = '" + personID + "'";
                cmd.CommandType = CommandType.Text;

                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read()) verimg = rdr[0].ToString();

                rdr.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en cargarVersionImagen: " + ex.Message, TiposLOG.HH);
                verimg = string.Empty;
            }
            finally
            {
                cnn.Close();
            }

            return verimg;
        }

        /// <summary>
        /// Devuelve el path de la imagen del empleado
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal string cargarPathImagen(string personID)
        {
            string imagenDB = string.Empty;

            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select Imagen from Empleados where PersonID = '" + personID + "'";
                cmd.CommandType = CommandType.Text;

                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read()) imagenDB = rdr[0].ToString();

                rdr.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en cargarPathImagen: " + ex.Message, TiposLOG.HH);
                imagenDB = string.Empty;
            }
            finally
            {
                cnn.Close();
            }

            return imagenDB;
        }

        internal void actualizarMemoriaEmpleados(string empleadoID)
        {
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Empleados where idEmpleado = '" + empleadoID + "'";
                cmd.CommandType = CommandType.Text;

                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read()) 
                    agregarOActualizarEmpleadoEnRAM(int.Parse(empleadoID), rdr["Nombre"].ToString(), rdr["Apellido"].ToString(),
                        rdr["Empresa"].ToString(), rdr["NumeroDocumento"].ToString(), 
                        rdr["PersonID"].ToString().Equals(string.Empty) ? 0:int.Parse(rdr["PersonID"].ToString()), 
                        rdr["Imagen"].ToString(),
                        rdr["Version"].ToString().Equals(string.Empty) ? 0 : int.Parse(rdr["Version"].ToString()), 
                        rdr["ultimaActualizacion"].ToString());
                
                rdr.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en actualizarMemoriaEmpleados: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
        }

        internal void actualizarMemoriaTarjetasyEmpleados(string tarjetaID)
        {
            SqlConnection cnn = new SqlConnection(conexion);
            try
            {
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = "select * from Tarjetas where idTarjeta = '" + tarjetaID + "'";
                cmd.CommandType = CommandType.Text;

                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    agregarOActualizarTarjetaEnRAM(rdr["Tarjeta"].ToString(), int.Parse(rdr["idEmpleado"].ToString()), rdr["idTarjeta"].ToString());
                    actualizarMemoriaEmpleados(rdr["idEmpleado"].ToString());
                }

                rdr.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepcion en actualizarMemoriaTarjetas: " + ex.Message, TiposLOG.HH);
            }
            finally
            {
                cnn.Close();
            }
        }

        internal void deleteEmpFromMemory(string empleadoID)
        {
            if (listaEmpleados.ContainsKey(int.Parse(empleadoID)))
                listaEmpleados.Remove(int.Parse(empleadoID));
        }
        internal void deleteTarjFromMemory(string numeroTarjeta) {

            if (listaEmpleados.ContainsKey(int.Parse(numeroTarjeta)))
                listaEmpleados.Remove(int.Parse(numeroTarjeta));
            
        }

        // Creado 7/10
        internal List<string> ObtenerHHxNumeroTarjeta(String tarjeta) {

            SqlConnection cnn = new SqlConnection(conexion);
            SqlDataReader rdr = null;
            List<string> lst = new List<string>();
            try
            {

                string query = "select distinct devices.HHID from Tarjetas,accesslevels,Devices where Tarjetas.Tarjeta='" + tarjeta + "' and Tarjetas.accessLevels=AccessLevels.idAccess and AccessLevels.LNLPanelID=devices.LNLPanelID ";
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    lst.Add(rdr[0].ToString());
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepción en ObtenerHHxIdTarjeta: " + ex.Message, TiposLOG.ALUTRACK);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                cnn.Close();
            }

            return lst;
        
        }
        internal List<string> ObtenerHHxIdTarjeta(string idTarjeta)
        {
            SqlConnection cnn = new SqlConnection(conexion);
            SqlDataReader rdr = null;
            List<string> lst = new List<string>();
            try
            {

                string query = "select distinct devices.HHID from Tarjetas,accesslevels,Devices where Tarjetas.idTarjeta='" + idTarjeta + "' and Tarjetas.accessLevels=AccessLevels.idAccess and AccessLevels.LNLPanelID=devices.LNLPanelID ";
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    lst.Add(rdr[0].ToString());
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepción en ObtenerHHxIdTarjeta: " + ex.Message, TiposLOG.ALUTRACK);
            }
            finally 
            {
                if (rdr != null) rdr.Close();
                cnn.Close(); 
            }

            return lst;
        }
        // hecho 7/10
        internal long obtenerIdTarjeta(string numeroTarjeta)
        {
            SqlConnection cnn = new SqlConnection(conexion);
            SqlDataReader rdr = null;
            long tarjeta = 0;
            try
            {

                string query = "select idTarjeta from Tarjetas where Tarjeta='" + numeroTarjeta + "'";
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                rdr = cmd.ExecuteReader();
                rdr.Read();
                tarjeta = long.Parse(rdr[0].ToString());

                rdr.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepción en ObtenerTarjeta: " + ex.Message, TiposLOG.ALUTRACK);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                cnn.Close();
            }

            return tarjeta;
        }
        internal long ObtenerTarjeta(string idTarjeta)
        {

            SqlConnection cnn = new SqlConnection(conexion);
            SqlDataReader rdr = null;
            long tarjeta = 0;
            try
            {

                string query = "select Tarjeta from Tarjetas where idTarjeta='" + idTarjeta+"'";
                cnn.Open();
                SqlCommand cmd = cnn.CreateCommand();

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                rdr = cmd.ExecuteReader();
                rdr.Read();
                tarjeta = long.Parse(rdr[0].ToString());
                
                rdr.Close();
            }
            catch (Exception ex)
            {
                loguearString("Excepción en ObtenerTarjeta: " + ex.Message, TiposLOG.ALUTRACK);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                cnn.Close();
            }

            return tarjeta;
        }

//***********************************LOG*******************************************
/// <summary>
/// 
/// </summary>
/// <param name="v_texto"></param>
/// <param name="v_tipoLOG"></param>
        private void loguearString(string v_texto, TiposLOG v_tipoLOG)
        {
            if (m_LOGHandler != null)
            {
                stringEventArgs p = new stringEventArgs(v_texto, null);
                p.LOGTYPE = v_tipoLOG;

                m_LOGHandler(this, p);          // Llamada al handler del LOG.
            }

        }



        
    }
}

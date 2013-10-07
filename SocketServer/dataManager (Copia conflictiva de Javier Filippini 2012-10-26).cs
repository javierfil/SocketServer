using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;

namespace ServerComunicacionesALUTEL
{
    public class dataManager
    {
        private Dictionary<string, Employee> listaUsuarios;
        private Dictionary<string, device> listaDevices;
        private Dictionary<string, List<string>> gruposUsuarios;
        private Dictionary<string, List<string>> gruposDevices;

        private Dictionary<string, Employee> usuariosActivos;
        private Dictionary<string, device> devicesActivos;
        private Dictionary<string, Acceso> eventosDeAcceso;

        private Dictionary<string, KeyValuePair<string, string>> reglasAcceso;
        private Dictionary<string, Acceso> listaAccesos;
        private Dictionary<string, Visita> listaVisitas;

        private Dictionary<string, List<EventoGPS>> listaEventosGPS;        // La clave de cada Lista es el HHID

        private Dictionary<string, Zone> listaZonas;                        // Lista de Zonas definidas. 

        private Dictionary<string, string> camposLenelCamposSistema;

        private List<VehicleAccess> listaAccesosVehiculos;

        private string dataSource;
        private Aplicacion mainApp;


        // Chequeo de la interseccion de puertas.
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
     /// Inicializo las estructuras de datos que se llenarán en RAM con el loadData
     /// </summary>
        public dataManager(string v_dataSource,Aplicacion v_app)
        {
            mainApp = v_app;
            dataSource = v_dataSource;
            listaUsuarios = new Dictionary<string, Employee>();
            listaDevices = new Dictionary<string, device>();
            gruposUsuarios = new Dictionary<string, List<string>>();        // Grupos de listas de id de Usuarios
            gruposDevices = new Dictionary<string, List<string>>();         // Grupos de listas de id de Devices
            usuariosActivos = new Dictionary<string, Employee>();
            devicesActivos = new Dictionary<string, device>();
            eventosDeAcceso = new Dictionary<string, Acceso>();

            reglasAcceso = new Dictionary<string, KeyValuePair<string, string>>();
            listaAccesos = new Dictionary<string, Acceso>();
            listaVisitas = new Dictionary<string, Visita>();

            listaEventosGPS = new Dictionary<string, List<EventoGPS>>();

            listaZonas = new Dictionary<string, Zone>();

            listaAccesosVehiculos = new List<VehicleAccess>();

            camposLenelCamposSistema = new Dictionary<string, string>();
        }

        /// <summary>
        /// Cargo desde el dataSource todas las estructuras de datos en RAM
        /// </summary>
        /// <param name="dataSource"></param>
        public void loadData()
        {
            // Carga la lista de usuarios y la lista de devices desde la dataSource
            LoadEmpleados();
            LoadDevices();
            LoadGruposUsuarios();
            LoadGruposDevices();
            LoadReglasAcceso();
            LoadAccesos();
            LoadVisitas();
            LoadEventosGPS();
           // LoadZonas();

        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadEventosGPS()
        {
            if (File.Exists(dataSource + @"\\eventosGPS.xml"))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(dataSource + @"\\eventosGPS.xml");
                
                // toma el recurso de lista de eventos.
                //Aplicacion.semaforo_GPS.WaitOne();
                
                listaEventosGPS.Clear();
                foreach (XmlElement elem in xDoc.SelectNodes("/EventosGPS/Evento"))
                {
                    string HHID = elem.Attributes["HHID"].Value;
                    string latitud = elem.Attributes["latitud"].Value;
                    string longitud = elem.Attributes["longitud"].Value;
                    string hora = elem.Attributes["hora"].Value;

                    if (!listaEventosGPS.ContainsKey(HHID))
                    {
                        List<EventoGPS> listaEvento = new List<EventoGPS>();
                        listaEventosGPS.Add(HHID, listaEvento);
                    }

                    EventoGPS nuevoEvento = new EventoGPS(HHID,latitud,longitud,hora);

                    listaEventosGPS[HHID].Add(nuevoEvento);
                }
               // Aplicacion.semaforo_GPS.Release();
            }
        }

        private void LoadVisitas()
        {
            if (File.Exists(dataSource + @"\\visitas.xml"))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(dataSource + @"\\visitas.xml");

                listaVisitas.Clear();

                foreach (XmlElement elem in xDoc.SelectNodes("/Visitas/Visita"))
                {
                    string id = elem.Attributes["id"].Value;
                    string HHID = elem.Attributes["HHID"].Value;
                    string nombre = elem.Attributes["nombre"].Value;
                    string apellido = elem.Attributes["apellido"].Value;
                    string documento = elem.Attributes["documento"].Value;
                    string empresa = elem.Attributes["empresa"].Value;
                    string tarjeta = elem.Attributes["tarjeta"].Value;
                    string latitud = elem.Attributes["latitud"].Value;
                    string longitud = elem.Attributes["longitud"].Value;
                    string hora = elem.Attributes["hora"].Value;
                    string imageFile = elem.Attributes["imageFile"].Value;

                    TiposAcceso tipoAcceso = (TiposAcceso)Enum.Parse(typeof(TiposAcceso), elem.Attributes["tipoAcceso"].Value);
                    byte[] imageBytes = null;

                    if (imageFile != "")
                    {
                        imageBytes = File.ReadAllBytes(imageFile);
                    }

                    if (!listaVisitas.ContainsKey(id))
                    {
                        Visita nuevaVisita = new Visita(HHID, nombre, apellido, documento, empresa, tarjeta, latitud, longitud, hora, imageBytes, tipoAcceso);

                        listaVisitas.Add(id, nuevaVisita);
                    }
                }
            }
        }


        private void LoadAccesos()
        {
            //byte[] bytes = File.ReadAllBytes(imagename);
            //v_emp.attachImage(bytes);
            if (File.Exists(dataSource + @"\\accesos.xml"))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(dataSource + @"\\accesos.xml");

                listaAccesos.Clear();

                foreach (XmlElement elem in xDoc.SelectNodes("/Accesos/Acceso"))
                {
                    string id = elem.Attributes["id"].Value;
                    string HHID = elem.Attributes["HHID"].Value;
                    string tarjeta = elem.Attributes["tarjeta"].Value;
                    string latitud = elem.Attributes["latitud"].Value;
                    string longitud = elem.Attributes["longitud"].Value;
                    string hora = elem.Attributes["hora"].Value;
                    string imageFile = elem.Attributes["imageFile"].Value;
                    
                    TiposAcceso tipoAcceso = (TiposAcceso)Enum.Parse(typeof(TiposAcceso),elem.Attributes["tipoAcceso"].Value);
                    byte[] imageBytes = null;

                    if (imageFile != "")
                    {
                        imageBytes = File.ReadAllBytes(imageFile);
                    }

                    if (!listaAccesos.ContainsKey(id))
                    {
                        Acceso nuevoAcceso = new Acceso(HHID, tarjeta, latitud, longitud, hora, imageBytes, tipoAcceso);

                        listaAccesos.Add(id, nuevoAcceso);
                    }
                }
            }
        }


        private void LoadDevices()
        {
            if (File.Exists(dataSource + @"\\devices.xml"))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(dataSource + @"\\devices.xml");

                listaDevices.Clear();

                foreach (XmlElement elem in xDoc.SelectNodes("/Devices/Device"))
                {
                    string id = elem.Attributes["id"].Value;

                    device newDevice = new device();
                    newDevice.id = id;

                    if (!listaDevices.ContainsKey(id))
                    {
                        listaDevices.Add(id, newDevice);
                    }
                }
            }
        }


        public void LoadReglasAcceso()
        {
            if (File.Exists(dataSource + @"\\reglasAcceso.xml"))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(dataSource + @"\\reglasAcceso.xml");

                reglasAcceso.Clear();
                foreach (XmlElement elem in xDoc.SelectNodes(@"/ReglasAcceso/ReglaAcceso"))
                {
                    string idRegla = elem.Attributes["id"].Value;
                    string grupoUsuario = elem.Attributes["grupoUsuario"].Value;
                    string grupoDevice = elem.Attributes["grupoDevice"].Value;
                    KeyValuePair<string, string> asociacion = new KeyValuePair<string, string>(grupoUsuario, grupoDevice);
                    if (!reglasAcceso.ContainsKey(idRegla))
                    {
                        reglasAcceso.Add(idRegla, asociacion);
                    }
                }
            }
            
        }
        public void LoadGruposDevices()
        {
            if (File.Exists(dataSource + @"\\gruposDevices.xml"))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(dataSource + @"\\gruposDevices.xml");

                gruposDevices.Clear();

                foreach (XmlElement elem in xDoc.SelectNodes(@"/GruposDevices/GrupoDevice"))
                {
                    string idGrupo = elem.Attributes["id"].Value;

                    List<string> listaDevices = new List<string>();
                    foreach (XmlElement elementUser in elem.SelectNodes(@"Device"))
                    {
                        string idDevice = elementUser.Attributes["id"].Value;

                        listaDevices.Add(idDevice);
                    }
                    if (!gruposDevices.ContainsKey(idGrupo))
                    {
                        gruposDevices.Add(idGrupo, listaDevices);
                    }
                }
            }
        }


        public void LoadGruposUsuarios()
        {
            if (File.Exists(dataSource + @"\\gruposUsuarios.xml"))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(dataSource + @"\\gruposUsuarios.xml");

               gruposUsuarios.Clear();

                foreach (XmlElement elem in xDoc.SelectNodes("/GruposUsuarios/GrupoUsuario"))
                {
                    string idGrupo = elem.Attributes["id"].Value;

                    List<string> listaUsers = new List<string>();
                    foreach (XmlElement elementUser in elem.SelectNodes("Usuario"))
                    {
                        string idUsuario = elementUser.Attributes["id"].Value;

                        listaUsers.Add(idUsuario);
                    }
                    if (!gruposUsuarios.ContainsKey(idGrupo))
                    {
                        gruposUsuarios.Add(idGrupo, listaUsers);
                    }
                }
            }
        }

        private void LoadEmpleados()
        {
            if (File.Exists(dataSource + @"\\personal.xml"))
            {

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(dataSource + @"\\personal.xml");


                listaUsuarios.Clear();

                foreach (XmlElement elem in xDoc.SelectNodes("/Empleados/Empleado"))
                {
                    string tarjeta = elem.Attributes["tarjeta"].Value;
                    string nombre = elem.Attributes["nombre"].Value;
                    string apellido = elem.Attributes["apellido"].Value;
                    string empresa = elem.Attributes["empresa"].Value;
                    bool acceso = bool.Parse(elem.Attributes["acceso"].Value);
                    bool sexo = bool.Parse(elem.Attributes["sexo"].Value);
                    DateTime FechaNacimiento = DateTime.Parse(elem.Attributes["fechaNacimiento"].Value);
                    string Funcion = elem.Attributes["funcion"].Value;
                    string Direccion = elem.Attributes["direccion"].Value;
                    string Departamento = elem.Attributes["departamento"].Value;
                    string Nacionalidad = elem.Attributes["nacionalidad"].Value;
                    string Telefono = elem.Attributes["telefono"].Value;
                    EstadosCiviles EstadoCivil = (EstadosCiviles)Enum.Parse(typeof(EstadosCiviles), elem.Attributes["estadoCivil"].Value);
                    string NombreConyugue = elem.Attributes["nombreConyugue"].Value;
                    string NombrePadre = elem.Attributes["nombrePadre"].Value;
                    string NombreMadre = elem.Attributes["nombreMadre"].Value;
                    string NivelEscolaridad = elem.Attributes["nivelEscolaridad"].Value;
                    DateTime ValidezCarnetSalud = DateTime.Parse(elem.Attributes["validezCarnetSalud"].Value);
                    string SSNO = elem.Attributes["SSNO"].Value;
                    DateTime FechaExpedicionSSNO = DateTime.Parse(elem.Attributes["fechaExpedicionSSNO"].Value);
                    DateTime FechaVencimientoSSNO = DateTime.Parse(elem.Attributes["fechaVencimientoSSNO"].Value);
                    CategoríasLibretasConducir CategoriaLibretaConducir = (CategoríasLibretasConducir)Enum.Parse(typeof(CategoríasLibretasConducir), elem.Attributes["categoriaLibretaConducir"].Value);
                    DateTime VencimientoLibretaConducir = DateTime.Parse(elem.Attributes["vencimientoLibretaConducir"].Value);
                    DateTime IngresoBPS = DateTime.Parse(elem.Attributes["ingresoBPS"].Value);
                    string TipoAportacion = elem.Attributes["tipoAportacion"].Value;
                    DateTime VigenciaBSE = DateTime.Parse(elem.Attributes["vigenciaBSE"].Value);
                    string imageName = elem.Attributes["imagen"].Value;

                    Employee emp = new Employee();

                    emp.Tarjeta = tarjeta;
                    emp.Nombre = nombre;
                    emp.Apellido = apellido;
                    emp.Empresa = empresa;
                    emp.Acceso = acceso;
                    emp.Sexo = sexo;
                    emp.FechaNacimiento = FechaNacimiento;
                    emp.Funcion = Funcion;
                    emp.Direccion = Direccion;
                    emp.Departamento = Departamento;
                    emp.Nacionalidad = Nacionalidad;
                    emp.Telefono = Telefono;
                    emp.EstadoCivil = EstadoCivil;
                    emp.NombreConyugue = NombreConyugue;
                    emp.NombrePadre = NombrePadre;
                    emp.NombreMadre = NombreMadre;
                    emp.NivelEscolaridad = NivelEscolaridad;
                    emp.ValidezCarnetSalud = ValidezCarnetSalud;
                    emp.SSNO = SSNO;
                    emp.FechaExpedicionSSNO = FechaExpedicionSSNO;
                    emp.FechaVencimientoSSNO = FechaVencimientoSSNO;
                    emp.CategoriaLibretaConducir = CategoriaLibretaConducir;
                    emp.VencimientoLibretaConducir = VencimientoLibretaConducir;
                    emp.IngresoBPS = IngresoBPS;
                    emp.TipoAportación = TipoAportacion;
                    emp.VigenciaBSE = VigenciaBSE;
                    emp.imageFileName = imageName;
                    if (imageName!="")
                        attachImageByteArray(emp, imageName);

                    if (!listaUsuarios.ContainsKey(tarjeta))
                    {
                        listaUsuarios.Add(tarjeta, emp);
                    }
                }
            }
        }


        private void attachImageByteArray(Employee v_emp, string imagename)
        {
            byte[] bytes = File.ReadAllBytes(imagename);
            v_emp.attachImage(bytes);
        }
        private void SaveEmpleados()
        {

            XmlDocument xdoc = new XmlDocument();
            XmlElement elementRoot = xdoc.CreateElement("Empleados");
            xdoc.AppendChild(elementRoot);

            foreach (KeyValuePair<string,Employee> keyValue in listaUsuarios)
            {
                XmlElement infoEmpleado = xdoc.CreateElement("Empleado");

                XmlAttribute att_Tarjeta = xdoc.CreateAttribute("tarjeta");
                att_Tarjeta.Value = keyValue.Value.Tarjeta;
                infoEmpleado.Attributes.Append(att_Tarjeta);

                XmlAttribute att_Nombre = xdoc.CreateAttribute("nombre");
                att_Nombre.Value = keyValue.Value.Nombre;
                infoEmpleado.Attributes.Append(att_Nombre);

                XmlAttribute att_Apellido = xdoc.CreateAttribute("apellido");
                att_Apellido.Value = keyValue.Value.Apellido;
                infoEmpleado.Attributes.Append(att_Apellido);

                XmlAttribute att_Empresa = xdoc.CreateAttribute("empresa");
                att_Empresa.Value = keyValue.Value.Empresa;
                infoEmpleado.Attributes.Append(att_Empresa);

                XmlAttribute att_Acceso = xdoc.CreateAttribute("acceso");
                att_Acceso.Value = keyValue.Value.Acceso.ToString();
                infoEmpleado.Attributes.Append(att_Acceso);

                XmlAttribute att_Sexo = xdoc.CreateAttribute("sexo");
                att_Sexo.Value = keyValue.Value.Sexo.ToString();
                infoEmpleado.Attributes.Append(att_Sexo);

                XmlAttribute att_Fechanac = xdoc.CreateAttribute("fechaNacimiento");
                att_Fechanac.Value = keyValue.Value.FechaNacimiento.ToString();
                infoEmpleado.Attributes.Append(att_Fechanac);

                XmlAttribute att_Funcion = xdoc.CreateAttribute("funcion");
                att_Funcion.Value = keyValue.Value.Funcion;
                infoEmpleado.Attributes.Append(att_Funcion);

                XmlAttribute att_Direccion = xdoc.CreateAttribute("direccion");
                att_Direccion.Value = keyValue.Value.Direccion;
                infoEmpleado.Attributes.Append(att_Direccion);

                XmlAttribute att_Depto = xdoc.CreateAttribute("departamento");
                att_Depto.Value = keyValue.Value.Departamento;
                infoEmpleado.Attributes.Append(att_Depto);

                XmlAttribute att_Nac = xdoc.CreateAttribute("nacionalidad");
                att_Nac.Value = keyValue.Value.Nacionalidad;
                infoEmpleado.Attributes.Append(att_Nac);

                XmlAttribute att_Tel = xdoc.CreateAttribute("telefono");
                att_Tel.Value = keyValue.Value.Telefono;
                infoEmpleado.Attributes.Append(att_Tel);

                XmlAttribute att_EstadoCiv = xdoc.CreateAttribute("estadoCivil");
                att_EstadoCiv.Value = keyValue.Value.EstadoCivil.ToString();
                infoEmpleado.Attributes.Append(att_EstadoCiv);

                XmlAttribute att_NombreCony = xdoc.CreateAttribute("nombreConyugue");
                att_NombreCony.Value = keyValue.Value.NombreConyugue;
                infoEmpleado.Attributes.Append(att_NombreCony);

                XmlAttribute att_NombrePadre = xdoc.CreateAttribute("nombrePadre");
                att_NombrePadre.Value = keyValue.Value.NombrePadre;
                infoEmpleado.Attributes.Append(att_NombrePadre);

                XmlAttribute att_NombreMadre = xdoc.CreateAttribute("nombreMadre");
                att_NombreMadre.Value = keyValue.Value.NombreMadre;
                infoEmpleado.Attributes.Append(att_NombreMadre);

                XmlAttribute att_NivelEsc = xdoc.CreateAttribute("nivelEscolaridad");
                att_NivelEsc.Value = keyValue.Value.NivelEscolaridad;
                infoEmpleado.Attributes.Append(att_NivelEsc);

                XmlAttribute att_ValCarSal = xdoc.CreateAttribute("validezCarnetSalud");
                att_ValCarSal.Value = keyValue.Value.ValidezCarnetSalud.ToString();
                infoEmpleado.Attributes.Append(att_ValCarSal);

                XmlAttribute att_SSNO = xdoc.CreateAttribute("SSNO");
                att_SSNO.Value = keyValue.Value.SSNO;
                infoEmpleado.Attributes.Append(att_SSNO);

                XmlAttribute att_FechaExpSSNO = xdoc.CreateAttribute("fechaExpedicionSSNO");
                att_FechaExpSSNO.Value = keyValue.Value.FechaExpedicionSSNO.ToString();
                infoEmpleado.Attributes.Append(att_FechaExpSSNO);

                XmlAttribute att_FechaVenSSNO = xdoc.CreateAttribute("fechaVencimientoSSNO");
                att_FechaVenSSNO.Value = keyValue.Value.FechaVencimientoSSNO.ToString();
                infoEmpleado.Attributes.Append(att_FechaVenSSNO);

                XmlAttribute att_CatLibCond = xdoc.CreateAttribute("categoriaLibretaConducir");
                att_CatLibCond.Value = keyValue.Value.CategoriaLibretaConducir.ToString();
                infoEmpleado.Attributes.Append(att_CatLibCond);

                XmlAttribute att_FechaVenLibCond = xdoc.CreateAttribute("vencimientoLibretaConducir");
                att_FechaVenLibCond.Value = keyValue.Value.VencimientoLibretaConducir.ToString();
                infoEmpleado.Attributes.Append(att_FechaVenLibCond);

                XmlAttribute att_IngresoBPS = xdoc.CreateAttribute("ingresoBPS");
                att_IngresoBPS.Value = keyValue.Value.IngresoBPS.ToString();
                infoEmpleado.Attributes.Append(att_IngresoBPS);

                XmlAttribute att_TipAp = xdoc.CreateAttribute("tipoAportacion");
                att_TipAp.Value = keyValue.Value.TipoAportación;
                infoEmpleado.Attributes.Append(att_TipAp);

                XmlAttribute att_VigenciaBSE = xdoc.CreateAttribute("vigenciaBSE");
                att_VigenciaBSE.Value = keyValue.Value.VigenciaBSE.ToString();
                infoEmpleado.Attributes.Append(att_VigenciaBSE);

                XmlAttribute att_Imagen = xdoc.CreateAttribute("imagen");
                att_Imagen.Value = keyValue.Value.imageFileName;
                infoEmpleado.Attributes.Append(att_Imagen);

                elementRoot.AppendChild(infoEmpleado);

                // graba la imagen del empleado
                if ((keyValue.Value.imageFileName != null) && (keyValue.Value.getImage() !=null))
                {
                    if (keyValue.Value.imageFileName != "")
                    {
                        File.WriteAllBytes(dataSource + keyValue.Value.imageFileName, keyValue.Value.getImage());
                    }
                }
            }

            // Graba el XML Final
            xdoc.Save(dataSource + @"\\personal.xml");
        }

        private void SaveVisitas()
        {
            XmlDocument xdoc = new XmlDocument();
            XmlElement elementRoot = xdoc.CreateElement("Visitas");
            xdoc.AppendChild(elementRoot);

            foreach (KeyValuePair<string, Visita> keyValue in listaVisitas)
            {
                XmlElement infoVisita = xdoc.CreateElement("Visita");

                XmlAttribute att_id = xdoc.CreateAttribute("id");
                att_id.Value = keyValue.Key;
                infoVisita.Attributes.Append(att_id);

                XmlAttribute att_HHID = xdoc.CreateAttribute("HHID");
                att_HHID.Value = keyValue.Value.HHID;
                infoVisita.Attributes.Append(att_HHID);

                XmlAttribute att_nombre = xdoc.CreateAttribute("nombre");
                att_nombre.Value = keyValue.Value.Nombre;
                infoVisita.Attributes.Append(att_nombre);

                XmlAttribute att_apellido = xdoc.CreateAttribute("apellido");
                att_apellido.Value = keyValue.Value.Apellido;
                infoVisita.Attributes.Append(att_apellido);

                XmlAttribute att_documento = xdoc.CreateAttribute("documento");
                att_documento.Value = keyValue.Value.Documento;
                infoVisita.Attributes.Append(att_documento);

                XmlAttribute att_empresa = xdoc.CreateAttribute("empresa");
                att_empresa.Value = keyValue.Value.Empresa;
                infoVisita.Attributes.Append(att_empresa);

                XmlAttribute att_tarjeta = xdoc.CreateAttribute("tarjeta");
                att_tarjeta.Value = keyValue.Value.Tarjeta;
                infoVisita.Attributes.Append(att_tarjeta);

                XmlAttribute att_latitud = xdoc.CreateAttribute("latitud");
                att_latitud.Value = keyValue.Value.Latitud;
                infoVisita.Attributes.Append(att_latitud);

                XmlAttribute att_longitud = xdoc.CreateAttribute("longitud");
                att_longitud.Value = keyValue.Value.Longitud;
                infoVisita.Attributes.Append(att_longitud);

                XmlAttribute att_hora = xdoc.CreateAttribute("hora");
                att_hora.Value = keyValue.Value.Hora;
                infoVisita.Attributes.Append(att_hora);

                string imageFileName = "";

                if (keyValue.Value.imagen != null)
                {
                    if (keyValue.Value.imagen.Length > 0)
                    {
                        imageFileName = keyValue.Key + ".jpg";
                    }
                }
                XmlAttribute att_imageFile = xdoc.CreateAttribute("imageFile");
                att_imageFile.Value = imageFileName;
                infoVisita.Attributes.Append(att_imageFile);

                XmlAttribute att_tipoAcceso = xdoc.CreateAttribute("tipoAcceso");
                att_tipoAcceso.Value = keyValue.Value.tipoAcceso.ToString();
                infoVisita.Attributes.Append(att_tipoAcceso);

                if (imageFileName != "")
                {
                    File.WriteAllBytes(dataSource + @"\" + imageFileName, keyValue.Value.imagen);
                }

                elementRoot.AppendChild(infoVisita);
            }
            xdoc.Save(dataSource + @"\\visitas.xml");
        }

        private void SaveAccesos()
        {
            XmlDocument xdoc = new XmlDocument();
            XmlElement elementRoot = xdoc.CreateElement("Accesos");
            xdoc.AppendChild(elementRoot);

            foreach (KeyValuePair<string, Acceso> keyValue in listaAccesos)
            {
                XmlElement infoAcceso = xdoc.CreateElement("Acceso");

                XmlAttribute att_id = xdoc.CreateAttribute("id");
                att_id.Value = keyValue.Key;
                infoAcceso.Attributes.Append(att_id);

                XmlAttribute att_HHID = xdoc.CreateAttribute("HHID");
                att_HHID.Value = keyValue.Value.HHID;
                infoAcceso.Attributes.Append(att_HHID);

                XmlAttribute att_tarjeta = xdoc.CreateAttribute("tarjeta");
                att_tarjeta.Value = keyValue.Value.Tarjeta;
                infoAcceso.Attributes.Append(att_tarjeta);

                XmlAttribute att_latitud = xdoc.CreateAttribute("latitud");
                att_latitud.Value = keyValue.Value.Latitud;
                infoAcceso.Attributes.Append(att_latitud);

                XmlAttribute att_longitud = xdoc.CreateAttribute("longitud");
                att_longitud.Value = keyValue.Value.Longitud;
                infoAcceso.Attributes.Append(att_longitud);

                XmlAttribute att_hora = xdoc.CreateAttribute("hora");
                att_hora.Value = keyValue.Value.Hora;
                infoAcceso.Attributes.Append(att_hora);

                string imageFileName ="";
                if (keyValue.Value.imagen != null)
                {
                    if (keyValue.Value.imagen.Length>0)
                    {
                        imageFileName = keyValue.Key + ".jpg";
                    }
                }

                XmlAttribute att_imageFile = xdoc.CreateAttribute("imageFile");
                att_imageFile.Value = imageFileName;
                infoAcceso.Attributes.Append(att_imageFile);

                XmlAttribute att_tipoAcceso = xdoc.CreateAttribute("tipoAcceso");
                att_tipoAcceso.Value = keyValue.Value.tipoAcceso.ToString();
                infoAcceso.Attributes.Append(att_tipoAcceso);


                if (imageFileName != "")
                {
                    File.WriteAllBytes(dataSource + @"\"+imageFileName, keyValue.Value.imagen);
                }
                elementRoot.AppendChild(infoAcceso);
            }
            xdoc.Save(dataSource + @"\\accesos.xml");
        }

        
        private void SaveDevices()
        {
            
            XmlDocument xdoc = new XmlDocument();
            XmlElement elementRoot = xdoc.CreateElement("Devices");
            xdoc.AppendChild(elementRoot);

            foreach (KeyValuePair<string, device> keyValue in listaDevices)
            {
                XmlElement infoDevice = xdoc.CreateElement("Device");

                XmlAttribute att_id = xdoc.CreateAttribute("id");
                att_id.Value = keyValue.Value.id;
                infoDevice.Attributes.Append(att_id);

                elementRoot.AppendChild(infoDevice);
            }

            xdoc.Save(dataSource + @"\\devices.xml");

        }

        // SaveEventosGPS. El semaforo que pide el recurso EventoGPS se setea y libera en la llamada.
        //
        private void SaveEventosGPS(string id_HH)
        {
            mainApp.ComunicationSystem.LOGInformation.Enqueue("Va a grabar eventosGPS");
            //Aplicacion.semaforo_GPS.WaitOne();
            try
            {
                XmlDocument xdoc = new XmlDocument();
                XmlElement elementRoot = xdoc.CreateElement("EventosGPS");
                xdoc.AppendChild(elementRoot);
                foreach (KeyValuePair<string, List<EventoGPS>> pair in listaEventosGPS)
                {
                    List<EventoGPS> listaEvento = pair.Value;
                    foreach (EventoGPS evento in listaEvento)
                    {
                        XmlElement infoEvento = xdoc.CreateElement("Evento");

                        XmlAttribute att_HHID = xdoc.CreateAttribute("HHID");
                        att_HHID.Value = pair.Key;
                        infoEvento.Attributes.Append(att_HHID);

                        XmlAttribute att_latitud = xdoc.CreateAttribute("latitud");
                        att_latitud.Value = evento.Latitud;
                        infoEvento.Attributes.Append(att_latitud);

                        XmlAttribute att_longitud = xdoc.CreateAttribute("longitud");
                        att_longitud.Value = evento.Longitud;
                        infoEvento.Attributes.Append(att_longitud);

                        XmlAttribute att_hora = xdoc.CreateAttribute("hora");
                        att_hora.Value = evento.Hora;
                        infoEvento.Attributes.Append(att_hora);
                        elementRoot.AppendChild(infoEvento);
                    }
                }
                //string filename = @"\\"+id_HH + @"_eventosGPS.xml";
                //xdoc.Save(dataSource + filename);

                xdoc.Save(dataSource + @"\\eventosGPS.xml");
                Thread.Sleep(500);
            }
            catch (Exception)
            {
                mainApp.ComunicationSystem.LOGInformation.Enqueue("Excepcion al grabar eventos. Continuando");
            }


           // Aplicacion.semaforo_GPS.Release();

        }

        /// <summary>
        /// Volcado de las estructuras de datos en RAM al disco
        /// </summary>
        /// <param name="dataSource"></param>
        public void updateData(string dataSource)
        {

            // hace un update de los datos en el sistema de persistencia.
        }
        public Dictionary<string, string> getCamposLenelCamposSistema()
        {
            return camposLenelCamposSistema;
        }

        public Dictionary<string, KeyValuePair<string, string>> getReglasAcceso()
        {
            return reglasAcceso;
        }

        public Dictionary<string, List<string>> getGruposUsuarios()
        {
            return gruposUsuarios;
        }

        public Dictionary<string, List<string>> getGruposDevices()
        {
            return gruposDevices;
        }

        public Dictionary<string, Employee> getListaUsuarios()
        {
            return listaUsuarios;
        }

        public Dictionary<string, device> getListaDevices()
        {
            return listaDevices;
        }

        public Dictionary<string, Acceso> getListaAccesos()
        {
            return listaAccesos;
        }


        public Dictionary<string, Visita> getListaVisitas()
        {
            return listaVisitas;
        }

        public Dictionary<string, List<EventoGPS>> getEventosGPS()
        {
            return listaEventosGPS;
        }

        public Dictionary<string, Zone> getListaZonas()
        {
            return listaZonas;
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

        public void addGrupoUsuarios(string id_grupo, List<string> lstUsuarios)
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

            xdoc.Save(dataSource + @"\\reglasAcceso.xml");
        }

        public void SaveGruposDevices()
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
            xdoc.Save(dataSource + @"\\gruposDevices.xml");
        }

        public void SaveGruposUsuarios()
        {

            XmlDocument xdoc = new XmlDocument();
            XmlElement elementRoot = xdoc.CreateElement("GruposUsuarios");
            xdoc.AppendChild(elementRoot);

            foreach (KeyValuePair<string, List<string>> keyValue in gruposUsuarios)
            {
                XmlElement grupoUsuario = xdoc.CreateElement("GrupoUsuario");
                XmlAttribute att_idGrupo = xdoc.CreateAttribute("id");
                att_idGrupo.Value = keyValue.Key;
                grupoUsuario.Attributes.Append(att_idGrupo);

                foreach (string s in keyValue.Value)
                {
                    XmlElement usuario = xdoc.CreateElement("Usuario");
                    XmlAttribute att_id = xdoc.CreateAttribute("id");
                    att_id.Value = s;
                    usuario.Attributes.Append(att_id);
                    grupoUsuario.AppendChild(usuario);
                }
                elementRoot.AppendChild(grupoUsuario);
            }
            xdoc.Save(dataSource + @"\\gruposUsuarios.xml");
        }

        public void addEventoGPS(string id_hh, EventoGPS v_evento)
        {
            if (!listaEventosGPS.ContainsKey(id_hh))
            {
                listaEventosGPS.Add(id_hh, new List<EventoGPS>());
            }
            
            if (listaEventosGPS.ContainsKey(id_hh))
            {
                List<EventoGPS> eventos = listaEventosGPS[id_hh];
                eventos.Add(v_evento);
                SaveEventosGPS(id_hh);
            }
        }

        public EventoGPS getLastEventoGPS(string HHID)
        {
            EventoGPS retEvento = null;

            if (listaEventosGPS.ContainsKey(HHID))
            {
                List<EventoGPS> lstEventos = listaEventosGPS[HHID];

                retEvento = lstEventos[lstEventos.Count-1]; // Zero based array...
            }
            return retEvento;
        }


        public void updateVirtualGateEvents(EventoGPS GPSDesde, EventoGPS GPSHacia)
        {
            Punto A = new Punto(float.Parse(GPSDesde.Latitud), float.Parse(GPSDesde.Longitud));
            Punto B = new Punto(float.Parse(GPSHacia.Latitud), float.Parse(GPSHacia.Longitud));

            foreach (KeyValuePair<string, Zone> pair in listaZonas)
            {
                foreach (KeyValuePair<string, Zone.GateDefinition> gate in pair.Value.listaPuertas)
                {
                    Punto C = new Punto(float.Parse(gate.Value.from.position.latitude), float.Parse(gate.Value.from.position.longitude));
                    Punto D = new Punto(float.Parse(gate.Value.to.position.latitude), float.Parse(gate.Value.to.position.longitude));

                    if (intersecta(A, B, C, D))
                    {
                        GateAccessType tipoAcceso;

                        tipoAcceso = gate.Value.type;

                        VehicleAccess nuevoAcceso = new VehicleAccess(GPSDesde.HHID, gate.Value.ID, GPSHacia.Hora, tipoAcceso);

                        mainApp.DataManager.addAccesoVehiculo(nuevoAcceso);
                        mainApp.ComunicationSystem.actualizeListViewAccesosVirtualGates = true;

                    }
                }
            }
        }

        
        public void addVisita(string id_visita, Visita v_visita)
        {
            if (!listaVisitas.ContainsKey(id_visita))
            {
                listaVisitas.Add(id_visita, v_visita);
            }

            SaveVisitas();
        }

       /// <summary>
        /// Agrega un acceso a la lista de accesos en memoria.
        /// </summary>
        /// <param name="v_acceso"></param>
        public void addAcceso(Acceso v_acceso)
        {
            Dictionary<String, Acceso>.KeyCollection claves = listaAccesos.Keys;

            string[] clavesStr = new string[claves.Count];
            claves.CopyTo(clavesStr, 0);
            string ultima = clavesStr[claves.Count-1];
            int ultimaInt = int.Parse(ultima);
            string id_acceso = (ultimaInt +1).ToString(); 
            if(!listaAccesos.ContainsKey(id_acceso))
            {
                listaAccesos.Add(id_acceso,v_acceso);
            }
            SaveAccesos();
        }



        public void addDevice(string id_Device, device v_device)
        {
            if (listaDevices.ContainsKey(id_Device))
            {
                listaDevices.Remove(id_Device);
                listaDevices.Add(id_Device, v_device);
            }
            else
            {
                listaDevices.Add(id_Device, v_device);
            }

            SaveDevices();
        }

        public void removeDevice(string id_Device)
        {
            if (listaDevices.ContainsKey(id_Device))
            {
                listaDevices.Remove(id_Device);
                SaveDevices();
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


        public void addUser(string idUser, Employee v_usuario)
        {
            if (listaUsuarios.ContainsKey(idUser))
            {
                listaUsuarios.Remove(idUser);
                listaUsuarios.Add(idUser, v_usuario);
            }
            else
            {
                listaUsuarios.Add(idUser, v_usuario);
            }

            SaveEmpleados();            // Guarda los datos en el disco
        }


        public void addZone(string v_idZone, Zone v_Zone)
        {
            listaZonas.Add(v_idZone, v_Zone);
        }

        public void deleteZone(string v_zone)
        {
            if (listaZonas.ContainsKey(v_zone))
            {
                listaZonas.Remove(v_zone);
            }
        }

        public void addAccesoVehiculo(VehicleAccess v_acceso)
        {
            listaAccesosVehiculos.Add(v_acceso);
        }

        public List<VehicleAccess> getAccesosVehiculos()
        {
            return listaAccesosVehiculos;
        }

    }
}

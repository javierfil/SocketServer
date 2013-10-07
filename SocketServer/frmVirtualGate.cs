using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;

namespace ServerComunicacionesALUTEL
{
    public partial class frmVirtualGate : Form
    {
        private Aplicacion mainApp;

        private bool flagAddmarker = false;
        
        // Diccionario temporal de puntos (nombre y ubicacion) usados para definir una zona.
        private Dictionary <string, Zone.GeoCoord> markerPoints = new Dictionary<string,Zone.GeoCoord>();

        Zone.GeoCoord actualPoint = new Zone.GeoCoord();    // Punto actualmente seleccionado.

        string actualZone = "";
        string actualGate = "";

        int actualPointID = 0;

        /// <summary>
        /// Constructor del formulario. Se crea el link con mainApp para acceder a todo el resto del sistema
        /// </summary>
        /// <param name="v_app"></param>
        public frmVirtualGate(Aplicacion v_app)
        {
            mainApp = v_app;
            InitializeComponent();
        }

        /// <summary>
        /// Load: Se inicializa el mapa y los estados iniciales de todos los controles.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmVirtualGate_Load(object sender, EventArgs e)
        {
            initializeHHelds();
            initializeMap();
            tmrUpdate.Interval = 200;
            tmrUpdate.Enabled = true;
            mainApp.ComunicationSystem.actualizeListViewAccesosVirtualGates = true;
            actualizeListViewAccesosVirtualGates();
            actualizarListViewZones();
        }

        private void initializeMap()
        {
            // Cargo el mapa con las coordenadas de la ultima seleccion registrada en la ventana main
            string lat = mainApp.formInicial.generalLat;
            string lng = mainApp.formInicial.generalLong;

            string zoom = mainApp.formInicial.generalZoom;

            StreamReader streamReader = new StreamReader(@"MapZonas.html");
            string text = streamReader.ReadToEnd();
            streamReader.Close();

            text = text.Replace(@"[ZOOM]", zoom);
            text = text.Replace(@"[LAT]", lat);
            text = text.Replace(@"[LONG]", lng);

            String PersonalFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            StreamWriter writer = new StreamWriter(@PersonalFolder + @"\\Map.html");

            writer.Write(text);
            writer.Close();
            webBrowser2.Navigate(@PersonalFolder + @"/Map.html");
        }

        private void initializeHHelds()
        {
            // Lleno el combo de handHelds con los ID de los Hhelds
            cmbHandHeld.Items.Clear();
            Dictionary<string, device> listadevices = mainApp.DataManager.getListaDevices();

            foreach (KeyValuePair<string, device> pair in listadevices)
            {
                cmbHandHeld.Items.Add(pair.Key);
            }

        }

        private void btnAddPoint_Click(object sender, EventArgs e)
        {

            if (flagAddmarker)
            {
                btnAddPoints.BackColor = Color.LightGray;
            }
            else
            {
                btnAddPoints.BackColor = Color.Red;
                deleteAllMarkers();
            }
            flagAddmarker = !flagAddmarker;
        }

        private void webBrowser2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlDocument Document = webBrowser2.Document;

            HtmlElementEventHandler mouseDownEvent = new HtmlElementEventHandler(EventoClickMouse);
            Document.Body.MouseDown += mouseDownEvent;
        }

// Evento general de atencion de eventos de click sobre el mapa. 
// Las distintas funciones a cumplir vienen definidas por flags externos: flagAddMarker, etc.
        public void EventoClickMouse(object sender, HtmlElementEventArgs e)
        {
            object[] args = { };

            string res = (string)webBrowser2.Document.InvokeScript("actualCoords", args);

            this.Text = "La posicion es: " + res;
            
            // Hizo un click para agregar un punto
            if (flagAddmarker)
            {
                actualPointID++;

                string idNuevoPunto = "P" + actualPointID.ToString();
                string nuevoPunto = (string)webBrowser2.Document.InvokeScript("actualCoords");

                Zone.GeoCoord coordNuevoPunto = stringCoordToGeoCoord(nuevoPunto);

                markerPoints.Add(idNuevoPunto, coordNuevoPunto);
                actualPoint = coordNuevoPunto;

                // Actualiza el ListView de los puntos de la zona.
                actualizarListaPuntos();
                actualizarMarkersEnMapa(markerPoints);
            }
        }

        private void actualizarMarkersEnMapa(Dictionary<string,Zone.GeoCoord> markers)
        {
            string coordArray = "";

            foreach (KeyValuePair<string, Zone.GeoCoord> par in markers)
            {
                coordArray = coordArray + par.Value.latitude + "," + par.Value.longitude + ",";
            }
            object[] args = { coordArray };

            string ret = (string)webBrowser2.Document.InvokeScript("verMarkers",args);
        }

        private void btnCrearZona_Click(object sender, EventArgs e)
        {
            string nombreZona = Microsoft.VisualBasic.Interaction.InputBox("Nombre de la zona: ", "Crear nueva zona", "", 100, 100);

            if (nombreZona != "")
            {
                Zone nuevaZona = new Zone(nombreZona);
                Zone.GateDefinition nuevaGate = new Zone.GateDefinition(); ;
                nuevaZona.listaPuertas = new Dictionary<string,Zone.GateDefinition>();
                string firstLat = "";
                string firstLong = "";
                string firstID ="";
                bool primerPunto = true;

                foreach (KeyValuePair<string, Zone.GeoCoord> pair in markerPoints)
                {
                    if (primerPunto)
                    {
                        nuevaGate.from = new Zone.ZonePoint("Desde", pair.Value.latitude, pair.Value.longitude);
                        nuevaGate.type = GateAccessType.Forbidden;
                        //nuevaGate.ID = pair.Key;
                        firstLat = pair.Value.latitude;
                        firstLong = pair.Value.longitude;
                        firstID = pair.Key;
                        primerPunto=false;
                    }
                    else
                    {
                        nuevaGate.to = new Zone.ZonePoint("Hacia", pair.Value.latitude, pair.Value.longitude);
                        nuevaGate.ID = nombreZona + "- Gate " + (nuevaZona.listaPuertas.Count+1).ToString();
                        nuevaZona.listaPuertas.Add(nuevaGate.ID,nuevaGate);
                        nuevaGate = new Zone.GateDefinition();
                        nuevaGate.from = new Zone.ZonePoint("Desde", pair.Value.latitude, pair.Value.longitude);
                        nuevaGate.type = GateAccessType.Forbidden;
                        nuevaGate.ID = pair.Key;
                    }
                }
                // La ultima puerta se genera entre el ultimo punto y el primero.
                nuevaGate.to = nuevaGate.to = new Zone.ZonePoint("Hacia", firstLat,firstLong);
                nuevaGate.ID = nombreZona + "- Gate " + (nuevaZona.listaPuertas.Count+1).ToString();
                
                nuevaZona.listaPuertas.Add(nuevaGate.ID,nuevaGate);

                mainApp.DataManager.addZone(nombreZona, nuevaZona);
                actualizarZonasEnMapa(mainApp.DataManager.getListaZonas());

                actualizarListViewZones();
                actualizarListViewGates(nombreZona);
                deleteAllMarkers();
                actualZone = nombreZona;
                flagAddmarker = true;
                btnAddPoint_Click(null, null);
            }
        }
        /// <summary>
        /// "" para borrar el listvies
        /// </summary>
        /// <param name="v_nombreZona"></param>
        private void actualizarListViewGates(string v_nombreZona)
        {

            //if (v_nombreZona == "")
            //{
            //    listViewGates.Items.Clear();
            //}
            //else
            //{

            //    Dictionary<string, Zone> listaZonas = mainApp.DataManager.getListaZonas();
            //    Zone zona = listaZonas[v_nombreZona];

            //    listViewGates.View = View.Details;
            //    listViewGates.GridLines = true;
            //    listViewGates.Columns.Clear();
            //    listViewGates.Columns.Add("Gate definition", 120, HorizontalAlignment.Left);
            //    listViewGates.Columns.Add("Access", 60, HorizontalAlignment.Left);
            //    listViewGates.Items.Clear();

            //    foreach (KeyValuePair<string,Zone.GateDefinition> puerta in zona.listaPuertas)
            //    {
            //        ListViewItem list;
            //        list = listViewGates.Items.Add(puerta.Value.ID);
            //        list.SubItems.Add(puerta.Value.type.ToString());
            //    }

            //    this.listViewGates.MultiSelect = true;
            //    this.listViewGates.HideSelection = false;
            //    this.listViewGates.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            //}

        }

        private void actualizarListViewZones()
        {
            Dictionary<string,Zone> listaZonas =  mainApp.DataManager.getListaZonas();

            listViewZones.View = View.Details;
            listViewZones.GridLines = true;
            listViewZones.Columns.Clear();
            listViewZones.Columns.Add("Zone Name", 160, HorizontalAlignment.Left);
            listViewZones.Items.Clear();

            foreach (string nombre in listaZonas.Keys)
            {
                ListViewItem list;
                list = listViewZones.Items.Add(nombre);
            }

            this.listViewZones.MultiSelect = true;
            this.listViewZones.HideSelection = false;
            this.listViewZones.HeaderStyle = ColumnHeaderStyle.Nonclickable;

        }


        /// <summary>
        /// Serializa los datos de las zonas definidas para pasarselo al javascript del Mapa que las dibuja directamente.
        /// Los datos pasados son: lat,long,lat,long,COLOR,lat,long,lat,long,COLOR...| (zona 2) | (zona 3)
        /// </summary>
        /// <param name="listaZonas"></param>
        private void actualizarZonasEnMapa(Dictionary<string, Zone> listaZonas)
        {

            string coordArray = "";
            string gateColor = "";
            foreach (KeyValuePair<string, Zone> pair in listaZonas)
            {
                foreach (KeyValuePair<string,Zone.GateDefinition> puerta in pair.Value.listaPuertas)
                {
                    coordArray = coordArray + puerta.Value.from.position.latitude + "," + puerta.Value.from.position.longitude + "," + puerta.Value.to.position.latitude + "," + puerta.Value.to.position.longitude;
                    if (puerta.Value.type ==GateAccessType.Granted)
                    {
                        gateColor = Zone.GateColorAccessGranted;
                    }
                    else
                    {
                        gateColor = Zone.GateColorAccessDenied;
                    }
                    coordArray = coordArray + "," + gateColor + ",";
                }
                coordArray = coordArray.Substring(0, coordArray.Length - 1);
                coordArray = coordArray + "|";
            }
            if (coordArray.Length > 0)          // Caso de borde con 0 zonas
            {
                coordArray = coordArray.Substring(0, coordArray.Length - 1);
            }

            // LLama al javascript() para dibujar las zonas en el mapa.

            object[] args = { coordArray };

            string ret = (string)webBrowser2.Document.InvokeScript("verZonas", args);
            //Form.ActiveForm.Text = "Cantidad de gates: " + ret;

        }
        // Convesion de string a Coordenadas Geometricas.
        private Zone.GeoCoord stringCoordToGeoCoord(string v_punto)
        {
            Zone.GeoCoord result = new Zone.GeoCoord();

            string[] coords = v_punto.Split(',');

            result.latitude = coords[0].Substring(1);
            result.longitude = coords[1].Substring(0, coords[1].Length - 1);
            return result;
        }

        // Actualiza la lista de puntos actuales: tempPoints
        public void actualizarListaPuntos()
        {
            listViewPoints.View = View.Details;
            listViewPoints.GridLines = true;
            listViewPoints.Columns.Clear();
            listViewPoints.Columns.Add("ID", 40, HorizontalAlignment.Left);
            listViewPoints.Columns.Add("Latitude", 80, HorizontalAlignment.Left);
            listViewPoints.Columns.Add("Longitude", 80, HorizontalAlignment.Left);
            listViewPoints.Items.Clear();

            foreach (KeyValuePair<string,Zone.GeoCoord> pair in markerPoints)
            {
                ListViewItem list;
                list = listViewPoints.Items.Add(pair.Key);
                list.SubItems.Add(pair.Value.latitude);
                list.SubItems.Add(pair.Value.longitude);
            }
            this.listViewPoints.MultiSelect = true;
            this.listViewPoints.HideSelection = false;
            this.listViewPoints.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        private void listViewPoints_SelectedIndexChanged(object sender, EventArgs e)
        {

            ListView.SelectedListViewItemCollection itemCollection = listViewPoints.SelectedItems;

            if (itemCollection.Count > 0)
            {
                ListViewItem item = itemCollection[0];

                actualPointID = int.Parse(item.SubItems[0].Text.Substring(1));
                
                string lat = item.SubItems[1].Text;
                string lng = item.SubItems[2].Text;

                string jsArray = lat + "," + lng;           // Serializa los datos..

                object[] args2 = { jsArray };
                
                string ret = (string)webBrowser2.Document.InvokeScript("resaltarPunto", args2);

                Form.ActiveForm.Text = "El valor retornado es: " + ret;
            }
        }

        private void btnDeletePoint_Click(object sender, EventArgs e)
        {
            string idPunto = "P" + actualPointID.ToString();
            //object[] args2 = { idPunto };
            //string res = (string)webBrowser2.Document.InvokeScript("borrarMarker", args2);
            //Form.ActiveForm.Text = res;
            markerPoints.Remove(idPunto);
            actualizarListaPuntos();
            actualizarMarkersEnMapa(markerPoints);
            webBrowser2.Document.InvokeScript("noResaltarPunto");

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            object[] args = { };

            Form.ActiveForm.Text = "El indice es: " + webBrowser2.Document.InvokeScript("resetearTrack", args);

        }

        private void cmbHandHeld_SelectedIndexChanged(object sender, EventArgs e)
        {
            string HHID = cmbHandHeld.Text;
            Dictionary<string, List<EventoGPS>> listaEventos = mainApp.DataManager.getEventosGPS();

            List<EventoGPS> eventos = listaEventos[HHID];

            int maxValue = eventos.Count-1;
            trStart.Minimum = 0;
            trStart.Maximum = maxValue;
            trStart.Value = 0;

            trEnd.Minimum = 0;
            trEnd.Maximum = maxValue;
            trEnd.Value = maxValue;

            actualizarTrackEnMapa(HHID,true, optOptimize.Checked);
        }

        private void actualizarTrackEnMapa(string v_HHID, bool actMap, bool optimize)
        {
            Dictionary<string, List<EventoGPS>> listaEventos = mainApp.DataManager.getEventosGPS();
            webBrowser2.Document.InvokeScript("resetearTrack");

            int cantTracks = 0;


            if (listaEventos.ContainsKey(v_HHID))
            {
                List<EventoGPS> eventos = listaEventos[v_HHID];

                string coordArray = "";
                EventoGPS eventoAnterior = null;


                List<EventoGPS> eventos2 = new List<EventoGPS>();
                int eventStart = trStart.Value;
                int eventEnd = trEnd.Value;

                int eventCount = 0;
                if (eventStart <= eventEnd)
                {
                    foreach (EventoGPS evento in eventos)
                    {
                        if (eventCount >= eventStart && eventCount <= eventEnd)
                        {
                            eventos2.Add(evento);
                        }
                        eventCount++;

                    }

                    foreach (EventoGPS evento in eventos2)
                    {
                        if (!dividirTrack(eventoAnterior, evento) || !optimize)
                        {
                            coordArray = coordArray + evento.Latitud + "," + evento.Longitud + ",";
                        }
                        else
                        {
                            coordArray = coordArray + "|" + evento.Latitud + "," + evento.Longitud + ",";

                            cantTracks++;
                        }
                        eventoAnterior = evento;
                    }
                    // Le saco la ultima coma del final
                    coordArray = coordArray.Substring(0, coordArray.Length - 1);

                    // Borro todos los tracks previos
                    webBrowser2.Document.InvokeScript("resetearTodosTrack");

                    // Agrego cada segmento al mapa.
                    string[] listaTracks = coordArray.Split(new char[] { '|' });
                    foreach (string track in listaTracks)
                    {
                        string elTrack = track.Substring(0, track.Length - 1);
                        object[] args = { elTrack };
                        webBrowser2.Document.InvokeScript("agregarTrack", args);
                    }
                    if (actMap)
                    {
                        object[] args2 = { eventos2[eventos2.Count - 1].Latitud, eventos2[eventos2.Count - 1].Longitud };

                        //// Centrar el mapa
                        webBrowser2.Document.InvokeScript("centrarMapa", args2);
                    }
                }
            }
        }
        /// <summary>
        /// Analiza la diferencia de espacio y horario entre los dos eventos GPS e indica si el track se separó o no.
        /// El criterio para determinarlo es que la velocidad calculada entre los dos eventos no sea mayor a MAXVEL kilometros por hora.
        /// </summary>
        /// <param name="evento1"></param>
        /// <param name="evento2"></param>
        /// <returns></returns>
        private bool dividirTrack(EventoGPS evento1, EventoGPS evento2)
        {
            bool res = false;
            if (evento1 != null && evento2 != null)
            {
                DateTime hora1 = DateTime.Parse(evento1.Hora, CultureInfo.InvariantCulture.DateTimeFormat);
                DateTime hora2 = DateTime.Parse(evento2.Hora, CultureInfo.InvariantCulture.DateTimeFormat);

                TimeSpan delta = hora1.Subtract(hora2);
                float horas = (float)Math.Abs(delta.TotalHours);

                float lat1 = float.Parse(evento1.Latitud, CultureInfo.InvariantCulture.NumberFormat);
                float lat2 = float.Parse(evento2.Latitud, CultureInfo.InvariantCulture.NumberFormat);
                float deltaLat = Math.Abs(lat1 - lat2);

                float long1 = float.Parse(evento1.Longitud, CultureInfo.InvariantCulture.NumberFormat);
                float long2 = float.Parse(evento2.Longitud, CultureInfo.InvariantCulture.NumberFormat);
                float deltaLong = Math.Abs(long1 - long2);

                float distance = (float)Math.Sqrt(deltaLat * deltaLat + deltaLong * deltaLong);

                float distanceKiloMetros = ((distance / 0.0006f) * 50) / 1000.0f;       // Distancia en kilometros

                float vel = distanceKiloMetros / horas;

                if (vel > Zone.MAXVEL)
                {
                   // MessageBox.Show("vel = " + vel.ToString());
                }

                if (distanceKiloMetros > 0.5)
                {
                   // MessageBox.Show("distanceKiloMetros = " + distanceKiloMetros.ToString());
                }

                return (vel > Zone.MAXVEL)||(distanceKiloMetros > 0.5);     // Es la velocidad mayor que la permitida? o es la distancia > que la permitida?
            }
            return res;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDeleteAll_Click(object sender, EventArgs e)
        {
            deleteAllMarkers();
        }

        private void deleteAllMarkers()
        {
            webBrowser2.Document.InvokeScript("deleteAllMarkers");
            markerPoints.Clear();
            actualPointID = 0;
            actualizarListaPuntos();
            webBrowser2.Document.InvokeScript("noResaltarPunto");
        }
        private void listViewZones_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection itemCollection = listViewZones.SelectedItems;
            if (itemCollection.Count > 0)
            {
                ListViewItem item = itemCollection[0];
                actualZone = item.SubItems[0].Text;

                actualizarListViewGates(actualZone);

                Dictionary<string,Zone.GateDefinition> listapuertas = mainApp.DataManager.getListaZonas()[actualZone].listaPuertas;

                string latGate1 = "";
                string longGate1 = "";
                foreach(KeyValuePair<string,Zone.GateDefinition> puerta in listapuertas)
                {
                    latGate1 = puerta.Value.from.position.latitude;
                    longGate1 = puerta.Value.from.position.longitude;
                    break;

                }
                actualizarZonasEnMapa(mainApp.DataManager.getListaZonas());

                object[] args2 = { latGate1, longGate1 };

                //// Centrar el mapa
                webBrowser2.Document.InvokeScript("centrarMapa", args2);

                deleteAllMarkers();
            }
        }

        private void listViewGates_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection itemCollection = listViewGates.SelectedItems;
            if (itemCollection.Count > 0)
            {
                ListViewItem item = itemCollection[0];
                actualGate = item.SubItems[0].Text;

                actualizarGateEnMapa(actualZone,actualGate);
            }
        }

        private void actualizarGateEnMapa(string v_zone, string v_gate)
        {
            Dictionary<string, Zone> zonas = mainApp.DataManager.getListaZonas();

            if (zonas.ContainsKey(v_zone))
            {
                Zone.GateDefinition selectedGate = zonas[v_zone].listaPuertas[v_gate];

                Dictionary<string, Zone.GeoCoord> pointsInGate = new Dictionary<string, Zone.GeoCoord>();
                pointsInGate.Add(selectedGate.ID + "1", new Zone.GeoCoord(selectedGate.from.position.latitude, selectedGate.from.position.longitude));
                pointsInGate.Add(selectedGate.ID + "2", new Zone.GeoCoord(selectedGate.to.position.latitude, selectedGate.to.position.longitude));
                actualizarMarkersEnMapa(pointsInGate);
            }
        }

        private void btnGrant_Click(object sender, EventArgs e)
        {
            //Dictionary<string, Zone> zonas = mainApp.DataManager.getListaZonas();

            //if (zonas.ContainsKey(actualZone))
            //{
            //    zonas[actualZone].listaPuertas[actualGate].type = GateAccessType.Granted;
            //    actualizarZonasEnMapa(mainApp.DataManager.getListaZonas());
            //    actualizarListViewGates(actualZone);
            //    mainApp.DataManager.SaveZonas();
            //}
        }

        private void btnDeleteZone_Click(object sender, EventArgs e)
        {
            if (actualZone != "")
            {
                DialogResult res = MessageBox.Show(this, "Delete zone " + actualZone + "?", "Delete Zone", MessageBoxButtons.YesNo);
                if (res == DialogResult.Yes)
                {
                    mainApp.DataManager.deleteZone(actualZone);
                    actualZone = "";
                    actualizarListViewZones();
                    actualizarListViewGates("");
                    actualizarZonasEnMapa(mainApp.DataManager.getListaZonas());


                }
            }
        }

        private void btnDeny_Click(object sender, EventArgs e)
        {
            Dictionary<string, Zone> zonas = mainApp.DataManager.getListaZonas();

            if (zonas.ContainsKey(actualZone))
            {
                zonas[actualZone].listaPuertas[actualGate].type = GateAccessType.Forbidden;
                actualizarZonasEnMapa(mainApp.DataManager.getListaZonas());
                actualizarListViewGates(actualZone);
            }
        }

        private void tmrUpdate_Tick(object sender, EventArgs e)
        {
            if (mainApp.ComunicationSystem.actualizeListViewAccesosVirtualGates)
            {
                mainApp.ComunicationSystem.actualizeListViewAccesosVirtualGates = false;
                actualizeListViewAccesosVirtualGates();
            }
        }

        public void actualizeListViewAccesosVirtualGates()
        {

            listViewAccesosVirtualGate.View = View.Details;
            listViewAccesosVirtualGate.GridLines = true;
            listViewAccesosVirtualGate.Columns.Clear();
            listViewAccesosVirtualGate.Columns.Add("GPS Device", 100, HorizontalAlignment.Left);
            listViewAccesosVirtualGate.Columns.Add("Zone ID", 100, HorizontalAlignment.Left);
            listViewAccesosVirtualGate.Columns.Add("Gate Id", 100, HorizontalAlignment.Left);
            listViewAccesosVirtualGate.Columns.Add("Time", 100, HorizontalAlignment.Left);
            listViewAccesosVirtualGate.Columns.Add("Type", 80, HorizontalAlignment.Left);
            listViewAccesosVirtualGate.Items.Clear();

            List<ZoneAccess> listaAccesos = mainApp.DataManager.getAccesosZonas();

            foreach (ZoneAccess acceso in listaAccesos)
            {
                ListViewItem list;
                list = listViewAccesosVirtualGate.Items.Add(acceso.HHID);
                list.SubItems.Add(acceso.ZoneID);
                list.SubItems.Add(acceso.GateID);
                list.SubItems.Add(acceso.Hora);
                list.SubItems.Add(acceso.tipoAcceso.ToString());
            }
            this.listViewAccesosVirtualGate.MultiSelect = true;
            this.listViewAccesosVirtualGate.HideSelection = false;
            this.listViewAccesosVirtualGate.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //actualizarZonasEnMapa(mainApp.DataManager.getListaZonas());


            string coordArray = "1,2,3,4,#FF00FF,20,20,30,30,#FF00FF";

            // LLama al javascript() para dibujar las zonas en el mapa.

            object[] args = { coordArray };

            string ret = (string)webBrowser2.Document.InvokeScript("verZonas", args);
            //Form.ActiveForm.Text = "Cantidad de gates: " + ret;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string latitud="-34.89639";
            string longitud = "-56.1822";
            string hora = "2012-10-16 14:43:01";
            string HHID = "Nacho";

            EventoGPS nuevoEvento = new EventoGPS(HHID, latitud, longitud,hora );

            EventoGPS ultimoEvento = mainApp.DataManager.getLastEventoGPS(HHID);

            mainApp.DataManager.addEventoGPS(HHID, nuevoEvento);

            if (ultimoEvento != null)
            {
                mainApp.DataManager.updateVirtualGateEvents(ultimoEvento, nuevoEvento);
            }
            actualizarTrackEnMapa(HHID,true,true);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string latitud = "-34.89664";
            string longitud = "-56.18188";
            string hora = "2012-10-16 15:41:30";
            string HHID = "Nacho";

            EventoGPS nuevoEvento = new EventoGPS(HHID, latitud, longitud, hora);

            EventoGPS ultimoEvento = mainApp.DataManager.getLastEventoGPS(HHID);

            mainApp.DataManager.addEventoGPS(HHID, nuevoEvento);

            if (ultimoEvento != null)
            {
                mainApp.DataManager.updateVirtualGateEvents(ultimoEvento, nuevoEvento);
            }
            actualizarTrackEnMapa(HHID,true,true);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // latitud="-34.89684" longitud="-56.18095" hora="2012-10-16 15:41:34"
            string latitud = "-34.89684";
            string longitud = "-56.18095";
            string hora = "2012-10-16 15:41:30";
            string HHID = "Nacho";

            EventoGPS nuevoEvento = new EventoGPS(HHID, latitud, longitud, hora);

            EventoGPS ultimoEvento = mainApp.DataManager.getLastEventoGPS(HHID);

            mainApp.DataManager.addEventoGPS(HHID, nuevoEvento);

            if (ultimoEvento != null)
            {
                mainApp.DataManager.updateVirtualGateEvents(ultimoEvento, nuevoEvento);
            }

            actualizarTrackEnMapa(HHID,true,true);
        }

        private void trStart_Scroll(object sender, EventArgs e)
        {
            string HHID = cmbHandHeld.Text;
            actualizarTrackEnMapa(HHID,false,optOptimize.Checked);
        }

        private void trEnd_Scroll(object sender, EventArgs e)
        {
            string HHID = cmbHandHeld.Text;
            actualizarTrackEnMapa(HHID,true, optOptimize.Checked);
        }

        private void optOptimize_CheckedChanged(object sender, EventArgs e)
        {
            string HHID = cmbHandHeld.Text;
            actualizarTrackEnMapa(HHID, false, optOptimize.Checked);
        }

        private void listViewAccesosVirtualGate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

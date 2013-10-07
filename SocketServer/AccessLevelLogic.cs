using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public  class AccessLevelLogic
    {
        private static  Aplicacion mainApp;
        public static Dictionary<int, LENELTimeZones> ListaTimeZones;                      // Coleccion de LENELTimeZones indexada por idOrganizacion
        public static Dictionary<int, List<HolidayData>> ListaHolidays;                   // Lista de definiciones de Holidays indexada por idOrganizacion. Son las mismas para todos los panels de la organizacion
        public static Dictionary<int, LENELAccessLevels> ListaAccessLevels;               // Coleccion de LENELAccessLevels indexada por idOrganizacion
        public static Dictionary<int, LENELBadgeAccessLevels> ListaBadgeAccessLevels;   


        public static void LoadAccessLevels(Aplicacion App)
        {    
            mainApp = App;           
            ListaHolidays= mainApp.DataManager.LoadListaHolidays();
            ListaTimeZones = mainApp.DataManager.LoadListaTimeZones();
            ListaAccessLevels = mainApp.DataManager.LoadListaAccessLevels();
            ListaBadgeAccessLevels = mainApp.DataManager.LoadListaBadgeAccessLevels();           
        }
        public static void LoadAccessLevels()
        {
            ListaHolidays = mainApp.DataManager.LoadListaHolidays();
            ListaTimeZones = mainApp.DataManager.LoadListaTimeZones();
            ListaAccessLevels = mainApp.DataManager.LoadListaAccessLevels();
            ListaBadgeAccessLevels = mainApp.DataManager.LoadListaBadgeAccessLevels();
        }


        public static List<HolidayData> ListaHolidaysPorOrganizacion(int OrgId)
        {
            return ListaHolidays[OrgId];
        }

        public static void addTimezone(int organizationID, int TZNumber, string v_IntervalData)
        {

            if (!ListaTimeZones.ContainsKey(organizationID))
            {
                LENELTimeZones nuevaLNLTimeZone = new LENELTimeZones(organizationID);
                ListaTimeZones.Add(organizationID, nuevaLNLTimeZone);
            }
            LENELTimeZones LNLTimeZone = ListaTimeZones[organizationID];

            LNLTimeZone.createTimeZoneData(TZNumber, v_IntervalData);

            mainApp.DataManager.addLenelTimeZone(LNLTimeZone, organizationID, TZNumber);
        }

        /// <summary>
        /// Agrega un accessLevel a la organizacion
        /// </summary>
        /// <param name="orgID"></param>
        /// <param name="LNLpanelID"></param>
        /// <param name="accessLevelID"></param>
        /// <param name="TZReaderData"></param>
        public static void addAccessLevel(int orgID, int LNLpanelID, int accessLevelID, string TZReaderData)
        {

            LENELAccessLevels LNLACC;
            if (!ListaAccessLevels.ContainsKey(orgID))
            {
                LNLACC = new LENELAccessLevels(orgID);
                ListaAccessLevels.Add(orgID, LNLACC);
            }
            LNLACC = ListaAccessLevels[orgID];
            LNLACC.createAccessLevel(LNLpanelID, accessLevelID, TZReaderData);
         
            // Alta en la BD
            mainApp.DataManager.addLenelAccesLevel(LNLACC, LNLpanelID, accessLevelID, orgID);

        }


        /// <summary>
        /// Agrega todas las  holidays a una  idOrganization
        /// </summary>
        /// <param name="LNL_PanelID"></param>
        /// <param name="holidaysData"></param>
        public static void addLenelHolidays(string v_idOrganization, string holidaysData)
        {
            string[] holidayDef = holidaysData.Split('|');
            int idOrganization = int.Parse(v_idOrganization);

            List<HolidayData> LENELHolidays;
            if (!ListaHolidays.ContainsKey(idOrganization))
            {
                LENELHolidays = new List<HolidayData>();
                ListaHolidays.Add(idOrganization, LENELHolidays);
            }
            LENELHolidays = ListaHolidays[idOrganization];    // Obtiene las holidays de la organizacion
            LENELHolidays.Clear();
            foreach (string holiday in holidayDef)
            {
                if (holiday.Trim().Length > 0)
                {
                    HolidayData holy = new HolidayData(holiday);
                    LENELHolidays.Add(holy);
                }
            }
            // Alta en la BD
            mainApp.DataManager.addLenelHoliday(idOrganization, LENELHolidays);
        }

        public static string getAccessLevelsByBadgeInHH(string v_badge, string v_HHID)
        {
            string resultado = "";
            try
            {
                string panelIDstr = mainApp.DataManager.ObtenerLenelPanelID(v_HHID);
                int orgID = mainApp.DataManager.obtenerOrganizationIDFromHHID(v_HHID);

                if ((panelIDstr.Length > 0) && (orgID > 0))
                {
                    LENELAccessLevels LNACC = ListaAccessLevels[orgID];

                    int panelID = int.Parse(panelIDstr);

                    LENELBadgeAccessLevels LBACC = ListaBadgeAccessLevels[orgID];

                    string accessLevels = LBACC.getAccessLevels(v_badge);

                    if (accessLevels != "")
                    {
                        string[] ACCIDs = accessLevels.Split(',');
                        AccessLevelData accessLevel = LNACC.getAccessLevel(panelID);

                        foreach (string accessID in ACCIDs)
                        {
                            if (accessLevel != null)
                            {
                                if (accessLevel.ReaderTZ.ContainsKey(int.Parse(accessID)))
                                {
                                    if (accessLevel.ReaderTZ[int.Parse(accessID)].Count > 0)
                                    {
                                        resultado = resultado + accessID + ",";
                                    }
                                    if (tieneNeverTZ(accessLevel.ReaderTZ[int.Parse(accessID)]))    // Timezone correspondiente a NEVER
                                    {
                                        return "";
                                    }
                                }
                            }
                        }
                    }
                }
                if (resultado.Length > 0)
                {
                    resultado = resultado.Substring(0, resultado.Length - 1);
                }
            }

            catch 
            {
                throw new Exception("Excepcion en getAccesslevelsByBadgeInHH: ");
            }
            return resultado;
        }

        /// <summary>
        /// Devuelve true si el diccionario de ReaderTZ tiene la TZ(Value) 1.
        /// </summary>
        /// <param name="v_ReaderTZ"></param>
        /// <returns></returns>
        public static bool tieneNeverTZ(Dictionary<int, int> v_ReaderTZ)
        {
            bool res = false;
            foreach (KeyValuePair<int, int> R_TZ in v_ReaderTZ)
            {
                if (R_TZ.Value == 1)
                {
                    res = true;
                    break;
                }
            }
            return res;

        }

        /// <summary>
        /// Devuelve el string completo de definicion correspondiente a un TimeZone
        /// </summary>
        /// <param name="v_PanelID"></param>
        /// <param name="v_Timezone"></param>
        /// <param name="v_Entrada"></param>
        /// <param name="v_Salida"></param>
        /// <returns></returns>
        public static string getStrAccess(int v_organizationID, int v_ACCID, int v_PanelID, int v_Timezone, bool v_Entrada, bool v_Salida)
        {
            string res = v_ACCID.ToString() + ":";
            string resHoli = "";    // Las holidays van al final

            TimeZoneData TZInfo = ListaTimeZones[v_organizationID].getTimeZoneData();

            if (TZInfo.TZDefinition.ContainsKey(v_Timezone))
            {
                List<TZInterval> listaIntervalos = TZInfo.TZDefinition[v_Timezone];

                foreach (TZInterval intervalo in listaIntervalos)
                {
                    string listaDias = intervalo.obtenerListaDias();

                    string Hini = intervalo.horaIni.ToString();
                    string MIni = intervalo.minIni.ToString();
                    string Hfin = intervalo.horaFin.ToString();
                    string MFin = intervalo.minFin.ToString();

                    List<int> tiposHol = intervalo.obtenerListaTiposHoilidays();

                    // Obtengo las holidays de la organizacion
                    List<HolidayData> LENELHolidays = ListaHolidays[v_organizationID];

                    foreach (int tipoToAdd in tiposHol)
                    {
                        foreach (HolidayData holi in LENELHolidays)
                        {
                            if (holi.contieneBit(tipoToAdd))
                            {
                                resHoli = resHoli + "{" + holi.año.ToString() + "-" + holi.mes.ToString() + "-" + holi.dia.ToString() + "," + holi.cantDias.ToString() + "[" + Hini + ":" + MIni + "," + Hfin + ":" + MFin + "," + v_Entrada.ToString() + "," + v_Salida.ToString() + "]}";
                            }
                        }
                    }
                    if (listaDias.Length > 0)
                    {
                        res = res + "{" + listaDias + "[" + Hini + ":" + MIni + "," + Hfin + ":" + MFin + "," + v_Entrada.ToString() + "," + v_Salida.ToString() + "]}";
                    }
                }
            }
            else
            {
               // loguearString("La timezone: " + v_Timezone + " no está definida en el panelID: " + v_PanelID);
            }
            if (resHoli.Length > 0)
            {
                return res + "|" + resHoli;       // Primero los dias y luego las vacaciones.
            }
            else
            {
                return res;                     // no mando el caracter | si no hay vacaciones
            }
        }



    }
}

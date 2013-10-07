using System;
using System.Collections.Generic;
using System.Text;

namespace ServerComunicacionesALUTEL
{
    public class LENELAccessLevels
    {
        Dictionary<int, AccessLevelData> AccessLevels;          // // Coleccion de PANELID con su AccessLevelData (diccionario con accesslevels y lista de Reader y TZnumber)
        int organizationID;

        public LENELAccessLevels(int v_organizacion)
        {
            AccessLevels = new Dictionary<int, AccessLevelData>();
            organizationID = v_organizacion;
        }

        public AccessLevelData getAccessLevel(int v_LNLPanelID)
        {
            if (AccessLevels.ContainsKey(v_LNLPanelID))
            {
                return AccessLevels[v_LNLPanelID];
            }
            else
            {
                return null;
            }
        }

        // Crea desde cero una nueva definicion de accesslevels para ese LNLPanelID
        public void createAccessLevel(int LNLpanelID, int accessLevelID, string TZReaderData)
        {
            if (!AccessLevels.ContainsKey(LNLpanelID))
            {
                AccessLevelData nuevoALData = new AccessLevelData();
                AccessLevels.Add(LNLpanelID, nuevoALData);
            }
            AccessLevelData datosAccessLevel = AccessLevels[LNLpanelID];
            datosAccessLevel.createReaderTZData(accessLevelID, TZReaderData);


        }

        public string obtenerAccessLevelsStringFromPanel(int v_PanelID)
        {
            string res = "";
            if (AccessLevels.ContainsKey(v_PanelID))
            {
                AccessLevelData ALD = AccessLevels[v_PanelID];

                foreach (int key in ALD.ReaderTZ.Keys)
                {
                    res = res + key.ToString() + ",";
                }
            }

            if (res.Length > 0)
            {
                return res.Substring(0, res.Length - 1);            // Le saca la coma al final
            }
            else
            {
                return res;
            }
        }

        // Devuelve el AccessLevelData del panel. Si no est{a lo crea y devuelve la referencia para poder trabajar con ella.
        public AccessLevelData obtenerAccessLevelsFromPanel(int v_PanelID)
        {
            if (!AccessLevels.ContainsKey(v_PanelID))
            {
                AccessLevelData ALD = new AccessLevelData();
                AccessLevels.Add(v_PanelID, ALD);
            }
            return AccessLevels[v_PanelID];;
        }

    }
}

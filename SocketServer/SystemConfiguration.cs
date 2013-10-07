
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace ServerComunicacionesALUTEL
{
    /// <summary>
    /// Clase de configuraciòn general del sistema. Lee del XML AccesoMovilConfig.XML los parametros globales 
    /// y los expone en variables static
    /// 
    /// </summary>
    static class SystemConfiguration
    {
        public static string applicationPath = "";
        public static string ImagesPath = "";
        public static string DataSource = "";
        public static string DataBaseName = "";
        public static string GPSControlIP = "";
        public static int GPSControlPort = 0;
        public static int SendPort = 0;
        public static int ReceivePort = 0;
        public static int OnGuardPort1 = 0;
        public static int OnGuardPort2 = 0;
        public static int AlutrackPort1 = 7981;
        public static int AlutrackPort2 = 7987;


        public static string DBUserName = "";
        public static string DBPassword = "";
        public static string serverType = "TINY";   // Por defecto la version chica. Cualquier otro string lo agranda

        static SystemConfiguration()
        {
            //MessageBox.Show("entra a SystemConfiguration()");
            applicationPath = Application.StartupPath;


            LoadConfiguration();

            //MessageBox.Show("SALE de SystemConfiguration()");
        }

        public static void LoadConfiguration()
        {
            //MessageBox.Show("entra a LoadConfiguration()");
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(applicationPath + @"\\ALUTELServerConfig.xml");

                foreach (XmlElement elem in xDoc.SelectNodes(@"/ConfigParameters/ConfigParameter"))
                {
                    string parameterID = elem.Attributes["id"].Value;

                    switch (parameterID)
                    {
                        case "ImagesPath":
                            ImagesPath = elem.Attributes["value"].Value;
                            break;
                        case "DataSource":
                            DataSource = elem.Attributes["value"].Value;
                            break;
                        case "DataBaseName":
                            DataBaseName = elem.Attributes["value"].Value;
                            break;
                        case "GPSControlIP":
                            GPSControlIP = elem.Attributes["value"].Value;
                            break;
                        case "GPSControlPort":
                            GPSControlPort = Convert.ToInt16(elem.Attributes["value"].Value);
                            break;
                        case "SendPort":
                            SendPort = Convert.ToInt16(elem.Attributes["value"].Value);
                            break;
                        case "ReceivePort":
                            ReceivePort = Convert.ToInt16(elem.Attributes["value"].Value);
                            break;
                        case "OnGuardPort1":
                            OnGuardPort1 = Convert.ToInt16(elem.Attributes["value"].Value);
                            break;
                        case "OnGuardPort2":
                            OnGuardPort2 = Convert.ToInt16(elem.Attributes["value"].Value);
                            break;
                        case "DBUserName":
                            DBUserName = elem.Attributes["value"].Value;
                            break;
                        case "DBPassword":
                            DBPassword = elem.Attributes["value"].Value;
                            break;
                        case "ServerType":
                            serverType = elem.Attributes["value"].Value;

                            break;
                    }
                }

                //if (GPSControlIP == "" || GPSControlPort == 0)
                //{
                //    MessageBox.Show("ATENCION: AlutrakIP/AlutrackPort no fueron definidos. No se enviaran datos GPS al ALUTRACK");
                //}

                if (ImagesPath == "")
                {
                    MessageBox.Show("ATENCION: ImagesPath no fue definido. Las imágenes no se grabarán correctamente en el servidor");
                }

            }
            catch (Exception)
            {
                MessageBox.Show("Warning: Error loading configuration parameters. Please configure the server again", "ERROR");
            }

            //MessageBox.Show("SALE de LoadConfiguration()");


        }

        public static void SaveConfiguration()
        {
            XmlDocument xdoc = new XmlDocument();
            XmlElement elementRoot = xdoc.CreateElement("ConfigParameters");
            xdoc.AppendChild(elementRoot);


            XmlElement infoOnGuardPort1 = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_OnGuardPort1 = xdoc.CreateAttribute("id");
            att_OnGuardPort1.Value = "OnGuardPort1";
            infoOnGuardPort1.Attributes.Append(att_OnGuardPort1);
            XmlAttribute att_OnGuardPort1Value = xdoc.CreateAttribute("value");
            att_OnGuardPort1Value.Value = OnGuardPort1.ToString();
            infoOnGuardPort1.Attributes.Append(att_OnGuardPort1Value);
            elementRoot.AppendChild(infoOnGuardPort1);


            XmlElement infoAlutrackPORT2 = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_PORT2 = xdoc.CreateAttribute("id");
            att_PORT2.Value = "OnGuardPort2";
            infoAlutrackPORT2.Attributes.Append(att_PORT2);
            XmlAttribute att_PORTValue2 = xdoc.CreateAttribute("value");
            att_PORTValue2.Value = OnGuardPort2.ToString();
            infoAlutrackPORT2.Attributes.Append(att_PORTValue2);
            elementRoot.AppendChild(infoAlutrackPORT2);


            XmlElement infoSendPort = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_SendPort = xdoc.CreateAttribute("id");
            att_SendPort.Value = "SendPort";
            infoSendPort.Attributes.Append(att_SendPort);
            XmlAttribute att_SendPortValue = xdoc.CreateAttribute("value");
            att_SendPortValue.Value = SendPort.ToString();
            infoSendPort.Attributes.Append(att_SendPortValue);
            elementRoot.AppendChild(infoSendPort);

            XmlElement infoReceivePort = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_ReceivePort = xdoc.CreateAttribute("id");
            att_ReceivePort.Value = "ReceivePort";
            infoReceivePort.Attributes.Append(att_ReceivePort);
            XmlAttribute att_ReceivePortValue = xdoc.CreateAttribute("value");
            att_ReceivePortValue.Value = ReceivePort.ToString();
            infoReceivePort.Attributes.Append(att_ReceivePortValue);
            elementRoot.AppendChild(infoReceivePort);


            if (!string.IsNullOrEmpty(GPSControlIP))
            {
                XmlElement infoAlutrackIP = xdoc.CreateElement("ConfigParameter");
                XmlAttribute att_IP = xdoc.CreateAttribute("id");
                att_IP.Value = "GPSControlIP";
                infoAlutrackIP.Attributes.Append(att_IP);
                XmlAttribute att_IPValue = xdoc.CreateAttribute("value");
                att_IPValue.Value = GPSControlIP;
                infoAlutrackIP.Attributes.Append(att_IPValue);
                elementRoot.AppendChild(infoAlutrackIP);
            }

            if (GPSControlPort > 0)
            {
                XmlElement infoAlutrackPORT = xdoc.CreateElement("ConfigParameter");
                XmlAttribute att_PORT = xdoc.CreateAttribute("id");
                att_PORT.Value = "GPSControlPort";
                infoAlutrackPORT.Attributes.Append(att_PORT);
                XmlAttribute att_PORTValue = xdoc.CreateAttribute("value");
                att_PORTValue.Value = GPSControlPort.ToString();
                infoAlutrackPORT.Attributes.Append(att_PORTValue);
                elementRoot.AppendChild(infoAlutrackPORT);
            }


            XmlElement infoImagesPath = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_id = xdoc.CreateAttribute("id");
            att_id.Value = "ImagesPath";
            infoImagesPath.Attributes.Append(att_id);
            XmlAttribute att_Value = xdoc.CreateAttribute("value");
            att_Value.Value = ImagesPath;
            infoImagesPath.Attributes.Append(att_Value);
            elementRoot.AppendChild(infoImagesPath);

            XmlElement infoDataSource = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_DSid = xdoc.CreateAttribute("id");
            att_DSid.Value = "DataSource";
            infoDataSource.Attributes.Append(att_DSid);
            XmlAttribute att_DSValue = xdoc.CreateAttribute("value");
            att_DSValue.Value = DataSource;
            infoDataSource.Attributes.Append(att_DSValue);
            elementRoot.AppendChild(infoDataSource);

            XmlElement infoDataBaseName = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_dbName = xdoc.CreateAttribute("id");
            att_dbName.Value = "DataBaseName";
            infoDataBaseName.Attributes.Append(att_dbName);
            XmlAttribute att_dbnameValue = xdoc.CreateAttribute("value");
            att_dbnameValue.Value = DataBaseName;
            infoDataBaseName.Attributes.Append(att_dbnameValue);
            elementRoot.AppendChild(infoDataBaseName);

            XmlElement XmlDBUserName = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_dbUserName = xdoc.CreateAttribute("id");
            att_dbUserName.Value = "DBUserName";
            XmlDBUserName.Attributes.Append(att_dbUserName);
            XmlAttribute att_dbUserNameValue = xdoc.CreateAttribute("value");
            att_dbUserNameValue.Value = DBUserName;
            XmlDBUserName.Attributes.Append(att_dbUserNameValue);
            elementRoot.AppendChild(XmlDBUserName);


            XmlElement XmlDBPassword = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_dbPassword = xdoc.CreateAttribute("id");
            att_dbPassword.Value = "DBPassword";
            XmlDBPassword.Attributes.Append(att_dbPassword);
            XmlAttribute att_dbPasswordValue = xdoc.CreateAttribute("value");
            att_dbPasswordValue.Value = DBPassword;
            XmlDBPassword.Attributes.Append(att_dbPasswordValue);
            elementRoot.AppendChild(XmlDBPassword);

            XmlElement XmlServerType = xdoc.CreateElement("ConfigParameter");
            XmlAttribute att_serverType = xdoc.CreateAttribute("id");
            att_serverType.Value = "ServerType";
            XmlServerType.Attributes.Append(att_serverType);
            XmlAttribute att_serverTypeValue = xdoc.CreateAttribute("value");
            att_serverTypeValue.Value = serverType;
            XmlServerType.Attributes.Append(att_serverTypeValue);
            elementRoot.AppendChild(XmlServerType);

            xdoc.Save(applicationPath + @"\\ALUTELServerConfig.xml");

            //MessageBox.Show("AccesoMovilConfig.XML guardado con éxito en: " + applicationPath);

        }
    }
}

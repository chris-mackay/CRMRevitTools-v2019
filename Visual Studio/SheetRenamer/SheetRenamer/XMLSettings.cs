using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SheetRenamer
{

    public static class XMLSettings
    {
        public static string AppSettingsDir  = Path.Combine(@"C:\Users\", Environment.UserName, @"AppData\Local\CRMRevitTools\v2019\Settings\SheetRenamer");
        public static string AppSettingsFile = Path.Combine(@"C:\Users\", Environment.UserName, @"AppData\Local\CRMRevitTools\v2019\Settings\SheetRenamer\Settings.xml");

        public static bool SettingsFileExists()
        {
            bool flag = false;

            if (File.Exists(AppSettingsFile))
                flag = true;
            else
                flag = false;

            return flag;
        }

        public static string GetSettingsValue(string _Field)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettingsFile);

            XmlNode node = null;
            node = doc.SelectSingleNode(_Field);

            string value = string.Empty;

            if (node == null)
                value = string.Empty;
            else
                value = node.InnerText;

            return value;
        }

        public static void SetSettingsValue(string _Field, string _Value)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(AppSettingsFile);

            if (doc.SelectSingleNode(_Field) == null)
            {
                _Field = _Field.Replace("//Settings/", "");
                XmlNode field = doc.CreateElement(_Field);
                field.InnerText = _Value;
                doc.DocumentElement.AppendChild(field);
                doc.Save(AppSettingsFile);
            }
            else
            {
                XmlNode node = null;
                node = doc.SelectSingleNode(_Field);
                node.InnerText = _Value;
                doc.Save(AppSettingsFile);
            }
        }

        public static void CreateAppSettings_SetDefaults()
        {
            StringCollection appSettings = new StringCollection();

            if (!Directory.Exists(AppSettingsDir))
                Directory.CreateDirectory(AppSettingsDir);

            appSettings.Add("DrawingDirectory," + "");

            if (!SettingsFileExists())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                XmlWriter XmlWrt = XmlWriter.Create(AppSettingsFile, settings);

                {
                    var withBlock = XmlWrt;
                    withBlock.WriteStartDocument();

                    withBlock.WriteComment("Application Settings");
                    withBlock.WriteStartElement("Settings");

                    string[] arr;

                    foreach (string setting in appSettings)
                    {
                        arr = setting.Split(',');

                        string settingName = arr[0];
                        string defaultValue = arr[1];

                        withBlock.WriteStartElement(settingName);
                        withBlock.WriteString(defaultValue);
                        withBlock.WriteEndElement();
                    }

                    withBlock.WriteEndDocument();
                    withBlock.Close();
                }

                XmlWrt = null;
            }
            else
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(AppSettingsFile);
                XmlElement elm = xmlDoc.DocumentElement;
                XmlNodeList lstSettings = elm.ChildNodes;
                string[] arr;
                StringCollection nodeNames = new StringCollection();

                foreach (XmlNode node in lstSettings)
                {
                    nodeNames.Add(node.Name);
                }

                foreach (string setting in appSettings)
                {
                    arr = setting.Split(',');

                    string settingName = arr[0];
                    string defaultValue = arr[1];

                    if (!nodeNames.Contains(settingName))
                    {
                        XmlNode newSetting = xmlDoc.CreateElement(settingName);
                        newSetting.InnerText = defaultValue;
                        xmlDoc.DocumentElement.AppendChild(newSetting);
                        xmlDoc.Save(AppSettingsFile);
                    }
                }
            }
        }

        public sealed class ApplicationSettings
        {
            // General
            public const string DrawingDirectory = "//Settings/DrawingDirectory";
        }
    }
}

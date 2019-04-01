namespace BWC.Utility.CustomSettingsProvider
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.IO.Abstractions;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using BWC.Utility.CustomSettingsProvider.Interfaces;

    public class CustomSettingsProvider<
    GenericTypeFileSystemProvider,
    GenericTypeUserSettingsXmlProvider,
    GenericTypeUserSettingsToUpgradeXmlProvider,
    GenericTypeAppSettingsXmlProvider,
    GenericTypeMachineSettingsXmlProvider> :
    SettingsProvider, IApplicationSettingsProvider
    where GenericTypeFileSystemProvider : IFileSystemProvider, new()
    where GenericTypeUserSettingsXmlProvider : ISettingsPersistableXmlProvider, new()
    where GenericTypeUserSettingsToUpgradeXmlProvider : ISettingsXmlProvider, new()
    where GenericTypeAppSettingsXmlProvider : ISettingsXmlProvider, new()
    where GenericTypeMachineSettingsXmlProvider : ISettingsXmlProvider, new()
    {
        private const string ClassName = "stableSettingsProvider";
        private const string SerializeAs = "serializeAs";
        private const string Config = "configuration";
        private const string UserSettings = "userSettings";
        private const string Value = "value";

        private IFileSystemProvider fileSystemProvider;
        private ISettingsPersistableXmlProvider userSettingsXmlProvider;
        private ISettingsXmlProvider userSettingsToUpgradeXmlProvider;
        private ISettingsXmlProvider appSettingsXmlProvider;
        private ISettingsXmlProvider machineSettingsXmlProvider;

        public CustomSettingsProvider()
        {
            this.fileSystemProvider = new GenericTypeFileSystemProvider();
            this.userSettingsXmlProvider = new GenericTypeUserSettingsXmlProvider();
            this.userSettingsToUpgradeXmlProvider = new GenericTypeUserSettingsToUpgradeXmlProvider();
            this.appSettingsXmlProvider = new GenericTypeAppSettingsXmlProvider();
            this.machineSettingsXmlProvider = new GenericTypeMachineSettingsXmlProvider();
        }

        public override string ApplicationName
        {
            get { return Assembly.GetCallingAssembly().GetName().Name; }
            set { }
        }

        public override string Name
        {
            get { return ClassName; }
        }

        private IFileSystem FileSystem
        {
            get
            {
                if (this.fileSystemProvider == null)
                {
                    this.fileSystemProvider = new GenericTypeFileSystemProvider();
                }

                return this.fileSystemProvider.FileSystem;
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(this.Name, config);
        }

        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            var document = this.userSettingsXmlProvider.XmlOutputDocument(context);
            foreach (SettingsPropertyValue propertyValue in collection)
            {
                this.SetValue(propertyValue, document, context);
            }

            this.userSettingsXmlProvider.Persist(document);
        }

        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            var userSettingsDocument = this.userSettingsXmlProvider.XmlInputDocument(context);
            var appSettingsDocument = this.appSettingsXmlProvider.XmlInputDocument(context);
            var machineSettingsDocument = this.machineSettingsXmlProvider.XmlInputDocument(context);

            var propertyValues = this.GetPropertyValuesFrom(userSettingsDocument, appSettingsDocument, machineSettingsDocument, collection);

            return propertyValues;
        }

        public void Reset(SettingsContext context)
        {
            // TODO do nothing for now
        }

        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property)
        {
            // TODO do nothing for now
            return new SettingsPropertyValue(property);
        }

        public void Upgrade(SettingsContext context, SettingsPropertyCollection collection)
        {
            var document = this.userSettingsToUpgradeXmlProvider.XmlInputDocument(context);
            this.SetPropertyValues(context, this.GetPropertyValuesFrom(document, null, null, collection));
        }

        private XmlNode TryGetUserSettingsClassNode(XmlDocument document)
        {
            try
            {
                return document.SelectSingleNode(Config).SelectSingleNode("userSettings").FirstChild; // there should be only one childnode
            }
            catch (Exception)
            {
                return null;
            }
        }

        private XmlNode TryGetApplicationSettingsClassNode(XmlDocument document)
        {
            try
            {
                return document.SelectSingleNode(Config).SelectSingleNode("applicationSettings").FirstChild; // there should be only one childnode
            }
            catch (Exception)
            {
                return null;
            }
        }

        private XmlNode GetUserSettingsClassNode(XmlDocument document, SettingsContext context)
        {
            XmlNode settingsClass = this.TryGetUserSettingsClassNode(document);
            if (settingsClass == null)
            {
                XmlNode userSettings = document.SelectSingleNode("configuration").SelectSingleNode("userSettings");
                if (userSettings == null)
                {
                    userSettings = document.CreateElement("userSettings");
                    document.SelectSingleNode("configuration").AppendChild(userSettings);
                }

                settingsClass = document.CreateElement(context["SettingsClassType"].ToString());
                userSettings.AppendChild(settingsClass);
            }

            return settingsClass;
        }

        private SettingsPropertyValueCollection GetPropertyValuesFrom(
            XmlDocument userSettingsDocument,
            XmlDocument appSettingsDocument,
            XmlDocument machineSettingsDocument,
            SettingsPropertyCollection collection)
        {
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
            XmlNode userSettings;
            XmlNode appSettings;
            XmlNode machineSettings;

            foreach (SettingsProperty property in collection)
            {
                bool isAppSetting = this.IsApplicationScoped(property);
                bool isUserSetting = this.IsUserScoped(property);
                if (!(isAppSetting ^ isUserSetting))
                {
                    throw new ConfigurationErrorsException();
                }

                if (isAppSetting)
                {
                    userSettings = this.TryGetApplicationSettingsClassNode(userSettingsDocument);
                    appSettings = this.TryGetApplicationSettingsClassNode(appSettingsDocument);
                    machineSettings = this.TryGetApplicationSettingsClassNode(machineSettingsDocument);
                }
                else if (isUserSetting)
                {
                    userSettings = this.TryGetUserSettingsClassNode(userSettingsDocument);
                    appSettings = this.TryGetUserSettingsClassNode(appSettingsDocument);
                    machineSettings = this.TryGetUserSettingsClassNode(machineSettingsDocument);
                }
                else
                {
                    throw new Exception();
                }

                XmlNode userSetting = userSettings?.SelectSingleNode(string.Format("setting[@name='{0}']", property.Name));
                XmlNode appSetting = appSettings?.SelectSingleNode(string.Format("setting[@name='{0}']", property.Name));
                XmlNode machineSetting = machineSettings?.SelectSingleNode(string.Format("setting[@name='{0}']", property.Name));

                var value = new SettingsPropertyValue(property);
                if (userSetting == null)
                {
                    if (appSetting == null)
                    {
                        if (machineSetting == null)
                        {
                            if (property.DefaultValue != null)
                            {
                                value.SerializedValue = property.DefaultValue;
                            }
                            else
                            {
                                value.PropertyValue = null;
                            }
                        }
                        else
                        {
                            value.SerializedValue = this.GetValue(property, machineSetting);
                        }
                    }
                    else
                    {
                        value.SerializedValue = this.GetValue(property, appSetting);
                    }
                }
                else
                {
                    value.SerializedValue = this.GetValue(property, userSetting);
                }

                value.IsDirty = false;
                values.Add(value);
            }

            return values;
        }

        private bool IsApplicationScoped(SettingsProperty property)
        {
            return property.Attributes[typeof(ApplicationScopedSettingAttribute)] is ApplicationScopedSettingAttribute;
        }

        private bool IsUserScoped(SettingsProperty property)
        {
            return property.Attributes[typeof(UserScopedSettingAttribute)] is UserScopedSettingAttribute;
        }

        private void SetValue(SettingsPropertyValue propertyValue, XmlDocument document, SettingsContext context)
        {
            bool isAppSetting = this.IsApplicationScoped(propertyValue.Property);
            bool isUserSetting = this.IsUserScoped(propertyValue.Property);
            if (!(isAppSetting ^ isUserSetting))
            {
                throw new ConfigurationErrorsException();
            }

            // property.UsingDefault is never so we need to do the comparision
            if ((!propertyValue.IsDirty && propertyValue.SerializedValue == propertyValue.Property.DefaultValue) || isAppSetting)
            {
                return;
            }

            XmlNode targetNode = this.GetUserSettingsClassNode(document, context);
            XmlNode settingNode = targetNode.SelectSingleNode(string.Format("setting[@name='{0}']", propertyValue.Name));

            if (settingNode == null)
            {
                settingNode = document.CreateElement("setting");
                targetNode.AppendChild(settingNode);

                XmlAttribute nameAttribute = document.CreateAttribute("name");
                nameAttribute.Value = propertyValue.Name;
                settingNode.Attributes.Append(nameAttribute);

                XmlAttribute serializeAsAttribute = document.CreateAttribute("serializeAs");
                serializeAsAttribute.Value = propertyValue.Property.SerializeAs.ToString();
                settingNode.Attributes.Append(serializeAsAttribute);
            }

            XmlNode valueNode = settingNode.SelectSingleNode(string.Format(".//{0}", "value"));

            if (valueNode == null)
            {
                valueNode = document.CreateElement(Value);
                settingNode.AppendChild(valueNode);
            }

            if (propertyValue.Property.SerializeAs == SettingsSerializeAs.String)
            {
                valueNode.InnerText = WebUtility.HtmlEncode(propertyValue.SerializedValue.ToString());
            }
            else if (propertyValue.Property.SerializeAs == SettingsSerializeAs.Xml)
            {
                // not using propertyvalue.SerializedValue and doing the serialization on our own to avoid writing the xml prolog in the middle of the file
                var xmlDocument = new XmlDocument();
                var xmlNavigator = xmlDocument.CreateNavigator();
                var serializer = new XmlSerializer(propertyValue.Property.PropertyType);
                using (XmlWriter xmlWriter = xmlNavigator.AppendChild())
                {
                    serializer.Serialize(xmlWriter, propertyValue.PropertyValue);
                }

                valueNode.InnerXml = xmlDocument.FirstChild.OuterXml;
            }
            else if (propertyValue.Property.SerializeAs == SettingsSerializeAs.Binary)
            {
                byte[] buf = propertyValue.SerializedValue as byte[];
                if (buf != null)
                {
                    valueNode.InnerText = Convert.ToBase64String(buf);
                }
            }
            else
            {
                throw new NotImplementedException(string.Format("Can  not serialize setting to {0}", propertyValue.Property.SerializeAs.ToString()));
            }
        }

        private string GetValue(SettingsProperty property, XmlNode settingNode)
        {
            string innerContent;
            if (property.SerializeAs == SettingsSerializeAs.String)
            {
                innerContent = WebUtility.HtmlDecode(settingNode.SelectSingleNode(string.Format(".//{0}", Value))?.InnerText);
            }
            else if (property.SerializeAs == SettingsSerializeAs.Xml)
            {
                innerContent = settingNode.SelectSingleNode(string.Format(".//{0}", Value))?.InnerXml;
            }
            else if (property.SerializeAs == SettingsSerializeAs.Binary)
            {
                innerContent = settingNode.SelectSingleNode(string.Format(".//{0}", Value))?.InnerXml;
            }
            else
            {
                throw new NotImplementedException(string.Format("Can  not deserialize setting from {0}", property.SerializeAs.ToString()));
            }

            if (string.IsNullOrEmpty(innerContent))
            {
                if (property.DefaultValue == null)
                {
                    return string.Empty;
                }
                else
                {
                    return property.DefaultValue.ToString();
                }
            }
            else
            {
                return innerContent;
            }
        }
    }

    public class CustomPathSettingsProvider<
        GenericTypeUserSettingsPathProvider,
        GenericTypeUserSettingsToUpgradePathProvider,
        GenericTypeAppSettingsPathProvider,
        GenericTypeMachineSettingsPathProvider> :
        CustomSettingsProvider<
            DefaultProviders.FileSystemProvider,
            PersistantXmlFromPathProvider<GenericTypeUserSettingsPathProvider, DefaultProviders.FileSystemProvider>,
            XmlFromPathProvider<GenericTypeUserSettingsToUpgradePathProvider, DefaultProviders.FileSystemProvider>,
            XmlFromPathProvider<GenericTypeAppSettingsPathProvider, DefaultProviders.FileSystemProvider>,
            XmlFromPathProvider<GenericTypeMachineSettingsPathProvider, DefaultProviders.FileSystemProvider>>
        where GenericTypeUserSettingsPathProvider : ISettingsPathProvider, new()
        where GenericTypeUserSettingsToUpgradePathProvider : ISettingsPathProvider, new()
        where GenericTypeAppSettingsPathProvider : ISettingsPathProvider, new()
        where GenericTypeMachineSettingsPathProvider : ISettingsPathProvider, new()
    {
    }

    public class CustomXmlSettingsProvider<
        GenericTypeUserSettingsXmlProvider,
        GenericTypeUserSettingsToUpgradeXmlProvider,
        GenericTypeAppSettingsXmlProvider,
        GenericTypeMachineSettingsXmlProvider> :
    CustomSettingsProvider<
        DefaultProviders.FileSystemProvider,
        GenericTypeUserSettingsXmlProvider,
        GenericTypeUserSettingsToUpgradeXmlProvider,
        GenericTypeAppSettingsXmlProvider,
        GenericTypeMachineSettingsXmlProvider>
    where GenericTypeUserSettingsXmlProvider : ISettingsPersistableXmlProvider, new()
    where GenericTypeUserSettingsToUpgradeXmlProvider : ISettingsXmlProvider, new()
    where GenericTypeAppSettingsXmlProvider : ISettingsXmlProvider, new()
    where GenericTypeMachineSettingsXmlProvider : ISettingsXmlProvider, new()
    {
    }

    public class XmlFromPathProvider<GenericTypeSettingsPathProvider, GenericTypeFileSystemProvider> : ISettingsXmlProvider
        where GenericTypeSettingsPathProvider : ISettingsPathProvider, new()
        where GenericTypeFileSystemProvider : IFileSystemProvider, new()
    {
        public XmlFromPathProvider()
        {
            this.FileSystemProvider = new GenericTypeFileSystemProvider();
            this.SettingsPathProvider = new GenericTypeSettingsPathProvider();
        }

        protected ISettingsPathProvider SettingsPathProvider { get; set; }
        protected IFileSystemProvider FileSystemProvider { get; set; }

        public XmlDocument XmlInputDocument(SettingsContext context)
        {
            var document = new XmlDocument();
            try
            {
                document.LoadXml(this.FileSystemProvider.FileSystem.File.ReadAllText(this.SettingsPathProvider.SettingsPath));
            }
            catch (Exception ex) when (
                ex is System.IO.FileNotFoundException ||
                ex is System.IO.DirectoryNotFoundException ||
                ex is System.IO.DriveNotFoundException)
            {
                return null;
            }

            return document;
        }

        public XmlDocument XmlOutputDocument(SettingsContext context)
        {
            var document = new XmlDocument();
            try
            {
                var settingsPathInfo = this.FileSystemProvider.FileSystem.FileInfo.FromFileName(this.SettingsPathProvider.SettingsPath);
                this.FileSystemProvider.FileSystem.Directory.CreateDirectory(settingsPathInfo.DirectoryName);
                document.LoadXml(this.FileSystemProvider.FileSystem.File.ReadAllText(this.SettingsPathProvider.SettingsPath));
            }
            catch (System.IO.FileNotFoundException)
            {
                document.AppendChild(document.CreateXmlDeclaration("1.0", "utf-8", string.Empty));
                var config = document.CreateElement("configuration");
                document.AppendChild(config);
            }

            return document;
        }

        private XmlElement CreateConfigSectionsNode(XmlDocument document)
        {
            return document.CreateElement("configSections");
        }

        private XmlElement CreateSectionsGroupNode(XmlDocument document, string name, string type)
        {
            var sectionsGroupNode = document.CreateElement("sectionGroup");

            var nameAttribute = document.CreateAttribute("name");
            nameAttribute.InnerXml = name;
            sectionsGroupNode.Attributes.Append(nameAttribute);

            return sectionsGroupNode;
        }

        private XmlElement CreateSectionsNode(XmlDocument document, string name, string type, string allowDefinition = null, string allowLocation = null)
        {
            var sectionNode = document.CreateElement("section");

            var nameAttribute = document.CreateAttribute("name");
            nameAttribute.InnerXml = name;
            sectionNode.Attributes.Append(nameAttribute);

            var typeAttribute = document.CreateAttribute("type");
            typeAttribute.InnerXml = type;
            sectionNode.Attributes.Append(typeAttribute);

            if (allowDefinition != null)
            {
                var allowDefinitionAttribute = document.CreateAttribute("allowDefinition");
                allowDefinitionAttribute.InnerXml = allowDefinition;
                sectionNode.Attributes.Append(allowDefinitionAttribute);
            }

            if (allowLocation != null)
            {
                var allowLocationAttribute = document.CreateAttribute("allowLocation");
                allowLocationAttribute.InnerXml = allowLocation;
                sectionNode.Attributes.Append(allowLocationAttribute);
            }

            return sectionNode;
        }
    }

    public class PersistantXmlFromPathProvider<GenericTypeSettingsPathProvider, GenericTypeFileSystemProvider> :
        XmlFromPathProvider<
            GenericTypeSettingsPathProvider,
            GenericTypeFileSystemProvider>,
        ISettingsPersistableXmlProvider
        where GenericTypeSettingsPathProvider : ISettingsPathProvider, new()
        where GenericTypeFileSystemProvider : IFileSystemProvider, new()
    {
        public void Persist(XmlDocument document)
        {
            System.IO.StringWriter sw = new System.IO.StringWriter();
            XmlWriter xw = XmlWriter.Create(sw, new XmlWriterSettings { Encoding = new UTF8Encoding(false, true) });
            document.WriteTo(xw);
            xw.Flush();
            sw.Flush();
            this.FileSystemProvider.FileSystem.File.WriteAllText(this.SettingsPathProvider.SettingsPath, sw.ToString());
        }
    }
}

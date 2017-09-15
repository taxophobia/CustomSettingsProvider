using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Configuration;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Abstractions;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using BWC.Utility.CustomSettingsProvider;
using System.Security.Cryptography.Xml;
using System.Reflection;
using BWC.Utility.CustomSettingsProvider.Interfaces;

namespace StableSettingsProviderTest
{
    public static class Consts
    {
        // constants to generate filesystem
        public const string SettingsPath = "C:\\Settings.config";
        public const string LegacySettingsPath = "C:\\LegacySettings.config";
        public const string AppSettingsPath = "C:\\AppSettings.config";
        public const string MachineSettingsPath = "C:\\MachineSettings.config";
        // constants to generate reference xml
        public const string Config = "configuration";
        public const string UserSettings = "userSettings";
        public const string ApplicationSettings = "applicationSettings";
        public const string Setting = "setting";
        public const string Value = "value";
        public const string ConfigSections = "configSections";
        public const string SectionGroup = "sectionGroup";
        public const string Section = "section";
        //
        // constants of settings names and values
        // 
        // StringSettingescaped
        public const string StringSettingEscapedName = "StringSettingEscaped";
        public const string StringSettingEscapedValue = "&quot;&amp;&lt;&gt;";
        public const string StringSettingEscapedValueUnescaped = "\"&<>";
        public const string StringSettingEscapedSerializeAs = "String";
        // StringSetting
        public const string StringSettingName = "StringSetting";
        public const string StringSettingValueDefault = "StringSettingValueDefault";
        public const string StringSettingValueNonDefault = "StringSettingValueNonDefault";
        public const string StringSettingSerializeAs = "String";
        // BinarySetting
        public const string BinarySettingName = "BinarySetting";
        public const string BinarySettingValueDefault = "AAEAAAD/////AQAAAAAAAAAPAQAAAAIAAAACICAL"; // new byte[] { 0x20, 0x20 }
        public const string BinarySettingValueNonDefault = "AAEAAAD/////AQAAAAAAAAAPAQAAAAIAAAACQEAL"; // new byte[] { 0x40, 0x40 }
        public const string BinarySettingSerializeAs = "Binary";
        // XmlSetting
        public const string XmlSettingName = "XmlSetting";
        public const string XmlSettingValueDefault =
            "<XmlSettingValue xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<FirstChild>XmlSettingValueFirstChildValueDefault</FirstChild>" +
                "<SecondChild>XmlSettingValueSecondChildValueDefault</SecondChild>" +
            "</XmlSettingValue>";
        public const string XmlSettingValueNonDefault =
            "<XmlSettingValue xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">" +
                "<FirstChild>XmlSettingValueFirstChildValueNonDefault</FirstChild>" +
                "<SecondChild>XmlSettingValueSecondChildValueNonDefault</SecondChild>" +
            "</XmlSettingValue>";
        public const string XmlSettingSerializeAs = "Xml";
        // AppSetting
        public const string AppSettingName = "AppSetting";
        public const string AppSettingDefaultValue = "AppSettingDefaultValue";
        public const string AppSettingNonDefaultValue = "AppSettingNonDefaultValue";
        public const string AppSettingSerializeAs = "String";
    }
    public static class StaticMockFileSystem
    {
        public static IFileSystem MockFileSystem { get; set; }
    }
    public class TestableFileSystemProvider : IFileSystemProvider
    {
        public IFileSystem FileSystem
        {
            get { return StaticMockFileSystem.MockFileSystem; }
        }
    }
    public class TestableSettingsPathProvider : ISettingsPathProvider
    {
        public string SettingsPath { get { return Consts.SettingsPath; } }
    }
    public class TestableLegacySettingsPathProvider : ISettingsPathProvider
    {
        public string SettingsPath { get { return Consts.LegacySettingsPath; } }
    }
    public class TestableAppSettingsPathProvider : ISettingsPathProvider
    {
        public string SettingsPath { get { return Consts.AppSettingsPath; } }
    }
    public class TestableMachineSettingsPathProvider : ISettingsPathProvider
    {
        public string SettingsPath { get { return Consts.MachineSettingsPath; } }
    }

    public class TestableStableSettingsProvider :
        CustomSettingsProvider<
            TestableFileSystemProvider,
            PersistantXmlFromPathProvider<TestableSettingsPathProvider, TestableFileSystemProvider>,
            XmlFromPathProvider<TestableLegacySettingsPathProvider, TestableFileSystemProvider>,
            XmlFromPathProvider<TestableAppSettingsPathProvider, TestableFileSystemProvider>,
            XmlFromPathProvider<TestableMachineSettingsPathProvider, TestableFileSystemProvider>>
    { }

    [Serializable]
    [XmlRoot("XmlSettingValue")]
    public class XmlSettingValue
    {
        [XmlElement("FirstChild")]
        public string FirstChild { get; set; }
        [XmlElement("SecondChild")]
        public string SecondChild { get; set; }
    }
    public class TestSettingsBase : ApplicationSettingsBase
    {
        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.String)]
        [DefaultSettingValue(Consts.StringSettingValueDefault)]
        public string StringSetting
        {
            get
            {
                return ((string)(this["StringSetting"]));
            }
            set
            {
                this["StringSetting"] = value;
            }
        }
        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.String)]
        [DefaultSettingValue(Consts.StringSettingEscapedValueUnescaped)]
        public string StringSettingEscaped
        {
            get
            {
                return ((string)(this["StringSettingEscaped"]));
            }
            set
            {
                this["StringSettingEscaped"] = value;
            }
        }
        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.Xml)]
        [DefaultSettingValue(Consts.XmlSettingValueDefault)]
        public XmlSettingValue XmlSetting
        {
            get
            {
                return ((XmlSettingValue)(this["XmlSetting"]));
            }
            set
            {
                this["XmlSetting"] = value;
            }
        }
        [UserScopedSettingAttribute()]
        [SettingsSerializeAsAttribute(SettingsSerializeAs.Binary)]
        [DefaultSettingValue(Consts.BinarySettingValueDefault)]
        public byte[] BinarySetting
        {
            get
            {
                return ((byte[])(this["BinarySetting"]));
            }
            set
            {
                this["BinarySetting"] = value;
            }
        }
        [ApplicationScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.String)]
        [DefaultSettingValue(Consts.AppSettingDefaultValue)]
        public string AppSetting
        {
            get
            {
                return ((string)(this["AppSetting"]));
            }
            set
            {
                this["AppSetting"] = value;
            }
        }
        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.String)]
        [DefaultSettingValue("True")]
        public bool NeedsUpgrade
        {
            get
            {
                return ((bool)(this["NeedsUpgrade"]));
            }
            set
            {
                this["NeedsUpgrade"] = value;
            }
        }
    }
    [SettingsProvider(typeof(TestableStableSettingsProvider))]
    public class TestSettingsAllValid : TestSettingsBase
    {
        private static TestSettingsAllValid defaultInstance = ((TestSettingsAllValid)(Synchronized(new TestSettingsAllValid())));

        public static TestSettingsAllValid Default
        {
            get
            {
                return defaultInstance;
            }
        }
    }
    [SettingsProvider(typeof(TestableStableSettingsProvider))]
    public class TestSettingsSomeInvalid : TestSettingsBase
    {
        private static TestSettingsSomeInvalid defaultInstance = ((TestSettingsSomeInvalid)(Synchronized(new TestSettingsSomeInvalid())));

        public static TestSettingsSomeInvalid Default
        {
            get
            {
                return defaultInstance;
            }
        }
        [ApplicationScopedSetting()]
        [UserScopedSetting()]
        [SettingsSerializeAs(SettingsSerializeAs.String)]
        public string InvalidSetting
        {
            get
            {
                return ((string)(this["InvalidSetting"]));
            }
            set
            {
                this["InvalidSetting"] = value;
            }
        }
    }

    static class Helpers
    {
        public static string GetCanonicalXml(string xml)
        {
            XmlDocument document = new XmlDocument();
            document.PreserveWhitespace = true;
            document.LoadXml(xml);

            XmlDsigC14NTransform c14n = new XmlDsigC14NTransform();
            c14n.LoadInput(document);

            Stream s = (Stream)c14n.GetOutput(typeof(Stream));
            return new StreamReader(s, Encoding.UTF8).ReadToEnd();
        }
        public static string GenerateFileContent(
            List<Tuple<string, string, string>> SettingPropertiesList,
            Type applicationSettingsBaseType,
            bool hasUserSettings = true,
            bool hasAppSettings = false,
            ConfigurationAllowExeDefinition allowExeDefinition = ConfigurationAllowExeDefinition.MachineOnly,
            ConfigurationAllowDefinition allowDefinition = ConfigurationAllowDefinition.Everywhere,
            bool allowLocation = true)
        {
            var settingsClassType = applicationSettingsBaseType.Namespace + '.' + applicationSettingsBaseType.Name;

            string SettingContent = string.Empty;
            MemoryStream stream = new MemoryStream();
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", string.Empty));
            var config = doc.CreateElement(Consts.Config);
            doc.AppendChild(config);

            XmlElement settings;
            if (hasUserSettings)
            {
                settings = doc.CreateElement(Consts.UserSettings);
            }
            else if (hasAppSettings)
            {
                settings = doc.CreateElement(Consts.ApplicationSettings);
            }
            else
            {
                throw new Exception();
            }
            config.AppendChild(settings);

            var propertiesSettings =
                doc.CreateElement(settingsClassType);
            settings.AppendChild(propertiesSettings);

            //if (hasUserSettings)
            //{
            //    var userSectionGroupNode = doc.CreateElement(Consts.SectionGroup);

            //    var userSectionGroupNameAttribute = doc.CreateAttribute("name");
            //    userSectionGroupNameAttribute.InnerXml = Consts.UserSettings;
            //    userSectionGroupNode.Attributes.Append(userSectionGroupNameAttribute);

            //    var userSectionNode = doc.CreateElement(Consts.Section);

            //    var userSectionNameAttribute = doc.CreateAttribute("name");
            //    userSectionNameAttribute.InnerXml = settingsClassType;
            //    userSectionNode.Attributes.Append(userSectionNameAttribute);

            //    var userSectionTypeAttribute = doc.CreateAttribute("type");
            //    userSectionTypeAttribute.InnerXml = Assembly.GetCallingAssembly().GetType().AssemblyQualifiedName;
            //    userSectionNode.Attributes.Append(userSectionTypeAttribute);

            //    if (allowDefinition != ConfigurationAllowDefinition.Everywhere)
            //    {
            //        var allowDefinitionAttribute = doc.CreateAttribute("allowDefinition");
            //        allowDefinitionAttribute.InnerXml = allowDefinition.ToString();
            //        userSectionNode.Attributes.Append(allowDefinitionAttribute);
            //    }

            //    if (allowExeDefinition != ConfigurationAllowExeDefinition.MachineOnly)
            //    {
            //        var allowExeDefinitionAttribute = doc.CreateAttribute("allowExeDefinition");
            //        allowExeDefinitionAttribute.InnerXml = allowExeDefinition.ToString();
            //        userSectionNode.Attributes.Append(allowExeDefinitionAttribute);
            //    }

            //    if (allowLocation != true)
            //    {
            //        var allowLocationAttribute = doc.CreateAttribute("allowLocation");
            //        allowLocationAttribute.InnerXml = allowLocation.ToString();
            //        userSectionNode.Attributes.Append(allowLocationAttribute);
            //    }
            //    userSectionGroupNode.AppendChild(userSectionNode);
            //    configSectionsNode.AppendChild(userSectionGroupNode);
            //}

            //if (hasAppSettings)
            //{
            //    // TODO
            //}


            foreach (var SettingProperty in SettingPropertiesList)
            {
                var settingName = SettingProperty.Item1;
                var settingValue = SettingProperty.Item2;
                var settingSerializeAs = SettingProperty.Item3;
                XmlNode setting = GenerateSettingNode(doc, settingName, settingValue, settingSerializeAs);
                propertiesSettings.AppendChild(setting);
            }

            StringWriter sw = new StringWriter();
            XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { Encoding = new UTF8Encoding(false, true) });
            doc.WriteTo(writer);
            writer.Flush();
            sw.Flush();
            return sw.ToString();

        }
        static private XmlNode GenerateSettingNode(XmlDocument doc, string SettingName, string SettingValue, string SettingSerializeAs)
        {
            var settingNode = doc.CreateElement(Consts.Setting);

            var nameAttribute = doc.CreateAttribute("name");
            nameAttribute.Value = SettingName;
            settingNode.Attributes.Append(nameAttribute);

            var serializeAsAttribute = doc.CreateAttribute("serializeAs");
            serializeAsAttribute.Value = SettingSerializeAs;
            settingNode.Attributes.Append(serializeAsAttribute);

            var valueNode = doc.CreateElement(Consts.Value);
            valueNode.InnerXml = SettingValue; // do _not_ escape anything
            settingNode.AppendChild(valueNode);

            return settingNode;
        }
    }

    public class FileSystemFactory
    {
        private FileSystemFactory()
        {
        }
        public IFileSystem FileSystem { get; set; }

        public static FileSystemFactory Instance { get { return Nested.instance; } }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly FileSystemFactory instance = new FileSystemFactory();
        }
    }

    [SingleThreaded]
    [TestFixture]
    public class StableSettingsProviderTestFixture
    {
        /// <summary>
        /// Test Reload and Save without changes and without touching anything. Nothing should be written to the FS
        /// </summary>
        /// <param name="Setting"></param>
        [Test]
        public void TestReloadSaveCycle()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory("c:\\");
            StaticMockFileSystem.MockFileSystem = fileSystem;
            TestSettingsAllValid.Default.Reload(); //Context["SettingsClassType"]
            TestSettingsAllValid.Default.Save();
            var ex = Assert.Throws<FileNotFoundException>(() => StaticMockFileSystem.MockFileSystem.File.ReadAllText(Consts.SettingsPath));
            StaticMockFileSystem.MockFileSystem = null;
        }
        /// <summary>
        /// Test Reload, Touch and Save. Touched values should write their default value to the FS
        /// </summary>
        /// <param name="SettingName"></param>
        /// <param name="SettingValue"></param>
        /// <param name="SettingSerializeAs"></param>
        [TestCase(Consts.StringSettingName, Consts.StringSettingValueDefault, Consts.StringSettingSerializeAs)]
        [TestCase(Consts.BinarySettingName, Consts.BinarySettingValueDefault, Consts.BinarySettingSerializeAs)]
        [TestCase(Consts.XmlSettingName, Consts.XmlSettingValueDefault, Consts.XmlSettingSerializeAs)]
        public void TestNoSettingsFileReloadTouchSaveCycle(string SettingName, string SettingValue, string SettingSerializeAs)
        {
            string referenceConfigFileContent = Helpers.GenerateFileContent(new List<Tuple<string, string, string>> { new Tuple<string, string, string>(SettingName, SettingValue, SettingSerializeAs) }, typeof(TestSettingsAllValid));
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory("c:\\");
            StaticMockFileSystem.MockFileSystem = fileSystem;
            TestSettingsAllValid.Default.Reload();
            if (SettingName == "StringSetting")
                TestSettingsAllValid.Default.StringSetting = TestSettingsAllValid.Default.StringSetting;
            if (SettingName == "BinarySetting")
                TestSettingsAllValid.Default.BinarySetting = TestSettingsAllValid.Default.BinarySetting;
            if (SettingName == "XmlSetting")
                TestSettingsAllValid.Default.XmlSetting = TestSettingsAllValid.Default.XmlSetting;
            TestSettingsAllValid.Default.Save();
            var result = StaticMockFileSystem.MockFileSystem.File.ReadAllText(Consts.SettingsPath);
            Assert.AreEqual(Helpers.GetCanonicalXml(referenceConfigFileContent), Helpers.GetCanonicalXml(result));
            StaticMockFileSystem.MockFileSystem = null;
        }
        /// <summary>
        /// Test Reload, Touch and Save. Touched values should write their default value to the FS
        /// </summary>
        /// <param name="Setting"></param>
        /// <param name="Value"></param>
        [TestCase(Consts.StringSettingName, Consts.StringSettingValueNonDefault, Consts.StringSettingSerializeAs)]
        [TestCase(Consts.BinarySettingName, Consts.BinarySettingValueNonDefault, Consts.BinarySettingSerializeAs)]
        [TestCase(Consts.XmlSettingName, Consts.XmlSettingValueNonDefault, Consts.XmlSettingSerializeAs)]
        public void TestSettingsFileReloadTouchSaveCycle(string SettingName, string SettingValue, string SettingSerializeAs)
        {
            string referenceConfigFileContent = Helpers.GenerateFileContent(new List<Tuple<string, string, string>> { new Tuple<string, string, string>(SettingName, SettingValue, SettingSerializeAs) }, typeof(TestSettingsAllValid));
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Consts.SettingsPath, new MockFileData(referenceConfigFileContent) }
             });
            StaticMockFileSystem.MockFileSystem = fileSystem;
            TestSettingsAllValid.Default.Reload();
            if (SettingName == "StringSetting")
                TestSettingsAllValid.Default.StringSetting = TestSettingsAllValid.Default.StringSetting;
            if (SettingName == "BinarySetting")
                TestSettingsAllValid.Default.BinarySetting = TestSettingsAllValid.Default.BinarySetting;
            if (SettingName == "XmlSetting")
                TestSettingsAllValid.Default.XmlSetting = TestSettingsAllValid.Default.XmlSetting;
            TestSettingsAllValid.Default.Save();
            var result = StaticMockFileSystem.MockFileSystem.File.ReadAllText(Consts.SettingsPath);
            Assert.AreEqual(Helpers.GetCanonicalXml(referenceConfigFileContent), Helpers.GetCanonicalXml(result));
            StaticMockFileSystem.MockFileSystem = null;
        }
        [TestCase(
            Consts.StringSettingName, Consts.StringSettingValueNonDefault, Consts.StringSettingSerializeAs,
            Consts.BinarySettingName, Consts.BinarySettingValueNonDefault, Consts.BinarySettingSerializeAs)]
        [TestCase(
            Consts.BinarySettingName, Consts.BinarySettingValueNonDefault, Consts.BinarySettingSerializeAs,
            Consts.XmlSettingName, Consts.XmlSettingValueNonDefault, Consts.XmlSettingSerializeAs)]
        [TestCase(
            Consts.XmlSettingName, Consts.XmlSettingValueNonDefault, Consts.XmlSettingSerializeAs,
            Consts.StringSettingName, Consts.StringSettingValueNonDefault, Consts.StringSettingSerializeAs)]
        public void TestTwoOfThreeReloadTouchSaveCycle(
            string SettingNameA, string SettingValueA, string SettingSerializeAsA,
            string SettingNameB, string SettingValueB, string SettingSerializeAsB)
        {
            string referenceConfigFileContent = Helpers.GenerateFileContent(new List<Tuple<string, string, string>>
            {
                new Tuple<string, string, string>(SettingNameA, SettingValueA, SettingSerializeAsA),
                new Tuple<string, string, string>(SettingNameB, SettingValueB, SettingSerializeAsB)

            }, typeof(TestSettingsAllValid));
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Consts.SettingsPath, new MockFileData(referenceConfigFileContent) }
             });
            StaticMockFileSystem.MockFileSystem = fileSystem;
            TestSettingsAllValid.Default.Reload();
            if (SettingNameA == "StringSetting")
                TestSettingsAllValid.Default.StringSetting = TestSettingsAllValid.Default.StringSetting;
            if (SettingNameA == "BinarySetting")
                TestSettingsAllValid.Default.BinarySetting = TestSettingsAllValid.Default.BinarySetting;
            if (SettingNameA == "XmlSetting")
                TestSettingsAllValid.Default.XmlSetting = TestSettingsAllValid.Default.XmlSetting;
            TestSettingsAllValid.Default.Save();
            var result = StaticMockFileSystem.MockFileSystem.File.ReadAllText(Consts.SettingsPath);
            Assert.AreEqual(Helpers.GetCanonicalXml(referenceConfigFileContent), Helpers.GetCanonicalXml(result));
            StaticMockFileSystem.MockFileSystem = null;
        }
        /// <summary>
        /// Test reading and writing escaped string setting
        /// </summary>
        [Test]
        public void TestEscaping()
        {
            string referenceConfigFileContent = Helpers.GenerateFileContent(new List<Tuple<string, string, string>> { new Tuple<string, string, string>(Consts.StringSettingEscapedName, Consts.StringSettingEscapedValue, Consts.StringSettingEscapedSerializeAs) }, typeof(TestSettingsAllValid));
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Consts.SettingsPath, new MockFileData(referenceConfigFileContent) }
             });
            StaticMockFileSystem.MockFileSystem = fileSystem;
            Assert.AreEqual(Consts.StringSettingEscapedValueUnescaped, TestSettingsAllValid.Default.StringSettingEscaped);
            var result = StaticMockFileSystem.MockFileSystem.File.ReadAllText(Consts.SettingsPath);
            Assert.AreEqual(Helpers.GetCanonicalXml(referenceConfigFileContent), Helpers.GetCanonicalXml(result));
            StaticMockFileSystem.MockFileSystem = null;
        }
        /// <summary>
        /// Test reload, change and save for an application setting
        /// </summary>
        [Test]
        public void TestApplicationSettings()
        {
            string configFileContent =
                Helpers.GenerateFileContent(
                    new List<Tuple<string, string, string>>
                    {
                        new Tuple<string, string, string>(Consts.AppSettingName, Consts.AppSettingNonDefaultValue, Consts.AppSettingSerializeAs)
                    },
                    typeof(TestSettingsAllValid),
                    false /*hasUserSettings*/,
                    true /*hasAppSettings*/);
            //var appConfigPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Consts.SettingsPath, new MockFileData(configFileContent) }
             });
            StaticMockFileSystem.MockFileSystem = fileSystem;
            TestSettingsAllValid.Default.Reload();
            Assert.AreEqual(Consts.AppSettingNonDefaultValue, TestSettingsAllValid.Default.AppSetting);
            TestSettingsAllValid.Default.AppSetting = Consts.AppSettingDefaultValue;
            TestSettingsAllValid.Default.Save();
            TestSettingsAllValid.Default.Reload();
            Assert.AreEqual(Consts.AppSettingNonDefaultValue, TestSettingsAllValid.Default.AppSetting); // ignore writes to application settings
            StaticMockFileSystem.MockFileSystem = null;
        }
        /// <summary>
        /// Test applicationSettingsBase attributes that can not used together, should fail with exception
        /// </summary>
        [Test]
        public void TestIncompitableAttributes()
        {
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory("c:\\");
            StaticMockFileSystem.MockFileSystem = fileSystem;
            string foo;
            var exA = Assert.Throws<ConfigurationErrorsException>(() => TestSettingsSomeInvalid.Default.InvalidSetting = "foo");
            var exB = Assert.Throws<ConfigurationErrorsException>(() => foo = TestSettingsSomeInvalid.Default.InvalidSetting);
            StaticMockFileSystem.MockFileSystem = null;

        }
        /// <summary>
        /// Test migrating Settings from older versions, all values are dirty if loaded from legacy config
        /// (therefore the test is using a settings class without invalid values that would throw).
        /// Only non default values are writen to filesystem.
        /// </summary>
        [Test]
        public void TestUpgradingSettings()
        {
            string referenceConfigFileContent =
                Helpers.GenerateFileContent(
                    new List<Tuple<string, string, string>>
                    {
                        new Tuple<string, string, string>(Consts.StringSettingName, Consts.StringSettingValueDefault, Consts.StringSettingSerializeAs),
                        new Tuple<string, string, string>("NeedsUpgrade", "False", "String")
                    },
                    typeof(TestSettingsAllValid),
                    true /*hasUserSettings*/,
                    false /*hasAppSettings*/);
            string configFileContent =
                Helpers.GenerateFileContent(
                    new List<Tuple<string, string, string>>
                    {
                        new Tuple<string, string, string>(Consts.StringSettingName, Consts.StringSettingValueNonDefault, Consts.StringSettingSerializeAs)
                    },
                    typeof(TestSettingsAllValid),
                    true /*hasUserSettings*/,
                    false /*hasAppSettings*/);
            //var appConfigPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Consts.LegacySettingsPath, new MockFileData(configFileContent) }
             });
            StaticMockFileSystem.MockFileSystem = fileSystem;
            if (TestSettingsAllValid.Default.NeedsUpgrade)
            {
                TestSettingsAllValid.Default.Upgrade();
                TestSettingsAllValid.Default.NeedsUpgrade = false;
                TestSettingsAllValid.Default.Save();
            }
            Assert.AreEqual(Consts.StringSettingValueNonDefault, TestSettingsAllValid.Default.StringSetting);
            TestSettingsAllValid.Default.StringSetting = Consts.StringSettingValueDefault;
            TestSettingsAllValid.Default.Save();
            Assert.AreEqual(Consts.StringSettingValueDefault, TestSettingsAllValid.Default.StringSetting);
            var result = StaticMockFileSystem.MockFileSystem.File.ReadAllText(Consts.SettingsPath);
            Assert.AreEqual(Helpers.GetCanonicalXml(referenceConfigFileContent), Helpers.GetCanonicalXml(result));
            StaticMockFileSystem.MockFileSystem = null;
        }

        /// <summary>
        /// Read a setting from machineconfig and override it with userconfig
        /// </summary>
        [Test]
        public void TestPriorizeUserConfigOverMachineConfig()
        {
            string machineConfigFileContent =
                Helpers.GenerateFileContent(
                    new List<Tuple<string, string, string>>
                    {
                        new Tuple<string, string, string>(Consts.StringSettingName, "machine", Consts.StringSettingSerializeAs)
                    },
                    typeof(TestSettingsAllValid),
                    true /*hasUserSettings*/,
                    false /*hasAppSettings*/);
            string userConfigFileContent =
                 Helpers.GenerateFileContent(
                     new List<Tuple<string, string, string>>
                     {
                        new Tuple<string, string, string>(Consts.StringSettingName, "user", Consts.StringSettingSerializeAs)
                     },
                     typeof(TestSettingsAllValid),
                     true /*hasUserSettings*/,
                     false /*hasAppSettings*/);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Consts.SettingsPath, new MockFileData(userConfigFileContent) },
                { Consts.MachineSettingsPath, new MockFileData(machineConfigFileContent) }
             });
            StaticMockFileSystem.MockFileSystem = fileSystem;
            TestSettingsAllValid.Default.Reload();
            Assert.AreEqual("user", TestSettingsAllValid.Default.StringSetting);
        }

        /// <summary>
        /// Read a setting from machineconfig without any userconfig
        /// </summary>
        [Test]
        public void TestReadMachineConfig()
        {
            string machineConfigFileContent =
                Helpers.GenerateFileContent(
                    new List<Tuple<string, string, string>>
                    {
                        new Tuple<string, string, string>(Consts.StringSettingName, "machine", Consts.StringSettingSerializeAs)
                    },
                    typeof(TestSettingsAllValid),
                    true /*hasUserSettings*/,
                    false /*hasAppSettings*/);
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { Consts.MachineSettingsPath, new MockFileData(machineConfigFileContent) }
             });
            StaticMockFileSystem.MockFileSystem = fileSystem;
            TestSettingsAllValid.Default.Reload();
            Assert.AreEqual("machine", TestSettingsAllValid.Default.StringSetting);
        }
    }
}
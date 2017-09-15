namespace BWC.Utility.CustomSettingsProvider.Interfaces
{
    using System.Configuration;
    using System.Xml;

    public interface ISettingsXmlProvider
    {
        XmlDocument XmlInputDocument(SettingsContext context);
        XmlDocument XmlOutputDocument(SettingsContext context);
    }
}

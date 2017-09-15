namespace BWC.Utility.CustomSettingsProvider.Interfaces
{
    using System.Xml;

    public interface ISettingsPersistableXmlProvider : ISettingsXmlProvider
    {
        void Persist(XmlDocument document);
    }
}

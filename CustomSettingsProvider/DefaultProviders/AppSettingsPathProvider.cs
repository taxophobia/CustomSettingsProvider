namespace BWC.Utility.CustomSettingsProvider.DefaultProviders
{
    using BWC.Utility.CustomSettingsProvider.Interfaces;

    public class AppSettingsPathProvider : ISettingsPathProvider
    {
        public string SettingsPath
        {
            get
            {
                return System.AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            }
        }
    }
}

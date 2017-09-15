namespace BWC.Utility.CustomSettingsProvider.DefaultProviders
{
    using BWC.Utility.CustomSettingsProvider.Interfaces;

    public class MachineSettingsPathProvider : ISettingsPathProvider
    {
        public string SettingsPath
        {
            get
            {
                return System.Runtime.InteropServices.RuntimeEnvironment.SystemConfigurationFile;
            }
        }
    }
}

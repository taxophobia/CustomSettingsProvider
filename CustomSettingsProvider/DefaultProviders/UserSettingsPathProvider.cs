namespace BWC.Utility.CustomSettingsProvider.DefaultProviders
{
    using System;
    using System.Reflection;
    using BWC.Utility.CustomSettingsProvider.Interfaces;

    public class UserSettingsPathProvider : ISettingsPathProvider
    {
        public string SettingsPath
        {
            get
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var companyName = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company;
                var assemblyTitle = Assembly.GetCallingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
                var assemblyVersion = Assembly.GetCallingAssembly().GetName().Version.ToString();
                return System.IO.Path.Combine(appDataPath, companyName, assemblyTitle, assemblyVersion, "user.config");
            }
        }
    }
}

# Customizable SettingsProvider for DotNets Application Settings 

While the default dotNet localFileSettingsProvider (LFSP) is a good fit for certain deployment scenarios it lacks in other scenarios. The LFSP handles the deployment of clickOnce or strongNamed assemblies quite well and provides a stable settings path that does not change on updates. It does not work for side-by-side deployments of different versions or mobile deployments since these dont have a stable install path and therefore no stable settings path. The CostumSettingsProvider (CSP) offers more flexibility in these scenarios and allows for a stable settings path.

Basically CSP is a drop in replacement for LFSP where the reading and writing of settings can be customized without reimplementing the settingsProvider logic. It is possible to customize either the settings path or to provide the CSP with XmlDocuments that may be serialized and deserialized to and from the net or other sources. A simple example that customizes the user settings path is provided below

```cs
public class MyUserSettingsPathProvider : ISettingsPathProvider
{
   string SettingsPath
   {
       get
       {
          return "c:\\csp.config"
       }
   }
}

public class MyCustomPathSettingsProvider :
  CustomPathSettingsProvider<
    DefaultProviders.FileSystemProvider,
    MyUserSettingsPathProvider,
    DefaultProviders.AppSettingsPathProvider,
    DefaultProviders.MachineSettingsPathProvider,

[SettingsProvider(typeof(MyCustomPathSettingsProvider)]
public class Settings : ApplicationSettingsBase
{
// settings go here ...
}
```

## Features

* read user and application scoped settings from user, app and machine config
* write user scoped settings to user.config
* customize serializing and deserializing of settings
* testsuite

## Todo

* support roaming profiles
* support Reset and GetPreviousVersion methods

## Install

The CSP is avaible as nuget.

## References

* [LFSP source]{https://github.com/Microsoft/referencesource/tree/master/System/sys/system/configuration}
* [Custom Settings Provider in .NET 2.0]{http://sellsbrothers.com/public/writing/dotnet2customsettingsprovider.htm}
* [Portable Settings Provider]{https://github.com/crdx/PortableSettingsProvider}
* [Chuck Rostance Settings Provider Implementation at StackOverflow]{https://stackoverflow.com/questions/2265271/custom-path-of-the-user-config}
* [MSDN SettingsProvide FAQ]{https://blogs.msdn.microsoft.com/rprabhu/2005/06/29/client-settings-faq/}
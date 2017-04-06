# Net.Appclusive.Net.Client
[![Build Status](https://build.dfch.biz/app/rest/builds/buildType:(id:CSharpDotNet_NetAppclusiveNetClient_Build)/statusIcon)](https://build.dfch.biz/project.html?projectId=CSharpDotNet_NetAppclusiveNetClient&tab=projectOverview)
[![License](https://img.shields.io/badge/license-Apache%20License%202.0-blue.svg)](https://github.com/Appclusive/Net.Appclusive.Net.Client/blob/master/LICENSE)
[![Version](https://img.shields.io/nuget/v/Net.Appclusive.Net.Client.svg)](https://www.nuget.org/packages/Net.Appclusive.Net.Client/)
[![Version](https://img.shields.io/nuget/v/Net.Appclusive.PS.Client.svg)](https://www.nuget.org/packages/Net.Appclusive.PS.Client/)

C#/.Net and PowerShell Clients for the Appclusive Blueprint Modelling and Automation Engine

d-fens GmbH, General-Guisan-Strasse 6, CH-6300 Zug, Switzerland

## Add/Update Service References

When adding a service reference proxy classes for all entity sets and complex types get generated with exactly the same name and namespace. The entity sets and complex types are already defined in `Net.Appclusive.Public`. Unfortunately it's not possible to reuse types from an assembly (i.e. from `Net.Appclusive.Public`). Referencing `Net.Appclusive.Public` and `Net.Appclusive.Api` in another project can cause problems as exactly the same types with the same namespace are defined twice. To workaround this problem the `alias` of the `Net.Appclusive.Api` reference can be changed as described [here](http://stackoverflow.com/questions/9194495/type-exists-in-2-assemblies/32038867#32038867).

**IMPORTANT**

After updating or adding a service reference the following steps have to be performed:

* Change service reference class in `Reference.cs` to extend from `DataServiceContextBase`

  ![Screenshot](https://github.com/Appclusive/Net.Appclusive.Net.Client/blob/develop/VS2015-screenshot.png)
  
  ```C#
  public partial class Core : DataServiceContextBase
  ```

* The duplicated proxy classes (i.e. `User`) have to be deleted in the corresponding `Reference.cs` files

## Download

### C#/.Net Client

Coming soon

### PowerShell Client

* Get it on [NuGet](https://www.nuget.org/packages/Net.Appclusive.PS.Client/)

* Get it on [PowerShellGallery](https://www.powershellgallery.com/packages/Net.Appclusive.PS.Client)

* See [Releases](https://github.com/Appclusive/Net.Appclusive.Net.Client/releases) and [Tags](https://github.com/Appclusive/Net.Appclusive.Net.Client/tags) on [GitHub](https://github.com/Appclusive/Net.Appclusive.Net.Client)


[![TeamCity Logo](https://github.com/Appclusive/Net.Appclusive.Net.Client/blob/master/TeamCity.png)](https://www.jetbrains.com/teamcity/)

Released with TeamCity

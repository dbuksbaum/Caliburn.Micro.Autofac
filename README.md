Caliburn.Micro.Autofac
======================
                       
Source code to the [Caliburn.Micro.Autofac nuget package](http://nuget.org/List/Packages/Caliburn.Micro.Autofac).
- [Caliburn.Micro.Autofac](http://buksbaum.us/things-i-did/caliburn-micro-autofac/)
- [Dave's blog](http://buksbaum.us/)

__Version 1.4 - 2012/05/19__
  * Updated NUnit to 2.6
  * Minor project file updates to support TeamCity CI
  * Cleaned up AssemblyInfo
  * Modified .gitignore to support NuGet PackageRestore
  * Deprecated psake build scripts. These will be removed in the next release.
  * Updated Caliburn.Micro to 1.3.1
  * Updated Autofac to 2.6.1.841
  * Dropped support for Windows Phone 7.0
  * Added support for Windows Phone 7.1

__Version 1.3 - 2011/08/19__
  * Added AutoSubscribeEventAggegatorHandlers as a new property to AutofacBoostrapper
  * Defaults to false to maintain backwards compatibility
  * When true, will register the EventAggregationAutoSubscriptionModule module with Autofac
  * Added the EventAggregationAutoSubscriptionModule
  * This module will automaticaly subscribe new instances to the registered IEventAggregator services
  * Registration is done on activation only, so it will only happen once for singletons, and each time a non-singleton is created
  * Registration is only done if the instance derives from the IHandle marker interface

__Version 1.2.1 - 2011/08/11__
  * Fixed missing references in Caliburn.Micro.Autofac for .NET 4
  * Fixed incorrect AssemblyInfo.cs values for all projects

__Version 1.2 - 2011/08/10__
  * Upgraded .NET 4 and Silverlight 4 versions to Caliburn.Micro 1.2.0
  * Created a psake build script to be used in Continious Integration
  * Added new sample applications
  * First version of Windows Phone 7 support
  * Known Issues
    * The SimpleContainer class is not supported or exposed by the container 
    * The PhoneContainer class is not supported or exposed by the container
    * The IPhoneContainer class is not supported or exposed by the container
  * Exposed new Creators for the Windows Phone Bootstrapper
    * CreateFrameAdapter
    * CreatePhoneApplicationServiceAdapter
    * CreateVibrateController
    * CreateSoundEffectPlayer

__Version 1.1 - 2011/07/11__
  * Cleaned up source directories slightly
  * Refactored AssemblyInfo.cs into GlobalAssemblyInfo.cs, VersionAssemblyInfo.cs, and project AssemblyInfo.cs
  * Upgraded Autofac to 2.5.1
  * Added support for Autofac SL4
  * Began work to support Autofac on Windows Phone 7
  * Modified GetInterface call in Configure to be compatible with SL4
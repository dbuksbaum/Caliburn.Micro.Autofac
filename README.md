Caliburn.Micro.Autofac
======================
                       
Source code to the [Caliburn.Micro.Autofac nuget package](http://nuget.org/List/Packages/Caliburn.Micro.Autofac).
See [blog post](http://buksbaum.us/2011/06/12/introducing-caliburn-micro-autofac/)

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
using Caliburn.Micro.Autofac;

namespace Sample_WPF
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Caliburn.Micro;

  public class AppBootstrapper : AutofacBootstrapper<ShellViewModel>
  {
    protected override void ConfigureBootstrapper()
    {
      //  you must call the base version first!
      base.ConfigureBootstrapper();

      //  override namespace naming convention
      EnforceNamespaceConvention = false;
      //  change our view model base type
      ViewModelBaseType = typeof(IShell);
    }
  }
}

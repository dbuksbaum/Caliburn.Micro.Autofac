using Caliburn.Micro.Autofac;

namespace Sample_SL4
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel.Composition;
  using System.ComponentModel.Composition.Hosting;
  using System.ComponentModel.Composition.Primitives;
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

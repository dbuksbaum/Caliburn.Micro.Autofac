// Проект: WindowsPhoneTestApplication
// Имя файла: ApplicationBootstrapper.cs
// GUID файла: C16FE24E-6DE4-403E-A522-A273B4478CBF
// Автор: Mike Eshva (mike@eshva.ru)
// Дата создания: 14.05.2012

using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using WindowsPhoneTestApplication.Models;


namespace WindowsPhoneTestApplication.Framework
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationBootstrapper : AutofacBootstrapper
    {
        #region Protected interface

        protected override void ConfigureContainer(ContainerBuilder aBuilder)
        {
            aBuilder.RegisterType<DebugLog>().As<ILog>();
            // IMPORTANT: Types you plan to store using EntireGraph<T>().InPhoneState() 
            // must be registered within the container.
            aBuilder.RegisterType<MainModel>().AsSelf();
        }

        

        #endregion
    }
}
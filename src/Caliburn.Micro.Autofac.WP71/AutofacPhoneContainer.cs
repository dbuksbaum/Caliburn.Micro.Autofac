// Project: Caliburn.Micro.Autofac
// File name: AutofacBootstrapper.cs
// File GUID: CB807DF7-2D69-4B05-AC17-C3E191D7C182
// Authors: Mike Eshva (mike@eshva.ru)
// Date of creation: 20.05.2012

using System;
using System.IO.IsolatedStorage;
using Autofac;


namespace Caliburn.Micro.Autofac
{
    /// <summary>
    /// Component that allows storage system to communicate with Autofac IoC container.
    /// </summary>
    internal class AutofacPhoneContainer : IPhoneContainer
    {
        private readonly IComponentContext context;

        public AutofacPhoneContainer(IComponentContext context)
        {
            this.context = context;
        }

        public event Action<object> Activated = _ => { };
        public void RegisterWithAppSettings(Type service, string appSettingsKey, Type implementation)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(appSettingsKey ?? service.FullName))
            {
                IsolatedStorageSettings.ApplicationSettings[appSettingsKey ?? service.FullName] = context.Resolve(implementation);
            }

            // Effectively replace service registration in 
            // Autofac-container (the last registration wins).
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterType(implementation)
                .As(service)
                .OnActivating(
                // Replace an object build by the container with one 
                // from the application isolated storage using OnActivating event.
                args =>
                {
                    if (IsolatedStorageSettings.ApplicationSettings.Contains(appSettingsKey))
                    {
                        object instance =
                            IsolatedStorageSettings.ApplicationSettings[appSettingsKey];
                        args.ReplaceInstance(instance);
                    }
                });
            builder.Update(context.ComponentRegistry);
        }

        public void RegisterWithPhoneService(Type service, string phoneStateKey, Type implementation)
        {
            string objectKey = phoneStateKey ?? service.FullName;
            IPhoneService phoneService = context.Resolve<IPhoneService>();

            if (!phoneService.State.ContainsKey(objectKey))
            {
                // There is no object with such a key in the phone state.
                // Create a plain new object and store it the phone state.
                object entireGraphObject = context.Resolve(service);
                phoneService.State[objectKey] = entireGraphObject;
                // Effectively replace service registration in 
                // Autofac-container (the last registration wins).
                ContainerBuilder builder = new ContainerBuilder();
                builder.RegisterType(implementation)
                    .As(service)
                    .OnActivating(
                    // Replace an object build by the container with one 
                    // from the phone state using OnActivating event.
                    args =>
                    {
                        IPhoneService innerPhoneService =
                            args.Context.Resolve<IPhoneService>();

                        if (innerPhoneService.State.ContainsKey(objectKey))
                        {
                            object instance = innerPhoneService.State[objectKey];
                            args.ReplaceInstance(instance);
                        }
                    });
                builder.Update(context.ComponentRegistry);
            }
        }


        #region Assembly interface

        /// <summary>
        /// Activates <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">
        /// Instance to activate.
        /// </param>
        internal void ActivateInstance(object instance)
        {
            Activated(instance);
        }

        #endregion

    }
}
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
    public class AutofacPhoneContainer : IPhoneContainer
    {
        #region Constructors

        /// <summary>
        /// Initialize new instance of component that allows storage system to communicate with
        /// Autofac IoC container.
        /// </summary>
        /// <param name="aContext">
        /// Autofac IoC context.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Autofac IoC context not specified.
        /// </exception>
        public AutofacPhoneContainer(IComponentContext aContext)
        {
            if (aContext == null)
            {
                throw new ArgumentNullException("aContext", "Autofac IoC context is not specified.");
            }

            Context = aContext;
        }

        #endregion

        #region IPhoneContainer Members

        /// <summary>
        /// Registers the service as a singleton stored in the phone state.
        /// </summary>
        /// <param name="aService">
        /// The service.
        /// </param>
        /// <param name="aPhoneStateKey">
        /// The phone state key.
        /// </param>
        /// <param name="aImplementation">
        /// The implementation.
        /// </param>
        public void RegisterWithPhoneService
            (Type aService, string aPhoneStateKey, Type aImplementation)
        {
            string lObjectKey = aPhoneStateKey ?? aService.FullName;
            IPhoneService lPhoneService = Context.Resolve<IPhoneService>();

            if (!lPhoneService.State.ContainsKey(lObjectKey))
            {
                // There is no object with such a key in the phone state.
                // Create a plain new object and store it the phone state.
                object lEntireGraphObject = Context.Resolve(aService);
                lPhoneService.State[lObjectKey] = lEntireGraphObject;
                // Effectively replace service registration in 
                // Autofac-container (the last registration wins).
                ContainerBuilder lBuilder = new ContainerBuilder();
                lBuilder.RegisterType(aImplementation)
                    .As(aService)
                    .OnActivating(
                    // Replace an object build by the container with one 
                    // from the phone state using OnActivating event.
                    aArgs =>
                    {
                        IPhoneService lInnerPhoneService =
                            aArgs.Context.Resolve<IPhoneService>();

                        if (lInnerPhoneService.State.ContainsKey(lObjectKey))
                        {
                            object lInstance = lInnerPhoneService.State[lObjectKey];
                            aArgs.ReplaceInstance(lInstance);
                        }
                    });
                lBuilder.Update(Context.ComponentRegistry);
            }
        }

        /// <summary>
        /// Registers the service as a singleton stored in the app settings.
        /// </summary>
        /// <param name="aService">
        /// The service.
        /// </param>
        /// <param name="aPhoneSettingsKey">
        /// The app settings key.
        /// </param>
        /// <param name="aImplementation">
        /// The implementation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Service or implementation not specified.
        /// </exception>
        public void RegisterWithAppSettings
            (Type aService, string aPhoneSettingsKey, Type aImplementation)
        {
            string lObjectKey = aPhoneSettingsKey ?? aService.FullName;

            if (!IsolatedStorageSettings.ApplicationSettings.Contains(lObjectKey))
            {
                // There is no object with such a key in the application isolated storage.
                // Probably it is the first time application executed.
                // Create a plain new object and store it the application isolated storage.
                object lEntireGraphObject = Context.Resolve(aImplementation);
                IsolatedStorageSettings.ApplicationSettings[lObjectKey] = lEntireGraphObject;
            }

            // Effectively replace service registration in 
            // Autofac-container (the last registration wins).
            ContainerBuilder lBuilder = new ContainerBuilder();
            lBuilder.RegisterType(aImplementation)
                .As(aService)
                .OnActivating(
                // Replace an object build by the container with one 
                // from the application isolated storage using OnActivating event.
                aArgs =>
                {
                    if (IsolatedStorageSettings.ApplicationSettings.Contains(lObjectKey))
                    {
                        object lInstance =
                            IsolatedStorageSettings.ApplicationSettings[lObjectKey];
                        aArgs.ReplaceInstance(lInstance);
                    }
                });
            lBuilder.Update(Context.ComponentRegistry);
        }

        /// <summary>
        /// Occurs when a new instance is created.
        /// </summary>
        public event Action<object> Activated = delegate { };

        #endregion

        #region Assembly interface

        /// <summary>
        /// Activates <paramref name="aInstance"/>.
        /// </summary>
        /// <param name="aInstance">
        /// Instance to activate.
        /// </param>
        internal void ActivateInstance(object aInstance)
        {
            Activated(aInstance);
        }

        #endregion

        #region Private properties

        private IComponentContext Context { get; set; }

        #endregion
    }
}
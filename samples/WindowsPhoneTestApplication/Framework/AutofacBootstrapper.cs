// Project: Caliburn.Micro.Autofac
// File name: AutofacBootstrapper.cs
// File GUID: C5C743E2-7AD1-496C-824A-9AEFBF5F8979
// Authors: David Buksbaum (david@buksbaum.us), Mike Eshva (mike@eshva.ru)
// Date of creation: 20.05.2012

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Autofac;
using Microsoft.Phone.Controls;
using IContainer = Autofac.IContainer;


namespace Caliburn.Micro.Autofac
{
    /// <summary>
    /// Autofac specific <see cref="PhoneBootstrapper"/> realization.
    /// </summary>
    public class AutofacBootstrapper : PhoneBootstrapper
    {
        #region Public properties

        /// <summary>
        /// Gets or sets flag should the namespace convention be enforced for type registration. The
        /// default is true. For views, this would require a views namespace to end with Views For
        /// view-models, this would require a view models namespace to end with ViewModels.
        /// </summary>
        /// <remarks>
        /// Case is important as views would not match.
        /// </remarks>
        public bool EnforceNamespaceConvention { get; set; }

        /// <summary>
        /// Gets or sets flag should the view be treated as loaded when registering the 
        /// <see cref="INavigationService"/>.
        /// </summary>
        public bool TreatViewAsLoaded { get; set; }

        /// <summary>
        /// Gets or sets the base type required for a view model.
        /// </summary>
        public Type ViewModelBaseType { get; set; }

        /// <summary>
        /// Gets or sets method for creating the window manager.
        /// </summary>
        public Func<IWindowManager> CreateWindowManager { get; set; }

        /// <summary>
        /// Gets or sets method for creating the event aggregator.
        /// </summary>
        public Func<IEventAggregator> CreateEventAggregator { get; set; }

        /// <summary>
        /// Gets or sets method for creating the frame adapter.
        /// </summary>
        public Func<FrameAdapter> CreateFrameAdapter { get; set; }

        /// <summary>
        /// Gets or sets method for creating the phone application service adapter.
        /// </summary>
        public Func<PhoneApplicationServiceAdapter> CreatePhoneApplicationServiceAdapter { get; set; }

        /// <summary>
        /// Gets or sets method for creating the vibrate controller.
        /// </summary>
        public Func<IVibrateController> CreateVibrateController { get; set; }

        /// <summary>
        /// Gets or sets method for creating the sound effect player.
        /// </summary>
        public Func<ISoundEffectPlayer> CreateSoundEffectPlayer { get; set; }

        #endregion

        #region Protected interface

        /// <summary>
        /// Gets Autofac container instance.
        /// </summary>
        protected IContainer Container { get; private set; }

        /// <summary>
        /// Do not override this method. This is where the IoC container is configured.
        /// </summary>
        /// <exception cref="NullReferenceException">
        /// Either CreateFrameAdapter or CreateWindowManager or CreateEventAggregator or
        /// CreatePhoneApplicationServiceAdapter or CreateVibrateController or
        /// CreateSoundEffectPlayer is null.
        /// </exception>
        protected override void Configure()
        {
            //  allow base classes to change bootstrapper settings
            ConfigureBootstrapper();

            //  validate settings
            if (CreateFrameAdapter == null)
            {
                throw new NullReferenceException("CreateFrameAdapter is not specified.");
            }
            if (CreateWindowManager == null)
            {
                throw new NullReferenceException("CreateWindowManager is not specified.");
            }
            if (CreateEventAggregator == null)
            {
                throw new NullReferenceException("CreateEventAggregator is not specified.");
            }
            if (CreatePhoneApplicationServiceAdapter == null)
            {
                throw new NullReferenceException(
                    "CreatePhoneApplicationServiceAdapter is not specified.");
            }
            if (CreateVibrateController == null)
            {
                throw new NullReferenceException("CreateVibrateController is not specified.");
            }
            if (CreateSoundEffectPlayer == null)
            {
                throw new NullReferenceException("CreateSoundEffectPlayer is not specified.");
            }

            // Configure container.
            ContainerBuilder lBuilder = new ContainerBuilder();

            // Register phone services.
            Assembly lCaliburnAssembly = typeof (IStorageMechanism).Assembly;
            // Register IStorageMechanism implementors.
            lBuilder.RegisterAssemblyTypes(lCaliburnAssembly)
                .Where(
                    aType => typeof (IStorageMechanism).IsAssignableFrom(aType)
                             && !aType.IsAbstract
                             && !aType.IsInterface)
                .As<IStorageMechanism>()
                .InstancePerLifetimeScope();

            // Register IStorageHandler implementors.
            lBuilder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(
                    aType => typeof (IStorageHandler).IsAssignableFrom(aType)
                             && !aType.IsAbstract
                             && !aType.IsInterface)
                .As<IStorageHandler>()
                .InstancePerLifetimeScope();

            // Register view models.
            lBuilder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                // Must be a type with a name that ends with ViewModel.
                .Where(aType => aType.Name.EndsWith("ViewModel"))
                // Mmust be in a namespace ending with ViewModels.
                .Where(
                    aType =>
                    !EnforceNamespaceConvention ||
                    (!string.IsNullOrEmpty(aType.Namespace) &&
                     aType.Namespace.EndsWith("ViewModels")))
                // Must implement INotifyPropertyChanged (deriving from PropertyChangedBase will statisfy this).
                .Where(aType => aType.GetInterface(ViewModelBaseType.Name, false) != null)
                // Registered as self.
                .AsSelf()
                // Subscribe on Activated event for viewmodels to make storage mechanism work.
                .OnActivated(aArgs => ActivateInstance(aArgs.Instance))
                // Always create a new one.
                .InstancePerDependency();

            // Register views.
            lBuilder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                // Must be a type with a name that ends with View.
                .Where(aType => aType.Name.EndsWith("View"))
                // Must be in a namespace that ends in Views.
                .Where(
                    aType =>
                    !EnforceNamespaceConvention ||
                    (!string.IsNullOrEmpty(aType.Namespace) &&
                     aType.Namespace.EndsWith("Views")))
                // Registered as self.
                .AsSelf()
                // Always create a new one.
                .InstancePerDependency();

            // Register the singletons.
            lBuilder.Register<IPhoneContainer>(
                aContext => new AutofacPhoneContainer(aContext.Resolve<IComponentContext>()))
                .SingleInstance();
            lBuilder.RegisterInstance<INavigationService>(CreateFrameAdapter())
                .SingleInstance();
            PhoneApplicationServiceAdapter lPhoneService = CreatePhoneApplicationServiceAdapter();
            lBuilder.RegisterInstance<IPhoneService>(lPhoneService)
                .SingleInstance();

            lBuilder.Register(aContext => CreateEventAggregator())
                .SingleInstance();
            lBuilder.Register(aContext => CreateWindowManager())
                .SingleInstance();
            lBuilder.Register(aContext => CreateVibrateController())
                .SingleInstance();
            lBuilder.Register(aContext => CreateSoundEffectPlayer())
                .SingleInstance();
            lBuilder.RegisterType<StorageCoordinator>().AsSelf()
                .SingleInstance();
            lBuilder.RegisterType<TaskController>().AsSelf()
                .SingleInstance();

            // Allow derived classes to add to the container.
            ConfigureContainer(lBuilder);

            // Build the container
            Container = lBuilder.Build();
            // Get the phone container instance.
            PhoneContainer = (AutofacPhoneContainer) Container.Resolve<IPhoneContainer>();
            // Start the storage coordinator.
            StorageCoordinator lStorageCoordinator = Container.Resolve<StorageCoordinator>();
            lStorageCoordinator.Start();
            // Start the task controller.
            TaskController lTaskController = Container.Resolve<TaskController>();
            lTaskController.Start();
            // Add custom conventions for the phone.
            AddCustomConventions();
        }

        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="aService">
        /// The service to locate.
        /// </param>
        /// <param name="aKey">
        /// The key to locate.
        /// </param>
        /// <returns>
        /// The located service.
        /// </returns>
        /// <exception cref="Exception">
        /// Could not locate any instances of service.
        /// </exception>
        protected override object GetInstance(Type aService, string aKey)
        {
            object lInstance;
            if (string.IsNullOrEmpty(aKey))
            {
                if (Container.TryResolve(aService, out lInstance))
                {
                    return lInstance;
                }
            }
            else
            {
                if (Container.TryResolveNamed(aKey, aService, out lInstance))
                {
                    return lInstance;
                }
            }

            throw new Exception(
                string.Format(
                    "Could not locate any instances of service {0}.", aKey ?? aService.Name));
        }

        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="aService">
        /// The service to locate.
        /// </param>
        /// <returns>
        /// The located services.
        /// </returns>
        protected override IEnumerable<object> GetAllInstances(Type aService)
        {
            IEnumerable<object> lResult =
                Container.Resolve(typeof (IEnumerable<>).MakeGenericType(aService)) as
                IEnumerable<object>;
            return lResult;
        }

        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="aInstance">
        /// The instance to perform injection on.
        /// </param>
        protected override void BuildUp(object aInstance)
        {
            Container.InjectProperties(aInstance);
        }

        /// <summary>
        /// Override to provide configuration prior to the Autofac configuration. You must call the base version BEFORE any 
        /// other statement or the behaviour is undefined.
        /// Current Defaults:
        ///   EnforceNamespaceConvention = true
        ///   TreatViewAsLoaded = false
        ///   ViewModelBaseType = <see cref="System.ComponentModel.INotifyPropertyChanged"/> 
        ///   CreateWindowManager = <see cref="Caliburn.Micro.WindowManager"/> 
        ///   CreateEventAggregator = <see cref="Caliburn.Micro.EventAggregator"/>
        ///   CreateFrameAdapter = <see cref="Caliburn.Micro.FrameAdapter"/>
        ///   CreatePhoneApplicationServiceAdapter = <see cref="Caliburn.Micro.PhoneApplicationServiceAdapter"/>
        ///   CreateVibrateController = <see cref="Caliburn.Micro.SystemVibrateController"/>
        ///   CreateSoundEffectPlayer = <see cref="Caliburn.Micro.XnaSoundEffectPlayer"/>
        /// </summary>
        protected virtual void ConfigureBootstrapper()
        {
            // By default, enforce the namespace convention.
            EnforceNamespaceConvention = true;
            // By default, do not treat the view as loaded.
            TreatViewAsLoaded = false;
            // The default view model base type.
            ViewModelBaseType = typeof (INotifyPropertyChanged);
            // Default window manager.
            CreateWindowManager = () => new WindowManager();
            // Default event aggregator.
            CreateEventAggregator = () => new EventAggregator();
            // Default frame adapter.
            CreateFrameAdapter = () => new FrameAdapter(RootFrame, TreatViewAsLoaded);
            // Default phone application service adapter.
            CreatePhoneApplicationServiceAdapter =
                () => new PhoneApplicationServiceAdapter(RootFrame);
            // Default vibrate controller.
            CreateVibrateController = () => new SystemVibrateController();
            // Default sound effect player.
            CreateSoundEffectPlayer = () => new XnaSoundEffectPlayer();
        }

        /// <summary>
        /// Override to include your own Autofac configuration after the framework has finished its
        /// configuration, but  before the container is created.
        /// </summary>
        /// <param name="aBuilder">
        /// The Autofac configuration builder.
        /// </param>
        protected virtual void ConfigureContainer(ContainerBuilder aBuilder)
        {
        }

        /// <summary>
        /// Activates an instance in phone container. Derived types should call this method for
        /// registering types that should support storage mechanism.
        /// </summary>
        /// <param name="aInstance">
        /// Instance to activate.
        /// </param>
        /// <remarks>
        /// Use this method as event handler on service registation in 
        /// <see cref="ConfigureContainer"/> method. ViewModels already registering with using it.
        /// </remarks>
        protected void ActivateInstance(object aInstance)
        {
            if (PhoneContainer == null)
            {
                return;
            }

            PhoneContainer.ActivateInstance(aInstance);
        }

        #endregion

        #region Private properties

        private AutofacPhoneContainer PhoneContainer { get; set; }

        #endregion

        #region Private methods

        private static void AddCustomConventions()
        {
            ConventionManager.AddElementConvention<Pivot>(
                ItemsControl.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
                (aViewModelType, aPath, aProperty, aElement, aConvention) =>
                    {
                        if (ConventionManager
                            .GetElementConvention(typeof (ItemsControl))
                            .ApplyBinding(aViewModelType, aPath, aProperty, aElement, aConvention))
                        {
                            ConventionManager
                                .ConfigureSelectedItem(
                                    aElement, Pivot.SelectedItemProperty, aViewModelType, aPath);
                            ConventionManager
                                .ApplyHeaderTemplate(
                                    aElement, Pivot.HeaderTemplateProperty, null, aViewModelType);
                            return true;
                        }

                        return false;
                    };

            ConventionManager.AddElementConvention<Panorama>(
                ItemsControl.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
                (aViewModelType, aPath, aProperty, aElement, aConvention) =>
                    {
                        if (ConventionManager
                            .GetElementConvention(typeof (ItemsControl))
                            .ApplyBinding(aViewModelType, aPath, aProperty, aElement, aConvention))
                        {
                            ConventionManager
                                .ConfigureSelectedItem(
                                    aElement, Panorama.SelectedItemProperty, aViewModelType, aPath);
                            ConventionManager
                                .ApplyHeaderTemplate(
                                    aElement, Panorama.HeaderTemplateProperty, null, aViewModelType);
                            return true;
                        }

                        return false;
                    };
        }

        #endregion
    }
}
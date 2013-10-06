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
    public class AutofacBootstrapper : PhoneBootstrapper
    {
        #region Properties
        protected IContainer Container { get; private set; }
        /// <summary>
        /// Should the namespace convention be enforced for type registration. The default is true.
        /// For views, this would require a views namespace to end with Views
        /// For view-models, this would require a view models namespace to end with ViewModels
        /// <remarks>Case is important as views would not match.</remarks>
        /// </summary>
        public bool EnforceNamespaceConvention { get; set; }

        /// <summary>
        /// Should the view be treated as loaded when registering the INavigationService.
        /// </summary>
        public bool TreatViewAsLoaded { get; set; }

        /// <summary>
        /// The base type required for a view model
        /// </summary>
        public Type ViewModelBaseType { get; set; }
        /// <summary>
        /// Method for creating the window manager
        /// </summary>
        public Func<IWindowManager> CreateWindowManager { get; set; }
        /// <summary>
        /// Method for creating the event aggregator
        /// </summary>
        public Func<IEventAggregator> CreateEventAggregator { get; set; }
        //  Method for creating the frame adapter
        public Func<FrameAdapter> CreateFrameAdapter { get; set; }
        //  Method for creating the phone application service adapter
        public Func<PhoneApplicationServiceAdapter> CreatePhoneApplicationServiceAdapter { get; set; }
        //  Method for creating the vibrate controller
        public Func<IVibrateController> CreateVibrateController { get; set; }
        //  Method for creating the sound effect player
        public Func<ISoundEffectPlayer> CreateSoundEffectPlayer { get; set; }
        #endregion

        /// <summary>
        /// Do not override this method. This is where the IoC container is configured.
        /// <remarks>
        /// Will throw <see cref="System.ArgumentNullException"/> is either CreateWindowManager
        /// or CreateEventAggregator is null.
        /// </remarks>
        /// </summary>
        protected override void Configure()
        {
            //  allow base classes to change bootstrapper settings
            ConfigureBootstrapper();

            //  validate settings
            if (CreateFrameAdapter == null)
                throw new ArgumentNullException("CreateFrameAdapter");
            if (CreateWindowManager == null)
                throw new ArgumentNullException("CreateWindowManager");
            if (CreateEventAggregator == null)
                throw new ArgumentNullException("CreateEventAggregator");
            if (CreatePhoneApplicationServiceAdapter == null)
                throw new ArgumentNullException("CreatePhoneApplicationServiceAdapter");
            if (CreateVibrateController == null)
                throw new ArgumentNullException("CreateVibrateController");
            if (CreateSoundEffectPlayer == null)
                throw new ArgumentNullException("CreateSoundEffectPlayer");

            // Configure container.
            var builder = new ContainerBuilder();

            //  register phone services
            var caliburnAssembly = typeof(IStorageMechanism).Assembly;
            //  register IStorageMechanism implementors
            builder.RegisterAssemblyTypes(caliburnAssembly)
              .Where(type => typeof(IStorageMechanism).IsAssignableFrom(type)
                             && !type.IsAbstract
                             && !type.IsInterface)
              .As<IStorageMechanism>()
              .InstancePerLifetimeScope();

            // Register IStorageHandler implementors.
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
              .Where(type => typeof(IStorageHandler).IsAssignableFrom(type)
                             && !type.IsAbstract
                             && !type.IsInterface)
              .As<IStorageHandler>()
              .InstancePerLifetimeScope();

            // Register view models.
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                // Must be a type with a name that ends with ViewModel.
                .Where(type => type.Name.EndsWith("ViewModel"))
                // Must be in a namespace ending with ViewModels.
                .Where(
                    type =>
                    !EnforceNamespaceConvention ||
                    (!string.IsNullOrEmpty(type.Namespace) &&
                     type.Namespace.EndsWith("ViewModels")))
                //  must implement INotifyPropertyChanged (deriving from PropertyChangedBase will statisfy this)
              .Where(type => type.GetInterface(ViewModelBaseType.Name, false) != null)
                //  registered as self
              .AsSelf()
                .OnActivated(args => ActivateInstance(args.Instance))
                // Always create a new one.
                .InstancePerDependency();

            // Register views.
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                // Must be a type with a name that ends with View.
                .Where(type => type.Name.EndsWith("View"))
                // Must be in a namespace that ends in Views.
                .Where(
                    type =>
                    !EnforceNamespaceConvention ||
                    (!string.IsNullOrEmpty(type.Namespace) &&
                     type.Namespace.EndsWith("Views")))
                // Registered as self.
                .AsSelf()
                // Always create a new one.
                .InstancePerDependency();

            //  register as CM container
            //builder.RegisterInstance<SimpleContainer>(this).InstancePerLifetimeScope();
            //builder.RegisterInstance<PhoneContainer>(this).InstancePerLifetimeScope();
            //builder.RegisterInstance<IPhoneContainer>(this).InstancePerLifetimeScope();

            // The constructor of these services must be called
            // to attach to the framework properly.
            var phoneService = CreatePhoneApplicationServiceAdapter();
            var navigationService = CreateFrameAdapter();

            //  register the singletons
            builder.Register<IPhoneContainer>(c => new AutofacPhoneContainer(c)).InstancePerLifetimeScope();
            builder.RegisterInstance<INavigationService>(navigationService).SingleInstance();
            builder.RegisterInstance<IPhoneService>(phoneService).SingleInstance();
            builder.Register<IEventAggregator>(c => CreateEventAggregator()).InstancePerLifetimeScope();
            builder.Register<IWindowManager>(c => CreateWindowManager()).InstancePerLifetimeScope();
            builder.Register<IVibrateController>(c => CreateVibrateController()).InstancePerLifetimeScope();
            builder.Register<ISoundEffectPlayer>(c => CreateSoundEffectPlayer()).InstancePerLifetimeScope();
            builder.RegisterType<StorageCoordinator>().AsSelf().InstancePerLifetimeScope();
            builder.RegisterType<TaskController>().AsSelf().InstancePerLifetimeScope();

            // Allow derived classes to add to the container.
            ConfigureContainer(builder);

            // Build the container
            Container = builder.Build();
            // Get the phone container instance.
            PhoneContainer = (AutofacPhoneContainer)Container.Resolve<IPhoneContainer>();
            // Start the storage coordinator.
            StorageCoordinator storageCoordinator = Container.Resolve<StorageCoordinator>();
            storageCoordinator.Start();
            // Start the task controller.
            TaskController taskController = Container.Resolve<TaskController>();
            taskController.Start();
            // Add custom conventions for the phone.
            AddCustomConventions();
        }

        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <param name="key">The key to locate.</param>
        /// <returns>The located service.</returns>
        protected override object GetInstance(System.Type service, string key)
        {
            object instance;
            if (string.IsNullOrEmpty(key))
            {
                if (Container.TryResolve(service, out instance))
                    return instance;
            }
            else
            {
                if (Container.TryResolveNamed(key, service, out instance))
                    return instance;
            }
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? service.Name));
        }

        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="service">The service to locate.</param>
        /// <returns>The located services.</returns>
        protected override System.Collections.Generic.IEnumerable<object> GetAllInstances(System.Type service)
        {
            return Container.Resolve(typeof(IEnumerable<>).MakeGenericType(service)) as IEnumerable<object>;
        }
        /// <summary>
        /// Do not override unless you plan to full replace the logic. This is how the framework
        /// retrieves services from the Autofac container.
        /// </summary>
        /// <param name="instance">The instance to perform injection on.</param>
        protected override void BuildUp(object instance)
        {
            Container.InjectProperties(instance);
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
            ViewModelBaseType = typeof(INotifyPropertyChanged);
            // Default window manager.
            CreateWindowManager = () => new WindowManager();
            // Default event aggregator.
            CreateEventAggregator = () => new EventAggregator();
            // Default frame adapter.
            CreateFrameAdapter = () => new FrameAdapter(RootFrame, TreatViewAsLoaded);
            // Default phone application service adapter.
            CreatePhoneApplicationServiceAdapter = () => new PhoneApplicationServiceAdapter(PhoneService, RootFrame);
            // Default vibrate controller.
            CreateVibrateController = () => new SystemVibrateController();
            // Default sound effect player.
            CreateSoundEffectPlayer = () => new XnaSoundEffectPlayer();
        }

        /// <summary>
        /// Override to include your own Autofac configuration after the framework has finished its
        /// configuration, but  before the container is created.
        /// </summary>
        /// <param name="builder">
        /// The Autofac configuration builder.
        /// </param>
        protected virtual void ConfigureContainer(ContainerBuilder builder)
        {
        }

        /// <summary>
        /// Activates an instance in phone container. Derived types should call this method for
        /// registering types that should support storage mechanism.
        /// </summary>
        /// <param name="instance">
        /// Instance to activate.
        /// </param>
        /// <remarks>
        /// Use this method as event handler on service registation in 
        /// <see cref="ConfigureContainer"/> method. ViewModels already registering with using it.
        /// </remarks>
        protected void ActivateInstance(object instance)
        {
            if (PhoneContainer == null)
            {
                return;
            }

            PhoneContainer.ActivateInstance(instance);
        }


        private AutofacPhoneContainer PhoneContainer { get; set; }


        static void AddCustomConventions()
        {
            ConventionManager.AddElementConvention<Pivot>(Pivot.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
                (viewModelType, path, property, element, convention) =>
                {
                    if (ConventionManager
                        .GetElementConvention(typeof(ItemsControl))
                        .ApplyBinding(viewModelType, path, property, element, convention))
                    {
                        ConventionManager
                            .ConfigureSelectedItem(element, Pivot.SelectedItemProperty, viewModelType, path);
                        ConventionManager
                            .ApplyHeaderTemplate(element, Pivot.HeaderTemplateProperty, null, viewModelType);
                        return true;
                    }

                    return false;
                };

            ConventionManager.AddElementConvention<Panorama>(Panorama.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
                (viewModelType, path, property, element, convention) =>
                {
                    if (ConventionManager
                        .GetElementConvention(typeof(ItemsControl))
                        .ApplyBinding(viewModelType, path, property, element, convention))
                    {
                        ConventionManager
                            .ConfigureSelectedItem(element, Panorama.SelectedItemProperty, viewModelType, path);
                        ConventionManager
                            .ApplyHeaderTemplate(element, Panorama.HeaderTemplateProperty, null, viewModelType);
                        return true;
                    }

                    return false;
                };
        }
    }
}
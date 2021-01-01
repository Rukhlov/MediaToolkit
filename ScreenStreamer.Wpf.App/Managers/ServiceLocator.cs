
//using Unity;
//using Unity.Lifetime;

namespace ScreenStreamer.Wpf
{

    public static class ServiceLocator
    {
        public static void RegisterInstance<T>(T t) where T : class
        {
            GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.Register<T>(() => t);
        }

        public static T GetInstance<T>(bool containsCreated = false) where T : class
        {

            if (containsCreated)
            {
                if (!GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.ContainsCreated<T>())
                {
                    return null;
                }
            }

            return GalaSoft.MvvmLight.Ioc.SimpleIoc.Default.GetInstance<T>();
        }
    }
    
    /*
    public static class _ServiceLocator
    {
        private static object _locker = new object();
        private static UnityContainer _container = new UnityContainer();

        public static void RegisterInstance<T>(T t) where T: class
        {
            _container.RegisterInstance<T>(t, new SingletonLifetimeManager());
        }

        public static void RegisterInstance<T>(T t, IInstanceLifetimeManager manager)
        {
            _container.RegisterInstance<T>(t, manager);
        }

        public static T GetInstance<T>()
        {

            return _container.Resolve<T>();
        }

        //public static void RegisterInstance<T>(T t, IInstanceLifetimeManager manager)
        //{
        //    _container.RegisterInstance<T>(t, manager);
        //}

        //public static IUnityContainer Container
        //{
        //    get
        //    {
        //        if (_container == null)
        //        {
        //            lock (_locker)
        //            {
        //                if (_container == null)
        //                {
        //                    _container = new UnityContainer();
        //                    RegisterDependencies(_container);
        //                }
        //            }
        //        }
        //        return _container;
        //    }
        //}

        //public static void RegisterDependencies(IUnityContainer unityContainer)
        //{

        //    //unityContainer.RegisterFactory<IMapper>(container => AutoMapperManager.BuildMapper(),  new SingletonLifetimeManager());

        //    unityContainer.RegisterType<IDialogService, StreamDialogService>(new ContainerControlledLifetimeManager());

        //    //var model = ConfigurationManager.LoadConfigurations();

        //    //unityContainer.RegisterInstance(model, new SingletonLifetimeManager());

        //    unityContainer.RegisterFactory<StreamMainModel>(container => ConfigManager.LoadConfigurations(), new SingletonLifetimeManager()); 
        //    unityContainer.RegisterFactory<IMessenger>(container => Messenger.Default, new SingletonLifetimeManager());

        //    //unityContainer.RegisterFactory<StreamMainViewModel>(container => new StreamMainViewModel(), new SingletonLifetimeManager());
        //}
    }
    */
}

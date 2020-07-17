
using Unity;
using Unity.Lifetime;

namespace ScreenStreamer.Wpf.Helpers
{
    public static class ServiceLocator
    {
        private static object _locker = new object();
        private static UnityContainer _container = new UnityContainer();

        public static void RegisterSingleton<T>(T t)
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
}

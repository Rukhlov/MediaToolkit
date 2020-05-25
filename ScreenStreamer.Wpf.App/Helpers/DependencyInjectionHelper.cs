
using GalaSoft.MvvmLight.Messaging;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Managers;
using ScreenStreamer.Wpf.Common.Models;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using ScreenStreamer.Wpf.Common.Services;
using Unity;
using Unity.Lifetime;

namespace ScreenStreamer.Wpf.Common.Helpers
{
    public static class DependencyInjectionHelper
    {
        private static object _locker = new object();
        private static UnityContainer _container;
        public static UnityContainer Container
        {
            get
            {
                if (_container == null)
                {
                    lock (_locker)
                    {
                        if (_container == null)
                        {
                            _container = new UnityContainer();
                            RegisterDependencies(_container);
                        }
                    }
                }
                return _container;
            }
        }

        public static void RegisterDependencies(IUnityContainer unityContainer)
        {
            //unityContainer.RegisterFactory<IMapper>(container => AutoMapperManager.BuildMapper(),  new SingletonLifetimeManager());
            unityContainer.RegisterType<IDialogService, StreamDialogService>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterFactory<StreamMainModel>(container => ConfigurationManager.LoadConfigurations(), new SingletonLifetimeManager()); 
            unityContainer.RegisterFactory<IMessenger>(container => Messenger.Default, new SingletonLifetimeManager());
            //unityContainer.RegisterFactory<StreamMainViewModel>(container => new StreamMainViewModel(), new SingletonLifetimeManager());
        }
    }
}

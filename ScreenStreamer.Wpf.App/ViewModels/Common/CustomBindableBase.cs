using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Messaging;
using Prism.Mvvm;
using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Views;
using Unity;

namespace ScreenStreamer.Wpf.Common.Models
{
    //public class CustomBindableBase : BindableBase
    //{
    //    protected bool SetProperty<TObject, TProperty>(TObject storageObject,
    //        Expression<Func<dynamic>> storageObjectPropertyExpression, TProperty value, [CallerMemberName] string propertyName = null)
    //    {
    //        var storagePropertyName = Polywall.Share.UI.PropertySupport.ExtractPropertyName(storageObjectPropertyExpression);

    //        dynamic val = Polywall.Share.UI.PropertySupport.GetPropertyValue<TObject, TProperty>(storageObject, storagePropertyName);
    //        if ((val as object)?.GetType().IsValueType ?? false && val != null && value != null)
    //        {
    //            if (val.Equals(value))
    //            {
    //                return false;
    //            }
    //        }
    //        else if (val == value)
    //            return false;

    //        Polywall.Share.UI.PropertySupport.SetPropertyValue(storageObject, storagePropertyName, value);

    //        {
    //            RaisePropertyChanged(propertyName);
    //            DependencyInjectionHelper.Container.Resolve<IMessenger>().Send<AcceptChangesMessage>(new AcceptChangesMessage
    //            {
    //                Model = this,
    //                ChangedProperty = propertyName
    //            });
    //        }

    //        return true;
    //    }
    //}
}
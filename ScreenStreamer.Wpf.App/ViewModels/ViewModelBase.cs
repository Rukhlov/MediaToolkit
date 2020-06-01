using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using NLog;
using ScreenStreamer.Wpf.Common.Helpers;

namespace Polywall.Share.UI
{
    public interface IStorableBladeModel : INotifyPropertyChanged
    {
        bool IsChanged { get; }
        bool HasErrors { get; }
        void ResetChanges();
    }


    public class ViewModelBase : GalaSoft.MvvmLight.ViewModelBase, IDataErrorInfo, IStorableBladeModel
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Type _trackNestedPropertiesAttr;
        
        public ViewModelBase(ViewModelBase parent = null, Type trackNestedPropertiesAttr = null)
        {
            _trackNestedPropertiesAttr = trackNestedPropertiesAttr;
            this.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName != nameof(IsChanged))
                {
                    RaisePropertyChanged(() => IsChanged);
                    parent?.RaisePropertyChanged(() => IsChanged);
                }
            };

            if (_trackNestedPropertiesAttr != null)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new Action(() => TrackNestedProperties())
                );
            }
        }

        public virtual void TrackNestedProperties(bool recursive = false)
        {
            var properties = this.GetType().GetProperties().Where(info => info.GetCustomAttribute(_trackNestedPropertiesAttr) != null);
            foreach (var property in properties)
            {
                var value = property.GetValue(this);
                _initialValues[property.Name] = value;
                if (property.PropertyType.GetInterface(nameof(IStorableBladeModel), true) == null)
                {
                    Invalidate(property.Name);
                }
                if (recursive)
                {
                    (value as ViewModelBase)?.TrackNestedProperties(recursive);
                    foreach (var modelBase in (value as IEnumerable<ViewModelBase>) ?? new ViewModelBase[] { })
                    {
                        modelBase?.TrackNestedProperties(recursive);
                    }
                }
            }
        }

        #region Changes Tracking

        private readonly IDictionary<string, dynamic> _initialValues = new Dictionary<string, dynamic>();

        public void TrackProperties(params Expression<Func<object>>[] expressions)
        {
            foreach (var expression in expressions)
            {
                var propertyName = PropertySupport.ExtractPropertyName(expression);
                var propertyValue = PropertySupport.GetPropertyValue<object, dynamic>(this, propertyName);

                _initialValues[propertyName] = propertyValue;
                OnValidate(propertyName);
            }
        }

        protected virtual bool CheckChanges()
        {
            foreach (var propertyName in _initialValues.Keys)
            {
                if (CheckIsPropertyChanged(propertyName))
                    return true;
            }

            return false;
        }

        public bool CheckIsPropertyChanged(string propertyName)
        {
            var initialValue = _initialValues[propertyName];
            var actualValue = PropertySupport.GetPropertyValue<object, dynamic>(this, propertyName);
            if (actualValue is IStorableBladeModel storableBladeModel)
            {
                return storableBladeModel.IsChanged;
            }
            else
            {

                return !PropertySupport.CompareHelper.AreEquals(actualValue, initialValue);
            }
        }

        public virtual void ResetChanges()
        {
            foreach (var propertyName in _initialValues.Keys)
            {
                var initialValue = _initialValues[propertyName];
                if (initialValue is IStorableBladeModel storableBladeModel)
                {
                    storableBladeModel.ResetChanges();
                }
                else
                {
                    PropertySupport.SetPropertyValue<object, dynamic>(this, propertyName, initialValue);
                }
            }
        }

        public T GetInitialValue<T>(string propertyName)
        {
            return (T)_initialValues[propertyName];
        }

        public void SetInitialValue<T>(string propertyName, T value)
        {
            _initialValues[propertyName] = value;
            //if (CheckIsPropertyChanged(propertyName))
            //    ChangedCallback?.Invoke(OnChangedArgs.Default);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Sets value to the Model object's property
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="storageObject"></param>
        /// <param name="storageObjectPropertyExpression"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <param name="userInput"></param>
        /// <returns></returns>
        protected bool SetProperty<TObject, TProperty>(TObject storageObject,
            Expression<Func<dynamic>> storageObjectPropertyExpression, TProperty value,
            [CallerMemberName] string propertyName = null, bool userInput = false)
        {
            if (userInput && !_userInput.Contains(propertyName))
            {
                _userInput.Add(propertyName);
            }

            var storagePropertyName = PropertySupport.ExtractPropertyName(storageObjectPropertyExpression);

            dynamic val = PropertySupport.GetPropertyValue<TObject, TProperty>(storageObject, storagePropertyName);
            if ((val as object)?.GetType().IsValueType ?? false && val != null && value != null)
            {
                if (val.Equals(value))
                {
                    return false;
                }
            }
            else if (val == value)
                return false;

            PropertySupport.SetPropertyValue(storageObject, storagePropertyName, value);

            {
                Invalidate(propertyName);
            }

            return true;
        }

        private readonly IDictionary<string, IEnumerable<ValidationResult>> _errors = new Dictionary<string, IEnumerable<ValidationResult>>();

        private readonly HashSet<string> _userInput = new HashSet<string>();
        private bool _isBusy;

        protected virtual void SetError(string propertyName, IEnumerable<ValidationResult> errors)
        {
            _errors[propertyName] = errors;

            RaisePropertyChanged(() => HasErrors);
        }

        protected virtual bool CheckErrors()
        {
            if (_errors.Any(errors => errors.Value != null && errors.Value.Any()) ||
                _initialValues.Values.OfType<IStorableBladeModel>().Any(model => model.HasErrors))
                return true;

            return false;
        }

        protected virtual string OnValidate(string propertyName)
        {
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (!_errors.ContainsKey(propertyName))
            {
                if (Validator.TryValidateProperty(
                    GetType().GetProperty(propertyName).GetValue(this)
                    , new ValidationContext(this)
                    {
                        MemberName = propertyName
                    }
                    , validationResults))
                    if (!validationResults.Any())
                    {
                        SetError(propertyName, null);
                        return string.Empty;
                    }

                SetError(propertyName, validationResults);
            }


            var compositeValidationResult = _errors[propertyName]?.FirstOrDefault();
            if (compositeValidationResult != null)
                return compositeValidationResult.ErrorMessage;

            return !_userInput.Contains(propertyName)
                ? string.Empty
                : _errors[propertyName]?.FirstOrDefault()?.ErrorMessage ?? string.Empty;
        }

        public void Invalidate(params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (_errors.ContainsKey(propertyName))
                    _errors.Remove(propertyName);

                RaisePropertyChanged(propertyName);
            }
        }

        #endregion

        public bool HasErrors => CheckErrors();

        public bool IsChanged => CheckChanges();

        #region IDataErrorInfo

        public string this[string propertyName] => OnValidate(propertyName);

        public string Error => null;

        #endregion

        public bool IsBusy
        {
            get { return _isBusy; }
            set { Set(() => IsBusy, ref _isBusy, value); }
        }


        protected async Task PerformTaskAsync(Func<Task> func)
        {
            try
            {
                IsBusy = true;

                await func();
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected async Task<T> PerformTaskAsync<T>(Func<Task<T>> func)
        {
            try
            {
                IsBusy = true;

                return await func();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
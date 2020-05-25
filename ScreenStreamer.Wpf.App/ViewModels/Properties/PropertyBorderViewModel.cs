using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyBorderViewModel : PropertyBaseViewModel
    {
        private readonly PropertyBorderModel _model;
        public override string Name => "Show Border";

        #region IsBorderVisible
        [Track]
        public bool IsBorderVisible
        {
            get => _model.IsBorderVisible;
            set
            {
                SetProperty(_model, () => _model.IsBorderVisible, value);
                DialogService.Handle(_model.IsBorderVisible, Parent.IsStarted ? Parent.BorderViewModel : (IDialogViewModel)Parent.DesignViewModel);
            }
        }
        #endregion IsBorderVisible

        public PropertyBorderViewModel(StreamViewModel parent, PropertyBorderModel model) : base(parent)
        {
            _model = model;
        }

        protected override IDialogViewModel BuildDialogViewModel()
        {
            return new BorderSettingsViewModel(this, Parent);
        }
    }
}

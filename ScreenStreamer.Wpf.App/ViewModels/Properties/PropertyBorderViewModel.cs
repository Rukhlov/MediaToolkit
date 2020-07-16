using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyBorderViewModel : PropertyBaseViewModel
    {
        private readonly PropertyBorderModel _model;
        public override string Name => "Show Border";


        //[Track]
        public bool IsBorderVisible
        {
            get => _model.IsBorderVisible;
            set
            {
                SetProperty(_model, () => _model.IsBorderVisible, value);

                var borderViewModel = Parent.IsStarted ? Parent.BorderViewModel : (IDialogViewModel)Parent.DesignBorderViewModel;

                DialogService.Handle(_model.IsBorderVisible, borderViewModel);
            }
        }


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

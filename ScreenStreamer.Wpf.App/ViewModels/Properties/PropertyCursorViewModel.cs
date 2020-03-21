using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyCursorViewModel : PropertyBaseViewModel
    {
        private readonly PropertyCursorModel _model;
        public override string Name => "Show Cursor";

        #region IsCursorVisible
        private bool _isCursorVisible = true;
        public bool IsCursorVisible { get => _model.IsCursorVisible; set { SetProperty(_model,() => _model.IsCursorVisible, value); } }
        #endregion IsCursorVisible

        public PropertyCursorViewModel(StreamViewModel parent, PropertyCursorModel model) : base(parent)
        {
            _model = model;
        }

        //public override object Clone()
        //{
        //    return new PropertyCursorViewModel(null, )
        //    {
        //        Info = this.Info,
        //        IsCursorVisible = this.IsCursorVisible
        //    };
        //}

        protected override IDialogViewModel BuildDialogViewModel()
        {
            return new CursorSettingsViewModel(this);
        }
    }
}

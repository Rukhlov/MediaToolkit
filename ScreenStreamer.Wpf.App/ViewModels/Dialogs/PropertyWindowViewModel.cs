using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Properties;
using System;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public abstract class PropertyWindowViewModel : BaseWindowViewModel
    {
        protected StreamerViewModelBase parent;
        [Track]
        public PropertyBaseViewModel Property { get; }

        public PropertyWindowViewModel(PropertyBaseViewModel property, StreamerViewModelBase parent) : base(parent)
        {
            this.parent = parent;
            this.Property = property ?? throw new ArgumentNullException(nameof(property));
            this.Property.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IsChanged))
                {
                    RaisePropertyChanged(() => IsChanged);
                }
            };
        }



        protected override bool CheckChanges()
        {
            return base.CheckChanges() || this.Property.IsChanged;
        }

        public override void ResetChanges()
        {
            this.parent.ResetChanges();
        }
    }
}

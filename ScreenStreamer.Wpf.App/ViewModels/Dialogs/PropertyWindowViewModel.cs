using ScreenStreamer.Wpf.Interfaces;
using ScreenStreamer.Wpf.ViewModels.Common;
using ScreenStreamer.Wpf.ViewModels.Properties;
using System;

namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{
    public abstract class PropertyWindowViewModel : BaseWindowViewModel
    {
        protected TrackableViewModel parent;

        [Track]
        public PropertyBaseViewModel Property { get; }

        public PropertyWindowViewModel(PropertyBaseViewModel property, TrackableViewModel parent) : base(parent)
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

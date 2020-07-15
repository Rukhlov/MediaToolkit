using System;
using System.Drawing;

using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;
//using Polywall.Share.Exceptions;
using Prism.Commands;
using MediaToolkit.UI;
using ScreenStreamer.Wpf;
using System.Windows;
using System.Windows.Forms;
using ScreenStreamer.Wpf.App.Utils;
using static ScreenStreamer.Wpf.Common.Helpers.ScreenHelper;

namespace ScreenStreamer.Wpf.Common.Models.Properties
{
    public class PropertyVideoViewModel : PropertyBaseViewModel, App.Managers.IViewRect
	{
        private readonly PropertyVideoModel _model;

        public PropertyVideoViewModel(StreamViewModel parent, PropertyVideoModel model) : base(parent)
        {
            _model = model;
            ShowBorderCommand = new DelegateCommand(ShowBorder);

            selectAreaManager = new App.Managers.SelectAreaManager(this);

            //UpdateRegion();
        }

        private App.Managers.SelectAreaManager selectAreaManager = null;

        public override string Name => "Video";

        public ICommand ShowBorderCommand { get; }
        public ICommand AdjustCommand { get; }

        [Track]
        public bool IsRegion
        {
            get => _model.IsRegion;
            set
            {
                SetProperty(_model, () => _model.IsRegion, value);

                UpdateRegion();
                RaisePropertyChanged(nameof(Info));
            }
        }

        private VideoSourceItem display = null;

        ////[Track]
        //public VideoSourceItem Display
        //{
        //    get => display;
        //    set
        //    {

        //        //SetProperty(_model, () => _model.Display, value);
        //        display = value;
        //        if (_model != null)
        //        {
        //            _model.DeviceId = display.DeviceId;
        //            _model.DeviceName = display.Name;
        //            _model.IsUvcDevice = display.IsUvcDevice;

        //        }

        //        UpdateRegion();

        //        RaisePropertyChanged(nameof(Display));
        //        RaisePropertyChanged(nameof(Info));
        //        RaisePropertyChanged(nameof(IsScreenSource));
        //    }
        //}

        //[Track]
        public VideoSourceItem Display
        {
            get => _model.Display;
            set
            {

                SetProperty(_model, () => _model.Display, value);


                UpdateRegion();

                RaisePropertyChanged(nameof(Info));
                RaisePropertyChanged(nameof(IsScreenSource));
            }
        }

        public ScreenCaptureItem CaptureType
        {
            get => _model.CaptureType;
            set
            {

                SetProperty(_model, () => _model.CaptureType, value);

            }
        }


        public bool IsScreenSource
        {
            get
            {
                bool isScreenSource = true;
                var sourceItem = _model.Display;
                if (sourceItem != null)
                {
                    isScreenSource = !sourceItem.IsUvcDevice;
                }

                return isScreenSource;
            }

        }

        //[Track]
        public int ResolutionWidth
        {
            get => _model.ResolutionWidth;
            set
            {
                SetProperty(_model, () => _model.ResolutionWidth, value);
            }
        }


        //[Track]
        public int ResolutionHeight
        {
            get => _model.ResolutionHeight;
            set
            {
                SetProperty(_model, () => _model.ResolutionHeight, value);
            }
        }

        //[Track]
        public string Resolution
        {
            get => (_model.ResolutionWidth + "x" + _model.ResolutionHeight);

        }

       // [Track]
        public int Top
        {
            get => _model.Top;
            set
            {
                SetProperty(_model, () => _model.Top, value);
            }
        }

        //[Track]
        public int Left
        {
            get => _model.Left;
            set
            {
                SetProperty(_model, () => _model.Left, value);
            }
        }

        public Rectangle CaptureRect
        {
            get
            { 
                var p = new System.Drawing.Point(Left, Top);
                var s = new System.Drawing.Size(ResolutionWidth, ResolutionHeight);

                return new Rectangle(p, s);
            }
        }


        //[Track]
        public bool CaptureMouse
        {
            get => _model.CaptureMouse;
            set
            {
                SetProperty(_model, () => _model.CaptureMouse, value);
            }
        }

        public bool ShowCaptureBorder
        {
            get => _model.ShowCaptureBorder;
            set
            {
                SetProperty(_model, () => _model.ShowCaptureBorder, value);
            }
        }



		public void OnStreamStateChanged(bool isStarted)
		{
			//...
			selectAreaManager.SetBorder(isStarted);

		}

		//public SelectAreaForm selectAreaForm = null;
		public void UpdateRegion()
        {
            //if(!IsRegion)
            //{
            //    var region =  Display?.CaptureRegion ?? Rectangle.Empty;

            //    this.Top = region.Top;
            //    this.Left = region.Left;
            //    this.ResolutionHeight = region.Height;
            //    this.ResolutionWidth = region.Width;

            //}


            //var sourceItem = Display;
            var sourceItem = _model.Display;
            if (sourceItem == null)
            {
                return;
            }

            bool isRegionItem = sourceItem.DeviceId == "ScreenRegion";
            if (!isRegionItem)
            {// full screen
                var captureRegion = Display?.CaptureRegion ?? Rectangle.Empty;

                this.Top = captureRegion.Top;
                this.Left = captureRegion.Left;
                this.ResolutionHeight = captureRegion.Height;
                this.ResolutionWidth = captureRegion.Width;

				//selectAreaManager?.HideBorder();
                SetBorderVisible(false);

            }
            else
            { // custom region mode...

                //selectAreaManager?.ShowBorder(CaptureRect);

                var designViewModel = Parent.DesignViewModel;
                if (designViewModel != null)
                {
                    SetBorderVisible(true);

                    var region = designViewModel.GetScreenRegion();

                    this.Left = region.Left;
                    this.Top = region.Top;
                    this.ResolutionWidth = region.Width;
                    this.ResolutionHeight = region.Height;
                }


            }

        }

		private void SelectedAreaChanged(Rectangle region)
		{
			this.Top = region.Top;
			this.Left = region.Left;
			this.ResolutionHeight = region.Height;
			this.ResolutionWidth = region.Width;
		}

        private void SetBorderVisible(bool isVisible)
        {
            Parent.IsBorderVisible = isVisible;
            DialogService.Handle(isVisible, Parent.DesignViewModel);


            //var model = Parent.Properties.OfType<PropertyBorderViewModel>().FirstOrDefault();
            //if (model != null)
            //{
            //    model.IsBorderVisible = isVisible;

            //}

            //Parent.Properties.OfType<PropertyBorderViewModel>().FirstOrDefault()
            //    .Do(model => model.IsBorderVisible = true);
        }


        private void ShowBorder()
        {
            var model = Parent.Properties.OfType<PropertyBorderViewModel>().FirstOrDefault();
            if (model != null)
            {
                model.IsBorderVisible = true;
                
            }

            //Parent.Properties.OfType<PropertyBorderViewModel>().FirstOrDefault()
            //    .Do(model => model.IsBorderVisible = true);
        }

        

        protected override IDialogViewModel BuildDialogViewModel()
        {
            return new VideoSettingsViewModel(this,Parent);
        }

        public override string Info
        {
            get
            {
                var builder = new StringBuilder();
                if (IsRegion)
                {
                    builder.Append($"{nameof(this.Top)}: {this.Top}, {nameof(this.Left)}: {this.Left}, Width: {this.ResolutionWidth}, Height: {this.ResolutionHeight}");
                }

                else
                {

                    builder.Append(this.Display?.Name);
                }
 

                if (builder.Length > MaxInfoLength)
                {
                    return builder.ToString(0, MaxInfoLength - 3) + "...";
                }
                return builder.ToString();
            }
        }


    }

    //private (double Top, double Left, double Width, double Height) GetRegion()
    //{

    //    if (IsRegion)
    //    {
    //        return (this.Top, this.Left, this.ResolutionWidth, this.ResolutionHeight);
    //    }
    //    else
    //    {
    //        var rect = ScreenHelper.GetScreenBounds(Display) ?? new Rectangle();
    //        return (rect.Top, rect.Left, rect.Width, rect.Height);
    //    }
    //}


    //[Track]
    //public string Display
    //{
    //    get => _model.Display?.Name ?? "";
    //    set
    //    {
    //        SetProperty(_model, () => _model.Display, value);

    //        UpdateRegion();
    //        RaisePropertyChanged(nameof(Info));
    //    }
    //}




}

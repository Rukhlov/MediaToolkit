using System;
using System.Drawing;

using ScreenStreamer.Wpf.Common.Helpers;
using ScreenStreamer.Wpf.Common.Interfaces;
using ScreenStreamer.Wpf.Common.Models.Dialogs;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Input;

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

            //selectAreaManager = new App.Managers.SelectAreaManager(this);

        }

        //private App.Managers.SelectAreaManager selectAreaManager = null;

        public override string Name => "Video";

        public ICommand ShowBorderCommand { get; }
        public ICommand AdjustCommand { get; }

        public bool IsRegion => _model.IsRegion;
        public bool IsScreenSource => !_model.IsUvcDevice;

        private VideoSourceItem videoSource = null;

        //[Track]
        public VideoSourceItem Display
        {
            get => videoSource;
            set
            {

                //SetProperty(_model, () => _model.Display, value);
                videoSource = value;
                if (_model != null && videoSource !=null)
                {
                    _model.DeviceId = videoSource.DeviceId;
                    _model.DeviceName = videoSource.Name;
                    _model.IsUvcDevice = videoSource.IsUvcDevice;

                    SetupDisplayRegion();

                    RaisePropertyChanged(nameof(Display));
                    RaisePropertyChanged(nameof(Info));
                    RaisePropertyChanged(nameof(IsScreenSource));

                }

                //SetupDisplayRegion();

                //RaisePropertyChanged(nameof(Display));
                //RaisePropertyChanged(nameof(Info));
                //RaisePropertyChanged(nameof(IsScreenSource));
            }
        }


        private ScreenCaptureItem captureType = null;
        public ScreenCaptureItem CaptureType
        {
            get => captureType;
            set
            {
                captureType = value;
                if (_model != null)
                {
                    _model.CaptType = captureType.CaptType;

                    RaisePropertyChanged(nameof(CaptureType));
                }
            }
        }



        //[Track]
        public int Left
        {
            get => _model.Left;
            set
            {
                SetProperty(_model, () => _model.Left, value);
                RaisePropertyChanged(nameof(Info));
            }
        }

        // [Track]
        public int Top
        {
            get => _model.Top;
            set
            {
                SetProperty(_model, () => _model.Top, value);
                RaisePropertyChanged(nameof(Info));
            }
        }



        //[Track]
        public int ResolutionWidth
        {
            get => _model.ResolutionWidth;
            set
            {
                SetProperty(_model, () => _model.ResolutionWidth, value);
                RaisePropertyChanged(nameof(Info));
            }
        }


        //[Track]
        public int ResolutionHeight
        {
            get => _model.ResolutionHeight;
            set
            {
                SetProperty(_model, () => _model.ResolutionHeight, value);
                RaisePropertyChanged(nameof(Info));
            }
        }



        //[Track]
        public string Resolution
        {
            get => (_model.ResolutionWidth + "x" + _model.ResolutionHeight);
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



        public void SetupDisplayRegion()
        {
            if (Display == null)
            {
                return;
            }
            var captureRegion = Display.CaptureRegion;
            if (IsRegion)
            {// если выбран регион, то получаем границы из DesignBorderViewModel

                var designViewModel = Parent.DesignBorderViewModel;
                //if (designViewModel != null)
                {
                    captureRegion = designViewModel.GetScreenRegion();
                }

            }

            this.Top = captureRegion.Top;
            this.Left = captureRegion.Left;
            this.ResolutionHeight = captureRegion.Height;
            this.ResolutionWidth = captureRegion.Width;

            //selectAreaManager?.HideBorder();
            SetBorderVisible(IsRegion);


        }



        private void SetBorderVisible(bool isVisible)
        {
            Parent.IsBorderVisible = isVisible;

            //if (Parent.DesignBorderViewModel != null)
            {
                DialogService.Handle(isVisible, Parent.DesignBorderViewModel);


            }

            //var model = Parent.Properties.OfType<PropertyBorderViewModel>().FirstOrDefault();
            //if (model != null)
            //{
            //    model.IsBorderVisible = isVisible;
            //}
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
                    var displayName = Display?.Name ?? "Device is not found!";
                    builder.Append(displayName);

                    //builder.Append(this.Display?.Name);
                }
 

                if (builder.Length > MaxInfoLength)
                {
                    return builder.ToString(0, MaxInfoLength - 3) + "...";
                }
                return builder.ToString();
            }
        }


        public void OnStreamStateChanged(bool isStarted)
        {
            //...
            //selectAreaManager.SetBorder(isStarted);

        }

        //public SelectAreaForm selectAreaForm = null;

    }

    //private void SelectedAreaChanged(Rectangle region)
    //{
    //	this.Top = region.Top;
    //	this.Left = region.Left;
    //	this.ResolutionHeight = region.Height;
    //	this.ResolutionWidth = region.Width;
    //}
    //public ScreenCaptureItem CaptureType
    //{
    //    get => _model.CaptureType;
    //    set
    //    {

    //        SetProperty(_model, () => _model.CaptureType, value);

    //    }
    //}

    ////[Track]
    //public VideoSourceItem Display
    //{
    //    get => _model.Display;
    //    set
    //    {

    //        SetProperty(_model, () => _model.Display, value);


    //        UpdateRegion();

    //        RaisePropertyChanged(nameof(Info));
    //        RaisePropertyChanged(nameof(IsScreenSource));
    //    }
    //}

    //[Track]
    //public bool IsRegion
    //{
    //    get => _model.IsRegion;
    //    set
    //    {
    //        SetProperty(_model, () => _model.IsRegion, value);

    //        SetupDisplayRegion();
    //        RaisePropertyChanged(nameof(Info));
    //    }
    //}

    //public bool IsScreenSource
    //{
    //    get
    //    {
    //        bool isScreenSource = true;
    //        var sourceItem = _model.Display;
    //        if (sourceItem != null)
    //        {
    //            isScreenSource = !sourceItem.IsUvcDevice;
    //        }

    //        return isScreenSource;
    //    }

    //}

    /*
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
    */

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

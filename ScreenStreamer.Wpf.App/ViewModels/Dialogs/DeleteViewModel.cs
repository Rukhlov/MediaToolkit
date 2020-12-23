using Prism.Commands;
using ScreenStreamer.Wpf.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;


namespace ScreenStreamer.Wpf.ViewModels.Dialogs
{

    public class DeleteViewModel : WindowViewModel
    {
		//public override string Caption => "Delete";
		//private string _dialogTextFormat = "Are you sure want to delete '{0}'?";

		//Delete
		public override string Caption => LocalizationManager.GetString("DeleteStreamDialogCaption");
		//"Are you sure want to delete '{0}'?";
		private string _dialogTextFormat = LocalizationManager.GetString("DeleteStreamDialogMessageMask");


		public string DialogText { get; set; }

        public override double MinWidth => 300d;

        public override bool IsBottomVisible => false;

        public ICommand DeleteCommand { get; set; }

        private StreamViewModel _stream;


        public DeleteViewModel(StreamViewModel stream) : base(stream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            DeleteCommand = new DelegateCommand<Window>(SetDialogResult);
            DialogText = string.Format(_dialogTextFormat, _stream.Name);

        }

        private void SetDialogResult(Window selfWindow)
        {
            selfWindow.DialogResult = true;
            selfWindow.Close();
        }
    }

}

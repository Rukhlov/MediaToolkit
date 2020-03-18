using Prism.Commands;
using System;
using System.Windows;
using System.Windows.Input;

namespace ScreenStreamer.Wpf.Common.Models.Dialogs
{
    public class DeleteViewModel : BaseWindowViewModel
    {
        public override string Caption => "Delete";

        private string _dialogTextFormat = "Are you sure want to delete '{0}'?";
        public string DialogText { get; set; }

        public override double MinWidth => 300d;

        public override bool IsBottomVisible => false;

        public ICommand DeleteCommand { get; set; }

        private StreamViewModel _stream;

        public DeleteViewModel(StreamViewModel stream)
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

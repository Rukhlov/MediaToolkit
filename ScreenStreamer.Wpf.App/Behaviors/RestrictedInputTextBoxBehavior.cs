using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ScreenStreamer.Wpf.Common.Behaviors
{
    public class RestrictedInputTextBoxBehavior : Behavior<TextBox>
    {
        protected Regex DisallowedRegex = new Regex("[^0-9]+"); //regex that matches disallowed text

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += AssociatedObjectPreviewTextInput;
            DataObject.AddPastingHandler(AssociatedObject, TextBoxPasting);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= AssociatedObjectPreviewTextInput;
            DataObject.RemovePastingHandler(AssociatedObject, TextBoxPasting);
            base.OnDetaching();
        }

        private void AssociatedObjectPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        protected virtual bool IsTextAllowed(string text)
        {
            return !DisallowedRegex.IsMatch(text);
        }

        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }


    public class IpRestrictedInputTextBoxBehavior : RestrictedInputTextBoxBehavior
    {
        public IpRestrictedInputTextBoxBehavior()
        {
            DisallowedRegex = new Regex("[^0-9.]+");
        }
    }

    public class ResolutionRestrictedInputTextBoxBehavior : RestrictedInputTextBoxBehavior
    {
        public ResolutionRestrictedInputTextBoxBehavior()
        {
            DisallowedRegex = new Regex("[^0-9x]+");
        }
    }
}

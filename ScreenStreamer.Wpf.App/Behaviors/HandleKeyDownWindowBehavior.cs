using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ScreenStreamer.Wpf.Common.Behaviors
{
	public class HandleKeyDownWindowBehavior : Behavior<Window>
    {
		protected override void OnAttached()
		{
			base.OnAttached();
			AssociatedObject.KeyDown += AssociatedObject_KeyDown;


		}

		private void AssociatedObject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.System && e.SystemKey == Key.F4)
			{
				e.Handled = true;
			}
		}

		protected override void OnDetaching()
		{
			AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
			base.OnDetaching();
		}
	}
}

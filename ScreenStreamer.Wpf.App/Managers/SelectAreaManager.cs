using MediaToolkit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenStreamer.Wpf.App.Managers
{
	public interface IViewRect
	{
		int Left { get; set; }
		int Top { get; set; }
		int ResolutionWidth { get; set; }
		int ResolutionHeight { get; set; }

	}

	class SelectAreaManager
	{
		private IViewRect viewRect;

		private SelectAreaForm selectAreaForm = null;

		public SelectAreaManager(IViewRect rect)
		{
			Init(rect);
		}

		public void Init(IViewRect rect)
		{
			if (selectAreaForm == null)
			{
				selectAreaForm = new SelectAreaForm
				{
					StartPosition = System.Windows.Forms.FormStartPosition.Manual,
				};

				selectAreaForm.AreaChanged += SelectAreaForm_AreaChanged;
			}

			this.viewRect = rect;
		}

		public void ShowBorder(Rectangle rect)
		{
			if (selectAreaForm != null)
			{
				if (!rect.IsEmpty)
				{
					selectAreaForm.Location = rect.Location;
					selectAreaForm.Size = rect.Size;
				}

				selectAreaForm.Visible = true;
			}
		}

		public void LockBorder(bool locked)
		{
			if (selectAreaForm != null)
			{
				selectAreaForm.Locked = locked;
			}
		}

		public void SetBorder(bool Capturing)
		{
			if (selectAreaForm != null)
			{
				selectAreaForm.Capturing = Capturing;
			}

		}

		public void HideBorder()
		{
			if (selectAreaForm != null)
			{
				selectAreaForm.Visible = false;
			}

		}

		private void SelectAreaForm_AreaChanged(Rectangle rect)
		{
			if (viewRect != null)
			{
				viewRect.Left = rect.Left;
				viewRect.Top = rect.Top;
				viewRect.ResolutionWidth = rect.Width;
				viewRect.ResolutionHeight = rect.Height;

			}

		}

		public void Dispose()
		{
			if (selectAreaForm != null)
			{
				selectAreaForm.AreaChanged -= SelectAreaForm_AreaChanged;
				selectAreaForm.Close();
				selectAreaForm = null;
			}

			viewRect = null;
		}

	}
}

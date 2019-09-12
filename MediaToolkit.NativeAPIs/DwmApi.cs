using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs
{
    public sealed class DwmApi
    {
        public static void DisableAero(bool disable)
        {
            var compositionAction = disable ? CompositionAction.DWM_EC_DISABLECOMPOSITION : CompositionAction.DWM_EC_ENABLECOMPOSITION;

            DwmEnableComposition(compositionAction);

        }

        [DllImport("dwmapi.dll", PreserveSig = false)]
        internal static extern void DwmEnableComposition(CompositionAction uCompositionAction);

    }
}

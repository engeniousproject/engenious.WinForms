using OpenTK.Graphics;
using OpenTK.Platform;

namespace engenious.WinForms
{
    internal static class CocoaHelper
    {
        public static IWindowInfo CreateWindowInfo(GraphicsMode mode, GameControl control)
        {
            return Utilities.CreateMacOSCarbonWindowInfo(control.Handle, false, true);
        }
    }
}
using OpenTK.Graphics;
using OpenTK.Platform;

namespace engenious.WinForms
{
    internal static class WinHelper
    {
        public static IWindowInfo CreateWindowInfo(GraphicsMode mode, GameControl control)
        {
            return Utilities.CreateWindowsWindowInfo(control.Handle);
        }
    }
}
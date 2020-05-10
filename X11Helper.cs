using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Platform.X11;

namespace engenious.WinForms
{
    internal static class X11Helper
    {
        [DllImport("libX11")]
        static extern IntPtr XCreateColormap(IntPtr display, IntPtr window, IntPtr visual, int alloc);

        [DllImport("libX11", EntryPoint = "XGetVisualInfo")]
        static extern IntPtr XGetVisualInfoInternal(IntPtr display, IntPtr visualInfoMask, ref XVisualInfo template, out int numberOfItems);
        [DllImport ("libX11", EntryPoint="XDefaultVisual")]
        static extern IntPtr XDefaultVisual(IntPtr display, int screenNumber);
        static IntPtr XGetVisualInfo(IntPtr display, int visualInfoMask, ref XVisualInfo template, out int numberOfItems)
        {
            return XGetVisualInfoInternal(display, (IntPtr)visualInfoMask, ref template, out numberOfItems);
        }
        public static IWindowInfo CreateWindowInfo(GraphicsMode mode, GameControl control)
        {
            // Use reflection to retrieve the necessary values from Mono's Windows.Forms implementation.
            Type xplatui = Type.GetType("System.Windows.Forms.XplatUIX11, System.Windows.Forms");
            if (xplatui == null) throw new PlatformNotSupportedException(
                "System.Windows.Forms.XplatUIX11 missing. Unsupported platform or Mono runtime version, aborting.");

            // get the required handles from the X11 API.
            var display = (IntPtr)GetStaticFieldValue(xplatui, "DisplayHandle");
            IntPtr rootWindow = (IntPtr)GetStaticFieldValue(xplatui, "RootWindow");
            int screen = (int)GetStaticFieldValue(xplatui, "ScreenNo");

            // get the XVisualInfo for this GraphicsMode
            XVisualInfo info = new XVisualInfo();
            info.VisualID = mode.Index ?? IntPtr.Zero;
            IntPtr infoPtr = XGetVisualInfo(display, 1 /* VisualInfoMask.ID */, ref info, out _);
            if (infoPtr == IntPtr.Zero)
                infoPtr = (IntPtr) GetStaticFieldValue(xplatui, "CustomVisual");
            if (infoPtr == IntPtr.Zero)
                infoPtr = XDefaultVisual(display, screen);
            info = (XVisualInfo)Marshal.PtrToStructure(infoPtr, typeof(XVisualInfo));

            

            // set the X11 colormap.
            SetStaticFieldValue(xplatui, "CustomVisual", info.Visual);
            SetStaticFieldValue(xplatui, "CustomColormap", XCreateColormap(display, rootWindow, info.Visual, 0));

            return Utilities.CreateX11WindowInfo(display, screen, control.Handle, rootWindow, infoPtr);
        }
        static object GetStaticFieldValue(Type type, string fieldName)
        {
            return type.GetField(fieldName,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(null);
        }
        
        static void SetStaticFieldValue(Type type, string fieldName, object value)
        {
            type.GetField(fieldName,
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(null, value);
        }
    }
}
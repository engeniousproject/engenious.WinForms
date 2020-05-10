using System;
using System.Runtime.InteropServices;
using engenious.Graphics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Platform.X11;

namespace engenious.WinForms
{
    /// <inheritdoc />
    public class WinFormsGame : Game<GameControl>
    {
        private bool _initialized = false;
        private void InitializeOnce(GameControl control)
        {
            if (_initialized)
                return;
            lock (this)
            {
                if (_initialized)
                    return;
                _initialized = true;
                control.WindowInfo = CreateWindow(control);
                InitializeControl(control);
            }
        }


        private IWindowInfo CreateWindow(GameControl control)
        {
            GraphicsContext.ShareContexts = true;

            IWindowInfo baseWindow;
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.Win32NT:
                case PlatformID.WinCE:
                    baseWindow = WinHelper.CreateWindowInfo(GraphicsMode.Default, control);
                    break;
                case PlatformID.MacOSX:
                    baseWindow = CocoaHelper.CreateWindowInfo(GraphicsMode.Default, control);
                    break;
                case PlatformID.Unix:
                    baseWindow = X11Helper.CreateWindowInfo(GraphicsMode.Default, control);
                    break;
                default:
                    throw new NotSupportedException(Environment.OSVersion.Platform.ToString());
            }
            var options = new ToolkitOptions();
            options.Backend = PlatformBackend.PreferNative;
            OpenTK.Toolkit.Init(options);
            var context = new GraphicsContext(GraphicsMode.Default, baseWindow, 0, 0, ContextFlags);
            
            ConstructContext(baseWindow, context);

            CreateSharedContext(baseWindow);
            
            _context.MakeCurrent(baseWindow);
            _context.LoadAll();

            return baseWindow;
        }

        /// <inheritdoc />
        public WinFormsGame(GameControl control)
        {
            control.HandleCreated += ControlOnHandleCreated;
            if (control.IsHandleCreated)
                InitializeOnce(control);
        }

        private void ControlOnHandleCreated(object sender, EventArgs e)
        {
            InitializeOnce((GameControl)sender);
            ((GameControl)sender).HandleCreated -= ControlOnHandleCreated;
        }
    }
}
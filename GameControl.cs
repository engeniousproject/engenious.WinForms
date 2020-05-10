using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Input;
using OpenTK.Platform;
using KeyPressEventArgs = OpenTK.KeyPressEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace engenious.WinForms
{
    public class GameControl : UserControl, IRenderingSurface
    {
        private readonly Thread _updateThread;
        /// <inheritdoc />
        public GameControl()
        {
            CursorVisible = true;
            
            SetStyle(ControlStyles.UserPaint, true);
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var elapsed = _stopwatch.ElapsedMilliseconds / 1000.0;
            if (elapsed <= 0.0)
                return;

            _stopwatch.Restart();
            // TODO: better render handling
            _updateFrame?.Invoke(this, new FrameEventArgs(elapsed));
            _renderFrame?.Invoke(this, new FrameEventArgs(elapsed));
        }

        private Stopwatch _stopwatch = new Stopwatch();

        /// <inheritdoc />
        public Point PointToScreen(Point pt) => base.PointToScreen(new System.Drawing.Point(pt.X, pt.Y));

        /// <inheritdoc />
        public Point PointToClient(Point pt) => base.PointToClient(new System.Drawing.Point(pt.X, pt.Y));

        /// <inheritdoc />
        public new Rectangle ClientRectangle
        {
            get => new Rectangle(base.ClientRectangle.X,base.ClientRectangle.Y,base.ClientRectangle.Width,base.ClientRectangle.Height);
            set
            {
                var diffX = Location.X - base.ClientRectangle.X;
                var diffY = Location.Y - base.ClientRectangle.Y;
                Location = new System.Drawing.Point(value.X + diffX, value.Y + diffY);
                ClientSize = new System.Drawing.Size(value.Width, value.Height);
            }
        }

        /// <inheritdoc />
        public new Size ClientSize
        {
            get => new Size(base.ClientSize.Width, base.ClientSize.Height);
            set
            {
                base.ClientSize = new System.Drawing.Size(value.Width,value.Height);
            }
        }

        private bool _cursorVisible;
        /// <inheritdoc />
        public bool CursorVisible
        {
            get => _cursorVisible;
            set
            {
                _cursorVisible = value;
                if (value)
                    Cursor.Show();
                else
                    Cursor.Hide();
            }
        }

        public IWindowInfo WindowInfo { get; internal set; }


        private event EventHandler<FrameEventArgs> _renderFrame;
        event EventHandler<FrameEventArgs> IControlInternals.RenderFrame
        {
            add => _renderFrame += value;
            remove => _renderFrame -= value;
        }

        private event EventHandler<FrameEventArgs> _updateFrame;
        event EventHandler<FrameEventArgs> IControlInternals.UpdateFrame
        {
            add => _updateFrame += value;
            remove => _updateFrame -= value;
        }

        private double _lastElapsed = Environment.TickCount;

        /// <inheritdoc />
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {


        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
        }

        private Control GetParent(Control current)
        {
            while (true)
            {
                if (current == null) return null;
                if (current is Form) return current;

                current = current.Parent;
            }
        }

        private Form ParentWindow
        {
            get => (Form)GetParent(this);
        }
        
        private event EventHandler<CancelEventArgs> _closing;
        event EventHandler<CancelEventArgs> IControlInternals.Closing
        {
            add => _closing += value;
            remove => _closing -= value;
        }
        private void ParentWindowOnClosing(object sender, CancelEventArgs e)
        {
            _closing?.Invoke(this, e);
        }
        
        private event EventHandler<EventArgs> _focusChanged;
        event EventHandler<EventArgs> IControlInternals.FocusedChanged
        {
            add => _focusChanged += value;
            remove => _focusChanged -= value;
        }

        /// <inheritdoc />
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _focusChanged?.Invoke(this, e);
        }

        /// <inheritdoc />
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            _focusChanged?.Invoke(this, e);
        }

        private event EventHandler<EventArgs> _resize;
        event EventHandler<EventArgs> IControlInternals.Resize
        {
            add => _resize += value;
            remove => _resize -= value;
        }

        /// <inheritdoc />
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _resize?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        private event EventHandler<EventArgs> _load;
        event EventHandler<EventArgs> IControlInternals.Load
        {
            add => _load += value;
            remove => _load -= value;
        }

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            
            
            if (ParentWindow != null)
                ParentWindow.Closing += ParentWindowOnClosing;
            else
                HandleDestroyed += (sender, args) => ParentWindowOnClosing(this, new CancelEventArgs(false));
            _load?.Invoke(this, EventArgs.Empty);

            _stopwatch.Start();
        }

        private event EventHandler<KeyPressEventArgs> _keyPress;
        event EventHandler<KeyPressEventArgs> IControlInternals.KeyPress
        {
            add => _keyPress += value;
            remove => _keyPress-= value;
        }

        /// <inheritdoc />
        protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            _keyPress?.Invoke(this, new KeyPressEventArgs(e.KeyChar));
        }

        private event EventHandler<MouseWheelEventArgs> _mouseWheel;
        event EventHandler<MouseWheelEventArgs> IControlInternals.MouseWheel
        {
            add => _mouseWheel += value;
            remove => _mouseWheel -= value;
        }

        private int _wheelPos;

        /// <inheritdoc />
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            _wheelPos += e.Delta;
            _mouseWheel?.Invoke(this, new MouseWheelEventArgs(e.X, e.Y, _wheelPos, e.Delta));
        }
    }
}
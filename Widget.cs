using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WGP;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace WGP.Gui
{
    /// <summary>
    /// Base widget class.
    /// </summary>
    public abstract partial class Widget
    {

        public event EventHandler<MouseButtonEventArgs> MouseClick;
        public event EventHandler MouseEntered;
        public event EventHandler MouseLeaved;
        public event EventHandler<MouseButtonEventArgs> MouseDown;
        public event EventHandler<MouseButtonEventArgs> MouseUp;
        public event EventHandler<MouseMovedEventArgs> MouseMoved;
        public event EventHandler<PatternChangedEventArgs> PatternChanged;
        public event EventHandler<PaddingChangedEventArgs> PaddingChanged;

        protected internal FloatRect pattern;
        private Vector2f padding;
        internal virtual Window Parent { get; set; }

        private RectangleShape BackToolTip { get; set; }
        private Vertex[] BorderToolTip { get; set; }
        private Text TextToolTip { get; set; }
        private Clock TimedToolTip { get; set; }
        private bool ShowToolTip { get; set; }
        private bool MouseOnReserved { get; set; }
        /// <summary>
        /// The tooltip is the message that appears when the mouse is on the same widget for a few seconds.
        /// </summary>
        /// <value>String to display.</value>
        public string ToolTip
        {
            get => TextToolTip.DisplayedString;
            set => TextToolTip.DisplayedString = value;
        }

        /// <summary>
        /// The pattern is how the widget will be placed and sized within its reserved space. All values must be between [0,1].
        /// Left and Top are the position of the widget (0 for left/top, 1 for right/bottom).
        /// Width and Height are the size of the widget (0 for smallest, 1 for largest).
        /// </summary>
        /// <value>Pattern.</value>
        public FloatRect Pattern
        {
            get => pattern;
            set
            {
                FloatRect old = pattern;
                pattern = value;
                if (PatternChanged != null && value != old)
                    PatternChanged(this, new PatternChangedEventArgs(old, value));
            }
        }
        /// <summary>
        /// The padding is the blank space around the widget to not be sticked to other widgets.
        /// </summary>
        /// <value>Padding.</value>
        public Vector2f Padding
        {
            get => padding;
            set
            {
                Vector2f old = padding;
                padding = value;
                if (PaddingChanged != null && value != old)
                    PaddingChanged(this, new PaddingChangedEventArgs(old, value));
            }
        }
        protected bool Pressed { get; set; }
        protected bool MouseOnWidget { get; set; }

        protected FloatRect ReservedSpace { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Widget()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            Padding = new Vector2f(3, 3);
            Pattern = new FloatRect(0, 0, 1, 1);
            ReservedSpace = new FloatRect();
            Pressed = false;
            MouseOnWidget = false;
            BackToolTip = new RectangleShape();
            BackToolTip.FillColor = Init.LightX;
            BorderToolTip = new Vertex[5];
            for (int i = 0; i < 5; i++)
                BorderToolTip[i].Color = Init.BorderMedium;
            TextToolTip = new Text("", Init.Font, Init.TextSize);
            TextToolTip.Color = Init.TextDark;
            TimedToolTip = new Clock();
            ShowToolTip = false;
        }
        
        internal void Update(FloatRect availableSpace)
        {
            FloatRect newReserved = new FloatRect();
            Vector2f minimum = GetMinimumSize();
            newReserved.Width = Math.Max(Utilities.Interpolation(Pattern.Width, 0, availableSpace.Width), minimum.X);
            newReserved.Height = Math.Max(Utilities.Interpolation(Pattern.Height, 0, availableSpace.Height), minimum.Y);
            newReserved.Left = availableSpace.Left + Utilities.Interpolation(Pattern.Left, 0, availableSpace.Width - newReserved.Width);
            newReserved.Top = availableSpace.Top + Utilities.Interpolation(Pattern.Top, 0, availableSpace.Height - newReserved.Height);

            ReservedSpace = newReserved;

            if (MouseOnReserved)
            {
                if (TimedToolTip.ElapsedTime > Time.FromSeconds(1.5f) && ToolTip.Count() > 0)
                    ShowToolTip = true;
                TextToolTip.Position = BackToolTip.Position + new Vector2f(5, 0);
                BackToolTip.Size = new Vector2f(TextToolTip.FindCharacterPos((uint)TextToolTip.DisplayedString.Count()).X + 10, Init.TextSize + 5);
                BorderToolTip[0].Position = BackToolTip.Position + new Vector2f(.5f, .5f);
                BorderToolTip[1].Position = BorderToolTip[0].Position + new Vector2f(BackToolTip.Size.X, 0);
                BorderToolTip[2].Position = BorderToolTip[1].Position + new Vector2f(0, BackToolTip.Size.Y);
                BorderToolTip[3].Position = BorderToolTip[2].Position + new Vector2f(-BackToolTip.Size.X, 0);
                BorderToolTip[4].Position = BorderToolTip[0].Position;
            }
            else
                ShowToolTip = false;

            InternUpdate();
        }

        protected virtual void InternUpdate()
        {

        }

        protected virtual FloatRect GetHitbox()
        {
            FloatRect result = ReservedSpace;
            result.Left += Padding.X;
            result.Top += Padding.Y;
            result.Width -= Padding.X * 2;
            result.Height -= Padding.Y * 2;
            return result;
        }

        internal abstract Vector2f GetMinimumSize();

        internal virtual void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept = false)
        {
            if (MouseOnWidget)
            {
                if (button == Mouse.Button.Left)
                    Pressed = true;
                if (MouseDown != null)
                    MouseDown(this, new MouseButtonEventArgs(button, pos));
            }
        }

        internal virtual void MouseButtonUpCall(Mouse.Button button, Vector2f pos, bool intercept = false)
        {
            if (MouseOnWidget)
            {
                if (button == Mouse.Button.Left && Pressed)
                {
                    Pressed = false;
                    if (MouseClick != null)
                        MouseClick(this, new MouseButtonEventArgs(button, pos));
                }
                if (MouseUp != null)
                    MouseUp(this, new MouseButtonEventArgs(button, pos));
            }
        }

        internal virtual void MouseScrolledCall(int delta)
        {

        }

        internal virtual void MouseMovedCall(Vector2f pos, bool intercept = false)
        {
            bool callMovedEvent = MouseOnWidget || GetHitbox().Contains(pos);
            if (MouseOnWidget != (GetHitbox().Contains(pos) && !intercept))
            {
                if (MouseOnWidget || intercept)
                {
                    MouseOnWidget = false;
                    if (MouseLeaved != null)
                        MouseLeaved(this, new EventArgs());
                }
                else
                {
                    MouseOnWidget = true;
                    if (MouseEntered != null)
                        MouseEntered(this, new EventArgs());
                }
            }
            if (callMovedEvent && MouseMoved != null)
                MouseMoved(this, new MouseMovedEventArgs(pos));
            if (!MouseOnWidget)
                Pressed = false;
            BackToolTip.Position = pos + new Vector2f(0, 16);
            if ((ReservedSpace.Contains(pos) && !intercept) != MouseOnReserved)
            {
                if ((ReservedSpace.Contains(pos) && !intercept))
                {
                    MouseOnReserved = true;
                    TimedToolTip.Restart();
                }
                else
                    MouseOnReserved = false;
            }
        }

        internal virtual void TextEnteredCall(string code)
        {

        }

        internal virtual void KeyPressedCall(KeyEventArgs args)
        {

        }

        internal virtual void KeyReleasedCall(KeyEventArgs args)
        {

        }

        internal abstract void Draw(RenderTarget target, Vector2f decal);

        internal virtual void DrawUpper(RenderTarget target, Vector2f decal)
        {
            if (ShowToolTip)
            {
                Transformable tr = new Transformable();
                tr.Position = decal;
                target.Draw(BackToolTip, new RenderStates(tr.Transform));
                target.Draw(BorderToolTip, PrimitiveType.LinesStrip, new RenderStates(tr.Transform));
                target.Draw(TextToolTip, new RenderStates(tr.Transform));
            }
        }
    }
}

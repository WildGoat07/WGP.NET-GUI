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
    /// Scrollable box container. The scrollable box allow you to reduce the minimum size of a widget by putting it in a scrollable view.
    /// </summary>
    public class Scrollablebox : Widget
    {
        public enum Mode
        {
            ALLOW_VERTICAL_SCROLLING = 0x1,
            ALLOW_HORIZONTAL_SCROLLING = 0x2,
            ALLOW_NO_SCROLLING = 0,
            ALLOW_ALL_SCROLLING = ALLOW_VERTICAL_SCROLLING | ALLOW_HORIZONTAL_SCROLLING
        }

        private Window parent;
        private Widget content;

        internal override Window Parent
        {
            get => parent;
            set
            {
                parent = value;
                if (Content != null)
                    Content.Parent = parent;
            }
        }
        private static float Step { get => 20; }
        /// <summary>
        /// Its content.
        /// </summary>
        public Widget Content
        {
            get => content;
            set
            {
                content = value;
                if (content != null)
                content.Parent = Parent;
            }
        }
        private Vertex[] Border { get; set; }
        private RenderTexture Buffer { get; set; }
        private RectangleShape Displayer { get; set; }
        /// <summary>
        /// Its minimum size.
        /// </summary>
        /// <remarks>
        /// The content's size can be reduced as much as possible, so the minimum size must be defined to put a limit on the minimal view possible.
        /// </remarks>
        public Vector2f MinimumSize { get; set; }
        private Vector2f Offset { get; set; }
        private RectangleShape UpArrow { get; set; }
        private RectangleShape UpBack { get; set; }
        private RectangleShape DownArrow { get; set; }
        private RectangleShape DownBack { get; set; }
        private RectangleShape LeftArrow { get; set; }
        private RectangleShape LeftBack { get; set; }
        private RectangleShape RightArrow { get; set; }
        private RectangleShape RightBack { get; set; }
        private RectangleShape VerticalControl { get; set; }
        private RectangleShape VerticalBack { get; set; }
        private RectangleShape HorizontalControl { get; set; }
        private RectangleShape HorizontalBack { get; set; }
        private bool VGrabbed { get; set; }
        private bool HGrabbed { get; set; }
        private Vector2f Relative { get; set; }
        /// <summary>
        /// The scrolling style.
        /// </summary>
        /// <value>Flags.</value>
        public Mode ScrollingStyle { get; set; }
        /// <summary>
        /// Set to true if the scrollable view shouldn't react to the mouse wheel events.
        /// </summary>
        public bool NoWheelScrolling { get; set; }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Scrollablebox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();

            Content = null;

            Border = new Vertex[5];

            NoWheelScrolling = false;

            Buffer = new RenderTexture(VideoMode.DesktopMode.Width, VideoMode.DesktopMode.Height);
            Displayer = new RectangleShape();
            Displayer.Texture = Buffer.Texture;


            for (int i = 0; i < 5; i++)
                Border[i].Color = Init.BorderMedium;

            MinimumSize = new Vector2f();
            Offset = new Vector2f(0, 0);
            ScrollingStyle = Mode.ALLOW_ALL_SCROLLING;

            UpArrow = new RectangleShape(new Vector2f(20, 20));
            UpBack = new RectangleShape(new Vector2f(20, 20));
            DownArrow = new RectangleShape(new Vector2f(20, 20));
            DownBack = new RectangleShape(new Vector2f(20, 20));
            LeftArrow = new RectangleShape(new Vector2f(20, 20));
            LeftBack = new RectangleShape(new Vector2f(20, 20));
            RightArrow = new RectangleShape(new Vector2f(20, 20));
            RightBack = new RectangleShape(new Vector2f(20, 20));
            VerticalBack = new RectangleShape();
            VerticalControl = new RectangleShape();
            HorizontalBack = new RectangleShape();
            HorizontalControl = new RectangleShape();

            UpArrow.Texture = Init.ScrollUpTexture;
            DownArrow.Texture = Init.ScrollDownTexture;
            LeftArrow.Texture = Init.ScrollLeftTexture;
            RightArrow.Texture = Init.ScrollRightTexture;

            UpArrow.FillColor = Init.ControlDark;
            DownArrow.FillColor = Init.ControlDark;
            LeftArrow.FillColor = Init.ControlDark;
            RightArrow.FillColor = Init.ControlDark;

            VerticalBack.FillColor = Init.ControlLight;
            HorizontalBack.FillColor = Init.ControlLight;

            VGrabbed = false;
            HGrabbed = false;
            Relative = new Vector2f();

            InternUpdate();
        }

        protected override void InternUpdate()
        {
            if (Offset.X < 0)
                Offset = new Vector2f(0, Offset.Y);
            if (Offset.Y < 0)
                Offset = new Vector2f(Offset.X, 0);
            IntRect size = new IntRect();
            size.Width = (int)(ReservedSpace.Width - Padding.X * 2);
            size.Height = (int)(ReservedSpace.Height - Padding.Y * 2);
            if ((ScrollingStyle & Mode.ALLOW_VERTICAL_SCROLLING) == Mode.ALLOW_VERTICAL_SCROLLING)
            {
                size.Width -= 20;
            }
            if ((ScrollingStyle & Mode.ALLOW_HORIZONTAL_SCROLLING) == Mode.ALLOW_HORIZONTAL_SCROLLING)
            {
                size.Height -= 20;
            }
            Displayer.Size = new Vector2f(size.Width, size.Height);
            Displayer.TextureRect = size;
            Displayer.Position = new Vector2f((int)ReservedSpace.Left, (int)ReservedSpace.Top) + Padding;
            Border[0].Position = Displayer.Position + new Vector2f(.5f, .5f);
            Border[1].Position = Border[0].Position + new Vector2f(size.Width, 0);
            Border[2].Position = Border[1].Position + new Vector2f(0, size.Height);
            Border[3].Position = Border[2].Position + new Vector2f(-size.Width, 0);
            Border[4].Position = Border[0].Position;

            if (Content != null)
            {
                FloatRect availableSpace = new FloatRect();
                Vector2f minContentSize = Content.GetMinimumSize();
                availableSpace.Width = Math.Max(minContentSize.X, size.Width);
                availableSpace.Height = Math.Max(minContentSize.Y, size.Height);
                Content.Update(availableSpace);
                if (Offset.X > 0 && minContentSize.X - Offset.X < size.Width)
                    Offset = new Vector2f(minContentSize.X - size.Width, Offset.Y);
                if (Offset.Y > 0 && minContentSize.Y - Offset.Y < size.Height)
                    Offset = new Vector2f(Offset.X, minContentSize.Y - size.Height);

                if ((ScrollingStyle & Mode.ALLOW_VERTICAL_SCROLLING) == Mode.ALLOW_VERTICAL_SCROLLING)
                {
                    UpBack.Position = Displayer.Position + new Vector2f(Displayer.Size.X, 0);
                    UpArrow.Position = Displayer.Position + new Vector2f(Displayer.Size.X, 0);
                    DownBack.Position = Displayer.Position + new Vector2f(Displayer.Size.X, Displayer.Size.Y - 20);
                    DownArrow.Position = Displayer.Position + new Vector2f(Displayer.Size.X, Displayer.Size.Y - 20);
                    VerticalBack.Position = Displayer.Position + new Vector2f(Displayer.Size.X, 20);
                    VerticalBack.Size = new Vector2f(20, Displayer.Size.Y - 40);
                    VerticalControl.Size = new Vector2f(20, VerticalBack.Size.Y * Displayer.Size.Y / Content.GetMinimumSize().Y);
                    VerticalControl.Position = new Vector2f(Displayer.Size.X + ReservedSpace.Left, Utilities.Interpolation(Utilities.Percent(Offset.Y, 0, Content.GetMinimumSize().Y - Displayer.Size.Y),
                                                                                                    UpArrow.Size.Y, VerticalBack.Size.Y + UpArrow.Size.Y - VerticalControl.Size.Y) + ReservedSpace.Top) + Padding;
                }
                if ((ScrollingStyle & Mode.ALLOW_HORIZONTAL_SCROLLING) == Mode.ALLOW_HORIZONTAL_SCROLLING)
                {
                    LeftBack.Position = Displayer.Position + new Vector2f(0, Displayer.Size.Y);
                    LeftArrow.Position = Displayer.Position + new Vector2f(0, Displayer.Size.Y);
                    RightBack.Position = Displayer.Position + new Vector2f(Displayer.Size.X - 20, Displayer.Size.Y);
                    RightArrow.Position = Displayer.Position + new Vector2f(Displayer.Size.X - 20, Displayer.Size.Y);
                    HorizontalBack.Position = Displayer.Position + new Vector2f(20, Displayer.Size.Y);
                    HorizontalBack.Size = new Vector2f(Displayer.Size.X - 40, 20);
                    HorizontalControl.Size = new Vector2f((Displayer.Size.X - 40) * Displayer.Size.X / Content.GetMinimumSize().X, 20);
                    HorizontalControl.Position = Displayer.Position + new Vector2f(Utilities.Interpolation(Utilities.Percent(Offset.X, 0, Content.GetMinimumSize().X - Displayer.Size.X),
                                                                                                    LeftArrow.Size.X, RightArrow.Position.X - HorizontalControl.Size.X - Padding.X), Displayer.Size.Y);
                }
                if (HGrabbed)
                    HorizontalControl.FillColor = Init.ControlDark;
                else
                    HorizontalControl.FillColor = Init.ControlMedium;
                if (VGrabbed)
                    VerticalControl.FillColor = Init.ControlDark;
                else
                    VerticalControl.FillColor = Init.ControlMedium;
            }
        }

        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;

            if (Content != null)
            {
                Buffer.Clear(Color.Transparent);
                Vector2f depl = new Vector2f();
                if ((ScrollingStyle & Mode.ALLOW_VERTICAL_SCROLLING) == Mode.ALLOW_VERTICAL_SCROLLING)
                {
                    depl.Y = -(int)Offset.Y;
                }
                if ((ScrollingStyle & Mode.ALLOW_HORIZONTAL_SCROLLING) == Mode.ALLOW_HORIZONTAL_SCROLLING)
                {
                    depl.X = -(int)Offset.X;
                }
                Content.Draw(Buffer, depl);
                Buffer.Display();
            }
            target.Draw(Displayer, new RenderStates(tr.Transform));
            if ((ScrollingStyle & Mode.ALLOW_VERTICAL_SCROLLING) == Mode.ALLOW_VERTICAL_SCROLLING)
            {
                target.Draw(UpBack, new RenderStates(tr.Transform));
                target.Draw(UpArrow, new RenderStates(tr.Transform));
                target.Draw(DownBack, new RenderStates(tr.Transform));
                target.Draw(DownArrow, new RenderStates(tr.Transform));
                target.Draw(VerticalBack, new RenderStates(tr.Transform));
                if (Displayer.Size.Y < Content.GetMinimumSize().Y)
                    target.Draw(VerticalControl, new RenderStates(tr.Transform));
            }
            if ((ScrollingStyle & Mode.ALLOW_HORIZONTAL_SCROLLING) == Mode.ALLOW_HORIZONTAL_SCROLLING)
            {
                target.Draw(LeftBack, new RenderStates(tr.Transform));
                target.Draw(LeftArrow, new RenderStates(tr.Transform));
                target.Draw(RightBack, new RenderStates(tr.Transform));
                target.Draw(RightArrow, new RenderStates(tr.Transform));
                target.Draw(HorizontalBack, new RenderStates(tr.Transform));
                if (Displayer.Size.X < Content.GetMinimumSize().X)
                    target.Draw(HorizontalControl, new RenderStates(tr.Transform));
            }
            target.Draw(Border, PrimitiveType.LinesStrip, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = MinimumSize;
            if ((ScrollingStyle & Mode.ALLOW_HORIZONTAL_SCROLLING) != Mode.ALLOW_HORIZONTAL_SCROLLING)
            {
                result.X = Content.GetMinimumSize().X;
            }
            if ((ScrollingStyle & Mode.ALLOW_VERTICAL_SCROLLING) != Mode.ALLOW_VERTICAL_SCROLLING)
            {
                result.Y = Content.GetMinimumSize().Y;
            }
            if ((ScrollingStyle & Mode.ALLOW_HORIZONTAL_SCROLLING) == Mode.ALLOW_HORIZONTAL_SCROLLING)
            {
                result.Y += 20;
            }
            if ((ScrollingStyle & Mode.ALLOW_VERTICAL_SCROLLING) == Mode.ALLOW_VERTICAL_SCROLLING)
            {
                result.X += 20;
            }
            result += Padding * 2;
            return result;
        }

        internal override void DrawUpper(RenderTarget target, Vector2f decal)
        {
            base.DrawUpper(target, decal);
            if (Content != null)
                Content.DrawUpper(target, decal - Offset + Displayer.Position);
        }

        internal override void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonDownCall(button, pos, intercept);
            if (Content != null && Displayer.GetGlobalBounds().Contains(pos))
                Content.MouseButtonDownCall(button, pos - Displayer.Position + Offset, intercept);
            else if (Content != null)
                Content.MouseButtonDownCall(button, pos - Displayer.Position + Offset, true);
            if (!intercept)
            {
                if (button == Mouse.Button.Left)
                {
                    if (UpArrow.GetGlobalBounds().Contains(pos))
                    {
                        if (Offset.Y > 0)
                            Offset = new Vector2f(Offset.X, Math.Max(0, Offset.Y - Step));
                    }
                    if (DownArrow.GetGlobalBounds().Contains(pos))
                    {
                        if (Offset.Y < Content.GetMinimumSize().Y - Displayer.Size.Y)
                            Offset = new Vector2f(Offset.X, Math.Min(Content.GetMinimumSize().Y - Displayer.Size.Y, Offset.Y + Step));
                    }
                    if (LeftArrow.GetGlobalBounds().Contains(pos))
                    {
                        if (Offset.X > 0)
                            Offset = new Vector2f(Math.Max(0, Offset.X - Step), Offset.Y);
                    }
                    if (RightArrow.GetGlobalBounds().Contains(pos))
                    {
                        if (Offset.X < Content.GetMinimumSize().X - Displayer.Size.X)
                            Offset = new Vector2f(Math.Min(Content.GetMinimumSize().X - Displayer.Size.X, Offset.X + Step), Offset.Y);
                    }
                    if (VerticalControl.GetGlobalBounds().Contains(pos) && Displayer.Size.Y < Content.GetMinimumSize().Y)
                    {
                        VGrabbed = true;
                        Relative = pos - VerticalControl.Position;
                    }
                    if (HorizontalControl.GetGlobalBounds().Contains(pos) && Displayer.Size.X < Content.GetMinimumSize().X)
                    {
                        HGrabbed = true;
                        Relative = pos - HorizontalControl.Position;
                    }
                }
            }
        }

        internal override void MouseButtonUpCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonUpCall(button, pos, intercept);
            if (Content != null && Displayer.GetGlobalBounds().Contains(pos))
                Content.MouseButtonUpCall(button, pos - Displayer.Position + Offset, intercept);
            else if (Content != null)
                Content.MouseButtonUpCall(button, pos - Displayer.Position + Offset, true);
            if (VGrabbed)
                VGrabbed = false;
            if (HGrabbed)
                HGrabbed = false;
        }

        internal override void MouseScrolledCall(int delta)
        {
            base.MouseScrolledCall(delta);
            if (Content != null)
                Content.MouseScrolledCall(delta);
            if (MouseOnWidget && !NoWheelScrolling)
            {
                if ((ScrollingStyle & Mode.ALLOW_VERTICAL_SCROLLING) == Mode.ALLOW_VERTICAL_SCROLLING)
                {
                    float depl = -delta * Step;
                    if (depl < 0 && Offset.Y > 0)
                        Offset = new Vector2f(Offset.X, Math.Max(0, Offset.Y + depl));
                    if (depl > 0 && Offset.Y < Content.GetMinimumSize().Y - Displayer.Size.Y)
                        Offset = new Vector2f(Offset.X, Math.Min(Content.GetMinimumSize().Y - Displayer.Size.Y, Offset.Y + depl));
                }
                else if ((ScrollingStyle & Mode.ALLOW_HORIZONTAL_SCROLLING) == Mode.ALLOW_HORIZONTAL_SCROLLING)
                {
                    float depl = -delta * Step;
                    if (depl < 0 && Offset.X > 0)
                        Offset = new Vector2f(Math.Max(0, Offset.X + depl), Offset.Y);
                    if (depl > 0 && Offset.X < Content.GetMinimumSize().X - Displayer.Size.X)
                        Offset = new Vector2f(Math.Min(Content.GetMinimumSize().X - Displayer.Size.X, Offset.X + depl), Offset.Y);
                }
            }
        }

        internal override void MouseMovedCall(Vector2f pos, bool intercept)
        {
            base.MouseMovedCall(pos, intercept);
            if (Content != null && Displayer.GetGlobalBounds().Contains(pos))
                Content.MouseMovedCall(pos - Displayer.Position + Offset, intercept);
            else if (Content != null)
                Content.MouseMovedCall(pos - Displayer.Position + Offset, true);
            if (!intercept)
            {
                if (UpBack.GetGlobalBounds().Contains(pos))
                    UpBack.FillColor = Init.ControlMedium;
                else
                    UpBack.FillColor = Init.ControlLight;
                if (DownBack.GetGlobalBounds().Contains(pos))
                    DownBack.FillColor = Init.ControlMedium;
                else
                    DownBack.FillColor = Init.ControlLight;
                if (LeftBack.GetGlobalBounds().Contains(pos))
                    LeftBack.FillColor = Init.ControlMedium;
                else
                    LeftBack.FillColor = Init.ControlLight;
                if (RightBack.GetGlobalBounds().Contains(pos))
                    RightBack.FillColor = Init.ControlMedium;
                else
                    RightBack.FillColor = Init.ControlLight;
            }
            if (VGrabbed)
            {
                Offset = new Vector2f(Offset.X, Utilities.Interpolation(Utilities.Percent(pos.Y - Relative.Y, VerticalBack.Position.Y, VerticalBack.Position.Y + VerticalBack.Size.Y - VerticalControl.Size.Y), 0, Content.GetMinimumSize().Y - Displayer.Size.Y));
            }
            if (HGrabbed)
            {
                Offset = new Vector2f(Utilities.Interpolation(Utilities.Percent(pos.X - Relative.X, HorizontalBack.Position.X, HorizontalBack.Position.X + HorizontalBack.Size.X - HorizontalControl.Size.X), 0, Content.GetMinimumSize().X - Displayer.Size.X), Offset.Y);
            }
        }
        internal override void TextEnteredCall(string code)
        {
            base.TextEnteredCall(code);
            if (Content != null)
                Content.TextEnteredCall(code);
        }

        internal override void KeyPressedCall(KeyEventArgs args)
        {
            base.KeyPressedCall(args);
            if (Content != null)
                Content.KeyPressedCall(args);
        }

        internal override void KeyReleasedCall(KeyEventArgs args)
        {
            base.KeyReleasedCall(args);
            if (Content != null)
                Content.KeyReleasedCall(args);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace WGP.Gui
{
    /// <summary>
    /// The base class to render a widget. The window creates a GUI context to draw a widget.
    /// </summary>
    public class Window
    {
        public class Exception : System.Exception
        {
            public Exception(string str = "") : base(str) { }
        }
        
        private const float IconSize = 20;
        internal bool triggerEvents;

        public event EventHandler Grabbed;
        public event EventHandler MouseEntered;
        public event EventHandler MouseLeaved;
        public event EventHandler Closed;
        public event EventHandler Opened;
        public event EventHandler RequestClose;
        public event EventHandler Released;
        public event EventHandler Resized;
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
                    content.Parent = this;
                Size = Size;
            }
        }

        internal RenderWindow App { get; set; }
        /// <summary>
        /// The SFML window view used to draw must be specified if another one is used in the event loop and update loop. Most of the time, the default view is the one used to draw the GUI winwow.
        /// </summary>
        public View WindowView { get; set; }
        private RectangleShape Titlebar { get; set; }
        private RectangleShape Background { get; set; }
        private TEXT.Text TitleText { get; set; }
        private Vertex[] Border { get; set; }
        private RectangleShape HideContentIcon { get; set; }
        private RectangleShape CloseIcon { get; set; }
        private RectangleShape ResizeIcon { get; set; }
        /// <summary>
        /// Returns true if the window is moveable.
        /// </summary>
        public bool Moveable { get; }
        private RectangleShape TitleBack { get; set; }
        private Vector2f RelativeGrab { get; set; }
        private Vector2f RelativeSizer { get; set; }
        internal Widget InterceptEvents { get; set; }
        private bool hidden;
        /// <summary>
        /// Hidding the window will hide its widget, but not the titlebar. Require the WGP.Gui.Window.Mode.HIDE_CONTENT flag.
        /// </summary>
        public bool Hidden
        {
            get => hidden;
            set
            {
                if (HideContentIcon != null)
                    hidden = value;
                else
                    throw new Exception("Unable to hide a window that doesn't have the WGP.Gui.Window.Mode.HIDE_CONTENT flag.");
            }
        }
        /// <summary>
        /// Returns true if the window is open.
        /// </summary>
        public bool IsOpen { get; private set; }
        /// <summary>
        /// Returns true if the window is grabbed.
        /// </summary>
        public bool IsGrabbed { get; private set; }
        /// <summary>
        /// Returns true if the window is being resized.
        /// </summary>
        public bool IsResized { get; private set; }
        /// <summary>
        /// Returns true if the mouse is on the window.
        /// </summary>
        public bool MouseOnWindow { get; private set; }

        private Vector2f size;
        private Vector2f minimumDefinedSize;
        private Widget content;
        /// <summary>
        /// Its size.
        /// </summary>
        public Vector2f Size
        {
            get => size;
            set
            {
                Vector2f minimumSize = new Vector2f();
                float minTitleWidth = 0;
                if (Content != null)
                    minimumSize = Content.GetMinimumSize();
                if (ResizeIcon != null)
                    minimumSize += new Vector2f(IconSize, IconSize);
                if (TitleText != null)
                {
                    if (TitleText.String != "")
                        minTitleWidth += 10 + TitleText.FindCharacterPos((uint)TitleText.String.Count()).X;
                }
                if (CloseIcon != null)
                    minTitleWidth += IconSize;
                if (HideContentIcon != null)
                    minTitleWidth += IconSize;

                size.Y = Utilities.Max(minimumSize.Y, value.Y);
                size.X = Utilities.Max(Utilities.Max(minimumSize.X, minTitleWidth), value.X);
                if (ResizeIcon == null)
                    minimumDefinedSize = value;
                if (Resized != null)
                    Resized(this, new EventArgs());
            }
        }
        private Vector2f position;
        /// <summary>
        /// Its position.
        /// </summary>
        public Vector2f Position
        {
            get => position;
            set
            {
                position.X = (int)value.X;
                position.Y = (int)value.Y;
            }
        }
        /// <summary>
        /// Its title. Require the WGP.Gui.Window.Mode.TITLEBAR flag.
        /// </summary>
        public string Title
        {
            get => TitleText.String;
            set
            {
                if (TitleText == null)
                    throw new Exception("Unable to change the title of a window that doesn't have the WGP.Gui.Window.Mode.TITLEBAR flag.");
                TitleText.String = value;
                Size = Size;
            }
        }

        public enum Mode
        {
            NONE = 0,
            TITLEBAR = 0x1,
            RESIZE = 0x2 | BACKGROUND,
            CLOSE = 0x4 | TITLEBAR,
            HIDE_CONTENT = 0x8 | TITLEBAR,
            BACKGROUND = 0x10,
            MOVEABLE = 0x20 | TITLEBAR,
            DEFAULT = TITLEBAR | RESIZE | CLOSE | BACKGROUND | MOVEABLE | HIDE_CONTENT
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="target">The target SFML window.</param>
        /// <param name="mode">Options flags. (Optional)</param>
        public Window(RenderWindow target, Mode mode = Mode.DEFAULT)
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            App = target;
            App.MouseButtonPressed += OnMouseDown;
            App.MouseButtonReleased += OnMouseUp;
            App.MouseWheelScrolled += OnMouseScrolled;
            App.MouseMoved += OnMouseMoved;
            App.TextEntered += OnTextEntered;
            App.KeyPressed += OnKeyPressed;
            App.KeyReleased += OnKeyReleased;
            Size = new Vector2f();

            if ((mode & Mode.TITLEBAR) == Mode.TITLEBAR)
            {
                Titlebar = new RectangleShape();
                Titlebar.FillColor = Init.Titlebar;
                TitleText = new TEXT.Text();
                TitleText.Color = Init.TextDark;
                TitleText.Font = Init.Font;
            }
            if ((mode & Mode.MOVEABLE) == Mode.MOVEABLE)
            {
                Moveable = true;
                TitleBack = new RectangleShape();
                TitleBack.Texture = Init.TitleBackTexture;
                TitleBack.FillColor = Init.BackgroundBorder;
            }
            else
                Moveable = false;

            if ((mode & Mode.RESIZE) == Mode.RESIZE)
            {
                ResizeIcon = new RectangleShape(new Vector2f(IconSize, IconSize));
                ResizeIcon.FillColor = Init.BackgroundBorder;
                ResizeIcon.Texture = Init.ResizeTexture;
            }

            if ((mode & Mode.CLOSE) == Mode.CLOSE)
            {
                CloseIcon = new RectangleShape(new Vector2f(IconSize, IconSize));
                CloseIcon.FillColor = Init.BackgroundBorder;
                CloseIcon.Texture = Init.CloseTexture;
            }

            if ((mode & Mode.HIDE_CONTENT) == Mode.HIDE_CONTENT)
            {
                HideContentIcon = new RectangleShape(new Vector2f(IconSize, IconSize));
                HideContentIcon.FillColor = Init.BackgroundBorder;
            }

            if ((mode & Mode.BACKGROUND) == Mode.BACKGROUND)
            {
                Background = new RectangleShape();
                Background.FillColor = Init.Background;
                Border = new Vertex[8];
                for (int i = 0; i < 8; i++)
                    Border[i].Color = Init.BackgroundBorder;
            }
            if (HideContentIcon != null)
                Hidden = false;
            IsOpen = true;
            IsGrabbed = false;
            IsResized = false;
            MouseOnWindow = false;
            triggerEvents = true;
        }
        /// <summary>
        /// Draws the window and its widget.
        /// </summary>
        public void Draw()
        {
            if (IsOpen)
            {
                if (Titlebar != null)
                {
                    App.Draw(Titlebar);
                    App.Draw(TitleText);
                }
                if (TitleBack != null)
                    App.Draw(TitleBack);
                if (Background != null)
                {
                    if (!Hidden)
                        App.Draw(Background);
                    App.Draw(Border, PrimitiveType.Lines);
                }
                if (HideContentIcon != null)
                    App.Draw(HideContentIcon);
                if (CloseIcon != null)
                    App.Draw(CloseIcon);
                if (ResizeIcon != null && !Hidden)
                    App.Draw(ResizeIcon);
                if (Content != null && !Hidden)
                {
                    Vector2f decal = Position;
                    if (Titlebar != null)
                        decal.Y += IconSize;
                    Content.Draw(App, decal);
                    content.DrawUpper(App, decal);
                }
            }
        }
        /// <summary>
        /// Update the window and its widget.
        /// </summary>
        public void Update()
        {
            if (IsOpen)
            {
                if (Background != null)
                {
                    Background.Size = Size;
                    Vector2f entryPoint = Position;
                    if (Titlebar != null)
                        entryPoint.Y += IconSize;
                    Background.Position = entryPoint;

                    Vector2f globalSize = Size;
                    if (Hidden)
                        globalSize.Y = 0;
                    if (Titlebar != null)
                        globalSize.Y += IconSize;

                    Border[0].Position = new Vector2f(.5f + (int)Position.X, .5f + (int)Position.Y);
                    Border[1].Position = Border[0].Position + new Vector2f((int)globalSize.X, 0);
                    Border[2].Position = Border[1].Position;
                    Border[3].Position = Border[2].Position + new Vector2f(0, (int)globalSize.Y);
                    Border[4].Position = Border[3].Position;
                    Border[5].Position = Border[4].Position + new Vector2f(-(int)globalSize.X, 0);
                    Border[6].Position = Border[5].Position;
                    Border[7].Position = Border[6].Position + new Vector2f(0, -(int)globalSize.Y);
                }
                if (Titlebar != null)
                {
                    Titlebar.Size = new Vector2f(Size.X, IconSize);
                    Titlebar.Position = Position;
                    TitleText.Position = Position + new Vector2f(5, (int)((IconSize - Init.TextSize) / 2) + Init.TextSize);
                }
                if (TitleBack != null)
                {
                    if (Title.Count() > 0)
                    {
                        TitleBack.Position = new Vector2f(TitleText.Position.X + TitleText.FindCharacterPos((uint)TitleText.String.Count()).X + 5, Position.Y);
                        float decal = 0;
                        if (CloseIcon != null)
                            decal += IconSize;
                        if (HideContentIcon != null)
                            decal += IconSize;
                        TitleBack.Size = new Vector2f((Utilities.Max((int)Size.X - TitleText.FindCharacterPos((uint)TitleText.String.Count()).X - decal - 15, 0)), (int)IconSize);
                        TitleBack.TextureRect = new IntRect(0, 0, (int)TitleBack.Size.X, (int)IconSize);
                    }
                    else
                    {
                        TitleBack.Position = new Vector2f(5, 0) + Position;
                        float decal = 0;
                        if (CloseIcon != null)
                            decal += IconSize;
                        if (HideContentIcon != null)
                            decal += IconSize;
                        TitleBack.Size = new Vector2f(Utilities.Max(Size.X - decal - 10, 0), IconSize);
                        TitleBack.TextureRect = new IntRect(0, 0, (int)TitleBack.Size.X, (int)IconSize);
                    }
                }

                if (ResizeIcon != null)
                {
                    ResizeIcon.Position = Position + Size - new Vector2f(IconSize, IconSize);
                    if (Titlebar != null)
                        ResizeIcon.Position += new Vector2f(0, IconSize);
                }
                else
                    Size = minimumDefinedSize;

                if (CloseIcon != null)
                {
                    CloseIcon.Position = Position + new Vector2f(Size.X - IconSize, 0);
                }
                if (HideContentIcon != null)
                {
                    HideContentIcon.Position = Position + new Vector2f(Size.X - IconSize, 0);
                    if (CloseIcon != null)
                        HideContentIcon.Position -= new Vector2f(IconSize, 0);
                    if (Hidden)
                        HideContentIcon.Texture = Init.ShowTexture;
                    else
                        HideContentIcon.Texture = Init.HideTexture;
                }
                FloatRect availableSpace = new FloatRect(new Vector2f(), Size);
                if (ResizeIcon != null)
                {
                    availableSpace.Width -= IconSize;
                    availableSpace.Height -= IconSize;
                }
                if (Content != null)
                {
                    Vector2f offset = Position;
                    if (Titlebar != null)
                        offset.Y += IconSize;
                    Content.Update(availableSpace, App.MapPixelToCoords(Mouse.GetPosition(App), WindowView ?? App.GetView()) - offset);
                }

                Vector2f minimumSize = new Vector2f();
                float minTitleWidth = 0;
                if (Content != null)
                    minimumSize = Content.GetMinimumSize();
                if (ResizeIcon != null)
                    minimumSize += new Vector2f(IconSize, IconSize);
                if (TitleText != null)
                {
                    if (TitleText.String != "")
                        minTitleWidth += 10 + TitleText.FindCharacterPos((uint)TitleText.String.Count()).X;
                }
                if (CloseIcon != null)
                    minTitleWidth += IconSize;
                if (HideContentIcon != null)
                    minTitleWidth += IconSize;

                size.Y = Utilities.Max(minimumSize.Y, Size.Y);
                size.X = Utilities.Max(Utilities.Max(minimumSize.X, minTitleWidth), Size.X);
            }
        }
        /// <summary>
        /// Closes the window.
        /// </summary>
        public void Close()
        {
            if (IsOpen)
            {
                IsOpen = false;
                if (Closed != null)
                    Closed(this, new EventArgs());
            }
        }
        /// <summary>
        /// Open the window.
        /// </summary>
        public void Open()
        {
            if (!IsOpen)
            {
                IsOpen = true;
                if (Opened != null)
                    Opened(this, new EventArgs());
            }
        }

        private void OnMouseDown(object sender, SFML.Window.MouseButtonEventArgs e)
        {
            if (triggerEvents && IsOpen)
            {
                View tmp = null;
                if (WindowView != null)
                    tmp = WindowView;
                else
                    tmp = App.GetView();
                Vector2f pos = App.MapPixelToCoords(new Vector2i(e.X, e.Y), tmp);
                bool occuped = false;
                if (CloseIcon != null)
                {
                    if (CloseIcon.GetGlobalBounds().Contains(pos))
                    {
                        occuped = true;
                        if (RequestClose != null)
                            RequestClose(this, new EventArgs());
                    }
                }
                if (HideContentIcon != null)
                {
                    if (HideContentIcon.GetGlobalBounds().Contains(pos))
                    {
                        occuped = true;
                        Hidden = !Hidden;
                    }
                }
                if (ResizeIcon != null && !Hidden)
                {
                    if (ResizeIcon.GetGlobalBounds().Contains(pos))
                    {
                        RelativeSizer = ResizeIcon.Position + new Vector2f(IconSize, IconSize) - pos;
                        IsResized = true;
                    }
                }
                if (Titlebar != null && Moveable)
                {
                    if (!occuped && Titlebar.GetGlobalBounds().Contains(pos))
                    {
                        IsGrabbed = true;
                        if (Grabbed != null)
                            Grabbed(this, new EventArgs());
                        RelativeGrab = Position - pos;
                    }
                }
                View tmp1 = null;
                if (WindowView != null)
                    tmp1 = WindowView;
                else
                    tmp1 = App.GetView();
                Vector2f MousePos = App.MapPixelToCoords(new Vector2i(e.X, e.Y), tmp1) - Position;
                if (Titlebar != null)
                    MousePos.Y -= IconSize;
                if (Content != null && !Hidden && InterceptEvents == null)
                    Content.MouseButtonDownCall(e.Button, MousePos);
                else if (!Hidden && InterceptEvents != null)
                    InterceptEvents.MouseButtonDownCall(e.Button, MousePos);
            }
        }
        /// <summary>
        /// Returns the bounds of the window.
        /// </summary>
        /// <returns></returns>
        public FloatRect GetBounds()
        {
            if (!IsOpen)
                return new FloatRect(Position, new Vector2f());
            FloatRect result = new FloatRect();
            result.Width = Size.X;
            if (!Hidden)
            result.Height = Size.Y;
            if (Titlebar != null)
                result.Height += IconSize;
            result.Left = Position.X;
            result.Top = Position.Y;
            return result;
        }

        private void OnMouseUp(object sender, SFML.Window.MouseButtonEventArgs e)
        {
            if (IsOpen)
            {
                if (IsResized)
                {
                    IsResized = false;
                    if (Resized != null)
                        Resized(this, new EventArgs());
                }
                if (IsGrabbed)
                {
                    IsGrabbed = false;
                    if (Released != null)
                        Released(this, new EventArgs());
                }
                View tmp = null;
                if (WindowView != null)
                    tmp = WindowView;
                else
                    tmp = App.GetView();
                Vector2f MousePos = App.MapPixelToCoords(new Vector2i(e.X, e.Y), tmp) - Position;
                if (Titlebar != null)
                    MousePos.Y -= IconSize;
                if (Content != null && !Hidden && InterceptEvents == null)
                    Content.MouseButtonUpCall(e.Button, MousePos);
                if (!hidden && InterceptEvents != null)
                    InterceptEvents.MouseButtonUpCall(e.Button, MousePos);
            }
        }

        private void OnMouseScrolled(object sender, SFML.Window.MouseWheelScrollEventArgs e)
        {
            if (IsOpen)
            {
                if (Content != null && !Hidden && InterceptEvents == null && triggerEvents == true)
                    Content.MouseScrolledCall((int)e.Delta);
                if (!hidden && InterceptEvents != null && triggerEvents == true)
                    InterceptEvents.MouseScrolledCall((int)e.Delta);
            }
        }

        private void OnMouseMoved(object sender, SFML.Window.MouseMoveEventArgs e)
        {
            if (IsOpen)
            {
                View tmp = null;
                if (WindowView != null)
                    tmp = WindowView;
                else
                    tmp = App.GetView();
                Vector2f pos = App.MapPixelToCoords(new Vector2i(e.X, e.Y), tmp);
                if (IsResized)
                {
                    Vector2f newSize = pos - Position + RelativeSizer;
                    if (Titlebar != null)
                        newSize.Y -= IconSize;
                    Size = newSize;
                }
                if (IsGrabbed)
                    Position = pos + RelativeGrab;
                if (!MouseOnWindow)
                {
                    if (GetBounds().Contains(pos))
                    {
                        MouseOnWindow = true;
                        if (MouseEntered != null)
                            MouseEntered(this, new EventArgs());
                    }
                }
                else
                {
                    if (!GetBounds().Contains(pos))
                    {
                        MouseOnWindow = false;
                        if (MouseLeaved != null)
                            MouseLeaved(this, new EventArgs());
                    }
                }
                if (Content != null && !Hidden && InterceptEvents == null)
                {
                    pos -= Position;
                    if (Titlebar != null)
                        pos.Y -= IconSize;
                    Content.MouseMovedCall(pos);
                }
                if (!hidden && InterceptEvents != null)
                {
                    pos -= Position;
                    if (Titlebar != null)
                        pos.Y -= IconSize;
                    InterceptEvents.MouseMovedCall(pos);
                }
            }
        }

        private void OnTextEntered(object sender, SFML.Window.TextEventArgs e)
        {
            if (IsOpen)
            {
                if (Content != null && !Hidden && InterceptEvents == null)
                    Content.TextEnteredCall(e.Unicode);
                if (!hidden && InterceptEvents != null)
                    InterceptEvents.TextEnteredCall(e.Unicode);
            }
        }

        private void OnKeyPressed(object sender, SFML.Window.KeyEventArgs e)
        {
            if (IsOpen)
            {
                if (Content != null && !Hidden && InterceptEvents == null)
                    Content.KeyPressedCall(e);
                if (!hidden && InterceptEvents != null)
                    InterceptEvents.KeyPressedCall(e);
            }
        }

        private void OnKeyReleased(object sender, SFML.Window.KeyEventArgs e)
        {
            if (IsOpen)
            {
                if (Content != null && !Hidden && InterceptEvents == null)
                    Content.KeyReleasedCall(e);
                if (!hidden && InterceptEvents != null)
                    InterceptEvents.KeyReleasedCall(e);
            }
        }
    }
}

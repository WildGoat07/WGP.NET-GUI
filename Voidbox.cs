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
    /// Basic container that handle one widget with no graphics.
    /// </summary>
    public class Voidbox : Widget
    {
        private Window parent;
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
        private Widget content;
        /// <summary>
        /// Its widget.
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
        /// <summary>
        /// Constructor.
        /// </summary>
        public Voidbox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();

            Content = null;
            Pattern = new FloatRect(0, 0, 1, 1);
            Padding = new Vector2f(0, 0);

            InternUpdate();
        }

        protected override void InternUpdate()
        {
            if (Content != null)
            {
                FloatRect availableSpace = new FloatRect();
                availableSpace.Left = ReservedSpace.Left + Padding.X;
                availableSpace.Top = ReservedSpace.Top + Padding.Y;
                availableSpace.Width = ReservedSpace.Width - Padding.X * 2;
                availableSpace.Height = ReservedSpace.Height - Padding.Y * 2;
                Content.Update(availableSpace);
            }
        }

        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            if (Content != null)
                Content.Draw(target, decal);
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f();
            if (Content != null)
                result = Content.GetMinimumSize();
            result += Padding * 2;
            return result;
        }

        internal override void DrawUpper(RenderTarget target, Vector2f decal)
        {
            base.DrawUpper(target, decal);
            if (Content != null)
                Content.DrawUpper(target, decal);
        }

        internal override void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonDownCall(button, pos, intercept);
            if (Content != null)
                Content.MouseButtonDownCall(button, pos, intercept);
        }

        internal override void MouseButtonUpCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonUpCall(button, pos, intercept);
            if (Content != null)
                Content.MouseButtonUpCall(button, pos, intercept);
        }

        internal override void MouseScrolledCall(int delta)
        {
            base.MouseScrolledCall(delta);
            if (Content != null)
                Content.MouseScrolledCall(delta);
        }

        internal override void MouseMovedCall(Vector2f pos, bool intercept)
        {
            base.MouseMovedCall(pos, intercept);
            if (Content != null)
                Content.MouseMovedCall(pos, intercept);
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

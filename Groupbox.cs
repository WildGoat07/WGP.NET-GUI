﻿using System;
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
    /// Group box container.
    /// </summary>
    public class Groupbox : Widget
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
        private Vertex[] Border { get; set; }
        private Text Label { get; set; }
        /// <summary>
        /// Its title.
        /// </summary>
        public string Title
        {
            get => Label.DisplayedString;
            set => Label.DisplayedString = value;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Groupbox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();

            Content = null;
            Label = new Text("", Init.Font, Init.TextSize);

            Label.Color = Init.TextDark;

            Border = new Vertex[6];

            for (int i = 0; i < 6; i++)
                Border[i].Color = Init.BorderMedium;

            InternUpdate();
        }

        protected override void InternUpdate()
        {
            Border[0].Position = new Vector2f((int)ReservedSpace.Left + .5f + 5, (int)ReservedSpace.Top + .5f + Init.TextSize / 2) + Padding;
            Border[1].Position = Border[0].Position + new Vector2f(-5, 0);
            Border[2].Position = Border[1].Position + new Vector2f(0, (int)ReservedSpace.Height - Padding.Y * 2 - (Init.TextSize / 2));
            Border[3].Position = Border[2].Position + new Vector2f(ReservedSpace.Width - Padding.X * 2, 0);
            Border[4].Position = Border[3].Position + new Vector2f(0, -(int)ReservedSpace.Height + Padding.Y * 2 + (Init.TextSize / 2));
            Border[5].Position = new Vector2f(ReservedSpace.Left + 15 + Label.FindCharacterPos((uint)Label.DisplayedString.Count()).X, (int)ReservedSpace.Top + .5f + Init.TextSize / 2) + Padding;
            Label.Position = new Vector2f((int)ReservedSpace.Left + 10, (int)ReservedSpace.Top) + Padding;

            if (Content != null)
            {
                FloatRect availableSpace = new FloatRect();
                availableSpace.Left = ReservedSpace.Left + Padding.X * 2;
                availableSpace.Top = ReservedSpace.Top + Padding.Y * 2 + Init.TextSize;
                availableSpace.Width = ReservedSpace.Width - Padding.X * 4;
                availableSpace.Height = ReservedSpace.Height - Padding.Y * 4 - Init.TextSize;
                Content.Update(availableSpace);
            }
        }

        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Label, new RenderStates(tr.Transform));
            target.Draw(Border, PrimitiveType.LinesStrip, new RenderStates(tr.Transform));
            if (Content != null)
                Content.Draw(target, decal);
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f();
            float minW = Label.FindCharacterPos((uint)Label.DisplayedString.Count()).X + 20;
            if (Content != null)
                result.X = Math.Max(minW, Padding.X * 2 + Content.GetMinimumSize().X);
            else
                result.X = Math.Max(minW, Padding.X * 2);
            result.Y = Init.TextSize + Padding.Y * 2;
            if (Content != null)
                result.Y += Content.GetMinimumSize().Y;
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

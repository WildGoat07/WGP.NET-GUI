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
    public class Gauge : Widget
    {
        static private Color Blue = new Color(66, 163, 221);
        static private Color DarkBlue = new Color(20, 70, 150);

        public enum Mode
        {
            HORIZONTAL,
            VERTICAL
        }

        public event EventHandler ValueChanged;

        private CircleShape Cursor { get; set; }
        private Vertex[] Line { get; set; }
        private bool Grabbed { get; set; }
        private float percent;
        public float Percent
        {
            get => percent;
            set
            {
                percent = Math.Min(1, Math.Max(0, value));
                if (ValueChanged != null)
                    ValueChanged(this, new EventArgs());
            }
        }

        public Mode Orientation { get; set; }

        public Gauge() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            Cursor = new CircleShape(8, 15);
            Cursor.Origin = new Vector2f(Cursor.Radius, Cursor.Radius);
            Line = new Vertex[4];
            for (int i = 0; i < 2; i++)
                Line[i].Color = Blue;
            Orientation = Mode.HORIZONTAL;
            Percent = 0;

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Line, PrimitiveType.Lines, new RenderStates(tr.Transform));
            target.Draw(Cursor, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f(Cursor.Radius * 2, Cursor.Radius * 2);
            if (Orientation == Mode.HORIZONTAL)
                result.X += Cursor.Radius * 5;
            else if (Orientation == Mode.VERTICAL)
                result.Y += Cursor.Radius * 5;
            result += Padding * 2;
            return result;
        }

        protected override void InternUpdate()
        {
            if (Orientation == Mode.HORIZONTAL)
            {
                pattern = new FloatRect(Pattern.Left, Pattern.Top, Pattern.Width, 0);
                Line[0].Position = new Vector2f((int)ReservedSpace.Left + .5f + Cursor.Radius, (int)ReservedSpace.Top + .5f + Cursor.Radius) + Padding;
                Line[3].Position = Line[0].Position + new Vector2f((int)ReservedSpace.Width - Padding.X * 2 - Cursor.Radius * 2, 0);
            }
            else if (Orientation == Mode.VERTICAL)
            {
                pattern = new FloatRect(Pattern.Left, Pattern.Top, 0, Pattern.Height);
                Line[3].Position = new Vector2f((int)ReservedSpace.Left + .5f + Cursor.Radius, (int)ReservedSpace.Top + .5f + Cursor.Radius) + Padding;
                Line[0].Position = Line[3].Position + new Vector2f(0, (int)ReservedSpace.Height - Padding.Y * 2 - Cursor.Radius * 2);
            }
            Line[1].Position = new Vector2f(Utilities.Interpolation(Percent, Line[0].Position.X, Line[3].Position.X),
                                           Utilities.Interpolation(Percent, Line[0].Position.Y, Line[3].Position.Y));
            Line[2].Position = Line[1].Position;
            Cursor.Position = Line[1].Position;

            if (Grabbed)
            {
                for (int i = 2; i < 4; i++)
                    Line[i].Color = Init.BorderDark;
                Cursor.FillColor = DarkBlue;
            }
            else
            {
                for (int i = 2; i < 4; i++)
                    Line[i].Color = Init.BorderMedium;
                Cursor.FillColor = Blue;
            }
        }

        internal override void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonDownCall(button, pos, intercept);
            if (!intercept)
            {
                FloatRect hitbox = new FloatRect();
                if (Orientation == Mode.HORIZONTAL)
                {
                    hitbox.Left = Line[0].Position.X - Cursor.Radius;
                    hitbox.Top = Line[0].Position.Y - Cursor.Radius;
                    hitbox.Width = Line[3].Position.X - hitbox.Left + Cursor.Radius * 2;
                    hitbox.Height = Cursor.Radius * 2;
                }
                else if (Orientation == Mode.VERTICAL)
                {
                    hitbox.Left = Line[3].Position.X - Cursor.Radius;
                    hitbox.Top = Line[3].Position.Y - Cursor.Radius;
                    hitbox.Width = Cursor.Radius * 2;
                    hitbox.Height = Line[0].Position.Y - hitbox.Top + Cursor.Radius * 2;
                }
                if (hitbox.Contains(pos) && button == Mouse.Button.Left)
                {
                    Grabbed = true;
                    if (Orientation == Mode.HORIZONTAL)
                        Percent = Utilities.Percent(pos.X, Line[0].Position.X, Line[3].Position.X);
                    if (Orientation == Mode.VERTICAL)
                        Percent = Utilities.Percent(pos.Y, Line[0].Position.Y, Line[3].Position.Y);
                }
            }
        }

        internal override void MouseButtonUpCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonUpCall(button, pos, intercept);
            if (Grabbed && button == Mouse.Button.Left)
                Grabbed = false;
        }

        internal override void MouseMovedCall(Vector2f pos, bool intercept)
        {
            base.MouseMovedCall(pos, intercept);
            if (Grabbed)
            {
                if (Orientation == Mode.HORIZONTAL)
                    Percent = Utilities.Percent(pos.X, Line[0].Position.X, Line[3].Position.X);
                if (Orientation == Mode.VERTICAL)
                    Percent = Utilities.Percent(pos.Y, Line[0].Position.Y, Line[3].Position.Y);
            }
        }
    }
}

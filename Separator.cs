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
    /// Basic separator widget. The separator is a straight line to separate widgets.
    /// </summary>
    public class Separator : Widget
    {
        public enum Mode
        {
            HORIZONTAL,
            VERTICAL
        }
        private Vertex[] Line { get; set; }
        /// <summary>
        /// Its orientation.
        /// </summary>
        public Mode Orientation { get; set; }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Separator() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            Line = new Vertex[2];
            Orientation = Mode.HORIZONTAL;
            for (int i = 0; i < 2; i++)
                Line[i].Color = Init.BorderMedium;

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Line, PrimitiveType.Lines, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f(1, 1);
            result += Padding * 2;
            return result;
        }

        protected override void InternUpdate()
        {
            if (Orientation == Mode.HORIZONTAL)
            {
                pattern = new FloatRect(Pattern.Left, Pattern.Top, Pattern.Width, 0);
                Line[0].Position = new Vector2f((int)ReservedSpace.Left + .5f, (int)ReservedSpace.Top + .5f) + Padding;
                Line[1].Position = Line[0].Position + new Vector2f((int)ReservedSpace.Width - Padding.X * 2, 0);
            }
            else if (Orientation == Mode.VERTICAL)
            {
                pattern = new FloatRect(Pattern.Left, Pattern.Top, 0, Pattern.Height);
                Line[0].Position = new Vector2f((int)ReservedSpace.Left + .5f, (int)ReservedSpace.Top + .5f) + Padding;
                Line[1].Position = Line[0].Position + new Vector2f(0, (int)ReservedSpace.Height - Padding.Y * 2);
            }
        }
    }
}
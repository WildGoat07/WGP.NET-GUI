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
    /// Progressbar widget.
    /// </summary>
    public class Progressbar : Widget
    {
        private RectangleShape Back { get; set; }
        private Vertex[] Border { get; set; }
        private Vertex[] Fill { get; set; }
        private float percent;
        /// <summary>
        /// The percentage of filling of the progressbar.
        /// </summary>
        public float Percent
        {
            get => percent;
            set
            {
                percent = Utilities.Min(1, Utilities.Max(0, value));
            }
        }
        /// <summary>
        /// The minimal size of the progressbar.
        /// </summary>
        /// <remarks>
        /// It must be defined because the progressbar has no size reference, like a text.
        /// </remarks>
        public Vector2f MinimumizeSize { get; set; }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Progressbar() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            Back = new RectangleShape();
            Fill = new Vertex[4];
            Border = new Vertex[10];
            Percent = 0;
            MinimumizeSize = new Vector2f(Init.TextSize*2, Init.TextSize);
            for (int i = 0; i < 8; i++)
                Border[i].Color = Init.BorderMedium;

            Fill[0].Color = new Color(106, 230, 168);
            Fill[1].Color = new Color(150, 255, 190);
            Fill[2].Color = new Color(150, 255, 190);
            Fill[3].Color = new Color(106, 230, 168);

            for (int i = 8; i < 10; i++)
                Border[i].Color = Init.BorderMedium;

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Back, new RenderStates(tr.Transform));
            target.Draw(Fill, PrimitiveType.Quads, new RenderStates(tr.Transform));
            target.Draw(Border, PrimitiveType.Lines, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = MinimumizeSize;
            result += Padding * 2;
            result += new Vector2f(20, 10);
            return result;
        }

        protected override void InternUpdate()
        {
            Back.Size = new Vector2f(ReservedSpace.Width, ReservedSpace.Height) - Padding * 2;
            Back.Position = new Vector2f(ReservedSpace.Left, ReservedSpace.Top) + Padding;
            Vector2f fillSize = new Vector2f(Utilities.Interpolation(Percent, 0, Back.Size.X), Back.Size.Y);
            Border[0].Position = new Vector2f((int)ReservedSpace.Left + .5f, (int)ReservedSpace.Top + .5f) + Padding;
            Border[1].Position = Border[0].Position + new Vector2f((int)ReservedSpace.Width - Padding.X * 2, 0);
            Border[2].Position = Border[1].Position;
            Border[3].Position = Border[2].Position + new Vector2f(0, (int)ReservedSpace.Height - Padding.Y * 2);
            Border[4].Position = Border[3].Position;
            Border[5].Position = Border[4].Position + new Vector2f(-(int)ReservedSpace.Width + Padding.X * 2, 0);
            Border[6].Position = Border[5].Position;
            Border[7].Position = Border[6].Position + new Vector2f(0, -(int)ReservedSpace.Height + Padding.Y * 2);

            Fill[0].Position = Back.Position;
            Fill[1].Position = Fill[0].Position + new Vector2f(fillSize.X, 0);
            Fill[2].Position = Fill[1].Position + new Vector2f(0, fillSize.Y);
            Fill[3].Position = Fill[2].Position + new Vector2f(-fillSize.X, 0);

            Border[8].Position = new Vector2f((int)Fill[1].Position.X + .5f, Fill[1].Position.Y);
            Border[9].Position = new Vector2f((int)Fill[2].Position.X + .5f, Fill[2].Position.Y);
        }
    }
}

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
    /// Widget that displays an SFML texture.
    /// </summary>
    public class Picture : Widget
    {
        private RectangleShape Img { get; set; }
        /// <summary>
        /// Texture of the image.
        /// </summary>
        /// <value>Texture of the image.</value>
        public Texture Source
        {
            get => Img.Texture;
            set => Img.Texture = value;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Picture() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            Img = new RectangleShape();

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Img, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            if (Img.Texture == null)
                return new Vector2f();
            Vector2f result = new Vector2f(Img.Texture.Size.X, Img.Texture.Size.Y);
            result += Padding * 2;
            return result;
        }

        protected override void InternUpdate()
        {
            if (Img.Texture != null)
            {
                Img.Size = new Vector2f(ReservedSpace.Width, ReservedSpace.Height) - Padding * 2;
                Img.Position = new Vector2f(ReservedSpace.Left, ReservedSpace.Top) + Padding;
            }
        }
    }
}

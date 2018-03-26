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
    /// Base text widget.
    /// </summary>
    public class Label : Widget
    {

        private TEXT.Text label { get; set; }
        /// <summary>
        /// Title of the widget.
        /// </summary>
        /// <value>String of the title.</value>
        public string Title
        {
            get => label.String;
            set => label.String = value;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Label() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            label = new TEXT.Text();

            label.Font = Init.Font;
            label.Color = Init.TextDark;

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(label, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f(label.FindCharacterPos((uint)label.String.Count()).X, Init.TextSize);
            result += Padding * 2;
            return result;
        }
        protected override FloatRect GetHitbox()
        {
            return new FloatRect(Padding.X + ReservedSpace.Left, Padding.Y + ReservedSpace.Top, label.FindCharacterPos((uint)label.String.Count()).X, Init.TextSize);
        }

        protected override void InternUpdate()
        {
            pattern = new FloatRect(Pattern.Left, Pattern.Top, 0, 0);
            label.Position = new Vector2f((int)(ReservedSpace.Left),
                                          (int)(ReservedSpace.Top) + Init.TextSize) + Padding;

        }
    }
}

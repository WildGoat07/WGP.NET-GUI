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
    public class Label : Widget
    {

        private Text label { get; set; }
        public string Title
        {
            get => label.DisplayedString;
            set => label.DisplayedString = value;
        }

        public Label() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            label = new Text();

            label.Font = Init.Font;
            label.CharacterSize = Init.TextSize;
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
            Vector2f result = new Vector2f(label.GetGlobalBounds().Width, Init.TextSize);
            result += Padding * 2;
            return result;
        }
        protected override FloatRect GetHitbox()
        {
            return new FloatRect(Padding.X + ReservedSpace.Left, Padding.Y + ReservedSpace.Top, label.GetGlobalBounds().Width, Init.TextSize);
        }

        protected override void InternUpdate()
        {
            pattern = new FloatRect(Pattern.Left, Pattern.Top, 0, 0);
            label.Position = new Vector2f((int)(ReservedSpace.Left),
                                          (int)(ReservedSpace.Top)) + Padding;

        }
    }
}

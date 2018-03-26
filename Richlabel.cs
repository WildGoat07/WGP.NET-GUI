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
using WGP.TEXT;

namespace WGP.Gui
{
    /// <summary>
    /// Advanced text widget.
    /// </summary>
    public class Richlabel : Widget
    {

        private RichText Label { get; set; }
        /// <summary>
        /// Maximum width of the text.
        /// </summary>
        public float MaxWidth { get; set; }


        public event EventHandler ClickedOnText;

        public class TextCLickedEventArgs : EventArgs
        {
            public int Index { get; set; }
            public TextCLickedEventArgs() { }
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Richlabel() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            Label = new RichText();

            Label.Font = Init.Font;
            MouseClick += OnClick;

            InternUpdate();
        }

        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Label, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f(Label.GetGlobalBounds().Width, Label.GetGlobalBounds().Height + Init.TextSize);
            result += Padding * 2;
            return result;
        }
        /// <summary>
        /// Adds a part to the text.
        /// </summary>
        /// <param name="part">Part to add.</param>
        public void AddPart(RichText.Part part) => Label.AddPart(part);
        protected override void InternUpdate()
        {
            pattern = new FloatRect(Pattern.Left, Pattern.Top, 0, 0);
            Label.Position = new Vector2f((int)(ReservedSpace.Left),
                                          (int)(ReservedSpace.Top) + Init.TextSize) + Padding;
        }
        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            if (Label.PointOn(e.Position) != -1)
            {
                if (ClickedOnText != null)
                    ClickedOnText(this, new TextCLickedEventArgs() { Index = Label.PointOn(e.Position) });
            }
        }
        /// <summary>
        /// Generates the text.
        /// </summary>
        public void Generate() => Label.Generate();
        /// <summary>
        /// Clears the text.
        /// </summary>
        public void Clear() => Label.Clear();
    }
}

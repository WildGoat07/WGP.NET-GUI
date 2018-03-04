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
    /// Basic radio buttons widget.
    /// </summary>
    public class Radiogroup : Widget
    {
        public event EventHandler SelectionChanged;

        private List<Text> Labels { get; set; }
        private CircleShape FillBuff { get; set; }
        private Vertex[] BorderBuff { get; set; }
        private int MouseOnSelection { get; set; }

        private int check;
        /// <summary>
        /// Checked index.
        /// </summary>
        public int Checked
        {
            get => check;
            set
            {
                check = value;
                if (SelectionChanged != null)
                    SelectionChanged(this, new EventArgs());
            }
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Radiogroup() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            BorderBuff = new Vertex[20];
            FillBuff = new CircleShape(Init.TextSize/2f*(8f/14), 20);
            FillBuff.Origin = new Vector2f(FillBuff.Radius, FillBuff.Radius);
            FillBuff.FillColor = Init.ControlDark;
            Labels = new List<Text>();

            for (int i = 0; i < BorderBuff.Count(); i++)
                BorderBuff[i].Color = Init.ControlDark;

            MouseMoved += OnMouseMoved;
            MouseClick += OnClick;

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            for (int i = 0; i < Labels.Count; i++)
            {
                target.Draw(BorderBuff, PrimitiveType.LinesStrip, new RenderStates(tr.Transform));
                target.Draw(Labels[i], new RenderStates(tr.Transform));
                if (i == Checked)
                    target.Draw(FillBuff, new RenderStates(tr.Transform));
                tr.Position += new Vector2f(0, Init.TextSize + 5);
            }
        }

        internal override Vector2f GetMinimumSize()
        {
            float minWidth = 0;
            for (int i = 0; i < Labels.Count; i++)
            {
                float width = Init.TextSize + 5 + Labels[i].FindCharacterPos((uint)Labels[i].DisplayedString.Count()).X;
                if (width > minWidth)
                    minWidth = width;
            }
            Vector2f result = new Vector2f(minWidth, Labels.Count * (Init.TextSize + 5) - 5);
            result += Padding * 2;
            return result;
        }

        protected override void InternUpdate()
        {
            pattern = new FloatRect(Pattern.Left, Pattern.Top, 0, 0);
            for (int i = 0;i<Labels.Count;i++)
                Labels[i].Position = new Vector2f((int)(Init.TextSize + 5 + ReservedSpace.Left),
                                             (int)ReservedSpace.Top) + Padding;
            FillBuff.Position = new Vector2f(ReservedSpace.Left + Init.TextSize / 2f, ReservedSpace.Top + Init.TextSize / 2f) + Padding;

            for (float i = 0; i < BorderBuff.Count(); i++)
                BorderBuff[(int)i].Position = new Vector2f((float)(Init.TextSize / 2f + Math.Cos(i / (BorderBuff.Count() - 1) * Math.PI * 2) * Init.TextSize / 2f + ReservedSpace.Left), (float)(Init.TextSize / 2f + Math.Sin(i / (BorderBuff.Count() - 1) * Math.PI * 2) * Init.TextSize / 2f + ReservedSpace.Top)) + Padding;

        }

        protected override FloatRect GetHitbox()
        {
            FloatRect result;
            result.Left = ReservedSpace.Left + Padding.X;
            result.Top = ReservedSpace.Top + Padding.Y;
            result.Width = Init.TextSize;
            result.Height = Labels.Count * (Init.TextSize + 5) - 5;
            return result;
        }
        /// <summary>
        /// Adds a new options at the end of the group.
        /// </summary>
        /// <param name="name">Option's name.</param>
        public void AddOption(string name)
        {
            Text tmp = new Text(name, Init.Font, Init.TextSize);
            tmp.Color = Init.TextDark;
            Labels.Add(tmp);
            
        }

        private void OnMouseMoved(object sender, MouseMovedEventArgs e)
        {
            if (GetHitbox().Contains(e.Position))
            {
                MouseOnSelection = (int)(e.Position.Y - GetHitbox().Top) / (int)(Init.TextSize + 5);
            }
        }

        private void OnClick(object sender, EventArgs e)
        {
            Checked = MouseOnSelection;
        }
    }
}

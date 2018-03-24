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
    /// Basic combo box. An alternative to the radio buttons.
    /// </summary>
    public class Combobox : Widget
    {
        public event EventHandler SelectionChanged;

        private Text Label { get; set; }
        private RectangleShape Back { get; set; }
        private Vertex[] Border { get; set; }
        private RectangleShape Arrow { get; set; }
        private RectangleShape BackArrow { get; set; }
        private RectangleShape BackContext { get; set; }
        private RectangleShape SelectedContext { get; set; }
        private List<Text> Buffer { get; set; }
        /// <summary>
        /// List of the options.
        /// </summary>
        public List<string> List { get; set; }
        private bool MouseOnContext { get; set; }
        private int PointedAt { get; set; }
        private Vector2f UpperDecal { get; set; }
        private bool developped;
        /// <summary>
        /// When developped, the widget will intercept any other widget event until an option is selected or if canceled.
        /// </summary>
        public bool Developped
        {
            get => developped;
            set
            {
                developped = value;
                if (developped)
                    Parent.InterceptEvents = this;
                else
                    Parent.InterceptEvents = null;
            }
        }
        private Vertex[] BorderContext { get; set; }
        private uint selection;
        /// <summary>
        /// Current selection.
        /// </summary>
        public uint Selection
        {
            get => selection;
            set
            {
                if (List.Count() > 0)
                {
                    selection = value % (uint)List.Count();
                    if (SelectionChanged != null)
                        SelectionChanged(this, new EventArgs());
                }
            }
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Combobox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            Label = new Text();
            Back = new RectangleShape();
            Arrow = new RectangleShape(new Vector2f(10 + Init.TextSize, 10 + Init.TextSize));
            BackArrow = new RectangleShape(new Vector2f(10 + Init.TextSize, 10 + Init.TextSize));
            Border = new Vertex[10];
            BorderContext = new Vertex[5];

            Label.Font = Init.Font;
            Label.CharSize = Init.TextSize;
            Back.FillColor = Init.LightX;
            Label.Color = Init.DarkX;
            Arrow.FillColor = Init.ControlMedium;
            Arrow.Texture = Init.DownArrowTexture;
            Buffer = new List<Text>();
            BackContext = new RectangleShape();
            BackContext.FillColor = Init.LightX;
            SelectedContext = new RectangleShape();
            SelectedContext.FillColor = Init.ControlLight;
            selection = 0;
            List = new List<string>();
            for (int i = 0; i < 10; i++)
                Border[i].Color = Init.BorderMedium;
            for (int i = 0; i < 5; i++)
                BorderContext[i].Color = Init.BorderMedium;

            UpperDecal = new Vector2f();

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Back, new RenderStates(tr.Transform));
            target.Draw(BackArrow, new RenderStates(tr.Transform));
            target.Draw(Border, PrimitiveType.Lines, new RenderStates(tr.Transform));
            target.Draw(Label, new RenderStates(tr.Transform));
            target.Draw(Arrow, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f(Label.FindCharacterPos((uint)Label.String.Count()).X, Init.TextSize);
            result += Padding * 2;
            result += new Vector2f(20 + Arrow.Size.X, 10);
            return result;
        }

        protected override void InternUpdate()
        {
            pattern = new FloatRect(Pattern.Left, Pattern.Top, Pattern.Width, 0);
            Back.Size = new Vector2f(ReservedSpace.Width - Padding.X * 2, 10 + Init.TextSize);
            Back.Position = new Vector2f((int)ReservedSpace.Left, (int)ReservedSpace.Top) + Padding;
            Border[0].Position = new Vector2f((int)ReservedSpace.Left + .5f, (int)ReservedSpace.Top + .5f) + Padding;
            Border[1].Position = Border[0].Position + new Vector2f(ReservedSpace.Width - Padding.X * 2, 0);
            Border[2].Position = Border[1].Position;
            Border[3].Position = Border[2].Position + new Vector2f(0, ReservedSpace.Height - Padding.Y * 2);
            Border[4].Position = Border[3].Position;
            Border[5].Position = Border[4].Position + new Vector2f(-ReservedSpace.Width + Padding.X * 2, 0);
            Border[6].Position = Border[5].Position;
            Border[7].Position = Border[6].Position + new Vector2f(0, -ReservedSpace.Height + Padding.Y * 2);
            Border[8].Position = Border[1].Position - new Vector2f(Arrow.Size.X, 0);
            Border[9].Position = Border[8].Position + new Vector2f(0, ReservedSpace.Height - Padding.Y * 2);
            Arrow.Position = new Vector2f((int)(Back.Position.X + Back.Size.X - Arrow.Size.X), (int)Back.Position.Y);
            BackArrow.Position = new Vector2f((int)(Back.Position.X + Back.Size.X - Arrow.Size.X + 1), (int)Back.Position.Y);
            Label.Position = new Vector2f(10, 5 + Init.TextSize) + Back.Position;
            if (List.Count > 0)
                Label.String = List[(int)Selection];
            else
                Label.String = "";

            BackContext.Position = Back.Position + new Vector2f(0, Back.Size.Y);

            if (List.Count > 0 && Developped)
            {
                float LMax = Back.Size.X;
                Buffer.Clear();
                for (int i = 0;i<List.Count;i++)
                {
                    Text tmp = new Text(List[i], Init.Font, Init.TextSize);
                    tmp.Color = Init.DarkX;
                    tmp.Position = BackContext.Position + new Vector2f(10, i * (Init.TextSize + 5) + Init.TextSize);
                    LMax = Utilities.Max(LMax, 20 + tmp.FindCharacterPos((uint)tmp.String.Count()).X);
                    Buffer.Add(tmp);
                }
                SelectedContext.Size = new Vector2f(LMax, (Init.TextSize + 5));
                SelectedContext.Position = BackContext.Position + new Vector2f(0, PointedAt * (Init.TextSize + 5));
                BackContext.Size = new Vector2f(LMax, List.Count * (Init.TextSize + 5));

                BorderContext[0].Position = BackContext.Position + new Vector2f(.5f, .5f);
                BorderContext[1].Position = BorderContext[0].Position + new Vector2f(BackContext.Size.X, 0);
                BorderContext[2].Position = BorderContext[1].Position + new Vector2f(0, BackContext.Size.Y);
                BorderContext[3].Position = BorderContext[2].Position + new Vector2f(-BackContext.Size.X, 0);
                BorderContext[4].Position = BorderContext[0].Position;
            }

            if (Pressed || Developped)
            {
                BackArrow.FillColor = Init.ControlDark;
            }
            else if (MouseOnWidget)
            {
                BackArrow.FillColor = Init.ControlLight;
            }
            else
            {
                BackArrow.FillColor = Init.LightX;
            }
        }

        protected override FloatRect GetHitbox()
        {
            return BackArrow.GetGlobalBounds();
        }

        internal override void DrawUpper(RenderTarget target, Vector2f decal)
        {
            UpperDecal = decal;
            if (Developped && List.Count() > 0)
            {
                Transformable tr = new Transformable();
                tr.Position = decal;
                target.Draw(BackContext, new RenderStates(tr.Transform));
                if (PointedAt != -1)
                    target.Draw(SelectedContext, new RenderStates(tr.Transform));
                target.Draw(BorderContext, PrimitiveType.LinesStrip, new RenderStates(tr.Transform));
                foreach (var item in Buffer)
                {
                    target.Draw(item, new RenderStates(tr.Transform));
                }
            }
            base.DrawUpper(target, decal);
        }

        internal override void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonDownCall(button, pos, intercept);
            if (Developped && !MouseOnContext)
                Developped = false;
            else if (!intercept && !Developped && MouseOnWidget)
                Developped = true;
            if (MouseOnContext && Developped)
            {
                Selection = (uint)PointedAt;
                Developped = false;
            }
        }

        internal override void MouseMovedCall(Vector2f pos, bool intercept)
        {
            base.MouseMovedCall(pos, intercept);
            View tmp = null;
            if (Parent.WindowView != null)
                tmp = Parent.WindowView;
            else
                tmp = Parent.App.GetView();
            Vector2f MousePos = Parent.App.MapPixelToCoords(Mouse.GetPosition(Parent.App), tmp);
            MouseOnContext = BackContext.GetGlobalBounds().Contains(MousePos - UpperDecal);
            Vector2f relativPos = MousePos - BackContext.Position - UpperDecal;
            if (MouseOnContext)
            {
                PointedAt = (int)(relativPos.Y / (Init.TextSize + 5));
            }
            else
                PointedAt = -1;
        }
    }
}

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
    public class Tabcontrol : Widget
    {
        internal class Pair
        {
            public Widget Widget;
            public string Title;
        }

        private Window parent;
        internal override Window Parent
        {
            get => parent;
            set
            {
                parent = value;
                for (var it = Widgets.First; it != null; it = it.Next)
                    it.Value.Widget.Parent = parent;
            }
        }
        private ChainedList<Pair> Widgets { get; set; }
        private RectangleShape TitleBackBuffer { get; set; }
        private Text TitleBuffer { get; set; }
        private Vertex[] TitleBorderBuffer { get; set; }
        public Widget ActiveWidget { get; private set; }

        public enum Mode
        {
            HORIZONTAL,
            VERTICAL
        }

        public Tabcontrol() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();

            Widgets = new ChainedList<Pair>();
            ActiveWidget = null;
            TitleBackBuffer = new RectangleShape();
            TitleBuffer = new Text("", Init.Font, Init.TextSize);
            TitleBuffer.Color = Init.TextDark;
            TitleBorderBuffer = new Vertex[16];
            for (int i = 0; i < 8; i++)
                TitleBorderBuffer[i].Color = Init.BorderMedium;
            for (int i = 8; i < 16; i++)
                TitleBorderBuffer[i].Color = Init.BorderDark;
            InternUpdate();
        }

        protected override void InternUpdate()
        {
            TitleBorderBuffer[0].Position = new Vector2f((int)ReservedSpace.Left + .5f, ReservedSpace.Top + .5f + Init.TextSize + 6) + Padding;
            TitleBorderBuffer[1].Position = TitleBorderBuffer[0].Position + new Vector2f(0, (int)ReservedSpace.Height - Init.TextSize - 6 - Padding.Y * 2);
            TitleBorderBuffer[2].Position = TitleBorderBuffer[1].Position;
            TitleBorderBuffer[3].Position = TitleBorderBuffer[2].Position + new Vector2f((int)ReservedSpace.Width - Padding.X * 2, 0);
            TitleBorderBuffer[4].Position = TitleBorderBuffer[3].Position;
            TitleBorderBuffer[5].Position = TitleBorderBuffer[4].Position + new Vector2f(0, -(int)ReservedSpace.Height + Init.TextSize + 6 + Padding.Y * 2);
            TitleBorderBuffer[6].Position = TitleBorderBuffer[5].Position;
            TitleBorderBuffer[7].Position = TitleBorderBuffer[0].Position;

            FloatRect availableSpace = ReservedSpace;
            availableSpace.Height -= Init.TextSize + 6 + Padding.Y * 2;
            availableSpace.Top += Init.TextSize + 6 + Padding.Y;
            availableSpace.Left += Padding.X;
            availableSpace.Width -= Padding.X * 2;
            if (ActiveWidget != null)
            ActiveWidget.Update(availableSpace);
        }

        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            if (ActiveWidget != null)
                ActiveWidget.Draw(target, decal);
            Transformable tr = new Transformable();
            tr.Position = decal;
            int offset = 0;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                TitleBackBuffer.Position = new Vector2f((int)ReservedSpace.Left + offset, (int)ReservedSpace.Top) + Padding;
                TitleBuffer.DisplayedString = it.Value.Title;
                TitleBackBuffer.Size = new Vector2f(10 + (int)TitleBuffer.FindCharacterPos((uint)TitleBuffer.DisplayedString.Count()).X, 6 + Init.TextSize);
                TitleBuffer.Position = new Vector2f(5, 3) + TitleBackBuffer.Position;
                if (it.Value.Widget == ActiveWidget)
                    TitleBackBuffer.FillColor = Init.LightX;
                else
                    TitleBackBuffer.FillColor = Init.ControlLight;
                TitleBorderBuffer[8].Position = TitleBackBuffer.Position + new Vector2f(.5f, .5f);
                TitleBorderBuffer[9].Position = TitleBorderBuffer[8].Position + new Vector2f(TitleBackBuffer.Size.X, 0);
                TitleBorderBuffer[10].Position = TitleBorderBuffer[9].Position;
                TitleBorderBuffer[11].Position = TitleBorderBuffer[8].Position + new Vector2f(TitleBackBuffer.Size.X, TitleBackBuffer.Size.Y);
                TitleBorderBuffer[12].Position = TitleBorderBuffer[11].Position;
                TitleBorderBuffer[13].Position = TitleBorderBuffer[8].Position + new Vector2f(0, TitleBackBuffer.Size.Y);
                TitleBorderBuffer[14].Position = TitleBorderBuffer[13].Position;
                TitleBorderBuffer[15].Position = TitleBorderBuffer[8].Position;
                target.Draw(TitleBackBuffer, new RenderStates(tr.Transform));
                target.Draw(TitleBuffer, new RenderStates(tr.Transform));
                target.Draw(TitleBorderBuffer, 8, 8, PrimitiveType.Lines, new RenderStates(tr.Transform));
                offset += (int)TitleBackBuffer.Size.X;
            }
            target.Draw(TitleBorderBuffer, 0, 8, PrimitiveType.Lines, new RenderStates(tr.Transform));
        }

        public void Add(Widget widget, string title)
        {
            if (Widgets.Count == 0)
                ActiveWidget = widget;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (it.Value.Widget == widget)
                    throw new Exception("Le widget existe déjà dans le conteneur.");
            }
            Widgets.Add(new Pair() { Widget = widget, Title = title });
            widget.Parent = Parent;
        }

        public void Insert(Widget widget, Widget At, string title)
        {
            if (Widgets.Count == 0)
                ActiveWidget = widget;
            ChainedList<Pair>.Element<Pair> iterator = null;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (it.Value.Widget == widget)
                    throw new Exception("Le widget existe déjà dans le conteneur.");
                if (it.Value.Widget == At)
                    iterator = it;
            }
            if (iterator == null)
                throw new Exception("Le widget de réference n'existe pas dans le conteneur.");
            Widgets.Insert(iterator, new Pair() { Widget = widget, Title = title });
            widget.Parent = Parent;
        }

        public void SetTitle(Widget At, string title)
        {
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (it.Value.Widget == At)
                {
                    it.Value.Title = title;
                    return;
                }
            }
            throw new Exception("Le widget de réference n'existe pas dans le conteneur.");
        }

        public string GetTitle(Widget At)
        {
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (it.Value.Widget == At)
                {
                    return it.Value.Title;
                }
            }
            throw new Exception("Le widget de réference n'existe pas dans le conteneur.");
        }

        public void Remove(Widget At)
        {
            ChainedList<Pair>.Element<Pair> iterator = null;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (it.Value.Widget == At)
                    iterator = it;
            }
            if (iterator == null)
                throw new Exception("Le widget de réference n'existe pas dans le conteneur.");
            Widgets.Remove(iterator);
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f();
            if (ActiveWidget != null)
                result = ActiveWidget.GetMinimumSize();
            if (Widgets.Count > 0)
                result.Y += Init.TextSize + 10;
            float minW = 0;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                TitleBuffer.DisplayedString = it.Value.Title;
                minW += TitleBuffer.FindCharacterPos((uint)TitleBuffer.DisplayedString.Count()).X + 10;
            }
            result.X = Math.Max(result.X, minW);
            result += Padding * 2;
            return result;
        }

        internal override void DrawUpper(RenderTarget target, Vector2f decal)
        {
            base.DrawUpper(target, decal);
            if (ActiveWidget != null)
                ActiveWidget.DrawUpper(target, decal);
        }

        internal override void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonDownCall(button, pos, intercept);
            float offset = 0;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                TitleBuffer.DisplayedString = it.Value.Title;
                if (new FloatRect(ReservedSpace.Left + Padding.X + offset, ReservedSpace.Top, TitleBuffer.FindCharacterPos((uint)TitleBuffer.DisplayedString.Count()).X + 10, Init.TextSize + 6).Contains(pos))
                    ActiveWidget = it.Value.Widget;
                offset += TitleBuffer.FindCharacterPos((uint)TitleBuffer.DisplayedString.Count()).X + 10;
            }
            if (ActiveWidget != null)
                ActiveWidget.MouseButtonDownCall(button, pos, intercept);
        }

        internal override void MouseButtonUpCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonUpCall(button, pos, intercept);
            if (ActiveWidget != null)
                ActiveWidget.MouseButtonUpCall(button, pos, intercept);
        }

        internal override void MouseScrolledCall(int delta)
        {
            base.MouseScrolledCall(delta);
            if (ActiveWidget != null)
                ActiveWidget.MouseScrolledCall(delta);
        }

        internal override void MouseMovedCall(Vector2f pos, bool intercept)
        {
            base.MouseMovedCall(pos, intercept);
            if (ActiveWidget != null)
                ActiveWidget.MouseMovedCall(pos, intercept);
        }

        internal override void TextEnteredCall(string code)
        {
            base.TextEnteredCall(code);
            if (ActiveWidget != null)
                ActiveWidget.TextEnteredCall(code);
        }

        internal override void KeyPressedCall(KeyEventArgs args)
        {
            base.KeyPressedCall(args);
            if (ActiveWidget != null)
                ActiveWidget.KeyPressedCall(args);
        }

        internal override void KeyReleasedCall(KeyEventArgs args)
        {
            base.KeyReleasedCall(args);
            if (ActiveWidget != null)
                ActiveWidget.KeyReleasedCall(args);
        }
    }
}

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
    public class Multibox : Widget
    {
        internal class Pair : IComparable
        {
            public Widget Widget;
            public float Weight;

            public int CompareTo(object obj)
            {
                return (Weight * widgetSize).CompareTo(((Pair)obj).Weight * ((Pair)obj).widgetSize);
            }

            public FloatRect availableSpace;
            public float widgetSize;
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
        public Mode Orientation { get; set; }
        public bool AutomaticWeightDistribution { get; set; }

        public enum Mode
        {
            HORIZONTAL,
            VERTICAL
        }

        public Multibox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();

            Widgets = new ChainedList<Pair>();
            Padding = new Vector2f();
            Orientation = Mode.VERTICAL;
            AutomaticWeightDistribution = false;

            InternUpdate();
        }

        protected override void InternUpdate()
        {
            Pair[] Tab = Widgets.ToArray();
            if (Orientation == Mode.VERTICAL)
            {
                for (var it = Widgets.First; it != null; it = it.Next)
                    it.Value.widgetSize = it.Value.Widget.GetMinimumSize().Y;
            }
            if (Orientation == Mode.HORIZONTAL)
            {
                for (var it = Widgets.First; it != null; it = it.Next)
                    it.Value.widgetSize = it.Value.Widget.GetMinimumSize().X;
            }

            if (AutomaticWeightDistribution)
            {
                if (Orientation == Mode.VERTICAL)
                {
                    for (var it = Widgets.First; it != null; it = it.Next)
                        it.Value.Weight = it.Value.Widget.GetMinimumSize().Y;
                }
                if (Orientation == Mode.HORIZONTAL)
                {
                    for (var it = Widgets.First; it != null; it = it.Next)
                        it.Value.Weight = it.Value.Widget.GetMinimumSize().X;
                }
            }
            Array.Sort(Tab);
            float total = 0;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                total += it.Value.Weight;
            }
            float offset = 0;
            float spaceLeft = 0;
            float WeightUsed = 0;
            if (Orientation == Mode.VERTICAL)
            {
                offset = Padding.Y;
                spaceLeft = ReservedSpace.Height - Padding.Y * 2 - GetMinimumSize().Y;
            }
            if (Orientation == Mode.HORIZONTAL)
            {
                offset = Padding.X;
                spaceLeft = ReservedSpace.Width - Padding.X * 2 - GetMinimumSize().X;
            }
            foreach (var item in Tab)
            {
                if (Orientation == Mode.VERTICAL)
                {
                    item.availableSpace.Width = ReservedSpace.Width - Padding.X * 2;
                    item.availableSpace.Height = Math.Max(spaceLeft * Utilities.Percent(item.Weight, 0, total - WeightUsed), 0) + item.Widget.GetMinimumSize().Y;
                    spaceLeft -= Math.Max(spaceLeft * Utilities.Percent(item.Weight, 0, total - WeightUsed), 0);
                    WeightUsed += item.Weight;
                }
                if (Orientation == Mode.HORIZONTAL)
                {
                    item.availableSpace.Height = ReservedSpace.Height - Padding.Y * 2;
                    item.availableSpace.Width = Math.Max(spaceLeft * Utilities.Percent(item.Weight, 0, total - WeightUsed), 0) + item.Widget.GetMinimumSize().X;
                    spaceLeft -= Math.Max(spaceLeft * Utilities.Percent(item.Weight, 0, total - WeightUsed), 0);
                    WeightUsed += item.Weight;
                }
            }
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (Orientation == Mode.VERTICAL)
                {
                    it.Value.availableSpace.Left = ReservedSpace.Left + Padding.X;
                    it.Value.availableSpace.Top = offset + ReservedSpace.Top + Padding.Y;
                    offset += it.Value.availableSpace.Height;
                }
                if (Orientation == Mode.HORIZONTAL)
                {
                    it.Value.availableSpace.Top = ReservedSpace.Top + Padding.Y;
                    it.Value.availableSpace.Left = offset + ReservedSpace.Left + Padding.X;
                    offset += it.Value.availableSpace.Width;
                }
                it.Value.Widget.Update(it.Value.availableSpace);
            }
        }

        public void Add(Widget widget, float weight = 1)
        {
            if (weight <= 0)
                throw new Exception("Le poid doit être supérieur à 0.");
            for (var it = Widgets.First; it != null;it = it.Next)
            {
                if (it.Value.Widget == widget)
                    throw new Exception("Le widget existe déjà dans le conteneur.");
            }
            Widgets.Add(new Pair() { Widget = widget, Weight = weight });
            widget.Parent = Parent;
        }

        public void Insert(Widget widget, Widget At, float weight = 1)
        {
            if (weight <= 0)
                throw new Exception("Le poid doit être supérieur à 0.");
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
            Widgets.Insert(iterator, new Pair() { Widget = widget, Weight = weight });
            widget.Parent = Parent;
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

        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.Draw(target, decal);
            }
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f();
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (Orientation == Mode.VERTICAL)
                {
                    result.X = Math.Max(result.X, it.Value.Widget.GetMinimumSize().X);
                    result.Y += it.Value.Widget.GetMinimumSize().Y;
                }
                if (Orientation == Mode.HORIZONTAL)
                {
                    result.Y = Math.Max(result.Y, it.Value.Widget.GetMinimumSize().Y);
                    result.X += it.Value.Widget.GetMinimumSize().X;
                }
            }
            result += Padding * 2;
            return result;
        }

        internal override void DrawUpper(RenderTarget target, Vector2f decal)
        {
            base.DrawUpper(target, decal);
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.DrawUpper(target, decal);
            }
        }

        internal override void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonDownCall(button, pos, intercept);
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.MouseButtonDownCall(button, pos, intercept);
            }
        }

        internal override void MouseButtonUpCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonUpCall(button, pos, intercept);
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.MouseButtonUpCall(button, pos, intercept);
            }
        }

        internal override void MouseScrolledCall(int delta)
        {
            base.MouseScrolledCall(delta);
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.MouseScrolledCall(delta);
            }
        }

        internal override void MouseMovedCall(Vector2f pos, bool intercept)
        {
            base.MouseMovedCall(pos, intercept);
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.MouseMovedCall(pos, intercept);
            }
        }

        internal override void TextEnteredCall(string code)
        {
            base.TextEnteredCall(code);
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.TextEnteredCall(code);
            }
        }

        internal override void KeyPressedCall(KeyEventArgs args)
        {
            base.KeyPressedCall(args);
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.KeyPressedCall(args);
            }
        }

        internal override void KeyReleasedCall(KeyEventArgs args)
        {
            base.KeyReleasedCall(args);
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                it.Value.Widget.KeyReleasedCall(args);
            }
        }
    }
}

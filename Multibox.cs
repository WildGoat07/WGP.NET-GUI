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
    /// The Multibox can handle multiple widget within him.
    /// </summary>
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
        private LinkedList<Pair> Widgets { get; set; }
        /// <summary>
        /// Orientation of the list of widgets.
        /// </summary>
        /// <value>HORIZONTAL or VERTICAL.</value>
        public Mode Orientation { get; set; }
        /// <summary>
        /// The Automatic weight distribution set the weight of its widgets based on their minimum size. It will override the given weight using Add().
        /// </summary>
        public bool AutomaticWeightDistribution { get; set; }

        public enum Mode
        {
            HORIZONTAL,
            VERTICAL
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Multibox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();

            Widgets = new LinkedList<Pair>();
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
                    item.availableSpace.Height = Utilities.Max(spaceLeft * Utilities.Percent(item.Weight, 0, total - WeightUsed), 0) + item.Widget.GetMinimumSize().Y;
                    spaceLeft -= Utilities.Max(spaceLeft * Utilities.Percent(item.Weight, 0, total - WeightUsed), 0);
                    WeightUsed += item.Weight;
                }
                if (Orientation == Mode.HORIZONTAL)
                {
                    item.availableSpace.Height = ReservedSpace.Height - Padding.Y * 2;
                    item.availableSpace.Width = Utilities.Max(spaceLeft * Utilities.Percent(item.Weight, 0, total - WeightUsed), 0) + item.Widget.GetMinimumSize().X;
                    spaceLeft -= Utilities.Max(spaceLeft * Utilities.Percent(item.Weight, 0, total - WeightUsed), 0);
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
        /// <summary>
        /// Adds a widget to its list.
        /// </summary>
        /// <param name="widget">Widget to add.</param>
        /// <param name="weight">Weight of the widget. Must be superior than 0. The weight is an index to how will be calculated the reserved space of the widget if the size of the box is bigger than the minimum size of its widgets.</param>
        public void Add(Widget widget, float weight = 1)
        {
            if (weight <= 0)
                throw new Exception("The weight must be superior than 0.");
            for (var it = Widgets.First; it != null;it = it.Next)
            {
                if (it.Value.Widget == widget)
                    throw new Exception("The widget already exists in the container.");
            }
            Widgets.Add(new Pair() { Widget = widget, Weight = weight });
            widget.Parent = Parent;
        }
        /// <summary>
        /// Adds a widget to its list.
        /// </summary>
        /// <param name="widget">Widget to add.</param>
        /// <param name="At">Where to insert the widget.</param>
        /// <param name="weight">Weight of the widget. Must be superior than 0. The weight is an index to how will be calculated the reserved space of the widget if the size of the box is bigger than the minimum size of its widgets.</param>
        public void Insert(Widget widget, Widget At, float weight = 1)
        {
            if (weight <= 0)
                throw new Exception("The weight must be superior than 0.");
            LinkedList<Pair>.Element<Pair> iterator = null;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (it.Value.Widget == widget)
                    throw new Exception("The widget already exists in the container.");
                if (it.Value.Widget == At)
                    iterator = it;
            }
            if (iterator == null)
                throw new Exception("The reference widget doesn't exists in the container.");
            Widgets.Insert(iterator, new Pair() { Widget = widget, Weight = weight });
            widget.Parent = Parent;
        }
        /// <summary>
        /// Removes a widget from its list.
        /// </summary>
        /// <param name="At">The widget to remove.</param>
        public void Remove(Widget At)
        {
            LinkedList<Pair>.Element<Pair> iterator = null;
            for (var it = Widgets.First; it != null; it = it.Next)
            {
                if (it.Value.Widget == At)
                    iterator = it;
            }
            if (iterator == null)
                throw new Exception("The reference widget doesn't exists in the container.");
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
                    result.X = Utilities.Max(result.X, it.Value.Widget.GetMinimumSize().X);
                    result.Y += it.Value.Widget.GetMinimumSize().Y;
                }
                if (Orientation == Mode.HORIZONTAL)
                {
                    result.Y = Utilities.Max(result.Y, it.Value.Widget.GetMinimumSize().Y);
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

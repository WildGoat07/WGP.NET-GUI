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
    /// Basic checkbox.
    /// </summary>
    public class Checkbox : Widget
    {
        public enum State
        {
            UNCHECKED,
            CHECKED,
            INDETERMINATE
        }

        public class CheckboxStateEventArgs : EventArgs
        {
            public State State { get; set; }
        }

        public event EventHandler<CheckboxStateEventArgs> StateChanged;

        private Text Label { get; set; }
        private RectangleShape Back { get; set; }
        private RectangleShape Front { get; set; }
        private Vertex[] Border { get; set; }
        private bool _checked;
        /// <summary>
        /// Returns true if checked.
        /// </summary>
        public bool Checked
        {
            get => _checked;
            set
            {
                if (value)
                    CheckState = State.CHECKED;
                else
                    CheckState = State.UNCHECKED;
            }
        }
        private State checkState;
        /// <summary>
        /// Returns the current check state.
        /// </summary>
        public State CheckState
        {
            get => checkState;
            set
            {
                State tmp = checkState;
                checkState = value;
                if (checkState == State.CHECKED || checkState == State.INDETERMINATE)
                    _checked = true;
                else
                    _checked = false;
                if (StateChanged != null && tmp != value)
                    StateChanged(this, new CheckboxStateEventArgs() { State = CheckState });
            }
        }
        /// <summary>
        /// Its title.
        /// </summary>
        public string Title
        {
            get => Label.String;
            set => Label.String = value;
        }
        /// <summary>
        /// Constructor.
        /// </summary>
        public Checkbox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            Label = new Text();
            Back = new RectangleShape(new Vector2f(Init.TextSize, Init.TextSize));
            Front = new RectangleShape(new Vector2f(Init.TextSize, Init.TextSize));
            Front.FillColor = Init.ControlDark;
            Border = new Vertex[8];

            Label.Font = Init.Font;
            Label.CharSize = Init.TextSize;
            Label.Color = Init.TextDark;
            Checked = false;

            MouseClick += OnClick;

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Back, new RenderStates(tr.Transform));
            target.Draw(Border, PrimitiveType.Lines, new RenderStates(tr.Transform));
            target.Draw(Label, new RenderStates(tr.Transform));
            if (Checked && !Pressed)
                target.Draw(Front, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            Vector2f result = new Vector2f(Label.FindCharacterPos((uint)Label.String.Count()).X, Init.TextSize);
            result += Padding * 2;
            result += new Vector2f(Init.TextSize + 5, 0);
            return result;
        }
        protected override FloatRect GetHitbox()
        {
            return new FloatRect(Padding.X + ReservedSpace.Left, Padding.Y + ReservedSpace.Top, Init.TextSize, Init.TextSize);
        }

        protected override void InternUpdate()
        {
            pattern = new FloatRect(Pattern.Left, Pattern.Top, 0, 0);
            Back.Position = new Vector2f((int)ReservedSpace.Left, (int)ReservedSpace.Top) + Padding;
            Front.Position = Back.Position;
            Border[0].Position = Back.Position + new Vector2f(.5f, .5f);
            Border[1].Position = Border[0].Position + new Vector2f(Init.TextSize, 0);
            Border[2].Position = Border[1].Position;
            Border[3].Position = Border[2].Position + new Vector2f(0, Init.TextSize);
            Border[4].Position = Border[3].Position;
            Border[5].Position = Border[4].Position + new Vector2f(-Init.TextSize, 0);
            Border[6].Position = Border[5].Position;
            Border[7].Position = Border[6].Position + new Vector2f(0, -Init.TextSize);
            Label.Position = new Vector2f((int)(Init.TextSize + 5 + ReservedSpace.Left),
                                          (int)(ReservedSpace.Top) + Init.TextSize) + Padding;

            if (Pressed)
            {
                Back.FillColor = Init.ControlDark;
                for (int i = 0; i < 8; i++)
                    Border[i].Color = Init.BorderDark;
            }
            else if (MouseOnWidget)
            {
                Back.FillColor = Init.ControlMedium;
                for (int i = 0; i < 8; i++)
                    Border[i].Color = Init.BorderMedium;
            }
            else
            {
                Back.FillColor = Init.ControlLight;
                for (int i = 0; i < 8; i++)
                    Border[i].Color = Init.BorderLight;
            }
            if (checkState == State.CHECKED)
                Front.Texture = Init.CheckedBoxTexture;
            if (checkState == State.INDETERMINATE)
                Front.Texture = Init.IndeterminateBoxTexture;
        }
        private void OnClick(object sender, EventArgs e)
        {
            Checked = !Checked;
        }
    }
}

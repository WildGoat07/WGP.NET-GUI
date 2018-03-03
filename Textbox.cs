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
    public class Textbox : Widget
    {
        private Text DisplayText { get; set; }
        private Text DefaultTextBuffer { get; set; }
        private Text TextBuffer { get; set; }
        public string DefaultString { get; set; }
        private bool DrawCursor { get; set; }
        private Clock CursorTimer { get; set; }
        private string str;
        public string String
        {
            get => str;
            set
            {
                str = value;
                if (TextChanged != null)
                    TextChanged(this, new EventArgs());
            }
        }
        private RectangleShape Back { get; set; }
        private Vertex[] Border { get; set; }
        private Vertex[] Cursor { get; set; }
        private int CursPos { get; set; }
        private bool focused;
        public bool Focused
        {
            get => focused;
            set
            {
                focused = value;
                if (focused && FocusGained != null)
                    FocusGained(this, new EventArgs());
                if (!focused && FocusLost != null)
                    FocusLost(this, new EventArgs());
            }
        }

        public event EventHandler TextChanged;
        public event EventHandler Returned;
        public event EventHandler FocusGained;
        public event EventHandler FocusLost;

        public Textbox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            TextBuffer = new Text();
            DefaultTextBuffer = new Text();
            Back = new RectangleShape();
            CursorTimer = new Clock();
            Back.FillColor = Init.LightX;
            Border = new Vertex[5];
            Cursor = new Vertex[2];

            DrawCursor = true;

            String = "";
            DefaultString = "";
            for (int i = 0; i < 5; i++)
                Border[i].Color = Init.BorderMedium;
            for (int i = 0; i < 2; i++)
                Cursor[i].Color = Init.DarkX;


            DefaultTextBuffer.Font = Init.Font;
            DefaultTextBuffer.CharacterSize = Init.TextSize;
            DefaultTextBuffer.Color = Init.TextMedium;
            DefaultTextBuffer.Style = Text.Styles.Italic;

            TextBuffer.Font = Init.Font;
            TextBuffer.CharacterSize = Init.TextSize;
            TextBuffer.Color = Init.DarkX;
            TextBuffer.Style = Text.Styles.Regular;
            DisplayText = TextBuffer;

            MouseClick += OnClick;

            InternUpdate();
        }



        internal override void Draw(RenderTarget target, Vector2f decal)
        {
            Transformable tr = new Transformable();
            tr.Position = decal;
            target.Draw(Back, new RenderStates(tr.Transform));
            target.Draw(Border, PrimitiveType.LinesStrip, new RenderStates(tr.Transform));
            target.Draw(DisplayText, new RenderStates(tr.Transform));
            if (Focused && DrawCursor)
                target.Draw(Cursor, PrimitiveType.LinesStrip, new RenderStates(tr.Transform));
        }

        internal override Vector2f GetMinimumSize()
        {
            float minW;
            minW = Math.Max(TextBuffer.GetGlobalBounds().Width, DefaultTextBuffer.GetGlobalBounds().Width);
            Vector2f result = new Vector2f(minW + 10, Init.TextSize + 10);
            result += Padding * 2;
            return result;
        }

        protected override void InternUpdate()
        {
            pattern = new FloatRect(Pattern.Left, Pattern.Top, Pattern.Width, 0);
            Back.Size = new Vector2f((int)ReservedSpace.Width, (int)ReservedSpace.Height) - Padding * 2;
            Back.Position = new Vector2f((int)ReservedSpace.Left, (int)ReservedSpace.Top) + Padding;
            Border[0].Position = Back.Position + new Vector2f(.5f, .5f);
            Border[1].Position = Border[0].Position + new Vector2f(Back.Size.X, 0);
            Border[2].Position = Border[1].Position + new Vector2f(0, Back.Size.Y);
            Border[3].Position = Border[2].Position + new Vector2f(-Back.Size.X, 0);
            Border[4].Position = Border[0].Position;
            DisplayText.Position = new Vector2f((int)(5 + ReservedSpace.Left),
                                          (int)(Back.Size.Y - 5 - Init.TextSize + ReservedSpace.Top)) + Padding;
            Cursor[0].Position = TextBuffer.FindCharacterPos((uint)CursPos) + DisplayText.Position;
            Cursor[0].Position.X = (int)Cursor[0].Position.X + .5f;
            Cursor[0].Position.Y -= 1;
            Cursor[1].Position = Cursor[0].Position + new Vector2f(0, Init.TextSize + 4);

            DefaultTextBuffer.DisplayedString = DefaultString;
            TextBuffer.DisplayedString = String;

            if (CursorTimer.ElapsedTime > Time.FromMilliseconds(500))
            {
                CursorTimer.Restart();
                DrawCursor = !DrawCursor;
            }

            if (String.Count() == 0)
                DisplayText = DefaultTextBuffer;
            else
                DisplayText = TextBuffer;

        }

        private void OnClick(object sender, MouseButtonEventArgs e)
        {
            Focused = true;
            for (int i = 0; i <= String.Count(); i++)
            {
                if (TextBuffer.InverseTransform.TransformPoint(e.Position).X < (TextBuffer.FindCharacterPos((uint)Math.Max(i - 1, 0)).X + TextBuffer.FindCharacterPos((uint)i).X) / 2)
                {
                    CursPos = Math.Max(i - 1, 0);
                    return;
                }
            }
            CursPos = String.Count();
        }

        internal override void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonDownCall(button, pos, intercept);
            if (!GetHitbox().Contains(pos) || intercept)
                Focused = false;
        }

        internal override void TextEnteredCall(string code)
        {
            base.TextEnteredCall(code);

            if (Focused)
            {
                //caractères interdits
                if (!code.Contains("\n") && //Return
                    !code.Contains(new string(new char[] { (char)8 }))  //Backspace
                    )
                {
                    String = String.Insert(CursPos, code);
                    CursPos += code.Count();
                    //Console.WriteLine((int)code.First());
                }
            }
        }

        internal override void KeyPressedCall(KeyEventArgs args)
        {
            base.KeyPressedCall(args);
            if (Focused)
            {
                DrawCursor = true;
                CursorTimer.Restart();
                if (args.Code == Keyboard.Key.Return && args.Alt == false && args.Control == false && args.Shift == false && args.System == false)
                {
                    Focused = false;
                    if (Returned != null)
                        Returned(this, new EventArgs());
                }
                else if (args.Code == Keyboard.Key.BackSpace && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos > 0)
                {
                    String = String.Remove(CursPos - 1, 1);
                    CursPos--;
                }
                else if (args.Code == Keyboard.Key.Delete && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos < String.Count())
                {
                    String = String.Remove(CursPos, 1);
                }
                else if (args.Code == Keyboard.Key.Left && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos > 0)
                {
                    CursPos--;
                }
                else if (args.Code == Keyboard.Key.Right && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos < String.Count())
                {
                    CursPos++;
                }
            }
        }
    }
}

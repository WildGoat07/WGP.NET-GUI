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
using System.Text.RegularExpressions;

namespace WGP.Gui
{
    /// <summary>
    /// Basic text input widget.
    /// </summary>
    public class Textbox : Widget
    {
        private TEXT.Text DisplayText { get; set; }
        private TEXT.Text DefaultTextBuffer { get; set; }
        private TEXT.Text TextBuffer { get; set; }
        /// <summary>
        /// Its default string. The default string will be displayed if no text is entered.
        /// </summary>
        public string DefaultString { get; set; }
        /// <summary>
        /// List of the available chars. null for any char available.
        /// </summary>
        public char[] AvailableChars { get; set; }
        /// <summary>
        /// List of int number available chars.
        /// </summary>
        public static char[] NumericIntChars => new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-' };
        /// <summary>
        /// List of float number available chars. (same as int number, but with the dot)
        /// </summary>
        public static char[] NumericFloatChars => new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', '-' };
        /// <summary>
        /// Changes the chars of the string to a visual star (*).
        /// </summary>
        public bool HideString { get; set; }
        /// <summary>
        /// Sets a list of recommendations used when typing in the textbox.
        /// </summary>
        public IEnumerable<string> Recommendations { get; set; }
        private bool AcceptsRec { get; set; }
        private bool DrawCursor { get; set; }
        private Clock CursorTimer { get; set; }
        private string str;
        /// <summary>
        /// Entered text.
        /// </summary>
        public string String
        {
            get => str;
            set
            {
                str = value;
                if (CursPos > str.Length)
                {
                    CursPos = str.Length;
                    SecCursPos = str.Length;
                }
                if (TextChanged != null)
                    TextChanged(this, new EventArgs());
            }
        }
        private RectangleShape Back { get; set; }
        private RectangleShape Highlight { get; set; }
        private Vertex[] Border { get; set; }
        private Vertex[] Cursor { get; set; }
        private int CursPos { get; set; }
        private int SecCursPos { get; set; }
        private bool selecting;
        private bool focused;
        /// <summary>
        /// The text box is focused if the user can write inside it. Be careful when setting it to true if another text box is focused, both will react to the keyboard.
        /// </summary>
        public bool Focused
        {
            get => focused;
            set
            {
                bool oldFocus = focused;
                focused = value;
                if (focused && FocusGained != null && oldFocus != focused)
                    FocusGained(this, new EventArgs());
                if (!focused && FocusLost != null && oldFocus != focused)
                    FocusLost(this, new EventArgs());
            }
        }

        public event EventHandler TextChanged;
        public event EventHandler Returned;
        public event EventHandler FocusGained;
        public event EventHandler FocusLost;
        /// <summary>
        /// Constructor.
        /// </summary>
        public Textbox() : base()
        {
            if (!Init.IsInitialized)
                throw new Init.NotInitializedException();
            TextBuffer = new TEXT.Text();
            DefaultTextBuffer = new TEXT.Text();
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
            DefaultTextBuffer.Color = Init.TextMedium;
            DefaultTextBuffer.Style = SFML.Graphics.Text.Styles.Italic;

            TextBuffer.Font = Init.Font;
            TextBuffer.Color = Init.DarkX;
            TextBuffer.Style = SFML.Graphics.Text.Styles.Regular;
            DisplayText = TextBuffer;

            Highlight = new RectangleShape() { FillColor = Color.White };
            selecting = false;
            HideString = false;
            Recommendations = null;
            AcceptsRec = false;
            focused = false;

            InternUpdate();
        }
        /// <summary>
        /// Highlight the whole text.
        /// </summary>
        public void SelectAll()
        {
            SecCursPos = 0;
            CursPos = String.Length;
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
            if (CursPos != SecCursPos)
            {
                RenderStates states = new RenderStates(tr.Transform) { BlendMode = new BlendMode(BlendMode.Factor.OneMinusDstColor, BlendMode.Factor.OneMinusSrcColor, BlendMode.Equation.Add, BlendMode.Factor.One, BlendMode.Factor.One, BlendMode.Equation.Add) };
                target.Draw(Highlight, states);
            }
        }

        internal override Vector2f GetMinimumSize()
        {
            float minW;
            minW = Utilities.Max(TextBuffer.FindCharacterPos((uint)TextBuffer.String.Count()).X, DefaultTextBuffer.FindCharacterPos((uint)DefaultTextBuffer.String.Count()).X);
            Vector2f result = new Vector2f(minW + 10, Init.TextSize + 10);
            result += Padding * 2;
            return result;
        }

        protected override void InternUpdate()
        {
            if (AcceptsRec && Recommendations != null && String.Length > 0 && !HideString)
            {
                string regexStr = "^" + String;
                for (int i = 1; i < regexStr.Length; i++)
                {
                    char curr = regexStr[i];
                    if (curr == '\\' ||
                        curr == '?' ||
                        curr == '+' ||
                        curr == '*' ||
                        curr == '|' ||
                        curr == '[' ||
                        curr == ']' ||
                        curr == '^' ||
                        curr == '(' ||
                        curr == ')' ||
                        curr == '{' ||
                        curr == '}' ||
                        curr == '$' ||
                        curr == '.')
                    {
                        regexStr = regexStr.Insert(i, "\\");
                        i++;
                    }
                }
                Regex eng = new Regex(regexStr);
                foreach (var item in Recommendations)
                {
                    if (eng.IsMatch(item))
                    {
                        String = new string(item.ToArray());
                        SecCursPos = String.Length;
                        break;
                    }
                }
            }

            if (!HideString)
                TextBuffer.String = String;
            else
            {
                TextBuffer.String = "";
                foreach (var item in String)
                {
                    TextBuffer.String += '*';
                }
            }

            if (String.Count() == 0)
                DisplayText = DefaultTextBuffer;
            else
                DisplayText = TextBuffer;

            pattern = new FloatRect(Pattern.Left, Pattern.Top, Pattern.Width, 0);
            Back.Size = new Vector2f((int)ReservedSpace.Width, (int)ReservedSpace.Height) - Padding * 2;
            Back.Position = new Vector2f((int)ReservedSpace.Left, (int)ReservedSpace.Top) + Padding;
            Border[0].Position = Back.Position + new Vector2f(.5f, .5f);
            Border[1].Position = Border[0].Position + new Vector2f(Back.Size.X, 0);
            Border[2].Position = Border[1].Position + new Vector2f(0, Back.Size.Y);
            Border[3].Position = Border[2].Position + new Vector2f(-Back.Size.X, 0);
            Border[4].Position = Border[0].Position;
            DisplayText.Position = new Vector2f((int)(5 + ReservedSpace.Left),
                                          (int)(Back.Size.Y - 5 + ReservedSpace.Top)) + Padding;
            Cursor[0].Position = TextBuffer.FindCharacterPos((uint)CursPos) + DisplayText.Position - new Vector2f(0, Init.TextSize);
            Cursor[0].Position.X = (int)Cursor[0].Position.X + .5f;
            Cursor[0].Position.Y -= 1;
            Cursor[1].Position = Cursor[0].Position + new Vector2f(0, Init.TextSize + 4);

            if (CursPos != SecCursPos)
            {
                Highlight.Position = new Vector2f(DisplayText.Position.X, Cursor[0].Position.Y - 1) + DisplayText.FindCharacterPos((uint)Utilities.Min(CursPos, SecCursPos));
                Highlight.Size = new Vector2f(DisplayText.FindCharacterPos((uint)Utilities.Max(CursPos, SecCursPos)).X - DisplayText.FindCharacterPos((uint)Utilities.Min(CursPos, SecCursPos)).X + 1, Cursor[1].Position.Y - Cursor[0].Position.Y);
            }

            DefaultTextBuffer.String = DefaultString;

            if (CursorTimer.ElapsedTime > Time.FromMilliseconds(500))
            {
                CursorTimer.Restart();
                DrawCursor = !DrawCursor;
            }
        }

        internal override void MouseButtonUpCall(Mouse.Button button, Vector2f pos, bool intercept = false)
        {
            base.MouseButtonUpCall(button, pos, intercept);
            selecting = false;
        }

        internal override void MouseMovedCall(Vector2f pos, bool intercept)
        {
            base.MouseMovedCall(pos, intercept);
            if (!intercept && selecting)
            {
                for (int i = 0; i <= String.Count(); i++)
                {
                    if (TextBuffer.InverseTransform.TransformPoint(pos).X < (TextBuffer.FindCharacterPos((uint)Utilities.Max(i - 1, 0)).X + TextBuffer.FindCharacterPos((uint)i).X) / 2)
                    {
                        CursPos = Utilities.Max(i - 1, 0);
                        return;
                    }
                }
                CursPos = String.Count();
            }
        }

        internal override void MouseButtonDownCall(Mouse.Button button, Vector2f pos, bool intercept)
        {
            base.MouseButtonDownCall(button, pos, intercept);
            AcceptsRec = false;
            if (!GetHitbox().Contains(pos) || intercept)
            {
                Focused = false;
                SecCursPos = CursPos;
            }
            if (GetHitbox().Contains(pos) && !intercept)
            {
                CursorTimer.Restart();
                Focused = true;
                selecting = true;
                for (int i = 0; i <= String.Count(); i++)
                {
                    if (TextBuffer.InverseTransform.TransformPoint(pos).X < (TextBuffer.FindCharacterPos((uint)Utilities.Max(i - 1, 0)).X + TextBuffer.FindCharacterPos((uint)i).X) / 2)
                    {
                        SecCursPos = Utilities.Max(i - 1, 0);
                        CursPos = SecCursPos;
                        return;
                    }
                }
                SecCursPos = String.Count();
                CursPos = SecCursPos;
            }
        }

        internal override void TextEnteredCall(string code)
        {
            base.TextEnteredCall(code);

            if (Focused)
            {
                bool add = false;
                if (AvailableChars == null)
                {
                    add = true;
                }
                else
                {
                    foreach (var item in code)
                    {
                        if (AvailableChars.Contains(item))
                            add = true;
                    }
                }

                //caractères interdits
                if (code.Contains('\n') //Return
                    || code.Contains((char)8)  //Backspace
                    || code.Contains((char)1)  //Ctrl + A
                    || code.Contains((char)127)  //Ctrl + Backspace
                    || code.Contains((char)24)  //Ctrl + X
                    || code.Contains((char)3)  //Ctrl + C
                    || code.Contains((char)22)  //Ctrl + V
                    )
                    add = false;
                if (add)
                {
                    CursorTimer.Restart();
                    String = String.Remove(Utilities.Min(CursPos, SecCursPos), Math.Abs(CursPos - SecCursPos));
                    CursPos = Utilities.Min(CursPos, SecCursPos);
                    String = String.Insert(CursPos, code);
                    CursPos += code.Count();
                    SecCursPos = CursPos;
                    if (CursPos == String.Length)
                        AcceptsRec = true;
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
                    AcceptsRec = false;
                    Focused = false;
                    if (Returned != null)
                        Returned(this, new EventArgs());
                }
                else if (args.Code == Keyboard.Key.BackSpace && args.Alt == false && args.Control == true && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos > 0)
                {
                    if (AcceptsRec)
                    {
                        String = "";
                        CursPos = 0;
                        SecCursPos = 0;
                    }
                    else
                    {
                        SecCursPos = CursPos;
                        bool end = false;
                        bool textEncountered = false;
                        while (!end)
                        {
                            if (String[CursPos - 1] != ' ')
                                textEncountered = true;
                            if (String[CursPos - 1] == ' ' && textEncountered)
                                end = true;
                            if (!end)
                                CursPos--;
                            if (CursPos == 0)
                                end = true;
                        }
                        String = String.Remove(CursPos, SecCursPos - CursPos);
                        SecCursPos = CursPos;
                        AcceptsRec = false;
                    }
                }
                else if (args.Code == Keyboard.Key.Delete && args.Alt == false && args.Control == true && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos < String.Count())
                {
                    SecCursPos = CursPos;
                    bool end = false;
                    bool spaceEncountered = false;
                    while (!end)
                    {
                        if (String[CursPos] == ' ')
                            spaceEncountered = true;
                        if (String[CursPos] != ' ' && spaceEncountered)
                            end = true;
                        if (!end)
                            CursPos++;
                        if (CursPos == String.Count())
                            end = true;
                    }
                    String = String.Remove(SecCursPos, CursPos - SecCursPos);
                    CursPos = SecCursPos;
                    AcceptsRec = false;
                }
                else if (args.Code == Keyboard.Key.BackSpace && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && (CursPos > 0 || CursPos != SecCursPos))
                {
                    if (CursPos != SecCursPos)
                    {
                        String = String.Remove(Utilities.Min(CursPos, SecCursPos), Math.Abs(CursPos - SecCursPos));
                        CursPos = Utilities.Min(CursPos, SecCursPos);
                        if (AcceptsRec)
                        {
                            String = String.Remove(CursPos - 1, 1);
                            CursPos--;
                        }
                    }
                    else
                    {
                        String = String.Remove(CursPos - 1, 1);
                        CursPos--;
                    }
                    SecCursPos = CursPos;
                }
                else if (args.Code == Keyboard.Key.Delete && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && (CursPos < String.Count() || CursPos != SecCursPos))
                {
                    if (CursPos != SecCursPos)
                    {
                        AcceptsRec = false;
                        String = String.Remove(Utilities.Min(CursPos, SecCursPos), Math.Abs(CursPos - SecCursPos));
                        CursPos = Utilities.Min(CursPos, SecCursPos);
                    }
                    else
                        String = String.Remove(CursPos, 1);
                    SecCursPos = CursPos;
                    AcceptsRec = false;
                }
                else if (args.Code == Keyboard.Key.Left && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos > 0)
                {
                    if (CursPos != SecCursPos)
                        CursPos = Utilities.Min(CursPos, SecCursPos);
                    else
                        CursPos--;
                    SecCursPos = CursPos;
                    AcceptsRec = false;
                }
                else if (args.Code == Keyboard.Key.Right && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos < String.Count())
                {
                    if (CursPos != SecCursPos)
                        CursPos = Utilities.Max(CursPos, SecCursPos);
                    else
                        CursPos++;
                    SecCursPos = CursPos;
                }
                else if (args.Code == Keyboard.Key.Up && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused)
                {
                    CursPos = 0;
                    SecCursPos = CursPos;
                    AcceptsRec = false;
                }
                else if (args.Code == Keyboard.Key.Down && args.Alt == false && args.Control == false && args.Shift == false && args.System == false && Focused)
                {
                    CursPos = String.Length;
                    SecCursPos = CursPos;
                }
                else if (args.Code == Keyboard.Key.Left && args.Alt == false && args.Control == true && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos > 0)
                {
                    CursPos--;
                    bool end = CursPos == 0;
                    bool textEncountered = false;
                    while (!end)
                    {
                        if (String[CursPos - 1] != ' ')
                            textEncountered = true;
                        if (String[CursPos - 1] == ' ' && textEncountered)
                            end = true;
                        if (!end)
                            CursPos--;
                        if (CursPos == 0)
                            end = true;
                    }
                    SecCursPos = CursPos;
                    AcceptsRec = false;
                }
                else if (args.Code == Keyboard.Key.Right && args.Alt == false && args.Control == true && args.Shift == false && args.System == false && Focused
                    && String.Count() > 0 && CursPos < String.Count())
                {
                    CursPos++;
                    bool end = CursPos == String.Count();
                    bool spaceEncountered = false;
                    while (!end)
                    {
                        if (String[CursPos] == ' ')
                            spaceEncountered = true;
                        if (String[CursPos] != ' ' && spaceEncountered)
                            end = true;
                        if (!end)
                            CursPos++;
                        if (CursPos == String.Count())
                            end = true;
                    }
                    SecCursPos = CursPos;
                }
                else if (args.Code == Keyboard.Key.Left && args.Alt == false && args.Control == true && args.Shift == true && args.System == false && Focused
                    && String.Count() > 0 && CursPos > 0)
                {
                    CursPos--;
                    bool end = CursPos == 0;
                    bool textEncountered = false;
                    while (!end)
                    {
                        if (String[CursPos - 1] != ' ')
                            textEncountered = true;
                        if (String[CursPos - 1] == ' ' && textEncountered)
                            end = true;
                        if (!end)
                            CursPos--;
                        if (CursPos == 0)
                            end = true;
                    }
                    AcceptsRec = false;
                }
                else if (args.Code == Keyboard.Key.Right && args.Alt == false && args.Control == true && args.Shift == true && args.System == false && Focused
                    && String.Count() > 0 && CursPos < String.Count())
                {
                    CursPos++;
                    bool end = CursPos == String.Count();
                    bool spaceEncountered = false;
                    while (!end)
                    {
                        if (String[CursPos] == ' ')
                            spaceEncountered = true;
                        if (String[CursPos] != ' ' && spaceEncountered)
                            end = true;
                        if (!end)
                            CursPos++;
                        if (CursPos == String.Count())
                            end = true;
                    }
                }
                else if (args.Code == Keyboard.Key.Left && args.Alt == false && args.Control == false && args.Shift == true && args.System == false && Focused
                    && String.Count() > 0 && CursPos > 0)
                {
                    CursPos--;
                    AcceptsRec = false;
                }
                else if (args.Code == Keyboard.Key.Right && args.Alt == false && args.Control == false && args.Shift == true && args.System == false && Focused
                    && String.Count() > 0 && CursPos < String.Count())
                {
                    CursPos++;
                }
                else if (args.Code == Keyboard.Key.A && args.Alt == false && args.Control == true && args.Shift == false && args.System == false && Focused)
                {
                    CursPos = String.Count();
                    SecCursPos = 0;
                    AcceptsRec = false;
                }
                else if (args.Code == Keyboard.Key.X && args.Alt == false && args.Control == true && args.Shift == false && args.System == false && Focused
                    && !HideString)
                {
                    if (CursPos != SecCursPos)
                    {
                        string tmp = String.Substring(Utilities.Min(SecCursPos, CursPos), Math.Abs(SecCursPos - CursPos));
                        String = String.Remove(Utilities.Min(CursPos, SecCursPos), Math.Abs(CursPos - SecCursPos));
                        CursPos = Utilities.Min(CursPos, SecCursPos);
                        SecCursPos = CursPos;
                        System.Windows.Clipboard.SetText(tmp);
                    }
                }
                else if (args.Code == Keyboard.Key.C && args.Alt == false && args.Control == true && args.Shift == false && args.System == false && Focused
                    && !HideString)
                {
                    if (CursPos != SecCursPos)
                    {
                        string tmp = String.Substring(Utilities.Min(SecCursPos, CursPos), Math.Abs(SecCursPos - CursPos));
                        System.Windows.Clipboard.SetText(tmp);
                    }
                }
                else if (args.Code == Keyboard.Key.V && args.Alt == false && args.Control == true && args.Shift == false && args.System == false && Focused)
                {
                    if (System.Windows.Clipboard.ContainsText(System.Windows.TextDataFormat.Text))
                    {
                        string code = System.Windows.Clipboard.GetText(System.Windows.TextDataFormat.Text);
                        foreach (var item in code)
                        {
                            TextEnteredCall(new string(item, 1));
                        }
                    }
                }
            }
        }
    }
}

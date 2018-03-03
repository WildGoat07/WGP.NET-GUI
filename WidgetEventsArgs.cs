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
    public class MouseButtonEventArgs : EventArgs
    {
        public Mouse.Button Button { get; private set; }
        public Vector2f Position { get; private set; }
        public MouseButtonEventArgs(Mouse.Button button, Vector2f position)
        {
            Button = button;
            Position = position;
        }
    }
    public class MouseMovedEventArgs : EventArgs
    {
        public Vector2f Position { get; private set; }
        public MouseMovedEventArgs(Vector2f pos)
        {
            Position = pos;
        }
    }
    public class PaddingChangedEventArgs : EventArgs
    {
        public Vector2f NewPadding { get; private set; }
        public Vector2f OldPadding { get; private set; }
        public PaddingChangedEventArgs(Vector2f oldP, Vector2f newP)
        {
            NewPadding = newP;
            OldPadding = oldP;
        }
    }
    public class PatternChangedEventArgs : EventArgs
    {
        public FloatRect NewPattern { get; private set; }
        public FloatRect OldPattern { get; private set; }
        public PatternChangedEventArgs(FloatRect oldP, FloatRect newP)
        {
            NewPattern = newP;
            OldPattern = oldP;
        }
    }
}

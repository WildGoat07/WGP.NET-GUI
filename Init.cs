using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace WGP.Gui
{
    public static class Init
    {
        public class NotInitializedException : Exception
        {
            public NotInitializedException() : base("L'interface n'a pas été initialisée, appelez WGP.Gui.Init.InitializeResources().") { }
        }

        internal static Font Font { get; private set; }
        internal static Color Background { get; private set; }
        internal static Color LightX { get; private set; }
        internal static Color DarkX { get; private set; }
        internal static Color ControlLight { get; private set; }
        internal static Color ControlMedium { get; private set; }
        internal static Color ControlDark { get; private set; }
        internal static Color TextLight { get; private set; }
        internal static Color TextMedium { get; private set; }
        internal static Color TextDark { get; private set; }
        internal static Color BorderLight { get; private set; }
        internal static Color BorderMedium { get; private set; }
        internal static Color BorderDark { get; private set; }
        internal static Color BackgroundBorder { get; private set; }
        internal static Color Titlebar { get; private set; }
        internal static uint TextSize { get; private set; }
        internal static Texture ResizeTexture { get; private set; }
        internal static Texture ShowTexture { get; private set; }
        internal static Texture CloseTexture { get; private set; }
        internal static Texture HideTexture { get; private set; }
        internal static Texture TitleBackTexture { get; private set; }
        internal static Texture CheckedBoxTexture { get; private set; }
        internal static Texture IndeterminateBoxTexture { get; private set; }
        internal static Texture DownArrowTexture { get; private set; }
        internal static Texture ScrollDownTexture { get; private set; }
        internal static Texture ScrollUpTexture { get; private set; }
        internal static Texture ScrollLeftTexture { get; private set; }
        internal static Texture ScrollRightTexture { get; private set; }

        internal static bool IsInitialized
        {
            get
            {
                return Font != null;
            }
        }

        public static void InitializeResources()
        {
            Font = new Font(Properties.Resources.calibri);
            Background = new Color(220, 220, 220);
            LightX = Color.White;
            DarkX = Color.Black;
            ControlLight = new Color(200, 200, 200);
            ControlMedium = new Color(160, 160, 160);
            ControlDark = new Color(70, 70, 70);
            TextLight = new Color(220, 220, 220);
            TextMedium = new Color(120, 120, 120);
            TextDark = new Color(40, 40, 40);
            BorderLight = new Color(180, 180, 180);
            BorderMedium = new Color(140, 140, 140);
            BorderDark = new Color(90, 90, 90);
            BackgroundBorder = new Color(110, 110, 110);
            Titlebar = new Color(210, 210, 210);
            TextSize = 14;
            ResizeTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.resizeIcon));
            ShowTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.showIcon));
            CloseTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.closeIcon));
            HideTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.hideIcon));
            TitleBackTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.TitlebarBack));
            TitleBackTexture.Repeated = true;
            CheckedBoxTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.checkedBox));
            IndeterminateBoxTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.indeterminateBox));
            DownArrowTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.downArrow));
            ScrollDownTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.scrollDown));
            ScrollUpTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.scrollUp));
            ScrollLeftTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.scrollLeft));
            ScrollRightTexture = new Texture(Utilities.SystemBitmapAsSFML(Properties.Resources.scrollRight));
        }
    }
}

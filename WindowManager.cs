using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;

namespace WGP.Gui
{
    /// <summary>
    /// The window manager handle multiple window related conditional elements (window focus, window events, ...)
    /// </summary>
    public class WindowManager
    {
        private List<Window> windows;
        private Window front;

        public SFML.Graphics.RenderWindow App { get; private set; }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="app"></param>
        public WindowManager(SFML.Graphics.RenderWindow app)
        {
            front = null;
            windows = new List<Window>();
            App = app;
            App.MouseButtonPressed += OnMouseDown;
        }
        /// <summary>
        /// Updates the manager AND the windows
        /// </summary>
        public void Update()
        {
            bool noTrigger = false;
            if (windows.Count > 0 && windows.First().InterceptEvents != null)
                noTrigger = true;
            foreach (var window in windows)
            {
                window.Update();
                if (window.GetBounds().Contains(App.MapPixelToCoords(Mouse.GetPosition(App), window.WindowView ?? App.GetView())))
                {
                    if (noTrigger && window != windows.First())
                        window.triggerEvents = false;
                    else
                        window.triggerEvents = true;
                    noTrigger = true;
                }
                else
                    window.triggerEvents = true;
            }
            if (front != null)
            {
                windows.Remove(front);
                windows.Insert(0, front);
            }
        }

        private void OnMouseDown(object sender, SFML.Window.MouseButtonEventArgs e)
        {
            Window selectedWindow = null;
            foreach (var window in windows)
            {
                if (window.GetBounds().Contains(App.MapPixelToCoords(new SFML.System.Vector2i(e.X, e.Y), window.WindowView ?? App.GetView())))
                {
                    selectedWindow = window;
                    break;
                }
            }
            if (selectedWindow != null)
            {
                windows.Remove(selectedWindow);
                windows.Insert(0, selectedWindow);
            }
        }
        /// <summary>
        /// Adds a window to the manager
        /// </summary>
        /// <param name="window">Window to add</param>
        /// <param name="alwaysFront">True if the window is always front (there can be only one)</param>
        public void AddWindow(Window window, bool alwaysFront = false)
        {
            window.App = App;
            windows.Insert(0, window);
            if (alwaysFront)
                front = window;
        }
        /// <summary>
        /// Draws the windows managed
        /// </summary>
        public void DrawWindows()
        {
            for(int i = windows.Count - 1;i>=0;i--)
            {
                windows[i].Draw();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Window;

namespace WGP.Gui
{
    public class WindowManager
    {
        private List<Window> windows;

        public SFML.Graphics.RenderWindow App { get; private set; }

        public WindowManager(SFML.Graphics.RenderWindow app)
        {
            windows = new List<Window>();
            App = app;
            App.MouseButtonPressed += OnMouseDown;
        }

        public void Update()
        {
            bool noTrigger = false;
            foreach (var window in windows)
            {
                window.Update();
                if (window.GetBounds().Contains(App.MapPixelToCoords(Mouse.GetPosition(App), window.WindowView ?? App.GetView())))
                {
                    if (noTrigger)
                        window.triggerEvents = false;
                    else
                        window.triggerEvents = true;
                    noTrigger = true;
                }
                else
                    window.triggerEvents = true;
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

        public void AddWindow(Window window)
        {
            window.App = App;
            windows.Insert(0, window);
        }

        public void DrawWindows()
        {
            for(int i = windows.Count - 1;i>=0;i--)
            {
                windows[i].Draw();
            }
        }
    }
}

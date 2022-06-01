using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Keystrokes.Helpers
{
    public class Drawer
    {
        public Ellipse CreateEllipse(Canvas c, int width, int height, int left, int top, Brush brush)
        {
            Ellipse e = new Ellipse();
            e.Width = width;
            e.Height = height;
            e.Fill = brush;
            Canvas.SetLeft(e, left);
            Canvas.SetTop(e, top);

            c.Children.Add(e);
            return e;
        }

        public Ellipse CreatePoint(Canvas c, int width, int height, int left, int top, Brush brush, string toolTip)
        {
            Ellipse e = new Ellipse();
            e.Width = width;
            e.Height = height;
            e.Fill = brush;
            e.ToolTip = toolTip;
            Canvas.SetLeft(e, left);
            Canvas.SetTop(e, top);

            c.Children.Add(e);
            return e;
        }

        
        public Label CreateLabel(Canvas c, int width, int height, int left, int top, Brush brush, string content)
        {
            Label e = new Label();
            e.Width = 200;
            e.Height = 100;
            e.Foreground = brush;
            e.Content = content;
            Canvas.SetLeft(e, left);
            Canvas.SetTop(e, top);

            c.Children.Add(e);
            return e;
        }

        public Line CreateLine(Canvas c, int x1, int y1, int x2, int y2, Brush brush)
        {
            Line e = new Line();
            e.X1 = x1;
            e.Y1 = y1;
            e.X2 = x2;
            e.Y2 = y2;
            e.Fill = brush;
            e.StrokeThickness = 4;
            e.SnapsToDevicePixels = true;
            e.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
            Canvas.SetZIndex(e, 999);
            Canvas.SetLeft(e, x1);
            Canvas.SetTop(e, y1);
            c.Children.Add(e);
            return e;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Keystrokes.Models
{
    public class CanvasItems
    {

        public List<Ellipse> Ellipses { get; set; } = new List<Ellipse>();

        public List<Rectangle> Rectangles { get; set; } = new List<Rectangle>();

        public List<Line> Lines { get; set; } = new List<Line>();

        public List<Label> Labels { get; set; } = new List<Label>();
    }
}

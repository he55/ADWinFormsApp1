using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ADNewUI
{
    public class Leida:Canvas
    {
        protected override void OnRender(DrawingContext dc)
        {
            double w = this.ActualWidth;
            double h = this.ActualHeight;
            dc.DrawRectangle(this.Background, null, new Rect(0, 0, w, h));

            // center point
            Point center = new Point(w / 2, h / 2);
            dc.DrawEllipse(Brushes.Red, null, center, 2, 2);

            Pen pen = new Pen(Brushes.Red, 2);
            double radius = w > h ? h / 2 : w / 2;
            dc.DrawEllipse(Brushes.Transparent, pen, center, radius, radius);
        }
    }
}

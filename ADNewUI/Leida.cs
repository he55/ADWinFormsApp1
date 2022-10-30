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
            double offsetY = 120.0;
            Point center = new Point(w / 2, h -offsetY);
            dc.DrawEllipse(Brushes.Red, null, center, 3,3);

            Pen pen = new Pen(Brushes.Red, 2);
            double maxR = Math.Sqrt(center.X * center.X + center.Y * center.Y);
            double minR = 70.0;
            double minT = 70.0;
            for (double i = minR; i < maxR; i+=minT)
            {
                dc.DrawEllipse(Brushes.Transparent, pen, center, i, i);
            }
        }
    }
}

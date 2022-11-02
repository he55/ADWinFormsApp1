using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ADNewUI
{
    public class Leida:Canvas
    {
        AnimationClock animationClock ;

        public Leida()
        {
            double w = SystemParameters.PrimaryScreenWidth;
            double h = SystemParameters.PrimaryScreenHeight;
            double r = Math.Sqrt(w * w + h * h) / 2;

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.To = r;
            doubleAnimation.Duration = TimeSpan.FromSeconds(2);
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
             animationClock = doubleAnimation.CreateClock();
        }

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

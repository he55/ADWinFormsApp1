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

            AddMyUserControl();
        }

        List<(Rect,Control)> list= new List<(Rect,Control)> ();
        void AddMyUserControl()
        {
            MyUserControl myUserControl = new MyUserControl();
            Canvas.SetLeft(myUserControl, 100);
            Canvas.SetTop(myUserControl, 100);
            this.Children.Add(myUserControl);
            list.Add((new Rect(100, 100, myUserControl.Width, myUserControl.Height), myUserControl));

            MyUserControl myUserControl2 = new MyUserControl();
            Canvas.SetLeft(myUserControl2, 400);
            Canvas.SetTop(myUserControl2, 100);
            this.Children.Add(myUserControl2);
            list.Add((new Rect(400, 100, myUserControl2.Width, myUserControl2.Height), myUserControl2));
        }

        public void SetPoint(Point point)
        {
            foreach (var item in list)
            {
                if (item.Item1.Contains(point))
                {
                    //if(item.Item2.Background!=Brushes.Red)
                    item.Item2.Background = Brushes.Red;
                }
                else
                {
                    //if (item.Item2.Background != Brushes.Transparent)
                        item.Item2.Background = Brushes.Transparent;
                }
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            double w = this.ActualWidth;
            double h = this.ActualHeight;
            Rect rectangle = new Rect(0, 0, w, h);
            dc.PushClip(new RectangleGeometry(rectangle));
            dc.DrawRectangle(Brushes.Black, null, rectangle);

            // center point
            double offsetY = 120.0;
            Point center = new Point(w / 2, h -offsetY);
            dc.DrawEllipse(Brushes.Red, null, center, 3,3);
            dc.DrawEllipse(Brushes.Transparent, new Pen(this.Background, 10),
                center, null,
                3, animationClock,
                3, animationClock);

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

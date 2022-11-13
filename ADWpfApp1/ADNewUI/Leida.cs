using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ADWpfApp1
{
    public class Leida : Canvas
    {
        AnimationClock animationClock;

        public Leida()
        {
            double w = SystemParameters.PrimaryScreenWidth;
            double h = SystemParameters.PrimaryScreenHeight;
            double r = Math.Sqrt(w * w + h * h) / 2;

            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.To = r;
            doubleAnimation.Duration = TimeSpan.FromSeconds(2);
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            //animationClock = doubleAnimation.CreateClock();
        }

        public class CanvasItem
        {
            public Rect Item1 { get; set; }
            public Control Item2 { get; set; }
            public UserInfo Item3 { get; set; }
        }

       public List<CanvasItem> canvasItems = new List<CanvasItem>();
        public void AddDevice(UserInfo userInfo)
        {
            MyUserControl myUserControl = new MyUserControl();
            myUserControl.SetUserInfo(userInfo);

            this.Children.Add(myUserControl);
            canvasItems.Add(new CanvasItem { Item2 = myUserControl, Item3 = userInfo });

            UpdateRect();
        }

        public void RemoveDevice(UserInfo userInfo)
        {
            for (int i = 0; i < canvasItems.Count; i++)
            {
                CanvasItem item = canvasItems[i];
                if (item.Item3 == userInfo)
                {
                    this.Children.Remove(item.Item2);
                    canvasItems.Remove(item);
                }
            }

            UpdateRect();
        }

        public void UpdateRect()
        {
            double left = 100.0;
            double top = 100.0;
            foreach (var item in canvasItems)
            {
                Control control = item.Item2;
                Canvas.SetLeft(control, left);
                Canvas.SetTop(control, top);
                item.Item1 = new Rect(left, top, control.Width, control.Height);
                left += 100.0;
            }
        }

        public UserInfo SelectUserInfo { get; set; }
        public void SetPoint(Point point)
        {
            UserInfo userInfo=null;
            foreach (var item in canvasItems)
            {
                if (item.Item1.Contains(point))
                {
                    //if(item.Item2.Background!=Brushes.Red)
                    item.Item2.Background = Brushes.Red;
                    userInfo = item.Item3;
                }
                else
                {
                    //if (item.Item2.Background != Brushes.Transparent)
                    item.Item2.Background = Brushes.Transparent;
                }
            }

            SelectUserInfo = userInfo;
        }

        public void ResetBackground()
        {
            foreach (var item in canvasItems)
            {
                item.Item2.Background = Brushes.Transparent;
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
            Point center = new Point(w / 2, h - offsetY);
            dc.DrawEllipse(Brushes.Red, null, center, 3, 3);
            dc.DrawEllipse(Brushes.Transparent, new Pen(this.Background, 10),
                center, null,
                3, animationClock,
                3, animationClock);

            Pen pen = new Pen(Brushes.Red, 2);
            double maxR = Math.Sqrt(center.X * center.X + center.Y * center.Y);
            double minR = 70.0;
            double minT = 70.0;
            for (double i = minR; i < maxR; i += minT)
            {
                dc.DrawEllipse(Brushes.Transparent, pen, center, i, i);
            }
        }
    }
}

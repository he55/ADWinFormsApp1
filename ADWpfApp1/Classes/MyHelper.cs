using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ADWpfApp1
{
    public static class MyHelper
    {
        // How to get Item under cursor in WPF ListView
        // https://www.py4u.net/discuss/1453642 - Answer #2
        public static object GetObjectAtPoint(ItemsControl control, Point p)
        {
            var result = VisualTreeHelper.HitTest(control, p);
            if (result == null)
                return null;

            var obj = result.VisualHit;

            while (VisualTreeHelper.GetParent(obj) != null && !(obj is ListBoxItem))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (obj == null)
                return null;

            return control.ItemContainerGenerator.ItemFromContainer(obj);
        }

        public static int GetIndexAtPoint(ItemsControl control, Point p)
        {
            var result = VisualTreeHelper.HitTest(control, p);
            if (result == null)
                return -1;

            var obj = result.VisualHit;

            while (VisualTreeHelper.GetParent(obj) != null && !(obj is ListBoxItem))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (obj == null)
                return -1;

            return control.ItemContainerGenerator.IndexFromContainer(obj);
        }
    }
}

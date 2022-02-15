using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ADWinFormsApp1
{
    public class ListBoxDragDropManager
    {
        ListBox listBox;

        public ListBoxDragDropManager(ListBox listBox)
        {
            this.listBox = listBox;
            this.listBox.DragOver += ListBox_DragOver;
            this.listBox.Drop += ListBox_Drop;
        }

        void ListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            GetIndexUnderDragCursor();
        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            if (lastListBoxItem != null)
                ListBoxItemDragState.SetIsUnderDragCursor(lastListBoxItem, false);

            DataObject data = (DataObject) e.Data;
            string v = data.GetText();
            System.Collections.Specialized.StringCollection stringCollection = data.GetFileDropList();
        }


        ListBoxItem lastListBoxItem;
        private int GetIndexUnderDragCursor()
        {
            ListBoxItem overItem = null;
            int index = -1;
            for (int i = 0; i < this.listBox.Items.Count; ++i)
            {
                ListBoxItem item = (ListBoxItem)this.listBox.ItemContainerGenerator.ContainerFromIndex(i);
                if (this.IsMouseOver(item))
                {
                    overItem = item;
                    index = i;
                    break;
                }
            }

            if (lastListBoxItem != overItem)
            {
                if (overItem != null)
                    ListBoxItemDragState.SetIsUnderDragCursor(overItem, true);

                if (lastListBoxItem != null)
                    ListBoxItemDragState.SetIsUnderDragCursor(lastListBoxItem, false);

                lastListBoxItem = overItem;
            }

            return index;
        }

        bool IsMouseOver(Visual target)
        {
            // We need to use MouseUtilities to figure out the cursor
            // coordinates because, during a drag-drop operation, the WPF
            // mechanisms for getting the coordinates behave strangely.

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = MouseUtilities.GetMousePosition(target);
            return bounds.Contains(mousePos);
        }
    }


    public static class ListBoxItemDragState
    {
        public static readonly DependencyProperty IsUnderDragCursorProperty =
            DependencyProperty.RegisterAttached(
                "IsUnderDragCursor",
                typeof(bool),
                typeof(ListBoxItemDragState),
                new UIPropertyMetadata(false));

        public static bool GetIsUnderDragCursor(ListBoxItem item)
        {
            return (bool)item.GetValue(IsUnderDragCursorProperty);
        }

        internal static void SetIsUnderDragCursor(ListBoxItem item, bool value)
        {
            item.SetValue(IsUnderDragCursorProperty, value);
        }
    }
}

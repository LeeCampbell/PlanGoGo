using System.Windows;
using System.Windows.Controls;

namespace BlueCrest.BlueScreen.Controls
{
    //Straight copy of MSDN example
    //http://msdn.microsoft.com/en-us/library/ms771523.aspx
    //Original code can be found here.
    //http://archive.msdn.microsoft.com/wpfsamples
    public class TreeListViewItem : TreeViewItem
    {
        private int _level = -1;

        static TreeListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewItem), new FrameworkPropertyMetadata(typeof(TreeListViewItem)));
        }

        #region Columns DependencyProperty

        public GridViewColumnCollection Columns
        {
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(TreeListViewItem), new UIPropertyMetadata());

        #endregion

        /// <summary>
        /// Item's hierarchy in the tree
        /// </summary>
        public int Level
        {
            get
            {
                if (_level == -1)
                {
                    var parent = ItemsControlFromItemContainer(this) as TreeListViewItem;
                    _level = (parent != null) ? parent.Level + 1 : 0;
                }
                return _level;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }
    }
}

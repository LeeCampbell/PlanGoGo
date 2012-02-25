using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BlueCrest.BlueScreen.Presentation;

namespace BlueCrest.BlueScreen.Controls
{
    public class TreeListView : TreeView
    {
        //TODO: Implement multiple Select for Drag.
        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
        }

        #region Columns DependencyProperty

        public GridViewColumnCollection Columns
        {
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Columns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(TreeListView), new UIPropertyMetadata());

        #endregion


        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }


        #region Drag drop features.

        private Point _lastMouseDown;

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left)
            {
                _lastMouseDown = e.GetPosition(this);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(this);

                // Note: This should be based on some accessibility number and not just 2 pixels
                if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 2.0) ||
                    (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 2.0))
                {
                    DataObject dragData;
                    TreeViewItem dragSource;
                    var originalSource = e.OriginalSource as DependencyObject;

                    if (originalSource.TryGetDragData(out dragSource, out dragData))
                    {
                        DragDrop.DoDragDrop(dragSource, dragData, DragDropEffects.Move);
                    }
                }
            }
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            var target = e.OriginalSource.GetDataContext() as IDragTarget;
            var dragSource = e.Data.GetData(typeof(IDraggable)) as IDraggable;
            if (target != null && dragSource != null)
            {
                //I hope I can just do it all here. (In this order?)
                // The sample stick crap in fields and shared state (vomit).
                e.Effects = DragDropEffects.Move;
                target.ReceiveDrop(dragSource);
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            CheckDropTarget(e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            CheckDropTarget(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            CheckDropTarget(e);
        }

        private static void CheckDropTarget(DragEventArgs e)
        {
            if (!IsValidDropTarget(e))
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private static bool IsValidDropTarget(DragEventArgs e)
        {
            var target = e.OriginalSource.GetDataContext() as IDragTarget;
            var dragSource = e.Data.GetData(typeof(IDraggable)) as IDraggable;
            if (target == null || dragSource == null)
            {
                return false;
            }

            return target.CanReceiveDrop(dragSource);
        }

        #endregion
    }
}
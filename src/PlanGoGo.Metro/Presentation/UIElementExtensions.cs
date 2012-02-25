using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BlueCrest.BlueScreen.Presentation
{
    public static class UIElementExtensions
    {
        public static T FindParent<T>(this DependencyObject element) where T : DependencyObject
        {
            // Walk up the element tree to the nearest T item.
            var parent = element as T;
            while ((parent == null) && (element != null))
            {
                element = VisualTreeHelper.GetParent(element);
                parent = element as T;
            }
            return parent;
        }

        //This version creates a stack to represent the Trade tree,
        // then walks the visual tree moving from Parent (trade) to child, getting each of their containers.
        //Not needed in my version as I have a ListView?
        public static ItemsControl GetContainerFromTrade(this TreeView source, IChild target)
        {
            var stack = new Stack();
            stack.Push(target);
            var parent = target.Parent;

            while (parent != null)
            {
                stack.Push(parent);
                var next = parent as IChild;
                parent = next != null ? next.Parent : null;
            }

            ItemsControl container = source;
            while ((stack.Count > 0) && (container != null))
            {
                var top = stack.Pop();
                container = (ItemsControl)container.ItemContainerGenerator.ContainerFromItem(top);
            }

            return container;
        }

        public static object GetDataContext(this object source)
        {
            if (source == null) return null;

            var frameworkElement = source as FrameworkElement;
            if (frameworkElement != null)
                return frameworkElement.DataContext;


            var frameworkContentElement = source as FrameworkContentElement;
            return frameworkContentElement != null
                       ? frameworkContentElement.DataContext
                       : null;
        }

        public static bool TryGetDragData<TParent>(this DependencyObject originalSource, out TParent dragSource, out DataObject data)
            where TParent : FrameworkElement
        {
            data = null;
            dragSource = originalSource.FindParent<TParent>();
            if (dragSource != null)
            {
                var dataContext = dragSource.DataContext as IDraggable;
                if (dataContext != null && dataContext.CanDrag())
                {
                    data = new DataObject(dataContext);
                    data.SetData(typeof(IDraggable), dataContext);
                    return true;
                }
            }
            return false;
        }
    }
}
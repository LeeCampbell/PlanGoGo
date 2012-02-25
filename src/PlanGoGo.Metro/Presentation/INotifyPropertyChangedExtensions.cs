using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ArtemisWest.Presentation
{
    public static class NotifcationExtensions
    {
        /// <summary>
        /// Gets property information for the specified <paramref name="property"/> expression.
        /// </summary>
        /// <typeparam name="TSource">Type of the parameter in the <paramref name="property"/> expression.</typeparam>
        /// <typeparam name="TValue">Type of the property's value.</typeparam>
        /// <param name="property">The expression from which to retrieve the property information.</param>
        /// <returns>Property information for the specified expression.</returns>
        /// <exception cref="ArgumentException">The expression is not understood.</exception>
        public static PropertyInfo GetPropertyInfo<TSource, TValue>(this Expression<Func<TSource, TValue>> property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            var body = property.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Expression is not a property", "property");

            var propertyInfo = body.Member as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("Expression is not a property", "property");

            return propertyInfo;

        }

        public static IDisposable WhenPropertyChanges<T, TProperty>(this T source, Expression<Func<T, TProperty>> property, Action<TProperty> onPropertyChange)
            where T : INotifyPropertyChanged
        {
            var propertyName = property.GetPropertyInfo().Name;
            var propertySelector = property.Compile();

            PropertyChangedEventHandler handler = (sender, e) =>
                {
                    if (e.PropertyName == propertyName)
                    {
                        var newValue = propertySelector(source);
                        onPropertyChange(newValue);
                    }
                };

            source.PropertyChanged += handler;
            return Disposable.Create(() => source.PropertyChanged -= handler);
        }

        public static IDisposable WhenCollectionChanges<TItem>(this ObservableCollection<TItem> collection, Action<CollectionChanged<TItem>> onPropertyChange)
        {
            NotifyCollectionChangedEventHandler onCollectionChanged = (sender, e) =>
            {
                var payload = new CollectionChanged<TItem>(e);
                onPropertyChange(payload);
            };
            collection.CollectionChanged += onCollectionChanged;
            return Disposable.Create(() => { collection.CollectionChanged -= onCollectionChanged; });
        }

        public static IDisposable WhenCollectionChanges<T, TItem>(
            this T source,
            Expression<Func<T, ObservableCollection<TItem>>> property,
            Action<CollectionChanged<TItem>> onPropertyChange)
        {
            if (property.GetPropertyInfo().CanWrite)
                throw new ArgumentException("Cant apply to writable property");
            var propertySelector = property.Compile();
            var collection = propertySelector(source);

            return WhenCollectionChanges(collection, onPropertyChange);
        }

        public static IDisposable WhenCollectionItemsChange<TItem>(
           this ObservableCollection<TItem> collection,
           Action<CollectionChanged<TItem>> onPropertyChange)
        {
            PropertyChangedEventHandler onItemChanged = (sender, e) =>
            {
                var payload = new CollectionChanged<TItem>((TItem)sender);
                onPropertyChange(payload);
            };
            Action<IEnumerable<TItem>> registerItemChangeHandlers = items =>
            {
                foreach (var notifier in items.OfType<INotifyPropertyChanged>())
                {
                    notifier.PropertyChanged += onItemChanged;
                }
            };
            Action<IEnumerable<TItem>> unRegisterItemChangeHandlers = items =>
            {
                foreach (var notifier in items.OfType<INotifyPropertyChanged>())
                {
                    notifier.PropertyChanged -= onItemChanged;
                }
            };
            NotifyCollectionChangedEventHandler onCollectionChanged = (sender, e) =>
            {
                //If the collection is reset we cant unregister our handles. :(
                if (e.Action == NotifyCollectionChangedAction.Reset)
                    throw new NotSupportedException("This method does not support Reset Action on the underlying collection.");

                var payload = new CollectionChanged<TItem>(e);
                unRegisterItemChangeHandlers(payload.OldItems);
                registerItemChangeHandlers(payload.NewItems);
                onPropertyChange(payload);
            };

            registerItemChangeHandlers(collection);
            collection.CollectionChanged += onCollectionChanged;

            return Disposable.Create(() =>
            {
                collection.CollectionChanged -= onCollectionChanged;
                unRegisterItemChangeHandlers(collection);
            });
        }

        public static IDisposable WhenCollectionItemsChange<T, TItem>(
            this T source,
            Expression<Func<T, ObservableCollection<TItem>>> property,
            Action<CollectionChanged<TItem>> onPropertyChange)
        {
            if (property.GetPropertyInfo().CanWrite)
                throw new ArgumentException("Cant apply to writable property");
            var propertySelector = property.Compile();
            var collection = propertySelector(source);

            return WhenCollectionItemsChange(collection, onPropertyChange);
        }
    }

    public sealed class CollectionChanged<T>
    {
        private readonly NotifyCollectionChangedAction _action;
        private readonly ReadOnlyCollection<T> _newItems;
        private readonly ReadOnlyCollection<T> _oldItems;

        public CollectionChanged(NotifyCollectionChangedEventArgs collectionChangedEventArgs)
        {
            _action = collectionChangedEventArgs.Action;
            _newItems = collectionChangedEventArgs.NewItems == null
                ? new ReadOnlyCollection<T>(new T[] { })
                : new ReadOnlyCollection<T>(collectionChangedEventArgs.NewItems.Cast<T>().ToList());

            _oldItems = collectionChangedEventArgs.OldItems == null
                ? new ReadOnlyCollection<T>(new T[] { })
                : new ReadOnlyCollection<T>(collectionChangedEventArgs.OldItems.Cast<T>().ToList());
        }

        public CollectionChanged(T changedItem)
        {
            _action = NotifyCollectionChangedAction.Replace;
            _newItems = new ReadOnlyCollection<T>(new T[] { changedItem });
            _oldItems = new ReadOnlyCollection<T>(new T[] { });
        }

        public NotifyCollectionChangedAction Action
        {
            get { return _action; }
        }

        public ReadOnlyCollection<T> NewItems
        {
            get { return _newItems; }
        }

        public ReadOnlyCollection<T> OldItems
        {
            get { return _oldItems; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace BlueCrest.BlueScreen.Presentation
{
    //TODO:Potentially add the AddRange Method that everyone knows and loves.

    /// <summary>
    /// An implementation of <seealso cref="ObservableCollection{T}"/> that provides the ability to suppress
    /// change notifications. In sub-classes that allows performing batch work and raising notifications 
    /// on completion of work. Standard usage takes advantage of this feature by providing AddRange method.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public class ObservableList<T> : ObservableCollection<T>
    {
        #region Fields
        private readonly Queue<PropertyChangedEventArgs> _notifications = new Queue<PropertyChangedEventArgs>();
        private readonly Queue<NotifyCollectionChangedEventArgs> _collectionNotifications = new Queue<NotifyCollectionChangedEventArgs>();
        private int _notificationSupressionDepth = 0;
        #endregion

        public ObservableList()
            :base()
        {
        }
        public ObservableList(IEnumerable<T> collection)
            : base(collection)
        {
        }
        public ObservableList(List<T> list)
            : base(list)
        {
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (_notificationSupressionDepth == 0)
            {
                base.OnPropertyChanged(e);
            }
            else
            {
                if (!_notifications.Contains(e, NotifyEventComparer.Instance))
                {
                    _notifications.Enqueue(e);
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_notificationSupressionDepth == 0)
            {
                base.OnCollectionChanged(e);
            }
            else
            {
                //We cant filter duplicate Collection change events as this will break how UI controls work. -LC
                _collectionNotifications.Enqueue(e);
            }
        }

        protected void InvokePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected IDisposable QueueNotifications()
        {
            _notificationSupressionDepth++;
            return Disposable.Create(() =>
                                         {
                                             _notificationSupressionDepth--;
                                             TryNotify();
                                         });
        }

        private void TryNotify()
        {
            if (_notificationSupressionDepth == 0)
            {
                while (_collectionNotifications.Count > 0)
                {
                    var collectionNotification = _collectionNotifications.Dequeue();
                    base.OnCollectionChanged(collectionNotification);
                }

                while (_notifications.Count > 0)
                {
                    var notification = _notifications.Dequeue();
                    base.OnPropertyChanged(notification);
                }
            }
        }

        private sealed class NotifyEventComparer : IEqualityComparer<PropertyChangedEventArgs>
        {
            public static readonly NotifyEventComparer Instance = new NotifyEventComparer();

            public bool Equals(PropertyChangedEventArgs x, PropertyChangedEventArgs y)
            {
                return x.PropertyName == y.PropertyName;
            }

            public int GetHashCode(PropertyChangedEventArgs obj)
            {
                return obj.PropertyName.GetHashCode();
            }
        }
    }
}
using System;
using XenoGears.Assertions;
using XenoGears.ComponentModel;
using System_INotifyPropertyChanging = System.ComponentModel.INotifyPropertyChanging;
using System_PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;
using System_PropertyChangingEventArgs = System.ComponentModel.PropertyChangingEventArgs;
using System_INotifyPropertyChanged = System.ComponentModel.INotifyPropertyChanged;
using System_PropertyChangedEventHandler = System.ComponentModel.PropertyChangedEventHandler;
using System_PropertyChangedEventArgs = System.ComponentModel.PropertyChangedEventArgs;

namespace Truesight.Decompiler.Hir
{
    public abstract partial class Node : INotifyPropertyChanging, INotifyPropertyChanged, INotifyListChanging, INotifyListChanged
    {
        private void SetChangeTrackingHooks()
        {
            // todo. correctly set corrids of events raised below
            this.ParentChanging += ChangeTracking_ParentChanging;
            this.ParentChanged += ChangeTracking_ParentChanged;
            this.Children.ListChanging += ChangeTracking_ChildrenChanging;
            this.Children.ListChanged += ChangeTracking_ChildrenChanged;
            SetDomainRelatedChangeTrackingHooks();
        }

        private void ChangeTracking_ParentChanging(Node old, Node @new)
        {
            FirePropertyChanging("Parent", old, @new);
        }

        private void ChangeTracking_ParentChanged(Node old, Node @new)
        {
            FirePropertyChanging("Parent", old, @new);
        }

        private void ChangeTracking_ChildrenChanging(Object o, ListChangeEventArgs args)
        {
            FirePropertyChanging("Children", args);
            FireListChanging(args);
        }

        private void ChangeTracking_ChildrenChanged(Object o, ListChangeEventArgs args)
        {
            FireListChanged(args);
            FirePropertyChanged("Children", args);
        }

        protected void SetProperty(String name, Action setter)
        {
            var corrId = Guid.NewGuid();
            FirePropertyChanging(corrId, name);
            setter();
            FirePropertyChanged(corrId, name);
        }

        protected void SetProperty<T>(String name, Action<T> setter, T oldValue, T newValue)
        {
            if (!Equals(oldValue, newValue))
            {
                var corrId = Guid.NewGuid();
                FirePropertyChanging(corrId, name, oldValue, newValue);
                setter(newValue);
                FirePropertyChanged(corrId, name, oldValue, newValue);
            }
        }

        public event EventHandler<PropertyChangeEventArgs> PropertyChanging;
        event System_PropertyChangingEventHandler System_INotifyPropertyChanging.PropertyChanging { add { throw new NotImplementedException(); } remove { throw new NotImplementedException(); } }
        protected virtual void OnPropertyChanging(PropertyChangeEventArgs args) { }
        protected void FirePropertyChanging(String propertyName) { CoreFirePropertyChanging(new PropertyChangeEventArgs(propertyName)); }
        protected void FirePropertyChanging(Guid corrId, String propertyName) { CoreFirePropertyChanging(new PropertyChangeEventArgs(propertyName){CorrelationId = corrId }); }
        protected void FirePropertyChanging(String propertyName, Object oldValue, Object newValue) { CoreFirePropertyChanging(new PropertyChangeEventArgs(propertyName, oldValue, newValue)); }
        protected void FirePropertyChanging(Guid corrId, String propertyName, Object oldValue, Object newValue) { CoreFirePropertyChanging(new PropertyChangeEventArgs(propertyName, oldValue, newValue){CorrelationId = corrId}); }
        protected void FirePropertyChanging(String propertyName, Object tag) { CoreFirePropertyChanging(new PropertyChangeEventArgs(propertyName, tag)); }
        protected void FirePropertyChanging(Guid corrId, String propertyName, Object tag) { CoreFirePropertyChanging(new PropertyChangeEventArgs(propertyName, tag){CorrelationId = corrId}); }
        protected void FirePropertyChanging(String propertyName, Object oldValue, Object newValue, Object tag) { CoreFirePropertyChanging(new PropertyChangeEventArgs(propertyName, oldValue, newValue, tag)); }
        protected void FirePropertyChanging(Guid corrId, String propertyName, Object oldValue, Object newValue, Object tag) { CoreFirePropertyChanging(new PropertyChangeEventArgs(propertyName, oldValue, newValue, tag){CorrelationId = corrId}); }
        private void CoreFirePropertyChanging(PropertyChangeEventArgs args)
        {
            OnPropertyChanging(args);
            if (PropertyChanging != null) PropertyChanging(this, args);
            IsFrozen.AssertFalse();
        }

        public event EventHandler<PropertyChangeEventArgs> PropertyChanged;
        event System_PropertyChangedEventHandler System_INotifyPropertyChanged.PropertyChanged { add { throw new NotImplementedException(); } remove { throw new NotImplementedException(); } }
        protected virtual void OnPropertyChanged(PropertyChangeEventArgs args) { }
        protected void FirePropertyChanged(String propertyName) { CoreFirePropertyChanged(new PropertyChangeEventArgs(propertyName)); }
        protected void FirePropertyChanged(Guid corrId, String propertyName) { CoreFirePropertyChanged(new PropertyChangeEventArgs(propertyName){CorrelationId = corrId}); }
        protected void FirePropertyChanged(String propertyName, Object oldValue, Object newValue) { CoreFirePropertyChanged(new PropertyChangeEventArgs(propertyName, oldValue, newValue)); }
        protected void FirePropertyChanged(Guid corrId, String propertyName, Object oldValue, Object newValue) { CoreFirePropertyChanged(new PropertyChangeEventArgs(propertyName, oldValue, newValue){CorrelationId = corrId}); }
        protected void FirePropertyChanged(String propertyName, Object tag) { CoreFirePropertyChanged(new PropertyChangeEventArgs(propertyName, tag)); }
        protected void FirePropertyChanged(Guid corrId, String propertyName, Object tag) { CoreFirePropertyChanged(new PropertyChangeEventArgs(propertyName, tag){CorrelationId = corrId}); }
        protected void FirePropertyChanged(String propertyName, Object oldValue, Object newValue, Object tag) { CoreFirePropertyChanged(new PropertyChangeEventArgs(propertyName, oldValue, newValue, tag)); }
        protected void FirePropertyChanged(Guid corrId, String propertyName, Object oldValue, Object newValue, Object tag) { CoreFirePropertyChanged(new PropertyChangeEventArgs(propertyName, oldValue, newValue, tag){CorrelationId = corrId}); }
        private void CoreFirePropertyChanged(PropertyChangeEventArgs args)
        {
            OnPropertyChanged(args);
            if (PropertyChanged != null) PropertyChanged(this, args);
        }

        public event EventHandler<ListChangeEventArgs> ListChanging;
        protected virtual void OnListChanging(ListChangeEventArgs args) { }
        private void FireListChanging(ListChangeEventArgs args)
        {
            OnListChanging(args);
            if (ListChanging != null) ListChanging(this, args);
        }

        public event EventHandler<ListChangeEventArgs> ListChanged;
        protected virtual void OnListChanged(ListChangeEventArgs args) { }
        private void FireListChanged(ListChangeEventArgs args)
        {
            OnListChanged(args);
            if (ListChanged != null) ListChanged(this, args);
        }
    }
}

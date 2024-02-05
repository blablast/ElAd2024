using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace ElAd2024.Helpers;

public class ObservableCollectionNotifyPropertyChange<T> : ObservableCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    where T : INotifyPropertyChanged
{
    public ObservableCollectionNotifyPropertyChange() : base() {  }
    public ObservableCollectionNotifyPropertyChange(IEnumerable<T> collection) : base(collection) {  }
    public ObservableCollectionNotifyPropertyChange(List<T> list) : base(list) {  }
    private PropertyChangedEventHandler? PropertyChangedEventHandlerDelegate { get; set; }

    /// <summary>
    /// Starts listening to the PropertyChanged event of the items in the collection
    /// </summary>
    /// <param name="propertychangedDelegate"></param>
    public void Start(PropertyChangedEventHandler propertychangedDelegate)
    {
        PropertyChangedEventHandlerDelegate = propertychangedDelegate;
        CollectionChanged += Event_ColletionChanged;
        foreach (var item in Items)
        {
            item.PropertyChanged += PropertyChangedEventHandlerDelegate;
        }
    }
    
    /// <summary>
    /// Stops listening to the PropertyChanged event of the items in the collection
    /// </summary>
    public void Stop()
    {
        CollectionChanged -= Event_ColletionChanged;
        foreach (var item in Items)
        {
            item.PropertyChanged -= PropertyChangedEventHandlerDelegate;
        }
    }

    /// <summary>
    /// Changes the PropertyChanged event of the items in the collection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Event_ColletionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (T item in e.OldItems)
            {
                item.PropertyChanged -= PropertyChangedEventHandlerDelegate;
            }
        }

        if (e.NewItems is not null)
        {
            foreach (T item in e.NewItems)
            {
                item.PropertyChanged += PropertyChangedEventHandlerDelegate;
            }
        }
    }

    /// <summary>
    /// Forces the refresh of the collection
    /// </summary>
    /// <param name="sender"></param>
    public void ForceRefresh()
    {
        if (Count > 0)
        {
            ForceRefresh(this.First());
        }
        else
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
    public void ForceRefresh(object? sender)
    {
        if (sender is not null)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender)));
        }
    }


}

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlurFileEditor.Utils;
public class ObservableFilterCollection<T, F> : IEnumerable<F>, INotifyPropertyChanged, INotifyCollectionChanged where T : IList<F>, INotifyCollectionChanged
{
    T Items { get; }
    bool[] itemVisibilityMap;

    List<EventHandler<FilterArgs>> filters = new List<EventHandler<FilterArgs>>();

    public event EventHandler<FilterArgs>? Filters
    {
        add
        {
            if (value is null) return;
            filters.Add(value);
            Reset();
        }
        remove
        {
            if (value is null) return;
            filters.Remove(value);
            Reset();
        }
    }

    public int Count => itemVisibilityMap.Count(v => v);

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public ObservableFilterCollection(T items)
    {
        Items = items;
        itemVisibilityMap = new bool[Items.Count];
        for(int i = 0; i < Items.Count; i++)
        {
            itemVisibilityMap[i] = true;
        }
        foreach (var item in Items)
        {
            if(item is INotifyPropertyChanged n)
            {
                n.PropertyChanged += ItemPropertyChanged;
            }
        }
        Items.CollectionChanged += ItemsCollectionChanged;
    }

    private void ItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                if (e.NewItems is not null)
                {
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is INotifyPropertyChanged n)
                        {
                            n.PropertyChanged += ItemPropertyChanged;
                        }
                    }
                }
                Reset();
                break;
                if (Items.Count > itemVisibilityMap.Length)
                {
                    Array.Resize(ref itemVisibilityMap, Math.Min(Items.Count, itemVisibilityMap.Length * 2 + 1));
                }
                if(e.NewItems is not null)
                {
                    for(int i = Items.Count - 1; i >= e.NewStartingIndex; i++)
                    {
                        itemVisibilityMap[i + e.NewItems.Count] = itemVisibilityMap[i];
                    }
                    for(int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is F f)
                        {
                            TestItem(f, false, e.NewStartingIndex + i);
                        }
                    }
                }
                else
                {
                    Reset();
                }
                break;
            }
            case NotifyCollectionChangedAction.Remove:
            {

                if (e.OldItems is not null)
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is INotifyPropertyChanged n)
                        {
                            n.PropertyChanged -= ItemPropertyChanged;
                        }
                    }
                }
                Reset();
                break;
                if (e.OldItems is not null)
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is F f)
                        {

                        }
                    }
                }
                else
                {
                    Reset();
                }
                break;
            }
            case NotifyCollectionChangedAction.Replace:

                if (e.OldItems is not null)
                {
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        if (e.OldItems[i] is INotifyPropertyChanged n)
                        {
                            n.PropertyChanged -= ItemPropertyChanged;
                        }
                    }
                }

                if (e.NewItems is not null)
                {
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (e.NewItems[i] is INotifyPropertyChanged n)
                        {
                            n.PropertyChanged += ItemPropertyChanged;
                        }
                    }
                }
                Reset();
                break;
            case NotifyCollectionChangedAction.Move:
                Reset();
                break;
            case NotifyCollectionChangedAction.Reset:
                Reset();
                break;
            default:
                Reset();
                break;
        }
    }
    private void TestItem(F f) => TestItem(f, Items.IndexOf(f));
    private void TestItem(F f, bool isShowing) => TestItem(f, isShowing, Items.IndexOf(f));
    private void TestItem(F f, int index) => TestItem(f, IsShowing(index), index);
    private void TestItem(F f, bool isShowing, int index)
    {
        if (isShowing != IsAccepted(f))
        {
            itemVisibilityMap[index] = !isShowing;
            if (!isShowing)
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, f, GetVisibleIndex(f)));
            }
            else
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, f, GetVisibleIndex(f)));
            }
        }
    }
    void Reset()
    {
        if (Items.Count > itemVisibilityMap.Length)
        {
            Array.Resize(ref itemVisibilityMap, Math.Min(Items.Count, itemVisibilityMap.Length * 2 + 1));
        }
        for(int i = 0; i < Items.Count; i++)
        {
            bool isShowing = IsAccepted(Items[i]);
            itemVisibilityMap[i] = isShowing;
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
    }
    private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(sender is F f)
        {
            TestItem(f);
        }
    }
    int GetVisibleIndex(F f)
    {
        var index = Items.IndexOf(f);
        if (index == -1) return -1;

        int visibleIndex = 0;
        for(int i = 0; i < index; i++)
        {
            if (IsShowing(i)) visibleIndex++;
        }
        return visibleIndex;
    }
    bool IsShowing(int index) => itemVisibilityMap[index];
    bool IsShowing(F item) => Items.IndexOf(item) is >= 0 and int i ? IsShowing(i) : false;
    private bool IsAccepted(F item)
    {
        if (filters.Count == 0) return true;
        FilterArgs args = new FilterArgs(item);
        object[] invokeArgs = [this, args];
        for(int i = 0; i < filters.Count; i++)
        {
            filters[i].DynamicInvoke(invokeArgs);
            if (args.Accepted) return true;
        }
        return false;
    }

    public IEnumerator<F> GetEnumerator()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (itemVisibilityMap[i])
            {
                yield return Items[i];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class FilterArgs : EventArgs
    {
        public F Item { get; }
        public bool Accepted { get; set; }
        public FilterArgs(F item)
        {
            Item = item;
        }
    }
}
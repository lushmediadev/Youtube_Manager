using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using TqkLibrary.WpfUi.ObservableCollections;
using YoutubeManager.DataClass;
using YoutubeManager.Works;

namespace YoutubeManager.UI.ViewModels
{
    public class ChannelViewModelSaveObservableCollection : SaveFileGroupObservableCollection<Guid, ChannelData, ChannelViewModel>, IDropTarget
    {
        public ChannelViewModelSaveObservableCollection(Func<ChannelData, ChannelViewModel> func, string savePath) 
            : base(savePath, func)//x => new ChannelViewModel(x)
        {
        }

        readonly Dictionary<string, ListSortDirection> dict = new Dictionary<string, ListSortDirection>();
        public void Sort(string headerName, Guid guid)
        {
            if (string.IsNullOrEmpty(headerName)) return;
            List<ChannelViewModel>? list = null;
            Func<ChannelViewModel, object?>? func = null;

            if (headerName.Equals("Tên")) func = x => x.Title;
            if (headerName.Equals("Video")) func = x => x.VideoCount;
            if (headerName.Equals("Subscriber")) func = x => x.SubscriberCount;
            if (headerName.Equals("View")) func = x => x.ViewCount;

            if (func != null)
            {
                if (!dict.ContainsKey(headerName)) dict[headerName] = ListSortDirection.Descending;
                switch (dict[headerName])
                {
                    case ListSortDirection.Ascending:
                        dict[headerName] = ListSortDirection.Descending;
                        list = this.Where(x => x.Data.GroupId == guid).OrderByDescending(func).ToList();
                        break;

                    case ListSortDirection.Descending:
                        dict[headerName] = ListSortDirection.Ascending;
                        list = this.Where(x => x.Data.GroupId == guid).OrderBy(func).ToList();
                        break;
                }

                int i = 0;
                list?.ForEach(x => this.Move(this.IndexOf(x), i++));
            }
        }

        public void Sort(int tag, CustomColViewModel customColViewModel, Guid guid)
        {
            if (tag >= 0 && tag <= 9)
            {
                List<ChannelViewModel>? list = null;
                Func<ChannelViewModel, object>? func = x => x.CustomColumnDatas[tag].Text;

                switch (customColViewModel.ListSortDirection)
                {
                    case ListSortDirection.Ascending:
                        customColViewModel.ListSortDirection = ListSortDirection.Descending;
                        list = this.Where(x => x.Data.GroupId == guid).OrderByDescending(func).ToList();
                        break;

                    case ListSortDirection.Descending:
                        customColViewModel.ListSortDirection = ListSortDirection.Ascending;
                        list = this.Where(x => x.Data.GroupId == guid).OrderBy(func).ToList();
                        break;
                }

                int i = 0;
                list?.ForEach(x => this.Move(this.IndexOf(x), i++));
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            int i = 1;
            Singleton.IconLoad.ShutDown();
            foreach (var item in this)
            {
                item.STT = i++;
                Singleton.IconLoad.Add(new IconLoadWork(item));
            }
            Singleton.IconLoad.MaxRun = 1;
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];// this.Skip(index).FirstOrDefault();
            if (!string.IsNullOrEmpty(item.Data.Id))
            {
                if (this.Where(x => item.Data.Id.Equals(x.Data.Id)).Count() == 1)
                {
                    try
                    {
                        string filePath = $"{Directory.GetCurrentDirectory()}\\Datas\\Images\\{item.Data.Id}.png";
                        if (File.Exists(filePath)) File.Delete(filePath);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            base.RemoveItem(index);
        }
        protected override void InsertItem(int index, ChannelViewModel item)
        {
            //Extensions.IconLoad.Add(new IconLoadQueue(item));
            base.InsertItem(index, item);
        }

        #region IDropTarget
        public void DragOver(IDropInfo dropInfo)
        {
            if (CanAcceptData(dropInfo))
            {
                dropInfo.Effects = DragDropEffects.Copy;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            ChannelViewModelSaveObservableCollection target = dropInfo.TargetCollection as ChannelViewModelSaveObservableCollection;
            ChannelViewModelSaveObservableCollection source = dropInfo.DragInfo.SourceCollection as ChannelViewModelSaveObservableCollection;
            if (target != null && source != null && target.Equals(source))
            {
                int insertIndex = dropInfo.InsertIndex;
                List<ChannelViewModel> datas = ExtractData<ChannelViewModel>(dropInfo.Data).OrderBy(x => source.IndexOf(x)).ToList();

                foreach (ChannelViewModel channelViewModel in datas)
                {
                    int index_item = source.IndexOf(channelViewModel);
                    source.Move(index_item, insertIndex > index_item ? insertIndex - 1 : insertIndex++);
                }
            }
        }

        protected static bool CanAcceptData(IDropInfo dropInfo)
        {
            if (dropInfo.Data is DataObject dataObject)
            {
                string[] files = (string[])dataObject.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0) return true;
                else return false;
            }
            else if (dropInfo.DragInfo != null)
            {
                if (dropInfo.DragInfo.SourceCollection == dropInfo.TargetCollection)
                {
                    return GetList(dropInfo.TargetCollection) != null;
                }
                else if (dropInfo.DragInfo.SourceCollection is ItemCollection)
                {
                    return false;
                }
                else
                {
                    if (TestCompatibleTypes(dropInfo.TargetCollection, dropInfo.Data))
                    {
                        return !IsChildOf(dropInfo.VisualTargetItem, dropInfo.DragInfo.VisualSourceItem);
                    }
                }
            }
            return false;
        }
        protected static IEnumerable ExtractData(object data)
        {
            if (data is IEnumerable && !(data is string)) return (IEnumerable)data;
            else return Enumerable.Repeat(data, 1);
        }
        protected static IEnumerable<T> ExtractData<T>(object data)
        {
            return ExtractData(data).Cast<T>();
        }
        protected static IList GetList(IEnumerable enumerable)
        {
            if (enumerable is ICollectionView) return ((ICollectionView)enumerable).SourceCollection as IList;
            else return enumerable as IList;
        }
        protected static bool IsChildOf(UIElement targetItem, UIElement sourceItem)
        {
            ItemsControl parent = ItemsControl.ItemsControlFromItemContainer(targetItem);
            while (parent != null)
            {
                if (parent == sourceItem) return true;
                parent = ItemsControl.ItemsControlFromItemContainer(parent);
            }
            return false;
        }
        protected static bool TestCompatibleTypes(IEnumerable target, object data)
        {
            TypeFilter filter = (t, o) =>
            {
                return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            };

            var enumerableInterfaces = target.GetType().FindInterfaces(filter, null);
            var enumerableTypes = from i in enumerableInterfaces select i.GetGenericArguments().Single();

            if (enumerableTypes.Count() > 0)
            {
                Type dataType = TypeUtilities.GetCommonBaseClass(ExtractData(data));
                return enumerableTypes.Any(t => t.IsAssignableFrom(dataType));
            }
            else
            {
                return target is IList;
            }
        }
        #endregion
    }
}

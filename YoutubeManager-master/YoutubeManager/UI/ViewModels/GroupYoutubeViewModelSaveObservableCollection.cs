using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using TqkLibrary.WpfUi.ObservableCollections;
using YoutubeManager.DataClass;

namespace YoutubeManager.UI.ViewModels
{
    public class GroupYoutubeViewModelSaveObservableCollection : SaveFileObservableCollection<GroupYoutubeData, GroupYoutubeViewModel>, IDropTarget
    {
        public GroupYoutubeViewModelSaveObservableCollection(string savePath) : base(savePath, x => new GroupYoutubeViewModel(x))
        {

        }

        GroupYoutubeViewModel? _SelectedItem = null;
        public GroupYoutubeViewModel? SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; NotifyPropertyChange(); }
        }

        readonly Dictionary<string, ListSortDirection> dict = new Dictionary<string, ListSortDirection>();
        public void Sort(string headerName)
        {
            if (string.IsNullOrEmpty(headerName)) return;
            List<GroupYoutubeViewModel>? list = null;
            Func<GroupYoutubeViewModel, object>? func = null;
            if (headerName.Equals("Nhóm")) func = x => x.Name;


            if (func != null)
            {
                if (!dict.ContainsKey(headerName)) dict[headerName] = ListSortDirection.Descending;
                switch (dict[headerName])
                {
                    case ListSortDirection.Ascending:
                        dict[headerName] = ListSortDirection.Descending;
                        list = this.OrderByDescending(func).ToList();
                        break;

                    case ListSortDirection.Descending:
                        dict[headerName] = ListSortDirection.Ascending;
                        list = this.OrderBy(func).ToList();
                        break;
                }

                int i = 0;
                list?.ForEach(x => this.Move(this.IndexOf(x), i++));
            }
        }

        #region IDropTarget
        public void DragOver(IDropInfo dropInfo)
        {
            if (CanAcceptData(dropInfo))
            {
                dropInfo.Effects = DragDropEffects.Copy;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }
            else if (dropInfo.DragInfo.SourceItem is ChannelViewModel)
            {
                dropInfo.Effects = DragDropEffects.Copy;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            GroupYoutubeViewModelSaveObservableCollection? target = dropInfo.TargetCollection as GroupYoutubeViewModelSaveObservableCollection;
            GroupYoutubeViewModelSaveObservableCollection? source = dropInfo.DragInfo.SourceCollection as GroupYoutubeViewModelSaveObservableCollection;
            ChannelViewModelSaveObservableCollection? source_channel = dropInfo.DragInfo.SourceCollection as ChannelViewModelSaveObservableCollection;

            if (target != null && source != null && target.Equals(source))
            {
                int insertIndex = dropInfo.InsertIndex;
                List<GroupYoutubeViewModel> datas = ExtractData<GroupYoutubeViewModel>(dropInfo.Data).OrderBy(x => source.IndexOf(x)).ToList();

                foreach (GroupYoutubeViewModel groupYoutubeViewModel in datas)
                {
                    int index_item = source.IndexOf(groupYoutubeViewModel);
                    source.Move(index_item, insertIndex > index_item ? insertIndex - 1 : insertIndex++);
                }
            }
            else if (source_channel != null && target != null)
            {
                GroupYoutubeViewModel drop_target = (dropInfo.TargetItem as GroupYoutubeViewModel)!;
                List<ChannelViewModel> dragDatas = ExtractData<ChannelViewModel>(dropInfo.Data).ToList();
                foreach (var dragData in dragDatas)
                {
                    if (dragData.Data.GroupId != drop_target.Data.GroupId)//drag to diff group
                    {
                        if (source_channel.ViewModels
                            .Where(x => x.Data.GroupId == drop_target.Data.GroupId)
                            .Any(x => x.Data.Id?.Equals(dragData.Data.Id) == true))
                        //source_channel.Any(x => x.Data.Id.Equals(data.Data.Id) && x.Data.GroupId == target_item.Data.Guid))//dupp
                        {
                            if (MessageBox.Show($"Kênh \"{dragData.Title}\" đã có sẵn trong nhóm \"{drop_target.Name}\"\r\nBạn có muốn loại bỏ kênh \"{dragData.Title}\"?",
                              "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)//remove
                            {
                                source_channel.Remove(dragData);
                            }
                            else//cancel
                            {

                            }
                        }
                        else//
                        {
                            source_channel.Remove(dragData);
                            dragData.Data.GroupId = drop_target.Data.GroupId;
                            source_channel.Add(dragData);

                            if (this.SelectedItem is not null)
                            {
                                source_channel.ShowGroup(SelectedItem.Data.GroupId);
                            }
                        }
                    }
                }
                source_channel.Save();
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

using System;
using System.Collections.Generic;

namespace JinToliq.Umvvm.ViewModel.Properties
{
  public class PagedCollectionProperty<TElement> : ProperyBatch
  {
    public event Action<int> PageCountChanged;
    public event Action<int> PageIndexChanged;
    public event Action<int> PageSizeChanged;

    private readonly CollectionProperty<TElement> _collection;
    private readonly Property<int> _pageCount;
    private readonly Property<int> _pageIndex;
    private readonly Property<int> _pageSize;

    public IList<TElement> Value
    {
      get => _collection.Value;
      set => _collection.Value = value;
    }

    public int PageCount => _pageCount.Value;
    public int PageIndex => _pageIndex.Value;
    public int PageSize => _pageSize.Value;

    public PagedCollectionProperty(string name, string pageCountName = "PageCount", string pageIndexName = "PageIndex", string pageSizeName = "PageSize")
    {
      _collection = new(name);
      _pageCount = new(pageCountName);
      _pageIndex = new(pageIndexName);
      _pageSize = new(pageSizeName);

      With(_collection);
      With(_pageCount);
      With(_pageSize);
      With(_pageIndex);

      _pageCount.Changed += OnPageCountChanged;
      _pageIndex.Changed += OnPageIndexChanged;
      _pageSize.Changed += OnPageSizeChanged;
    }

    private void OnPageCountChanged(Property<int> obj) =>
      PageCountChanged?.Invoke(obj.Value);

    private void OnPageIndexChanged(Property<int> obj) =>
      PageIndexChanged?.Invoke(obj.Value);

    private void OnPageSizeChanged(Property<int> obj) =>
      PageSizeChanged?.Invoke(obj.Value);
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View.Binding.Specific
{
  public class PagedCollectionViewBinding : CollectionViewBinding
  {
    public enum UpdatePageSizeMode
    {
      OnBound = 0,
      OnEnabled = 1,
    }

    [SerializeField] private string _pageSizePath = "PageSize";
    [SerializeField] private string _pageCountPath = "PageCount";
    [SerializeField] private string _pageIndexPath = "PageIndex";

    [Min(1)]
    [SerializeField] private int _pageSize;
    [SerializeField] private UpdatePageSizeMode _updatePageSizeMode = UpdatePageSizeMode.OnBound;
    [SerializeField] private bool _padItemsToPageSize = true;

    private Property<int> _pageCountProperty;
    private Property<int> _pageIndexProperty;
    private Property<int> _pageSizeProperty;

    protected override IEnumerable<object> ValidCollection
    {
      get
      {
        var skipCount = PageIndex * _pageSize;
        var result = Collection
          .Skip(skipCount)
          .Take(_pageSize);

        if (!_padItemsToPageSize)
          return result;

        var unskippedCount = Collection.Count - skipCount;
        if (unskippedCount >= _pageSize)
          return result;

        var padCount = _pageSize - unskippedCount;
        var itemType = Property.GetDataType();
        var padItemInstance = itemType.IsValueType
          ? Activator.CreateInstance(itemType)
          : null;

        var padItems = Enumerable.Repeat(padItemInstance, padCount);
        return result.Concat(padItems);
      }
    }

    private int _pageCount;
    private int PageCount
    {
      get => _pageCount;
      set
      {
        if (_pageCount == value)
          return;

        _pageCount = value;
        _pageCountProperty.Value = value;
        OnChanged(Property);
      }
    }

    private int _pageIndex;
    private int PageIndex
    {
      get => _pageIndex;
      set
      {
        if (_pageIndex == value)
          return;

        _pageIndex = value;
        _pageIndexProperty.Value = value;
        OnChanged(Property);
      }
    }

    protected override void OnCollectionUpdated()
    {
      base.OnCollectionUpdated();
      var totalCount = Collection.Count;

      PageCount = Mathf.Min(Mathf.CeilToInt((float)totalCount / _pageSize), 1);
      if (PageIndex >= PageCount)
        PageIndex = Mathf.Max(PageCount - 1, 0);
    }

    protected override void Bind()
    {
      base.Bind();
      _pageSizeProperty = GetProperty<Property<int>>(_pageSizePath);
      _pageCountProperty = GetProperty<Property<int>>(_pageCountPath);
      _pageIndexProperty = GetProperty<Property<int>>(_pageIndexPath);

      _pageIndexProperty.Changed += OnPageIndexChanged;
    }

    protected override void OnBound()
    {
      base.OnBound();
      if (_updatePageSizeMode is UpdatePageSizeMode.OnBound)
        _pageSizeProperty.Value = _pageSize;
    }

    protected override void OnEnabled()
    {
      base.OnEnabled();
      if (_updatePageSizeMode is UpdatePageSizeMode.OnEnabled)
        _pageSizeProperty.Value = _pageSize;
    }

    protected override void Unbind()
    {
      base.Unbind();
      _pageIndexProperty.Changed -= OnPageIndexChanged;

      _pageSizeProperty = null;
      _pageCountProperty = null;
      _pageIndexProperty = null;
    }

    private void OnPageIndexChanged(Property<int> property)
    {
      var index = PageCount == 0
        ? 0
        : Mathf.Clamp(property.Value, 0, PageCount - 1);

      PageIndex = index;
    }
  }
}
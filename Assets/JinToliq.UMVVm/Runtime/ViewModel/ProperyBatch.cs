using System.Collections.Generic;
using JinToliq.Umvvm.ViewModel.Exceptions;

namespace JinToliq.Umvvm.ViewModel
{
  public class ProperyBatch
  {
    private readonly List<Property> _properties = new();

    public IReadOnlyList<Property> Properties => _properties;

    public ProperyBatch()
    { }

    public ProperyBatch(params Property[] properties) =>
      _properties.AddRange(properties);

    public ProperyBatch With(Property property)
    {
      _properties.Add(property);
      return this;
    }

    public Property<T> GetAt<T>(int index) where T : Property =>
      this[index] as Property<T> ?? throw new InvalidPropertyTypeException();

    public Property this[int index]
    {
      get
      {
        if (index < 0 || index >= _properties.Count)
          throw new System.ArgumentOutOfRangeException(nameof(index));

        return _properties[index];
      }
    }
  }
}
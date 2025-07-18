using System;
using JinToliq.Umvvm.ViewModel;

namespace JinToliq.Umvvm.View.Binding
{
  public abstract class SingleNumberPropertyBinding : SinglePropertyBinding
  {
    protected sealed override void OnChanged(Property property)
    {
      switch (property)
      {
        case Property<byte> byteProperty:
          OnChanged(byteProperty.Value);
          break;
        case Property<sbyte> sbyteProperty:
          OnChanged(sbyteProperty.Value);
          break;
        case Property<short> shortProperty:
          OnChanged(shortProperty.Value);
          break;
        case Property<ushort> ushortProperty:
          OnChanged(ushortProperty.Value);
          break;
        case Property<int> intProperty:
          OnChanged(intProperty.Value);
          break;
        case Property<uint> uintProperty:
          OnChanged(uintProperty.Value);
          break;
        case Property<long> longProperty:
          OnChanged(longProperty.Value);
          break;
        case Property<ulong> ulongProperty:
          OnChanged(ulongProperty.Value);
          break;
        case Property<float> floatProperty:
          OnChanged(floatProperty.Value);
          break;
        case Property<double> doubleProperty:
          OnChanged(doubleProperty.Value);
          break;
        case Property<decimal> decimalProperty:
          OnChanged((double)decimalProperty.Value);
          break;

        default:
          EvaluateNonNumber(property);
          break;
      }
    }

    private void EvaluateNonNumber(Property property)
    {
      var value = property.GetValue();
      switch (value)
      {
        case Enum enumValue:
          OnChanged(Convert.ToDouble(enumValue));
          break;
        case string stringValue when double.TryParse(stringValue, out var parsedValue):
          OnChanged(parsedValue);
          break;
        case string stringValue when long.TryParse(stringValue, out var parsedValue):
          OnChanged(parsedValue);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(property), property?.GetType().Name ?? "null", "Invalid property type for SingleNumberPropertyBinding. Expected a numeric type or convertible to double.");
      }
    }

    protected abstract void OnChanged(double value);
  }
}
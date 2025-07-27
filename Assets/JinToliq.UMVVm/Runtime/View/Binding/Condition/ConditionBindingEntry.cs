using System;
using System.Linq;
using JinToliq.Umvvm.ViewModel;
using UnityEngine;

namespace JinToliq.Umvvm.View.Binding.Condition
{
  public enum ConditionType
  {
    Bool = 0,
    Equals = 1,
    Greater = 2,
    Empty = 3,
    HasFlag = 4,
  }

  [Serializable]
  internal class ConditionBindingEntry
  {
    [SerializeField] private string _path;
    [SerializeField] private ConditionType _condition;
    [SerializeField] private string _value;
    [SerializeField] private bool _invert;

    private ConditionBinding _parent;
    private Property _property;
    private bool _lastResult;

    public bool LastResult
    {
      get => _lastResult;
      private set
      {
        if (_lastResult == value)
          return;

        _lastResult = value;
        _parent.OnEntryChanged(this);
      }
    }

    public void SetParent(ConditionBinding parent) =>
      _parent = parent ?? throw new ArgumentNullException(nameof(parent), "Parent cannot be null");

    public void Bind()
    {
      _property = _parent.GetProperty(_path);
      _property.Changed += OnChanged;
    }

    public void Unbind()
    {
      _property.Changed -= OnChanged;
      _property = null;
    }

    public bool ForceEvaluate()
    {
      var input = _property.GetValue();
      var direct = EvaluateDirect(input);
      _lastResult = _invert ? !direct : direct;
      return LastResult;
    }

    public string ToInspectString()
    {
      var sb = new System.Text.StringBuilder();
      if (_property is null)
      {
        sb.Append("Property is not set");
      }
      else
      {
        sb.Append('[').Append(_path).Append(']').Append(": ");
        sb.Append("cached: ").Append(LastResult).Append("; ");
        sb.Append("evaluated: ").Append(_property.GetValue()?.ToString() ?? "null");
      }

      return sb.ToString();
    }

    protected void OnChanged(Property property)
    {
      var input = _property.GetValue();
      var direct = EvaluateDirect(input);
      LastResult = _invert ? !direct : direct;
    }

    private bool EvaluateDirect(object input)
    {
      if (input == null)
        return EvaluateNull();

      switch (input)
      {
        case bool boolValue:
          return boolValue;
        case string stringValue:
          return EvaluateString(stringValue);
        case byte byteValue:
          return EvaluateNumber(byteValue);
        case short shortValue:
          return EvaluateNumber(shortValue);
        case int intValue:
          return EvaluateNumber(intValue);
        case long longValue:
          return EvaluateNumber(longValue);
        case float floatValue:
          return EvaluateNumber(floatValue);
        case double doubleValue:
          return EvaluateNumber(doubleValue);
        case Enum enumValue:
          return EvaluateEnum(enumValue);
        default:
          throw new ArgumentOutOfRangeException(nameof(input), _property.GetDataType().Name, "Unhandled property value type");
      }
    }

    private bool EvaluateNull()
    {
      switch (_condition)
      {
        case ConditionType.Bool:
        case ConditionType.Equals:
        case ConditionType.Greater:
        case ConditionType.HasFlag:
          return false;
        case ConditionType.Empty:
          return true;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private bool EvaluateString(string propertyValue)
    {
      switch (_condition)
      {
        case ConditionType.Bool:
          return false;
        case ConditionType.Equals:
          return string.Equals(propertyValue, _value, StringComparison.OrdinalIgnoreCase);
        case ConditionType.Greater:
          return false;
        case ConditionType.Empty:
          return string.IsNullOrEmpty(propertyValue);
        case ConditionType.HasFlag:
          throw new ArgumentOutOfRangeException(nameof(_condition), _condition, "HasFlag condition is not applicable for string evaluation");
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private bool EvaluateNumber(long propertyValue)
    {
      switch (_condition)
      {
        case ConditionType.Bool:
          return propertyValue > 0;
        case ConditionType.Equals:
          return propertyValue == long.Parse(_value);
        case ConditionType.Greater:
          return propertyValue > long.Parse(_value);
        case ConditionType.Empty:
          return false;
        case ConditionType.HasFlag:
          throw new ArgumentOutOfRangeException(nameof(_condition), _condition, "HasFlag condition is not applicable for long evaluation");
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private bool EvaluateNumber(double propertyValue)
    {
      switch (_condition)
      {
        case ConditionType.Bool:
          return propertyValue > 0;
        case ConditionType.Equals:
          return Math.Abs(propertyValue - double.Parse(_value)) < double.Epsilon;
        case ConditionType.Greater:
          return propertyValue > double.Parse(_value);
        case ConditionType.Empty:
          return false;
        case ConditionType.HasFlag:
          throw new ArgumentOutOfRangeException(nameof(_condition), _condition, "HasFlag condition is not applicable for number evaluation");
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private bool EvaluateEnum(Enum propertyValue)
    {
      var inputValueIsLong = long.TryParse(_value, out var inputLongValue);
      var type = _property.GetDataType();
      object inputValueAsEnum;

      switch (_condition)
      {
        case ConditionType.Bool:
        case ConditionType.Equals:
          if (inputValueIsLong)
            return inputLongValue == Convert.ToInt64(propertyValue);

          return string.Equals(_value, propertyValue.ToString(), StringComparison.OrdinalIgnoreCase);

        case ConditionType.Greater:
          if (inputValueIsLong)
            return inputLongValue > Convert.ToInt64(propertyValue);

          if (!Enum.TryParse(type, _value, true, out inputValueAsEnum))
            throw new ArgumentOutOfRangeException(nameof(_value), _value, $"Value: {_value} is not defined in the enum type: {type.Name}");

          return Convert.ToInt64(propertyValue) > Convert.ToInt64(inputValueAsEnum);

        case ConditionType.HasFlag:
          if (inputValueIsLong)
            return (Convert.ToInt64(propertyValue) & inputLongValue) == inputLongValue;

          if (!Enum.TryParse(type, _value, true, out inputValueAsEnum))
            throw new ArgumentOutOfRangeException(nameof(_value), _value, $"Value: {_value} is not defined in the enum type: {type.Name}");

          var inputValueAsLong = Convert.ToInt64(inputValueAsEnum);
          return (Convert.ToInt64(propertyValue) & inputValueAsLong) == inputValueAsLong;

        case ConditionType.Empty:
        default:
          throw new ArgumentOutOfRangeException(nameof(_condition), _condition, "Unhandled condition type for enum evaluation");
      }
    }
  }
}
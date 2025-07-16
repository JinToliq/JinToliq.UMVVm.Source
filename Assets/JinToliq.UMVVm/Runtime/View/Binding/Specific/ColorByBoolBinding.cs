using JinToliq.Umvvm.ViewModel;
using UnityEngine;
using UnityEngine.UI;

namespace JinToliq.Umvvm.View.Binding.Specific
{
  public class ColorByBoolBinding : SinglePropertyBinding<bool>
  {
    [SerializeField] private Graphic _graphics;
    [SerializeField] private Color _trueColor = Color.green;
    [SerializeField] private Color _falseColor = Color.red;

    protected override void OnBeforeChange(Property<bool> property)
    { }

    protected override void OnChanged(Property<bool> property)
    {
      _graphics.color = property.Value
        ? _trueColor
        : _falseColor;
    }

    private void Reset() =>
      _graphics = GetComponent<Graphic>();
  }
}
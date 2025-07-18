using UnityEngine;
using UnityEngine.UI;

namespace JinToliq.Umvvm.View.Binding.Specific
{
  public class ImageFillBinding : SingleNumberPropertyBinding
  {
    [SerializeField] private Image _image;
    [SerializeField] private bool _invert;

    protected override void OnChanged(double value)
    {
      if (_invert)
        value = 1f - value;

      _image.fillAmount = (float)value;
    }

    private void Reset() => _image = GetComponent<Image>();
  }
}
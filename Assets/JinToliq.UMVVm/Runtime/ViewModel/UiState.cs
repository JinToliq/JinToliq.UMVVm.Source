namespace JinToliq.Umvvm.ViewModel
{
  public enum UiType
  {
    PopAbove,
    OpenNewWindow,
  }

  public class UiState<TStateId>
  {
    public readonly TStateId Id;
    public readonly int Index;
    public readonly object OpenWithState;

    public UiState(TStateId id, int index, object openWithState = null)
    {
      Id = id;
      Index = index;
      OpenWithState = openWithState;
    }
  }
}
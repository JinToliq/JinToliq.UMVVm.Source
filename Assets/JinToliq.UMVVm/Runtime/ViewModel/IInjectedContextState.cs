namespace JinToliq.Umvvm.ViewModel
{
  public interface IInjectedContextState
  {
    IContextWithState Context { get; internal set; }
  }
}
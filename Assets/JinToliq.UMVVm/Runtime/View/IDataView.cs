using System.Diagnostics.CodeAnalysis;
using JinToliq.Umvvm.ViewModel;

namespace JinToliq.Umvvm.View
{
  public interface IDataView
  {
    public Context Context { get; }

    Property GetProperty([NotNull] string property, string masterPath = null);
    TProperty GetProperty<TProperty>([NotNull] string property, string masterPath = null) where TProperty : Property;
    Command GetCommand([NotNull] string property, string masterPath = null);
    TCommand GetCommand<TCommand>([NotNull] string property, string masterPath = null) where TCommand : ICommand;
  }
}
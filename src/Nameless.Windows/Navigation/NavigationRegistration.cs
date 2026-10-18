using Nameless.Registration;
using Wpf.Ui;
using Wpf.Ui.Abstractions.Controls;

namespace Nameless.Windows.Navigation;

public class NavigationRegistration : AssemblyScanAware<NavigationRegistration> {
    private static readonly Type NavigableViewType = typeof(INavigableView<>);

    private readonly HashSet<Type> _navigationViews = [];

    public Type? NavigationWindow { get; private set; }
    
    public IReadOnlyCollection<Type> NavigationViews => UseAssemblyScan
        ? ExecuteAssemblyScan(NavigableViewType)
        : _navigationViews;

    public NavigationRegistration RegisterNavigationWindow<TNavigationWindow>() where TNavigationWindow : INavigationWindow {
        return RegisterNavigationWindow(typeof(TNavigationWindow));
    }

    public NavigationRegistration RegisterNavigationWindow(Type type) {
        Throws.When.IsNonConcreteType(type);
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNotAssignableFrom(type, typeof(INavigationWindow));

        NavigationWindow = type;

        return this;
    }

    public NavigationRegistration RegisterNavigationView<TNavigationView, TView>() where TNavigationView : INavigableView<TView> {
        return RegisterNavigationView(typeof(TNavigationView));
    }

    public NavigationRegistration RegisterNavigationView(Type type) {
        Throws.When.IsNonConcreteType(type);
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNotAssignableFromGeneric(type, NavigableViewType);

        _navigationViews.Add(type);

        return this;
    }
}
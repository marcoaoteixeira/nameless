using Nameless.Registration;

namespace Nameless.Windows.UI;

public class WindowFactoryRegistration : AssemblyScanAware<WindowFactoryRegistration> {
    private static readonly Type WindowType = typeof(IWindow);

    private readonly HashSet<Type> _windows = [];

    public IReadOnlyCollection<Type> Windows => UseAssemblyScan
        ? ExecuteAssemblyScan(WindowType)
        : _windows;

    public WindowFactoryRegistration RegisterWindow<TWindow>() where TWindow : class, IWindow {
        return RegisterWindow(typeof(TWindow));
    }

    public WindowFactoryRegistration RegisterWindow(Type type) {
        Throws.When.IsNonConcreteType(type);
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNotAssignableFrom(type, WindowType);

        _windows.Add(type);

        return this;
    }
}
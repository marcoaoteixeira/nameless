namespace Nameless {

    /// <summary>
    /// Defines that the annotated class is a Null-Object Pattern
    /// implementation.
    /// (see: https://en.wikipedia.org/wiki/Null_Object_pattern)
    /// So, if we're using our Autofac (https://autofac.org) IoC
    /// implementation, we'll not get any Null-Object inside your container,
    /// after convention configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class NullObjectAttribute : Attribute { }
}
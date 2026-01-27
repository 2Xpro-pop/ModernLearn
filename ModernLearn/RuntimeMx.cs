using Avalonia;

namespace ModernLearn;

public abstract class RuntimeMx
{
    public abstract object ProvideValue(AvaloniaObject avaloniaObject, AvaloniaProperty avaloniaProperty, string value);
}

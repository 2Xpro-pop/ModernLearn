using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System;
using Avalonia.VisualTree;

namespace ModernLearn;

public sealed class SetResourceExtension : MarkupExtension
{
    public string Key { get; set; } = null!;
    public string Expression { get; set; } = null!;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var providerValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (providerValueTarget?.TargetObject is not AvaloniaObject avaloniaObject)
        {
            throw new InvalidOperationException("SetResourceExtension can only be used on Avalonia objects.");
        }

        if (Key is null)
        {
            return null!;
        }

        var resources = GetResourceDictionary(avaloniaObject, Key) ?? throw new InvalidOperationException($"Resource dictionary not found.");
        
        if (Expression == "!$self")
        {
            return ReactiveCommand.Create(() =>
            {
                var self = resources[Key];

                if (self is not bool boolValue)
                {
                    throw new InvalidOperationException($"Resource with key '{Key}' is not a boolean. Negate operator '!' required boolean. Current Value: {self}");
                }

                return resources[Key] = !boolValue;
            });
        }

        if(Expression == "$self")
        {
            return ReactiveCommand.Create(() => resources[Key]);
        }

        if(Expression == "$parameter")
        {
            return ReactiveCommand.Create<object?>(parameter => resources[Key] = parameter!);
        }

        var currentValue = resources[Key];

        if(currentValue is null)
        {
            return ReactiveCommand.Create(() => resources[Key] = null!);
        }

        var converter = DefaultValueConverter.Instance;

        var convertedValue = converter.Convert(Expression, currentValue.GetType(), null, null!);


        return ReactiveCommand.Create(() => resources[Key] = convertedValue);
    }


    private static IResourceDictionary? GetResourceDictionary(AvaloniaObject target, string key)
    {
        var currentResource = GetResourceDictionary(target);
        var currentTarget = target;

        while (currentResource != null)
        {
            if (currentResource.ContainsKey(key))
            {
                return currentResource;
            }

            currentTarget = GetParent(currentTarget);

            if (currentTarget is null)
            {
                break;
            }

            currentResource = GetResourceDictionary(currentTarget);
        }

        var resources = GetResourceDictionary(target);

        if (resources is null)
        {
            return null;
        }

        resources[key] = null!;

        return resources;
    }

    private static IResourceDictionary? GetResourceDictionary(AvaloniaObject target)
    {
        if (target is StyledElement styled)
        {
            return styled.Resources;
        }

        if (target is Application app)
        {
            return app.Resources;
        }

        return null;
    }

    private static StyledElement? GetParent(AvaloniaObject target)
    {
        if (target is StyledElement { Parent: StyledElement parrent })
        {
            return parrent;
        }

        if (target is Visual visual)
        {
            return visual.GetVisualParent();
        }

        return null;
    }
}

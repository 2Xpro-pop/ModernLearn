using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Xml;

namespace ModernLearn;

public static class SafeXamlLoader
{
    private static readonly FrozenDictionary<string, Type> WellKnownTypes = new Dictionary<string, Type>
    {
        { nameof(Grid),       typeof(Grid) },
        { nameof(StackPanel), typeof(StackPanel) },
        { nameof(DockPanel),  typeof(DockPanel) },
        { nameof(Button),     typeof(Button) },
        { nameof(Border),     typeof(Border) },
        { nameof(TextBlock),  typeof(TextBlock) },
        { "H1",               typeof(TextBlock) },
        { "P",                typeof(TextBlock) },

    }.ToFrozenDictionary(StringComparer.Ordinal);

    // AOT-safe factories (no Activator)
    private static readonly FrozenDictionary<string, Func<AvaloniaObject>> Factories =
        new Dictionary<string, Func<AvaloniaObject>>(StringComparer.Ordinal)
        {
            [nameof(Grid)] = static () => new Grid(),
            [nameof(StackPanel)] = static () => new StackPanel(),
            [nameof(DockPanel)] = static () => new DockPanel(),
            [nameof(Button)] = static () => new Button(),
            [nameof(Border)] = static () => new Border(),
            [nameof(TextBlock)] = static () => new TextBlock(),
            ["H1"] = static () => new TextBlock { Classes = { "H1" } },
            ["P"] = static () => new TextBlock { Classes = { "P" } },
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private static readonly FrozenDictionary<string, RuntimeMx> WellKnowMarkupExtensions =
        new Dictionary<string, RuntimeMx>(StringComparer.Ordinal)
        {
        }.ToFrozenDictionary(StringComparer.Ordinal);

    private static bool TryWellKnownType(string typeName, [MaybeNullWhen(false)] out Type type) =>
        WellKnownTypes.TryGetValue(typeName, out type);

    private static AvaloniaProperty? GetProperty(Type type, string propertyName)
    {
        return AvaloniaPropertyRegistry.Instance.FindRegistered(type, propertyName);
    }

    private static object? ProvideValue(AvaloniaObject avaloniaObject, AvaloniaProperty avaloniaProperty, string value)
    {
        Debug.Assert(value.StartsWith('{') && value.EndsWith('}'), "Value must be a markup extension.");

        value = value.TrimStart('{').TrimEnd('}');
        var typeName = value.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];

        if (WellKnowMarkupExtensions.TryGetValue(typeName, out var mx))
        {
            return mx.ProvideValue(avaloniaObject, avaloniaProperty, value);
        }

        return null;
    }

    public static object Parse([StringSyntax(StringSyntaxAttribute.Xml)] string xaml)
    {
        using var sr = new StringReader(xaml);

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
        };

        using var xr = XmlReader.Create(sr, settings);

        AvaloniaObject? root = null;
        var stack = new Stack<AvaloniaObject>();

        while (xr.Read())
        {
            switch (xr.NodeType)
            {
                case XmlNodeType.Element:
                    {
                        var localName = xr.LocalName;

                        if (localName.Contains('.', StringComparison.Ordinal))
                            throw new FormatException($"Property elements are not supported: <{xr.Name}>.");

                        if (!Factories.TryGetValue(localName, out var factory))
                            throw new FormatException($"Type is not allowed: '{localName}'.");

                        var avaloniaObject = factory();

                        if(avaloniaObject is StyledElement styledElement)
                        {
                            styledElement.BeginInit();
                        }

                        // attributes
                        if (xr.HasAttributes)
                        {
                            while (xr.MoveToNextAttribute())
                            {
                                if (IsIgnorableAttribute(xr))
                                    continue;

                                ApplyAttribute(avaloniaObject, xr.Name, xr.LocalName, xr.Prefix, xr.Value);
                            }

                            xr.MoveToElement();
                        }

                        // build tree
                        if (stack.Count == 0)
                        {
                            root = avaloniaObject;
                        }
                        else
                        {
                            AddChild(stack.Peek(), avaloniaObject);
                        }

                        // push if not empty
                        if (!xr.IsEmptyElement)
                            stack.Push(avaloniaObject);

                        break;
                    }

                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                    {
                        var text = xr.Value;
                        if (string.IsNullOrWhiteSpace(text))
                            break;

                        if (stack.Count == 0)
                            throw new FormatException("Text at document root is not supported.");

                        ApplyText(stack.Peek(), text);
                        break;
                    }

                case XmlNodeType.EndElement:
                    {
                        if (stack.Count == 0)
                        {
                            throw new FormatException($"Unexpected end element: </{xr.Name}>.");
                        }

                        var finished = stack.Pop();

                        if (finished is StyledElement styledElement)
                        {
                            styledElement.EndInit();
                        }

                        break;
                    }
            }
        }

        return root ?? throw new FormatException("Root element not found.");
    }

    private static bool IsIgnorableAttribute(XmlReader xr)
    {
        // xmlns / xmlns:*
        if (xr.Prefix == "xmlns" || xr.Name == "xmlns")
            return true;

        // design-time / x:*
        if (xr.Prefix is "x" or "d" or "mc")
            return true;

        if (xr.Name is "x:Class" or "x:DataType" or "x:CompileBindings")
            return true;

        return false;
    }

    private static void ApplyAttribute(AvaloniaObject obj, string attrName, string localName, string prefix, string rawValue)
    {
        if (prefix == "x" && localName == "Name" && obj is StyledElement se)
        {
            se.Name = rawValue;
            return;
        }

        if(localName is nameof(Grid.RowDefinitions) or nameof(Grid.ColumnDefinitions))
        {
            if (obj is Grid grid)
            {
                if (localName == nameof(Grid.RowDefinitions))
                {
                    grid.RowDefinitions = new RowDefinitions(rawValue);
                    return;
                }

                if (localName == nameof(Grid.ColumnDefinitions))
                {
                    grid.ColumnDefinitions = new ColumnDefinitions(rawValue);
                    return;
                }
            }
            else
            {
                throw new FormatException($"'{localName}' attribute is only supported on 'Grid' elements.");
            }
        }

        // attached: Grid.Row="1"
        if (attrName.Contains('.', StringComparison.Ordinal))
        {
            var parts = attrName.Split('.', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
                throw new FormatException($"Invalid attached property: '{attrName}'.");

            var ownerTypeName = parts[0];
            var propName = parts[1];

            if (!TryWellKnownType(ownerTypeName, out var ownerType))
                throw new FormatException($"Attached property owner type is not allowed: '{ownerTypeName}'.");

            var ap = GetProperty(ownerType, propName)
                     ?? throw new FormatException($"Unknown attached property: '{attrName}'.");

            SetAvaloniaProperty(obj, ap, rawValue);
            return;
        }

        var prop = GetProperty(obj.GetType(), localName)
                   ?? throw new FormatException($"Unknown property '{localName}' on type '{obj.GetType().Name}'.");

        SetAvaloniaProperty(obj, prop, rawValue);
    }

    private static void SetAvaloniaProperty(AvaloniaObject obj, AvaloniaProperty prop, string rawValue)
    {
        if (rawValue.StartsWith('{') && rawValue.EndsWith('}'))
        {
            var provided = ProvideValue(obj, prop, rawValue) ?? throw new FormatException($"MarkupExtension not allowed/unknown: '{rawValue}'.");
            obj.SetValue(prop, provided);
            return;
        }

        var converted = ConvertFromString(rawValue, prop.PropertyType);
        obj.SetValue(prop, converted);
    }

    private static object? ConvertFromString(string s, Type targetType)
    {
        // Nullable<T>
        var underlying = Nullable.GetUnderlyingType(targetType);
        if (underlying is not null)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;

            targetType = underlying;
        }

        if (targetType == typeof(string))
            return s;

        if (targetType == typeof(bool))
            return bool.Parse(s);

        if (targetType == typeof(int))
            return int.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture);

        if (targetType == typeof(double))
            return double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        if (targetType == typeof(float))
            return float.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        if (targetType == typeof(Thickness))
            return Thickness.Parse(s);

        if (targetType == typeof(CornerRadius))
            return CornerRadius.Parse(s);

        if (targetType == typeof(GridLength))
            return GridLength.Parse(s);

        if (targetType == typeof(Color))
            return Color.Parse(s);

        // Background/BorderBrush
        if (targetType == typeof(IBrush) || typeof(IBrush).IsAssignableFrom(targetType))
            return Brush.Parse(s);

        if (targetType == typeof(Uri))
            return new Uri(s, UriKind.RelativeOrAbsolute);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, s, ignoreCase: true);

        throw new NotSupportedException($"No converter for '{targetType.FullName}' from string value '{s}'.");
    }

    private static void AddChild(AvaloniaObject parent, AvaloniaObject child)
    {
        // Panels
        if (parent is Panel panel && child is Control c1)
        {
            panel.Children.Add(c1);
            return;
        }

        // Border
        if (parent is Border border && child is Control c2)
        {
            if (border.Child is not null)
                throw new FormatException("Border can only have one child.");

            border.Child = c2;
            return;
        }

        // ContentControl (Button)
        if (parent is ContentControl cc)
        {
            if (cc.Content is not null)
                throw new FormatException($"{parent.GetType().Name} can only have one content.");

            cc.Content = child;
            return;
        }

        throw new FormatException($"'{parent.GetType().Name}' cannot contain '{child.GetType().Name}'.");
    }

    private static void ApplyText(AvaloniaObject current, string text)
    {
        if (current is TextBlock tb)
        {
            tb.Text = tb.Text is null ? text : tb.Text + text;
            return;
        }

        if (current is ContentControl cc)
        {
            if (cc.Content is null)
            {
                cc.Content = text;
                return;
            }

            throw new FormatException($"{current.GetType().Name} already has Content; cannot add text node.");
        }

        throw new FormatException($"Text content is not supported for '{current.GetType().Name}'.");
    }
}

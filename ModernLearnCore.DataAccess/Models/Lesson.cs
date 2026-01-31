using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace ModernLearnCore.DataAccess.Models;

public sealed record class Lesson(
    Guid Id,
    string Xaml
) : IModel
{
    public string? Name => GetAttribute(nameof(Name));

    public string? Description => GetAttribute(nameof(Description));


    private string? GetAttribute(string name) => XDocument.Parse(Xaml).Root?.Attribute(name)?.Value;
}
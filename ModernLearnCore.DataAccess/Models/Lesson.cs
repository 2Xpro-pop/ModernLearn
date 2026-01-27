using System;
using System.Collections.Generic;
using System.Text;

namespace ModernLearnCore.DataAccess.Models;

public sealed record class Lesson(
    Guid Id,
    string Xaml
) : IModel;
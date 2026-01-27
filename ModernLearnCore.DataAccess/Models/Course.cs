using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace ModernLearnCore.DataAccess.Models;

public sealed record Course(Guid Id, string Name, string Description, string ImageName, ImmutableArray<Guid> LessonIds) : IModel;
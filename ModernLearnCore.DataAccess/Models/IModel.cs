using System;
using System.Collections.Generic;
using System.Text;

namespace ModernLearnCore.DataAccess.Models;

public interface IModel
{
    public Guid Id
    {
        get;
    }
}

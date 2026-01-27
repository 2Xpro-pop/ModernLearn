using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ModernLearn.ViewModels;

public interface IInitializableViewModel
{
    ValueTask InitializeAsync();
}
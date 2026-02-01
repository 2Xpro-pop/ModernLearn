using ModernLearn.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ModernLearn.ViewModels;

public sealed class FileVm: INotifyPropertyChanged
{
    private readonly File _file;

    public FileVm(File file)
    {
        _file = file;
    }


    public string FilePath => _file.FilePath;
    public string Language => _file.Language;
    public string Text => _file.Text;

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add
        {
            ((INotifyPropertyChanged)_file).PropertyChanged += value;
        }

        remove
        {
            ((INotifyPropertyChanged)_file).PropertyChanged -= value;
        }
    }
}

using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Metadata;
using DynamicData;
using DynamicData.Binding;
using ModernLearn.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;

namespace ModernLearn.Controls;

[TemplatePart(Name = "PART_FileHost", Type = typeof(ContentPresenter))]
[TemplatePart(Name = "PART_Button", Type = typeof(Button))]
[TemplatePart(Name = "PART_Files", Type = typeof(ItemsControl))]
public sealed class Examine : TemplatedControl
{
    public static readonly DirectProperty<Examine, ObservableCollection<File>> FilesProperty =
        AvaloniaProperty.RegisterDirect<Examine, ObservableCollection<File>>(
            nameof(Files),
            o => o.Files);

    public static readonly StyledProperty<File?> SelectedFileProperty =
        AvaloniaProperty.Register<Examine, File?>(nameof(SelectedFile));


    private ItemsControl? _filesControl;

    public Examine()
    {
        Files.CollectionChanged += ChildrenChanged;

        ResourcesChanged += OnResourcesChanged;
    }

    private void OnResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        SetFiles();
    }

    [Content]
    public ObservableCollection<File> Files
    {
        get;
    } = [];


    public File? SelectedFile
    {
        get => GetValue(SelectedFileProperty);
        set => SetValue(SelectedFileProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _filesControl = e.NameScope.Find<ItemsControl>("PART_Files");
        SetFiles();
    }

    private void SetFiles()
    {
        if(_filesControl is null)
        {
            return;
        }

        var buttons = Files.Select(file =>
        {
            var template = App.Current!.Resources.TryGetResource("ExamineFileButton", null, out var value) ? value as DataTemplate: null;

            var button = new Button
            {
                DataContext = file,
                Command = ReactiveCommand.Create(() => SelectedFile = file),
                Content = template?.Build(file)
            };

            return button;
        }).ToImmutableArray();

        _filesControl.ItemsSource = buttons;
    }

    private void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        SetFiles();
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                LogicalChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Control>().ToList());
                VisualChildren.InsertRange(e.NewStartingIndex, e.NewItems!.OfType<Visual>());
                break;

            case NotifyCollectionChangedAction.Move:
                LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                VisualChildren.MoveRange(e.OldStartingIndex, e.OldItems!.Count, e.NewStartingIndex);
                break;

            case NotifyCollectionChangedAction.Remove:
                LogicalChildren.RemoveAll(e.OldItems!.OfType<Control>().ToList());
                VisualChildren.RemoveAll(e.OldItems!.OfType<Visual>());
                break;

            case NotifyCollectionChangedAction.Replace:
                for (var i = 0; i < e.OldItems!.Count; ++i)
                {
                    var index = i + e.OldStartingIndex;
                    var child = (Control)e.NewItems![i]!;
                    LogicalChildren[index] = child;
                    VisualChildren[index] = child;
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                throw new NotSupportedException();
        }
    }
}
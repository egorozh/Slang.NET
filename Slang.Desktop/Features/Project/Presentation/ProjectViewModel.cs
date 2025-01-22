using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Slang.Desktop.Features.Project.Presentation;

public class ProjectViewModel : ObservableObject
{
    private readonly CultureInfo[] _cultures =
    [
        CultureInfo.GetCultureInfo("ru-RU"),
        CultureInfo.GetCultureInfo("en-US"),
        CultureInfo.GetCultureInfo("de-DE")
    ];
    
    private readonly ObservableCollection<NodeViewModel> _nodes;

    public HierarchicalTreeDataGridSource<NodeViewModel> Source { get; }

    public ProjectViewModel()
    {
        _nodes =
        [
            new()
            {
                Key = "main",
                Children =
                {
                    new NodeViewModel
                    {
                        Key = "open_project",
                        Cultures =
                        [
                            new CultureValue { Culture = _cultures[0], Value = "Открыть проект" },
                            new CultureValue { Culture = _cultures[1], Value = "Open project" },
                            new CultureValue { Culture = _cultures[2], Value = "Projekt öffnen" }
                        ]
                    },
                }
            }
        ];

        Source = new HierarchicalTreeDataGridSource<NodeViewModel>(_nodes);
        Source.Columns.Add(
            new HierarchicalExpanderColumn<NodeViewModel>(
                new TextColumn<NodeViewModel, string>("Key", x => x.Key),
                x => x.Children)
        );
        Source.Columns.AddRange(_cultures.Select(x => new TextColumn<NodeViewModel, string>(
            header: x.TwoLetterISOLanguageName,
            getter: n => n.Cultures.First(y => y.Culture == x).Value)));
    }
}

public class NodeViewModel
{
    public string? Key { get; set; }

    public CultureValue[] Cultures { get; set; }

    public ObservableCollection<NodeViewModel> Children { get; } = new();
}

public class CultureValue
{
    public CultureInfo Culture { get; set; }

    public string? Value { get; set; }
}
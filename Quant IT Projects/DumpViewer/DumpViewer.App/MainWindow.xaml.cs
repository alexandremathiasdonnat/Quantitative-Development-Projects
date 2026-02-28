using System.Windows;
using DumpViewer.Core.Services;
using DumpViewer.Core.ViewModels;
using Microsoft.Win32;

namespace DumpViewer.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel(new JsonDumpLoader());
        DataContext = _vm;
    }

    private async void LoadDump_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select a pricing dump (JSON)",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            InitialDirectory = System.Environment.CurrentDirectory
        };

        if (dialog.ShowDialog() != true)
            return;

        await _vm.LoadFromPathAsync(dialog.FileName);
    }
}

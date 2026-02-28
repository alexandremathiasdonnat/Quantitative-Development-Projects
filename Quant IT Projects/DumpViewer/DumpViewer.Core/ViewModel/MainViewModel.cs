using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DumpViewer.Core.Models;
using DumpViewer.Core.Services;

namespace DumpViewer.Core.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly IDumpLoader _dumpLoader;

    private PricingDump? _dump;
    private bool _isLoading;
    private string _statusMessage = "Ready.";
    private string? _lastLoadedPath;

    public event PropertyChangedEventHandler? PropertyChanged;

    public PricingDump? Dump
    {
        get => _dump;
        private set
        {
            _dump = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProductId));
            OnPropertyChanged(nameof(RunTime));
            OnPropertyChanged(nameof(Price));
        }
    }

    public string ProductId => Dump?.ProductId ?? "-";
    public string RunTime => Dump?.RunTime.ToString("u") ?? "-";
    public string Price => Dump is null ? "-" : Dump.Price.ToString("0.########");

    public ObservableCollection<KeyValuePair<string, decimal>> Greeks { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set { _statusMessage = value; OnPropertyChanged(); }
    }

    public string? LastLoadedPath
    {
        get => _lastLoadedPath;
        private set { _lastLoadedPath = value; OnPropertyChanged(); }
    }

    public MainViewModel(IDumpLoader dumpLoader)
    {
        _dumpLoader = dumpLoader ?? throw new ArgumentNullException(nameof(dumpLoader));
    }

    // Testable logic: no UI dependency
    public async Task LoadFromPathAsync(string path)
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading dump...";

            var dump = await _dumpLoader.LoadAsync(path);
            Dump = dump;
            LastLoadedPath = path;

            Greeks.Clear();
            foreach (var kv in dump.Greeks)
                Greeks.Add(kv);

            StatusMessage = $"Loaded: {dump.ProductId}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

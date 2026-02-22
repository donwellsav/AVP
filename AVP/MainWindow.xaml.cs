using AVP.Services;
using AVP.ViewModels;
using System;
using System.Windows;

namespace AVP;

public partial class MainWindow : Window
{
    private readonly IWindowService _windowService;

    public MainWindow(MainViewModel viewModel, IWindowService windowService)
    {
        InitializeComponent();
        DataContext = viewModel;
        _windowService = windowService;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _windowService.CloseVideoWindow();
    }
}

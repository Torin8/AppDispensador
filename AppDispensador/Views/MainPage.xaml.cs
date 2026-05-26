using Microsoft.Maui.Controls;
using AppDispensador.ViewModels;

namespace AppDispensador.Views;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public MainPage()
    {
        InitializeComponent();
#if WINDOWS || ANDROID || IOS || MACCATALYST
        _viewModel = IPlatformApplication.Current?.Services.GetService<MainViewModel>();
        BindingContext = _viewModel;
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Ejecuta la recarga de la base de datos de manera limpia al visualizar la pantalla
        if (_viewModel != null)
        {
            await _viewModel.LoadSchedulesAsync();
        }
    }
}

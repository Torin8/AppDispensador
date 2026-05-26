using AppDispensador.ViewModels;
using Microsoft.Maui.Controls;
namespace AppDispensador.Views;


public partial class AddSchedulePage : ContentPage
{
	public AddSchedulePage(AddScheduleViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}
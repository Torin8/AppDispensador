using AppDispensador.Views;
namespace AppDispensador
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // Registro de la ruta para la navegación
            Routing.RegisterRoute(nameof(AddSchedulePage), typeof(AddSchedulePage));
        }
    }
}

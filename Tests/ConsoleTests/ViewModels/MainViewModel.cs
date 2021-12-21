using ConsoleTests.ViewModels.Base;

namespace ConsoleTests.ViewModels;

[Service(ServiceLifetime.Singleton)]
internal class MainViewModel : ViewModel
{
    [Inject]
    public IUserDialog UI { get; set; }

    //[Inject]
    //private void Initialize(IUserDialog UI) => this.UI = UI;

    //public MainWindowViewModel(IUserDialog UI) => this.UI = UI;
}
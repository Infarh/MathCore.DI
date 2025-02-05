namespace ConsoleTests.Interfaces;

[Service(ServiceLifetime.Singleton, Implementation = typeof(UserDialogService))]
public interface IUserDialog
{
        
}
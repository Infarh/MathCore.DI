namespace ConsoleTests.Services;

[Service(ServiceLifetime.Transient, Interface = typeof(IUserDialog))]
public class UserDialogService : IUserDialog
{

}
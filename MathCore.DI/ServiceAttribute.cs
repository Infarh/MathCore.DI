namespace MathCore.DI;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public class ServiceAttribute : Attribute
{
    public ServiceLifetime Mode { get; set; } = ServiceLifetime.Transient;

    private Type? _Implementation;

    public Type? Implementation
    {
        get => _Implementation;
        set
        {
            if (_Interface is not null)
                throw new InvalidOperationException("Попытка установить реализацию сервиса при уже указанном значении интерфейса. Реализацией сервиса должен являться класс, к которому применяется данный атрибут.");
            _Implementation = value;
        }
    }

    private Type? _Interface;

    public Type? Interface
    {
        get => _Interface;
        set
        {
            if(_Implementation is not null)
                throw new InvalidOperationException("Попытка установить интерфейс при уже указанной реализации. Интерфейсом должен являться тип, к которому применяется данный интерфейс");
            _Interface = value;
        }
    }

    public ServiceAttribute() { }

    public ServiceAttribute(ServiceLifetime Mode) => this.Mode = Mode;

    public void Deconstruct(out Type? Implementation, out ServiceLifetime Mode)
    {
        Implementation = this.Implementation;
        Mode = this.Mode;
    }

    public void Deconstruct(out Type? Service, out Type? Implementation, out ServiceLifetime Mode)
    {
        Service = _Interface;
        Implementation = _Implementation;
        Mode = this.Mode;
    }
}
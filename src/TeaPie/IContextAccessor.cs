namespace TeaPie;

internal interface IContextAccessor<T>
{
    T? Context { get; set; }
}

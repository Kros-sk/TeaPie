namespace TeaPie;

internal interface IRegistry<TElement>
{
    void Register(string name, TElement element);

    TElement Get(string name);

    bool IsRegistered(string name);
}

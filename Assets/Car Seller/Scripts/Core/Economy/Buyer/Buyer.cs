public class Buyer : ILocatable
{
    public string Name { get; private set; }
    public Buyer(string name)
    {
        Name = name;
    }
}
public class Scope
{
    public Dictionary<string, string> Vars { get; } = new Dictionary<string, string>();
    public Scope? Parent { get; }

    public Scope(Scope? parent = null)
    {
        Parent = parent;
    }

    public void Declare(string name, string type)
    {
        if (Vars.ContainsKey(name))
        {
            throw new Exception($"Variable '{name}' ya declarada en este ámbito.");
        }
        Vars[name] = type;
    }

    public string? Resolve(string name)
    {
        if (Vars.TryGetValue(name, out var type))
        {
            return type;
        }
        return Parent?.Resolve(name);
    }
}

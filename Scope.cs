public class Scope
{
    public Dictionary<string, string> Vars { get; } = new Dictionary<string, string>();
    public Scope? Parent {get;}

    public Scope(Scope? parent = null)
    {
        Parent = parent;
    }

    public void Declare(string name, string type)
    {
        if (ContainsVar(name) && Vars[name] != type)
            throw new Exception("El tipo de la variable no coincide con el ya existente");
        
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

    public bool ContainsVar(string name)
    {
        if(Vars.ContainsKey(name))
            return true;
        if(Parent == null)
            return false;
        
        return Parent.ContainsVar(name);         
    }
}

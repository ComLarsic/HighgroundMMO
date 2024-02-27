using NLua;
using NLua.Exceptions;

namespace HGScript;

/// <summary>
/// A loaded lua script.
/// </summary>
public class Script
{
    /// <summary>
    /// The id of the script.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The name of the script.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The code of the script.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// The lua state of the script.
    /// </summary>
    public Lua State { get; set; }

    /// <summary>
    /// Create a new script.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="code"></param>
    public Script(string name, string code)
    {
        Name = name;
        Code = code;

        try
        {
            // Create a new lua state.
            State = new Lua();
            State.LoadCLRPackage();
            // Load the script.
            State.DoString(Code);
        }
        catch (LuaException e)
        {
            Console.WriteLine($"Error loading script {Name}: {e.Message}");
            State = new Lua();
        }
    }

    /// <summary>
    /// Load a script from a file.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The loaded script.</returns>
    public static Script Load(string path, List<string> libraries)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var code = File.ReadAllText(path);
        var moddedCode = LibrariesToImport(libraries) + code;
        return new Script(name, moddedCode);
    }

    /// <summary>
    /// Call a function in the script.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="args"></param>
    public void Call(string function, params object[] args)
    {
        try
        {
            var f = State.GetFunction(function);
            f?.Call(args);
        }
        catch (LuaException e)
        {
            Console.WriteLine($"Error calling function {function} in script {Name}: {e.Message}");
        }
    }

    /// <summary>
    /// Call a function in the script.
    /// </summary>
    /// <param name="function"></param>
    /// <param name="args"></param>
    public T? Call<T>(string function, params object[] args) where T : class
    {
        try
        {
            var f = State.GetFunction(function);
            if (f == null)
                return null;
            return (T?)f.Call(args).First();
        }
        catch (LuaException e)
        {
            Console.WriteLine($"Error calling function {function} in script {Name}: {e.Message}");
            return null;
        }
    }


    /// <summary>
    /// Dispose of the script.
    /// </summary>
    public void Dispose() => State.Dispose();

    /// <summary>
    /// Convert the libraries to a string of import statements.
    /// </summary>
    private static string LibrariesToImport(List<string> libraries)
    {
        var imports = "";
        foreach (var library in libraries)
        {
            imports += $"import (\"{library}\")\n";
        }
        return imports;
    }
}


namespace HGScript;

/// <summary>
/// The manager for the scripts.
/// </summary>
public class ScriptManager
{
    public Dictionary<string, Script> Scripts { get; } = [];

    public List<string> Libraries { get; } = [];

    /// <summary>
    /// Add a script
    /// </summary>
    /// <param name="script"></param>
    public void AddScript(Script script)
        => Scripts[script.Name] = script;

    /// <summary>
    /// Get a script
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    public Script? GetScript(string scriptId)
        => Scripts[scriptId];

    /// <summary>
    /// Check if the manager has a script with the provided name
    /// </summary>
    /// <param name="scriptId"></param>
    /// <returns></returns>
    public bool HasScript(string scriptId)
        => Scripts.ContainsKey(scriptId.ToString());

    /// <summary>
    /// Load a script
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Script LoadScript(string path)
    {
        var script = Script.Load(path, Libraries);
        AddScript(script);
        return script;
    }

    /// <summary>
    /// Remove a script
    /// </summary>
    /// <param name="scriptId"></param>
    public void RemoveScript(string scriptId)
        => Scripts.Remove(scriptId.ToString());


    /// <summary>
    /// Call a method on all scripts
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    public void CallMethodOnAllScripts(string methodName, params object[] args)
    {
        foreach (var script in Scripts.Values)
        {
            script.Call(methodName, args);
        }
    }

    /// <summary>
    /// Call a method on all scripts
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="args"></param>
    public List<T?> CallMethodOnAllScripts<T>(string methodName, params object[] args) where T : class
    {
        var results = new List<T?>();
        foreach (var script in Scripts.Values)
        {
            var result = script.Call<T>(methodName, args);
            results.Add(result);
        }
        return results;
    }
}

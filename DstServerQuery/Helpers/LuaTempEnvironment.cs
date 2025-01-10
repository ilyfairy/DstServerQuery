using System.Collections.Concurrent;
using System.Diagnostics;
using MoonSharp.Interpreter;

namespace DstServerQuery.Helpers;

public class LuaTempEnvironment
{
    public static LuaTempEnvironment Instance { get; set; }
    private readonly BlockingCollection<Script> _scripts = new();

    static LuaTempEnvironment()
    {
        Instance = new(5);
    }

    private LuaTempEnvironment(int initialSize)
    {
        for (int i = 0; i < initialSize; i++)
        {
            _scripts.Add(new Script(CoreModules.None, fastStackSize: 4096)
            {
                DebuggerEnabled = false,
            });
        }
    }

    public DynValue DoChunk(ReadOnlyMemory<char> code)
    {
        var script = _scripts.Take();
        try
        {
            var result = script.DoStringAndRemoveSource(code, null, string.Empty);
            script.ClearByteCode();
            return result;
        }
#if DEBUG
        catch (Exception ex)
        {
            Debugger.Break();
            return null!;
        }
#endif
        finally
        {
            _scripts.Add(script);
        }
    }
}

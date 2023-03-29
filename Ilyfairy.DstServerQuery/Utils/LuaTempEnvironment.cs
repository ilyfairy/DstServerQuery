using Neo.IronLua;
using System;
using System.Collections.Generic;

namespace Ilyfairy.DstServerQuery.Utils;

public class LuaTempEnvironment
{
    private static readonly Lazy<LuaTempEnvironment> laziedInstance;
    private readonly Lazy<LuaGlobal> laziedLuaGlobal;

    public static LuaTempEnvironment Instance => laziedInstance.Value;
    
    private readonly Lua lua = new();
    private LuaGlobal luaGlobal => laziedLuaGlobal.Value;


    static LuaTempEnvironment()
    {
        laziedInstance = new Lazy<LuaTempEnvironment>(() => new LuaTempEnvironment());
    }
    private LuaTempEnvironment()
    {
        laziedLuaGlobal = new Lazy<LuaGlobal>(() => lua.CreateEnvironment());
    }


    public LuaResult DoChunk(string code, string name)
    {
        lock (this)
        {
            LuaResult rst = luaGlobal.DoChunk(code, name);
            luaGlobal.Members.Clear();
            return rst;
        }
    }
}

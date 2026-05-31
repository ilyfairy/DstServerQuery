using System.Diagnostics;
using MoonSharp.Interpreter;

namespace DstDownloaders.Helpers;

public static class LuaConverter
{
    private static readonly Dictionary<int, object> _int32Cache = new();
    private static readonly HashSet<string> _stringCache = new();

    private static readonly object _true = true, _false = false;

    static LuaConverter()
    {
        for (int i = 0; i < 1000; i++)
        {
            _int32Cache[i] = i;
        }
        for (int i = 1000; i < 10000; i += 100)
        {
            _int32Cache[i] = i;
        }
        for (int i = 10000; i <= 100000; i += 10000)
        {
            _int32Cache[i] = i;
        }

        for (int i = 0; i < 1000; i++)
        {
            _stringCache.Add(i.ToString());
        }
        for (int i = 1000; i < 10000; i += 100)
        {
            _stringCache.Add(i.ToString());
        }
        for (int i = 10000; i <= 100000; i += 10000)
        {
            _stringCache.Add(i.ToString());
        }
        //foreach (var item in (ReadOnlySpan<string>)[
        //    string.Empty,
        //    "auto", "Auto", "AUTO",
        //    "Yes", "No", "yes", "no", "YES", "NO",
        //    "image/png", "client_only_mod", "utility", "tweak",
        //    "Disabled", "Enabled", 
        //    ])
        //{
        //    _stringCache.Add(item);
        //}
    }

    public static object? ToClrObject(object luaValue)
    {
        if(luaValue is DynValue dyn)
        {
            luaValue = dyn.ToObject();
        }

        if(luaValue is Table table)
        {
            bool isArray = true;
            int minValue = 1;
            int maxValue = table.Length;
            foreach (var item in table.Keys)
            {
                if(item.IsNumber)
                {
                    minValue = int.Min((int)item.Number, minValue);
                    maxValue = int.Max((int)item.Number, maxValue);
                    break;
                }
                else
                {
                    isArray = false;
                    break;
                }
            }
            if(minValue != 1 || maxValue != table.Length)
            {
                isArray = false;
            }

            if(isArray)
            {
                var list = new List<object?>();
                for (int i = minValue; i <= maxValue; i++)
                {
                    list.Add(ToClrObject(table.Get(i)));
                }
                return list.ToArray();
            }
            else
            {
                var dict = new Dictionary<string, object?>();
                foreach (var item in table.Pairs)
                {
                    if (!item.Key.IsString)
                        continue;

                    dict.Add(item.Key.String, ToClrObject(item.Value));
                }
                return dict;
            }
        }

        if (luaValue is int or double or string or float or bool)
        {
            Debug.Assert(luaValue is double or string or bool);
            return luaValue;
        }

         return null;
    }

    public static object CachePrimitive(object value)
    {
        return value switch
        {
            int i32 => _int32Cache.TryGetValue(i32, out var i32Object) ? i32Object : i32,
            bool b => b ? _true : _false,
            string str => str,
            _ => value,
        };
    }

}

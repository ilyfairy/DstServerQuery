using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DstServerQuery.Web.Helpers.Logging;

public sealed class RuntimeLogContextEnricher : ILogEventEnricher
{
    private const string Unknown = "?";
    private static readonly int ProcessId = Environment.ProcessId;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // 这里运行在日志事件创建阶段。线程 ID 必须在这里取，不能放到 sink 里取；
        // 否则 Async sink 写出日志时会变成后台写入线程的 ID。
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Environment.CurrentManagedThreadId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProcessId", ProcessId));

        var caller = FindCaller();
        // ILogger<T> 会写入 SourceContext，它比调用栈推断更稳定；
        // 静态 Serilog.Log 没有 SourceContext 时，再用调用栈补类型名。
        string typeName = GetSourceContextTypeName(logEvent) ?? caller.TypeName ?? Unknown;
        string methodName = caller.MethodName ?? Unknown;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TypeName", typeName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MethodName", methodName));
    }

    private static (string? TypeName, string? MethodName) FindCaller()
    {
        var stackTrace = new StackTrace(false);
        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            MethodBase? method = stackTrace.GetFrame(i)?.GetMethod();
            if (method?.DeclaringType is not { } declaringType || IsLoggingInfrastructure(declaringType))
            {
                continue;
            }

            // async/iterator 方法在调用栈里通常显示为 MoveNext，需要还原到用户写的原方法。
            MethodBase resolvedMethod = ResolveGeneratedMethod(method);
            Type? resolvedType = GetUserDeclaringType(resolvedMethod.DeclaringType ?? declaringType);
            return (resolvedType is null ? null : GetShortTypeName(resolvedType), CleanGeneratedMethodName(resolvedMethod.Name));
        }

        return (null, null);
    }

    private static bool IsLoggingInfrastructure(Type type)
    {
        string fullName = type.FullName ?? type.Name;
        if (fullName == typeof(RuntimeLogContextEnricher).FullName)
        {
            return true;
        }

        // 跳过日志框架自己的栈帧，直到找到真正发起日志调用的业务代码。
        return fullName.StartsWith("Serilog.", StringComparison.Ordinal)
               || fullName.StartsWith("Microsoft.Extensions.Logging.", StringComparison.Ordinal)
               || fullName.StartsWith("System.Runtime.CompilerServices.", StringComparison.Ordinal);
    }

    private static MethodBase ResolveGeneratedMethod(MethodBase method)
    {
        if (method.Name != "MoveNext" || method.DeclaringType is not { } stateMachineType || stateMachineType.DeclaringType is not { } containingType)
        {
            return method;
        }

        foreach (MethodInfo candidate in containingType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (candidate.GetCustomAttribute<AsyncStateMachineAttribute>()?.StateMachineType == stateMachineType
                || candidate.GetCustomAttribute<IteratorStateMachineAttribute>()?.StateMachineType == stateMachineType)
            {
                return candidate;
            }
        }

        return method;
    }

    private static Type? GetUserDeclaringType(Type type)
    {
        while (type.DeclaringType is not null
               && (type.Name.StartsWith("<>", StringComparison.Ordinal)
                   || type.GetCustomAttribute<CompilerGeneratedAttribute>() is not null))
        {
            type = type.DeclaringType;
        }

        return type;
    }

    private static string? GetSourceContextTypeName(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sourceContextValue)
            && sourceContextValue is ScalarValue { Value: string sourceContext }
            && !string.IsNullOrWhiteSpace(sourceContext))
        {
            return GetShortTypeName(sourceContext);
        }

        return null;
    }

    private static string GetShortTypeName(Type type)
    {
        return GetShortTypeName(type.FullName ?? type.Name);
    }

    private static string GetShortTypeName(string fullName)
    {
        string typeName = fullName.Replace('+', '.');
        int namespaceIndex = typeName.LastIndexOf('.');
        if (namespaceIndex >= 0)
        {
            typeName = typeName[(namespaceIndex + 1)..];
        }

        int genericIndex = typeName.IndexOf('`');
        if (genericIndex >= 0)
        {
            typeName = typeName[..genericIndex];
        }

        return typeName;
    }

    private static string CleanGeneratedMethodName(string methodName)
    {
        if (methodName.StartsWith("<<", StringComparison.Ordinal))
        {
            int end = methodName.IndexOf('>', 2);
            if (end > 2)
            {
                return methodName[2..end].TrimEnd('$');
            }
        }

        if (methodName.StartsWith('<'))
        {
            int end = methodName.IndexOf('>', 1);
            if (end > 1)
            {
                return methodName[1..end].TrimEnd('$');
            }
        }

        return methodName;
    }
}

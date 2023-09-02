using System;
using System.Collections.Generic;

namespace BLiveAPI;

/// <summary>
///     用来标记某个方法想要绑定哪些cmd对应的SMS消息事件
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TargetCmdAttribute : Attribute
{
    private readonly HashSet<string> _targetCmdArray;

    /// <inheritdoc cref="TargetCmdAttribute" />
    public TargetCmdAttribute(params string[] cmdArray)
    {
        _targetCmdArray = new HashSet<string>(cmdArray);
    }

    internal bool HasCmd(string cmd)
    {
        return _targetCmdArray.Contains(cmd);
    }
}
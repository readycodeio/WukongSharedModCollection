using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace WukongMp.Toolkit.Levels;

public static class LevelsEvents
{
    [Experimental("Not properly tested")]
    public static event Action<string>? OnLevelLoaded;
    
    [Experimental("Not properly tested")]
    public static event Action<string>? OnLevelUnloaded;

    
    internal static void InvokeOnLevelLoaded(string levelName)
    {
        OnLevelLoaded?.Invoke(levelName);
    }
    internal static void InvokeLevelUnloaded(string levelName)
    {
        OnLevelUnloaded?.Invoke(levelName);
    }
}

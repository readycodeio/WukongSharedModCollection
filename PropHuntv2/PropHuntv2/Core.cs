using System;
using System.Collections.Concurrent;

namespace WukongMp.PropHunt;

public static class Core
{
    public static GameConfig Config = GameConfig.Default;
    public static GameModeBase? CurrentGameMode { get; set; }

    public static readonly ConcurrentQueue<Action> ExecutionQueue = new();

    public static void InitializeDefaultMode()
    {
        CurrentGameMode = new PropHuntGameMode();
    }

    public static void Update(float deltaTime)
    {
        while (ExecutionQueue.TryDequeue(out var action))
        {
            action?.Invoke();
        }
        CurrentGameMode?.Update(deltaTime);
        CurrentGameMode?.ClientUpdate(deltaTime);
    }

    public enum Team
    {
        Hider,
        Seeker,
        Spectator
    }
}
namespace WukongMp.PropHunt;

public static class Core
{
    public static GameConfig Config = GameConfig.Default;
    public static GameModeBase? CurrentGameMode { get; set; }

    public static void InitializeDefaultMode()
    {
        CurrentGameMode = new PropHuntGameMode();
    }

    public static void Update(float deltaTime)
    {
        CurrentGameMode?.Update(deltaTime);
        CurrentGameMode?.ClientUpdate(deltaTime);
    }

    public enum Team
    {
        Seeker = 1,
        Hider = 2,
        Spectator = -1000,
        None = -999
    }
}
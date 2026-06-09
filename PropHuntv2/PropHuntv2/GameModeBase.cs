namespace WukongMp.PropHunt;

public abstract class GameModeBase
{
    public abstract void Update(float deltaTime);
    public abstract void ClientUpdate(float deltaTime);
    public abstract void OnStart();
    public abstract void OnEnd(Core.Team winnerTeam);
}
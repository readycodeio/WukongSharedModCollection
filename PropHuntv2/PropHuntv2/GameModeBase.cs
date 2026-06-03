namespace WukongMp.PropHunt;

public abstract class GameModeBase
{
    public virtual void Update(float deltaTime) { }
    public virtual void ClientUpdate(float deltaTime) { }

    public virtual void OnStart() { }

    public virtual void OnEnd(Core.Team winnerTeam) { }
}
namespace Game.App.Panels;

/// <summary>
/// Thin command interface for panels to invoke game actions.
/// Panels depend on this interface instead of the parent ViewModel,
/// keeping the dependency direction consistent.
/// </summary>
public interface IGameCommands
{
    void StartGame(string playerName);
    void UsePotion();
    void LaunchExpedition();
}

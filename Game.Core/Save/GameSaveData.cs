using System.Text.Json.Serialization;

namespace Game.Core.Save;

/// <summary>
/// Top-level save data DTO.
/// </summary>
public class GameSaveData
{
    public int SaveFormatVersion { get; set; } = 1;
    public ulong RunSeed { get; set; }
    public uint[] RngState { get; set; } = [];
    public SessionStateData SessionState { get; set; } = new();
    public ModeStateData ModeState { get; set; } = new InDungeonModeData();
    public List<ComponentData> ComponentStates { get; set; } = [];
    public AutomationStateData AutomationState { get; set; } = new();
}

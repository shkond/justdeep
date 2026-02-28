namespace Game.Core.Save;

/// <summary>
/// Automation/auto-play configuration stub DTO.
/// </summary>
public class AutomationStateData
{
    /// <summary>HP threshold ratio (0.0–1.0) below which to retreat.</summary>
    public double RetreatHpThreshold { get; set; } = 0.3;

    /// <summary>Whether the auto-return loop is enabled.</summary>
    public bool AutoReturnEnabled { get; set; } = false;

    /// <summary>Task queue for automation scheduler (stub).</summary>
    public List<string> TaskQueue { get; set; } = [];
}

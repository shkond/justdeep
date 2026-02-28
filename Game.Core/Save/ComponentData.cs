using System.Text.Json.Serialization;

namespace Game.Core.Save;

/// <summary>
/// Polymorphic base class for parallel component state data.
/// Phase 1: no derived types yet. Add [JsonDerivedType] as components are implemented.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$component")]
public abstract class ComponentData { }

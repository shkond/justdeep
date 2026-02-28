using System.IO;
using System.Text.Json;
using Game.Core.Save;

namespace Game.App.Services;

/// <summary>
/// JSON file-based implementation of ISaveStorage.
/// Lives in the App layer to keep Core free of I/O dependencies.
/// </summary>
public class JsonFileSaveStorage : ISaveStorage
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public JsonFileSaveStorage(string filePath)
    {
        _filePath = filePath;
    }

    public void Save(GameSaveData data)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(data, _options);
        File.WriteAllText(_filePath, json);
    }

    public GameSaveData? Load()
    {
        if (!File.Exists(_filePath))
            return null;

        string json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<GameSaveData>(json, _options);
    }
}

using System;
using System.Collections.Generic;
using Game.Core;

namespace Game.App.Panels;

/// <summary>
/// Immutable snapshot of all UI-relevant state, projected from GameSession.
/// Record equality enables automatic diff detection in UiStateStore.
/// </summary>
public record UiState(
    GameState Mode,
    // Player
    string PlayerName,
    int Level,
    int CurrentHp,
    int MaxHp,
    int Attack,
    int Defense,
    int Experience,
    int Gold,
    // Dungeon
    int CurrentFloor,
    int RoomsExplored,
    // Enemy (scalar — avoids UI dependency on Game.Core.Enemy)
    string? EnemyName,
    int? EnemyCurrentHp,
    int? EnemyMaxHp,
    int? EnemyAttack,
    int? EnemyDefense,
    // Log
    IReadOnlyList<string> GameLog
)
{
    /// <summary>Sensible default state for MainMenu before game start.</summary>
    public static UiState Initial { get; } = new(
        Mode: GameState.MainMenu,
        PlayerName: "",
        Level: 1,
        CurrentHp: 100,
        MaxHp: 100,
        Attack: 10,
        Defense: 5,
        Experience: 0,
        Gold: 0,
        CurrentFloor: 1,
        RoomsExplored: 0,
        EnemyName: null,
        EnemyCurrentHp: null,
        EnemyMaxHp: null,
        EnemyAttack: null,
        EnemyDefense: null,
        GameLog: Array.Empty<string>()
    );
}

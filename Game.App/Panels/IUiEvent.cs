using System.Collections.Generic;
using Game.Core;
using Game.Core.States;

namespace Game.App.Panels;

/// <summary>Marker interface for all UI events.</summary>
public interface IUiEvent { }

/// <summary>Fired when the active game mode changes (not every tick).</summary>
public record ModeChangedEvent(GameState OldMode, GameState NewMode) : IUiEvent;

/// <summary>Fired when new log entries are added.</summary>
public record LogAddedEvent(IReadOnlyList<string> NewEntries) : IUiEvent;

/// <summary>Fired when player stats (HP, attack, etc.) change.</summary>
public record StatsChangedEvent(Player Player, Enemy? CurrentEnemy) : IUiEvent;

/// <summary>Fired when the dungeon floor or room count changes.</summary>
public record FloorChangedEvent(int NewFloor, int RoomsExplored) : IUiEvent;

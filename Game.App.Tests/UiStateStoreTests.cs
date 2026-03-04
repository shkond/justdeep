using Game.App.Panels;
using Game.Core;
using Xunit;

namespace Game.App.Tests;

public class UiStateStoreTests
{
    [Fact]
    public void Initial_State_IsMainMenu()
    {
        var store = new UiStateStore();
        Assert.Equal(GameState.MainMenu, store.Current.Mode);
    }

    [Fact]
    public void Subscribe_ReceivesCurrentStateImmediately()
    {
        var store = new UiStateStore();
        UiState? received = null;

        store.Subscribe(s => received = s);

        Assert.NotNull(received);
        Assert.Equal(store.Current, received);
    }

    [Fact]
    public void Update_DifferentState_NotifiesSubscribers()
    {
        var store = new UiStateStore();
        int notifyCount = 0;
        UiState? lastState = null;

        store.Subscribe(s => { notifyCount++; lastState = s; });

        // notifyCount == 1 from initial subscription
        Assert.Equal(1, notifyCount);

        var newState = UiState.Initial with { Mode = GameState.InDungeon };
        store.Update(newState);

        Assert.Equal(2, notifyCount);
        Assert.Equal(GameState.InDungeon, lastState!.Mode);
    }

    [Fact]
    public void Update_SameState_DoesNotNotify()
    {
        var store = new UiStateStore();
        int notifyCount = 0;

        store.Subscribe(s => notifyCount++);
        Assert.Equal(1, notifyCount); // Initial

        // Update with identical state
        store.Update(UiState.Initial);

        Assert.Equal(1, notifyCount); // No additional notification
    }

    [Fact]
    public void Dispose_StopsNotifications()
    {
        var store = new UiStateStore();
        int notifyCount = 0;

        var sub = store.Subscribe(s => notifyCount++);
        Assert.Equal(1, notifyCount); // Initial

        sub.Dispose();

        store.Update(UiState.Initial with { Mode = GameState.InDungeon });
        Assert.Equal(1, notifyCount); // No further notification
    }

    [Fact]
    public void Multiple_Subscribers_AllNotified()
    {
        var store = new UiStateStore();
        int count1 = 0, count2 = 0;

        store.Subscribe(s => count1++);
        store.Subscribe(s => count2++);

        store.Update(UiState.Initial with { Mode = GameState.InDungeon });

        Assert.Equal(2, count1); // Initial + update
        Assert.Equal(2, count2);
    }

    [Fact]
    public void Update_Current_ReflectsNewState()
    {
        var store = new UiStateStore();
        var newState = UiState.Initial with { PlayerName = "テスト" };

        store.Update(newState);

        Assert.Equal("テスト", store.Current.PlayerName);
    }
}

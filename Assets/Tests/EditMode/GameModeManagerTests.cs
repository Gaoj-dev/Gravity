using NUnit.Framework;

public class GameModeManagerTests
{
    [SetUp]
    public void SetUp()
    {
        // Resetear al estado inicial antes de cada test ya que es una clase estatica
        GameModeManager.SetMode(GameMode.Space);
    }

    [Test]
    public void DefaultMode_IsSpace()
    {
        Assert.AreEqual(GameMode.Space, GameModeManager.CurrentMode);
    }

    [Test]
    public void SetMode_ChangesCurrentMode()
    {
        GameModeManager.SetMode(GameMode.Planet);
        Assert.AreEqual(GameMode.Planet, GameModeManager.CurrentMode);
    }

    [Test]
    public void SetMode_DifferentMode_FiresModeChangedEvent()
    {
        bool fired = false;
        GameModeManager.ModeChanged += _ => fired = true;

        GameModeManager.SetMode(GameMode.Planet);

        GameModeManager.ModeChanged -= _ => fired = true;
        Assert.IsTrue(fired);
    }

    [Test]
    public void SetMode_SameMode_DoesNotFireModeChangedEvent()
    {
        int callCount = 0;
        void OnModeChanged(GameMode _) => callCount++;
        GameModeManager.ModeChanged += OnModeChanged;

        GameModeManager.SetMode(GameMode.Space); // mismo modo que el inicial

        GameModeManager.ModeChanged -= OnModeChanged;
        Assert.AreEqual(0, callCount);
    }

    [Test]
    public void SetMode_EventReceivesNewMode()
    {
        GameMode receivedMode = GameMode.Space;
        void OnModeChanged(GameMode m) => receivedMode = m;
        GameModeManager.ModeChanged += OnModeChanged;

        GameModeManager.SetMode(GameMode.Planet);

        GameModeManager.ModeChanged -= OnModeChanged;
        Assert.AreEqual(GameMode.Planet, receivedMode);
    }
}

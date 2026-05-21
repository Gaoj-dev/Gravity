using NUnit.Framework;
using UnityEngine;

public class PlayerGoldWalletTests
{
    private PlayerGoldWallet wallet;

    [SetUp]
    public void SetUp()
    {
        wallet = new GameObject().AddComponent<PlayerGoldWallet>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(wallet.gameObject);
    }

    [Test]
    public void GoldCount_StartsAtZero()
    {
        Assert.AreEqual(0, wallet.GoldCount);
    }

    [Test]
    public void AddGold_PositiveAmount_IncreasesCount()
    {
        wallet.AddGold(10);
        Assert.AreEqual(10, wallet.GoldCount);
    }

    [Test]
    public void AddGold_MultipleCalls_Accumulates()
    {
        wallet.AddGold(3);
        wallet.AddGold(7);
        Assert.AreEqual(10, wallet.GoldCount);
    }

    [Test]
    public void AddGold_ZeroAmount_DoesNotChangeCount()
    {
        wallet.AddGold(0);
        Assert.AreEqual(0, wallet.GoldCount);
    }

    [Test]
    public void AddGold_NegativeAmount_DoesNotChangeCount()
    {
        wallet.AddGold(-5);
        Assert.AreEqual(0, wallet.GoldCount);
    }

    [Test]
    public void AddGold_PositiveAmount_FiresGoldChangedEvent()
    {
        bool fired = false;
        wallet.GoldChanged += () => fired = true;

        wallet.AddGold(1);

        Assert.IsTrue(fired);
    }

    [Test]
    public void AddGold_ZeroAmount_DoesNotFireEvent()
    {
        bool fired = false;
        wallet.GoldChanged += () => fired = true;

        wallet.AddGold(0);

        Assert.IsFalse(fired);
    }

    [Test]
    public void SetGoldCount_PositiveAmount_SetsCount()
    {
        wallet.SetGoldCount(42);
        Assert.AreEqual(42, wallet.GoldCount);
    }

    [Test]
    public void SetGoldCount_NegativeAmount_ClampsToZero()
    {
        wallet.SetGoldCount(-99);
        Assert.AreEqual(0, wallet.GoldCount);
    }

    [Test]
    public void SetGoldCount_FiresGoldChangedEvent()
    {
        bool fired = false;
        wallet.GoldChanged += () => fired = true;

        wallet.SetGoldCount(5);

        Assert.IsTrue(fired);
    }

    [Test]
    public void SetGoldCount_OverwritesPreviousValue()
    {
        wallet.AddGold(50);
        wallet.SetGoldCount(10);
        Assert.AreEqual(10, wallet.GoldCount);
    }
}

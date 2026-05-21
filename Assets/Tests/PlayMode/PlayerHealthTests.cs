using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerHealthTests
{
    private PlayerHealth health;

    [SetUp]
    public void SetUp()
    {
        var go = new GameObject();
        go.AddComponent<Rigidbody2D>();
        health = go.AddComponent<PlayerHealth>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(health.gameObject);
    }

    [UnityTest]
    public IEnumerator CurrentHealth_StartsAtMaxHealth()
    {
        yield return null;
        Assert.AreEqual(health.MaxHealth, health.CurrentHealth);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_ZeroDamage_ReturnsFalse()
    {
        yield return null;
        bool result = health.TryTakeDamage(0, Vector2.zero);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_NegativeDamage_ReturnsFalse()
    {
        yield return null;
        bool result = health.TryTakeDamage(-1, Vector2.zero);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_ValidDamage_ReturnsTrue()
    {
        yield return null;
        bool result = health.TryTakeDamage(1, Vector2.zero);
        Assert.IsTrue(result);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_ValidDamage_ReducesHealth()
    {
        yield return null;
        int maxHp = health.MaxHealth;
        health.TryTakeDamage(1, Vector2.zero);
        Assert.AreEqual(maxHp - 1, health.CurrentHealth);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_ValidDamage_FiresDamagedEvent()
    {
        yield return null;
        bool fired = false;
        health.Damaged += () => fired = true;

        health.TryTakeDamage(1, Vector2.zero);

        Assert.IsTrue(fired);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_LethalDamage_FiresDiedEvent()
    {
        yield return null;
        bool died = false;
        health.Died += () => died = true;

        health.TryTakeDamage(999, Vector2.zero);

        Assert.IsTrue(died);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_LethalDamage_SetsIsDead()
    {
        yield return null;
        health.TryTakeDamage(999, Vector2.zero);
        Assert.IsTrue(health.IsDead);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_WhenDead_ReturnsFalse()
    {
        yield return null;
        health.TryTakeDamage(999, Vector2.zero);
        bool result = health.TryTakeDamage(1, Vector2.zero);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_LethalDamage_FiresDiedOnlyOnce()
    {
        yield return null;
        int count = 0;
        health.Died += () => count++;

        health.TryTakeDamage(999, Vector2.zero);
        health.TryTakeDamage(999, Vector2.zero);

        Assert.AreEqual(1, count);
    }

    [UnityTest]
    public IEnumerator TryTakeDamage_SecondHitWhileInvincible_ReturnsFalse()
    {
        yield return null;
        health.TryTakeDamage(1, Vector2.zero);
        bool result = health.TryTakeDamage(1, Vector2.zero);
        Assert.IsFalse(result);
    }

    [UnityTest]
    public IEnumerator SetCurrentHealth_ClampsAboveMax()
    {
        yield return null;
        health.SetCurrentHealth(999);
        Assert.AreEqual(health.MaxHealth, health.CurrentHealth);
    }

    [UnityTest]
    public IEnumerator SetCurrentHealth_ClampsBelowZero()
    {
        yield return null;
        health.SetCurrentHealth(-1);
        Assert.AreEqual(0, health.CurrentHealth);
    }

    [UnityTest]
    public IEnumerator SetCurrentHealth_ZeroHealth_SetsIsDead()
    {
        yield return null;
        health.SetCurrentHealth(0);
        Assert.IsTrue(health.IsDead);
    }

    [UnityTest]
    public IEnumerator SetCurrentHealth_PositiveHealth_IsNotDead()
    {
        yield return null;
        health.SetCurrentHealth(1);
        Assert.IsFalse(health.IsDead);
    }
}

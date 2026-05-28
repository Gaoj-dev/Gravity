using System;
using UnityEngine;

public class PlayerGoldWallet : MonoBehaviour
{
    public int GoldCount { get; private set; }

    public event Action GoldChanged;

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        GoldCount += amount;
        if (GoldChanged != null) GoldChanged.Invoke();
    }

    public void SetGoldCount(int amount)
    {
        GoldCount = Mathf.Max(0, amount);
        if (GoldChanged != null) GoldChanged.Invoke();
    }
}

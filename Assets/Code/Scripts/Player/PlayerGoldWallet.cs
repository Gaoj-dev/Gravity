using UnityEngine;

public class PlayerGoldWallet : MonoBehaviour
{
    public int GoldCount { get; private set; }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        GoldCount += amount;
    }

    public void SetGoldCount(int amount)
    {
        GoldCount = Mathf.Max(0, amount);
    }
}

using System;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public event Action<int> OnGoldChanged;

    [SerializeField] private int startingGold = 0;

    public int Gold { get; private set; }

    public void Initialize(int overrideStartingGold)
    {
        Gold = Mathf.Max(0, overrideStartingGold);
        NotifyGoldChanged();
    }

    public void InitializeFromInspectorValue()
    {
        Gold = Mathf.Max(0, startingGold);
        NotifyGoldChanged();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Gold += amount;
        NotifyGoldChanged();
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (Gold < amount)
        {
            return false;
        }

        Gold -= amount;
        NotifyGoldChanged();
        return true;
    }

    private void NotifyGoldChanged()
    {
        OnGoldChanged?.Invoke(Gold);
    }
}
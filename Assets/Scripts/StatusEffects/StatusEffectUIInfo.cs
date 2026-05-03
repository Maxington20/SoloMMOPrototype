using UnityEngine;

public readonly struct StatusEffectUIInfo
{
    public readonly StatusEffectData Data;
    public readonly float RemainingTime;
    public readonly float Duration;

    public StatusEffectUIInfo(StatusEffectData data, float remainingTime, float duration)
    {
        Data = data;
        RemainingTime = remainingTime;
        Duration = duration;
    }
}
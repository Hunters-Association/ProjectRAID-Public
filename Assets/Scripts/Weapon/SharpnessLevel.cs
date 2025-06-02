using UnityEngine;

public enum SharpnessLevel
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    White,
    Purple
}

public static class SharpnessHelper
{
    public static float GetMultiplier(SharpnessLevel level)
    {
        return level switch
        {
            SharpnessLevel.Red => 0.50f,
            SharpnessLevel.Orange => 0.75f,
            SharpnessLevel.Yellow => 1.00f,
            SharpnessLevel.Green => 1.05f,
            SharpnessLevel.Blue => 1.20f,
            SharpnessLevel.White => 1.32f,
            SharpnessLevel.Purple => 1.39f,
            _ => 1f
        };
    }
}
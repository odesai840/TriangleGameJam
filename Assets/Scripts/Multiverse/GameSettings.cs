using UnityEngine;

public static class GameSettings
{
    public static int GlobalSeed { get; private set; }

    // Call this once at game start.
    public static void InitializeSeed()
    {
        // You could generate a new random seed, or load from player settings, etc.
        GlobalSeed = Random.Range(int.MinValue, int.MaxValue);
        Debug.Log("Global Seed: " + GlobalSeed);
    }
}

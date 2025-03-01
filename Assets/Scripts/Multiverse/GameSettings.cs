using UnityEngine;

public static class GameSettings
{
    public static int GlobalSeed { get; private set; }

    public static bool[] gotUniverse = new bool[] { false, false, false };

    //at end of universe level, set to 1,2, or 3 if won. set to null if lost. then transition scene to multiverse
    public static int? levelWon = null;

    // Call this once at game start.
    public static void Initialize()
    {
        // You could generate a new random seed, or load from player settings, etc.
        GlobalSeed = Random.Range(int.MinValue, int.MaxValue);
        Debug.Log("Global Seed: " + GlobalSeed);
    }
}

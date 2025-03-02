using UnityEngine;

public static class GameSettings
{
    public static int GlobalSeed { get; private set; }

    public static bool[] gotUniverse = new bool[] { false, false, false };

    //at end of universe level, set to 1,2, or 3 if won. set to null if lost. then transition scene to multiverse
    public static void LevelWon(int levelNum)
    {
        gotUniverse[levelNum] = true;
    }

    public static bool GameWon()
    {
        int count = 0;
        foreach (bool b in gotUniverse)
            if (b)
                count++;
        return count == 3;
    }

    // Call this once at game start.
    public static void Initialize()
    {
        // You could generate a new random seed, or load from player settings, etc.
        GlobalSeed = Random.Range(int.MinValue, int.MaxValue);

        multiverseStartPoint = new Vector3(0, 0, 0);
    }

    //used for positioning ship when entering the multiverse
    public static Vector3 multiverseStartPoint;
}

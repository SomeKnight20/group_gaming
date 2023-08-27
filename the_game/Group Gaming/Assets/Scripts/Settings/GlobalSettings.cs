using UnityEngine;

[CreateAssetMenu(menuName = "GameSettings/GlobalSettings")]
public class GlobalSettings : ScriptableObject
{
    [Tooltip("Seed that the world generation uses")]
    public int globalSeed = 0;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureSpawnController : ScriptableObject
{
    [Header("Structure")]
    [Tooltip("What structure to spawn")]
    Structure structure;

    public virtual void GenerateStructureAt(int x, int y, Biome biome)
    {

    }
}

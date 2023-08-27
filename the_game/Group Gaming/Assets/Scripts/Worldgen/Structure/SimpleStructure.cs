using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Generator;

[CreateAssetMenu(menuName = "Worldgen/Structure/SimpleStructure")]
public class SimpleStructure : Structure
{
    [Tooltip("Filename of the JSON-file containing all tiledata that is related to this structure. NOTE: Don't include extension. It is assumed to be a JSON-file")]
    public string tileDataFilename = "";

    public override void LoadTileData()
    {
        this.tiles = dataManager.LoadStructureTiles(tileDataFilename + ".json");
    }
}

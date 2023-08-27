using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Generator;

[CreateAssetMenu(menuName = "Worldgen/Structure/StructureDataManager")]
public class StructureDataManager : ScriptableObject
{
    public TileAtlas tileAtlas;

    private class StructureTileIdsData
    {
        public List<List<int>> tileIds = new List<List<int>>();
    }

    public List<string> pathToStructures = new List<string>();

    public Dictionary<Coord, TileAtlasTile> LoadStructureTiles(string filename)
    {
        Dictionary<Coord, TileAtlasTile> tiles = new Dictionary<Coord, TileAtlasTile>();

        // Make sure file exists
        string fullPath = Path.Join(GetStructureDataPath(), filename);
        if (!File.Exists(fullPath))
        {
            return tiles;
        }

        // Read the file
        using (StreamReader reader = new StreamReader(fullPath))
        {
            // Get the data
            string data = reader.ReadToEnd();

            // Read and assign the data
            StructureTileIdsData structureTileIdsDataClass = new StructureTileIdsData();
            JsonUtility.FromJsonOverwrite(data, structureTileIdsDataClass);

            // Loop each coordinate and add the tile to a list
            for (int x = 0; x < structureTileIdsDataClass.tileIds.Count; x++)
            {
                for (int y = 0; y < structureTileIdsDataClass.tileIds[x].Count; y++)
                {
                    int tileId = structureTileIdsDataClass.tileIds[x][y];
                    TileAtlasTile tile = tileAtlas.GetTileWithID(tileId);
                    tiles.Add(new Coord(x, y), tile);
                }
            }
        }

        return tiles;
    }
    string GetStructureDataPath()
    {
        string finalPath = Application.dataPath;
        foreach (string path in pathToStructures)
        {
            finalPath = Path.Join(finalPath, path);
        }
        return finalPath;
    }
}

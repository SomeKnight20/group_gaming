using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class test2 : MonoBehaviour
{
    public Biome biome;

    public int width = 100;
    public int height = 300;

    Thread backgroundThread = null;

    public Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
        GenerateWorld();
        biome.SetDefaultTilemap(tilemap);
    }

    private void OnApplicationQuit()
    {
        if (backgroundThread != null)
        {
            backgroundThread.Abort();
        }
        biome.GetGenerator().ResetMap();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GenerateWorld();
        }

        if (Input.GetMouseButtonDown(1))
        {
            biome.CreateMapFromArea(width, 0, 100, 100);
            biome.ProcessMap(width, 0, 100, 100);
            biome.FillTilemap(width, 0, 100, 100);
        }
    }

    void GenerateWorld()
    {
        biome.ResetMap();

        biome.CreateMapFromArea(0, 0, width, height);
        biome.ProcessMap(0, 0, width, height);
        biome.FillTilemap(width, 0, 100, 100);
    }

    private void OnDrawGizmos()
    {
        if (backgroundThread == null || backgroundThread.IsAlive)
        {
            return;
        }
        foreach (KeyValuePair<Generator.Coord, TileType> tile in biome.GetGenerator().map)
        {
            Gizmos.color = (tile.Value == TileType.SOLID) ? Color.black : Color.white;
            Vector3 pos = new Vector3(tile.Key.tileX, tile.Key.tileY, 0);
            Gizmos.DrawCube(pos, Vector3.one);
        }
    }
}

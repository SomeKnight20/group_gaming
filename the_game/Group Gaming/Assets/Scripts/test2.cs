using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class test2 : MonoBehaviour
{
    public Generator generator;

    public int width = 100;
    public int height = 300;

    Thread backgroundThread = null;

    // Start is called before the first frame update
    void Start()
    {
        GenerateWorld();
    }

    private void OnApplicationQuit()
    {
        generator.ResetMap();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GenerateWorld();
        }

        if (Input.GetMouseButtonDown(1))
        {
            backgroundThread = new Thread(() => { generator.CreateMapFromArea(width, 0, 100, 100); });
            backgroundThread.Start();
        }
    }

    void GenerateWorld()
    {
        generator.ResetMap();

        // Start a new thread
        backgroundThread = new Thread(() =>  { generator.CreateMapFromArea(0, 0, width, height); });
        backgroundThread.Start();
    }

    private void OnDrawGizmos()
    {
        if (backgroundThread.IsAlive)
        {
            return;
        }
        foreach(KeyValuePair<Generator.Coord, int> tile in generator.map)
        {
            Gizmos.color = (tile.Value == 1) ? Color.black : Color.white;
            Vector3 pos = new Vector3(-width / 2 + tile.Key.tileX + .5f, -height / 2 + tile.Key.tileY + .5f, 0);
            Gizmos.DrawCube(pos, Vector3.one);
        }
    }
}

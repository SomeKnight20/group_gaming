using System;
using System.Collections.Generic;
using UnityEngine;
using static Generator;

[CreateAssetMenu(menuName = "Worldgen/Biome/DefaultCaveBiome")]
public class DefaultCaveBiome : Biome
{
    [Serializable]
    public struct DecorativeTile
    {
        public TileAtlasTile tile;
        [Tooltip("1 = Default")]
        public float spawnChanceWeight;
    }

    [Header("Default Tiles")]
    public TileAtlasTile stone;
    public List<DecorativeTile> groundDecorationList;
    public List<DecorativeTile> ceilingDecorationList;

    // Ground
    protected float allGroundDecorationChanceWeight = 0;
    protected Dictionary<DecorativeTile, float> groundDecorationChanceMap;
    System.Random groundDecorationRandom;

    // Ceiling
    protected float allCeilingDecorationChanceWeight = 0;
    protected Dictionary<DecorativeTile, float> ceilingDecorationChanceMap;
    System.Random ceilingDecorationRandom;

    [Header("Noises")]
    [Tooltip("Controls plant areas")]
    public Noise groundDecorationNoise;
    public Noise ceilingDecorationNoise;

    public override void OnStart()
    {
        base.OnStart();

        groundDecorationRandom = new System.Random(settings.globalSeed);
        ceilingDecorationRandom = new System.Random(settings.globalSeed);

        CalculateGroundDecorationSpawnChances();
        CalculateCeilingDecorationSpawnChances();
    }

    protected void CalculateGroundDecorationSpawnChances()
    {
        allGroundDecorationChanceWeight = 0f;
        groundDecorationChanceMap = new Dictionary<DecorativeTile, float>();

        foreach (DecorativeTile decoration in groundDecorationList)
        {
            allGroundDecorationChanceWeight += decoration.spawnChanceWeight;
        }

        // Sort deocrations so in theory they take less computing power
        groundDecorationList.Sort(
            delegate (DecorativeTile t1, DecorativeTile t2)
            {
                return t2.spawnChanceWeight.CompareTo(t1.spawnChanceWeight);
            }
        );

        // Calculate true decoration chances
        float chance = 0;
        foreach (DecorativeTile decoration in groundDecorationList)
        {
            chance += decoration.spawnChanceWeight / allGroundDecorationChanceWeight;
            groundDecorationChanceMap[decoration] = chance;
        }
    }
    protected void CalculateCeilingDecorationSpawnChances()
    {
        allCeilingDecorationChanceWeight = 0f;
        ceilingDecorationChanceMap = new Dictionary<DecorativeTile, float>();

        foreach (DecorativeTile decoration in ceilingDecorationList)
        {
            allCeilingDecorationChanceWeight += decoration.spawnChanceWeight;
        }

        // Sort deocrations so in theory they take less computing power
        ceilingDecorationList.Sort(
            delegate (DecorativeTile t1, DecorativeTile t2)
            {
                return t2.spawnChanceWeight.CompareTo(t1.spawnChanceWeight);
            }
        );

        // Calculate true decoration chances
        float chance = 0;
        foreach (DecorativeTile decoration in ceilingDecorationList)
        {
            chance += decoration.spawnChanceWeight / allCeilingDecorationChanceWeight;
            ceilingDecorationChanceMap[decoration] = chance;
        }
    }


    public override void ResetMap()
    {
        base.ResetMap();
    }

    public override void ProcessMap(int startX, int startY, int width, int height)
    {
        base.ProcessMap(startX, startY, width, height);

        Dictionary<Coord, TileType> map = generator.GetMapData();

        foreach (KeyValuePair<Coord, TileType> tile in map)
        {
            Coord coord = tile.Key;
            TileType tileType = tile.Value;

            switch (tileType)
            {
                case TileType.SOLID:
                    {
                        this.tilemapData.Add(coord, stone);
                        break;
                    }
                case TileType.AIR:
                    {
                        AddCeilingDecorationAt(coord.tileX, coord.tileY);
                        AddGroundDecorationAt(coord.tileX, coord.tileY);
                        break;
                    }
            }
        }
    }
    protected void AddCeilingDecorationAt(int x, int y)
    {
        if (ceilingDecorationNoise == null)
        {
            return;
        }
        // If there is a solid tile below, add decorations like plants etc.
        if (!generator.TileExistsAt(new Coord(x, y + 1)))
        {
            return;
        }
        // If there is no ground decoration spot here
        if (ceilingDecorationNoise.GenerateNoiseAt(x, y) == 0)
        {
            return;
        }

        float minChance;
        foreach (KeyValuePair<DecorativeTile, float> kvp in ceilingDecorationChanceMap)
        {
            minChance = (float)ceilingDecorationRandom.NextDouble();

            if (minChance <= kvp.Value)
            {
                if (kvp.Key.tile == null)
                {
                    return;
                }

                this.tilemapData.Add(new Coord(x, y), kvp.Key.tile);
                break;
            }
        }
    }

    protected void AddGroundDecorationAt(int x, int y)
    {
        if (groundDecorationNoise == null)
        {
            return;
        }
        // If there is a solid tile below, add decorations like plants etc.
        if (!generator.TileExistsAt(new Coord(x, y - 1)))
        {
            return;
        }
        // If there is no ground decoration spot here
        if (groundDecorationNoise.GenerateNoiseAt(x, y) == 0)
        {
            return;
        }

        float minChance;
        foreach (KeyValuePair<DecorativeTile, float> kvp in groundDecorationChanceMap)
        {
            minChance = (float)groundDecorationRandom.NextDouble();

            if (minChance <= kvp.Value)
            {
                if (kvp.Key.tile == null)
                {
                    return;
                }
                this.tilemapData.Add(new Coord(x, y), kvp.Key.tile);
                break;
            }
        }
    }
}

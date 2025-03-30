using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public List<int> wall;

    public int flippedDoraCount = 0;
    public int usedTileCount = 0;

    private const int wanpai = 14;
    private const int allTiles = 136;

    private void Awake()
    {
        wall = new List<int>();
    }

    private void Start()
    {
        PileTiles();
        Shuffle();
    }

    void PileTiles()
    {
        int count = 0;
        for (int tile = 0; tile < 37; tile++)
        {
            // tile num = 5 or tile kind = tragon
            if ((tile / 10 == 3) || ((tile % 10 != 4) && (tile % 10 != 5)))
            {
                for (int i = 0; i < 4; i++)
                {
                    wall.Add(tile);
                    count++;
                }
            }
            else
            {
                if (tile % 10 == 5)
                {
                    wall.Add(tile);
                    count++;
                }
                else
                {
                    for (int i = 0; i < 3; i++)
                    {
                        wall.Add(tile);
                        count++;
                    }
                }
            }
        }
        Debug.Assert(count == allTiles);
    }

    void Shuffle()
    {
        for (int i = allTiles; i > 0; i--)
        {
            int rnd = Random.Range(0, i);
            (wall[i - 1], wall[rnd]) = (wall[rnd], wall[i - 1]);
        }
    }

    public int FlipNewDora()
    {
        flippedDoraCount++;
        return wall[^(2 * flippedDoraCount - 1) ];
    }

    public int NewTsumo()
    {
        if (usedTileCount >= allTiles - wanpai) return -1;
        return wall[usedTileCount++];
    }

    public List<int> Dora()
    {
        List<int> ret = new();
        for (int i = 0; i < flippedDoraCount; i++)
        {
            ret.Add(Library.toDora[wall[^(1 + 2 * i)]]);
        }
        return ret;
    }

    public List<int> UraDora()
    {
        List<int> ret = new();
        for (int i = 0; i < flippedDoraCount; i++)
        {
            ret.Add(Library.toDora[wall[^(2 * i + 2)]]);
        }
        return ret;
    }
}

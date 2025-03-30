using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int turnCount = 0;
    public List<int> hand, head;
    public List<List<int>> furoHand;
    public int furoCount = 0;
    public bool[,] machiTiles;
    // from parent, from childs
    // OR
    // from who, point
    public List<string> tempYakuNames, yakuNames;
    public int tempHan = 2, tempFu = 20, han = 2, fu = 20;
    public int[] maxPoints, tempMaxPoints;
    // Absolute
    public int id = 0;
    public int latestTile = 0;
    // state
    public int riichiTurn;
    public bool[] machiTile;
    public int[] kakan;
    public bool[] pon, chi;
    public bool[] kan;
    public int[] count;
    public bool[,] ryanmenIf;
    public bool[] ryanmen;
    public int[] seqMeld = new int[37];
    public int[] tripletMeld = new int[37];
    public int[] furoSeqMeld = new int[37];
    public int[] furoTripletMeld = new int[37];
    public int[] kantsu = new int[37];
    public int[] ankantsu = new int[37];
    public int[] tileCount;
    public bool[,] penchanKanchanTankiIf;
    public bool[] penchanKanchanTanki;
    public int hola;
    public int[] shantenIf;
    // count When Get New Tile
    public int redDoraCount;
    /// <summary>
    /// (activeTileId, handId1, handId2) -> Can Furo ? 
    /// </summary>
    public bool[,,] ponPair, chiPair;
    public int shanten;
    public bool doubleriichiNow;
    public bool kanJustNow = false;
    // when win
    public bool canAnkan = false;

    private void Awake()
    {
        hand = new List<int>();
        furoHand = new List<List<int>>();
        //pastFuroPlace = new();
        machiTiles = new bool[37, 37];
        machiTile = new bool[37];
        kakan = new int[37];
        pon = new bool[37];
        chi = new bool[37];
        kan = new bool[37];
        penchanKanchanTanki = new bool[37];
        ryanmen = new bool[37];
        ponPair = new bool[37, 37, 37];
        chiPair = new bool[37, 37, 37];
        penchanKanchanTankiIf = new bool[37, 37];
        ryanmenIf = new bool[37, 37];
        shanten = 13;

        seqMeld = new int[37];
        tripletMeld = new int[37];
        furoSeqMeld = new int[37];
        furoTripletMeld = new int[37];
        kantsu = new int[37];
        ankantsu = new int[37];
        count = new int[37];
        riichiTurn = -1;
    }
    private void Start()
    {
        //riichiNow = false;
        for (int i = 0; i < 37; i++)
        {
            for (int j = 0; j < 37; j++)
            {
                for (int k = 0; k < 37; k++)
                {
                    ponPair[i, j, k] = false;
                    chiPair[i, j, k] = false;
                }
                penchanKanchanTankiIf[i, j] = false;
            }
            kakan[i] = -1;
            furoSeqMeld[i] = 0;
            furoTripletMeld[i] = 0;
            kantsu[i] = 0;
            ankantsu[i] = 0;
            penchanKanchanTanki[i] = false;
        }
    }
}

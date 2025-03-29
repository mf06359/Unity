using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int point;
    public int turnCount = 0;
    public List<int> hand;
    public List<List<int>> furoHand;
    public int furoCount = 0;
    public List<int> ankanHand;
    public List<Vector3> pastFuroPlace;
    public bool[,] machiTiles;
    public List<int> head;
    // from parent, from childs
    // OR
    // from who, point
    public List<string> tempYakuNames;
    public int[] tempMaxPoints = new int[4];
    public int tempHan = 2, tempFu = 20;
    public List<string> yakuNames;
    public int[] maxPoints = new int[4];
    public int han = 2, fu = 20;
    // Absolute
    public int id = 0;
    public int tsumo = 0;
    // state
    public int riichiTurn = 0;
    public bool[] machiTile;
    public int[] kakan;
    public bool[] pon, chi; 
    public bool[] kan;
    public bool[] didKan;
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
    // count When Get New Tile
    public int redDoraCount;
    /// <summary>
    /// (activeTileId, handId1, handId2) -> Can Furo ? 
    /// </summary>
    public bool[,,] ponPair, chiPair;
    public int shanten;
    public bool doubleriichiNow, riichiNow, isParent;
    public bool kanJustNow = false;
    // when win
    public bool myTurn = true, furoNow = false, ankan = false;

    private void Awake()
    {
        hand = new List<int>();
        furoHand = new List<List<int>>();
        pastFuroPlace = new();
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
    }
    private void Start()
    {
        point = 25000;
        riichiNow = false;
        isParent = false;
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

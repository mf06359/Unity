using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class Library : MonoBehaviour
{
    public static int[] kokushi =
    {
        0, 9, 10, 19, 20, 29, 30, 31, 32, 33, 34, 35, 36
    };

    public static int[] tanyao =
    {
         1,  2,  3,  4,  5,  6,  7,  8,
        11, 12, 13, 14, 15, 16, 17, 18,
        21, 22, 23, 24, 25, 26, 27, 28,
    };

    public static int[] toNumberOfTile =
    {
        1, 2, 3, 4, 5, 5, 6, 7, 8, 9,
        1, 2, 3, 4, 5, 5, 6, 7, 8, 9,
        1, 2, 3, 4, 5, 5, 6, 7, 8, 9,
        1, 2, 3, 4, 5, 6, 7
    };

    public static int[] idWithoutRed =
    {
         0,  1,  2,  3,  4,  4,  6,  7,  8,  9,
        10, 11, 12, 13, 14, 14, 16, 17, 18, 19,
        20, 21, 22, 23, 24, 24, 26, 27, 28, 29,
        30, 31, 32, 33, 34, 35, 36
    };

    public static int[] toRed =
    {
         0,  1,  2,  3,  5,  5,  6,  7,  8,  9,
        10, 11, 12, 13, 15, 15, 16, 17, 18, 19,
        20, 21, 22, 23, 25, 25, 26, 27, 28, 29,
        30, 31, 32, 33, 34, 35, 36
    };

    public static int[] toDora =
    { 
         1,  2,  3,  4,  6,  6,  7,  8,  9,  0,
        11, 12, 13, 14, 16, 16, 17, 18, 19, 10,
        21, 22, 23, 24, 26, 26, 27, 28, 29, 20,
        31, 32, 33, 30, 35, 36, 34
    };
    public static int[] smallerSideTile = {
        -1,  0,  1,  2,  3,  3,  4,  6,  7,  8,
        -1, 10, 11, 12, 13, 13, 14, 16, 17, 18,
        -1, 20, 21, 22, 23, 23, 24, 26, 27, 28,
        -1, 30, 31, 32, 33, 34, 35
    };
    public static string[] ToSpriteName =
    {
        "m1b", "m2b", "m3b", "m4b", "m5b", "m5r", "m6b", "m7b", "m8b", "m9b",
        "p1b", "p2b", "p3b", "p4b", "p5b", "p5r", "p6b", "p7b", "p8b", "p9b",
        "s1b", "s2b", "s3b", "s4b", "s5b", "s5r", "s6b", "s7b", "s8b", "s9b",
        "d1b", "d2b", "d3b", "d4b", "d5b", "d6b", "d7b",
    };
    public static int[] biggerSideTile = {
         1,  2,  3,  4,  6,  6,  7,  8,  9, -1,
        11, 12, 13, 14, 16, 16, 17, 18, 19, -1,
        21, 22, 23, 24, 26, 26, 27, 28, 29, -1,
        31, 32, 33, 34, 35, 36, -1
    };

    private static readonly bool[] includedAllGreen =
    {
        false, false, false, false, false, false, false, false, false, false,
        false, false, false, false, false, false, false, false, false, false,
        false,  true,  true,  true, false, false, true, false,  true, false,
        false, false, false, false, false,  true, false
    };

    // child's tsumo -> BP from child and 2 BP from parent
    // child's ron -> 4BP from who dealed in
    // parent's tsumo -> 2BP from 3 child
    // parent's ron -> 6BP from who dealed in
    public static int BasePoints(Player player, int C)
    {
        if (player.tempHan >= 78) return 48000 * C;
        if (player.tempHan >= 65) return 40000 * C;
        if (player.tempHan >= 52) return 32000 * C;
        if (player.tempHan >= 39) return 24000 * C;
        if (player.tempHan >= 26) return 16000 * C;
        if (player.tempHan >= 13) return 8000 * C;
        if (player.tempHan >= 11) return 6000 * C;
        if (player.tempHan >= 8) return 4000 * C;
        if (player.tempHan >= 6) return 3000 * C;
        if (player.tempHan >= 5 || (player.tempHan >= 4 && player.tempFu >= 40) || (player.tempHan >= 3 && player.tempFu >= 70)) return 2000 * C;
        return (int) (C * 1d * player.tempFu * Math.Pow(2, player.tempHan + 2) + 99)  / 100 * 100;
    }

    public static int PopCount(int x)
    {
        int r = (x & 0x55555555) + (x >> 1 & 0x55555555);
        r = (r & 0x33333333) + (r >> 2 & 0x33333333);
        r = (r & 0x0f0f0f0f) + (r >> 4 & 0x0f0f0f0f);
        r = (r & 0x00ff00ff) + (r >> 8 & 0x00ff00ff);
        return (r & 0x0000ffff) + (r >> 16 & 0x0000ffff);
    }

    public static int ToInt(string name)
    {
        return (name[0] == 'm' ? 0 : name[0] == 'p' ? 1 : name[0] == 's' ? 2 : 3) * 10
             + ((name[1] <= '5' || name[0] == 'd') ? (name[1] - '0') - 1 : (name[1] - '0')) + (name[2] == 'r' ? 1 : 0);
    }

    // if RonToWho == -1 => TsumoHo else RonHo
    public static void CalculatePoints(Player player, int RonToWho = -1)
    {
        Debug.Log("CalculatePoints Called");
        CountTiles(player);
        HolaYakuman(player, RonToWho);
        HolaSevenPairs(player, RonToWho);
        HolaWithHead(player, 0, RonToWho);
    }
    private static void CountTiles(Player player)
    {
        Debug.Log("CountTiles Called");
        for (int i = 0; i < 37; i++)
        {
            if (player.seqMeld[i] > 0)
            {
                for (int j = 0; j < 2; j++)
                {
                    player.count[i - j] += player.seqMeld[i];
                }
            }
            if (player.furoSeqMeld[i] > 0)
            {
                for (int j = 0; j < 2; j++)
                {
                    player.count[i - j] += player.furoSeqMeld[i];
                }
            }
            player.count[i] += player.kantsu[i] * 4;
            player.count[i] += player.ankantsu[i] * 4;
            player.count[i] += player.tripletMeld[i] * 3;
            player.count[i] += player.furoTripletMeld[i] * 3;
        }
        for (int i = 0; i < player.hand.Count; i++)
        {
            player.count[idWithoutRed[player.hand[i]]]++;
        }
    }
    private static void HolaWithHead(Player player, int usedTiles, int RonToWho = -1)
    {
        if (PopCount(((1 << player.hand.Count) - 1) & ~usedTiles) == 2)
        {
            //Debug.Log("HolaWithHead Called");
            for (int i = 0; i < player.hand.Count; i++)
            {
                if (((usedTiles >> i) & 1) == 0)
                {
                    player.head.Add(player.hand[i]);
                }
            }
            if (player.head[0] == player.head[1]) HolaNormal(player, RonToWho);
            //Debug.Log($"{player.head[0]} : {player.head[1]}");
            player.head.Clear();
        }
        player.tsumo = PlayerManager.instance.activeTile;
        for (int i = 0; i + 2 < player.hand.Count; i++)
        {
            if (((usedTiles >> i) & 1) == 1)
            {
                continue;
            }
            for (int j = i + 1; j + 1 < player.hand.Count; j++)
            {
                if (((usedTiles >> j) & 1) == 1)
                {
                    continue;
                }
                for (int k = j + 1; k < player.hand.Count; k++)
                {
                    if (((usedTiles >> k) & 1) == 1)
                    {
                        continue;
                    }
                    if (ThreeIdToShantenCount(player, i, j, k) > 0) continue;
                    if (RonToWho != -1)
                    {
                        if (player.hand[i] == player.tsumo)
                        {
                            player.tsumo = -1;
                            if (player.hand[i] == player.hand[j])
                            {
                                player.furoTripletMeld[player.hand[i]]++;
                                HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                                player.furoTripletMeld[player.hand[i]]--;
                            }
                            else
                            {
                                player.furoSeqMeld[player.hand[k]]++;
                                HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                                player.furoSeqMeld[player.hand[k]]--;
                            }
                            player.tsumo = player.hand[i];
                        }
                        if (player.hand[j] == player.tsumo)
                        {
                            player.tsumo = -1;
                            if (player.hand[i] == player.hand[j])
                            {
                                player.furoTripletMeld[player.hand[i]]++;
                                HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                                player.furoTripletMeld[player.hand[i]]--;
                            }
                            else
                            {
                                player.furoSeqMeld[player.hand[k]]++;
                                HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                                player.furoSeqMeld[player.hand[k]]--;
                            }
                            player.tsumo = player.hand[j];
                        }
                        if (player.hand[k] == player.tsumo)
                        {
                            player.tsumo = -1;
                            if (player.hand[i] == player.hand[j])
                            {
                                player.furoTripletMeld[player.hand[i]]++;
                                HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                                player.furoTripletMeld[player.hand[i]]--;
                            }
                            else
                            {
                                player.furoSeqMeld[player.hand[k]]++;
                                HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                                player.furoSeqMeld[player.hand[k]]--;
                            }
                            player.tsumo = player.hand[k];
                        }
                        if (player.hand[i] == player.hand[j])
                        {
                            player.tripletMeld[player.hand[i]]++;
                            HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                            player.tripletMeld[player.hand[i]]--;
                        }
                        else
                        {
                            player.seqMeld[player.hand[k]]++;
                            HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                            player.seqMeld[player.hand[k]]--;
                        }
                    }
                    else
                    {
                        if (player.hand[i] == player.hand[j])
                        {
                            player.tripletMeld[player.hand[i]]++;
                            HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                            player.tripletMeld[player.hand[i]]--;
                        }
                        else
                        {
                            player.seqMeld[player.hand[k]]++;
                            HolaWithHead(player, usedTiles | (1 << i) | (1 << j) | (1 << k), RonToWho);
                            player.seqMeld[player.hand[k]]--;
                        }
                    }
                }
            }
        }
    }
    private static void HolaNormal(Player player, int RonToWho = -1)
    {
        player.tempFu = 20;
        player.tempHan = 0;
        player.tempYakuNames = new();
        bool didFuro = (player.furoCount > 0);
        // fu & han
        // mentsu
        {
            foreach (int i in tanyao)
            {
                if (player.tripletMeld[i] > 0)
                {
                    player.tempFu += player.tripletMeld[i] * 4;
                }
                if (player.furoTripletMeld[i] > 0)
                {
                    player.tempFu += player.furoTripletMeld[i] * 2;
                }
                if (player.kantsu[i] > 0)
                {
                    player.tempFu += 8;
                }
                if (player.ankantsu[i] > 0)
                {
                    player.tempFu += 16;
                }
            }
            foreach (int i in kokushi)
            {
                if (player.tripletMeld[i] > 0)
                {
                    player.tempFu += player.tripletMeld[i] * 8;
                }
                if (player.furoTripletMeld[i] > 0)
                {
                    player.tempFu += player.furoTripletMeld[i] * 4;
                }
                if (player.kantsu[i] > 0)
                {
                    player.tempFu += 16;
                }
                if (player.ankantsu[i] > 0)
                {
                    player.tempFu += 32;
                }
            }
        }
            // head
        {
            if (player.head[0] == 30 + (player.id - PlayerManager.instance.parentId + 4) % 4)
            {
                player.tempFu += 2;
            }
            if (34 <= player.head[0] && player.head[0] <= 36)
            {
                player.tempFu += 2;
            }
            if (player.head[0] == 30 + PlayerManager.instance.placeWind)
            {
                player.tempFu += 2;
            }
        }
        // penchan // kanchan // tanki
        {
            if (player.penchanKanchanTanki[idWithoutRed[PlayerManager.instance.activeTile]])
            {
                player.tempFu += 2;
            }
        }
        // Ron OR TSUMO
        {
            if (RonToWho == -1)
            {
                player.tempFu += 2;
            }
            else if (!didFuro)
            {
                player.tempFu += 10;
            }
        }


        bool headIsKanji = false;
        bool isPinfu = false;
        //  Y A K U 
        // Riichi // Double Riichi // CHECKED
        {
            if (player.doubleriichiNow)
            {
                player.tempYakuNames.Add("ダブル立直");
                player.tempHan += 2;
            }
            if (player.riichiNow)
            {
                player.tempYakuNames.Add("立直");
                player.tempHan += 1;
            }
        }
        // Ippatsu // Tsumo // CHECKED
        {
            if (player.turnCount - player.riichiTurn == 1)
            {
                player.tempYakuNames.Add("一発");
                player.tempHan += 1;
            }
            // Tsumo
            {
                if (RonToWho == -1 && !didFuro)
                {
                    player.tempYakuNames.Add("門前清自摸和");
                    player.tempHan += 1;
                }
            }
        }
        // Hon itsu // Chin itsu // CHECKED
        {
            bool kanjiExist = false;
            for (int i = 30; i < 37; i++)
            {
                if (player.count[i] > 0)
                {
                    kanjiExist = true;
                }
            }
            int[] sum = new int[] { 0, 0, 0 };
            int allSum = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    sum[i] += player.count[i * 10 + j];
                    allSum += player.count[i * 10 + j];
                }
            }
            if (allSum == sum[0] || allSum == sum[1] || allSum == sum[2])
            {
                if (!kanjiExist)
                {
                    player.tempYakuNames.Add("清一色");
                    if (!didFuro)
                    {
                        player.tempHan += 6;
                    }
                    else
                    {
                        player.tempHan += 5;
                    }
                }
                else
                {
                    player.tempYakuNames.Add("混一色");
                    if (!didFuro)
                    {
                        player.tempHan += 3;
                    }
                    else
                    {
                        player.tempHan += 2;
                    }
                }
            }
        }
        // ToiToi // CHECKED
        {
            bool isToiToi = true;
            for (int i = 0; i < 37; i++)
            {
                if (player.seqMeld[i] + player.furoSeqMeld[i] > 0)
                {
                    isToiToi = false;
                }
            }
            if (isToiToi)
            {
                player.tempYakuNames.Add("対々和");
                player.tempHan += 2;
            }
        }
        // Ton
        {
            if (player.id == 0)
            {
                if (player.count[30 + player.id] >= 3)
                {
                    player.tempYakuNames.Add("自風牌 東");
                    player.tempHan += 1;
                    if (player.head[0] == 30)
                    {
                        headIsKanji = true;
                    }
                }
            }
        }
        // Double Ton
        {
            if (PlayerManager.instance.placeWind == 0)
            {
                if (player.count[30] >= 3)
                {
                    player.tempYakuNames.Add("場風牌 東");
                    player.tempHan += 1;
                }
            }
            if (player.head[0] == 30)
            {
                headIsKanji = true;
            }
        }
        // Nan
        {
            if (player.id == 1)
            {
                if (player.count[30 + player.id] >= 3)
                {
                    player.tempYakuNames.Add("自風牌 南");
                    player.tempHan += 1;
                }
                if (player.head[0] == 31)
                {
                    headIsKanji = true;
                }
            }
        }
        // Double Nan
        {
            if (PlayerManager.instance.placeWind == 1)
            {
                if (player.count[31] >= 3)
                {
                    player.tempYakuNames.Add("場風牌 南");
                    player.tempHan += 1;
                }
            }
            if (player.head[0] == 31)
            {
                headIsKanji = true;
            }
        }
        // Sha
        {
            if (player.id == 2)
            {
                if (player.count[30 + player.id] >= 3)
                {
                    player.tempYakuNames.Add("自風牌 西");
                    player.tempHan += 1;
                }
                if (player.head[0] == 32)
                {
                    headIsKanji = true;
                }
            }
        }
        // Pei
        {
            if (player.id == 3)
            {
                if (player.count[30 + player.id] >= 3)
                {
                    player.tempYakuNames.Add("自風牌 北");
                    player.tempHan += 1;
                }
                if (player.head[0] == 33)
                {
                    headIsKanji = true;
                }
            }
        }
        // Haku
        {
            if (player.count[34] >= 3)
            {
                player.tempYakuNames.Add("白");
                player.tempHan += 1;
            }
            if (player.head[0] == 34)
            {
                headIsKanji = true;
            }
        }
        // Hatsu
        {
            if (player.count[35] >= 3)
            {
                player.tempYakuNames.Add("發");
                player.tempHan += 1;
            }
            if (player.head[0] == 35)
            {
                headIsKanji = true;
            }
        }
        // Chun
        {
            if (player.count[36] >= 3)
            {
                player.tempYakuNames.Add("中");
                player.tempHan += 1;
            }
            if (player.head[0] == 36)
            {
                headIsKanji = true;
            }
        }
        // Tanyao // CHECKED
        {
            bool isTanyao = true;
            foreach (int i in kokushi)
            {
                if (player.count[i] > 0)
                {
                    isTanyao = false;
                }
            }
            if (isTanyao)
            {
                player.tempYakuNames.Add("断么九");
                player.tempHan += 1;
            }
        }
        // Haitei // Houtei // CHECKED
        {
            if (PlayerManager.instance.wall.usedTileCount == 136 - 14)
            {
                if (PlayerManager.instance.activeTilePlayer == player.id)
                {
                    player.tempYakuNames.Add("海底撈月");
                    player.tempHan += 1;
                }
                else
                {
                    player.tempYakuNames.Add("河底撈魚");
                    player.tempHan += 1;
                }
            }
        }
        // Sanshoku Dojun // CHECKED
        {
            for (int i = 0; i < 10; i++)
            {
                if (player.seqMeld[i] + player.furoSeqMeld[i] > 0 && 
                    player.seqMeld[i + 10] + player.furoSeqMeld[i + 10] > 0 && 
                    player.seqMeld[i + 20] + player.furoSeqMeld[i + 20] > 0 )
                {
                    player.tempYakuNames.Add("三色同順");
                    if (!didFuro)
                    {
                        player.tempHan += 2;
                    }
                    else
                    {
                        player.tempHan += 1;
                    }
                }
            }
        }
        // Sanshoku DoKo // CHECKED
        {
            for (int i = 0; i < 10; i++)
            {
                if (player.tripletMeld[i] + player.furoTripletMeld[i] > 0 &&
                    player.tripletMeld[i + 10] + player.furoTripletMeld[i + 10] > 0 &&
                    player.tripletMeld[i + 20] + player.furoTripletMeld[i + 20] > 0)
                {
                    player.tempYakuNames.Add("三色同刻");
                    player.tempHan += 2;
                }
            }
        }
        // Ikki Tsukan // CHECKED
        {
            for (int i = 0; i < 3; i++)
            {
                if (player.seqMeld[i * 10 + 2] + player.furoSeqMeld[i * 10 + 2] > 0 && 
                    player.seqMeld[i * 10 + 5] + player.furoSeqMeld[i * 10 + 5] > 0 && 
                    player.seqMeld[i * 10 + 9] + player.furoSeqMeld[i * 10 + 9] > 0 )
                {
                    player.tempYakuNames.Add("一気通貫");
                    if (!didFuro)
                    {
                        player.tempHan += 2;
                    }
                    else
                    {
                        player.tempHan += 1;
                    }
                }
            }
        }
        // IPEKO // RYANPEKO // CHECKED
        {
            if (!didFuro)
            {
                int count = 0;
                for (int i = 0; i < 30; i++)
                {
                    if (player.seqMeld[i] >= 2)
                    {
                        count++;
                    }
                }
                if (count == 1)
                {
                    player.tempYakuNames.Add("一盃口");
                    player.tempHan += 1;
                }
                if (count >= 2)
                {
                    player.tempYakuNames.Add("二盃口");
                    player.tempHan += 3;
                }
            }
        }
        // CHANTA // JUNG CHANG
        {
            bool kanjiExist = false;
            for (int i = 30; i < 37; i++)
            {
                if (player.count[i] > 0)
                {
                    kanjiExist = true;
                }
                if (player.head[0] == i)
                {
                    kanjiExist = true;
                }
            }
            bool tanyaoExist = false;
            foreach (int i in tanyao)
            {
                if (player.tripletMeld[i] + player.furoTripletMeld[i] > 0)
                {
                    tanyaoExist = true;
                }
                if (player.head[0] == i)
                {
                    tanyaoExist = true;
                }
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 3; j <= 8; j++)
                {
                    int t = i * 10 + j;
                    if (player.seqMeld[t] + player.furoSeqMeld[t] > 0)
                    {
                        tanyaoExist = true;
                    }
                }
            }
            if (!tanyaoExist)
            {
                if (kanjiExist)
                {
                    player.tempYakuNames.Add("混全帯么九");
                    if (!didFuro)
                    {
                        player.tempHan += 2;
                    }
                    else
                    {
                        player.tempHan += 1;
                    }
                }
                else
                {
                    player.tempYakuNames.Add("純全帯么九");
                    if (!didFuro)
                    {
                        player.tempHan += 3;
                    }
                    else
                    {
                        player.tempHan += 2;
                    }
                }
            }
        }
        // SAN ANKO
        {
            int count = 0;
            for (int i = 0; i < 37; i++)
            {
                count += player.tripletMeld[i];
                count += player.ankantsu[i];
            }
            if (count >= 3)
            {
                player.tempYakuNames.Add("三暗刻");
                player.tempHan += 2;
            }
        }
        // SHO SAN GEN
        {
            if (player.head[0] == 34 && player.count[35] >= 3 && player.count[36] >= 3)
            {
                player.tempYakuNames.Add("小三元");
                player.tempHan += 2;
            }
            if (player.head[0] == 35 && player.count[36] >= 3 && player.count[34] >= 3)
            {
                player.tempYakuNames.Add("小三元");
                player.tempHan += 2;
            }
            if (player.head[0] == 36 && player.count[34] >= 3 && player.count[35] >= 3)
            {
                player.tempYakuNames.Add("小三元");
                player.tempHan += 2;
            }
        }
        // HON RO TO // CHECKED
        {
            bool isHonroto = true;
            foreach (int i in tanyao)
            {
                if (player.count[i] > 0)
                {
                    isHonroto = false;
                }
            }
            if (isHonroto)
            {
                player.tempYakuNames.Add("混老頭");
                player.tempHan += 2;
            }
        }
        // Pinfu
        {
            int meldCount = 0;
            for (int i = 0; i < 37; i++)
            {
                meldCount += player.kantsu[i];
                meldCount += player.ankantsu[i];
                meldCount += player.furoTripletMeld[i];
                meldCount += player.tripletMeld[i];
            }          
            if (meldCount == 0 && player.ryanmen[idWithoutRed[PlayerManager.instance.activeTile]] && !headIsKanji)
            {
                player.tempYakuNames.Add("平和");
                player.tempHan += 1;
                isPinfu = true;
            }
        }
        // RinShan
        {
            if (player.kanJustNow)
            {
                player.tempYakuNames.Add("嶺上開花");
                player.tempHan += 1;
            }
        }
        // ChanKan
        {
            if (PlayerManager.instance.activeTileKanned)
            {
                player.tempYakuNames.Add("槍槓");
                player.tempHan += 1;
            }
        }
        // SAN KANTSU
        {
            int count = 0;
            for (int i = 0; i < 37; i++)
            {
                if (player.kantsu[i] + player.ankantsu[i] >= 1)
                {
                    Debug.Assert(player.kantsu[i] != player.ankantsu[i]);
                    count++;
                }
            }
            if (count >= 3)
            {
                player.tempYakuNames.Add("三槓子");
                player.tempHan += 2;
            }
        }
        // Dora 
        {
            int doraCount = 0;
            foreach (var dora in PlayerManager.instance.dora)
            {
                doraCount += player.count[dora];
            } 
            if (doraCount > 0)
            {
                player.tempYakuNames.Add("ドラ");
                player.tempHan += doraCount;
            }
        }
        // Ura Dora
        {
            if (player.riichiNow)
            {
                int uraDoraCount = 0;
                foreach (var dora in PlayerManager.instance.uraDora)
                {
                    uraDoraCount += player.count[dora];
                }
                if (uraDoraCount > 0)
                {
                    player.tempYakuNames.Add("裏ドラ");
                    player.tempHan += uraDoraCount;
                }
            }
        }
        // Red Dora
        {
            if (player.redDoraCount > 0)
            {
                player.tempYakuNames.Add("赤ドラ");
                player.tempHan += player.redDoraCount;
            }
        }
        if (isPinfu)
        {
            player.tempFu = 20;
        }
        ReloadPayPoints(player, RonToWho);
    }
    public static void HolaSevenPairs(Player player, int RonToWho = -1)
    {
        Debug.Log("HolaSevenPairs Called");
        bool didFuro = (player.furoCount > 0);
        // SevenPairs // VERIFIED
        {
            player.tempFu = 0;
            player.tempHan = 0;
            player.tempYakuNames = new List<string>();
            if (player.hand.Count != 14)
            {
                Debug.Log("HolaSevenPairs does not match");
                return;
            }
            int pairCount = 0;
            for (int tile = 0; tile < 37; tile++)
            {
                if (player.count[tile] == 2)
                {
                    pairCount++;
                }
                //else
                //{
                //    return;
                //}
            }
            if (pairCount != 7) return;
        }
        // Riichi // VERIFIED 
        // Double Riichi 
        {
            if (player.doubleriichiNow)
            {
                player.tempYakuNames.Add("ダブル立直");
                Debug.Log("double riichi");
                player.tempHan += 2;
            }
            if (player.riichiNow)
            {
                player.tempYakuNames.Add("立直");
                Debug.Log("riichi");
                player.tempHan += 1;
            }
        }
        // Ippatsu
        // Tsumo // VERIFIED
        {
            // Tsumo 
            if (player.turnCount - player.riichiTurn == 1)
            {
                player.tempYakuNames.Add("一発");
                Debug.Log("ippatsu");
                player.tempHan += 1;
            }
            // Tsumo // VERIFIED
            {
                if (RonToWho == -1)
                {
                    player.tempYakuNames.Add("門前清自摸和");
                    Debug.Log("tsumo");
                    player.tempHan += 1;
                }
            }
        }
        // Haitei // Houtei
        {
            if (PlayerManager.instance.wall.usedTileCount == 136 - 14)
            {
                if (PlayerManager.instance.activeTilePlayer == player.id)
                {
                    player.tempYakuNames.Add("海底撈月");
                    player.tempHan += 1;
                }
                else
                {
                    player.tempYakuNames.Add("河底撈魚");
                    player.tempHan += 1;
                }
            }
        }
        // Hon itsu // So VERIFIED
        // Chin itsu // Man VERIFIED
        {
            int kanjiExist = 0;
            for (int i = 30; i < 37; i++)
            {
                if (player.count[i] > 0)
                {
                    kanjiExist += player.count[i];
                }
            }
            for (int i = 0; i < 3; i++)
            {
                int sum = kanjiExist;
                for (int j = 0; j < 10; j++)
                {
                    sum += player.count[j + i * 10];
                }
                if (sum == 14)
                {
                    if (kanjiExist == 0)
                    {
                        player.tempYakuNames.Add("清一色");
                        Debug.Log("chin i so");
                        if (!didFuro)
                        {
                            player.tempHan += 6;
                        }
                        else
                        {
                            player.tempHan += 5;
                        }
                    }
                    else
                    {
                        player.tempYakuNames.Add("混一色");
                        Debug.Log("hon i so");
                        if (!didFuro)
                        {
                            player.tempHan += 3;
                        }
                        else
                        {
                            player.tempHan += 2;
                        }
                    }
                }
            }
        }
        // HON RO TO 
        {
            int sum = 0;
            foreach (int i in kokushi)
            {
                sum += player.count[i];
            }
            if (sum == 14)
            {
                player.tempYakuNames.Add("混老頭");
                player.tempHan += 2;
            }
        }
        // ChanKan 
        {
            if (PlayerManager.instance.activeTileKanned)
            {
                player.tempYakuNames.Add("槍槓");
                player.tempHan += 1;
            }
        }
        player.tempYakuNames.Add("七対子");
        Debug.Log(" K O S H O ");
        player.tempFu = 25;
        player.tempHan += 2;
        // Dora // VERIFIED
        {
            int doraCount = 0;
            foreach (var dora in PlayerManager.instance.dora)
            {
                if (player.count[dora] > 0)
                {
                    doraCount += player.count[dora];
                }
            } 
            if (doraCount > 0)
            {
                player.tempYakuNames.Add("ドラ");
                player.tempHan += doraCount;
                //player.tempHan += 2;
                Debug.Log($"dora : {doraCount}");
            }
        }
        // Ura Dora // VERIFIED
        {
            if (player.riichiNow)
            {
                int uraDoraCount = 0;
                foreach (var dora in PlayerManager.instance.uraDora)
                {
                    if (player.count[dora] > 0)
                    {
                        uraDoraCount += player.count[dora];
                    }
                }
                if (uraDoraCount > 0)
                {
                    player.tempYakuNames.Add("裏ドラ");
                    player.tempHan += uraDoraCount;
                    Debug.Log($"ura Dora ; {uraDoraCount}");
                }
            }
        }
        // Red Dora // VERIFIED
        {
            if (player.redDoraCount > 0)
            {
                player.tempYakuNames.Add("赤ドラ");
                player.tempHan += player.redDoraCount;
                Debug.Log($"red Dora ; {player.redDoraCount}");
            }
        }
        ReloadPayPoints(player, RonToWho);
    }
    public static void HolaYakuman(Player player, int RonToWho)
    {
        Debug.Log("HolaYakuman");
        player.tempFu = 0;
        player.tempHan = 0;
        player.tempYakuNames = new List<string> ();
        // TENHO && CHIHO
        {
            if (player.turnCount == 1 && RonToWho == -1)
            {
                if (PlayerManager.instance.parentId == player.id)
                {
                    player.tempYakuNames.Add("天和");
                    player.tempHan += 13;
                }
                else
                {
                    player.tempYakuNames.Add("地和");
                    player.tempHan += 13;
                }
            }
        }
        // thirteen orphans // VERIFIED
        {
            bool isKokushi = true;
            if (player.hand.Count != 14)
            {
                isKokushi = false;
            }
            int[] count = new int[37];
            for (int i = 0; i < 37; i++)
            {
                count[i] = 0;
            }
            for (int i = 0; i < player.hand.Count; i++)
            {
                count[idWithoutRed[player.hand[i]]]++;
            }
            int pairTileNumber = -1;
            int kindCount = 0;
            foreach (int pai in kokushi)
            {
                if (count[pai] >= 1)
                {
                    kindCount++;
                }
                if (count[pai] == 2)
                {
                    pairTileNumber = idWithoutRed[pai];
                }
            }
            if (kindCount == 13 && isKokushi)
            {
                if (PlayerManager.instance.activeTile == pairTileNumber)
                {
                    player.tempYakuNames.Add("国士無双13面待ち");
                    player.tempHan = 26;
                }
                else
                {
                    player.tempYakuNames.Add("国士無双");
                    player.tempHan = 13;
                }
            }
        }
        // SU ANKO  // VERIFIED
        // CHUREN OF POTO // checked
        {
            if (player.furoCount == 0)
            {
                // SU ANKO
                // Su ANKO Tanki // checked
                int kindCount = 0;
                for (int i = 0; i < 37; i++)
                {
                    if (player.count[i] >= 3)
                    {
                        kindCount++;
                    }
                }
                if (kindCount == 4)
                {
                    if (player.count[player.hola] == 2)
                    {
                        player.tempYakuNames.Add("四暗刻単騎待ち");
                        player.tempHan += 26;
                    }
                    else
                    {
                        player.tempYakuNames.Add("四暗刻");
                        player.tempHan += 13;
                    }
                }
                // CHUREN OF POTO
                // JUNSEI CHUREN OF POTO
                for (int j = 0; j < 3; j++)
                {
                    bool isChuren = true;
                    int doubleCountId = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        if (i == 5) continue;
                        int baseline = (i == 0 || i == 9 ? 3 : 1);
                        if (baseline < player.count[i])
                        {
                            doubleCountId = i;
                        }
                        else if (baseline > player.count[i])
                        {
                            isChuren = false;
                        }
                    }
                    if (isChuren)
                    {
                        if (doubleCountId == player.hola)
                        {
                            player.tempYakuNames.Add("純正九蓮宝燈");
                            player.tempHan += 26;
                        }
                        else
                        {
                            player.tempYakuNames.Add("九蓮宝燈");
                            player.tempHan += 13;
                        }
                    }
                }
            }
        }
        // Can Judge only with Counts
        // DAI SAN GEN
        {
            if (player.count[34] >= 3 && player.count[35] >= 3 && player.count[36] >= 3)
            {
                player.tempYakuNames.Add("大三元");
                player.tempHan += 13;
            }
        }
        // DAI SU SHI && SHO SU SHI
        // checked
        {
            int pairCount = 0, meldCount = 0;
            for (int i = 30; i <= 33; i++)
            {
                if (player.count[i] == 2)
                {
                    pairCount++;
                }
                else if (player.count[i] >= 3)
                {
                    meldCount++;
                    pairCount++;
                }
            }
            if (pairCount >= 4)
            {
                if (meldCount == 4)
                {
                    player.tempYakuNames.Add("大四喜");
                    player.tempHan += 26;
                }
                else if (meldCount == 3)
                {
                    player.tempYakuNames.Add("小四喜");
                    player.tempHan += 13;
                }
            }
        }
        // ALL GREEN
        // checked
        {
            int notIncludedAllGreen = 0;
            for (int i = 0; i < 37; i++)
            {
                if (!includedAllGreen[i])
                {
                    notIncludedAllGreen += player.count[i];
                }
            }
            if (notIncludedAllGreen == 0)
            {
                player.tempYakuNames.Add("緑一色");
                player.tempHan += 13;
            }
        }
        // ALL Kanji // VERIFIED
        {
            bool allKanji = true;
            for (int i = 0; i < 29; i++)
            {
                if (player.count[i] >= 1)
                {
                    allKanji = false;
                }
            }
            if (allKanji)
            {
                player.tempYakuNames.Add("字一色");
                player.tempHan += 13;
            }
        }
        // CHIN RO TO // VERIFIED
        {
            bool isChinRoTo = true;
            for (int i = 1; i <= 8; i++)
            {
                if (player.count[i] >= 1)
                {
                    isChinRoTo = false;
                }
            }
            for (int i = 11; i <= 18; i++)
            {
                if (player.count[i] >= 1)
                {
                    isChinRoTo = false;
                }
            }
            for (int i = 21; i <= 28; i++)
            {
                if (player.count[i] >= 1)
                {
                    isChinRoTo = false;
                }
            }
            for (int i = 30; i < 37; i++)
            {
                if (player.count[i] >= 1)
                {
                    isChinRoTo = false;
                }
            }
            if (isChinRoTo)
            {
                player.tempYakuNames.Add("清老頭");
                player.tempHan += 13;
            }
        }
        // SU KANTSU
        {
            int count = 0;
            for (int i = 0; i < 37; i++)
            {
                if (player.kantsu[i] + player.ankantsu[i] >= 1)
                {
                    Debug.Assert(player.kantsu[i] != player.ankantsu[i]);
                    count++;
                }
            }
            if (count == 4)
            {
                player.tempYakuNames.Add("四槓子");
                player.tempHan += 13;
            }
        }
        string temp = "";
        foreach (string s in player.tempYakuNames)
        {
            temp += s;
            temp += ", ";
        }
        Debug.Log("Yakuman Names" + temp);
        ReloadPayPoints(player, RonToWho);
    }
    public static void ReloadPayPoints (Player player, int RonToWho = -1)
    {
        player.tempMaxPoints = new int[4] { 0, 0, 0, 0 };
        player.tempFu = (player.tempFu == 25) ? player.tempFu : (player.tempFu + 9) / 10 * 10;
        for (int i = 0; i < 4; i++)
        {
            // TSUMO
            if (RonToWho == -1)
            {
                if (PlayerManager.instance.parentId == player.id)
                {
                    if (i == player.id)
                    {
                        player.tempMaxPoints[i] = 3 * BasePoints(player, 2);
                    }
                    else
                    {
                        player.tempMaxPoints[i] = -BasePoints(player, 2);
                    }
                }
                else
                {
                    if (i == player.id)
                    {
                        player.tempMaxPoints[i] = BasePoints(player, 1) * 2 + BasePoints(player, 2);
                    }
                    else
                    {
                        if (i == PlayerManager.instance.parentId)
                        {
                            player.tempMaxPoints[i] = -BasePoints(player, 2);
                        }
                        else
                        {
                            player.tempMaxPoints[i] = -BasePoints(player, 1);
                        }
                    }
                }
            }
            // RON
            else
            {
                if (RonToWho == i)
                {
                    if (PlayerManager.instance.parentId == player.id)
                    {
                        player.tempMaxPoints[i] = -BasePoints(player, 6);
                        player.tempMaxPoints[player.id] = +BasePoints(player, 6);
                    }
                    else
                    {
                        player.tempMaxPoints[i] = -BasePoints(player, 4);
                        player.tempMaxPoints[player.id] = +BasePoints(player, 4);
                    }
                }
            }
        }
        if (player.tempMaxPoints[player.id] > player.maxPoints[player.id])
        {
            string temp = "";
            foreach (string s in player.tempYakuNames)
            {
                temp += s;
                temp += ", ";
            }
            Debug.Log(temp);
            player.maxPoints = player.tempMaxPoints;
            player.fu = player.tempFu;
            player.han = player.tempHan;
            player.yakuNames = player.tempYakuNames;
        }
        player.tempFu = 0;
        player.tempHan = 0;
        player.tempYakuNames = new List<string> ();
    }

    // Written Below Is Machi Judging Function
    // OK
    public static void ReloadFuroMachi(Player player)
    {
        for (int i = 0; i < 37; i++)
        {
            for (int j = 0; j < 37; j++)
            {
                for (int k = 0; k < 37; k++)
                {
                    player.ponPair[k, i, j] = false;
                    player.chiPair[k, i, j] = false;
                }
            }
            player.kan[i] = false;
            player.pon[i] = false;
            player.chi[i] = false;
        }
        for (int i = 0; i + 1 < player.hand.Count; i++)
        {
            for (int j = i + 1; j < player.hand.Count; j++)
            {
                if (IsSameKind(player, i, j) == false) continue;
                if (idWithoutRed[player.hand[i]] == idWithoutRed[player.hand[j]])
                {
                    player.ponPair[player.hand[i], player.hand[i], player.hand[j]] = true;
                    player.ponPair[toRed[player.hand[i]], player.hand[i], player.hand[j]] = true;
                    player.pon[player.hand[i]] = true;
                    player.pon[toRed[player.hand[i]]] = true;
                    for (int k = j + 1; k < player.hand.Count; k++)
                    {
                        if (idWithoutRed[player.hand[i]] == idWithoutRed[player.hand[k]])
                        {
                            int id = idWithoutRed[player.hand[i]];
                            player.kan[id] = true;
                            player.kan[toRed[id]] = true;
                        }
                    }
                }
                if (!IsNotKanji(player, i)) continue;
                if (biggerSideTile[idWithoutRed[player.hand[i]]] == idWithoutRed[player.hand[j]])
                {
                    if (toNumberOfTile[player.hand[i]] > 1)
                    {
                        int id = smallerSideTile[idWithoutRed[player.hand[i]]];
                        player.chiPair[id, player.hand[i], player.hand[j]] = true;
                        player.chiPair[toRed[id], player.hand[i], player.hand[j]] = true;
                        player.chi[id] = true;
                        player.chi[toRed[id]] = true;
                    }
                    if (toNumberOfTile[player.hand[j]] < 9)
                    {
                        int id = biggerSideTile[idWithoutRed[player.hand[j]]];
                        player.chiPair[id, player.hand[i], player.hand[j]] = true;
                        player.chiPair[toRed[id], player.hand[i], player.hand[j]] = true;
                        player.chi[id] = true;
                        player.chi[toRed[id]] = true;
                    }
                }
                if (DistanceInSame(player, i, j) == 2)
                {
                    int id = biggerSideTile[idWithoutRed[player.hand[i]]];
                    player.chiPair[id, player.hand[i], player.hand[j]] = true;
                    player.chiPair[toRed[id], player.hand[i], player.hand[j]] = true;
                    player.chi[id] = true;
                    player.chi[toRed[id]] = true;
                }
            }
        }
    }
    public static void CalculateShantenCount(Player player)
    {
        for (int i = 0; i < 37; i++)
        {
            for (int j = 0; j < 37; j++)
            {
                player.machiTiles[i, j] = false;
                player.ryanmenIf[i, j] = false;
                player.penchanKanchanTankiIf[i, j] = false;
            }
        }
        for (int i = 0; i < 37; i++)
        {
            player.pon[i] = false;
            player.chi[i] = false;
            player.kan[i] = false;
            player.ankan = false;
        }
        player.shanten = 13;
        player.hand.Sort();
        for (int i = 0; i < player.hand.Count; i++)
        {
            ShantenCount(player, i);
        }
        for (int i = 0; i + 3 < player.hand.Count; i++)
        {
            if (idWithoutRed[player.hand[i]] == idWithoutRed[player.hand[i + 1]]
            && idWithoutRed[player.hand[i + 1]] == idWithoutRed[player.hand[i + 2]]
            && idWithoutRed[player.hand[i + 2]] == idWithoutRed[player.hand[i + 3]])
            {
                player.ankan = true;
                break;
            }
        }
    }

    public static void ShantenCount(Player player, int exceptTileId)
    {
        NormalShantenCount(player, exceptTileId);
        SevenPairsShantenCount(player, exceptTileId);
        ThirteenOrphansShantenCount(player, exceptTileId);
    }

    public static void NormalShantenCount(Player player, int exceptTileId)
    {
        for (int i = 0; i + 1 < player.hand.Count; i++)
        {
            if ((exceptTileId == i) || (exceptTileId == i + 1))
            {
                continue;
            }
            if (idWithoutRed[player.hand[i]] == idWithoutRed[player.hand[i + 1]])
            {
                player.pon[idWithoutRed[player.hand[i]]] = true;
                ShantenCountWithHead(player, exceptTileId, (1 << i) | (1 << (i + 1)) | (1 << exceptTileId), 0, i);
                i++;
            }
        }
        // Single Wait
        for (int i = 0; i < player.hand.Count; i++)
        {
            if (exceptTileId != i)
            {
                ShantenCountWithTanki(player, exceptTileId, (1 << i) | (1 << exceptTileId), 0, i);
            }
        }
    }

    private static void ShantenCountWithHead(Player player, int exceptTileId, int usedTiles, int shanten, int headId)
    {
        if (PopCount(((1 << player.hand.Count) - 1) & ~usedTiles) == 2)
        {
            for (int i = 0; i + 1 < player.hand.Count; i++)
            {
                if (((usedTiles >> i) & 1) == 1) continue;
                for (int j = i + 1; j < player.hand.Count; j++)
                {
                    if (((usedTiles >> j) & 1) == 1) continue;
                    if (idWithoutRed[player.hand[i]] == idWithoutRed[player.hand[j]])
                    {
                        player.pon[idWithoutRed[player.hand[i]]] = true;
                        if (shanten == 0)
                        {
                            player.machiTiles[idWithoutRed[player.hand[exceptTileId]], idWithoutRed[player.hand[headId]]] = true;
                            player.machiTiles[idWithoutRed[player.hand[exceptTileId]], idWithoutRed[player.hand[j]]] = true;
                        }
                    }
                    else if (IsSameKind(player, i, j))
                    {
                        if (IsNotKanji(player, i))
                        {
                            int dist = DistanceInSame(player, i, j);
                            Debug.Assert(dist > 0);
                            if (dist == 1)
                            {
                                if (toNumberOfTile[player.hand[i]] > 1)
                                {
                                    player.chi[smallerSideTile[idWithoutRed[player.hand[i]]]] = true;
                                    if (shanten == 0)
                                    {
                                        player.machiTiles[idWithoutRed[player.hand[exceptTileId]], smallerSideTile[idWithoutRed[player.hand[i]]]] = true;
                                        if (toNumberOfTile[player.hand[j]] == 9)
                                        {
                                            player.penchanKanchanTankiIf[idWithoutRed[player.hand[exceptTileId]], smallerSideTile[idWithoutRed[player.hand[i]]]] = true;
                                        }
                                        else
                                        {
                                            player.ryanmenIf[idWithoutRed[player.hand[exceptTileId]], smallerSideTile[idWithoutRed[player.hand[i]]]] = true;
                                        }
                                    }
                                }
                                if (toNumberOfTile[player.hand[j]] < 9)
                                {
                                    player.chi[biggerSideTile[idWithoutRed[player.hand[j]]]] = true;
                                    if (shanten == 0)
                                    {
                                        player.machiTiles[idWithoutRed[player.hand[exceptTileId]], biggerSideTile[idWithoutRed[player.hand[j]]]] = true;
                                        if (toNumberOfTile[player.hand[i]] == 1)
                                        {
                                            player.penchanKanchanTankiIf[idWithoutRed[player.hand[exceptTileId]], biggerSideTile[idWithoutRed[player.hand[j]]]] = true;
                                        }
                                        else
                                        {
                                            player.ryanmenIf[idWithoutRed[player.hand[exceptTileId]], biggerSideTile[idWithoutRed[player.hand[j]]]] = true;
                                        }
                                    }
                                }
                            }
                            else if (dist == 2)
                            {
                                player.chi[biggerSideTile[idWithoutRed[player.hand[i]]]] = true;
                                if (shanten == 0)
                                {
                                    player.machiTiles[idWithoutRed[player.hand[exceptTileId]], biggerSideTile[idWithoutRed[player.hand[i]]]] = true;
                                    player.penchanKanchanTankiIf[idWithoutRed[player.hand[exceptTileId]], biggerSideTile[idWithoutRed[player.hand[i]]]] = true;
                                }
                            }
                        }
                    }
                    player.shanten = Mathf.Min(player.shanten, shanten + PairShantenCount(player, i, j));
                }
                return;
            }
        }
        
        for (int i = 0; i + 2 < player.hand.Count; i++)
        {
            if (((usedTiles >> i) & 1) == 1)
            {
                continue;
            }
            for (int j = i + 1; j + 1 < player.hand.Count; j++)
            {
                if (((usedTiles >> j) & 1) == 1)
                {
                    continue;
                }
                for (int k = j + 1; k < player.hand.Count; k++)
                {
                    if (((usedTiles >> k) & 1) == 1)
                    {
                        continue;
                    }
                    ShantenCountWithHead(player, exceptTileId, usedTiles | (1 << i) | (1 << j) | (1 << k), shanten + ThreeIdToShantenCount(player, i, j, k), headId);
                }
            }
        }
    }

    private static void ShantenCountWithTanki(Player player, int exceptTileId, int usedTiles, int shanten, int tankiId) 
    {
        if (PopCount(((1 << player.hand.Count) - 1) & ~usedTiles) == 0)
        {
            if (shanten == 0)
            {
                player.machiTiles[idWithoutRed[player.hand[exceptTileId]], idWithoutRed[player.hand[tankiId]]] = true;
                player.penchanKanchanTankiIf[idWithoutRed[player.hand[exceptTileId]], idWithoutRed[player.hand[tankiId]]] = true;
            }
            player.shanten = Mathf.Min(player.shanten, shanten);
            return;
        }
        for (int i = 0; i + 2 < player.hand.Count; i++)
        {
            if (((usedTiles >> i) & 1) == 1)
            {
                continue;
            }
            for (int j = i + 1; j + 1 < player.hand.Count; j++)
            {
                if (((usedTiles >> j) & 1) == 1)
                {
                    continue;
                }
                for (int k = j + 1; k < player.hand.Count; k++)
                {
                    if (((usedTiles >> k) & 1) == 1)
                    {
                        continue;
                    }
                    ShantenCountWithTanki(player, exceptTileId, usedTiles | (1 << i) | (1 << j) | (1 << k), shanten + ThreeIdToShantenCount(player, i, j, k), tankiId);
                }
            }
            break;
        }
    }
    private static int PairShantenCount(Player player, int i, int j)
    {
        if (idWithoutRed[player.hand[i]] == idWithoutRed[player.hand[j]])
        {
            player.pon[idWithoutRed[player.hand[i]]] = true;
            return 0;
        }
        else if (IsNotKanji(player, i) && IsSameKind(player, i, j))
        {
            int dist = DistanceInSame(player, i, j);
            if (dist == 1)
            {
                if (toNumberOfTile[player.hand[i]] > 1)
                {
                    player.chi[smallerSideTile[idWithoutRed[player.hand[i]]]] = true;
                }
                if (toNumberOfTile[player.hand[j]] < 9)
                {
                    player.chi[biggerSideTile[idWithoutRed[player.hand[j]]]] = true;
                }
                return 0;
            }
            else if (dist == 2)
            {
                player.chi[biggerSideTile[idWithoutRed[player.hand[i]]]] = true;
                return 0;
            }
        }
        return 1;
    }

    private static bool IsNotKanji(Player player, int i)
    {
        return (player.hand[i] / 10) != 3;
    }
    private static bool IsSameKind(Player player, int i, int j)
    {
        return (player.hand[i] / 10) == (player.hand[j] / 10);
    }

    private static int ThreeIdToShantenCount(Player player, int i, int j, int k)
    {
        if (IsSameKind(player, i, j))
        {
            if (IsSameKind(player, j, k))
            {
                return SameKindThreeIdToShantenCount(player, i, j, k);
            }
            else
            {
                return PairShantenCount(player, i, j) + 1;
            }
        }
        else
        {
            if (IsSameKind(player, j, k))
            {
                return PairShantenCount(player, j, k) + 1;
            }
            else
            {
                return 2;
            }
        }
    }

    private static int SameKindThreeIdToShantenCount(Player player, int i, int j, int k)
    {
        if (DistanceInSame(player, i, j) == 0 && DistanceInSame(player, j, k) == 0)
        {
            player.pon[idWithoutRed[player.hand[i]]] = true;
            return 0;
        }
        if (IsNotKanji(player, i) 
            && biggerSideTile[idWithoutRed[player.hand[i]]] == idWithoutRed[player.hand[j]] 
            && biggerSideTile[idWithoutRed[player.hand[j]]] == idWithoutRed[player.hand[k]])
        {
            PairShantenCount(player, i, j);
            PairShantenCount(player, j, k);
            PairShantenCount(player, i, k);
            return 0;
        }
        return 1 + Mathf.Min(PairShantenCount(player, i, j), PairShantenCount(player, j, k), PairShantenCount(player, i, k));
    }

    private static int DistanceInSame(Player player, int i, int j)
    {
        return toNumberOfTile[player.hand[j]] - toNumberOfTile[player.hand[i]];
    }

    public static void SevenPairsShantenCount(Player player, int exceptTileId)
    {
        if (player.hand.Count < 13)
        {
            player.shanten = Mathf.Min(player.shanten, 6);
            return;
        }
        int[] count = new int[37];
        for (int i = 0; i < 37; i++)
        {
            count[i] = 0;
        }
        for (int i = 0; i < player.hand.Count; i++)
        {
            if (i == exceptTileId)
            {
                continue;
            }
            else
            {
                count[idWithoutRed[player.hand[i]]]++;
            }
        }
        int pairCount = 0, threeTilesCount = 0, wait = -1;
        for (int tile = 0; tile < 37; tile++)
        {
            if (count[tile] >= 2)
            {
                pairCount++;
                if (count[tile] >= 3)
                {
                    threeTilesCount++;
                    wait = tile;
                }
            }
            else if (count[tile] == 1)
            {
                wait = tile;
            }
        }
        if (pairCount - threeTilesCount == 6)
        {
            player.machiTiles[idWithoutRed[player.hand[exceptTileId]], wait] = true;
        }
        player.shanten = Mathf.Min(player.shanten, 6 - pairCount + threeTilesCount);
    }

    public static void ThirteenOrphansShantenCount(Player player, int exceptTileId)
    {
        if (player.hand.Count < 13)
        {
            player.shanten = Mathf.Min(player.shanten, 13);
            return;
        }
        int[] count = new int[37];
        for (int i = 0; i < 37; i++)
        {
            count[i] = 0;
        }
        for (int i = 0; i < player.hand.Count; i++)
        {
            if (i == exceptTileId)
            {
                continue;
            }
            count[idWithoutRed[player.hand[i]]]++;
        }
        int kindCount = 0;
        int kokushiCount = 0;
        int wait = -1;
        foreach (int pai in kokushi)
        {
            if (count[pai] >= 2)
            {
                kindCount++;
                kokushiCount += count[pai];
            }
            else if (count[pai] >= 1)
            {
                kindCount++;
                kokushiCount++;
            }
            else
            {
                wait = pai;
            }
        }
        int shanten = 13 - kindCount - (kokushiCount > kindCount ? 1 : 0);
        if (shanten == 0)
        {
            if (kindCount == 13)
            {
                foreach (int pai in kokushi)
                {
                    player.machiTiles[idWithoutRed[player.hand[exceptTileId]], pai] = true;
                }
            }
            else
            {
                Debug.Assert(wait != -1);
                player.machiTiles[idWithoutRed[player.hand[exceptTileId]], wait] = true;
            }
        }
        player.shanten = Mathf.Min(player.shanten, shanten);
    }
}

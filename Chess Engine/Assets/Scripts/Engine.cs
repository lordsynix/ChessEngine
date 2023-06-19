using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Engine
{
    [HideInInspector] public static string positionFEN = "";

    public static int friendlyColor;
    public static int enemyColor;

    const int pawnValue = 100;
    const int knightValue = 300;
    const int bishopValue = 300;
    const int rookValue = 500;
    const int queenValue = 900;

    public static void GenerateGameTree()
    {

    }

    public static int Evaluate()
    {
        int material = 0;

        return material;
    }

}

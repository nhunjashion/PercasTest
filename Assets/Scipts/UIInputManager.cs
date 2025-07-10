using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIInputManager : MonoBehaviour
{
    [SerializeField] int inputMapWidth;
    [SerializeField] int inputMapHeight;
    [SerializeField] int inputWallAmount;

    public static event Action<int, int, int> OnInputMapData;

    public void GetMapWidth(string inputText)
    {
        int.TryParse(inputText, out int value);
        inputMapWidth = value;
    }

    public void GetMapHeight(string inputText)
    {
        int.TryParse(inputText, out int value);
        inputMapHeight = value;
    }

    public void GetWallAmount(string inputText)
    {
        int.TryParse(inputText, out int value);
        inputWallAmount = value;
    }

    public void OnClickCreateMap()
    {
        if (inputMapHeight == 0 || inputMapWidth == 0 || inputWallAmount == 0) return;
        else OnInputMapData?.Invoke(inputMapWidth, inputMapHeight, inputWallAmount);
    }

    public void OnClickSetStart()
    {
        GameStatus.gameStatus = 1;
    }

    public void OnClickSetEnd()
    {
        GameStatus.gameStatus = 2;
    }
}

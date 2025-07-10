using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NodeItem: MonoBehaviour
{
    public Image bg;
    public NodeData node;

    public void SetData(bool isBlock)
    {
        bg.color = isBlock ? Color.black : Color.white;
        node.isWall = isBlock;
        bg.raycastTarget = !isBlock;
    }

    public void OnSelectNode()
    {
        if (GameStatus.gameStatus == 0) return;
        else if (GameStatus.gameStatus == 1)
        {
            bg.color = Color.green;
            GameStatus.gameStatus = 0;
        }
        else
        {
            bg.color = Color.red;
            GameStatus.gameStatus = 0;
        }
    }

}
[Serializable]
public class NodeData
{
    [Header("Node Data")]
    public float gCost;
    public float hCost;
    public float FCost => gCost + hCost;

    public NodeData previousNode;
    public List<NodeData> neighbors = new();
    public bool isWall;

}
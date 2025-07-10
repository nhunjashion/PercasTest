using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SimplePathFinding : MonoBehaviour
{
    [Header("Path Finding")]
    public GridMap gridMap;
    
    [Header("Settings")]
    public Vector2Int startPosition = new Vector2Int(0, 0);
    public Vector2Int endPosition = new Vector2Int(5, 5);
    
    [Header("Visualization")]
    public Color pathColor = Color.yellow;
    public Color startColor = Color.green;
    public Color endColor = Color.red;
    
    private List<NodeItem> currentPathNodes = new List<NodeItem>();
    
    private List<Vector2Int> wallPositions = new List<Vector2Int>();
    private List<Vector2Int> pathPositions = new List<Vector2Int>();
    
    [System.Serializable]
    public class PathResult
    {
        public List<Vector2Int> path = new List<Vector2Int>();
        public Vector2Int[] pathArray = new Vector2Int[0];
        public bool pathFound = false;
        public float totalCost = 0f;
    }
    
    private void Start()
    {
        if (gridMap == null)
            gridMap = FindObjectOfType<GridMap>();
    }
    
    public void StartPathFinding()
    {
        if (gridMap == null) return;
        
        Vector2Int start = FindStartPosition();
        Vector2Int end = FindEndPosition();
        
        if (start.x == -1 || end.x == -1) return;
        
        startPosition = start;
        endPosition = end;
        
        SaveWallPositions();
        
        PathResult result = FindPath();
        
        if (result.pathFound)
        {
            SavePathPositions(result.path);
            
            VisualizePath(result);
        }
    }
    

    
    private void SaveWallPositions()
    {
        wallPositions.Clear();
        
        NodeData[] nodeArray = GetNodeArrayFromGridMap();
        if (nodeArray == null) return;
        
        int width = GetGridWidth();
        int height = GetGridHeight();
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < nodeArray.Length && nodeArray[index].isWall)
                {
                    wallPositions.Add(new Vector2Int(x, y));
                }
            }
        }
    }
    
    private void SavePathPositions(List<Vector2Int> path)
    {
        pathPositions.Clear();
        pathPositions.AddRange(path);
    }
    
    public void FindPathWithGameStatus()
    {
        Vector2Int start = FindStartPosition();
        Vector2Int end = FindEndPosition();
        
        if (start.x == -1 || end.x == -1) return;
        
        startPosition = start;
        endPosition = end;
        
        StartPathFinding();
    }
    
    private Vector2Int FindStartPosition()
    {
        if (gridMap == null) return new Vector2Int(-1, -1);
        
        List<NodeItem> map = gridMap.GetMap();
        if (map == null) return new Vector2Int(-1, -1);
        
        int width = GetGridWidth();
        
        for (int i = 0; i < map.Count; i++)
        {
            if (map[i].bg.color == Color.green)
            {
                int y = i / width;
                int x = i % width;
                return new Vector2Int(x, y);
            }
        }
        
        return new Vector2Int(-1, -1);
    }
    
    private Vector2Int FindEndPosition()
    {
        if (gridMap == null) return new Vector2Int(-1, -1);
        
        List<NodeItem> map = gridMap.GetMap();
        if (map == null) return new Vector2Int(-1, -1);
        
        int width = GetGridWidth();
        
        for (int i = 0; i < map.Count; i++)
        {
            if (map[i].bg.color == Color.red)
            {
                int y = i / width;
                int x = i % width;
                return new Vector2Int(x, y);
            }
        }
        
        return new Vector2Int(-1, -1);
    }
    

    

    
    public PathResult FindPath()
    {
        Vector2Int start = FindStartPosition();
        Vector2Int end = FindEndPosition();
        
        if (start.x == -1 || end.x == -1)
        {
            return new PathResult();
        }
        
        startPosition = start;
        endPosition = end;
        
        return FindPath(startPosition, endPosition);
    }
    
    public PathResult FindPath(Vector2Int start, Vector2Int end)
    {
        NodeData[] nodeArray = GetNodeArrayFromGridMap();
        if (nodeArray == null || nodeArray.Length == 0)
        {
            return new PathResult();
        }
        
        int width = GetGridWidth();
        int height = GetGridHeight();
        
        PathResult result = FindPathAlgorithm(nodeArray, width, height, start, end);
        
        if (result.pathFound)
        {
            VisualizePath(result);
        }
        
        return result;
    }
    
    private PathResult FindPathAlgorithm(NodeData[] nodeArray, int width, int height, Vector2Int startPos, Vector2Int endPos)
    {
        PathResult result = new PathResult();
        
        if (nodeArray == null || nodeArray.Length == 0)
        {
            return result;
        }
        
        if (startPos.x < 0 || startPos.x >= width || startPos.y < 0 || startPos.y >= height)
        {
            return result;
        }
        
        if (endPos.x < 0 || endPos.x >= width || endPos.y < 0 || endPos.y >= height)
        {
            return result;
        }
        
        int startIndex = startPos.y * width + startPos.x;
        int endIndex = endPos.y * width + endPos.x;
        
        if (nodeArray[startIndex].isWall || nodeArray[endIndex].isWall)
        {
            return result;
        }
        
        ResetAllNodes(nodeArray);
        
        List<NodeData> openSet = new List<NodeData>();
        HashSet<NodeData> closedSet = new HashSet<NodeData>();
        
        NodeData startNode = nodeArray[startIndex];
        startNode.gCost = 0;
        startNode.hCost = CalculateHeuristic(startPos, endPos);
        
        openSet.Add(startNode);
        
        while (openSet.Count > 0)
        {
            NodeData currentNode = openSet.OrderBy(n => n.FCost).ThenBy(n => n.hCost).First();
            
            if (currentNode == nodeArray[endIndex])
            {
                result.pathFound = true;
                result.path = RetracePath(currentNode, startPos, width, nodeArray);
                result.pathArray = result.path.ToArray();
                result.totalCost = currentNode.gCost;
                return result;
            }
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);
            
            Vector2Int currentPos = GetNodePosition(currentNode, nodeArray, width);
            
            List<NodeData> neighbors = GetNeighbors(currentPos, nodeArray, width, height);
            foreach (NodeData neighbor in neighbors)
            {
                if (closedSet.Contains(neighbor) || neighbor.isWall)
                    continue;
                
                float newGCost = currentNode.gCost + 1f;
                
                if (newGCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newGCost;
                    Vector2Int neighborPos = GetNodePosition(neighbor, nodeArray, width);
                    neighbor.hCost = CalculateHeuristic(neighborPos, endPos);
                    neighbor.previousNode = currentNode;
                    
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        
        return result;
    }
    
    private float CalculateHeuristic(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
    
    private Vector2Int GetNodePosition(NodeData targetNode, NodeData[] nodeArray, int width)
    {
        for (int i = 0; i < nodeArray.Length; i++)
        {
            if (nodeArray[i] == targetNode)
            {
                int y = i / width;
                int x = i % width;
                return new Vector2Int(x, y);
            }
        }
        return Vector2Int.zero;
    }
    
    private List<NodeData> GetNeighbors(Vector2Int pos, NodeData[] nodeArray, int width, int height)
    {
        List<NodeData> neighbors = new List<NodeData>();
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = pos + dir;
            
            if (neighborPos.x >= 0 && neighborPos.x < width && 
                neighborPos.y >= 0 && neighborPos.y < height)
            {
                int index = neighborPos.y * width + neighborPos.x;
                if (index < nodeArray.Length)
                {
                    neighbors.Add(nodeArray[index]);
                }
            }
        }
        
        return neighbors;
    }
    
    private void ResetAllNodes(NodeData[] nodeArray)
    {
        foreach (NodeData node in nodeArray)
        {
            if (node != null)
            {
                node.gCost = float.MaxValue;
                node.hCost = 0;
                node.previousNode = null;
            }
        }
    }
    
    private List<Vector2Int> RetracePath(NodeData endNode, Vector2Int startPos, int width, NodeData[] nodeArray)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        NodeData currentNode = endNode;
        
        while (currentNode != null)
        {
            Vector2Int nodePos = GetNodePosition(currentNode, nodeArray, width);
            path.Add(nodePos);
            currentNode = currentNode.previousNode;
        }
        
        path.Reverse();
        return path;
    }
    
    private NodeData[] GetNodeArrayFromGridMap()
    {
        if (gridMap == null) return null;
        
        List<NodeItem> map = gridMap.GetMap();
        if (map == null || map.Count == 0) return null;
        
        NodeData[] nodeArray = new NodeData[map.Count];
        for (int i = 0; i < map.Count; i++)
        {
            nodeArray[i] = map[i].node;
        }
        return nodeArray;
    }
    
    private int GetGridWidth()
    {
        if (gridMap == null) return 0;
        return gridMap.GetGridWidth();
    }
    
    private int GetGridHeight()
    {
        if (gridMap == null) return 0;
        return gridMap.GetGridHeight();
    }
    
    private void VisualizePath(PathResult result)
    {
        ClearPath();
        
        if (!result.pathFound) return;
        
        NodeData[] nodeArray = GetNodeArrayFromGridMap();
        int width = GetGridWidth();
        
        foreach (Vector2Int pos in result.path)
        {
            int index = pos.y * width + pos.x;
            if (index >= 0 && index < nodeArray.Length)
            {
                NodeItem nodeItem = FindNodeItemByData(nodeArray[index]);
                if (nodeItem != null)
                {
                    nodeItem.bg.color = pathColor;
                    currentPathNodes.Add(nodeItem);
                }
            }
        }
        
        MarkStartAndEnd();
    }
    
    private NodeItem FindNodeItemByData(NodeData nodeData)
    {
        List<NodeItem> map = gridMap.GetMap();
        if (map != null)
        {
            foreach (NodeItem nodeItem in map)
            {
                if (nodeItem.node == nodeData)
                    return nodeItem;
            }
        }
        return null;
    }
    
    private void MarkStartAndEnd()
    {
        NodeData[] nodeArray = GetNodeArrayFromGridMap();
        int width = GetGridWidth();
        
        int startIndex = startPosition.y * width + startPosition.x;
        if (startIndex >= 0 && startIndex < nodeArray.Length)
        {
            NodeItem startNode = FindNodeItemByData(nodeArray[startIndex]);
            if (startNode != null)
                startNode.bg.color = startColor;
        }
        
        int endIndex = endPosition.y * width + endPosition.x;
        if (endIndex >= 0 && endIndex < nodeArray.Length)
        {
            NodeItem endNode = FindNodeItemByData(nodeArray[endIndex]);
            if (endNode != null)
                endNode.bg.color = endColor;
        }
    }
    
    public void ClearPath()
    {
        foreach (NodeItem nodeItem in currentPathNodes)
        {
            if (nodeItem != null)
            {
                nodeItem.bg.color = nodeItem.node.isWall ? Color.black : Color.white;
            }
        }
        currentPathNodes.Clear();
    }
} 
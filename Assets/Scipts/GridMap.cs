using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridMap : MonoBehaviour
{
    [SerializeField]  List<NodeItem> map;

   // [SerializeField] int mapHeight;
   // [SerializeField] int mapWidth;
    [SerializeField] int nodeSize;
    [SerializeField] GridLayoutGroup gridMap;
    [SerializeField] NodeItem nodePrefabs;
    [SerializeField] Transform gridMapTransform;
    [SerializeField] Camera cam;
   // [SerializeField] int wallCount;

    // Thêm list để lưu vị trí tường và đường đi
    private List<Vector2Int> wallPositions = new List<Vector2Int>();
    private List<Vector2Int> pathPositions = new List<Vector2Int>();

    private void Start()
    {
        UIInputManager.OnInputMapData += CreateMap;
       // CreateMap();        
    }


    public void CreateMap(int mapHeight, int mapWidth, int wallCount)
    {
        ClearDara(gridMapTransform);
        map.Clear();
        
        gridMap.cellSize = new Vector2(nodeSize,nodeSize);
        gridMap.constraintCount = mapWidth;

        int pixel = (int)((cam.pixelWidth - mapWidth*4 + 4 - 10) / mapWidth);
        int pixel2 = (int)((cam.pixelHeight - mapHeight * 4 + 4 - 500) / mapHeight);

        int value = Mathf.Min(pixel, pixel2);
        gridMap.cellSize = new Vector2(value,value);
        
        /*        float height = (mapHeight * 50 + mapHeight * 4 - 4) * 1.7f;

                float width = (mapWidth * 50 + mapWidth * 4 - 4) * cam.pixelHeight / cam.pixelWidth;

                cam.orthographicSize = Mathf.Max(height, width) * .005f + 2;
        */


        int nodeCount = mapHeight * mapWidth;
        
        for (int i = 0; i< nodeCount; i++)
        {
            NodeItem newNode = Instantiate(nodePrefabs, gridMapTransform);
            newNode.SetData(false);
            map.Add(newNode);
        }

        for(int i = 0; i< wallCount; i++)
        {
            int index = Random.Range(0, nodeCount - 1);
            if (map[index].node.isWall) 
            {
                continue;
            }
            else 
            {
                map[index].SetData(true);
            }
        }
        
        // Lưu vị trí tường sau khi tạo map
        SaveWallPositions();
    }

    public void ClearDara(Transform group)
    {
        for (int i = 0; i < group.childCount; i++)
        {
            Destroy(group.GetChild(i).gameObject);
        }
    }
    
    // Lưu vị trí tường từ GridLayoutGroup
    public void SaveWallPositions()
    {
        wallPositions.Clear();
        
        if (map == null || map.Count == 0) return;
        
        int width = gridMap.constraintCount;
        int height = map.Count / width;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index < map.Count && map[index].node.isWall)
                {
                    wallPositions.Add(new Vector2Int(x, y));
                }
            }
        }
    }
    
    // Lưu vị trí đường đi
    public void SavePathPositions(List<Vector2Int> path)
    {
        pathPositions.Clear();
        pathPositions.AddRange(path);
    }
    

    

    
    // Lấy chiều rộng grid
    public int GetGridWidth()
    {
        return gridMap.constraintCount;
    }
    
    // Lấy chiều cao grid
    public int GetGridHeight()
    {
        return map.Count / gridMap.constraintCount;
    }
    
    // Lấy map để các script khác có thể truy cập
    public List<NodeItem> GetMap()
    {
        return map;
    }

}

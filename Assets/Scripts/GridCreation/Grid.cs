using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public LayerMask unitMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    public int obstacleProximityPenalty = 10;
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionsDict = new Dictionary<int, int>();
    Node[,] grid;

    public static float nodeDiameter;
    int gridSizeX, gridSizeY;

    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;

    static Grid instance;
    public int MaxSize { get => gridSizeX * gridSizeY; }
    void Awake() {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        foreach (TerrainType region in walkableRegions)
        {
            walkableMask.value |= region.TerrainMask.value;
            walkableRegionsDict.Add((int)Mathf.Log(region.TerrainMask.value, 2), region.terrainPenalty);
        }
        CreateGrid();
        instance = this;
    }

    void CreateGrid() {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
                int movementPenalty = 0;
                Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 100, walkableMask)) walkableRegionsDict.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                if (!walkable) movementPenalty += obstacleProximityPenalty;
                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }
        BlurPenaltyMap(3);
    }

    public static Node GetNodeFromWorldPosition(Vector3 worldPosition) {
        float percentX = (worldPosition.x + instance.gridWorldSize.x / 2) / instance.gridWorldSize.x;
        float percentY = (worldPosition.z + instance.gridWorldSize.y / 2) / instance.gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((instance.gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((instance.gridSizeY - 1) * percentY);
        return instance.grid[x, y];
    }

    void BlurPenaltyMap(int blurSize) {
        int kernelSize = blurSize * 2 + 1;
        int kernelExtends = (kernelSize - 1) / 2;

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        for (int y = 0; y < gridSizeY; y++) {
            for (int x = -kernelExtends; x <= kernelExtends; x++) {
                int sampleX = Mathf.Clamp(x, 0, kernelExtends);
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
            }

            for (int x = 1; x < gridSizeX; x++) {
                int removeIndex = Mathf.Clamp(x - kernelExtends - 1, 0, gridSizeX);
                int addIndex = Mathf.Clamp(x + kernelExtends, 0, gridSizeX - 1);
                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x-1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        for (int x = 0; x < gridSizeX; x++) {
            for (int y = -kernelExtends; y <= kernelExtends; y++) {
                int sampleY = Mathf.Clamp(y, 0, kernelExtends);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
            }

            for (int y = 1; y < gridSizeY; y++) {
                int removeIndex = Mathf.Clamp(y - kernelExtends - 1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtends, 0, gridSizeY - 1);
                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y-1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                grid[x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax) penaltyMax = blurredPenalty;
                if (blurredPenalty < penaltyMin) penaltyMin = blurredPenalty;
            }
        }
    }
    // Get All Neighbours
    public static List<Node> GetNeighbours(Node node) {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y; 
                if (checkX >= 0 && checkX < instance.gridSizeX && checkY >= 0 && checkY < instance.gridSizeY) neighbours.Add(instance.grid[checkX, checkY]); 
            }
        }
        return neighbours;
    }
    // Get neighbour closest to target position and not occupied by other units.
    public static Node GetNeighbour(Node node, Vector3 targetPos)
    {
        Node closest = null;
        float minDst = 0;
        foreach (Node n in GetNeighbours(node)) {
            if (!n.walkable || 
                Physics.CheckSphere(n.worldPosition, instance.nodeRadius, instance.unitMask) ||
                GetNodeFromWorldPosition(targetPos) == n) continue;
            if (closest == null) { closest = n; minDst = Vector3.Distance(n.worldPosition, targetPos); continue; }
            float dst = Vector3.Distance(n.worldPosition, targetPos);
            if (dst < minDst) { minDst = dst; closest = n; }
        }
        return closest;
    }
    public static Node GetRelativeNode(Node node, Vector2 offset) {
        if (node.gridX + offset.x < 0 || node.gridX + offset.x >= instance.gridWorldSize.x) return null;
        if (node.gridY + offset.y < 0 || node.gridY + offset.y >= instance.gridWorldSize.y) return null;
        return instance.grid[node.gridX + (int)offset.x, node.gridY + (int)offset.y];
    }
    void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && displayGridGizmos)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, node.movementPenalty));
                Gizmos.color = node.walkable ? Gizmos.color : Color.red;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * nodeDiameter);
            }
        }
    }

    [System.Serializable]
    public class TerrainType {
        public LayerMask TerrainMask;
        public int terrainPenalty;
    }
}

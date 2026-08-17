using System.Collections.Generic;
using UnityEngine;

/// Endless, navigable, first‑person maze that culls behind the player
/// and spawns walls only where the camera looks.
public class GroceryMaze : MonoBehaviour
{
    // ─────  Inspector  ─────────────────────────────────────────────
    [Header("References")]
    public Transform player;                    // usually Main Camera
    public List<GameObject> wallPrefabs;        // ≥1, oriented on +Z

    [Header("Geometry")]
    [Min(0.5f)] public float corridorWidth = 2f;
    [Min(0.05f)] public float wallThickness = 0.25f;

    [Header("Spawn Surface")]
    public LayerMask spawnPlaneMask = ~0; // default = “Everything”
    [Tooltip("Upward offset used when ray‑casting down to find the plane.")]
    public float rayHeight = 5f;

    [Header("Performance")]
    public int  buildRadius   = 12;
    public int  cullRadius    = 14;
    [Range(10,90)] public float viewHalfAngle = 60f;

    // read‑only cell size
    public float CellSize => corridorWidth + wallThickness;

    float wallHeight = 4f; 

    // ─────  Internals  ─────────────────────────────────────────────
    private readonly Dictionary<Vector2Int, Cell> grid = new();
    private Vector2Int currentCell;

    private readonly Vector2Int[] dirs  = {
        new (0, 1),   // N
        new (1, 0),   // E
        new (0, -1),  // S
        new (-1, 0)   // W
    };
    private const int N = 0, E = 1, S = 2, W = 3;

    private class Cell
    {
        public bool visited;
        public bool[] wallClosed = { true, true, true, true };   // N,E,S,W
        public GameObject[] wallObj = new GameObject[4];         // same order
    }

    // ─────  Unity  ────────────────────────────────────────────────
    private void Start()
    {
        if (!player) Debug.LogError("Assign Player (camera) reference.");
        if (wallPrefabs == null || wallPrefabs.Count == 0)
            Debug.LogError("Assign at least one wall prefab.");

        currentCell = WorldToCell(player.position);
        GenerateAround(currentCell);
    }

    private void Update()
    {
        Vector2Int pCell = WorldToCell(player.position);
        if (pCell != currentCell)
        {
            currentCell = pCell;
            GenerateAround(currentCell);   // extend maze
            CullFarCells();                // prune behind
        }
        RefreshVisibleWalls();             // add meshes as you turn
         CullInvisibleWalls();
    }

    // ─────  Generation  ───────────────────────────────────────────
    private void GenerateAround(Vector2Int centre)
    {
        Stack<Vector2Int> stack = new();
        EnsureCell(centre).visited = true;
        stack.Push(centre);

        while (stack.Count > 0)
        {
            Vector2Int c = stack.Pop();
            Shuffle(dirs);

            foreach (Vector2Int d in dirs)
            {
                Vector2Int n = c + d;
                if (Manhattan(n, centre) > buildRadius) continue;

                Cell neigh = EnsureCell(n);
                if (neigh.visited) continue;

                // carve passage c <-> n
                OpenWall(c, n, d);
                neigh.visited = true;
                stack.Push(n);
            }
        }
    }

    private void OpenWall(Vector2Int a, Vector2Int b, Vector2Int dir)
    {
        int wa = DirIdx(dir);
        int wb = DirIdx(-dir);

        Cell ca = grid[a];
        Cell cb = grid[b];

        ca.wallClosed[wa] = false;
        cb.wallClosed[wb] = false;

        DestroyWallObj(ref ca.wallObj[wa]);
        DestroyWallObj(ref cb.wallObj[wb]);
    }

    // ─────  Mesh instantiation / refresh  ────────────────────────
    private void RefreshVisibleWalls()
    {
        Vector3 fwdXZ = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;

        foreach (var kv in grid)
        {
            Vector2Int pos  = kv.Key;
            if (Manhattan(pos, currentCell) > buildRadius) continue;

            Vector3 toCell  = CellCenter(pos) - player.position;
            Vector3 toCellXZ= Vector3.ProjectOnPlane(toCell, Vector3.up).normalized;
            float angle     = Vector3.Angle(fwdXZ, toCellXZ);

            if (angle < viewHalfAngle)
                PlaceNeededWalls(pos, kv.Value);
        }
    }

    private void PlaceNeededWalls(Vector2Int pos, Cell cell)
    {
        // Unique‑edge rule: each physical wall lives on one cell only.
        // We choose North & East edges to own the mesh.
        if (cell.wallClosed[N] && cell.wallObj[N] == null)
            cell.wallObj[N] = SpawnWall(pos, N);
        if (cell.wallClosed[E] && cell.wallObj[E] == null)
            cell.wallObj[E] = SpawnWall(pos, E);
    }

    // ─────  Culling  ──────────────────────────────────────────────
    private void CullFarCells()
    {
        List<Vector2Int> toKill = new();
        foreach (var kv in grid)
            if (Manhattan(kv.Key, currentCell) > cullRadius)
            {
                KillCellVisuals(kv.Value);
                toKill.Add(kv.Key);
            }

        foreach (var p in toKill) grid.Remove(p);
    }

    /// Destroy any physical wall mesh that is no longer within the camera’s
/// horizontal view cone.  Logical cell data stays intact, so the mesh can be
/// re‑spawned later by RefreshVisibleWalls().
private void CullInvisibleWalls()
{
    // camera forward flattened to X‑Z so looking up/down doesn’t matter
    Vector3 fwdXZ = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;

    // Small hysteresis so rapid mouse wiggles don’t thrash meshes
    float killAngle = viewHalfAngle + 5f;   // 5° buffer

    foreach (var kv in grid)
    {
        Cell cell = kv.Value;
        for (int i = 0; i < 4; i++)
        {
            GameObject w = cell.wallObj[i];
            if (!w) continue;

            Vector3 toWall = w.transform.position - player.position;
            Vector3 toWallXZ = Vector3.ProjectOnPlane(toWall, Vector3.up).normalized;
            float angle = Vector3.Angle(fwdXZ, toWallXZ);

            if (angle > killAngle)
            {
                Destroy(w);
                cell.wallObj[i] = null;
            }
        }
    }
}

    // ─────  Helpers  ──────────────────────────────────────────────
    private Cell EnsureCell(Vector2Int p)
    {
        if (!grid.TryGetValue(p, out Cell c))
            grid[p] = c = new Cell();
        return c;
    }

    private GameObject SpawnWall(Vector2Int cell, int side)
    {
        GameObject prefab = wallPrefabs[Random.Range(0, wallPrefabs.Count)];

    Vector3 pos  = CellCorner(cell);
    Quaternion rot;
    Vector3 scale = prefab.transform.localScale;

    switch (side)
    {
        case N: // +Z edge
            pos += new Vector3(corridorWidth * .5f, 0, CellSize);
            rot  = Quaternion.identity;
            scale.z = wallThickness;
            break;

        case E: // +X edge
            pos += new Vector3(CellSize, 0, corridorWidth * .5f);
            rot  = Quaternion.Euler(0, 90, 0);
            scale.z = wallThickness;
            break;

        default: return null; // we never spawn S or W
    }

    // ── NEW: make sure there’s a valid plane directly beneath ──────────────
    Vector3 rayOrigin = pos + Vector3.up * rayHeight;
    if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit,
                         rayHeight * 2f, spawnPlaneMask, QueryTriggerInteraction.Ignore))
    {
        return null;   // no allowed ground → don’t spawn this wall
    }
    pos = hit.point + hit.normal * (wallHeight * 0.5f);
    // ───────────────────────────────────────────────────────────────────────

    GameObject w = Instantiate(prefab, pos, rot, transform);
    w.transform.localScale = scale;
    return w;
    }

    private static void DestroyWallObj(ref GameObject go)
    {
        if (go) { Object.Destroy(go); go = null; }
    }

    private void KillCellVisuals(Cell cell)
    {
        for (int i = 0; i < 4; i++) DestroyWallObj(ref cell.wallObj[i]);
    }

    // Grid maths
    private Vector2Int WorldToCell(Vector3 w)
        => new(Mathf.FloorToInt(w.x / CellSize), Mathf.FloorToInt(w.z / CellSize));

    private Vector3 CellCorner(Vector2Int c)
        => new(c.x * CellSize, 0, c.y * CellSize);

    private Vector3 CellCenter(Vector2Int c)
        => CellCorner(c) + new Vector3(corridorWidth * .5f, 0, corridorWidth * .5f);

    private static int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private static int DirIdx(Vector2Int d)
         => (d == new Vector2Int(0, 1)) ? N :
            (d == new Vector2Int(1, 0)) ? E :
            (d == new Vector2Int(0, -1))? S : W;

    private static void Shuffle<T>(IList<T> a)
    {
        for (int i = 0; i < a.Count; ++i)
        {
            int j = Random.Range(i, a.Count);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }
}

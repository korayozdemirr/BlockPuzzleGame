using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Active Level")]
    public LevelData currentLevel;

    [Header("Grid Visuals")]
    public float cellSize = 1.0f;
    public float verticalOffset = 1.5f;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject explosionEffectPrefab;
    public GameObject badgePrefab;

    private int width;
    private int height;
    private Vector3[,] cellPositions;
    private bool[,] isCellOccupied;

    private float startX;
    private float startY;

    private List<DraggablePiece> placedPieces = new List<DraggablePiece>();
    private List<GameObject> activeGridCells = new List<GameObject>();
    private List<GameObject> spawnedPieces = new List<GameObject>();
    private Dictionary<GameObject, PieceStackSlot> activeSlots = new Dictionary<GameObject, PieceStackSlot>();

    // Geri Al (Undo) için hamle kaydý
    private struct PlacementAction
    {
        public DraggablePiece piece;
        public Vector2Int[] occupiedCoords;
        public PieceStackSlot sourceSlot;
    }
    private Stack<PlacementAction> moveHistory = new Stack<PlacementAction>();

    // Puan yönetimi
    public int currentScore = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void LoadLevel(LevelData level)
    {
        currentLevel = level;
        width = level.gridWidth;
        height = level.gridHeight;

        moveHistory.Clear();
        activeSlots.Clear();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLevelUI(level.levelNumber);
            UIManager.Instance.UpdateScoreUI(currentScore);
            UIManager.Instance.ToggleNextLevelPanel(false);
        }

        ClearOldBoard();
        BuildGrid();
        SpawnPieces(level.piecesToSpawn);
    }

    private void ClearOldBoard()
    {
        foreach (GameObject cell in activeGridCells) Destroy(cell);
        activeGridCells.Clear();

        foreach (GameObject piece in spawnedPieces) Destroy(piece);
        spawnedPieces.Clear();

        placedPieces.Clear();
    }

    private void BuildGrid()
    {
        cellPositions = new Vector3[width, height];
        isCellOccupied = new bool[width, height];

        startX = -(width - 1) * cellSize / 2f;
        startY = -(height - 1) * cellSize / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(
                    startX + (x * cellSize),
                    startY + (y * cellSize) + verticalOffset,
                    0f
                );

                cellPositions[x, y] = worldPos;
                isCellOccupied[x, y] = false;

                GameObject newCell = Instantiate(cellPrefab, worldPos, Quaternion.identity);
                newCell.name = $"Cell_{x}_{y}";
                newCell.transform.SetParent(this.transform);
                activeGridCells.Add(newCell);
            }
        }
    }

    private void SpawnPieces(GameObject[] piecePrefabs)
    {
        if (piecePrefabs == null || piecePrefabs.Length == 0) return;

        Dictionary<GameObject, int> pieceCounts = new Dictionary<GameObject, int>();
        foreach (GameObject prefab in piecePrefabs)
        {
            if (pieceCounts.ContainsKey(prefab)) pieceCounts[prefab]++;
            else pieceCounts[prefab] = 1;
        }

        int uniqueSlots = pieceCounts.Count;
        float maxAvailableWidth = 4.2f;
        float spacingX = (uniqueSlots > 1) ? Mathf.Min(1.6f, maxAvailableWidth / (uniqueSlots - 1)) : 0f;
        float startX = -(uniqueSlots - 1) * spacingX / 2f;
        float spawnY = -4.2f;

        int index = 0;
        foreach (var pair in pieceCounts)
        {
            Vector3 slotPos = new Vector3(startX + (index * spacingX), spawnY, 0f);

            GameObject slotObj = new GameObject($"Slot_{pair.Key.name}");
            slotObj.transform.position = slotPos;

            PieceStackSlot slotScript = slotObj.AddComponent<PieceStackSlot>();
            slotScript.Initialize(pair.Key, pair.Value, badgePrefab);

            spawnedPieces.Add(slotObj);
            activeSlots[pair.Key] = slotScript;
            index++;
        }
    }

    public bool TryPlacePiece(Transform pieceTransform, PieceStackSlot sourceSlot)
    {
        int childCount = pieceTransform.childCount;
        Vector2Int[] targetGridCoords = new Vector2Int[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform subBlock = pieceTransform.GetChild(i);
            Vector2Int gridCoord = GetClosestGridCoord(subBlock.position);

            if (!IsValidCoord(gridCoord) || isCellOccupied[gridCoord.x, gridCoord.y])
            {
                return false;
            }

            float distance = Vector2.Distance(subBlock.position, cellPositions[gridCoord.x, gridCoord.y]);
            if (distance > cellSize * 0.6f)
            {
                return false;
            }

            targetGridCoords[i] = gridCoord;
        }

        Vector3 firstBlockWorldTarget = cellPositions[targetGridCoords[0].x, targetGridCoords[0].y];
        Vector3 offset = pieceTransform.GetChild(0).position - pieceTransform.position;
        pieceTransform.position = firstBlockWorldTarget - offset;

        for (int i = 0; i < childCount; i++)
        {
            Vector2Int coord = targetGridCoords[i];
            isCellOccupied[coord.x, coord.y] = true;
        }

        DraggablePiece pieceScript = pieceTransform.GetComponent<DraggablePiece>();
        if (pieceScript != null)
        {
            placedPieces.Add(pieceScript);

            // Hamleyi kaydet
            moveHistory.Push(new PlacementAction
            {
                piece = pieceScript,
                occupiedCoords = targetGridCoords,
                sourceSlot = sourceSlot
            });
        }

        // Puan ekle (+50)
        AddScore(50);

        CheckLevelCompletion();
        return true;
    }

    public void UndoLastMove()
    {
        if (moveHistory.Count == 0) return;

        PlacementAction lastAction = moveHistory.Pop();

        // 1. Tahtadaki hücreleri tekrar boþalt
        foreach (Vector2Int coord in lastAction.occupiedCoords)
        {
            isCellOccupied[coord.x, coord.y] = false;
        }

        // 2. Tahtadaki parçayý yok et
        placedPieces.Remove(lastAction.piece);
        Destroy(lastAction.piece.gameObject);

        // 3. Ýlgili yuvadaki sayýyý bir artýr
        if (lastAction.sourceSlot != null)
        {
            lastAction.sourceSlot.RestorePiece();
        }

        // Puan cezasý (-25)
        AddScore(-25);
    }

    private void AddScore(int amount)
    {
        currentScore = Mathf.Max(0, currentScore + amount);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreUI(currentScore);
        }
    }

    private Vector2Int GetClosestGridCoord(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - startX) / cellSize);
        int y = Mathf.RoundToInt((worldPos.y - (startY + verticalOffset)) / cellSize);
        return new Vector2Int(x, y);
    }

    private bool IsValidCoord(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;
    }

    private void CheckLevelCompletion()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!isCellOccupied[x, y]) return;
            }
        }

        AddScore(200); // Bölüm tamamlama ödülü
        StartCoroutine(ExplodeAllPiecesSequence());
    }

    private IEnumerator ExplodeAllPiecesSequence()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (DraggablePiece piece in placedPieces)
        {
            foreach (Transform block in piece.transform)
            {
                SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
                Color blockColor = sr != null ? sr.color : Color.white;

                if (explosionEffectPrefab != null)
                {
                    GameObject fx = Instantiate(explosionEffectPrefab, block.position, Quaternion.identity);
                    var main = fx.GetComponent<ParticleSystem>().main;
                    main.startColor = blockColor;
                }
            }

            Destroy(piece.gameObject);
            yield return new WaitForSeconds(0.08f);
        }

        placedPieces.Clear();
        moveHistory.Clear();

        // Otomatik geçiþ yerine tebrik panelini aç
        yield return new WaitForSeconds(0.3f);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleNextLevelPanel(true);
        }
    }
}
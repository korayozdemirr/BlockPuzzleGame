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

    [Header("Board Tray")]
    [SerializeField] private GameObject boardBackgroundPrefab;
    private GameObject activeBoardBackground;

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

    // Geri Al (Undo) icin hamle kaydi
    private struct PlacementAction
    {
        public DraggablePiece piece;
        public Vector2Int[] occupiedCoords;
        public PieceStackSlot sourceSlot;
    }
    private Stack<PlacementAction> moveHistory = new Stack<PlacementAction>();

    // Puan yonetimi
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
            if (SaveManager.Instance != null)
            {
                UIManager.Instance.UpdateHighScoreUI(SaveManager.Instance.GetHighScore());
            }
            UIManager.Instance.ToggleNextLevelPanel(false);
            UIManager.Instance.ToggleGameOverPanel(false);
        }

        ClearOldBoard();
        BuildGrid();
        SpawnPieces(level.piecesToSpawn);
    }

    public void RestartCurrentLevel()
    {
        currentScore = 0;
        if (currentLevel != null)
        {
            LoadLevel(currentLevel);
        }
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

        // Taban panosunu ozgara boyutuna gore ayarla
        UpdateBoardBackground(width, height);

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

    private void UpdateBoardBackground(int gridW, int gridH)
    {
        if (boardBackgroundPrefab == null) return;

        if (activeBoardBackground == null)
        {
            activeBoardBackground = Instantiate(boardBackgroundPrefab, transform);
            activeBoardBackground.name = "Grid_Board_Tray";
        }

        activeBoardBackground.transform.position = new Vector3(0f, verticalOffset, 0f);

        float padding = 0.35f;
        float totalWidth = (gridW * cellSize) + padding;
        float totalHeight = (gridH * cellSize) + padding;

        activeBoardBackground.transform.localScale = new Vector3(totalWidth, totalHeight, 1f);
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
        float maxAvailableWidth = 3.4f;
        float spacingX = (uniqueSlots > 1) ? Mathf.Min(1.6f, maxAvailableWidth / (uniqueSlots - 1)) : 0f;
        float startX = -(uniqueSlots - 1) * spacingX / 2f;
        float spawnY = -3.2f;

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

            moveHistory.Push(new PlacementAction
            {
                piece = pieceScript,
                occupiedCoords = targetGridCoords,
                sourceSlot = sourceSlot
            });
        }

        AddScore(50);
        FloatingScoreText.Spawn(pieceTransform.position, "+50", new Color(1f, 0.88f, 0.2f), 4.5f);

        bool isCompleted = CheckLevelCompletion();

        if (!isCompleted && !CanAnyPieceBePlaced())
        {
            TriggerGameOver();
        }

        return true;
    }

    public void UndoLastMove()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUndo();
        if (moveHistory.Count == 0) return;

        PlacementAction lastAction = moveHistory.Pop();

        foreach (Vector2Int coord in lastAction.occupiedCoords)
        {
            isCellOccupied[coord.x, coord.y] = false;
        }

        placedPieces.Remove(lastAction.piece);
        Destroy(lastAction.piece.gameObject);

        if (lastAction.sourceSlot != null)
        {
            lastAction.sourceSlot.RestorePiece();
        }

        AddScore(-25);

        // Geri alindiginda Game Over panelini kapat ve oyuncunun devam etmesine izin ver
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleGameOverPanel(false);
        }
    }

    private void AddScore(int amount)
    {
        currentScore = Mathf.Max(0, currentScore + amount);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScoreUI(currentScore);
        }

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.TryUpdateHighScore(currentScore);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHighScoreUI(SaveManager.Instance.GetHighScore());
            }
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

    private bool CheckLevelCompletion()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!isCellOccupied[x, y]) return false;
            }
        }

        AddScore(200);
        FloatingScoreText.Spawn(new Vector3(0f, verticalOffset, 0f), "+200 EXCELLENT!", new Color(0.2f, 1f, 0.4f), 5.5f);
        StartCoroutine(ExplodeAllPiecesSequence());
        return true;
    }

    public bool CanAnyPieceBePlaced()
    {
        List<GameObject> availablePrefabs = new List<GameObject>();
        foreach (var pair in activeSlots)
        {
            if (pair.Value != null && pair.Value.count > 0)
            {
                availablePrefabs.Add(pair.Key);
            }
        }

        if (availablePrefabs.Count == 0) return true;

        foreach (GameObject prefab in availablePrefabs)
        {
            List<Vector2Int> baseOffsets = GetPrefabSubBlockOffsets(prefab);
            if (baseOffsets.Count == 0) continue;

            for (int rot = 0; rot < 4; rot++)
            {
                List<Vector2Int> rotatedOffsets = RotateOffsets(baseOffsets, rot);

                for (int gx = 0; gx < width; gx++)
                {
                    for (int gy = 0; gy < height; gy++)
                    {
                        if (CanFitAt(gx, gy, rotatedOffsets))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    private List<Vector2Int> GetPrefabSubBlockOffsets(GameObject prefab)
    {
        List<Vector2Int> offsets = new List<Vector2Int>();
        if (prefab == null || prefab.transform.childCount == 0) return offsets;

        Vector3 firstChildPos = prefab.transform.GetChild(0).localPosition;
        for (int i = 0; i < prefab.transform.childCount; i++)
        {
            Vector3 pos = prefab.transform.GetChild(i).localPosition - firstChildPos;
            int rx = Mathf.RoundToInt(pos.x / cellSize);
            int ry = Mathf.RoundToInt(pos.y / cellSize);
            offsets.Add(new Vector2Int(rx, ry));
        }
        return offsets;
    }

    private List<Vector2Int> RotateOffsets(List<Vector2Int> original, int rotationSteps)
    {
        List<Vector2Int> rotated = new List<Vector2Int>();
        foreach (var coord in original)
        {
            int x = coord.x;
            int y = coord.y;
            for (int step = 0; step < rotationSteps; step++)
            {
                int temp = x;
                x = y;
                y = -temp;
            }
            rotated.Add(new Vector2Int(x, y));
        }
        return rotated;
    }

    private bool CanFitAt(int startX, int startY, List<Vector2Int> offsets)
    {
        foreach (var offset in offsets)
        {
            int targetX = startX + offset.x;
            int targetY = startY + offset.y;

            if (!IsValidCoord(new Vector2Int(targetX, targetY)) || isCellOccupied[targetX, targetY])
            {
                return false;
            }
        }
        return true;
    }

    private void TriggerGameOver()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayFail();
        FloatingScoreText.Spawn(new Vector3(0f, verticalOffset, 0f), "NO MORE MOVES!", new Color(1f, 0.2f, 0.2f), 5.5f);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleGameOverPanel(true);
        }
    }

    private IEnumerator ExplodeAllPiecesSequence()
    {
        yield return new WaitForSeconds(0.2f);

        foreach (DraggablePiece piece in placedPieces)
        {
            if (piece == null) continue;

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

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayPop();
                }

                yield return new WaitForSeconds(0.06f);
            }

            Destroy(piece.gameObject);
        }

        placedPieces.Clear();
        moveHistory.Clear();

        float waitDuration = 1.2f;
        if (AudioManager.Instance != null && AudioManager.Instance.victorySound != null)
        {
            AudioManager.Instance.PlayVictory();
            waitDuration = AudioManager.Instance.victorySound.length;
        }

        yield return new WaitForSeconds(waitDuration);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ToggleNextLevelPanel(true);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }

    [Header("Piece Prefabs")]
    public GameObject pieceSquarePrefab; // 4 blok
    public GameObject pieceLPrefab;      // 3 blok
    public GameObject pieceI3Prefab;     // 3 blok
    public GameObject pieceI2Prefab;     // 2 blok

    private int generatedLevelCounter = 3;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public LevelData GenerateNextLevel()
    {
        LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
        newLevel.levelNumber = generatedLevelCounter;
        newLevel.discoveredBy = "Player_" + Random.Range(1000, 9999);

        // Her zaman toplam alaný ÇÝFT SAYI olan tahtalar seçiyoruz (Eksik hücre kalmasýný önler)
        int w = 3;
        int h = 4; // 12 hücre

        if (generatedLevelCounter >= 6) { w = 4; h = 4; } // 16 hücre
        if (generatedLevelCounter >= 10) { w = 4; h = 5; } // 20 hücre

        newLevel.gridWidth = w;
        newLevel.gridHeight = h;

        List<GameObject> pieces = new List<GameObject>();
        bool generationSuccess = false;

        // KUSURSUZ DOÐRULAMA DÖNGÜSÜ:
        // Tahtada 1 kare bile boþ kalýrsa algoritma anýnda baþtan dener
        while (!generationSuccess)
        {
            pieces.Clear();
            bool[,] virtualBoard = new bool[w, h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (virtualBoard[x, y]) continue;

                    bool placed = false;
                    float roll = Random.value;

                    // 1. DENE: 2x2 Kare (4 Blok)
                    if (pieceSquarePrefab != null && roll < 0.35f)
                    {
                        if (x + 1 < w && y + 1 < h && !virtualBoard[x + 1, y] && !virtualBoard[x, y + 1] && !virtualBoard[x + 1, y + 1])
                        {
                            virtualBoard[x, y] = true;
                            virtualBoard[x + 1, y] = true;
                            virtualBoard[x, y + 1] = true;
                            virtualBoard[x + 1, y + 1] = true;
                            pieces.Add(pieceSquarePrefab);
                            placed = true;
                        }
                    }

                    // 2. DENE: L Parçasý (3 Blok)
                    if (!placed && pieceLPrefab != null && roll < 0.65f)
                    {
                        if (x + 1 < w && y + 1 < h && !virtualBoard[x + 1, y] && !virtualBoard[x, y + 1])
                        {
                            virtualBoard[x, y] = true;
                            virtualBoard[x + 1, y] = true;
                            virtualBoard[x, y + 1] = true;
                            pieces.Add(pieceLPrefab);
                            placed = true;
                        }
                    }

                    // 3. DENE: 3'lü Düz Çubuk (3 Blok)
                    if (!placed && pieceI3Prefab != null && roll < 0.85f)
                    {
                        if (x + 2 < w && !virtualBoard[x + 1, y] && !virtualBoard[x + 2, y])
                        {
                            virtualBoard[x, y] = true;
                            virtualBoard[x + 1, y] = true;
                            virtualBoard[x + 2, y] = true;
                            pieces.Add(pieceI3Prefab);
                            placed = true;
                        }
                    }

                    // 4. DENE: 2'li Çubuk (2 Blok - Kurtarýcý)
                    if (!placed && pieceI2Prefab != null)
                    {
                        if (x + 1 < w && !virtualBoard[x + 1, y])
                        {
                            virtualBoard[x, y] = true;
                            virtualBoard[x + 1, y] = true;
                            pieces.Add(pieceI2Prefab);
                            placed = true;
                        }
                        else if (y + 1 < h && !virtualBoard[x, y + 1])
                        {
                            virtualBoard[x, y] = true;
                            virtualBoard[x, y + 1] = true;
                            pieces.Add(pieceI2Prefab);
                            placed = true;
                        }
                    }
                }
            }

            // Doðrulama: Tüm hücreler gerçekten doldu mu?
            bool hasEmptySlot = false;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (!virtualBoard[x, y])
                    {
                        hasEmptySlot = true;
                        break;
                    }
                }
                if (hasEmptySlot) break;
            }

            // Boþluk kalmadýysa baþarýyla tamamlandý, döngüden çýk
            if (!hasEmptySlot)
            {
                generationSuccess = true;
            }
        }

        // Parça sýrasýný karýþtýr
        for (int i = 0; i < pieces.Count; i++)
        {
            GameObject temp = pieces[i];
            int randomIndex = Random.Range(i, pieces.Count);
            pieces[i] = pieces[randomIndex];
            pieces[randomIndex] = temp;
        }

        newLevel.piecesToSpawn = pieces.ToArray();
        generatedLevelCounter++;

        return newLevel;
    }
}
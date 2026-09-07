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

    public LevelData GenerateNextLevel(int targetLevelNumber = -1)
    {
        LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
        
        int lvlNum = targetLevelNumber > 0 ? targetLevelNumber : generatedLevelCounter;
        newLevel.levelNumber = lvlNum;
        generatedLevelCounter = lvlNum + 1;

        newLevel.discoveredBy = "Player_" + Random.Range(1000, 9999);

        // Her zaman toplam alani CIFT SAYI olan tahtalar seciyoruz (Eksik hucre kalmasini onler)
        int w = 3;
        int h = 4; // 12 hucre

        if (lvlNum >= 6) { w = 4; h = 4; } // 16 hucre
        if (lvlNum >= 10) { w = 4; h = 5; } // 20 hucre

        newLevel.gridWidth = w;
        newLevel.gridHeight = h;

        List<GameObject> pieces = new List<GameObject>();
        bool generationSuccess = false;

        // KUSURSUZ DOGRULAMA DUNUGUSU:
        // Tahtada 1 kare bile bos kalirsa algoritma aninda bastan dener
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

                    // 2. DENE: L Parcası (3 Blok)
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

                    // 3. DENE: 3'lu Duz Cubuk (3 Blok)
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

                    // 4. DENE: 2'li Cubuk (2 Blok - Kurtarıcı)
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

            // Dogrulama: Tum hucreler gercekten doldu mu?
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

            // Bosluk kalmadıysa basarıyla tamamlandı, dunguden cık
            if (!hasEmptySlot)
            {
                generationSuccess = true;
            }
        }

        // Parca sırasını karıstır
        for (int i = 0; i < pieces.Count; i++)
        {
            GameObject temp = pieces[i];
            int randomIndex = Random.Range(i, pieces.Count);
            pieces[i] = pieces[randomIndex];
            pieces[randomIndex] = temp;
        }

        newLevel.piecesToSpawn = pieces.ToArray();
        return newLevel;
    }
}

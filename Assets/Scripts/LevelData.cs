using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "BlockPuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;
    public string discoveredBy = "AI"; // Bu b�l�m� ilk ��zen oyuncunun ad�

    [Header("Grid Dimensions")]
    public int gridWidth = 2;
    public int gridHeight = 2;

    [Header("Pieces Given to Player")]
    public GameObject[] piecesToSpawn; // Bu seviyede oyuncunun �n�ne gelecek prefab'ler
}
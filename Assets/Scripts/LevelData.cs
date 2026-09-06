using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "BlockPuzzle/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber = 1;
    public string discoveredBy = "AI"; // Bu bölümü ilk çözen oyuncunun adý

    [Header("Grid Dimensions")]
    public int gridWidth = 2;
    public int gridHeight = 2;

    [Header("Pieces Given to Player")]
    public GameObject[] piecesToSpawn; // Bu seviyede oyuncunun önüne gelecek prefab'ler
}
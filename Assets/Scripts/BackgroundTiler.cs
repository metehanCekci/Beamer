using UnityEngine;
using System.Collections.Generic; // Required for Dictionary

public class BackgroundTiler : MonoBehaviour
{
    [Header("Settings")]
    public GameObject tilePrefab; // Our BackgroundTile prefab
    public Transform playerTransform; // Player object
    public float tileSize = 20f; // Tile size (Set in Inspector)

    // Hangi koordinatlarda hangi karonun oldu�unu tutan S�zl�k
    private Dictionary<Vector2Int, GameObject> spawnedTiles = new Dictionary<Vector2Int, GameObject>();

    private Vector2Int playerChunkPos;

    void Start()
    {
        // Ba�lang�� de�erlerini al
        playerChunkPos = new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / tileSize),
            Mathf.FloorToInt(playerTransform.position.y / tileSize)
        );
        CheckAndGenerateTiles();
    }

    void Update()
    {
        // Oyuncunun bulundu�u yeni karoyu hesapla
        Vector2Int newPlayerChunkPos = new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / tileSize),
            Mathf.FloorToInt(playerTransform.position.y / tileSize)
        );

        // E�er farkl� bir karoya ge�tiysek, yeni karolar� olu�tur
        if (newPlayerChunkPos != playerChunkPos)
        {
            playerChunkPos = newPlayerChunkPos;
            CheckAndGenerateTiles();
        }
    }

    void CheckAndGenerateTiles()
    {
        // Oyuncunun 3x3 �evresindeki 9 karoyu kontrol et
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int tilePos = playerChunkPos + new Vector2Int(x, y);

                // E�er bu koordinatta bir karo yoksa, olu�tur
                if (!spawnedTiles.ContainsKey(tilePos))
                {
                    GenerateTile(tilePos);
                }
            }
        }
    }

    void GenerateTile(Vector2Int tilePos)
    {
        // Karoyu merkeze hizal� pozisyonda olu�tur
        Vector3 position = new Vector3(
            tilePos.x * tileSize + tileSize / 2,
            tilePos.y * tileSize + tileSize / 2,
            1f // Karakterin arkas�nda kalmas� i�in Z de�eri (Player Z=0 olmal�)
        );

        GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity);
        newTile.transform.SetParent(this.transform); // GameManager'�n �ocu�u yap
        spawnedTiles.Add(tilePos, newTile);
    }
}
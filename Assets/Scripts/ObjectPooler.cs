using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // Singleton Eklemesi
    public static ObjectPooler Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;          // Havuzu tan�mlayan etiket (�rn: "Enemy")
        public GameObject prefab;   // Havuza at�lacak Prefab
        public int size;            // Havuzdaki ba�lang�� obje say�s�
    }

    public List<Pool> pools; // Olu�turulacak t�m havuzlar�n listesi
    public Dictionary<string, Queue<GameObject>> poolDictionary; // Havuzlar�n tutuldu�u yer

    void Awake()
    {
        // Singleton ayarı
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // OPTİMİZASYON: Başlangıçta obje oluşturma (Lazy Initialization)
            // Oyun başladığında ani yüklenmeyi önlemek için havuzu boş başlatıyoruz.
            // İhtiyaç duyuldukça SpawnFromPool fonksiyonu yeni objeler yaratacak.
            
            /*
            for (int i = 0; i < pool.size; i++)
            {
                // Obje yarat ve pasif yap
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            */

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // Obje isteme fonksiyonu (Instantiate yerine �a�r�l�r)
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            // Debug.LogWarning("Pool tag " + tag + " mevcut deil.");
            return null;
        }

        // Havuzda obje yoksa yeni obje yarat
        if (poolDictionary[tag].Count == 0)
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool == null || pool.prefab == null)
            {
                Debug.LogError($"Pool veya Prefab bulunamadı: {tag}");
                return null;
            }

            GameObject newObj = Instantiate(pool.prefab);
            newObj.transform.position = position;
            newObj.transform.rotation = rotation;
            newObj.SetActive(true);
            return newObj;
        }

        // Havuzdan obje al ve aktif et
        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    // Obje iade etme fonksiyonu (Destroy yerine �a�r�l�r)
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            // Debug.LogWarning("Pool tag " + tag + " mevcut de�il.");
            Destroy(objectToReturn); // Hata varsa yok et
            return;
        }

        objectToReturn.SetActive(false);
        // Objenin hareketini s�f�rla (Rigidbody kullananlar i�in temizlik)
        Rigidbody2D rb = objectToReturn.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
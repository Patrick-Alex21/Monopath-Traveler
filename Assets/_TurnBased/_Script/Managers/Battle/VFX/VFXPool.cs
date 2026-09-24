using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX; 


[System.Serializable]
public class PoolEntry
{
    public string key;
    public GameObject prefab;
    public int initialSize = 20;
}

public class VFXPool : MonoBehaviour
{
    public static VFXPool Instance { get; private set; }

    [SerializeField] private List<PoolEntry> poolEntries;

    private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        foreach (var entry in poolEntries)
        {
            _prefabs[entry.key] = entry.prefab;
            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < entry.initialSize; i++)
            {
                GameObject obj = Instantiate(entry.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            _pools[entry.key] = queue;
        }
    }

    public GameObject Get(string key, Vector3 position, Quaternion rotation = default, Vector3? localScale = null)
    {
        if (!_pools.ContainsKey(key))
        {
            Debug.LogWarning($"[VFXPool] No pool registered for key '{key}'");
            return null;
        }

        Queue<GameObject> queue = _pools[key];
        GameObject obj = queue.Count > 0 ? queue.Dequeue() : Instantiate(_prefabs[key], transform);

        obj.transform.SetPositionAndRotation(position, rotation == default ? Quaternion.identity : rotation);
        obj.transform.localScale = localScale ?? Vector3.one;
        obj.SetActive(true);

        ParticleSystem ps = obj.GetComponentInChildren<ParticleSystem>();
        if (ps != null) {
            ps.Play();
        }

        VisualEffect vfx = obj.GetComponentInChildren<VisualEffect>();
        if (vfx != null) {
            vfx.Play();
        }

        return obj;
    }

    public void Return(string key, GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        if (_pools.ContainsKey(key)) _pools[key].Enqueue(obj);
    }
}
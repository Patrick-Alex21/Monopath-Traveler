using UnityEngine;

public static class Bootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        GameObject managerPrefab = Resources.Load<GameObject>("[SYSTEM_MANAGERS]");

        if (managerPrefab != null)
        {
            GameObject managerClone = Object.Instantiate(managerPrefab);
            
            managerClone.name = "[SYSTEM_MANAGERS]";
            
            Object.DontDestroyOnLoad(managerClone);
            
        }
    }
}
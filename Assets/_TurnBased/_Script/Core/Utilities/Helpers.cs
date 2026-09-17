using UnityEngine;



public static class Helpers
{

    
    
    public static void DestroyChilderns(this Transform t)
    {
        foreach(Transform child in t)
        {
            Object.Destroy(child.gameObject);
        }
    }
}

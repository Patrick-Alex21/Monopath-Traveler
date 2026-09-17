using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Destination Scene")]
    [SerializeField] private string sceneToLoad; 
    
    [Tooltip("Destination spawn door")]
    [SerializeField] private SpawnId destinationSpawnId; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.isTransitioning)
            {
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Exploring)
            {
                return; 
            }
            
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.TransitionToScene(sceneToLoad, destinationSpawnId);
        }
    }
}

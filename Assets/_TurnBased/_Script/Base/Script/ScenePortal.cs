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

            if (GameManager.Instance.State != GameState.Exploring)
            {
                return; 
            }
            
            SceneTransitionManager.Instance.TransitionToScene(sceneToLoad, destinationSpawnId);
        }
    }
}

using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [Tooltip("Spawn ID for Transition")]
    [SerializeField] private SpawnId spawnId;

    private void Start()
    {
        if (SceneTransitionManager.Instance != null && 
            SceneTransitionManager.Instance.NextSpawnPointId != SpawnId.None && 
            SceneTransitionManager.Instance.NextSpawnPointId == spawnId)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                
                player.transform.position = transform.position;
                player.transform.rotation = transform.rotation;
                
                if (cc != null) cc.enabled = true;

                SceneTransitionManager.Instance.SetNextSpawnPointId(SpawnId.None);
            }
        }
    }
}

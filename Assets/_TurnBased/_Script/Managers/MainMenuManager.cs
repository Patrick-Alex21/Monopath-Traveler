using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "StartScene"; 
    [SerializeField] private SpawnId destinationSpawnId; 
    [SerializeField] private AudioClip mainMenuBGM;
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip clickSound;

    private void Start()
    {
        AudioSystem.Instance.PlayMusic(mainMenuBGM);
    }

    public void OnTapToStart()
    {
        Debug.Log("CLICK");
        if (AudioSystem.Instance != null && clickSound != null)
            AudioSystem.Instance.PlayUISound(clickSound);

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(sceneToLoad, destinationSpawnId);
        }
    }
}
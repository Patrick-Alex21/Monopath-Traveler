using UnityEngine;
using Unity.Cinemachine; 

public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine Settings")]
    [SerializeField] private CinemachineBrain cinemachineBrain;

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (cinemachineBrain == null) return;

        switch (newState)
        {
            case GameState.Exploring:
                cinemachineBrain.enabled = true;
                break;

            case GameState.InDialog:
            case GameState.InBattle:
                cinemachineBrain.enabled = false;
                break;
        }
    }
}

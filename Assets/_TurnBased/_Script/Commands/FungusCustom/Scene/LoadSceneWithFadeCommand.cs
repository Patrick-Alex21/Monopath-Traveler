using UnityEngine;
using UnityEngine.SceneManagement;
using Fungus.DentedPixel;

namespace Fungus
{
    [CommandInfo("Scene", 
                "Load Scene With Fade", 
                "Change Scene with Fade in and Out")]
    public class LoadSceneWithFadeCommand : Command
    {
        [Header("Scene Settings")]
        [Tooltip("Destionation scene  (Ex: EncounterParty)")]
        [SerializeField] private string targetSceneName;

        public override void OnEnter()
        {
            if (SceneTransitionManager.Instance != null)
            {
                Debug.Log("Transitions");
                SceneTransitionManager.Instance.TransitionToScene(targetSceneName, SpawnId.None); 
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }

            
        }

        public override string GetSummary()
        {
            if (string.IsNullOrEmpty(targetSceneName)) return "Error: Nama scene kosong";
            return "Fade to: " + targetSceneName;
        }

        public override Color GetButtonColor()
        {
            return new Color32(200, 200, 200, 255); 
        }
    }
}   
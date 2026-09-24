using UnityEngine;
using Unity.Cinemachine;
using Fungus.DentedPixel;

namespace Fungus
{

    [CommandInfo("Camera", 
                "Toggle Cinemachine Brain", 
                "Menyalakan atau mematikan Cinemachine Brain pada Main Camera.")]
    public class ToggleCinemachineCommand : Command
    {
        [Tooltip("Centang = Nyala (Kamera ikuti player). Kosong = Mati (Kamera bebas untuk cutscene)")]
        [SerializeField] private bool enableBrain = false;

        public override void OnEnter()
        {
            if (Camera.main != null)
            {
                CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
                if (brain != null)
                {
                    brain.enabled = enableBrain;
                }
            }
            Continue();
        }

        public override string GetSummary()
        {
            return enableBrain ? "Turn ON (Follow Mode)" : "Turn OFF (Free Mode)";
        }

        public override Color GetButtonColor()
        {
            return new Color32(200, 150, 250, 255); 
        }
    }
}
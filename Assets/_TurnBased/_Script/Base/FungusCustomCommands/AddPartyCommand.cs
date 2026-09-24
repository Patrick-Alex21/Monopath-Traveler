using UnityEngine;
using Fungus.DentedPixel;

namespace Fungus
{
    [CommandInfo("RPG Systems", 
                "Add Party Member", 
                "Add new Hero int party List in GameManager.")]
    public class AddPartyCommand : Command
    {
        [Tooltip("Pilih tipe hero yang akan dimasukkan ke dalam party")]
        [SerializeField] private HeroType heroToAdd; 

        public override void OnEnter()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddPartyMember(heroToAdd);
            }

            Continue();
        }

        public override string GetSummary()
        {
            return "Add to Party: " + heroToAdd.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(50, 200, 50, 255);
        }
    }
}
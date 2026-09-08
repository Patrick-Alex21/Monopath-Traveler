using UnityEngine;
using Fungus.DentedPixel;

namespace Fungus
{
    [CommandInfo("GameObject", 
                "Spawn Character With Flip", 
                "Spawn Object/Prefab to Scene with Position, Sprite override, and Flip options.")]
    public class SpawnCharacterWithFlip : Command
    {
        [Tooltip("Prefab or Object to Spawn")]
        [SerializeField] private GameObject sourceObject;

        [Tooltip("If filled, Object will copy the position of this transform")]
        [SerializeField] private Transform spawnAtTransform;

        [Header("Manual Coordinates (If Transform Empty)")]
        [Tooltip("Manual Position (X, Y, Z)")]
        [SerializeField] private Vector3 customPosition;

        [Header("Sprite Settings")]
        [Tooltip("Optional: Assign a specific Sprite to change what the hero looks like when spawned.")]
        [SerializeField] private Sprite customSprite;

        [Tooltip("Flip X (Left/Right)")]
        [SerializeField] private bool flipX = false;

        [Tooltip("Flip Y (Up/Down)")]
        [SerializeField] private bool flipY = false;

        [Header("Hierarchy")]
        [Tooltip("Optional: Make the new object a child of this Transform")]
        [SerializeField] private Transform parentTransform;

        [Header("Character Data (Optional - Body only, leave empty for pure props)")]
        [Tooltip("Works for Hero or Enemy data. Only used as a fallback sprite source, and only if Custom Sprite above is empty.")]
        [SerializeField] private ScriptableBaseCharacter characterData;

        [Header("NPC Data (Optional - Mind, required if this NPC should be interactable)")]
        [Tooltip("Masukkan ScriptableNPC agar NPC yang di-spawn punya data dialog/interaksi")]
        [SerializeField] private ScriptableNPC npcData;
        

        public override void OnEnter()
        {
            if (sourceObject == null)
            {
                Continue();
                return;
            }


            GameObject newObject = Instantiate(sourceObject);

            if (parentTransform != null)
            {
                newObject.transform.SetParent(parentTransform);
            }

            if (spawnAtTransform != null)
            {
                newObject.transform.position = spawnAtTransform.position;
            }
            else
            {
                newObject.transform.position = customPosition;
            }

            NPCBase npcComponent = newObject.GetComponent<NPCBase>();
            if (npcComponent != null && npcData != null)
            {
                npcComponent.SetData(npcData);
            }

            SpriteRenderer spawnedSpriteRenderer = newObject.GetComponentInChildren<SpriteRenderer>();

            if (spawnedSpriteRenderer != null)
            {
                if (customSprite != null)
                {
                    spawnedSpriteRenderer.sprite = customSprite;
                }
                else if (characterData != null && characterData.DefaultSprite != null)
                {
                    spawnedSpriteRenderer.sprite = characterData.DefaultSprite;
                }

                // Apply flip settings
                spawnedSpriteRenderer.flipX = flipX;
                spawnedSpriteRenderer.flipY = flipY;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (sourceObject == null) return "Error: No object selected";
            
            string extraInfo = "";
            
            if (customSprite != null) extraInfo += $" [Sprite: {customSprite.name}]";
            else if (characterData != null) extraInfo += $" [{characterData.name}]";
            
            if (flipX) extraInfo += " [Flip X]";
            if (flipY) extraInfo += " [Flip Y]";

            string targetText = spawnAtTransform != null ? spawnAtTransform.name : "Custom Pos";
            return $"{sourceObject.name} at {targetText}{extraInfo}";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255); 
        }
    }
}
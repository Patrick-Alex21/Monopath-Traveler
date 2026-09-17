using UnityEngine;
using UnityEngine.UI;

public class TurnIconVisual : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Sprite heroFrame;
    [SerializeField] private Sprite enemyFrame;

    [Header("Frame Colors")]
    [SerializeField] private Color heroFrameColor = new Color(0.55f, 0.75f, 1f, 1f);
    [SerializeField] private Color enemyFrameColor = new Color(1f, 0.45f, 0.45f, 1f);

    public void Setup(CharacterBase character)
    {
        if (character == null || character.CharacterData == null) return;

        if (portraitImage != null)
            portraitImage.sprite = character.CharacterData.MenuSprite;

        if (frameImage != null)
        {
            frameImage.sprite = character is HeroCharBase ? heroFrame : enemyFrame;
            frameImage.color = character is HeroCharBase ? heroFrameColor : enemyFrameColor;
        }
    }

    public void SetDimmed(bool dimmed)
    {
        Color c = dimmed ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
        if (portraitImage != null) portraitImage.color = c;
        if (frameImage != null) frameImage.color = c;
    }
} 
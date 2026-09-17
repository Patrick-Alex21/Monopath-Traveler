using UnityEngine;
using UnityEngine.UI;

public class NPCBase : MonoBehaviour, IInteractable
{
    [Header("NPC References")]
    [SerializeField] private InteractionPromptUI promptUI;
    [SerializeField] private ScriptableNPC _data;
    public ScriptableNPC Data => _data;

    private void Awake()
    {
        SetupFromData();
    }

    // ==================== TAMBAHKAN METHOD INI ====================
    public void SetData(ScriptableNPC newData)
    {
        _data = newData;
        SetupFromData();
    }

    private void SetupFromData()
    {
        if (_data == null) return;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && _data.portrait != null)
        {
            sr.sprite = _data.portrait;
        }
    }

    public void Interact()
    {
        if (Data != null && !string.IsNullOrEmpty(Data.fungusBlockName))
        {
            DialogManager.Instance.PlayDialog(Data.fungusBlockName);
        }
    }

    public void ShowPrompt()
    {
        if (promptUI != null) promptUI.ShowPrompt();
    }

    public void HidePrompt()
    {
        if (promptUI != null) promptUI.HidePrompt();
    }
}
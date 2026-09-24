using UnityEngine;


[CreateAssetMenu(fileName = "NewEventFlag", menuName = "Game/Event Flag")]
public class GameEventFlag : ScriptableObject
{
    [Header("Event Data")]
    [Tooltip("Short Description of event (opsional)")]
    [TextArea]
    public string description;
    
}
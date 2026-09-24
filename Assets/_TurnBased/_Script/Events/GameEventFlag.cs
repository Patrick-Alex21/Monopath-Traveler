using UnityEngine;


[CreateAssetMenu(fileName = "NewEventFlag", menuName = "Game/Event Flag")]
public class GameEventFlag : ScriptableObject
{
    [Tooltip("Short Description of event (opsional)")]
    [TextArea]
    public string description;
    
}
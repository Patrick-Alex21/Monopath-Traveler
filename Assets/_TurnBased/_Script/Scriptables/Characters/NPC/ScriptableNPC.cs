using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "Game/NPC")]
public class ScriptableNPC : ScriptableObject
{
    [Header("Identity")]
    public string npcName;
    public Sprite portrait;

    [Header("Fungus Integration")]
    [Tooltip("Nama Block di Fungus Flowchart yang akan dipanggil")]
    public string fungusBlockName = "Start";

    [Header("Optional Overrides")]
    public NPCBase customPrefab;
}
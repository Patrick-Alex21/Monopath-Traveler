using UnityEngine;

public class EncounterTrigger : MonoBehaviour
{
    [Header("EnemyData")]
    [SerializeField] private ScriptableEncounter encounterData; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameEvents.RequestBattle(encounterData);
            
            Destroy(gameObject); 
        }
        else
        {
            Debug.Log("Nope");
        }
    }
}

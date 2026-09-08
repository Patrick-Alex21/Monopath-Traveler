 using UnityEngine;
using Fungus;
using System.Collections;

public class AutoPlayCutscene : MonoBehaviour
{
    [Header("Event Data")]
    [SerializeField] private GameEventFlag eventFlag;
    
    [Header("Syarat / Prerequisite (Opsional)")]
    [SerializeField] private GameEventFlag prerequisiteFlag; 

    [Header("Fungus Settings")]
    [SerializeField] private Flowchart flowchart;
    [SerializeField] private string blockName;

    private IEnumerator Start()
    {
        if (ProgressManager.Instance == null) yield break;
        
        if (prerequisiteFlag != null && !ProgressManager.Instance.IsEventCompleted(prerequisiteFlag))
        {
            Destroy(gameObject);
            yield break;
        }
        if (eventFlag != null && ProgressManager.Instance.IsEventCompleted(eventFlag))
        {
            Destroy(gameObject);
            yield break; 
        }

        if (SceneTransitionManager.Instance != null)
        {
            yield return new WaitUntil(() => !SceneTransitionManager.Instance.isTransitioning);
        }
        else 
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (flowchart != null && flowchart.HasBlock(blockName))
        {
            GameManager.Instance.ChangeState(GameState.InDialog);

            if (eventFlag != null) ProgressManager.Instance.StartEvent(eventFlag);
            
            flowchart.ExecuteBlock(blockName);
            
            
            StartCoroutine(WaitForBlockEnd());
        }
    }

    private IEnumerator WaitForBlockEnd()
    {
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => flowchart.HasExecutingBlocks());
        yield return new WaitUntil(() => !flowchart.HasExecutingBlocks());

        if (GameManager.Instance.State != GameState.InBattle)
        {
            GameManager.Instance.ChangeState(GameState.Exploring);
        }
        
        Destroy(gameObject);
    }
}
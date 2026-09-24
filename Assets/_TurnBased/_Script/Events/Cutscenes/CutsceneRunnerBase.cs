using UnityEngine;
using System.Collections;
using Fungus;

public abstract class CutsceneRunnerBase : MonoBehaviour
{
    [Header("Event Data")]
    [SerializeField] protected GameEventFlag eventFlag;
    [SerializeField] protected GameEventFlag prerequisiteFlag;

    [Header("Fungus Settings")]
    [SerializeField] protected Flowchart flowchart;
    [SerializeField] protected string blockName;

    protected bool CanPlay()
    {
        if (ProgressManager.Instance == null) return false;
        if (prerequisiteFlag != null && !ProgressManager.Instance.IsEventCompleted(prerequisiteFlag)) return false;
        if (eventFlag != null && ProgressManager.Instance.IsEventCompleted(eventFlag)) return false;
        return true;
    }

    protected IEnumerator RunCutscene()
    {
        GameManager.Instance.ChangeState(GameState.InDialog);
        if (eventFlag != null) ProgressManager.Instance.StartEvent(eventFlag);

        flowchart.ExecuteBlock(blockName);

        yield return StartCoroutine(FungusBlockRunner.RunAndWaitForBlock(flowchart));
        Destroy(gameObject);
    }
}
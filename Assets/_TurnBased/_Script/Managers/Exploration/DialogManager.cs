using UnityEngine;
using Fungus;
using System.Collections;

public class DialogManager : Singleton<DialogManager>
{
    [SerializeField] private Flowchart mainFlowchart;

    public void PlayDialog(string blockName)
    {
        if (mainFlowchart == null) return;

        if (!mainFlowchart.HasExecutingBlocks())
        {
            GameManager.Instance.ChangeState(GameState.InDialog);
            mainFlowchart.ExecuteBlock(blockName);
            StartCoroutine(FungusBlockRunner.RunAndWaitForBlock(mainFlowchart));
        }
    }
  }

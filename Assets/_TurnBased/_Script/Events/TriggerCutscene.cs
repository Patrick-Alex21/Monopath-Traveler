using UnityEngine;
using System.Collections;
using Fungus;

public class TriggerCutscene : CutsceneRunnerBase
{
    private void Start()
    {
        if (!CanPlay()) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!CanPlay()) return;
        if (flowchart != null && flowchart.HasBlock(blockName))
            StartCoroutine(RunCutscene());
    }
}
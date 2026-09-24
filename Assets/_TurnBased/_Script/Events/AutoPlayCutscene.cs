using UnityEngine;
using System.Collections;
using Fungus;

public class AutoPlayCutscene : CutsceneRunnerBase
{
    private void OnEnable() => SceneTransitionManager.OnBeforeFadeIn += SpawnDuringBlackScreen;
    private void OnDisable() => SceneTransitionManager.OnBeforeFadeIn -= SpawnDuringBlackScreen;

    private void SpawnDuringBlackScreen()
    {
        if (!CanPlay()) return;
        if (flowchart != null && flowchart.HasBlock("SetupScene"))
            flowchart.ExecuteBlock("SetupScene"); // instant, no waits inside, runs while still black
    }

    private IEnumerator Start()
    {
        if (!CanPlay()) { Destroy(gameObject); yield break; }

        if (SceneTransitionManager.Instance != null)
            yield return new WaitUntil(() => !SceneTransitionManager.Instance.isTransitioning);
        else
            yield return new WaitForSeconds(0.5f);

        if (flowchart != null && flowchart.HasBlock(blockName)) // blockName = "TalkNPCs"
            yield return StartCoroutine(RunCutscene());
    }
}
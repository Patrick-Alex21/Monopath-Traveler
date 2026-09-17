using System.Collections;
using Fungus;

public static class FungusBlockRunner
{
    public static IEnumerator RunAndWaitForBlock(Flowchart flowchart)
    {
        yield return new UnityEngine.WaitForSeconds(0.2f);
        yield return new UnityEngine.WaitUntil(() => flowchart.HasExecutingBlocks());
        yield return new UnityEngine.WaitUntil(() => !flowchart.HasExecutingBlocks());

        if (GameManager.Instance.State != GameState.InBattle)
            GameManager.Instance.ChangeState(GameState.Exploring);
    }
}
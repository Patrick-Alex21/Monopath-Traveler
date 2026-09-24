using UnityEngine;
    
public class PlayerRestorePosition : MonoBehaviour{
    private void Start()
    {
        if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.isReturningFromBattle)
        {
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = SceneTransitionManager.Instance.lastPlayerPosition;

            if (cc != null) cc.enabled = true;

            SceneTransitionManager.Instance.isReturningFromBattle = false;
        }
    }
}
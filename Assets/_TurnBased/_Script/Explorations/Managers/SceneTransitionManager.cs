using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class SceneTransitionManager : PersistentSingleton<SceneTransitionManager>
{

    [Header("Scene Names")]
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string startSceneName  = "StartScene";
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("Transition Data")]
    [SerializeField] private SpawnId nextSpawnPointId = SpawnId.None;

    [Header("BGM (set per-transition by caller)")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip exploringBGM;
    [SerializeField] private AudioClip battleBGM;
    private AudioClip _pendingBGM;

    [Header("Transition UI (Fade)")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDurationExploration = 0.5f;
    [SerializeField] private float fadeDurationBattle = 0.35f;

    [Header("Loading Screen UI (Exploration only)")]
    [SerializeField] private CanvasGroup loadingPanelGroup;
    [SerializeField] private float minLoadingTime = 3.0f;   
    public static event Action OnBeforeFadeIn;
    public string lastSceneBeforeBattle;
    public Vector3 lastPlayerPosition;
    public bool isReturningFromBattle = false;
    public bool isTransitioning { get; private set; } = false;
    public string BattleSceneName  => battleSceneName;
    public SpawnId NextSpawnPointId => nextSpawnPointId;
    private AsyncOperation _preloadedOperation;
    private string _preloadedSceneName;

    protected override void Awake()
    {
        base.Awake();
        SetLoadingVisible(false);
    }

    public void SetNextSpawnPointId(SpawnId spawnId) => nextSpawnPointId = spawnId;


    public void TransitionToScene(string sceneName, SpawnId spawnId)
    {
        if (isTransitioning) return;
        SetNextSpawnPointId(spawnId);
        _pendingBGM = exploringBGM;
        StartCoroutine(TransitionRoutine(sceneName, GameState.Exploring, true, fadeDurationExploration));
    }

    public void TransitionToStartScene()
    {
        if (isTransitioning) return;
        isReturningFromBattle = false;
        if (ProgressManager.Instance != null) ProgressManager.Instance.ResetAllProgress();
        _pendingBGM = mainMenuBGM;
        StartCoroutine(TransitionRoutine(startSceneName, GameState.Exploring, true, fadeDurationExploration));
    }

    public void TransitionToMainMenu()
    {
        if (isTransitioning) return;
        isReturningFromBattle = false;
        if (ProgressManager.Instance != null) ProgressManager.Instance.ResetAllProgress();
        _pendingBGM = mainMenuBGM;
        StartCoroutine(TransitionRoutine(mainMenuScene, GameState.Exploring, true, fadeDurationExploration));
    }

    public void TransitionToBattle()
    {
        if (isTransitioning) return;
        lastSceneBeforeBattle = SceneManager.GetActiveScene().name;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) lastPlayerPosition = player.transform.position;
        isReturningFromBattle = true;
        GameManager.Instance.ChangeState(GameState.InBattle);
        _pendingBGM = battleBGM;
        StartCoroutine(TransitionRoutine(battleSceneName, GameState.InBattle, false, fadeDurationBattle));
    }


    public void PreloadReturnScene()
    {
        if (_preloadedOperation != null) return;

        string sceneToLoad = !string.IsNullOrEmpty(lastSceneBeforeBattle)
            ? lastSceneBeforeBattle : "StartScene";

        _preloadedSceneName = sceneToLoad;
        _preloadedOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        _preloadedOperation.allowSceneActivation = false;
    }

    public void ReturnFromBattle()
    {
        if (isTransitioning) return;
        string sceneToLoad = !string.IsNullOrEmpty(lastSceneBeforeBattle) ? lastSceneBeforeBattle : "StartScene";
        _pendingBGM = exploringBGM;
        StartCoroutine(TransitionRoutine(sceneToLoad, GameState.Exploring, false, fadeDurationBattle));
    }

    private IEnumerator TransitionRoutine(string targetScene, GameState targetStateAfterFade, bool useLoadingScreen, float fadeDuration)
    {
        isTransitioning = true;

        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;
            float timer = 0;
            while (timer < fadeDuration) { fadePanel.alpha = Mathf.Lerp(0, 1, timer / fadeDuration); timer += Time.deltaTime; yield return null; }
            fadePanel.alpha = 1;
        }

        if (AudioSystem.Instance != null) AudioSystem.Instance.StopMusic();

        if (useLoadingScreen) SetLoadingVisible(true);

        bool usingPreload = _preloadedOperation != null && _preloadedSceneName == targetScene;
        AsyncOperation operation = usingPreload ? _preloadedOperation : SceneManager.LoadSceneAsync(targetScene);
        if (!usingPreload) operation.allowSceneActivation = false;

        if (useLoadingScreen)
        {
            float loadingTimer = 0f;
            while (loadingTimer < minLoadingTime || operation.progress < 0.9f) { loadingTimer += Time.deltaTime; yield return null; }
        }
        else
        {
            while (operation.progress < 0.9f) yield return null;
        }

        operation.allowSceneActivation = true;
        while (!operation.isDone) yield return null;

        _preloadedOperation = null;
        _preloadedSceneName = null;

        if (useLoadingScreen) SetLoadingVisible(false);

        OnBeforeFadeIn?.Invoke();
        yield return null;

        if (fadePanel != null)
        {
            float timer = 0;
            while (timer < fadeDuration) { fadePanel.alpha = Mathf.Lerp(1, 0, timer / fadeDuration); timer += Time.deltaTime; yield return null; }
            fadePanel.alpha = 0;
            fadePanel.blocksRaycasts = false;
        }

        GameManager.Instance.ChangeState(targetStateAfterFade);
        isTransitioning = false;

        if (_pendingBGM != null && AudioSystem.Instance != null)
            AudioSystem.Instance.PlayMusic(_pendingBGM);

        _pendingBGM = null;
    }
    
    private void SetLoadingVisible(bool visible)
    {
        if (loadingPanelGroup == null) return;
        loadingPanelGroup.alpha = visible ? 1f : 0f;
        loadingPanelGroup.interactable = visible;
        loadingPanelGroup.blocksRaycasts = visible;
    }
}
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class ProgressManager : PersistentSingleton<ProgressManager>
{
    private Dictionary<GameEventFlag, EventStatus> eventProgress = new Dictionary<GameEventFlag, EventStatus>();
    public GameEventFlag CurrentActiveEvent { get; private set; }

    public void StartEvent(GameEventFlag flag)
    {
        if (flag != null)
        {
            CurrentActiveEvent = flag; 
            SetEventStatus(flag, EventStatus.InProgress);
            Debug.Log($"[ProgressManager] Memulai event & mencatat aktif: {flag.name}");
        }
    }
    public void CompleteActiveEvent()
    {
        if (CurrentActiveEvent != null)
        {
            MarkCompleted(CurrentActiveEvent);
            CurrentActiveEvent = null; 
        }
    }
    
    public EventStatus GetEventStatus(GameEventFlag flag)
    {
        
        if (!eventProgress.ContainsKey(flag))
        {
            return EventStatus.NotStarted;
        }
        return eventProgress[flag];
    }

    
    public void SetEventStatus(GameEventFlag flag, EventStatus newStatus)
    {
        if (eventProgress.ContainsKey(flag))
        {
            eventProgress[flag] = newStatus;
        }
        else
        {
            eventProgress.Add(flag, newStatus);
        }
        
        Debug.Log($"[ProgressManager] Event {flag.name} diubah menjadi {newStatus}");
    }

    
    public bool IsEventCompleted(GameEventFlag flag)
    {
        return GetEventStatus(flag) == EventStatus.Completed;
    }
    
    public void MarkCompleted(GameEventFlag flag)
    {
        if (flag != null)
        {
            SetEventStatus(flag, EventStatus.Completed);
            Debug.Log($"[ProgressManager] Event '{flag.name}' resmi diselesaikan.");
        }
    }
    public void ResetAllProgress()
    {
        eventProgress.Clear();
        CurrentActiveEvent = null; 
        
        Debug.Log("[ProgressManager] Semua memori progress telah di-reset ke 0!");
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Objective
{
    public string id;
    public string title;

    [TextArea]
    public string description;

    public bool completed;
}

public class ObjectiveManager : MonoBehaviour
{
    [Header("Lista de objetivos")]
    [SerializeField] private List<Objective> objectives = new List<Objective>();

    public event Action OnObjectivesChanged;

    public List<Objective> Objectives => objectives;

    private void Start()
    {
        NotifyObjectivesChanged();
    }

    public void CompleteObjective(string objectiveId)
    {
        Objective objective = GetObjective(objectiveId);

        if (objective == null)
        {
            Debug.LogWarning($"No existe el objetivo: {objectiveId}");
            return;
        }

        if (objective.completed)
            return;

        objective.completed = true;
        NotifyObjectivesChanged();
    }

    public void NotifyObjectivesChanged()
    {
        OnObjectivesChanged?.Invoke();
    }

    public bool IsObjectiveCompleted(string objectiveId)
    {
        Objective objective = GetObjective(objectiveId);
        return objective != null && objective.completed;
    }

    public Objective GetObjective(string objectiveId)
    {
        return objectives.Find(o => o.id == objectiveId);
    }
}

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

    // Evento para avisar a la interfaz que debe actualizarse.
    public event Action OnObjectivesChanged;

    public List<Objective> Objectives => objectives;

    private void Start()
    {
        NotifyObjectivesChanged();
    }

    public void CompleteObjective(string objectiveId)
    {
        Objective objective = objectives.Find(o => o.id == objectiveId);

        if (objective == null)
        {
            Debug.LogWarning("No existe el objetivo: " + objectiveId);
            return;
        }

        if (objective.completed)
            return;

        objective.completed = true;

        Debug.Log("Objetivo completado: " + objective.title);

        NotifyObjectivesChanged();
    }

    public void NotifyObjectivesChanged()
    {
        OnObjectivesChanged?.Invoke();
    }

    public bool IsObjectiveCompleted(string objectiveId)
    {
        Objective objective = objectives.Find(o => o.id == objectiveId);

        return objective != null && objective.completed;
    }

    public Objective GetObjective(string objectiveId)
    {
        return objectives.Find(o => o.id == objectiveId);
    }
}
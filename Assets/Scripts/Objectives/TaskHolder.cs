using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskHolder : MonoBehaviour
{
    public int TasksCompleted;
    public int TasksTotal;

    public List<TaskZone> Tasks = new List<TaskZone>();
    public bool AllTasksFinished;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.AddTasks(this);
        TasksCompleted = 0;
        TasksTotal = 0;
        AllTasksFinished = false;
    }
    public void AddTask(TaskZone task)
    {
        Tasks.Add(task);
        TasksTotal++;
    }
    public void CompletedTask(TaskZone task)
    {
        TasksCompleted++;
        task.gameObject.SetActive(false);
        if(TasksFinishCheck())
        {
            AllTasksFinished = true;
            GameManager.Instance.WinCheck(false);
        }
    }
    public bool TasksFinishCheck()
    {
        if(TasksCompleted >= TasksTotal)
        {
            return true;
        }
        return false;
    }

}

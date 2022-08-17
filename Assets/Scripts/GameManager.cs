using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Camera minimapCamera;
    public PlayerUIManager UIManager;
    public GameObject Player;
    public PlayerMovement pScript;

    public List<BasicAI> Enemies = new List<BasicAI>();

    public List<TaskHolder> AllTasks = new List<TaskHolder>();
    public TaskHolder currentTask;

    private bool isPaused = false;
    // Start is called before the first frame update
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(this.gameObject); //Avoids loading new manager
        }
        Instance = this;
    }
    public void Update()
    {
        UpdateTaskList();
        UIManager.UpdateSprintBar(pScript.currentEnergy);
    }
    public void Intro()
    {
        UIManager.UpdateDialogueText("This is the test for the dialogue\nAnd if it looks good");
    }
    public void IntroEnd()
    {
        pScript.InputAllowed = true;
    }
    public void Death()
    {
        UIManager.Death();
    }
    public void AddPlayer(GameObject player)
    {
        Player = player;
        pScript = player.GetComponent<PlayerMovement>();
    }

    public void AddEnemy(BasicAI enemy)
    {
        Enemies.Add(enemy);
    }
    public void RemoveEnemy(BasicAI enemy)
    {
        Enemies.Remove(enemy);
    }
    public GameObject GetPlayer()
    {
        return Player;
    }
    public void AttackedPlayer(float damage)
    {
        pScript.Damage(damage);
    }
    public void AddTasks(TaskHolder tasks)
    {
        AllTasks.Add(tasks);

    }
    public void UpdateTaskList()
    {
        string TaskList = "";
        foreach(TaskHolder tasks in AllTasks)
        {
            Task temp = tasks.Tasks[0].task;
            TaskList += temp.name + " (" + tasks.TasksCompleted + "/" + tasks.TasksTotal + ")\n";
        }
        UIManager.UpdateTaskText(TaskList);
    }
    public void DisplayCurrentTask(string name,float CurrentTime,float MaxTime)
    {
        UIManager.TaskEnter(name);
        UIManager.UpdateProgress(CurrentTime, MaxTime);
    }
    public void DisplayHealthWarning(bool active)
    {
        UIManager.HealthWarning(active);
    }
    public void DisplaySprintWarning(bool active)
    {
        UIManager.SprintWarning(active);
    }
    public void DisplayHurtWarning(bool active)
    {
        UIManager.HurtWarning(active);
    }
    public void UpdateFOV()
    {
        pScript.cam.fieldOfView = UIManager.FOV;
    }
    public void Pause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            AudioManager.Instance.PauseAll();
            UIManager.Pause();
            Time.timeScale = 0f;
        } else
        {
            AudioManager.Instance.ResumeAll();
            UIManager.Resume();
            Time.timeScale = 1f;
        }
    }
    public void WinCheck(bool Instant)
    {
        if (!Instant)
        {
            foreach (TaskHolder taskList in AllTasks)
            {
                if (!taskList.AllTasksFinished)
                {
                    return; //doesnt check 
                }
            }
            pScript.InputAllowed = false;
            AudioManager.Instance.Play("Win", this.gameObject);
            UIManager.Win();
        }
        else
        {
            pScript.InputAllowed = false;
            AudioManager.Instance.Play("Win", this.gameObject);
            UIManager.Win();
        }
    }
    //Finish analyzing path, billboard update then active event of all robots being down and send tasks to this robot
    //end of tasks send robot back to base
    //Event Triggers for when tasks are reached.
    //Events called for each machine being activated
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskZone : MonoBehaviour
{
    public GameObject objectiveIcon;
    public Renderer currentRenderer;

    public Material completedMat;

    public TaskHolder TaskGrouper;
    public Task task;

    public bool EnterTask;// used for roads
    public bool StartHidden; //Becomes Hidden until requirements met

    private void Start()
    {
        TaskGrouper.AddTask(this);

        currentRenderer = objectiveIcon.GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //If other entering is a player
        if(other.GetComponent<PlayerMovement>() != null){
            if(EnterTask)
            {
                CompletedCurrentTask();
            }
            else
            {
                GameManager.Instance.pScript.currentTask = this;
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerMovement>() != null)
        {
            if (GameManager.Instance.pScript.currentTask == this)
            {
                GameManager.Instance.pScript.currentTask = null;
            }
        }
    }
    public void CompletedCurrentTask()
    {
        Debug.Log("Finished task");
        GameManager.Instance.pScript.currentTask = null;
        currentRenderer.material = completedMat;
        TaskGrouper.CompletedTask(this);
    }
}

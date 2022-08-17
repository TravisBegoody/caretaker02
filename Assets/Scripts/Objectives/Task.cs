using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewTask", menuName = "Tasks/NewTask")]
public class Task : ScriptableObject
{
    public string Name;
    public string Info;

    //Requires holding down button to complete
    public bool Progress;
    //How long it takes to finish it.
    public float Time;

    public int ID;
}

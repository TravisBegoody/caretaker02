using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerMovement : MonoBehaviour
{
    public bool InputAllowed;//Allows player input or not for cutscenes and death
    public enum PlayerState
    {
        Idle, Walking, Running
    };
    public PlayerState previousState;
    public PlayerState currentState;
    //Components
    [Header("Components")]
    public Rigidbody rb;
    public Animator animator;
    public Camera cam;
    public Animator camAnimator;
    public Volume cameraWeight;

    public GameObject neck;
    public Vector3 neckOffset;
    //Modifiers
    [Header("Modifiers")]
    public float MovementSpeed;
    public float MovementSpeedMultiplier;
    public float JumpHeight;
    public float Gravity;

    public float MouseSensitivity;
    //Player Inputs
    float HorizontalInput;
    float VerticalInput;

    bool IsSprinting;

    float MouseX;
    float MouseY;

    float mouseX;
    float mouseY;

    private float maxHP = 100f;
    public float currentHealth;
    public bool isDead;

    private float maxEnergy = 100f;
    public float currentEnergy;
    public TaskZone currentTask;
    public float timeSpentOnTask = 0f;

    private bool isHurting;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        cameraWeight = cam.GetComponent<Volume>();
    }
    private void Start()
    {
        GameManager.Instance.AddPlayer(this.gameObject);

        mouseX = this.transform.rotation.eulerAngles.x;
        mouseY = this.transform.rotation.eulerAngles.y;

        currentHealth = maxHP;
        currentEnergy = maxEnergy;

        GameManager.Instance.UpdateFOV();
    }
    // Update is called once per frame
    void Update()
    {
        if (InputAllowed)
            UserInput();
        Camera();
        Health();
    }
    private void FixedUpdate()
    {
        Movement();
    }
    void UserInput()
    {
        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");

        IsSprinting = Input.GetKey(KeyCode.LeftShift) ? true : false;

        if (HorizontalInput != 0f || VerticalInput != 0f)
        {
            currentState = PlayerState.Walking;
            animator.SetBool("PlayerMoving", true);
        } else
        {
            currentState = PlayerState.Idle;
            animator.SetBool("PlayerMoving", false);
        }
        //if player is sprinting swap to running state
        currentState = IsSprinting ? PlayerState.Running : currentState;
        animator.SetBool("PlayerSprinting", IsSprinting);

        MouseX = Input.GetAxis("Mouse Y");
        MouseY = Input.GetAxis("Mouse X");

        if (currentTask != null)
        {
            GameManager.Instance.DisplayCurrentTask(currentTask.task.Name, timeSpentOnTask, currentTask.task.Time);
            if (Input.GetKey(KeyCode.F))
            {
                timeSpentOnTask += Time.deltaTime;
                AudioManager.Instance.CheckPlay("Interact");
                if (timeSpentOnTask > currentTask.task.Time)
                {
                    currentTask.CompletedCurrentTask();
                }
            } else { timeSpentOnTask = 0f; }
        }
        else
        {
            GameManager.Instance.UIManager.HideProgress();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.Pause();
        }
    }
    void Movement()
    {
        //direction of forward and back
        Vector3 movement = (VerticalInput * transform.forward) * MovementSpeed * Time.deltaTime;
        movement += (HorizontalInput * transform.right) * MovementSpeed * Time.deltaTime; //direction of left and right

        if (IsSprinting)
        {
            movement *= MovementSpeedMultiplier;

            //If the player is running while no energy it damages them instead or subtracts 20f per second
            if (currentEnergy > 0f)
                currentEnergy = Mathf.Clamp(currentEnergy - (20f * Time.deltaTime), 0f, maxEnergy);
            else //Damage 5f per second 
            {
                GameManager.Instance.DisplaySprintWarning(true);
                Damage(5f * Time.deltaTime);
            }
        }

        rb.MovePosition(this.transform.position + movement);

        //Add Sprint if not sprinting
        if (!IsSprinting && currentEnergy < 100f)
        {
            GameManager.Instance.DisplaySprintWarning(false);
            currentEnergy = Mathf.Clamp(currentEnergy + (12f * Time.deltaTime), 0f, maxEnergy);
        }
    }
    void Camera()
    {
        mouseX -= MouseX * MouseSensitivity * Time.deltaTime;
        mouseY += MouseY * MouseSensitivity * Time.deltaTime;
        mouseX = Mathf.Clamp(mouseX, -90f, 90f);

        //Rotates tanks to turn 
        Vector3 camRotation = new Vector3(mouseX, mouseY, 0f);
        Vector3 playerRotation = new Vector3(0f, mouseY, 0f);
        Vector3 neckRotation = new Vector3(transform.eulerAngles.x + neckOffset.x, transform.eulerAngles.y + neckOffset.y, -mouseX + neckOffset.z);

        cam.transform.eulerAngles = camRotation;

        this.transform.eulerAngles = playerRotation;
        neck.transform.eulerAngles = neckRotation;
    }
    void Health()
    {
        //lowest it can go is 0.1f and highest is 0.9f
        cameraWeight.weight = 1f - (((currentHealth / maxHP) * 0.8f) + 0.1f);
    }
    public void Damage(float damage)
    {
        currentHealth -= damage;
        AudioManager.Instance.CheckPlay("Hurt");
        //if isn't running then run
        if(!isHurting)
        {
            StartCoroutine(HurtFlare());
        }

        if (currentHealth <= maxHP * 0.20f) //20%hp remains
        {
            AudioManager.Instance.CheckPlay("LowHealth");
            GameManager.Instance.DisplayHealthWarning(true);
        } else
        {
            AudioManager.Instance.Stop("LowHealth");
            GameManager.Instance.DisplayHealthWarning(false);
        }
        //Hasn't died yet and health just reached
        if (currentHealth <= 0f && !isDead)
        {
            Death();
        }
    }
    public void Death()
    {
        InputAllowed = false;
        AudioManager.Instance.StopAll();
        AudioManager.Instance.Play("Death", this.gameObject);

        isDead = true;
        animator.SetTrigger("PlayerDies");

        camAnimator.enabled = true;
        camAnimator.SetTrigger("CameraDeath");

        GameManager.Instance.Death();
    }
    //When the player swaps from one movement system to another it checks
    public void MovementUpdate()
    {
        //Swaps from previous state to current state
        if (currentState != previousState)
        {
            switch (previousState)
            {
                case PlayerState.Walking:
                    AudioManager.Instance.Stop("Walking");
                    break;
                case PlayerState.Running:
                    AudioManager.Instance.Stop("Running");
                    break;
            }
            switch (currentState)
            {
                case PlayerState.Walking:
                    AudioManager.Instance.Play("Walking", this.gameObject);
                    break;
                case PlayerState.Running:
                    AudioManager.Instance.Play("Running", this.gameObject);
                    break;
            }

        }
        previousState = currentState;
    }
    IEnumerator HurtFlare()
    {
        Debug.Log("Hurt Player");
        isHurting = true;
        GameManager.Instance.DisplayHurtWarning(true);
        yield return new WaitForSeconds(1f);
        isHurting = false;
        GameManager.Instance.DisplayHurtWarning(false);

    }
}

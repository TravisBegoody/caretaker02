using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance;
    private Animator animator;
    public Animator dialogueAnimator;

    Coroutine dialogueDisplay;

    [SerializeField] private GameObject CameraImage;
    [SerializeField] private GameObject FadeImage;
    [SerializeField] private GameObject MiniMap;

    [SerializeField] private Text TaskText;
    string Tasks;
    [SerializeField] private Text DialogueText;
    string Dialogue;

    [SerializeField] private Image SprintBar;
    [SerializeField] private Image ProgressBar;
    [SerializeField] private Text ProgressText;


    [SerializeField] private Image IHealthWarning;
    [SerializeField] private Image ISprintWarning;
    [SerializeField] private Image IHurtWarning;

    [SerializeField] private GameObject DeathMenu;

    [SerializeField] private GameObject MainMenu;

    [SerializeField] private GameObject OptionsMenu;
    [SerializeField] private Text FOVText;
    [SerializeField] public int FOV;


    [SerializeField] private GameObject PauseMenu;

    [SerializeField] private GameObject WinMenu;

    [Range(0.01f, 0.5f)]
    public float ScrollSpeed = 0.1f;
    public bool skip;
    private bool ranIntro;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject); //Avoids loading new manager
            return;
        }
        //First time load
        else if (Instance != this)
        {
            Instance = this;

            animator = this.GetComponent<Animator>();
            DontDestroyOnLoad(gameObject);

            FOV = 90;
        }
    }
    private void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        animator = this.GetComponent<Animator>();

        if (scene.name == "MainMenu")
        {
            MainMenuVisibilty(true);
            DeathMenuVisibility(false);
            OptionsVisibilty(false);
            PauseMenuVisibilty(false);
            WinMenuVisibilty(false);
            GameHudVisibilty(false);
            
            AudioManager.Instance.StopAll();
            AudioManager.Instance.Play("MainMenu", this.gameObject);

            ranIntro = false;
            animator.SetBool("RanIntro", ranIntro);

            Cursor.visible = true;
        }
        else if (scene.name == "MainLevel")
        {
            MainMenuVisibilty(false);
            DeathMenuVisibility(false);
            OptionsVisibilty(false);
            PauseMenuVisibilty(false);
            WinMenuVisibilty(false);
            GameHudVisibilty(true);

            animator.SetBool("RanIntro", ranIntro);

            AudioManager.Instance.StopAll();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UIManager = this;
            }
            Cursor.visible = false;
        }

    }
    private void OnDisable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void PlayIntro()
    {
        AudioManager.Instance.Play("Intro", this.gameObject);
    }
    public void PlayIntro2()
    {
        AudioManager.Instance.Play("Intro2", this.gameObject);
    }
    public void PlayIntro3()
    {
        AudioManager.Instance.Play("Intro3", this.gameObject);
    }
    public void IntroEnd()
    {
        //DialogueText.transform.parent.gameObject.SetActive(false);
        FadeImage.SetActive(false);
        ranIntro = true;//Skips intro or not
        animator.SetBool("RanIntro", ranIntro);

        GameManager.Instance.IntroEnd();
    }
    public void UpdateDialogueText(string dialogue)
    {
        Dialogue = dialogue;
        if (dialogueDisplay != null)
        {
            StopCoroutine(DialogueRoll());
        }
        dialogueDisplay = StartCoroutine(DialogueRoll());
    }
    public void UpdateTaskText(string list)
    {
        TaskText.text = list;
    }
    public void UpdateSprintBar(float currentSprint)
    {
        Vector3 barSize = new Vector3(currentSprint / 100f, SprintBar.transform.localScale.y, SprintBar.transform.localScale.z);
        SprintBar.transform.localScale = barSize;
    }
    public void TaskEnter(string taskName)
    {
        ProgressText.gameObject.SetActive(true);
        ProgressText.text = taskName;
    }
    public void UpdateProgress(float time, float maxTime)
    {
        ProgressBar.transform.parent.gameObject.SetActive(true);
        Vector3 barSize = new Vector3(ProgressBar.transform.localScale.x, time / maxTime, ProgressBar.transform.localScale.z);
        ProgressBar.transform.localScale = barSize;
    }
    public void HideProgress()
    {
        ProgressBar.transform.parent.gameObject.SetActive(false);
    }
    public void HealthWarning(bool active)
    {
        IHealthWarning.gameObject.SetActive(active);
    }
    public void SprintWarning(bool active)
    {
        ISprintWarning.gameObject.SetActive(active);
    }
    public void HurtWarning(bool active)
    {
        IHurtWarning.gameObject.SetActive(active);
    }
    public void Death()
    {
        FadeImage.SetActive(true);
        DeathMenuVisibility(true);
        animator.SetTrigger("Death");
    }
    public void Win()
    {
        Debug.Log("WON GAME");
        FadeImage.SetActive(false);
        GameHudVisibilty(false);
        animator.enabled = true;
        WinMenuVisibilty(true);
        animator.SetTrigger("Win");
    }
    public void DeathSound()
    {
        AudioManager.Instance.Play("DeathMenu", this.gameObject);
    }
    //Shows the dialogue bit by bit
    IEnumerator DialogueRoll()
    {
        //Runs animation for opening

        DialogueText.text = "";
        dialogueAnimator.SetTrigger("Open");
        yield return new WaitForSeconds(0.8f);

        DialogueText.enabled = true;
        int i = 0;
        string loadText = "";
        while (i < Dialogue.Length)
        {
            yield return new WaitForSeconds(ScrollSpeed);
            loadText += Dialogue[i];
            DialogueText.text = loadText;
            i++;
        }
        if (i >= Dialogue.Length)
        {
            //Waits three seoncds then closes
            yield return new WaitForSeconds(2.9f);

            //Closes dialogue box
            dialogueAnimator.SetTrigger("Close");

            yield return new WaitForSeconds(0.3f);
        }
    }
    public void OnButtonClick()
    {
        AudioManager.Instance.Play("ButtonClick", this.gameObject);
    }
    public void WinButtonClick()
    {
        GameManager.Instance.WinCheck(true);
    }
    private void DeathMenuVisibility(bool visibilty)
    {
        DeathMenu.SetActive(visibilty);
    }
    private void MainMenuVisibilty(bool visibilty)
    {
        MainMenu.SetActive(visibilty);
    }
    private void GameHudVisibilty(bool visibilty)
    {
        animator.enabled = visibilty;
        CameraImage.SetActive(visibilty);
        //FadeImage.SetActive(visibilty);
        MiniMap.SetActive(visibilty);

        TaskText.transform.parent.gameObject.SetActive(visibilty);
        //DialogueText.transform.parent.gameObject.SetActive(visibilty);

        SprintBar.transform.parent.gameObject.SetActive(visibilty);
        ProgressBar.transform.parent.gameObject.SetActive(visibilty);
    }
    private void OptionsVisibilty(bool visibilty)
    {
        OptionsMenu.SetActive(visibilty);
    }
    private void PauseMenuVisibilty(bool visibilty)
    {
        PauseMenu.SetActive(visibilty);
    }
    private void WinMenuVisibilty(bool visibilty)
    {
        WinMenu.SetActive(visibilty);
    }
    public void LoadMainLevel()
    {
        Time.timeScale = 1f;
        FadeImage.SetActive(true);
        SceneManager.LoadScene("MainLevel");
    }
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    public void LoadOptions()
    {
        MainMenuVisibilty(false);
        OptionsVisibilty(true);
        PauseMenuVisibilty(false);
        GameHudVisibilty(false);
        CameraImage.SetActive(true);
    }
    public void Pause()
    {
        MainMenuVisibilty(false);
        OptionsVisibilty(false);
        PauseMenuVisibilty(true);
        GameHudVisibilty(false);
        CameraImage.SetActive(true);

        Cursor.visible = true;
    }
    public void ResumeClick()
    {
        GameManager.Instance.Pause();
    }
    public void Resume()
    {
        MainMenuVisibilty(false);
        OptionsVisibilty(false);
        PauseMenuVisibilty(false);
        GameHudVisibilty(true);

        Cursor.visible = false;
    }
    public void DiagnosticEnd()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            MainMenuVisibilty(true);
            OptionsVisibilty(false);
            PauseMenuVisibilty(false);
            GameHudVisibilty(false);
        }
        else if (SceneManager.GetActiveScene().name == "MainLevel")
        {
            MainMenuVisibilty(false);
            OptionsVisibilty(false);
            PauseMenuVisibilty(true);
            GameHudVisibilty(false);
            CameraImage.SetActive(true);
        }
    }
    public void SetSkip(bool toggle)
    {
        this.skip = toggle;
        //Skips intro or not
        if (skip)
        {
            Debug.Log("Quick Load");
            this.animator.SetBool("QuickIntro", toggle);
        }
        else
        {
            Debug.Log("Full Intro");
            animator.SetBool("QuickIntro", toggle);
        }
    }
    public void AmbienceChange(float value)
    {
        AudioManager.Instance.ambienceMultiplier = value;
        AudioManager.Instance.AmbienceVolumeChange();
    }
    public void SFXChange(float value)
    {
        AudioManager.Instance.sfxMultiplier = value;
        AudioManager.Instance.SFXVolumeChange();
    }
    public void FOVChange(float value)
    {
        FOV = (int)value; //Goes from 60f to 100f
        FOVText.text = FOV.ToString();
        if (SceneManager.GetActiveScene().name == "MainLevel")
        {
            GameManager.Instance.UpdateFOV();
        }
    }
    public void Quit()
    {
        Application.Quit();
    }
}

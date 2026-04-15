using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class OilBurnEvent : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Player")]
    [SerializeField] private Transform playerCamera;

    [Header("UI")]
    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image radialFill;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, -0.2f, 1.5f);

    [Header("Timer")]
    [SerializeField] private float scenarioTime = 10f;

    [Header("Spark")]
    [SerializeField] private ParticleSystem sparkParticles;
    [SerializeField] private AudioSource sparkAudio;

    [Header("Fire")]
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private AudioSource fireAudio;
    [SerializeField] private float fireDelay = 2f;

    [Header("Siren")]
    [SerializeField] private AudioSource sirenAudio;
    [SerializeField] private float sirenDelay = 2f;

    private bool hasTriggered = false;
    private bool isScenarioRunning = false;
    private float timer;
    private bool uiParentSet = false; // To ensure canvas parent is set only once

    void Start()
    {
        SafeSet(canvasRoot, false);
        SafeSet(retryButton, false);

        // Ensure particles are stopped and deactivated at start
        if (sparkParticles != null) sparkParticles.gameObject.SetActive(false);
        if (fireParticles != null) fireParticles.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isScenarioRunning) return;

        RunTimer();
        ForceLookAtFire();

       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;
            StartCoroutine(BurnSequence());
        }
    }

    IEnumerator BurnSequence()
    {
        PlayPS(sparkParticles, true); // Play spark particles
        PlayAudio(sparkAudio, false);

        yield return new WaitForSeconds(fireDelay);

        PlayPS(fireParticles, true); // Play fire particles
        PlayAudio(fireAudio, true);

        yield return new WaitForSeconds(sirenDelay);

        PlayAudio(sirenAudio, true);

        StartScenario();
    }

    // -------------------- SCENARIO --------------------

    void StartScenario()
    {
        isScenarioRunning = true;
        timer = scenarioTime;

        if (canvasRoot != null && playerCamera != null && !uiParentSet)
        {
            canvasRoot.SetActive(true);
            canvasRoot.transform.SetParent(playerCamera);
            canvasRoot.transform.localPosition = cameraOffset;
            canvasRoot.transform.localRotation = Quaternion.identity;
            uiParentSet = true; // Mark that the parent has been set
        }
        else if (canvasRoot != null && uiParentSet)
        {
            // If parent is already set, just activate it
            canvasRoot.SetActive(true);
        }

        SafeSet(retryButton, false);

        if (timerText != null)
        {
            timerText.color = Color.white;
            timerText.text = "Extinguish the Fire!";
        }

        if (radialFill != null)
            radialFill.fillAmount = 1f;
    }

    void RunTimer()
    {
        if (!isScenarioRunning) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            FailScenario();
            return;
        }

        if (radialFill != null)
            radialFill.fillAmount = timer / scenarioTime;

        if (timerText != null)
            timerText.text = "Extinguish the Fire\nTime: " + Mathf.Ceil(timer);
    }

    
    public void CompleteScenario()
    {
        if (!isScenarioRunning) return; 

        isScenarioRunning = false;
        hasTriggered = false;

        StopAllEffects(); 

        if (timerText != null)
        {
            timerText.text = "Task Completed!";
            timerText.color = Color.green;
        }

        if (radialFill != null)
            radialFill.fillAmount = 1f;

        SafeSet(retryButton, false);

        StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(2.5f);
        SafeSet(canvasRoot, false);
        uiParentSet = false; // Reset this if you want the canvas to re-parent on next trigger
    }

    void FailScenario()
    {
        if (!isScenarioRunning) return;

        isScenarioRunning = false;
        hasTriggered = false; // Reset for potential re-trigger if scene isn't reloaded

        StopAllEffects(); // Stop all effects on failure as well

        if (timerText != null)
        {
            timerText.text = "Failed!";
            timerText.color = Color.red;
        }

        if (radialFill != null)
            radialFill.fillAmount = 0f;

        SafeSet(retryButton, true);
    }

    // -------------------- CAMERA --------------------

    void ForceLookAtFire()
    {
        if (playerCamera == null) return;

        Vector3 dir = (transform.position - playerCamera.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        playerCamera.rotation = Quaternion.Slerp(
            playerCamera.rotation,
            targetRot,
            Time.deltaTime * 5f
        );
    }

    // -------------------- HELPERS --------------------

    void PlayPS(ParticleSystem ps, bool activateGameObject)
    {
        if (ps != null)
        {
            if (activateGameObject) ps.gameObject.SetActive(true);
            ps.Play();
        }
    }

    void StopPS(ParticleSystem ps)
    {
        if (ps != null)
        {
            ps.Stop();
            ps.gameObject.SetActive(false); // Deactivate the game object
        }
    }

    void PlayAudio(AudioSource audio, bool loop)
    {
        if (audio != null)
        {
            audio.loop = loop;
            audio.Play();
        }
    }

    void StopAllEffects()
    {
        StopPS(sparkParticles);
        StopPS(fireParticles);
        if (sparkAudio != null) sparkAudio.Stop();
        if (fireAudio != null) fireAudio.Stop();
        if (sirenAudio != null) sirenAudio.Stop();
    }

    void SafeSet(GameObject obj, bool state)
    {
        if (obj != null)
            obj.SetActive(state);
    }

    // -------------------- RETRY --------------------

    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
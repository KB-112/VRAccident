using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FireExtinguisherSystem : MonoBehaviour
{
    [Header("Grab Check")]
    [SerializeField] private XRGrabWithOffset grabModule;
    [SerializeField] private XRGrabInteractable grabInteractable;

    [Header("Spray")]
    [SerializeField] private ParticleSystem sprayParticles;
    [SerializeField] private AudioSource sprayAudio;

    [Header("Fire & Smoke")]
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private ParticleSystem smokeParticles;

    [Header("Timing")]
    [SerializeField] private float requiredSprayTime = 10f;
    [SerializeField] private float smokeFadeSpeed = 2f;

    private bool isSpraying = false;
    private float sprayTimer = 0f;

    private float fireStartSize;
    private float smokeStartSize;
    public OilBurnEvent oilBurnEvent;

    void Update()
    {
        if (grabModule == null || grabInteractable == null)
            return;

        // Start Spray (ONLY if this object is held)
        if (grabModule.IsHeld &&
            grabModule.CurrentInteractable == grabInteractable &&
            Input.GetKeyDown(KeyCode.F) &&
            !isSpraying)
        {
            StartSpray();
        }

        // Stop Spray
        if (grabModule.CurrentInteractable == grabInteractable &&
            Input.GetKeyUp(KeyCode.F) &&
            isSpraying)
        {
            StopSpray();
        }

        // While spraying
        if (isSpraying)
        {
            sprayTimer += Time.deltaTime;

            if (fireParticles != null)
            {
                var fireMain = fireParticles.main;
                fireMain.startSize = Mathf.Lerp(fireStartSize, 0f, sprayTimer / requiredSprayTime);
            }

            if (smokeParticles != null)
            {
                var smokeMain = smokeParticles.main;
                smokeMain.startSize = Mathf.Lerp(
                    smokeMain.startSize.constant,
                    smokeStartSize,
                    Time.deltaTime * 2f
                );
            }

            if (sprayTimer >= requiredSprayTime)
            {
                CompleteExtinguish();
            }
        }
        else
        {
            if (smokeParticles != null)
            {
                var smokeMain = smokeParticles.main;
                smokeMain.startSize = Mathf.Lerp(
                    smokeMain.startSize.constant,
                    0f,
                    Time.deltaTime * smokeFadeSpeed
                );
            }
        }
    }

    void StartSpray()
    {
        isSpraying = true;
        sprayTimer = 0f;

        if (sprayParticles != null)
            sprayParticles.Play();

        if (sprayAudio != null)
        {
            sprayAudio.loop = true;
            sprayAudio.Play();
        }

        if (fireParticles != null)
            fireStartSize = fireParticles.main.startSize.constant;

        if (smokeParticles != null)
        {
            smokeStartSize = smokeParticles.main.startSize.constant;
            smokeParticles.Play();
        }
    }

    void StopSpray()
    {
        isSpraying = false;

        if (sprayParticles != null)
            sprayParticles.Stop();

        if (sprayAudio != null)
            sprayAudio.Stop();
    }

    void CompleteExtinguish()
    {
        StopSpray();

        if (fireParticles != null)
            fireParticles.Stop();

        if (smokeParticles != null)
            smokeParticles.Stop();

        oilBurnEvent.CompleteScenario();
    }
}
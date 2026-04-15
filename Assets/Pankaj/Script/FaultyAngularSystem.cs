using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FaultyAngularSystem : MonoBehaviour
{
    [SerializeField] private XRGrabWithOffset grabModule;
    [SerializeField] private XRGrabInteractable grabInteractable;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem inspectParticles;

    private bool isInspecting = false;

    void Update()
    {
        if (grabModule == null || grabInteractable == null)
            return;

        if (grabModule.IsHeld &&
    grabModule.CurrentInteractable == grabInteractable &&
    Input.GetKeyDown(KeyCode.F) &&
    !isInspecting)
        {
            StartCoroutine(InspectRoutine());
        }
    }

    IEnumerator InspectRoutine()
    {
        isInspecting = true;

        grabInteractable.trackRotation = false;

        Transform obj = grabInteractable.transform;

        if (audioSource != null)
            audioSource.Play();

        if (inspectParticles != null)
            inspectParticles.Play();

        float timer = 0f;
        float duration = (audioSource != null && audioSource.clip != null)
            ? audioSource.clip.length
            : 2f;

        Quaternion startRot = obj.rotation;

     
        Quaternion particleInitialRotation = Quaternion.identity;
        if (inspectParticles != null)
        {
            particleInitialRotation = inspectParticles.transform.rotation;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float yaw = Mathf.Sin(timer * 2f) * 90;

            obj.rotation = Quaternion.Euler(
                startRot.eulerAngles.x,
                startRot.eulerAngles.y ,
                startRot.eulerAngles.z + yaw
            );

           
            if (inspectParticles != null)
            {
                inspectParticles.transform.rotation = particleInitialRotation;
            }

            yield return null;
        }

        float returnTime = 0.2f;
        float r = 0f;
        Quaternion currentRot = obj.rotation;

        while (r < returnTime)
        {
            r += Time.deltaTime;
            obj.rotation = Quaternion.Slerp(currentRot, startRot, r / returnTime);

          
            if (inspectParticles != null)
            {
                inspectParticles.transform.rotation = particleInitialRotation;
            }

            yield return null;
        }

        if (inspectParticles != null)
            inspectParticles.Stop();

        grabInteractable.trackRotation = true;

        isInspecting = false;
    }
}
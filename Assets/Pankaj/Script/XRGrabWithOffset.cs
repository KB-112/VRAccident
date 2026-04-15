using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRGrabWithOffset : MonoBehaviour
{
    [System.Serializable]
    public class GrabItem
    {
        public string name;
        public XRGrabInteractable grabInteractable;
        public NearFarInteractor interactor;

        public Vector3 positionOffset;
        public Vector3 rotationOffset;

        [HideInInspector] public Vector3 originalPosition;
        [HideInInspector] public Quaternion originalRotation;
    }

    [SerializeField] private List<GrabItem> grabItems = new List<GrabItem>();

    private GrabItem currentItem;
    private bool isHeld = false;

    public bool IsHeld => isHeld;
    public XRGrabInteractable CurrentInteractable => currentItem?.grabInteractable;

    void Start()
    {

        foreach (var item in grabItems)
        {
            if (item.grabInteractable != null)
            {
                item.originalPosition = item.grabInteractable.transform.position;
                item.originalRotation = item.grabInteractable.transform.rotation;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (!isHeld)
                TryGrab();
            else
                Release();
        }
    }

    void TryGrab()
    {
        foreach (var item in grabItems)
        {
            if (item.interactor == null || item.grabInteractable == null)
                continue;

            var hovered = item.interactor.GetOldestInteractableHovered();

            if (hovered != null && hovered.transform == item.grabInteractable.transform)
            {
                currentItem = item;
                StartCoroutine(GrabRoutine(item));
                return;
            }
        }
    }

    IEnumerator GrabRoutine(GrabItem item)
    {
        yield return null;

        item.interactor.StartManualInteraction((IXRSelectInteractable)item.grabInteractable);
        isHeld = true;

        yield return null;

        if (item.interactor.attachTransform != null)
        {
            Transform t = item.interactor.transform;
            Transform attach = item.interactor.attachTransform;

            attach.position =
                t.position +
                t.right * item.positionOffset.x +
                t.up * item.positionOffset.y +
                t.forward * item.positionOffset.z;

            attach.rotation = t.rotation * Quaternion.Euler(item.rotationOffset);
        }
    }

    void Release()
    {
        if (currentItem != null && currentItem.interactor != null)
        {
            var interactor = currentItem.interactor;
            var interactable = currentItem.grabInteractable;



            if (interactor.hasSelection)
            {
                interactor.interactionManager.SelectExit(
                    (IXRSelectInteractor)interactor,
                    (IXRSelectInteractable)interactable
                );
            }


            interactor.EndManualInteraction();


            Transform obj = interactable.transform;
            obj.position = currentItem.originalPosition;
            obj.rotation = currentItem.originalRotation;
        }

        isHeld = false;
        currentItem = null;
    }
}

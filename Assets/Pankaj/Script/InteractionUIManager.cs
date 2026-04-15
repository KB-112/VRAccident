using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class InteractionUIManager : MonoBehaviour
{
    [Header("XR")]
    [SerializeField] private NearFarInteractor interactor;

    [Header("Camera")]
    [SerializeField] private Transform playerCamera;

    [Header("Canvas Offset")]
    [SerializeField] private Vector3 objectOffset = new Vector3(0, 0.3f, 0);

    [System.Serializable]
    public class LayerInfo
    {
        public LayerMask layer;
        public string infoText;
    }

    [Header("Layer Info")]
    [SerializeField] private List<LayerInfo> layerInfos;

    [Header("UI")]
    [SerializeField] private GameObject canvasRoot;
    [SerializeField] private TextMeshProUGUI infoText;

    void Start()
    {
        if (canvasRoot != null)
            canvasRoot.SetActive(false);
    }

    void Update()
    {
        HandleHoverInfo();
        UpdateCanvasOverObject();
    }

    // -------------------- HOVER INFO --------------------

    void HandleHoverInfo()
    {
        if (interactor == null)
            return;

        var hovered = interactor.GetOldestInteractableHovered();

        if (hovered == null)
        {
            canvasRoot.SetActive(false);
            return;
        }

        GameObject obj = hovered.transform.gameObject;

        foreach (var info in layerInfos)
        {
            if (((1 << obj.layer) & info.layer) != 0)
            {
                canvasRoot.SetActive(true);
                infoText.text = info.infoText;
                return;
            }
        }

        canvasRoot.SetActive(false);
    }

    // -------------------- CANVAS OVER OBJECT --------------------

    void UpdateCanvasOverObject()
    {
        if (canvasRoot == null || playerCamera == null || interactor == null)
            return;

        var hovered = interactor.GetOldestInteractableHovered();

        if (hovered == null)
            return;

        Transform target = hovered.transform;

        Vector3 topPosition = target.position;

        // Use collider height
        Collider col = target.GetComponent<Collider>();
        if (col != null)
        {
            topPosition = col.bounds.center + new Vector3(0, col.bounds.extents.y, 0);
        }
        else
        {
            Renderer rend = target.GetComponent<Renderer>();
            if (rend != null)
            {
                topPosition = rend.bounds.center + new Vector3(0, rend.bounds.extents.y, 0);
            }
        }

        canvasRoot.transform.position = topPosition + objectOffset;

        // Face camera
        canvasRoot.transform.rotation = Quaternion.LookRotation(
            canvasRoot.transform.position - playerCamera.position
        );
    }
}
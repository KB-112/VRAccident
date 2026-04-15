using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class EvidenceCollector : MonoBehaviour
{
    [SerializeField] private NearFarInteractor interactor;

    [SerializeField] private LayerMask evidenceLayer;
    [SerializeField] private int maxEvidence = 4;

    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image progressImage;

    [SerializeField] private GameObject progressCanvas;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Vector3 showOffset = new Vector3(0, 0, 2f);

    private int currentEvidence = 0;
    private bool isCanvasVisible = false;

    void Start()
    {
        UpdateUI();

        if (progressCanvas != null)
            progressCanvas.SetActive(false);
    }

    void Update()
    {
        if (interactor == null)
            return;

        if (Input.GetKeyDown(KeyCode.C))
            TryCollect();

        if (Input.GetKeyDown(KeyCode.R))
            ToggleCanvas();
    }

    void TryCollect()
    {
        var hovered = interactor.GetOldestInteractableHovered();

        if (hovered == null)
            return;

        GameObject obj = hovered.transform.gameObject;

        if (((1 << obj.layer) & evidenceLayer) == 0)
            return;

        if (currentEvidence >= maxEvidence)
            return;

        CollectEvidence(obj);
    }

    void CollectEvidence(GameObject obj)
    {
        currentEvidence++;
        Destroy(obj);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (progressText != null)
            progressText.text = currentEvidence + " / " + maxEvidence + " Evidence";

        if (progressImage != null)
            progressImage.fillAmount = (float)currentEvidence / maxEvidence;
    }

    void ToggleCanvas()
    {
        if (progressCanvas == null || playerCamera == null)
            return;

        isCanvasVisible = !isCanvasVisible;

        if (isCanvasVisible)
        {
            progressCanvas.SetActive(true);

            Transform canvasTransform = progressCanvas.transform;

            canvasTransform.position =
                playerCamera.position +
                playerCamera.forward * showOffset.z +
                playerCamera.up * showOffset.y;

            canvasTransform.LookAt(playerCamera);
            canvasTransform.forward *= -1;
        }
        else
        {
            progressCanvas.SetActive(false);
        }
    }
}
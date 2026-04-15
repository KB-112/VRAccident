using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors    ;
public class WarningTape : MonoBehaviour
{
    [Header("XR Setup")]
    public XRBaseInteractor interactor;

    [Header("Setup")]
    public GameObject warningTapePrefab;
   

    private Transform firstCone = null;
    private Transform currentHovered = null;

    private bool isTapeSelected = false;
    private GameObject previewTape = null;

    private List<GameObject> spawnedTapes = new List<GameObject>();

    void OnEnable()
    {
        interactor.hoverEntered.AddListener(OnHoverEnter);
        interactor.hoverExited.AddListener(OnHoverExit);
    }

    void OnDisable()
    {
        interactor.hoverEntered.RemoveListener(OnHoverEnter);
        interactor.hoverExited.RemoveListener(OnHoverExit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            HandleInput();
        }

        UpdatePreview();
    }

    void HandleInput()
    {
        if (!isTapeSelected)
        {
            if (currentHovered != null &&
                currentHovered.gameObject.layer == LayerMask.NameToLayer("YellowTape"))
            {
                isTapeSelected = true;
                Debug.Log("Tape selected. Select cones.");
            }
            return;
        }

        TrySelectCone();
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        currentHovered = args.interactableObject.transform;

        int coneLayer = LayerMask.NameToLayer("Cone");
        int tapeLayer = LayerMask.NameToLayer("YellowTape");

        if (currentHovered.gameObject.layer == tapeLayer)
        {
            SetRayColor(Color.cyan);
        }
        else if (currentHovered.gameObject.layer == coneLayer && isTapeSelected)
        {
            SetRayColor(Color.green);
        }

       
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        if (args.interactableObject.transform == currentHovered)
        {
            currentHovered = null;
        }

        SetRayColor(Color.white);
       
    }

    void TrySelectCone()
    {
        if (currentHovered == null)
            return;

        if (currentHovered.gameObject.layer != LayerMask.NameToLayer("Cone"))
            return;

        if (firstCone == null)
        {
            firstCone = currentHovered;
            Debug.Log("First cone selected: " + firstCone.name);

            previewTape = Instantiate(warningTapePrefab);
        }
        else if (currentHovered != firstCone)
        {
            Debug.Log("Second cone selected: " + currentHovered.name);

            CreateTape(firstCone, currentHovered);

            if (previewTape != null)
                Destroy(previewTape);

            previewTape = null;

            firstCone = currentHovered;
        }
    }

    void UpdatePreview()
    {
        if (previewTape == null || firstCone == null || currentHovered == null)
            return;

        if (currentHovered.gameObject.layer != LayerMask.NameToLayer("Cone"))
            return;

        SetupTape(previewTape.transform, firstCone.position, currentHovered.position);
    }

    void CreateTape(Transform a, Transform b)
    {
        GameObject tape = Instantiate(warningTapePrefab);
        SetupTape(tape.transform, a.position, b.position);
        spawnedTapes.Add(tape);
    }

    void SetupTape(Transform tape, Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        Vector3 mid = (start + end) * 0.5f;
        mid.y += 0.5f;

        tape.position = mid;

        // Adjust rotation depending on prefab axis
        tape.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 90, 0);

        // Scale along X axis (length)
        tape.localScale = new Vector3(distance, 0.2f, 0.03f);
    }

    void SetRayColor(Color color)
    {
        var visual = interactor.GetComponent("XRInteractorLineVisual");

        if (visual != null)
        {
            var prop = visual.GetType().GetProperty("validColorGradient");
            if (prop != null)
            {
                Gradient g = new Gradient();
                g.SetKeys(
                    new GradientColorKey[] {
                        new GradientColorKey(color, 0f),
                        new GradientColorKey(color, 1f)
                    },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 1f)
                    }
                );

                prop.SetValue(visual, g);
            }
        }
    }
}
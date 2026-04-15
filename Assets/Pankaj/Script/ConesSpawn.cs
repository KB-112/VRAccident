using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ConesSpawner : MonoBehaviour
{
    [SerializeField] private NearFarInteractor interactor;
    [SerializeField] private GameObject prefabToSpawn;

    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0, 0.5f);

    
    [SerializeField] private LayerMask spawnableLayer;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        if (interactor == null || prefabToSpawn == null)
        {
            Debug.LogError("Missing references!");
            return;
        }

        var hovered = interactor.GetOldestInteractableHovered();

        if (hovered == null)
        {
            Debug.Log("Nothing hovered");
            return;
        }

        GameObject obj = hovered.transform.gameObject;

     
        if (((1 << obj.layer) & spawnableLayer) == 0)
        {
            Debug.Log("Not spawnable layer");
            return;
        }

        Transform target = obj.transform;

        Vector3 spawnPos = target.position + Vector3.back * spawnOffset.z;

        Instantiate(prefabToSpawn, spawnPos, target.rotation);

        Debug.Log("Spawned on: " + target.name);
    }
}
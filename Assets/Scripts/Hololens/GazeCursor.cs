using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeCursor : MonoBehaviour {

    public bool hide = false;

    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
    }

    private void LateUpdate()
    {
        transform.position = GazeManager.instances.main.location;
        transform.rotation = Quaternion.FromToRotation(Vector3.up, GazeManager.instances.main.normal);

        meshRenderer.enabled = !hide || GazeManager.instances.main.gazeOnObject;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrateSpace : MonoBehaviour
{
    public Transform cameraOffset;
    public Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetCalibration()
    {
        Vector3 newOffset = camera.position * -1;
        float yOffset = cameraOffset.position.y;
        cameraOffset.position = new Vector3(newOffset.x, yOffset, newOffset.z);
    }
}

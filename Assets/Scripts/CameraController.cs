using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private bool isRotatingPreFrame = false;
    Vector3 initRotation;

    public bool IsRotating { get; set; } = false; // 自動プロパティ.

    private void Awake()
    {
        initRotation = transform.localRotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsRotating)
        {
            if (!isRotatingPreFrame)
            {
                initRotation = transform.localRotation.eulerAngles;
                isRotatingPreFrame = true;
            }
            Vector3 _parent = transform.parent.transform.localRotation.eulerAngles;
            transform.localRotation = Quaternion.Euler(initRotation - _parent);
        }
        else
        {
            if (isRotatingPreFrame)
            {
                transform.localRotation = Quaternion.Euler(initRotation);
                isRotatingPreFrame = false;
            }
        }
    }
}

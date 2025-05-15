using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotationFixer : MonoBehaviour
{
    private void Start()
    {
        gameObject.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position) /** Quaternion.Euler(0, 90, 0)*/;
        gameObject.transform.rotation = Quaternion.Euler(0, gameObject.transform.rotation.eulerAngles.y, 0);
    }
}

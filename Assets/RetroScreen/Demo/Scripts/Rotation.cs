using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotates object around Y axis with speed <see cref="rotSpeed"/>.
/// </summary>
public class Rotation : MonoBehaviour
{

    [SerializeField]
    private float rotSpeed = 45;

    private void Update()
    {
        transform.Rotate(Vector3.up * rotSpeed * Time.deltaTime, Space.World);
    }

}

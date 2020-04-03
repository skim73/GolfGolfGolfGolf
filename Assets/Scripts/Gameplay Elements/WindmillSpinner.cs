using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillSpinner : MonoBehaviour
{
    public float speed = 75;
    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }
}

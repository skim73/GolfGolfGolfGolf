using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rolly : MonoBehaviour
{
    public bool roll = false;
    public int speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (roll)
        {
            this.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + (speed * Time.deltaTime));
           
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject LINKED_TELEPORTER;
    public float POP_FORCE = 1.0f;
    public float COOL_DOWN_TIMER = 2.0f;

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("collided with:" + collision.gameObject.name + collision.gameObject.tag);
        if (collision.gameObject.tag == "ball")
        {
            if( collision.gameObject.GetComponent<BallScript>().coolDownTimer <= 0)
            {
                collision.gameObject.transform.position += LINKED_TELEPORTER.transform.position - this.transform.position;
                collision.gameObject.GetComponent<Rigidbody>().AddForce(0, POP_FORCE, 0);
                collision.gameObject.GetComponent<BallScript>().coolDownTimer = COOL_DOWN_TIMER;
            }
        }
    }
}

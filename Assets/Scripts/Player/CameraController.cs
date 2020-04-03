using System;
using Mirror;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Vector3 DEFAULT_POS = new Vector3( 21, 12, 6 );
    public Quaternion DEFAULT_ROT = new Quaternion( 0.1788776f, -0.8586125f, 0, 0.4804033f);
    public Vector3 last_position = Vector3.zero;
    public Vector3 curr_pos;

    /*
    // Start is called before the first frame update
    private void Awake()
    {
        _PlayerUpdated(ClientScene.localPlayer);

        localPlayerAnnouncer.OnLocalPlayerUpdated += _PlayerUpdated;

    }

    private void OnDestroy()
    {
        localPlayerAnnouncer.OnLocalPlayerUpdated -= _PlayerUpdated;
    }
    */
    // Update is called once per frame
    void FixedUpdate()
    {
        if (ClientScene.localPlayer)
        {
            curr_pos = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.position;
            if ( curr_pos != last_position) {
                this.gameObject.transform.position = curr_pos;
                this.gameObject.transform.rotation = ClientScene.localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.rotation;
                last_position = curr_pos;
            }
        }
    }
    /*
    private void _PlayerUpdated(NetworkIdentity localPlayer)
    {
        Debug.Log("NETWORKD HERE");
        if ( localPlayer != null)
        {
            this.gameObject.transform.position = localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.position;
            this.gameObject.transform.rotation = localPlayer.gameObject.GetComponent<PlayerScript>().CAMERA_OBJ.transform.rotation;
        }
        else
        {
            this.gameObject.transform.position = DEFAULT_POS;
            this.gameObject.transform.rotation = DEFAULT_ROT;
        }
    }
    */
}

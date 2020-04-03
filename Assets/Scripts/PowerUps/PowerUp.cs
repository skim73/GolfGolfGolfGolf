using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PowerUp
{
    string name { get; set; }
    void onUse(GameObject ball);
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdminTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Delivered meal");
        }
    }
}

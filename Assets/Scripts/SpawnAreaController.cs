using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAreaController : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject player1SpawnPlane;
    public GameObject player2SpawnPlane;
    void Start()
    {
        if (GameManager.instance.DEBUG)
        {
            player1SpawnPlane.GetComponent<MeshRenderer>().enabled = true;
            player2SpawnPlane.GetComponent<MeshRenderer>().enabled = true;
        }
        else
        {
            player1SpawnPlane.GetComponent<MeshRenderer>().enabled = false;
            player2SpawnPlane.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

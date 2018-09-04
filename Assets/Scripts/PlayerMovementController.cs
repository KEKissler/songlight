using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour {
    public float speed = 1.0f;
    public KeyCode pickUpEmitterKey;

    private List<GameObject> nearbyObjects = new List<GameObject>();
    private Rigidbody rb;
    private GameObject heldEmitter = null;

	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	void Update () {
        rb.velocity = speed * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if (Input.GetKeyDown(pickUpEmitterKey))
        {
            if(heldEmitter)
            {
                heldEmitter.GetComponent<EmitterController>().PutDown();
                heldEmitter = null;
            }
            else
            {
                heldEmitter = pickUpNearestEmitter(nearbyObjects);
                if(heldEmitter)
                heldEmitter.GetComponent<EmitterController>().PickUp(this.transform);
            }
            
        }
	}

    public void OnTriggerEnter(Collider other)
    {
        nearbyObjects.Add(other.gameObject);
    }

    public void OnTriggerExit(Collider other)
    {
        nearbyObjects.Remove(other.gameObject);
    }

    private GameObject pickUpNearestEmitter(List<GameObject> emitters)
    {
        GameObject toReturn = null;
        float minDistance = float.MaxValue;
        foreach(GameObject g in emitters)
        {
            if(g && g.GetComponent<EmitterController>() && Vector3.Distance(transform.position, g.transform.position) < minDistance)
            {
                toReturn = g;
                minDistance = Vector3.Distance(transform.position, g.transform.position);
            }
        }
        return toReturn;
    }
}

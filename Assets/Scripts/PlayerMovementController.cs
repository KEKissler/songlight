using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UIController))]
public class PlayerMovementController : MonoBehaviour {
    public float speed = 1.0f;
    public KeyCode pickUpEmitterKey;
    [HideInInspector] public bool isRespawning = false;
    [HideInInspector] public Vector3 respawnPoint;

    private List<GameObject> nearbyObjects = new List<GameObject>();
    private Rigidbody rb;
    private GameObject heldEmitter = null;
    private PedestalController nearbyPedestal;
    private UIController UI;

	void Start () {
        rb = GetComponent<Rigidbody>();
        UI = GetComponent<UIController>();
        respawnPoint = transform.position;
	}

    void Update()
    {
        if (!isRespawning)
        {
            rb.velocity = speed * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            if (Input.GetKeyDown(pickUpEmitterKey))
            {
                if (heldEmitter)
                {
                    heldEmitter.GetComponent<EmitterController>().PutDown();
                    if (nearbyPedestal)
                    {
                        nearbyPedestal.AddTrack(heldEmitter);
                       nearbyObjects.Remove(heldEmitter);//no longer interactable as it is part of the pedestal now
                    }
                    heldEmitter = null;

                }
                else
                {
                    heldEmitter = pickUpNearestEmitter(nearbyObjects);
                    if (heldEmitter)
                    {
                        heldEmitter.GetComponent<EmitterController>().PickUp(this.transform);
                    }
                    else
                    {
                        //picked up nothing, play error sound
                        UI.PlayClip("meepmorp");
                    }
                }

            }
        }
	}

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EmitterController>() && other.GetComponent<EmitterController>().canBePickedUp)
        {
            nearbyObjects.Add(other.gameObject);
            UI.PlayClip("wilhelm");
        }
        else if (other.GetComponent<PedestalController>())
        {
            nearbyPedestal = other.GetComponent<PedestalController>();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<EmitterController>() && other.GetComponent<EmitterController>().canBePickedUp)
        {
            nearbyObjects.Remove(other.gameObject);
        }
        else if (other.GetComponent<PedestalController>())
        {
            nearbyPedestal = null;
        }
    }

    private GameObject pickUpNearestEmitter(List<GameObject> emitters)
    {
        GameObject toReturn = null;
        float minDistance = float.MaxValue;
        foreach(GameObject g in emitters)
        {
            if(Vector3.Distance(transform.position, g.transform.position) < minDistance)
            {
                toReturn = g;
                minDistance = Vector3.Distance(transform.position, g.transform.position);
            }
        }
        return toReturn;
    }
}

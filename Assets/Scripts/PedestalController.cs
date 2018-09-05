using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(FogController))]
public class PedestalController : MonoBehaviour {
    private FogController fc;
    private List<GameObject> acceptedEmitters = new List<GameObject>();
    private int numTracksAdded = 0;

    public GameObject emitterParent;

    void Start () {
        fc = GetComponent<FogController>();
        foreach(Transform t in emitterParent.transform)
        {
            acceptedEmitters.Add(t.gameObject);
        }
	}

    public void AddTrack(GameObject unverifiedEmitter)
    {
        if (acceptedEmitters.Contains(unverifiedEmitter))
        {
            unverifiedEmitter.GetComponent<EmitterController>().canBePickedUp = false;
            unverifiedEmitter.transform.parent = transform;
            unverifiedEmitter.transform.position = transform.position;
            fc.UpdateFogRange(numTracksAdded++);
        }
    }
}

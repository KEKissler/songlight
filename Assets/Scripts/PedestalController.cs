using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PedestalController : MonoBehaviour {
    private List<GameObject> acceptedEmitters = new List<GameObject>();
    private int numTracksAdded = 0;

    [HideInInspector] public float fogRange;
    public float initialFogRange;
    public int[] extendedFogRanges;
    public GameObject emitterParent;
    public PlayerMovementController pmc;

    void Start () {
        if (emitterParent)
        { 
            foreach (Transform t in emitterParent.transform)
            {
                acceptedEmitters.Add(t.gameObject);
            }
        }
        fogRange = initialFogRange;
    }

    private void UpdateFogRange(int index)
    {
        fogRange = extendedFogRanges[index];
    }

    public void AddTrack(GameObject unverifiedEmitter)
    {
        if (acceptedEmitters.Contains(unverifiedEmitter))
        {
            unverifiedEmitter.GetComponent<EmitterController>().canBePickedUp = false;
            //unverifiedEmitter.GetComponent<AudioSource>().spatialBlend = 0;
            unverifiedEmitter.GetComponent<AudioSource>().volume = GetComponent<AudioSource>().volume;
            unverifiedEmitter.transform.parent = transform;
            unverifiedEmitter.transform.position = transform.position;
            UpdateFogRange(numTracksAdded++);
        }
        if(numTracksAdded == acceptedEmitters.Count)
        {
            pmc.respawnPoint = new Vector3(transform.position.x, pmc.respawnPoint.y, transform.position.z);
        }
    }
}

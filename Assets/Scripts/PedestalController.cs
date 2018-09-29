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

    public bool Contains(GameObject unverifiedEmitter)
    {
        return acceptedEmitters.Contains(unverifiedEmitter);
    }

    //USER is exepcted to check if valid to add track here first using the contains function above
    public void AddTrack(GameObject emitter)
    {
        emitter.tag = "Quietable";
        emitter.GetComponent<EmitterController>().canBePickedUp = false;
        //emitter.GetComponent<AudioSource>().spatialBlend = 0;
        emitter.GetComponent<AudioSource>().volume = GetComponent<AudioSource>().volume;
        emitter.transform.parent = transform;
        emitter.transform.position = transform.position;
        UpdateFogRange(numTracksAdded++);

        if(numTracksAdded == acceptedEmitters.Count)
        {
            pmc.respawnPoint = new Vector3(transform.position.x, pmc.respawnPoint.y, transform.position.z);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class EmitterController : MonoBehaviour {
    public float heldVolumePercentage;
    public Transform emitterParent;

    private AudioSource aud;

	void Start () {
        aud = GetComponent<AudioSource>();
        heldVolumePercentage = Mathf.Clamp(heldVolumePercentage, 0, 1);
	}

    public void PickUp(Transform source)
    {
        transform.parent = source;
        transform.position = source.transform.position;
        aud.volume *= heldVolumePercentage;
    }

    public void PutDown()
    {
        transform.parent = emitterParent;
        aud.volume /= heldVolumePercentage;
    }
}

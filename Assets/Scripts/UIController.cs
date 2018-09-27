using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(PlayerMovementController))]
public class UIController : MonoBehaviour {
    public AudClip[] clips;
    private AudioSource audSource;

	// Use this for initialization
	void Start () {
        audSource = GetComponent<AudioSource>();
	}
    
    //search for and play the first clip with the given title
    public void PlayClip(string clipName)
    {
        foreach(AudClip ac in clips)
        {
            if(ac.title == clipName)
            {
                audSource.PlayOneShot(ac.track);
                return;
            }
        }
    }
}

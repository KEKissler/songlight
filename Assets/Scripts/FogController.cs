using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PedestalController))]
public class FogController : MonoBehaviour {
    private Component[] cmpnts;
    private AudioSource[] aud;
    private PlayerMovementController pmc;
    private bool fadedOut, fadedIn, hasSetPos = false;

    public float initialFogRange;
    public int[] extendedFogRanges;
    public float fadeOutVolumePerSecond;
    public float fadeInVolumePerSecond;
    public Transform player;

	void Start () {
        fadeInVolumePerSecond /= 100;
        fadeOutVolumePerSecond /= 100;
        pmc = player.GetComponent<PlayerMovementController>();
        cmpnts = GetComponents(typeof(AudioSource));
        aud = new AudioSource[cmpnts.Length];
        for(int i = 0; i < cmpnts.Length; ++i)
        {
            aud[i] = (AudioSource)cmpnts[i];
        }
        aud[0].maxDistance = initialFogRange;
        
	}

	public void UpdateFogRange(int index)
    {
        aud[0].maxDistance = extendedFogRanges[index];
    }

    void Update()
    {
        if (pmc.isRespawning)
        {
            if (!fadedOut)
            {
                FadeOut();
            }
            if (fadedOut)
            {
                if (!hasSetPos)
                {
                    player.position = transform.position + new Vector3(0, 1, 0);
                    hasSetPos = true;
                    StartCoroutine("FadeIn");
                }
                if (!fadedIn)
                {
                    FadeIn();
                }
                if (fadedIn)
                {
                    pmc.isRespawning = false;
                }

            }
        }
        else if (Vector3.Distance(player.position, transform.position + new Vector3(0, 1, 0)) > aud[0].maxDistance)
        {
            respawnPlayer();
            fadedOut = false;
            fadedIn = false;
            hasSetPos = false;
        }
    }

    private void respawnPlayer()
    {
        player.GetComponent<Rigidbody>().velocity = new Vector3();
        pmc.isRespawning = true;
        StartCoroutine("FadeOut");
    }
    private IEnumerator FadeOut()
    {
        while(AudioListener.volume > 0)
        {
            yield return null;
            AudioListener.volume -= fadeOutVolumePerSecond * Time.deltaTime;
            
        }
        AudioListener.volume = 0;
        fadedOut = true;
        yield break;
    }

    private IEnumerator FadeIn()
    {
        while (AudioListener.volume < 1)
        {
            yield return null;
            AudioListener.volume += fadeInVolumePerSecond * Time.deltaTime;
        }
        fadedIn = true;
        AudioListener.volume = 1;
        pmc.isRespawning = false;
        yield break;
    }
    
}

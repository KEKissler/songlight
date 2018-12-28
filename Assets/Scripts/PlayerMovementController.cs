using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UIController))]
public class PlayerMovementController : MonoBehaviour {
    public float speed = 1.0f;
    public KeyCode pickUpEmitterKey, scanKey;
    public float scanRange;
    public float scanSilenceMultiplier;
    public float scanFadeTime;
    public LayerMask lm;
    [HideInInspector] public bool isRespawning = false;
    [HideInInspector] public Vector3 respawnPoint;

    private List<GameObject> nearbyObjects = new List<GameObject>();
    private Rigidbody rb;
    private GameObject heldEmitter = null;
    private PedestalController nearbyPedestal;
    [HideInInspector]
    public UIController UI;
    private List<AudioSource> sources;
    private List<AudioSource> quietedSources;
    private float savedSpeed;
    private bool fadingOut = false;
    private float timeSinceStartofFadeOut;



    void Start () {
        rb = GetComponent<Rigidbody>();
        UI = GetComponent<UIController>();
        respawnPoint = transform.position;
        sources = new List<AudioSource>();
        quietedSources = new List<AudioSource>();
        savedSpeed = speed;
        timeSinceStartofFadeOut = scanFadeTime + 1;//anything larger than scanFadeTime
    }

    void Update()
    {
        if (!isRespawning)
        {
            //actual player movement
            rb.velocity = speed * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            //managing fadingout and timesince
            if(fadingOut){
                if(timeSinceStartofFadeOut < scanFadeTime)
                {
                    timeSinceStartofFadeOut += Time.deltaTime;
                }
                else
                {
                    //will be guaranteed to finish by now
                    timeSinceStartofFadeOut = scanFadeTime + 1f;
                    fadingOut = false;
                }
            
               
            }
            if (Input.GetKeyDown(pickUpEmitterKey))
            {
                    //already holding an emitter
                if (heldEmitter)
                {
                    heldEmitter.GetComponent<EmitterController>().PutDown();
                    if (nearbyPedestal && nearbyPedestal.Contains(heldEmitter))
                    {
                        heldEmitter.GetComponent<AudioSource>().SetCustomCurve(AudioSourceCurveType.CustomRolloff, nearbyPedestal.GetComponent<AudioSource>().GetCustomCurve(AudioSourceCurveType.CustomRolloff));
                        nearbyPedestal.AddTrack(heldEmitter);
                        nearbyObjects.Remove(heldEmitter);//no longer interactable as it is part of the pedestal now
                        UI.PlayClip("DropOffSuccessful");
                    }
                    heldEmitter = null;

                }
                else
                {
                    //not already holding something, pick up nearest emitter
                    heldEmitter = pickUpNearestEmitter(nearbyObjects);
                    if (heldEmitter)
                    {
                        //picked up something!
                        heldEmitter.GetComponent<EmitterController>().PickUp(this.transform);
                        UI.PlayClip("PickupSuccessful");
                    }
                    else
                    {
                        //picked up nothing, play error sound
                        UI.PlayClip("meepmorp");
                    }
                }
            }
            if (Input.GetKeyDown(scanKey))
            {
                //player cannot move while scanning
                speed = 0;
                int i = 0;
                foreach (RaycastHit r in Physics.SphereCastAll(transform.position + Vector3.down, scanRange, new Vector3(0,0,1), 2f, lm, QueryTriggerInteraction.Collide))
                {
                    ++i;
                    AudioSource test = r.transform.GetComponent<AudioSource>();
                    if (test)
                    {
                        sources.Add(test);
                    }
                }
                changeSourceSounds();
            }
            if (Input.GetKeyUp(scanKey))
            {
                speed = savedSpeed;
                unchangeSourceSounds();
            }
        }
	}

    private void changeSourceSounds()
    {
        foreach(AudioSource aud in sources)
        {
            if(aud.tag == "Quietable")
            {
                Debug.Log("quieting \"" + aud.name + "\"");
                Debug.Log(aud.volume + "    " + aud.volume * scanSilenceMultiplier);
                fadingOut = true;
                timeSinceStartofFadeOut = 0f;
                //StartCoroutine(fadeToTargetVolume(aud, aud.volume * scanSilenceMultiplier, scanFadeTime));   
                aud.volume *= scanSilenceMultiplier;
                quietedSources.Add(aud);
            }
        }
    }

    //valid param checking beforehand plz
    public IEnumerator fadeToTargetVolume(AudioSource aud, float targetVolume, float seconds)
    {
        int i = 0;
        float step = Time.fixedDeltaTime * (targetVolume - aud.volume) / seconds;
        Debug.Log(step);
        //while applying the next step would bring the current volume closer to the target volume, keep going
        while (Mathf.Abs(aud.volume + step - targetVolume) < Mathf.Abs(aud.volume - targetVolume))
        {
            aud.volume += step;
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
        //at this point the difference between current volume and target volume is less than a step, so just make that little jump
        aud.volume = targetVolume;
        yield break;
    }

    private void unchangeSourceSounds()
    {
        if (fadingOut)
        {
            Invoke("unchangeSourceSounds", scanFadeTime - timeSinceStartofFadeOut);
        }
        foreach(AudioSource aud in quietedSources)
        {
            aud.volume /= scanSilenceMultiplier;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EmitterController>() && other.GetComponent<EmitterController>().canBePickedUp)
        {
            nearbyObjects.Add(other.gameObject);
            //UI.PlayClip("wilhelm");
        }
        else if (other.GetComponent<PedestalController>())
        {
            nearbyPedestal = other.GetComponent<PedestalController>();
            //checking for applying a ui sound
            if (heldEmitter && nearbyPedestal && nearbyPedestal.Contains(heldEmitter))
            {
                UI.PlayClip("EnterDropoffRange");
            }
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
            if(g.GetComponent<EmitterController>().canBePickedUp && Vector3.Distance(transform.position, g.transform.position) < minDistance)
            {
                toReturn = g;
                minDistance = Vector3.Distance(transform.position, g.transform.position);
            }
        }
        return toReturn;
    }
}

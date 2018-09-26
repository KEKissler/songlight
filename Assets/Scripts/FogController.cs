using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class FogController : MonoBehaviour {
    private PlayerMovementController pmc;

    private bool fadedOut, fadedIn, hasSetPos = false;
    private List<Transform> pedestals = new List<Transform>();
    private const float distanceLeniency = 0.01f;

    
    public float fadeOutVolumePerSecond;
    public float fadeInVolumePerSecond;
    public Transform player;
    public GameObject pedestalParent;

    void Start () {
        fadeInVolumePerSecond /= 100;
        fadeOutVolumePerSecond /= 100;
        pmc = player.GetComponent<PlayerMovementController>();

        foreach (Transform t in pedestalParent.transform)
        {
            pedestals.Add(t);
        }

    }

	

    void Update()
    {
        //respawning hijacks this function, so the normal execution of update is below
        if (pmc.isRespawning)
        {
            handleRespawnCoroutines();
        }
        else
        {
            transform.position = nearestFogPoint();
        }
        //if (Vector3.Distance(player.position, transform.position + new Vector3(0, 1, 0)) > aud.maxDistance)
        //{
        //    respawnPlayer();
        //}
    }
    private Vector3 nearestFogPoint()
    {
        Vector3 toReturn = new Vector3();
        List<Transform> pedInRange = pedestalsInRange(new Vector2(player.position.x, player.position.z));
        if(pedInRange.Count == 1)
        {
            Vector2 normalPart = new Vector2((player.position - pedInRange[0].position).x, (player.position - pedInRange[0].position).z).normalized;
            if (normalPart.magnitude != 0)
            {
                toReturn = pedInRange[0].position + pedInRange[0].GetComponent<PedestalController>().fogRange * new Vector3(normalPart.x, 0, normalPart.y);
                toReturn.y = transform.position.y;
            }
            else
            {
                toReturn = pedInRange[0].position + pedInRange[0].GetComponent<PedestalController>().fogRange * Vector3.right;
                toReturn.y = transform.position.y;
            }
            //this portion is just to test that the point just selected is even valid, and not within another pedestal's range
            List<Transform> test = pedestalsInRange(new Vector2(toReturn.x, toReturn.z));
            if(test.Count > 1)
            {
                //if it IS within another pedestal's range, change the list of nearby pedestals to be relative to the point selected, not the player.
                //this way the resolution will be able to see the multiple circles that are needed to solve this type of problem
                pedInRange = test;
            }
        }
        if (pedInRange.Count == 2)
        {
            //untested
            Vector2[] toTest = getCircleIntersectionPoints(
                new Vector2(pedInRange[0].position.x, pedInRange[0].position.z),
                new Vector2(pedInRange[1].position.x, pedInRange[1].position.z),
                pedInRange[0].GetComponent<PedestalController>().fogRange,
                pedInRange[1].GetComponent<PedestalController>().fogRange);
            toReturn = new Vector3(toTest[0].x, player.position.y, toTest[0].y);
            Vector3 alternativeOption = new Vector3(toTest[1].x, player.position.y, toTest[1].y);
            //this first if check it to avoid little differences making a large impact on the second, meaningful check
            if(Mathf.Approximately(Vector3.Distance(player.position, alternativeOption), Vector3.Distance(player.position, toReturn)))
            {
                return toReturn;
            }
            if(Vector3.Distance(player.position, alternativeOption) < Vector3.Distance(player.position, toReturn))
            {
                toReturn = alternativeOption;
            }
            toReturn.y = transform.position.y;
        }
        else if(pedInRange.Count > 2)
        {
            Debug.Log("lmaonerd");
        }
      
        return toReturn;
    }
    private List<Transform> pedestalsInRange(Vector2 positionToCheckFrom)
    {
        List<Transform> toReturn = new List<Transform>();
        foreach(Transform t in pedestals)
        {
            if(Vector2.Distance(positionToCheckFrom, new Vector2(t.position.x, t.position.z)) - t.GetComponent<PedestalController>().fogRange <= distanceLeniency)
            {
                toReturn.Add(t);
            }
        }
        return toReturn;
    }

    private Vector2[] getCircleIntersectionPoints(Vector2 c1, Vector2 c2, float r1, float r2)
    {
        float dx = c2.x - c1.x, dy = c2.y - c1.y, d = Mathf.Sqrt(dx * dx + dy * dy);
        float chorddistance = (r1 * r1 - r2 * r2 + d * d) / (2 * d);
        float halfchordlength = Mathf.Sqrt(r1 * r1 - chorddistance * chorddistance);
        Vector2 mdpt = new Vector2(c1.x + chorddistance * dx / d, c1.y + chorddistance * dy / d);
        return new Vector2[] {
            new Vector2(mdpt.x + halfchordlength * dy / d, mdpt.y - halfchordlength * dx / d),
            new Vector2(mdpt.x - halfchordlength * dy / d, mdpt.y + halfchordlength * dx / d)};
    }
    private void handleRespawnCoroutines()
    {
        if (!fadedOut)
        {
            FadeOut();
        }
        if (fadedOut)
        {
            if (!hasSetPos)
            {
                player.position = pmc.respawnPoint;
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

    private void respawnPlayer()
    {
        fadedOut = false;
        fadedIn = false;
        hasSetPos = false;
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

    public void OnTriggerEnter(Collider other)
    {
        if(other.transform == player)
        {
            respawnPlayer();
        }
    }
}

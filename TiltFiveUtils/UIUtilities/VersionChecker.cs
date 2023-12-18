using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionChecker : MonoBehaviour
{
    bool isVersionOutOfDate = true;
    public GameObject versionOutOfDateScreen;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isVersionOutOfDate)
        {
            if (TiltFive.TiltFiveManager2.Instance.NeedsDriverUpdate())
            {
                versionOutOfDateScreen.SetActive(true);
                Debug.Log("Version out of date");
            }
            else
            {
                versionOutOfDateScreen.SetActive(false);
                isVersionOutOfDate = false;
            }
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections;
using Random = UnityEngine.Random;

public class NetworkManager : MonoBehaviour
{
    public GameObject canvas;

    private void Start()
    {
        StartCoroutine(CoCheck());
    }

    IEnumerator CoCheck()
    {
        WaitForSeconds wait = new WaitForSeconds(3);
        while (true)
        {
            CheckInternetConnection();
            yield return wait;

        }
    }

    public static bool IsInternetConnected()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return false;
        }
        return true;
    }
    public void CheckInternetConnection()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                //Debug.Log("No internet connection available.");
                HandleNoConnection();
                break;

            case NetworkReachability.ReachableViaCarrierDataNetwork:
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                //Debug.Log("Connected via mobile data.");
                HandleConnected();
                break;

        }
    }

    private void HandleNoConnection()
    {
        canvas.SetActive(true);
        // Handle actions when there's no connection
        Debug.LogWarning("Internet is not accessible. Please check your network settings.");
    }

    private void HandleConnected()
    {
        if (canvas.activeSelf)
        {
            canvas.SetActive(false);
        }
        // Handle actions when connected via mobile data
        //Debug.Log("Using mobile data for internet access.");
    }


}

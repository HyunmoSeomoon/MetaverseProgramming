using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using System;
using System.Runtime.CompilerServices;

public class GPSmanager : MonoBehaviour
{
    public Text text_ui;
    public GameObject startGame_popUp;
    public GameObject endGame_popUp;

    public GameObject insectEgg;
    public bool isFirst = false;

    public double[] lats;
    public double[] longs;

    IEnumerator Start()
    {
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            yield return null;
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
            Debug.Log("Location not enabled on device or app does not have permission to access location");

        // Starts the location service.
        Input.location.Start(10, 1);

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
            
            while( true)
            {
                yield return null;
                text_ui.text = "위도: " + Input.location.lastData.latitude + " / 경도: " + Input.location.lastData.longitude;
            }
        }

        // Stops the location service if there is no need to query location updates continuously.( disabled)
        //Input.location.Stop();
    }

    void Upate(){
        if (Input.location.status == LocationServiceStatus.Running)
        {
            double myLat = Input.location.lastData.latitude;
            double myLong = Input.location.lastData.longitude;

            double remainDistance1 = distance(myLat, myLong, lats[0], longs[0]);
            double remainDistance2 = distance(myLat, myLong, lats[1], longs[1]);

            if (!isFirst && remainDistance1 <= 10f)
            {
                isFirst = true;
                startGame_popUp.SetActive(true);
                CreateObjectAtCameraView();
            }
            if (!isFirst && remainDistance2 <= 10f)
            {
                isFirst = true;
                endGame_popUp.SetActive(true);
            }
        }
    }

    private void CreateObjectAtCameraView()
    {
        if (insectEgg != null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.forward * 5f; // camera - foward 5m
                Instantiate(insectEgg, spawnPosition, Quaternion.identity);
                Debug.Log("Object created in front of the camera.");
            }
            else
            {
                Debug.LogError("Main Camera not found!");
            }
        }
        else
        {
            Debug.LogError("Object Prefab is not assigned.");
        }
    }
    
    private double distance(double lat1, double lon1, double lat2, double lon2)
    {
        double theta = lon1 - lon2;

        double dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));

        dist = Math.Acos(dist);

        dist = Rad2Deg(dist);

        dist = dist * 60 * 1.1515;

        dist = dist * 1609.344; // 미터 변환

        return dist;
    }

    private double Deg2Rad(double deg)
    {
        return (deg * Mathf.PI / 180.0f);
    }

    private double Rad2Deg(double rad)
    {
        return (rad * 180.0f / Mathf.PI);
    }
}


﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private static CameraManager _instance;

    public static CameraManager Instance
    {
        get
        {
            if (_instance == null)
            {
                if (Camera.main)
                {
                    _instance = Camera.main.gameObject.GetComponent<CameraManager>();
                    if (_instance == null)
                    {
                        _instance = Camera.main.gameObject.AddComponent<CameraManager>();
                    }
                    

                }
                else
                {
                    GameObject go = new GameObject("Main Camera");
                    go.tag = "MainCamera";
                    Camera camera = go.AddComponent<Camera>();
                    camera.clearFlags = CameraClearFlags.Skybox;
                    camera.fieldOfView = 60;
                   
                    _instance = go.AddComponent<CameraManager>();

                }
            }

            return _instance;
        }
    }

    public Camera mainCamera { get; private set; }

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
    }

    void Update()
    {
        Drag();
        Scroll();
    }

    Vector3 oldMousePosition;
    Plane mPlane = new Plane(Vector3.up, Vector3.zero);
    void Drag()
    {
        if (Input.GetMouseButton(1) && mainCamera)
        {
            float xDelta = Input.GetAxis("Mouse X");
            float yDelta = Input.GetAxis("Mouse Y");
            if (xDelta != 0.0f || yDelta != 0.0f)
            {
                if (oldMousePosition != Vector3.zero)
                {
                    Ray rayDest = mainCamera.ScreenPointToRay(Input.mousePosition);

                    float distance = 0;
                    mPlane.Raycast(rayDest, out distance);

                    Vector3 dest = rayDest.GetPoint(distance);
                    distance = 0;
                    Ray rayOld = mainCamera.ScreenPointToRay(oldMousePosition);
                    mPlane.Raycast(rayOld, out distance);


                    Vector3 pos = mainCamera.transform.localPosition + rayOld.GetPoint(distance) - dest;

                    mainCamera.transform.localPosition = pos;
                }

                oldMousePosition = Input.mousePosition;
            }

        }
        if (Input.GetMouseButtonUp(1))
        {
            oldMousePosition = Vector3.zero;
        }

    }
    //缩放距离限制   

    public float minDistance = 10;
    public float maxDistance = 200;
    public float scrollSpeed = 20;

    public float distance;

    
    void Scroll()
    {
        if (mainCamera == null)
        {
            return;
        }
        // 鼠标滚轮触发缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if ( (scroll < -0.001 || scroll > 0.001) ||(distance < minDistance || distance > maxDistance) )
        {
            float displacement = scrollSpeed * scroll;

            mainCamera.transform.position += mainCamera.transform.forward * displacement;

            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
  
            mPlane.Raycast(ray, out distance);

            if (distance < minDistance)
            {
                mainCamera.transform.position = ray.GetPoint(distance - minDistance);
            }
            else if (distance > maxDistance)
            {
                mainCamera.transform.position = ray.GetPoint(distance - maxDistance);
            }
        }

    }

    public Vector3 GetWorldMousePosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        float distance;
        mPlane.Raycast(ray, out distance);

        return ray.GetPoint(distance);
    }
}

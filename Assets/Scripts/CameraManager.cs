using System;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class CameraManager : MonoBehaviour
{
    private static CameraManager mInstance;

    public static CameraManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                if (Camera.main)
                {
                    mInstance = Camera.main.gameObject.GetComponent<CameraManager>();
                    if (mInstance == null)
                    {
                        mInstance = Camera.main.gameObject.AddComponent<CameraManager>();
                    }                 
                }
                else
                {
                    GameObject go = new GameObject("Main Camera");
                    go.tag = "MainCamera";
                    Camera camera = go.AddComponent<Camera>();
                    camera.clearFlags = CameraClearFlags.Skybox;
                    camera.fieldOfView = 60;
                   
                    mInstance = go.AddComponent<CameraManager>();

                }
            }

            return mInstance;
        }
    }

    public Camera mainCamera { get; private set; }

    public event Action<float> onZoom;
    public event Action onMove;

    public Vector3 center
    {
        get
        {
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            float distance;
            mPlane.Raycast(ray, out distance);
            return ray.GetPoint(distance);
        }
    }

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        UpdateCorner();
    }

    void Update()
    {
        Move();
        Zoom();
    }

    private Vector3 oldMousePosition;
    private Plane mPlane = new Plane(Vector3.up, Vector3.zero);
    void Move()
    {
        if (Input.GetMouseButton(0) && mainCamera)
        {
            if (Input.mousePosition - oldMousePosition != Vector3.zero)
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

                UpdateCorner();

                if (onMove != null)
                {
                    onMove();
                }
            }

        }
        if (Input.GetMouseButtonUp(0))
        {
            oldMousePosition = Vector3.zero;
        }

    }
    //缩放距离限制   
    public LODSettings lodSettings;
 
    public float scrollSpeed = 20;

    public float distance;

    
    void Zoom()
    {
        if (mainCamera == null)
        {
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        if (mousePosition.x < 0 
            || mousePosition.x > Screen.width 
            || mousePosition.y < 0 
            || mousePosition.y > Screen.height)
        {
            return;
        }
        // 鼠标滚轮触发缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if ( (scroll < -0.001 || scroll > 0.001) ||(distance < lodSettings. minDistance || distance > lodSettings.maxDistance) )
        {
            float displacement = scrollSpeed * scroll;

            mainCamera.transform.position += mainCamera.transform.forward * displacement;

            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
  
            mPlane.Raycast(ray, out distance);

            if (distance < lodSettings.minDistance)
            {
                mainCamera.transform.position = ray.GetPoint(distance - lodSettings.minDistance);
            }
            else if (distance > lodSettings.maxDistance)
            {
                mainCamera.transform.position = ray.GetPoint(distance - lodSettings.maxDistance);
            }

            UpdateCorner();

            if (onZoom != null)
            {
                onZoom(distance);
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

   

    #region DrawView
    public bool rendering = true;
    public Color color = Color.red;


    public Material material;


    public Vector3 leftBottom { get; private set; }
    public Vector3 leftTop { get; private set; }
    public Vector3 rightBottom { get; private set; }
    public Vector3 rightTop { get; private set; }

    private void UpdateCorner()
    {
        if (rendering)
        {
            leftBottom = RayCast(Vector3.zero);
            leftTop = RayCast(new Vector3(0, Screen.height, 0));
            rightTop = RayCast(new Vector3(Screen.width, Screen.height, 0));
            rightBottom = RayCast(new Vector3(Screen.width, 0, 0));
        }
    }
    private void OnRenderObject()
    {
        if (material && rendering)
        {
            material.SetPass(0);
            GL.Begin(GL.LINE_STRIP);
            GL.Color(color);
            GL.Vertex(leftBottom);
            GL.Vertex(leftTop);
            GL.Vertex(rightTop);
            GL.Vertex(rightBottom);
            GL.Vertex(leftBottom);
            GL.End();
        }
    }

    Vector3 RayCast(Vector3 screenPosition)
    {
        if (mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            float distance;
            mPlane.Raycast(ray, out distance);
            Vector3 point = ray.GetPoint(distance);
            point.y += 0.1f;
            return point;
        }
        return Vector3.zero;
    }


    #endregion
}


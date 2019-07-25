//////////////////////////////////////////////////////////////////////////
// 
// 文件：Assets\Scripts\Component\FPS.cs
// 作者：Xiexx
// 时间：2019/01/23
// 描述：FPS脚本
// 说明：
//
//////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class FPS : MonoBehaviour
{
    private float m_fFPS;

    public GameObject CameraObj;
    //private Camera m_Camera;

    void Awake()
    {
        
    }

    void Start()
    {
        // 必要参数设置
        Application.targetFrameRate = 60;                  // 最大帧率
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        FPSInit();

        DebugInit();
      
    }


    void Update()
    {
        try
        {
            FPSUpdate();
        }
        catch
        {
            
        }
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////
    // FPS
    private float m_fFPSCheckTime;
    private int m_nFPSFrames;

    private void FPSInit()
    {
        m_fFPSCheckTime = 0.0f;
        m_nFPSFrames = 0;
    }

    private void FPSUpdate()
    {
        ++m_nFPSFrames;
        float fTimeNow = Time.realtimeSinceStartup;
        if (fTimeNow > m_fFPSCheckTime + 0.5f)      // FPS 每0.5秒检测一次
        {
            m_fFPS = m_nFPSFrames / (fTimeNow - m_fFPSCheckTime);

            m_fFPSCheckTime = fTimeNow;
            m_nFPSFrames = 0;
        }
    }

    private readonly Rect m_FPSRect = new Rect(100.0f, 2.0f, 500.0f, 300.0f);
    private GUIStyle m_FPSStyle;

    private void DebugInit()
    {
        m_FPSStyle = new GUIStyle();
        m_FPSStyle.fontSize = 20;
        m_FPSStyle.normal.textColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
        m_FPSStyle.fontStyle = FontStyle.Bold;
    }

    void OnGUI()
    {
        GUI.Label(m_FPSRect, string.Format("FPS:{0:F1}", m_fFPS), m_FPSStyle);
    }


    private void OnZoom(float fDeltaDis)
    {
        Debug.Log("OnZoom fDeltaDis=" + fDeltaDis);
        CameraObj.transform.position += CameraObj.transform.forward * fDeltaDis * 0.1f;
    }

    private Vector3 m_StartPos;
    private Vector3 m_DragPos;

    private void OnDragStart(Vector3 pos)
    {
        m_StartPos = pos;

        Debug.Log("OnDragStart=" + m_StartPos);

        //Ray ray = m_Camera.ScreenPointToRay(pos);

        //RaycastHit info;

        //if (Physics.Raycast(ray, out info, 3000, 1 << LayerMask.NameToLayer("Terrain")))
        //{
         //   m_DragPos = info.point;
        //}
    }
    
    private void OnDragEnd(Vector3 pos)
    {


    }

    private void OnDrag(Vector3 pos)
    {
        Debug.Log("OnDragStart=" + pos);

        Vector3 start = new Vector3(m_StartPos.x, 0, m_StartPos.y);
        Vector3 end = new Vector3(pos.x, 0, pos.y);

        Vector3 dir = (start - end).normalized;

        Debug.Log("Dir=" + dir);

        float fSpeed = (CameraObj.transform.position.y / 95.0f) * 0.1f;

        float dis = Vector3.Distance(start, end) * fSpeed;

        m_StartPos = pos;

        CameraObj.transform.position += dir * dis;
    }


}

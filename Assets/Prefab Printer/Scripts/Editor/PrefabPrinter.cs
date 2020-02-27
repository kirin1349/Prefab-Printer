using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabPrinter
{
    private Camera m_camera = null;
    private Vector2Int m_canvasSize = Vector2Int.zero;
    private Vector2Int m_printSize = Vector2Int.zero;
    private int m_frameTotal = 0;
    private float m_duration = 0;
    private string m_outputFolder = string.Empty;
    private string m_outputNameFormat = string.Empty;
    private PrefabPrinterTextureTypes m_outputTextureType = PrefabPrinterTextureTypes.PNG;
    private TextureFormat m_outputTextureFormat = TextureFormat.ARGB32;
    private bool m_outputCreateFolder = false;

    private RenderTexture m_canvas = null;

    private List<GameObject> m_objects = new List<GameObject>();

    private bool m_running = false;


    private GameObject m_currentObject = null;
    private string m_currentObjectName = string.Empty;
    private string m_currentOutputPath = string.Empty;
    private float m_currentFrame = 0;
    private float m_currentTimePass = 0;
    private float m_currentIntervalCount = 0;
    private float m_currentIntervalTotal = 0;
    private float m_currentDuration = 0;

    public void setCamera(Camera value)
    {
        m_camera = value;
    }

    public void setCanvasSize(Vector2Int value)
    {
        m_canvasSize = value;
    }

    public void setPrintSize(Vector2Int value)
    {
        m_printSize = value;
    }

    public void setFrameTotal(int value)
    {
        m_frameTotal = value;
    }

    public void setDuration(float value)
    {
        m_duration = value;
    }

    public void setOutputFolder(string value)
    {
        m_outputFolder = value;
    }

    public void setOutputNameFormat(string value)
    {
        m_outputNameFormat = value;
    }

    public void setOutputTextureType(PrefabPrinterTextureTypes value)
    {
        m_outputTextureType = value;
    }

    public void setOutputTextureFormat(TextureFormat value)
    {
        m_outputTextureFormat = value;
    }

    public void setOutputCreateFolder(bool value)
    {
        m_outputCreateFolder = value;
    }

    public void addObject(GameObject value)
    {
        if (!m_objects.Contains(value))
        {
            m_objects.Add(value);
        }
    }

    public void clearObjects()
    {
        m_objects.Clear();
    }

    public void start()
    {
        if (m_running) return;
        m_running = true;
        m_canvas = RenderTexture.GetTemporary(m_canvasSize.x, m_canvasSize.y, 32, RenderTextureFormat.ARGB32);
        m_camera.targetTexture = m_canvas;
        int count = m_objects.Count;
        for (int i = 0; i < count; i++)
        {
            m_objects[i].SetActive(false);
        }
        update(0);
    }

    public void done()
    {
        showComplete();
        m_running = false;
        m_camera.targetTexture = null;
        AssetDatabase.Refresh();
    }

    protected void showComplete()
    {
        EditorUtility.DisplayDialog("Prefab Printer", "done.", "OK");
        Debug.Log("Prefab Printer: done.");
    }

    public void update(float deltaSec)
    {
        if (!m_running) return;
        if (m_currentObject == null)
        {
            if (m_objects.Count > 0)
            {
                m_currentObject = m_objects[0];
                m_currentObject.SetActive(true);
                m_objects.RemoveAt(0);
                startPrint();
            }
            else
            {
                done();
            }
            return;
        }
        updatePrint(deltaSec);
    }

    protected void startPrint()
    {
        m_currentFrame = 0;
        m_currentIntervalCount = 0;
        m_currentTimePass = 0;
        m_currentDuration = m_duration > 0 ? m_duration : PrefabPrinterUtility.CalculateObjectDuraion(m_currentObject, true);
        m_currentIntervalTotal = m_currentDuration / (m_frameTotal - 1);
        m_currentOutputPath = System.IO.Path.Combine(Application.dataPath, m_outputFolder);
        m_currentObjectName = m_currentObject.name;
        if (!System.IO.Directory.Exists(m_currentOutputPath))
        {
            System.IO.Directory.CreateDirectory(m_currentOutputPath);
        }
        if (m_outputCreateFolder)
        {
            m_currentOutputPath = System.IO.Path.Combine(m_currentOutputPath, m_currentObjectName);
            if (!System.IO.Directory.Exists(m_currentOutputPath))
            {
                System.IO.Directory.CreateDirectory(m_currentOutputPath);
            }
        }
        PrefabPrinterUtility.PlayObject(m_currentObject);
        print();
        if (m_currentDuration <= 0)
        {
            completePrint();
        }
    }

    protected void updatePrint(float deltaSec)
    {
        m_currentIntervalCount += deltaSec;
        while (m_currentIntervalCount >= m_currentIntervalTotal)
        {
            m_currentIntervalCount -= m_currentIntervalTotal;
            print();
            if (m_currentFrame >= m_frameTotal)
            {
                break;
            }
        }
        m_currentTimePass += deltaSec;
        if (m_currentFrame >= m_frameTotal)
        {
            completePrint();
        }
        else
        {
            if (m_currentTimePass >= m_currentDuration)
            {
                completePrint();
            }
        }
    }

    protected void completePrint()
    {
        m_currentObject = null;
    }

    protected void print()
    {
        m_currentFrame++;
        RenderTexture cachedTexture = RenderTexture.active;
        RenderTexture.active = m_canvas;
        Color cachedColor = m_camera.backgroundColor;
        int vx = 0;
        int vy = 0;
        int vw = m_printSize.x;
        int vh = m_printSize.y;
        if (m_printSize.x != m_canvasSize.x) vx = Mathf.FloorToInt(m_canvas.width * 0.5f - m_printSize.x * 0.5f);
        if (m_printSize.y != m_canvasSize.y) vy = Mathf.FloorToInt(m_canvas.height * 0.5f - m_printSize.y * 0.5f);
        m_camera.backgroundColor = Color.clear;
        m_camera.Render();
        Texture2D texture = new Texture2D(vw, vh, m_outputTextureFormat, false);
        texture.ReadPixels(new Rect(vx, vy, m_printSize.x, m_printSize.y), 0, 0);
        string ext = string.Empty;
        byte[] bytes = null;
        switch (m_outputTextureType)
        {
            case PrefabPrinterTextureTypes.JPG:
                {
                    bytes = texture.EncodeToJPG();
                    ext = "jpg";
                }
                break;
            default:
                {
                    Color blackCol;
                    for (int i = 0; i < m_printSize.x; i++)
                    {
                        for (int j = 0; j < m_printSize.y; j++)
                        {
                            blackCol = texture.GetPixel(i, j);
                            blackCol.a = 0.299f * blackCol.r + 0.587f * blackCol.g + 0.114f * blackCol.b;
                            texture.SetPixel(i, j, blackCol);
                        }
                    }
                    bytes = texture.EncodeToPNG();
                    ext = "png";
                }
                break;
        }
        string fileName = string.Format(m_outputNameFormat, m_currentObjectName, m_currentFrame);
        fileName = string.Format("{0}.{1}", fileName, ext);
        string path = System.IO.Path.Combine(m_currentOutputPath, fileName);
        System.IO.File.WriteAllBytes(path, bytes);
        RenderTexture.active = cachedTexture;
        m_camera.backgroundColor = cachedColor;
    }

    public void dispose()
    {
        Debug.Log("dispose Prefab Printer");
        if (m_canvas != null)
        {
            RenderTexture.ReleaseTemporary(m_canvas);
            m_canvas = null;
        }
        m_camera = null;
    }
}

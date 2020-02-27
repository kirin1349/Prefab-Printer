using UnityEditor;
using UnityEngine;

public class PrefabPrinterUtility
{
    public static float CalculateObjectDuraion(GameObject go, bool ignoreParentPartical)
    {
        float duration = 0;
        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            if (ignoreParentPartical)
            {
                ParticleSystem[] childPSs = go.GetComponentsInChildren<ParticleSystem>();
                if (childPSs != null && childPSs.Length > 0)
                {
                    int count = childPSs.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (childPSs[i].main.loop) continue;
                        duration = Mathf.Max(duration, childPSs[i].main.duration);
                    }
                }
            }
            else
            {
                if (!ps.main.loop)
                {
                    duration = ps.main.duration;
                }
            }
        }
        Animation ani = go.GetComponent<Animation>();
        if (ani != null)
        {
            if (ani.clip != null)
            {
                if (ani.clip.wrapMode == WrapMode.Default || ani.clip.wrapMode == WrapMode.Once)
                {
                    duration = Mathf.Max(duration, ani.clip.length);
                }
            }
        }
        Animator amt = go.GetComponent<Animator>();
        if (amt != null)
        {
            AnimatorStateInfo amtStateInfo = amt.GetCurrentAnimatorStateInfo(0);
            if (!amtStateInfo.loop)
            {
                duration = Mathf.Max(duration, amtStateInfo.length);
            }
        }
        return duration;
    }

    public static void PlayObject(GameObject go)
    {
        Selection.activeGameObject = go;
        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop();
            ps.Play();
        }
        Animation ani = go.GetComponent<Animation>();
        if (ani != null)
        {
            if (ani.clip != null)
            {
                ani.Stop();
                ani.Play();
            }
        }
        Animator amt = go.GetComponent<Animator>();
        if (amt != null)
        {
            // TODO: 
        }
    }
}

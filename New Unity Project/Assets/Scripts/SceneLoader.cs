using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    public Animator Ani;
    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadSceneWithAni(string Scenename)
    {
        StartCoroutine(LS(Scenename));
    }

    public void LoadSceneWithAniEndGame(string Scenename, Dictionary<string, int> Eg)
    {
        StartCoroutine(LSE(Scenename, Eg));
    }
    public void LoadSceneWithAniError(string Scenename, string ErrorMes)
    {
        StartCoroutine(LSG(Scenename, ErrorMes));
    }
    public void LoadSceneWithAniFake()
    {
        Ani.SetTrigger("trans");
    }

    public void LoadSceneWithAniFakeClose()
    {
        Ani.SetTrigger("Reset");
    }


    public void LoadSceneWithGam(GameObject a, GameObject b)
    {
        StartCoroutine(LSG(a, b));
    }

    IEnumerator LS(string name)
    {
        Ani.SetTrigger("trans");
        yield return new WaitForSeconds(0.8f);
        SceneManager.LoadScene(name);
        Ani.SetTrigger("Reset");
    }
    IEnumerator LSG(string name, string e)
    {
        Ani.SetTrigger("trans");
        yield return new WaitForSeconds(0.8f);
        SceneManager.LoadScene(name);
        Ani.SetTrigger("Reset");
        ConnectToServer.Instance.DisplayErrorMesage(e);
    }
    IEnumerator LSE(string name, Dictionary<string, int> eg)
    {
        Ani.SetTrigger("trans");
        yield return new WaitForSeconds(0.8f);
        SceneManager.LoadScene(name);
        Ani.SetTrigger("Reset");
        yield return new WaitForSeconds(0.1f);
        EndGameManeger.Instance.Info = eg;
    }
    IEnumerator LSG(GameObject a, GameObject b)
    {
        Ani.SetTrigger("trans");
        yield return new WaitForSeconds(0.8f);
        a.SetActive(false);
        b.SetActive(true);
        Ani.SetTrigger("Reset");
    }

    public void ResetAni()
    {
        Ani.SetTrigger("Reset");
    }
}

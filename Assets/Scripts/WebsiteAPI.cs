using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

public class WebsiteAPI : MonoBehaviour {

     
    public MeshRenderer meshRenderer;

    public void LoadImage(string url, UnityAction callback){
        StartCoroutine(GetImageFromURL(url,callback));
    }

    IEnumerator GetImageFromURL(string url, UnityAction callback) {
        string ENDPOINT = "https://api.urlbox.io/v1/iXuQiQD5YyIblY9W/png?url=" + url + "&thumb_width=600&ttl=86400";
        Debug.Log(ENDPOINT);
        UnityWebRequest www = UnityWebRequest.Get(ENDPOINT);
       
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
        } else {
            Debug.Log("no error");
            byte[] imageBytes = www.downloadHandler.data;
            Texture2D tempTex = new Texture2D(2, 2);
            tempTex.LoadImage(imageBytes);
            meshRenderer.material.mainTexture = tempTex;
            callback();
        }
    }

    public void LoadImage1(string url)
    {
        StartCoroutine(GetImageFromURL1(url));
    }

    IEnumerator GetImageFromURL1(string url)
    {
        string ENDPOINT = "https://api.urlbox.io/v1/iXuQiQD5YyIblY9W/png?url=" + url + "&thumb_width=600&ttl=86400";
        Debug.Log(ENDPOINT);
        UnityWebRequest www = UnityWebRequest.Get(ENDPOINT);

        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("no error");
            byte[] imageBytes = www.downloadHandler.data;
            Texture2D tempTex = new Texture2D(2, 2);
            tempTex.LoadImage(imageBytes);
            meshRenderer.material.mainTexture = tempTex;
           
        }
    }




}

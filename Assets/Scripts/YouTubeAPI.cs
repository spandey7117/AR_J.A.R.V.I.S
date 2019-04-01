using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.Linq;
using System;
using UnityEngine.Events;

[System.Serializable]
public class VideoInfo {
    public string quality;
    public string url;
    public string itag;
    public string mimeType;
}

[System.Serializable]
public class VideoInfoCollection {
    public VideoInfo[] videoInfoCollection;
}

[RequireComponent(typeof(VideoPlayer))]
public class YouTubeAPI : MonoBehaviour {

    const string API_ENDPOINT = "https://you-link.herokuapp.com/?url=";

    public string YouTubeURL;

    VideoPlayer videoPlayer;

    // Start is called before the first frame update
    void Start() {
        videoPlayer = GetComponent<VideoPlayer>();
      
    }

    public void LoadVideo(string url ,UnityAction callback) {
        url = "https://www.youtube.com/watch?v=TcMBFSGVi1c";
        StartCoroutine(GetYouTubeLinkRoutine(url, callback));
    }

    IEnumerator GetYouTubeLinkRoutine(string url , UnityAction callback) {
        UnityWebRequest www = UnityWebRequest.Get(API_ENDPOINT + url);
        yield return www.SendWebRequest();
        //read json response into object
        VideoInfo[] videoArray = JsonUtility.FromJson<VideoInfoCollection>(
        "{\"videoInfoCollection\":" + www.downloadHandler.text + "}").videoInfoCollection;
        //find video link with desired quality
        VideoInfo videoInfo = videoArray.Where(
        item => item.quality == "medium" && item.mimeType.Contains("mp4")).FirstOrDefault();
        try {
            videoPlayer.url = videoInfo.url;
            videoPlayer.Prepare();
            Debug.Log("Video Loaded");
        } catch (NullReferenceException e) {
            Debug.Log(e);
            StopAllCoroutines();
        }
        while (!videoPlayer.isPrepared) {
            yield return new WaitForEndOfFrame();
        }
        callback();
        videoPlayer.Play();

    }


}

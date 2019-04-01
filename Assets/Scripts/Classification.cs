using UnityEngine;
using TensorFlow;
using System.Linq;
using System.Threading;
using Vuforia;
using System.Net;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System;
using UnityEngine.Networking;

public class Classification : MonoBehaviour {
    private AudioSource ass;
    [Header("Constants")]
	private const int INPUT_SIZE = 224;
	private const int IMAGE_MEAN = 117;
	private const float IMAGE_STD = 1;
	private const string INPUT_TENSOR = "input";
	private const string OUTPUT_TENSOR = "output";
    public GameObject gm;
    [Header("Inspector Stuff")]
	public CameraFeedBehavior camFeed;
    public TextAsset labelMap;
    public TextAsset model;
	public TextMeshPro messageBehavior;

	private TFGraph graph;
	private TFSession session;
	private string [] labels;

    Thread modelThread;
    Color32[] pixels;
    TFTensor tensor;
    private string currLabel = "";
    string Ttext;
	// Use this for initialization
	void Start() {
        ass = gameObject.GetComponent<AudioSource>();
#if UNITY_ANDROID && !UNITY_EDITOR
        TensorFlowSharp.Android.NativeBinding.Init();
#endif
        //load labels into string array
        labels = labelMap.ToString ().Split ('\n');
		//load graph
		graph = new TFGraph ();
		graph.Import (model.bytes);
		session = new TFSession (graph);
    }

    private void Update() {
        if (currLabel.Length > 0) {
            messageBehavior.text=currLabel;
            gm.SetActive(true);
            Ttext = currLabel;
            StartCoroutine(GetAudioClip());
            StartCoroutine(PauseRoutine());
            currLabel = "";
        }
    }

    public void ProcessImage(){
        Image image = camFeed.GetImage();
        Texture2D camTex = new Texture2D(image.Width, image.Height);
        //copy to texture
        image.CopyToTexture(camTex);
        //crop
        var cropped = TextureTools.CropTexture(camTex);
        //scale
        var scaled = TextureTools.scaled(cropped, 224, 224, FilterMode.Bilinear);
        //return scaled color32[]
        pixels = scaled.GetPixels32();
        //create tensor
        tensor = TransformInput(pixels, INPUT_SIZE, INPUT_SIZE);
        //run model on other thread
        modelThread = new Thread(RunModel);
        modelThread.Start();
	}

    void RunModel() {
        //pass in input tensor
        var runner = session.GetRunner();
        runner.AddInput(graph[INPUT_TENSOR][0], tensor).Fetch(graph[OUTPUT_TENSOR][0]);
        var output = runner.Run();
        //put results into one dimensional array
        float[] probs = ((float[][])output[0].GetValue(jagged: true))[0];
        //get max value of probabilities and find its associated label index
        float maxValue = probs.Max();
        int maxIndex = probs.ToList().IndexOf(maxValue);
        //print label with highest probability
        string label = labels[maxIndex];
        currLabel = label;
    }

    //stole from https://github.com/Syn-McJ/TFClassify-Unity
    public static TFTensor TransformInput (Color32 [] pic, int width, int height) {
		float [] floatValues = new float [width * height * 3];

		for (int i = 0; i < pic.Length; ++i) {
			var color = pic [i];

			floatValues [i * 3 + 0] = (color.r - IMAGE_MEAN) / IMAGE_STD;
			floatValues [i * 3 + 1] = (color.g - IMAGE_MEAN) / IMAGE_STD;
			floatValues [i * 3 + 2] = (color.b - IMAGE_MEAN) / IMAGE_STD;
		}

		TFShape shape = new TFShape (1, width, height, 3);

		return TFTensor.FromBuffer (shape, floatValues, 0, floatValues.Length);
	}

    IEnumerator TextToSpeech()
    {

       
        String url = "https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q=" + Ttext;
        WWW www = new WWW(url);
        yield return www;
        ass.clip = www.GetAudioClip(false, true, AudioType.MPEG);
        ass.Play();
    }

    private IEnumerator PauseRoutine()
    {
        yield return new WaitForSeconds(2f);
        gm.SetActive(false);
    }
    IEnumerator GetAudioClip()
    {
        String url = "https://translate.google.com/translate_tts?ie=UTF-8&client=tw-ob&tl=en&q=" + Ttext;
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
               // AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                ass.clip = DownloadHandlerAudioClip.GetContent(www);
                ass.Play();
            }
        }
    }

}


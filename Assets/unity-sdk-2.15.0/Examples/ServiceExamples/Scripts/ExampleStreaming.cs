/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/
#pragma warning disable 0649

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Networking;
using UnityEngine.Video;

public class ExampleStreaming : MonoBehaviour
{
    String Ttext = "";
    public GameObject hud;
    #region PLEASE SET THESE VARIABLES IN THE INSPECTOR
    [Space(10)]
    [Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/speech-to-text/api\"")]
    [SerializeField]
    private string _serviceUrl;
    [Tooltip("Text field to display the results of streaming.")]
    public TextMeshPro ResultsField;
    [Header("IAM Authentication")]
    [Tooltip("The IAM apikey.")]
    [SerializeField]
    private string _iamApikey;
    public GameObject gm;
    [Header("Parameters")]
    // https://www.ibm.com/watson/developercloud/speech-to-text/api/v1/curl.html?curl#get-model
    [Tooltip("The Model to use. This defaults to en-US_BroadbandModel")]
    [SerializeField]
    private string _recognizeModel;
    #endregion


    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;

    private SpeechToText _service;
    AudioSource aas;
  public  void Start()
    {
        aas = GetComponent<AudioSource>();
        LogSystem.InstallDefaultReactors();
        Runnable.Run(CreateService());
    }

    private IEnumerator CreateService()
    {
        if (string.IsNullOrEmpty(_iamApikey))
        {
            throw new WatsonException("Plesae provide IAM ApiKey for the service.");
        }

        //  Create credential and instantiate service
        Credentials credentials = null;

        //  Authenticate using iamApikey
        TokenOptions tokenOptions = new TokenOptions()
        {
            IamApiKey = _iamApikey
        };

        credentials = new Credentials(tokenOptions, _serviceUrl);

        //  Wait for tokendata
        while (!credentials.HasIamTokenData())
            yield return null;

        _service = new SpeechToText(credentials);
        _service.StreamMultipart = true;

        Active = true;
        StartRecording();
    }

    public bool Active
    {
        get { return _service.IsListening; }
        set
        {
            if (value && !_service.IsListening)
            {
                _service.RecognizeModel = (string.IsNullOrEmpty(_recognizeModel) ? "en-US_BroadbandModel" : _recognizeModel);
                _service.DetectSilence = true;
                _service.EnableWordConfidence = true;
                _service.EnableTimestamps = true;
                _service.SilenceThreshold = 0.01f;
                _service.MaxAlternatives = 0;
                _service.EnableInterimResults = true;
                _service.OnError = OnError;
                _service.InactivityTimeout = -1;
                _service.ProfanityFilter = false;
                _service.SmartFormatting = true;
                _service.SpeakerLabels = false;
                _service.WordAlternativesThreshold = null;
                _service.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _service.IsListening)
            {
                _service.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
				record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _service.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }
    int busy1 = 0;
    int busy2 = 0;
    int busy3 = 0;
    public GameObject screen3;
    public GameObject screen2;
    public GameObject screen1;

    public GameObject screen3o;
    public GameObject screen2o;
    public GameObject screen1o;
    private void OnRecognize(SpeechRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence);
                    Log.Debug("ExampleStreaming.OnRecognize()", text);

                    if (text.Contains("Final")) {
                        if (text.Contains("Control") || text.Contains("control"))
                        {
                            gm.SetActive(true);
                            ResultsField.text = text.Substring(0, text.Length - 14);
                            StartCoroutine(PauseRoutine());
                            if ((text.Contains("Display") || text.Contains("display")) && (text.Contains("ready") || text.Contains("Ready")))
                            {
                                controllerScript ss = hud.GetComponent<controllerScript>();
                                if (screen3 == null)
                                {
                                    screen3 = ss.CreateNewScreen();
                                }
                                else
                                {
                                    Ttext = "Screen already Present";
                                    ttss();
                                }
                            }

                            else if ((text.Contains("Display") || text.Contains("display")) && (text.Contains("destroy") || text.Contains("Destroy")))
                            {
                                controllerScript ss = hud.GetComponent<controllerScript>();
                                if (screen3 != null)
                                {
                                    ss.deleteScreen(screen3);
                                }
                                else
                                {
                                    Ttext = "Nothing to destroy";
                                    ttss();
                                }



                            }
                            else if ((text.Contains("fresh") || text.Contains("Refresh")))
                            {
                                   if (screen2 != null )
                                {
                                    busy2 = 0;
                                    Material loading = Resources.Load("Resources/loading.mat", typeof(Material)) as Material;
                                    screen2.GetComponent<VideoPlayer>().Stop();
                                    MeshRenderer mr = screen2.GetComponent<MeshRenderer>();
                                    mr.material.color = Color.black;
                                    // screen2.SetActive(false);

                                    // screen2o.SetActive(true);
                                    // screen2o.transform.position = new Vector3(screen2o.transform.position.x, (float)(screen2o.transform.position.y + .2), screen2o.transform.position.z);
                                    //screen2.GetComponents<MeshRenderer>
                                }

                                if (screen1 != null)
                                {
                                    busy1 = 0;
                                    Material loading = Resources.Load("Resources/loading.mat", typeof(Material)) as Material;
                                    screen1.GetComponent<VideoPlayer>().Stop();
                                    MeshRenderer mr = screen1.GetComponent<MeshRenderer>();
                                    mr.material.color = Color.black;
                                    //screen1.SetActive(false);

                                    //screen1o.SetActive(true);
                                    // screen1o.transform.position = new Vector3(screen1o.transform.position.x, (float)(screen1o.transform.position.y  .2), screen1o.transform.position.z);
                                    //screen2.GetComponents<MeshRenderer>
                                }

                            } 
                        else if ((text.Contains("what") || text.Contains("What")) && (text.Contains("this") || text.Contains("This")))
                        {
                            Classification cc = hud.GetComponent<Classification>();
                            cc.ProcessImage();

                        }
                        else if (text.Contains("Load") || text.Contains("load") || text.Contains("Lord") || text.Contains("lord"))
                        {
                                string url = "www." + "google" + ".com";
                                if (text.Contains("google") || text.Contains("goo"))
                                {
                                     url = "www." + "google" + ".com";
                                }
                                else if (text.Contains("apple") || text.Contains("apple"))
                                {
                                    url = "www." + "apple" + ".com";
                                }
                                else if (text.Contains("tcd") || text.Contains("trinity"))
                                {
                                    url = "www." + "tcd" + ".ie";
                                }
                                else if (text.Contains("vsense") || text.Contains("sense"))
                                {
                                    url = "v-sense.scss.tcd.ie";
                                }


                                if (screen3 != null)
                            {
                                screen3.GetComponent<DisplayBehavior>().LoadWebsite(url);
                                busy3 = 1;
                            }
                            else if (screen2 != null && busy2 == 0)
                            {
                                Debug.Log("inside screen 2:");
                                Debug.Log("busy2:" + busy2);
                                Debug.Log("busy1:" + busy1);
                                
                                  screen2.SetActive(true);
                                screen2.GetComponent<WebsiteAPI>().LoadImage1(url);
                                    screen2o = GameObject.FindWithTag("screen2");
                                    screen2o.SetActive(false);
                                     //  screen2o.transform.position = new Vector3(screen2o.transform.position.x, (float)(screen2o.transform.position.y - .2), screen2o.transform.position.z);
                                     busy2 = 1;
                            }
                            else if (screen1 != null && busy1 == 0)
                            {
                                Debug.Log("inside screen 1:");
                                Debug.Log("busy2:" + busy2);
                                Debug.Log("busy1:" + busy1);
                                
                                  screen1.SetActive(true);
                                screen1.GetComponent<WebsiteAPI>().LoadImage1(url);
                                    screen1o = GameObject.FindWithTag("screen1");
                                    screen1o.SetActive(false);
                                    // screen1o.transform.position = new Vector3(screen1o.transform.position.x, (float)(screen1o.transform.position.y - .2), screen1o.transform.position.z);
                                    busy1 = 1;
                            }

                        }
                        else if (text.Contains("video") || text.Contains("video")|| text.Contains("radio"))
                        {
                                string url = "https://www.youtube.com/watch?v=TcMBFSGVi1c";
                                if (text.Contains("marvel") || text.Contains("capt") || text.Contains("mar") )
                                {
                                    url = "https://www.youtube.com/watch?v=0LHxvxdRnYc";
                                }
                                else if (text.Contains("avenger") || text.Contains("avg") || text.Contains("end") || text.Contains("game"))
                                {
                                    url = "https://www.youtube.com/watch?v=TcMBFSGVi1c";
                                }
                                else if (text.Contains("uri") || text.Contains("attack"))
                                {
                                    url = "https://www.youtube.com/watch?v=Cg8sbRFS3zU";
                                }
                                else if (text.Contains("ana") || text.Contains("bel"))
                                {
                                    url = "https://www.youtube.com/watch?v=tCXGJQYZ9JA";
                                }
                                else if (text.Contains("black") || text.Contains("pan"))
                                {
                                    url = "https://www.youtube.com/watch?v=gAEXKmfAFaE";
                                }

                                if (screen3 != null)
                            {
                                screen3.GetComponent<DisplayBehavior>().LoadVideo(url);
                                busy3 = 1;
                            }
                            else if (screen2 != null && busy2 == 0)
                            {
                               
                                  screen2.SetActive(true);
                                screen2.GetComponent<YouTubeAPI>().LoadVideo1(url);
                                GameObject.FindWithTag("screen2").SetActive(false);
                                busy2 = 1;
                            }
                            else if (screen1 != null && busy1 == 0)
                            {

                                
                                 screen1.SetActive(true);
                                screen1.GetComponent<YouTubeAPI>().LoadVideo1(url);
                                GameObject.FindWithTag("screen1").SetActive(false);
                                busy1 = 1;
                            }


                        }
                        else
                        {
                            integrator ii = GameObject.FindWithTag("agent").GetComponent<integrator>();
                            ii.SendText(text.Substring(7));
                        }
                        }
                    }
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach(var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result, Dictionary<string, object> customData)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
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
            aas.clip = DownloadHandlerAudioClip.GetContent(www);
            aas.Play();
        }
    }
}

public void ttss()
{
    StartCoroutine(GetAudioClip());
}


}
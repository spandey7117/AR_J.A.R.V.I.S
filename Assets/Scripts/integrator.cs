//
// API.AI Unity SDK Sample
// =================================================
//
// Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
// https://www.api.ai
//
// ***********************************************************************************************************************
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
// an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
//
// ***********************************************************************************************************************
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using ApiAiSDK;
using ApiAiSDK.Model;
using ApiAiSDK.Unity;
using System.Net;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Networking;

public class integrator : MonoBehaviour
{
    public GameObject gm;
    private AudioSource ass;
    public TextMeshPro answerTextField;
    public TextMeshPro inputTextField;
    private ApiAiUnity apiAiUnity;

    String Ttext;

    private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
    };

    private readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    // Use this for initialization

    IEnumerator GetAudioClip()
    {
        Ttext = answerTextField.text;
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

    public void ttss()
    {
        StartCoroutine(GetAudioClip());
    }

    IEnumerator Start()
    {
        ass = gameObject.GetComponent<AudioSource>();
        // check access to the Microphone
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            throw new NotSupportedException("Microphone using not authorized");
        }

        ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) =>
        {
            return true;
        };

        const string ACCESS_TOKEN = "5e8b3253362c4f038ce24e40a650271f";

        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

        apiAiUnity = new ApiAiUnity();
        apiAiUnity.Initialize(config);

        apiAiUnity.OnError += HandleOnError;
        apiAiUnity.OnResult += HandleOnResult;


    }

    void HandleOnResult(object sender, AIResponseEventArgs e)
    {
        RunInMainThread(() => {
            var aiResponse = e.Response;
            if (aiResponse != null)
            {
                Debug.Log(aiResponse.Result.ResolvedQuery);
                var outText = JsonConvert.SerializeObject(aiResponse, jsonSettings);

                Debug.Log(outText);

                answerTextField.text = outText;

            }
            else
            {
                Debug.LogError("Response is null");
            }
        });
    }

    void HandleOnError(object sender, AIErrorEventArgs e)
    {
        RunInMainThread(() => {
            Debug.LogException(e.Exception);
            Debug.Log(e.ToString());
            answerTextField.text = e.Exception.Message;
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (apiAiUnity != null)
        {
            apiAiUnity.Update();
        }

        // dispatch stuff on main thread
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

    private void RunInMainThread(Action action)
    {
        ExecuteOnMainThread.Enqueue(action);
    }

    public void PluginInit()
    {

    }

   

    public void SendText( String text)
    {



        Debug.Log(text);

        AIResponse response = apiAiUnity.TextRequest(text);

        if (response != null)
        {
            Debug.Log("Resolved query: " + response.Result.ResolvedQuery);
            var outText = JsonConvert.SerializeObject(response, jsonSettings);

            Debug.Log("Result: " + outText);
            String[] new1=outText.Split(':');
         //   Debug.Log(new1.Length);
          //  for (int i=0;i<new1.Length;i++)
          //  {
                Debug.Log( new1[14]);
            String[] res = new1[14].Split('}');
            String res1 = res[0];
            res1 = res1.Substring(1, res1.Length - 2);
            //  }
            gm.SetActive(true);
            answerTextField.text = res1;
            ttss();
            StartCoroutine(PauseRoutine());

        }
        else
        {
            Debug.LogError("Response is null");
        }

    }
    private IEnumerator PauseRoutine()
    {
        yield return new WaitForSeconds(4f);
        gm.SetActive(false);
    }



}

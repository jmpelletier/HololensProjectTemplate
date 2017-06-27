using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VR.WSA;
using UnityEngine.Windows.Speech;

/**
 * このクラスはスピーチ認識機能を管理する
 */
public class SpeechManager : MonoBehaviour
{
    public static InstanceManager<SpeechManager> instances = new InstanceManager<SpeechManager>();

    public delegate void DictationUpdateDelegate(string text);
    public delegate void DictationCompleteDelegate(string text);

    public event DictationUpdateDelegate OnDictationUpdate;
    public event DictationCompleteDelegate OnDictationComplete;

    // 認識するオブジェクト
    KeywordRecognizer keywordRecognizer = null;

    DictationRecognizer dictationRecognizer = null;

    // 認識できるフレーズと認識された時に実行されるアクションを格納するDictionary
    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    public string dictationText
    {
        get;
        private set;
    }
  
    void Awake()
    {
        instances.Add(this);
    }

    private void Start()
    {
        // この様に認識されるフレーズと実行するアクションを登録する。ユーザが「hello」と
        // 声かけたら、コンソールに「Hello」が出力される。
        keywords.Add("Hello", () =>
        {
            NotificationView.Print("Hi!");
        });

        keywords.Add("Select", () =>
        {
            GestureManager.instances.main.Tap();
        });

        keywords.Add("Show mesh", () => {
            foreach (SpatialMappingRenderer renderer in FindObjectsOfType(typeof(SpatialMappingRenderer)))
            {
                renderer.renderState = SpatialMappingRenderer.RenderState.Visualization;
            }
        });

        keywords.Add("Hide mesh", () => {
            foreach (SpatialMappingRenderer renderer in FindObjectsOfType(typeof(SpatialMappingRenderer)))
            {
                renderer.renderState = SpatialMappingRenderer.RenderState.Occlusion;
            }
        });

        // 認識するKeywordRecognizerオブジェクトに認識可能なフレーズを登録する
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());

        // フレーズが認識された時に実行するメソッドを登録する
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;


        dictationRecognizer = new DictationRecognizer();
        dictationRecognizer.DictationHypothesis += (text) =>
        {
            dictationText = text;
            OnDictationUpdate(text);
        };

        dictationRecognizer.DictationResult += (text, confidence) =>
        {
            dictationText = text;
            OnDictationComplete(text);
        };

        dictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogError(string.Format("Dictation error: {0}", error));
        };

        // 認識を開始する
        keywordRecognizer.Start();
    }

    private IEnumerator _toggleDictation(bool state)
    {
        if (state)
        {
            keywordRecognizer.Stop();
            PhraseRecognitionSystem.Shutdown();

            while (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
            {
                yield return null;
            }

            dictationRecognizer.Start();
        } else
        {
            dictationRecognizer.Stop();

            while (dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                yield return null;
            }

            PhraseRecognitionSystem.Restart();
            keywordRecognizer.Start();
        }
    }

    public void StartDictation()
    {
        StartCoroutine(_toggleDictation(true));
    }

    public void EndDictation()
    {
        StartCoroutine(_toggleDictation(false));
    }

    public void ClearDictation()
    {
        dictationText = "";
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        // 認識されたフレーズ（args.text）から実行するアクションを取得して、実行する。
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}
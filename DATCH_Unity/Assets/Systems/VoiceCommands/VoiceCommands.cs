using HoloToolkit.Examples.GazeRuler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceCommands : MonoBehaviour {

    public MeasureManager measureManager;
    public StorageManager storageManager;

    KeywordRecognizer keywordRecognizer;

    // Defines which function to call when a keyword is recognized
    delegate void KeywordAction(PhraseRecognizedEventArgs args);
    Dictionary<string, KeywordAction> keywordCollection = new Dictionary<string, KeywordAction>();

    // Use this for initialization
    void Start ()
    {
        //SetupVoiceCommands();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void SetupVoiceCommands()
    {
        // Line Editing
        keywordCollection.Add("Edit", measureManager.Edit);
        keywordCollection.Add("Draw", measureManager.Draw);
        keywordCollection.Add("Clear All", measureManager.ClearAllShapes); // clears all finisehd shaped in the stack
        keywordCollection.Add("Reset Shape", measureManager.ResetShape);
        keywordCollection.Add("Close Shape", measureManager.OnPolygonClose);


        keywordRecognizer = new KeywordRecognizer(keywordCollection.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        KeywordAction keywordAction;

        if (keywordCollection.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke(args);
        }
    }
}

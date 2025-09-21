using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TalkingScript : MonoBehaviour
{
    public GameObject policeChief;
    public GameObject detective;

    public GameObject whoDidItPanel;

    [System.Serializable]
    public class ConversationData
    {
        public string text;
        public GameObject speaker;
    }

    public ConversationData[] conversations;

    private int dialougeNumber;
    private TMP_Text txt;
    public float typingSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //policeChief.GetComponent<SliderInt>().slide;
        policeChief.SetActive(false);
        detective.SetActive(false);
        NextDialouge();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NextDialouge()
    {
        if (dialougeNumber >= conversations.Length)
        {
            // If current scene is Intro
            if (SceneManager.GetActiveScene().name == "Intro")
            {
                SceneManager.LoadScene("Tutorial");
                return;
            }
            else
            {
                detective.SetActive(false);
                policeChief.SetActive(false);
                whoDidItPanel.SetActive(true);
                return;
            }
        }

        ConversationData current = conversations[dialougeNumber];
        if (current.speaker == policeChief)
        {
            detective.SetActive(false);
            policeChief.SetActive(true);
        }
        else if (current.speaker == detective)
        {
            policeChief.SetActive(false);
            detective.SetActive(true);
        }
        GameObject currentSpeaker = current.speaker;

        Button btn = currentSpeaker.GetComponentInChildren<Button>();

        btn.gameObject.SetActive(false);
        StopAllCoroutines();
        txt = currentSpeaker.GetComponentInChildren<TMP_Text>();
        StartCoroutine(TypeDialogue(conversations[dialougeNumber].text));

        btn.gameObject.SetActive(true);
        dialougeNumber++;
        
    }
    private System.Collections.IEnumerator TypeDialogue(string dialogue)
    {
        txt.text = "";
        foreach (char letter in dialogue.ToCharArray())
        {
            txt.text += letter;
            yield return new WaitForSeconds(typingSpeed); // Adjust typing speed as needed
        }

    }
}

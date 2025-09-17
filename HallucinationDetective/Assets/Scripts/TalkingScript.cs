using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;




public class TalkingScript : MonoBehaviour
{

    public struct conversation {
        public string text;
        public GameObject speaker;
    }

    public GameObject policeChief;
    public GameObject detective;

    public conversation[] conversations;

    private int dialougeNumber;
    public TMP_Text txt;
    public float typingSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //policeChief.GetComponent<SliderInt>().slide;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NextDialouge()
    {
        StopAllCoroutines();
        StartCoroutine(TypeDialogue(conversations[dialougeNumber].text));
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

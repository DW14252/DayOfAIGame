using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using TMPro.EditorUtilities;

public class TutorialManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public String[] pregameplayTutorialText;
    public GameObject clickToContinuePanel;

    public TMP_Text tutorialTextBox;

    public Button continueButton;

    public GameObject claimsPanel;
    public String[] claimsPanelTutorialText;

    public String[] detectiveToolsTutorialText;

    public GameObject dummycard;
    public GameObject cardInfo;
    public bool readInfo = false;

    public String[] useDetectiveToolsTutorialText;

    public bool draggedDummyCard = false;
    public TMPro.TMP_Text detectiveCluesText;


    public String[] dragClaimToFalsePanelText;

    public String[] endOfTutorialText;



    void Start()
    {
        clickToContinuePanel.SetActive(false);
        tutorialTextBox.text = "";
        //StartCoroutine(TypeWriterEffect(pregameplayTutorialText[0], 0.05f));
        //Invoke("EnableClickToContinue", 5f);
        //wait until the user clicks to continue
        //then show the next text
        //repeat until all text is shown
        //then hide the tutorial panel
        
        StartCoroutine(LoopThroughTutorialText());
    }

    private System.Collections.IEnumerator LoopThroughTutorialText()
    {
        for (int i = 0; i < pregameplayTutorialText.Length; i++)
        {
            tutorialTextBox.text = "";
            yield return TypeWriterEffect(pregameplayTutorialText[i], 0.03f);
            clickToContinuePanel.SetActive(true);
            bool clicked = false;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => clicked = true);
            yield return new WaitUntil(() => clicked);
            clickToContinuePanel.SetActive(false);
        }
        tutorialTextBox.text = "";

        // start the claims panel coroutine
        claimsPanel.SetActive(true);

        for (int i = 0; i < claimsPanelTutorialText.Length; i++)
        {
            tutorialTextBox.text = "";
            yield return TypeWriterEffect(claimsPanelTutorialText[i], 0.03f);
            clickToContinuePanel.SetActive(true);
            bool clicked = false;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => clicked = true);
            if (i < claimsPanelTutorialText.Length - 1)
            {
                yield return new WaitUntil(() => clicked);
            }
            clickToContinuePanel.SetActive(false);
        }
        continueButton.gameObject.SetActive(false);

        // wait until the claims panel is dragged to the true position, the claims panel will have a script that sets a bool to true when it is in the correct position
        yield return new WaitUntil(() => claimsPanel.GetComponent<claimGenerator>().isTutorialPosTrue);

        claimsPanel.SetActive(false);
        tutorialTextBox.text = "";
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() => EnableClickToContinue());
        for (int i = 0; i < detectiveToolsTutorialText.Length; i++)
        {
            tutorialTextBox.text = "";
            yield return TypeWriterEffect(detectiveToolsTutorialText[i], 0.03f);
            clickToContinuePanel.SetActive(true);
            bool clicked = false;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => clicked = true);
            // only wait for click if not the last text
            if (i < detectiveToolsTutorialText.Length - 1)
            {
                yield return new WaitUntil(() => clicked);
            }
            clickToContinuePanel.SetActive(false);
        }

        dummycard.SetActive(true);
        continueButton.gameObject.SetActive(false);

        // wait until readInfo is true
        yield return new WaitUntil(() => readInfo);
        //claimsPanel.transform.position = new Vector3(0, 93, 0);
        claimsPanel.SetActive(true);
        claimsPanel.GetComponentInChildren<TMPro.TMP_Text>().text = "The answer to the Riemann Hypothesis is 12";



        tutorialTextBox.text = "";
        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() => EnableClickToContinue());

        for (int i = 0; i < useDetectiveToolsTutorialText.Length; i++)
        {
            tutorialTextBox.text = "";
            yield return TypeWriterEffect(useDetectiveToolsTutorialText[i], 0.03f);
            clickToContinuePanel.SetActive(true);
            bool clicked = false;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => clicked = true);
            if (i < useDetectiveToolsTutorialText.Length - 1)
            {
                yield return new WaitUntil(() => clicked);
            }
            clickToContinuePanel.SetActive(false);
        }

        continueButton.gameObject.SetActive(false);

        //tutorialTextBox.text = "";

        // wait until draggedDummyCard is true
        yield return new WaitUntil(() => draggedDummyCard);
        dummycard.SetActive(false);

        detectiveCluesText.text = "Asking AI: false";

        continueButton.gameObject.SetActive(true);

        for (int i = 0; i < dragClaimToFalsePanelText.Length; i++)
        {
            tutorialTextBox.text = "";
            yield return TypeWriterEffect(dragClaimToFalsePanelText[i], 0.03f);
            clickToContinuePanel.SetActive(true);
            bool clicked = false;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => clicked = true);
            // only wait for click if not the last text
            if (i < dragClaimToFalsePanelText.Length - 1)
            {
                yield return new WaitUntil(() => clicked);
            }
            clickToContinuePanel.SetActive(false);
        }

        continueButton.gameObject.SetActive(false);

        // wait until claims panel is dragged to the false position

        yield return new WaitUntil(() => claimsPanel.GetComponent<claimGenerator>().isTutorialPosHall);

        claimsPanel.SetActive(false);

        tutorialTextBox.text = "";
        continueButton.gameObject.SetActive(true);
        for (int i = 0; i < endOfTutorialText.Length; i++)
        {
            tutorialTextBox.text = "";
            yield return TypeWriterEffect(endOfTutorialText[i], 0.03f);
            clickToContinuePanel.SetActive(true);
            bool clicked = false;
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() => clicked = true);
            yield return new WaitUntil(() => clicked);
            clickToContinuePanel.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);

        // load the main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
        
    }
    private System.Collections.IEnumerator TypeWriterEffect(string text, float delay)
    {
        tutorialTextBox.text = "";
        foreach (char c in text)
        {
            tutorialTextBox.text += c;
            yield return new WaitForSeconds(delay);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void EnableClickToContinue()
    {
        clickToContinuePanel.SetActive(true);
    }
}

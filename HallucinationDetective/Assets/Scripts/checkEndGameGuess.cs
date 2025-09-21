using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UIElements;
public class checkEndGameGuess : MonoBehaviour
{
    public TMPro.TMP_Dropdown suspectDropdown;
    public GameObject correctPanel;
    public GameObject incorrectPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CheckGuess()
    {
        int playerGuess = suspectDropdown.value;

        if (playerGuess == 2)
        {
            Debug.Log("Correct Guess!");
            correctPanel.SetActive(true);
            incorrectPanel.SetActive(false);
            StartCoroutine(LoadSceneAfterDelay(5f));
        }
        else
        {
            Debug.Log("Incorrect Guess!");
            correctPanel.SetActive(false);
            incorrectPanel.SetActive(true);
            StartCoroutine(LoadSceneAfterDelay(5f));
        }
    }

    private System.Collections.IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Menu");
    }

}

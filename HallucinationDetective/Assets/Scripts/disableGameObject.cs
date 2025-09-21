using UnityEngine;

public class disableGameObject : MonoBehaviour
{
    public GameObject gameObjectToDisable;
    public GameObject gameObjectToEnable;

    public GameObject TutorialManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void DisableObject()
    {
        gameObjectToDisable.SetActive(false);
    }
    public void DisableObjectTutorial()
    {
        TutorialManager.GetComponent<TutorialManager>().readInfo = true;
        gameObjectToDisable.SetActive(false);
    }
    public void DisableandEnableObject()
    {
        gameObjectToEnable.SetActive(true);
        gameObjectToDisable.SetActive(false);
    }
}

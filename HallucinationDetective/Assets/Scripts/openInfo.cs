using UnityEngine;

public class openInfo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject infoPanel;
    public int panelNumber;
    public void OpenInfoPanel()
    {
        infoPanel.SetActive(true);
    }
    void Start()
    {
        infoPanel = GameObject.Find("CardInfoPanels").GetComponent<displayInfoPanels>().infoPanels[panelNumber - 1];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

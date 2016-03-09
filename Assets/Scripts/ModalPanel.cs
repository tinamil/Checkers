using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class EventButtonDetails {
    public string buttonTitle;
    public Sprite buttonBackground;
    public UnityAction action;
}

public class ModalPanel : MonoBehaviour {

    public Text question;
    public Image icon;
    public Button button1;
    public Button button2;
    public Button button3;

    public Text button1Text;
    public Text button2Text;
    public Text button3Text;

    public GameObject modalPanelObject;

    private static ModalPanel modalPanel;

    public static ModalPanel Instance() {
        if(!modalPanel) {
            modalPanel = FindObjectOfType(typeof(ModalPanel)) as ModalPanel;
            if(!modalPanel) {
                Debug.LogError("There needs to be one active ModalPanel script on a GameObject in your scene.");
            }
        }

        return modalPanel;
    }

    public void Choice(string title, string question, Sprite iconImage = null, Sprite backgroundImage = null, 
        EventButtonDetails detail1 = null, EventButtonDetails detail2 = null, EventButtonDetails detail3 = null) {

        modalPanelObject.SetActive(true);

        if(backgroundImage != null) {
            modalPanelObject.GetComponent<Image>().sprite = backgroundImage;
        }

        this.icon.gameObject.SetActive(false);
        button1.gameObject.SetActive(false);
        button2.gameObject.SetActive(false);
        button3.gameObject.SetActive(false);

        this.question.text = question;

        if(iconImage != null) {
            this.icon.sprite = iconImage;
            this.icon.gameObject.SetActive(true);
        }
        SetupButton(button1, button1Text, detail1);
        SetupButton(button2, button2Text, detail2);
        SetupButton(button3, button3Text, detail3);
    }

    void SetupButton(Button button, Text buttonText, EventButtonDetails details) {
        button.onClick.RemoveAllListeners();
        if(details != null) {
            if(details.action != null) button.onClick.AddListener(details.action);
            if(details.buttonBackground != null) button.GetComponent<Image>().sprite = details.buttonBackground;
            if(details.buttonTitle != null) buttonText.text = details.buttonTitle;
            button.gameObject.SetActive(true);
        }
    }

    void ClosePanel() {
        modalPanelObject.SetActive(false);
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    public GameObject menu;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetButtonDown("Cancel") && SceneManager.GetActiveScene().buildIndex != 0) {
            ToggleMenu();
        }	
	}

    public void ToggleMenu() {
        menu.SetActive(!menu.activeSelf);
    }

    public void Quit() {
        //TODO Prompt for save
        Application.Quit();
    }

    public void LoadCheckers() {
        SceneManager.LoadScene("Checkers");
    }

    public void LoadMenu() {
        SceneManager.LoadScene("Menu");
    }
}

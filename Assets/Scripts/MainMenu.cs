using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {

    public Camera mainCamera;
    public GameObject menu;
    public Dropdown resolutionDropdown;

	// Use this for initialization
	void Start () {
        Debug.Assert(resolutionDropdown != null, "Resolution dropdown was null, was it assigned in the editor?");
        Debug.Assert(mainCamera != null, "Main camera was null, was it assigned in the editor?");
        Debug.Assert(menu != null, "Main menu was null, was it assigned in the editor?");
        SetResolutionOptions();
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

    public void SetWindowType(int selection) {
        
    }

    public void SetResolutionOptions() {
       
    }

    public void ChangeResolution(int index) {
       
    }

    public void SetSMAAQuality(int index0) {
        Smaa.QualityPreset preset = Smaa.QualityPreset.Low;
        switch(index0) {
            case 0: preset = Smaa.QualityPreset.Low;
                break;
            case 1: preset = Smaa.QualityPreset.Medium;
                break;
            case 2: preset = Smaa.QualityPreset.High;
                break;
            case 3: preset = Smaa.QualityPreset.Ultra;
                break;
            default:
                Debug.Assert(false, "Unknown smaa quality preset value: " + index0);
                break;
        }
        mainCamera.GetComponent<Smaa.SMAA>().Quality = preset;
        PlayerPrefs.SetInt("SMAAQuality", index0);
    }
}

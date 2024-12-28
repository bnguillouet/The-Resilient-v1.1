using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.IO;
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance; // Singleton pour accéder facilement aux paramètres du jeu
    public int gridSize; // Taille de la grille carré
    public int ownedgridSizeX; // Taille en X de la grille possédée
    public int ownedgridSizeY; // Taille en Y de la grille possédée
    public int waterFrequency = 1; // Fréquence d'eau (0 = aucune, 1 = 2%, 2 = 5%, 3 = 10%)
    public int hoverMode = 4; // 0 = Inactive; 1 = Info parcelle; 2 = Construction; 3 = Plantation; 4 = Info plant 5 = Action 10 = Property, 11 = Water, 12 = Electricity; 
    public int viewtileMode = 0; // 0 = Normal; 1 = Niveau argile; 
    public string theme = "Breton"; // Français, Breton, Quebecois, Aquitain, ...
    
    public int latitude = 48;

    public string SoilType = "Bog"; // Bog ou Limestone, Sand ou Clay ou Normal
    public int DifficultyLevel = 99; // 0 = Easy; 1 = Medium; 2 = Hard
    
    /*Attributs du menu principale */
    public Toggle bretonToggle, quebecToggle;
    public Toggle bogToggle, limestoneToggle, normalToggle, sandToggle, clayToggle;    
    public Slider difficultySlider, waterSlider, sizeSlider; 
    public  TextMeshProUGUI difficultyText, waterText, sizeText;
    public Button newGameButton, loadGameButton, saveGameButton, settingsButton, quitButton, newGameReturnButton, launchButton;

    public GameObject menuGameObject, newGameGameObject ;

    private void Awake()
    {
        // Assurez-vous qu'il n'y ait qu'une seule instance de GameSettings
        if (Instance == null)
        {
            Instance = this;
            if (newGameButton == null){newGameButton = GetComponent<Button>();}
            if (newGameButton != null){newGameButton.onClick.AddListener(OnNewGameButtonClick);}            
            if (loadGameButton == null){loadGameButton = GetComponent<Button>();}
            if (loadGameButton != null){loadGameButton.onClick.AddListener(OnLoadButtonClick);}            
            if (saveGameButton == null){saveGameButton = GetComponent<Button>();}
            if (saveGameButton != null){saveGameButton.onClick.AddListener(OnSaveButtonClick);}            
            if (settingsButton == null){settingsButton = GetComponent<Button>();}
            if (settingsButton != null){settingsButton.onClick.AddListener(OnSettingButtonClick);}            
            if (quitButton == null){quitButton = GetComponent<Button>();}
            if (quitButton != null){quitButton.onClick.AddListener(OnQuitButtonClick);}            
            if (newGameReturnButton == null){newGameReturnButton = GetComponent<Button>();}
            if (newGameReturnButton != null){newGameReturnButton.onClick.AddListener(OnReturnButtonClick);}
            if (launchButton == null){launchButton = GetComponent<Button>();}
            if (launchButton != null){launchButton.onClick.AddListener(OnLaunchButtonClick);}
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnNewGameButtonClick()
    {
        menuGameObject.SetActive(false);
        newGameGameObject.SetActive(true);
    }
    public void OnLoadButtonClick()
    {
        Settlement.Instance.LoadSettlement(Application.persistentDataPath + "/settlement_save.json");
    }    
    public void OnSaveButtonClick()
    {
        //string filePath = Path.Combine(, "tribe_save.json");
        Tribe.Instance.SaveTribe(Application.persistentDataPath + "/tribe_save.json");
        Settlement.Instance.SaveSettlement(Application.persistentDataPath + "/settlement_save.json");
    }    
    public void OnSettingButtonClick()
    {
        
    }
    public void OnQuitButtonClick()
    {
        Application.Quit();
    }    
    public void OnReturnButtonClick()
    {
        menuGameObject.SetActive(true);
        newGameGameObject.SetActive(false);
    }

    public void OnLaunchButtonClick()
    {
        if (bretonToggle.isOn){theme = "Breton";}
        if (quebecToggle.isOn){theme = "Quebecois";}
        if (bogToggle.isOn){SoilType = "Bog";}
        if (limestoneToggle.isOn){SoilType = "Limestone";}
        if (normalToggle.isOn){SoilType = "Normal";}
        if (sandToggle.isOn){SoilType = "Sand";}
        if (clayToggle.isOn){SoilType = "Clay";}
        DifficultyLevel = Mathf.RoundToInt(difficultySlider.value); // Arrondi à l'entier le plus proche
        if (DifficultyLevel == 3) {DifficultyLevel = 99;} 
        waterFrequency = Mathf.RoundToInt(waterSlider.value);
        gridSize = Mathf.RoundToInt(sizeSlider.value * 20);
        TileGrid.Instance.LaunchGame();
        newGameGameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // KeyCode.Escape est pour la touche Echap
        {
            if (menuGameObject.activeSelf)
            {
                menuGameObject.SetActive(false);
                newGameGameObject.SetActive(false);
            }
            else if (hoverMode == 9)
            {
                MenuManager.Instance.HideSubMenu();
            }
            else 
            {
                menuGameObject.SetActive(true);
            }
        }
        if (newGameGameObject.activeSelf) //Update Toggle values when the newGameGameObject if active
        {
            int valeurDifficulty = Mathf.RoundToInt(difficultySlider.value); // Arrondit à l'entier le plus proche
            string difficulty = "Hard";
            if (valeurDifficulty == 0) {difficulty = "Easy";}
            else if (valeurDifficulty == 1) {difficulty = "Normal";}
            else if (valeurDifficulty == 3) {difficulty = "God !";}
            difficultyText.text = "Difficulty : "+ difficulty;

            int valeurWater = Mathf.RoundToInt(waterSlider.value); // Arrondit à l'entier le plus proche
            string water = "River";
            if (valeurWater == 0) {water = "None";}
            else if (valeurWater == 1) {water = "Unfrequent";}
            else if (valeurWater == 2) {water = "Normal";}
            else if (valeurWater == 3) {water = "Abundant";}
            waterText.text = "Water Frequency : "+ water;    

            int valeurSize = Mathf.RoundToInt(sizeSlider.value); // Arrondit à l'entier le plus proche
            sizeText.text = "Map size : " + valeurSize * 20 +"x" + valeurSize * 20;
        }
    }

    //TO BE DELETED
    /*    void ToggleValueChanged(Toggle change)
    {
        Debug.Log("Le toggle est maintenant : " + change.isOn);
    }*/
}
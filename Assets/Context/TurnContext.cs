using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using TMPro;

public class TurnContext : MonoBehaviour
{
    public static TurnContext Instance; // Singleton pour accéder facilement aux paramètres du jeu
    // Informations globales du jeu
    public int temperature { get; set; } // De -20 a 45
    public int temperatureMin { get; set; } // De -20 a 20
    public int sunlight { get; set; } //contient de 0 a 14
    public int windIntensity { get; set; } //contient de 0 a 100
    public int rainlevel { get; set; } // De 0 a 500
    public int month { get; set; }
    public int year { get; set; }
    public int money { get; set; }
    public float timer { get; set; }
    public int speed { get; set; }
    public int level { get; set; }
    public Button timerButton;
    public GameObject pauseScreen; 
    public Image filterScreen; 
    public  TextMeshProUGUI timerText;
    public  TextMeshProUGUI dateText, sunnyText, rainText, tempText, minTempText, windText;

    // Méthode pour avancer d'un tour de jeu
    public void NextTurn()
    {
        // Mettez à jour les informations du jeu pour le prochain tour : incrémenter le mois, ajustez la température, etc.
        month++;
        if (month == 13) {month = 1; year ++;} 
        UpdateWeither();
        UpdateFilter();
        timer = 30f; //Timer réinitialiser -TO DO : Dynamique en fonction du mois
        //Tribe tribe = Tribe.Instance;
    }
    void Update()
    {
        timer = timer - Time.deltaTime * speed * speed;
        if (speed >= 1){UpdateChrono();}
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (speed > 0){speed = 0; }
            else {speed = 1; }
            UpdateTimeButton();
        } 

        // Vérifiez si la touche "T" a été enfoncée
        if (Input.GetKeyDown(KeyCode.T) || timer <= 0f) //TO DO : Enlever à terme le T
        {
            NextTurn();
            UpdateChrono();
            speed = 0;
            UpdateTimeButton();
            UpdateTurnContext();
            /*
            foreach (Plant plant in PlantManager.plants)
            {
                plant.ChangeMonth();
            }*/
            for (int x = 0; x < GameSettings.Instance.ownedgridSizeX; x++)
            {
                for (int y = 0; y < GameSettings.Instance.ownedgridSizeY; y++) 
                {
                    TileGrid.Instance.tiles[x,y].NextMonth();
                }
            }
            /*Settlement.Instance.NextMonth();
            */ //TO DO : Remettre les appels de méthode ChangeMonth pour les plants, Tile et Settlement.
            //PlantManager.ListPlants();
            Debug.LogError($"Update ----- Nous voici en {month}/{year}");
        }
    }

    public void ForcePause()
    {
        speed = 0;
        UpdateTimeButton();
    }

    public void UpdateChrono()
    {
        timerText.text = (((int)timer)/60) + ":" + (((int)timer)%60).ToString("D2");
    }

    private void Awake()
    {
        // Assurez-vous qu'il n'y ait qu'une seule instance de GameSettings
        if (Instance == null)
        {
            Instance = this;
            NewGame();
            if (timerButton == null){timerButton = GetComponent<Button>();}
            if (timerButton != null){timerButton.onClick.AddListener(OnTimerButtonClick);}
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NewGame()
    {
        year = 2040;
        month = 3;
        money = 100000;
        timer = 30f;
        speed = 1;
        level = 1;
        UpdateWeither();
        UpdateFilter();
        UpdateTurnContext();
    }

    public void UpdateFilter()
    {
        if (sunlight > 8){filterScreen.color = new Color(0f, 0f, 0f, 0f);}
        else 
        {
            filterScreen.color = new Color(0f, 0f, 0f, 0.8f - (float)sunlight/10);
        }
    }

    public void OnTimerButtonClick() //rouge TileInfo
    {
        speed = (speed + 1)%3; 
        UpdateTimeButton();
    }

    public void UpdateTimeButton() //rouge TileInfo
    {
        RawImage rawImage = timerButton.transform.Find("TimerImage").GetComponent<RawImage>();
        if (rawImage != null)
        {
            Texture2D texture = Resources.Load<Texture2D>("Menu/timer_"+ speed);
            rawImage.texture = texture; 
        }
        if (speed == 0)
        {
            pauseScreen.SetActive(true);
        }
        else {pauseScreen.SetActive(false);}
        
    }

    public void UpdateWeither() // TO DO : Mettre a jour des temperature random suivant un climat + changement climatique
    {
        if (month == 1) {temperature = 3; sunlight = 3;windIntensity = 60; rainlevel = 400;}
        else if (month == 2) {temperature = 5; sunlight = 4; windIntensity = 50; rainlevel = 200;}
        else if (month == 3) {temperature = 10; sunlight = 5;windIntensity = 70; rainlevel = 250;}
        else if (month == 4) {temperature = 14; sunlight = 6;windIntensity = 30; rainlevel = 150;}
        else if (month == 5) {temperature = 19; sunlight = 7;windIntensity = 20; rainlevel = 100;}
        else if (month == 6) {temperature = 24; sunlight = 8;windIntensity = 30; rainlevel = 70;}
        else if (month == 7) {temperature = 28; sunlight = 9;windIntensity = 10; rainlevel = 50;}
        else if (month == 8) {temperature = 27; sunlight = 8;windIntensity = 0; rainlevel = 100;}
        else if (month == 9) {temperature = 19; sunlight = 6;windIntensity = 40; rainlevel = 150;}
        else if (month == 10) {temperature = 10; sunlight = 4;windIntensity = 70; rainlevel = 350;}
        else if (month == 11) {temperature = 7; sunlight = 3; windIntensity = 90; rainlevel = 500;}
        else if (month == 12) {temperature = 5; sunlight = 2; windIntensity = 70; rainlevel = 350;}
    }

    public void UpdateTurnContext()
    {
        dateText.text = NumberToMonth(month) + "<br>" + year;
        sunnyText.text = sunlight + " h/jr";
        rainText.text = rainlevel + "mm";
        tempText.text = "Moy: " +temperature + "°C";
        minTempText.text = "Min: " +temperatureMin + "°C";
        windText.text = windIntensity + "km/h";
    }

    public string NumberToMonth(int index)
    {
        if (index == 1){ return "Janvier";}
        else if (index == 2){ return "Février";}
        else if (index == 3){ return "Mars";}
        else if (index == 4){ return "Avril";}
        else if (index == 5){ return "Mai";}
        else if (index == 6){ return "Juin";}
        else if (index == 7){ return "Juillet";}
        else if (index == 8){ return "Août";}
        else if (index == 9){ return "Septembre";}
        else if (index == 10){ return "Octobre";}
        else if (index == 11){ return "Novembre";}
        else if (index == 12){ return "Décembre";}
        else {return "Inconnu";}
    }
}
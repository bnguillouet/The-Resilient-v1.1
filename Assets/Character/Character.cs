using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum Gender
{
    Male,
    Female,
    Other
}
/* public enum AgeGroup{ Baby, Child, Adult, Senior} */
public enum Status
{
    Normal,
    IsPregnant, //Pas implementé
    Feeding,
    Sick,
    Cold,
    Happy,
    Escaped,
    Dead
}

public class Character : MonoBehaviour
{
    public int Id { get; private set; }
    public string FirstName { get; set; }
    public Gender Gender { get; set; }
    public Status CharacterStatus { get; set; }
    public DateTime DateOfBirth { get; private set; }
    public GameObject characterObject;
    public CharacterToken characterToken;

    /**********************************************/
    /********** CREATION DU PERSONNAGE ************/
    /**********************************************/
    public Character(string firstName, int idImage, Gender gender, DateTime dateOfBirth)
    {
        //INITIALISATION DES INFORMATION DU PERSONNAGE
        Id = UnityEngine.Random.Range(10000, 99999);
        FirstName = firstName;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        CharacterStatus = Status.Normal;

        //INITIALISATION DU TOKEN DU PERSONNAGE
        characterObject = new GameObject("CharacterToken_"+firstName);
        if (characterObject != null && FirstName == "Bees_1")
        {
            characterObject.transform.localScale = new Vector3(3, 3, 1f);
        }
        characterToken = characterObject.AddComponent<CharacterToken>();
        characterToken.Initialize (this, idImage);
    }

    /**************************************************/
    /********** FIN CREATION DU PERSONNAGE ************/
    /**************************************************/

    /*************************************************************/
    /********** FONCTIONS D'INFORMATION DU PERSONNAGE ************/
    /*************************************************************/
    public int CalculateAge()
    {
        int age = TurnContext.Instance.year - DateOfBirth.Year;
        if (TurnContext.Instance.month < DateOfBirth.Month)
        {
            age -= 1;
        }
        return age;
    }

    /*****************************************************************/
    /********** FIN FONCTIONS D'INFORMATION DU PERSONNAGE ************/
    /*****************************************************************/

    public void LoseHealth(int healthPoints)
    {
        // Implement health loss logic here
    }

    public void GetSick()
    {
        CharacterStatus = Status.Sick;
    }

    public virtual bool IsAvailable()
    {
        return true; // TO DO : VOIR SI OCCUPE OU AUTRE
    }

    public virtual void NextMonth()
    {

    }

    public void Die()
    {
        CharacterStatus = Status.Dead;
        characterToken.actionsExecution = null;
        Destroy(characterToken.characterImage.transform.parent.gameObject);
    }
}

//**************************************************************************************************//
//                                           HUMAIN                                                 //
//**************************************************************************************************//

public class Human : Character
{
    public List<string> skills { get; set; }
    public House house { get; set; }
    public GameObject characterButton;
    public GameObject characterButtonText;
    private RectTransform rectTransform;
    public Button activeCharacterButton;
    public Human(string firstName, int idImage, Gender gender, DateTime dateOfBirth) : base(firstName, idImage, gender, dateOfBirth)
    {
        base.characterToken.SetTargetPosition(new Vector3(1, 0, 3));
        base.characterToken.SetTargetPosition(new Vector3(20, 3, 25));

        skills = new List<string>();
        // INITIALISATION DU TEXTE CONTENU DANS LE BOUTON DU PERSONNAGE
        characterButtonText = new GameObject("CharacterButtonText_"+firstName); // Créez un nouveau GameObject pour le texte
        TextMeshProUGUI textComponent = characterButtonText.AddComponent<TextMeshProUGUI>();
        RectTransform rectTransformText = characterButtonText.GetComponent<RectTransform>();
        rectTransformText.anchorMin = new Vector2(1, 0);
        rectTransformText.anchorMax = new Vector2(1, 0);
        UpdateButtonStatus(Color.green);

        //INITIALISATION DU BOUTON DU PERSONNAGE
        string type = null;
        if (Gender == Gender.Female) {type = "woman";}
        else if (Gender == Gender.Male) {type = "man";}
        characterButton = new GameObject("CharacterButton_"+firstName);
        Image iconImage = characterButton.AddComponent<Image>();
        Texture2D texture = Resources.Load<Texture2D>("Tribe/Button/"+ type +"_"+ idImage);

        if (texture == null)
        {
            Debug.LogError("Image not found at path: Tribe/Button/"+ type +"_"+ idImage);
            return;
        }
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 420, 600), new Vector2(1f, 1f), 25);
        characterButton.GetComponent<Image>().sprite = sprite;
        characterButton.transform.localScale = new Vector3 (1f,1.5f,1f);
        activeCharacterButton = characterButton.AddComponent<Button>();
        rectTransform = characterButton.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
    }

    private void Update()
    {
        // Gérez le clic gauche de la souris sur les images
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                Debug.Log("Cliqué sur le bouton du personnage");
            }
        }
    }

    public void InstallInHouse(House houseInstall)
    {
        house = houseInstall;
    }

    public void UpdateButtonStatus(Color colorinput)
    {
        TextMeshProUGUI textComponent = characterButtonText.GetComponent<TextMeshProUGUI>();
        textComponent.fontSize = 14;
        textComponent.color = colorinput;
        textComponent.fontWeight = FontWeight.Bold;      
        textComponent.text = FirstName + "<br>Age: " + base.CalculateAge();
    }

    public override void NextMonth()
    {
        
    }

}


//**************************************************************************************************//
//                                         ANIMAUX                                                  //
//**************************************************************************************************//

public class Animal : Character
{
    public Animal(string firstName, int idImage, Gender gender, DateTime dateOfBirth) : base(firstName, idImage, gender, dateOfBirth)
    {
    }

    public virtual void Save() // Un animal peut se sauver
    {
        CharacterStatus = Status.Escaped;
        base.characterToken = null;
        base.characterObject = null;
    }
}

public class Chicken : Animal
{
    public ChickenCoop chickenCoop;
    public Chicken(string firstName, int idImage, Gender gender, DateTime dateOfBirth) : base(firstName, idImage, gender, dateOfBirth)
    {
        int random = UnityEngine.Random.Range(1, 4); //TO DO ; Changer pour où pop les animaux
        int random2 = UnityEngine.Random.Range(1, 4);
        base.characterToken.SetTargetPosition(new Vector3(random, random2, 2));
    }

    public void Eat()
    {
        // Implement eating logic here
    }

    public void Pluck()
    {
        CharacterStatus = Status.Dead; // Change status to "Dead" when plucked
    }

    public void Fertilize()
    {
        // Implement fertilization logic here
    }
    public void InstallInChickenCoop(ChickenCoop chickenCoopInstall)
    {
        chickenCoop = chickenCoopInstall;
        Vector3 position = new Vector3 (chickenCoop.position.x, chickenCoop.position.y, 2);
        base.characterToken.SetTargetPosition(position);
    }
    
}

public class SwarmBees : Animal
{
    public int adaptatability; // Valeur de 1 à 10; 10 Très rustique, 1 adapté aux climats chauds
    public int size; // Valeur de 1 à 10;
    public int stressLevel; // lié a pollution, condition climatique, déplacement, une intervention humaine,...
    public int condition; // Froid et humide en hiver; ... a voir
    public BeeHive beehive;
    public SwarmBees(string firstName, int idImage, DateTime dateOfBirth, int adaptabilityLevel, int sizeSwarm) : base(firstName, idImage, Gender.Other , dateOfBirth)
    {
        base.characterToken.SetTargetPosition(new Vector3(1, 1, 2));
        adaptatability = adaptabilityLevel;
        size = sizeSwarm;
        beehive = null;
        stressLevel = 5;
    }

    public void InstallInBeeHive(BeeHive beehiveInstall)
    {
        beehive = beehiveInstall;
        Vector3 position = new Vector3 (beehive.position.x, beehive.position.y, 2);
        base.characterToken.SetTargetPosition(position);
    }  

    public void Manipulated(int intensity)
    {
        stressLevel = stressLevel + intensity;
    }

    public override bool IsAvailable()
    {
        if (beehive == null) {return true;}
        else {return false;}
    }

    public override void NextMonth()
    {
        if (stressLevel > 5)
        {
            size = size -1;
        }
        if (size == 0)
        {
            CharacterStatus = Status.Dead;
            base.characterToken = null;
            base.characterObject = null;
            beehive.SwarmLeave();
        }
        if (stressLevel > 9){Save();}        
        // Mise a niveau du stress suivant conditions climatiques
        // Mise a niveau des quantités de cire, gelée royal, propolis, miel dans la ruche
        // Consomme en hiver de l'eau sucrée ou du miel (en priorité)
        // La quantité de gelée royal et propolis reste au maximum si pas touché
        // Produit suivant la quantité de fleur et arbre "fleuris" et contenant quelque chose de profitable pour les abeilles
    }
    public override void Save()
    {
        CharacterStatus = Status.Escaped;
        base.characterToken = null;
        base.characterObject = null;
        beehive.SwarmLeave();
    }
}
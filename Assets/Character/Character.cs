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
    Unhappy, //Plusieurs choses ne vont pas
    Escaped,
    Dead
}

public class Character //: MonoBehaviour
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public Gender Gender { get; set; }
    public Status CharacterStatus { get; set; }
    public DateTime DateOfBirth { get; set; }

    public int feedLevel { get; set; } //Niveau de satiété alimentaire entre 0 (mort de faim) et 100 (nourri). En fonction de l'apport en nourriture.

    public int happinessLevel { get; set; } //Niveau de bonheur entre 0 (suicidaire) et 100 (heureux). En fonction des attentes de chacun + besoins concret, sociable,...

    public int energyLevel { get; set; } //Niveau d'energie entre 0 (mort de fatigue) et 100 (en pleine forme). Dépend si malade, blessé, ou pas assez mangé. //TO DO : Energie en fonction de l'activité

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

        //INITIALISATION DES CARACTERISTIQUES
        feedLevel = 90;
        happinessLevel = 90;
        energyLevel = 100;
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

    public bool Alive()
    {
        if (CharacterStatus == Status.Dead || CharacterStatus == Status.Escaped)
        {
            return false;
        }
        else {return true;}
    }
    public virtual bool IsAvailable()
    {
        return true; // TO DO : VOIR SI OCCUPE OU AUTRE
    }

    public virtual void NextMonth(){}

    public virtual void NextMonth(float slicedOfNeed, int bonus, int variety){}

    public void Die()
    {
        
        CharacterStatus = Status.Dead;
        if (characterToken != null)
        {
        characterToken.actionsExecution = null;
        characterToken.DestroyToken();
        }
    }

    public virtual void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo(FirstName + "<br>Date de naissance : "+ DateOfBirth.ToShortDateString() + "<br>Age : " + CalculateAge() + "<br>Statut : " + CharacterStatus + "<br>Bonheur : " + happinessLevel+ "/100<br>Satiété : " + feedLevel + "/100<br> Energie : " + energyLevel + "/100");
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
    
    public string characteristic { get; set; }
    public string role { get; set; }
    //communityInvolvement + moveModifier + feedNeed + chillyTolerence

    
    public Human(string firstName, int idImage, Gender gender, DateTime dateOfBirth) : base(firstName, idImage, gender, dateOfBirth)
    {
        base.characterToken.SetTargetPosition(new Vector2Int(-1, 0));

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

        //INITIALISATION DES CARACTERISTIQUES
        int roleIndex = UnityEngine.Random.Range(0, roleList.Count);
        role = roleList[roleIndex];
        int characteristicIndex = UnityEngine.Random.Range(0, characteristicsList.Count);
        characteristic = characteristicsList[characteristicIndex];
    }

    public string GetSkillInfo()
    {
        string skillinfo = "<br>Compétences : ";
        int compter = 0;
        foreach (string skill in skills)
        {
            if (compter == 0)
            {
                skillinfo += skill;
            }
            else if (compter % 3 == 0)
            {
                skillinfo += "<br>" + skill; 
            }
            else
            {
                skillinfo += ", " + skill;
            }
            compter++;
        }
        return skillinfo;
    }

    public override void UpdateInfo()
    {
        string houseInfo = "Dort à la belle étoile !";
        if (house != null)
        {
            houseInfo = house.buildingName;
        }
        InformationManager.Instance.AddHoverInfo("Homo Sapiens - " + FirstName + "<br>Date de naissance : "+ DateOfBirth.ToShortDateString() + "<br>Age : " + CalculateAge() + "<br>Statut : " + CharacterStatus + "<br>Bonheur : " + happinessLevel+ "/100<br>Satiété : " + feedLevel + "/100<br> Energie : " + energyLevel + "/100" + GetSkillInfo() + "<br>Caractéristique : " + characteristic+ "<br>Rôle : " + role + "<br> Habitat : " +  houseInfo);
    }



    //**************************************************************************************************//
    //                                    GESTION DES SKILLS                                            //
    //**************************************************************************************************//
    public void AddSkill(string skillName, bool eventMessage)
    {
        if (SkillsManager.Instance.CanAddSkill(skillName, skills))
        {
            skills.Add(skillName);
        }
    }

    public bool SkillLearned (string skillName)
    {
        if (skills.Contains(skillName))
        { 
            return true;
        }
        else 
        {
            return false;
        }
    }

    //**************************************************************************************************//
    //                                    GESTION DU PERSONNAGE                                         //
    //**************************************************************************************************//
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

    public override bool IsAvailable()
    {
        if (house == null) {return true;}
        else {return false;}
    }

    public void InstallInHouse(House houseInstall)
    {
        if (houseInstall == null)
        {
            Debug.LogError("House is null ! Installation failure.");
            return;
        }
        bool installed = houseInstall.PutHuman(FirstName);
        if (installed)
        {
            if (house != null) {house.RemoveHuman();}
            house = houseInstall;
            Debug.Log(FirstName + " is now installed in the House.");
            EventPile.Instance.AddEvent(FirstName + " est maintenant installé dans sa maison.", "Pending",  2, house.position);
        }
        else
        {
            EventPile.Instance.AddEvent(FirstName + " n'a pas pu être installé dans sa nouvelle maison.", "Pending", 1, new Vector2Int(-1,-1));
        }
    }

    public void UpdateButtonStatus(Color colorinput)
    {
        TextMeshProUGUI textComponent = characterButtonText.GetComponent<TextMeshProUGUI>();
        textComponent.fontSize = 14;
        textComponent.color = colorinput;
        textComponent.fontWeight = FontWeight.Bold;      
        textComponent.text = FirstName + "<br>Age: " + base.CalculateAge();
    }

    public override void NextMonth(float slicedOfNeed, int bonus, int variety)
    {
        //EventPile.Instance.AddEvent("Tout va bien", "Happy",  0);
        
        //return "Ok";
    }

    private static List<string> roleList = new List<string>
    {"Visionnaire","Communicateur","Médiateur","Stratège","Soignant","Innovateur","Gardien de la sagesse","Motivé","Réaliste","Optimiste","Connecteur","Protecteur"}; //"Educateur", "Gardien de la nature"
    private static List<string> characteristicsList = new List<string>
    {"Végétarien","Technocrate","Blagueur réveur","Hyperactif","Solitaire","Enfant à papa","Amoureux des arbres","Baroudeur","Militant","Minimaliste","Bon vivant"}; //"Végétalien"

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

    public void MoveToEnclosure()
    {
        if(Tribe.Instance.EnclosureAvailable())
        {
            base.characterToken.ToPosition(Tribe.Instance.EnclosureAvailableSpace()); //SetTargetPosition ?
        }
    }
}

public class Chicken : Animal
{
    public ChickenCoop chickenCoop;
    public int foodConsommation;
    public int waterConsommation;

    public int eggToLay;
    public Chicken(string firstName, int idImage, Gender gender, DateTime dateOfBirth) : base(firstName, idImage, gender, dateOfBirth)
    {
        base.characterToken.ToPosition(Tribe.Instance.EnclosureAvailableSpace());
        foodConsommation = 3;
        waterConsommation = 1;
        eggToLay = 10;
        feedLevel = 51; //Dynamic a la création d'une poule ?
    }

    public void Eat(bool hungry)
    {
        if (waterConsommation == 0){Die();}
        else if (waterConsommation == 1)
        {
            GetSick();
            happinessLevel -= 10;
            energyLevel -= 8;
        }
        else if (waterConsommation == 2)
        {
            happinessLevel -= 2;
            energyLevel -= 4;
        }   
        if (foodConsommation < 2 || (foodConsommation < 3 && hungry)){Die();}
        else if (foodConsommation < 3 || (foodConsommation < 4 && hungry))
        {
            GetSick();
            happinessLevel -= 10;
            energyLevel -= 10;
            feedLevel -= 1;
        }    
        else if (foodConsommation < 4 || (foodConsommation < 5 && hungry))
        {
            GetSick(); //TO DO :  1 chance sur 3
            happinessLevel -= 5;
            energyLevel -= 5;
            feedLevel += 0;
        }  
        else
        {
            feedLevel += 1;
        }         
        if (feedLevel > 100){feedLevel = 100;} 
    }

    public override void NextMonth()
    {
        Debug.LogError("Passe par la fin de mois de poule");
        happinessLevel = (happinessLevel + 100)/2;
        energyLevel = (energyLevel + 100)/2;
        if(CharacterStatus == Status.Sick)
        {
            energyLevel -= 10;
        }
        bool hungry = false;
        if (feedLevel < 50){hungry = true;}
        Eat(hungry);
        if (!IsAvailable())
        {
            if (chickenCoop.SpaceAvailable() > 2){happinessLevel -= 3;}
        }
        if (TurnContext.Instance.temperatureMin < 0){happinessLevel -= 3;} //A changer si chauffage, si dans une serre, et en fonction du type de poule...
        eggToLay = (energyLevel/20) + (happinessLevel/20);
        int random = UnityEngine.Random.Range(0, 50);
        if (CalculateAge() >= 9)
        {
            if (random < 8){Die();}
            else
            {
                if (random < 12){GetSick();}
                eggToLay = 1;
            }
        }
        else if (CalculateAge() >= 4)
        {
            if (random < 2){GetSick();}
            eggToLay = eggToLay/2;
        }
        foodConsommation = 0;
        waterConsommation = 0;
    }

    public int Pluck()
    {
        int meetQuantity = feedLevel/5 + 1;
        EventPile.Instance.AddEvent("Une poule a été plumée et a permet d'obtenir " + meetQuantity + " volailles" , "Chicken",  0, chickenCoop.position);
        chickenCoop.RemoveChicken(this);
        Die();
        return meetQuantity;
    }

    public void Lay()
    {
        if (eggToLay > 0)
        {
            eggToLay --;
            chickenCoop.eggNumber ++;
        }
    }

    public void Fertilize()
    {
        //base.characterToken.
    }
    public void InstallInChickenCoop(ChickenCoop chickenCoopInstall)
    {
        if (chickenCoopInstall == null)
        {
            Debug.LogError("Chickencoop is null ! Installation failure.");
            return;
        }
        chickenCoop = chickenCoopInstall;
        Debug.Log("Chicken "+ FirstName + " is now associated with the chickencoop.");
        base.characterToken.SetTargetPosition(chickenCoop.nearBuilding()); 
        EventPile.Instance.AddEvent("La poule " + FirstName + " a été installée dans un poulailler.", "Chicken",  0, chickenCoop.position);
    }
    public override bool IsAvailable()
    {
        if (chickenCoop == null) {return true;}
        else {return false;}
    }

    public override void UpdateInfo()
    {
        string houseInfo = "Enclos";
        if (chickenCoop != null)
        {
            houseInfo = chickenCoop.buildingName;
        }
        InformationManager.Instance.AddHoverInfo("Gallus gallus domesticus - " + FirstName + "<br>Date de naissance : "+ DateOfBirth.ToShortDateString() + "<br>Age : " + CalculateAge() + "<br>Statut : " + CharacterStatus + "<br>Bonheur : " + happinessLevel+ "/100<br>Satiété : " + feedLevel + "/100<br> Energie : " + energyLevel + "/100<br> Habitat : " +  houseInfo + "<br>Bouffe ceréales" + foodConsommation + "<br>Water" + waterConsommation);
    }
}

public class Duck : Animal
{
    //public DuckCoop duckCoop;
    public Duck(string firstName, int idImage, Gender gender, DateTime dateOfBirth) : base(firstName, idImage, gender, dateOfBirth)
    {
        base.characterToken.ToPosition(Tribe.Instance.EnclosureAvailableSpace());
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
        //base.characterToken.
    }
    /*public void InstallInDuckCoop(DuckCoop duckCoopInstall)
    {
        if (duckCoopInstall == null)
        {
            Debug.LogError("duckCoop is null ! Installation failure.");
            return;
        }
        duckCoop = duckCoopInstall;
        Debug.Log("Duck "+ firstName + " is now associated with the duckcoop.");
        base.characterToken.SetTargetPosition(duckCoop.nearBuilding()); 
    }*/
    public override bool IsAvailable()
    {
        /*if (duckCoop == null) {return true;}
        else {return false;}*/
        return true;
    }

    
    public override void UpdateInfo()
    {
        InformationManager.Instance.AddHoverInfo("Anas platyrhynchos - " + FirstName + "<br>Date de naissance : "+ DateOfBirth.ToShortDateString() + "<br>Age : " + CalculateAge() + "<br>Statut : " + CharacterStatus + "<br>Bonheur : " + happinessLevel+ "/100<br>Satiété : " + feedLevel + "/100<br> Energie : " + energyLevel + "/100" /*+ "<br> Habitat : " +  houseInfo */);
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
        /*base.characterToken.SetTargetPosition(new Vector2Int(1, 1));*/
        base.characterToken.ToPosition(Tribe.Instance.EnclosureAvailableSpace());
        adaptatability = adaptabilityLevel;
        size = sizeSwarm;
        beehive = null;
        stressLevel = 5;
    }

    public void InstallInBeeHive(BeeHive beehiveInstall)
    {
        if (beehiveInstall == null)
        {
            Debug.LogError("Beehive is null ! Installation failure.");
            return;
        }
        beehive = beehiveInstall;
        Debug.Log("Swarmbees "+Id + " is now associated with the beehive.");
        EventPile.Instance.AddEvent("Une essaim a été installé dans une ruche", "Bees",  0, beehive.position);
        base.characterToken.SetTargetPosition(beehive.position);
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
        if (size == 0)
        {
            CharacterStatus = Status.Dead;
            base.characterToken = null;
            base.characterObject = null;
            beehive.SwarmLeave();
            //return "La reine de la ruche est morte. La collonie disparait."; 
            EventPile.Instance.AddEvent("La reine de la ruche est morte. La colonie disparait.", "Bees",  0, beehive.position);
        }
        if (stressLevel > 9)
        {
            EventPile.Instance.AddEvent("L'esseim a été trop stressé et a choisi d'emmigrer.", "Bees",  0, beehive.position);
            Save();
            //return "L'esseim a été trop stressé et a choisi d'emmigrer.";    
        }   
        if (stressLevel > 5)
        {   
            size = size -1;
            //return "L'esseim a été stressé et a perdu 1 taille.";   
            EventPile.Instance.AddEvent("L'esseim a été stressé et a perdu 1 taille : "+ size, "Bees",  0, beehive.position);
        }
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
    
    public override void UpdateInfo()
    {
        string houseInfo = "Sans ruche";
        if (beehive != null)
        {
            houseInfo = beehive.buildingName;
        }
        InformationManager.Instance.AddHoverInfo("Apis mellifera - " + FirstName + "<br>Date de naissance : "+ DateOfBirth.ToShortDateString() + "<br>Age : " + CalculateAge() + "<br>Statut : " + CharacterStatus + "<br>Bonheur : " + happinessLevel+ "/100<br>Satiété : " + feedLevel + "/100<br> Energie : " + energyLevel + "/100" /*+ "<br> Habitat : " +  houseInfo */);
    }
}
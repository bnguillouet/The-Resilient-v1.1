using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;

public class Tribe : MonoBehaviour
{
    // Liste des personnages de la tribu
    public static List<Character> members { get; private set; } //Liste des Character de la tribu (Animaux et Humains)
    public static Human activeMember { get; private set; } // L'humain selectionné
    public GameObject charactereMenu;
    public Button addCharacterButton;
    public int humanNumber { get; set; } //Pour information

    /****************************************************/
    /********** INITIALISATION DE LA TRIBU **************/
    /****************************************************/
    public static Tribe Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTribe();
            if (addCharacterButton != null){addCharacterButton.onClick.AddListener(AddMember);}
        }
        else
        {
            Debug.LogWarning("Il ne peut y avoir qu'une seule instance de Tribu.");
            Destroy(gameObject);
        }
    }
    
    public void InitializeTribe()
    {
        members = new List<Character>(); members.Clear();
        DateTime currentdate = new DateTime(TurnContext.Instance.year, TurnContext.Instance.month, 1);
        Character man = CreateRandomCharacter(Gender.Male, "Human");
        //man.CharacterStatus = Status.Cold;
        OnCharacterClick(man);
        Character woman = CreateRandomCharacter(Gender.Female, "Human");
        members.Add(man); 
        members.Add(woman);
        activeMember = (Human)members[0];
        humanNumber = 2;
        UpdateCharacterMenu();

        //-----------TEMPORAIRE POUR CREER DES ANIMAUX --------------
        // Créez les animaux avec les valeurs aléatoires
        SwarmBees bees = new SwarmBees("Bees_1", 1, currentdate, 5, 3);
        members.Add(bees);
        Chicken chicken = new Chicken("Chicken_1",  1, Gender.Female, currentdate);
        members.Add(chicken);
        Chicken chicken2 = new Chicken("Chicken_2",  2, Gender.Female, currentdate);
        members.Add(chicken2);
        Chicken chicken3 = new Chicken("Chicken_3",  3, Gender.Female, currentdate);
        members.Add(chicken3);
        Chicken chicken4 = new Chicken("Chicken_4",  4, Gender.Female, currentdate);
        members.Add(chicken4);
        //----------- A SUPPRIMER -------------------    
    }

    public void ReinitializeTribe()
    {
        foreach (UnityEngine.Transform child in this.gameObject.transform){GameObject.Destroy(child.gameObject);} 
        foreach(Character member in members)
        {
            member.Die();
        }
        InitializeTribe();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /********************************************************/
    /********** FIN INITIALISATION DE LA TRIBU **************/
    /********************************************************/

    /*************************************************************/
    /********** FONCTIONS D'INFORMATION DE LA TRIBU **************/
    /*************************************************************/
    public int Need(string need)
    {
        if (need=="Bois chauffage"){return 100;} // TO DO ; Changer suivant la composition de la tribu
        else if (need=="Eau potable"){return 10;}
        else if (need=="Fruit"){return 10;}
        else if (need=="Légume"){return 10;}
        else if (need=="Céréale"){return 10;}
        else if (need=="Protéine"){return 10;}
        else return 0;
    }

    public List<Character> GetMembers()
    {
        return members;
    }

    public static void ListMembers() // Méthode pour afficher la liste des membres avec leurs informations dans la console
    {
        Debug.Log("Liste des membres de la tribu :");
        foreach (Character member in members)
        {
            Debug.Log($"Nom : {member.FirstName}, Sexe : {member.Gender}, Age : {member.CalculateAge()}, Statut : {member.CharacterStatus}");
        }
    }

    /*****************************************************************/
    /********** FIN FONCTIONS D'INFORMATION DE LA TRIBU **************/
    /*****************************************************************/

    /************************************************************/
    /********** FONCTIONS DE CHANGEMENT SUR LA TRIBU ************/
    /************************************************************/
    private static Character CreateRandomCharacter(Gender gender, string type)
    {
        // Nouveau personnage a entre 20 et 35 ans
        int randomYearsold = UnityEngine.Random.Range(20, 36);
        // Générez un nom aléatoire pour le personnage
        List<string> names = (gender == Gender.Male) ? maleNames : femaleNames;
        int randomNumber = UnityEngine.Random.Range(0, names.Count);
        string randomName = names[randomNumber];

        // Calculez la date de naissance
        int randomMonth = UnityEngine.Random.Range(1, 13);
        int daysInMonth = DateTime.DaysInMonth(TurnContext.Instance.year-randomYearsold, randomMonth);
        int randomDay = UnityEngine.Random.Range(1, daysInMonth+1);
        DateTime birthdate = new DateTime(TurnContext.Instance.year-randomYearsold, randomMonth, randomDay);
        
        int randomid = 0;
        bool available = false;
        while (!available)
        {
            available = true;
            randomid = UnityEngine.Random.Range(1, 8);
            if (randomid == 7){randomid = 15;}
            foreach (Character member in members)
            {   
                Debug.LogError("Random : "+ randomid + " test : " +member.characterToken.idImage + " gender " + gender + "gender test" + member.Gender);
                if (member is Human && randomid == member.characterToken.idImage && gender == member.Gender)
                {
                    available = false;
                }
            }
        }
            int random_x = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize-11);
            int random_y = UnityEngine.Random.Range(0, GameSettings.Instance.gridSize-7);
            bool canbuild = true;


        // Créez le personnage avec les valeurs aléatoires
        Human character = new Human(randomName, randomid, gender, birthdate);

        return character;
    }


    private void AddMember() 
    {
        if (humanNumber <= 20)
        {
            int randomNumber = UnityEngine.Random.Range(0, 2);
            Character newCharacter = null;
            if (randomNumber == 0) {newCharacter = CreateRandomCharacter(Gender.Male, "Human");}
            else {newCharacter = CreateRandomCharacter(Gender.Female, "Human");}
            members.Add(newCharacter);
            humanNumber++;
            UpdateCharacterMenu();
        }
        else {Debug.LogWarning("Vous avez atteint le maximum dans votre tribu...");}
    }
    
    void Update() // Fonction pour changer de membre de tribu
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool found = false;
            int currentIndex = members.IndexOf(activeMember);
            for (int i = currentIndex + 1; i < members.Count; i++)
            {
                if (members[i] is Human nextHuman)
                {
                    OnCharacterClick(nextHuman);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                // Si aucun autre Human n'a été trouvé après l'activeMember, recherche à partir du début de la liste
                for (int i = 0; i <= currentIndex; i++)
                {
                    if (members[i] is Human nextHuman)
                    {
                        OnCharacterClick(nextHuman);
                        break;
                    }
                }
            }
        }
    }

    public void  UpdateCharacterMenu()
    {
        //foreach (UnityEngine.Transform child in this.gameObject.transform){GameObject.Destroy(child.gameObject);} 
        //int humanCount = members.Count(member => member is Human); // TO DO : Faire un menu déroulant
        int humanCount = 0;
        foreach (Character member in members)
        {   
            if (member is Human humanMember)
            {
                humanMember.characterButtonText.transform.SetParent(this.gameObject.transform, false);
                humanMember.characterButtonText.transform.SetSiblingIndex(0);
                humanMember.characterButton.transform.SetParent(this.gameObject.transform, false);
                humanMember.characterButton.transform.SetSiblingIndex(0);
                humanMember.characterButton.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(-35 - (humanCount * 65), -75, 0);
                humanMember.characterButtonText.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(15 - (humanCount * 65), 10, 0);

                humanCount++;

                // Capturer la variable locale pour éviter les problèmes de portée dans les expressions lambda
                Human capturedHumanMember = humanMember;
                capturedHumanMember.activeCharacterButton.onClick.AddListener(() => OnCharacterClick(capturedHumanMember));
            }
        }        
    }

    public void OnCharacterClick(Character member) //Selection d'un Personnage
    {
        if (member is Human)
        {
            /*ActionContext.Instance.HideActionMenu();*/ //TO DO : REMETTRE
            GameSettings.Instance.hoverMode = 5; 
            activeMember = (Human)member;
            //CameraController.PlaceOnPosition(member.characterToken.transform.position); //TO DO : Voir pour aller au Token si on clique sur un personnage
            /*ActionPile.Instance.Refresh();*/ //TO DO : REMETTRE
            if (charactereMenu != null)
            {
                WhitedCharacterIcons();  
                string characterButtonName = "CharacterButton_" + member.FirstName;
                UnityEngine.Transform characterTransform = charactereMenu.transform.Find(characterButtonName);
                if (characterTransform != null)
                {
                    GameObject characterObject = characterTransform.gameObject;
                    Image characterImage = characterObject.GetComponent<Image>();
                    if (characterImage != null)
                    { 
                        characterImage.color = new Color32(0x52, 0xFF, 0x83, 0xFF); // Changez la couleur de l'image 
                        activeMember.UpdateButtonStatus(Color.white);
                    }
                }
                else
                {
                    Debug.Log("Le transform du personnage est introuvable.");
                }
            }
            else
            {
                Debug.Log("Le menu de personnage est introuvable.");
            }
        }
        else {Debug.Log("Vous n'avez pas selectionné un humain"); }
        Debug.Log("Personnage selectionné" + activeMember.FirstName);
    }

    public void WhitedCharacterIcons()
    {
        if (charactereMenu.transform is null) 
        {
            Debug.Log("Le personnage est introuvable");
        }
        foreach (UnityEngine.Transform child in charactereMenu.transform)
        {
            GameObject characterObject = child.gameObject;
            Image characterImage = characterObject.GetComponent<Image>();
            if (characterImage != null)
            {
                characterImage.color = Color.white; // Modifier la couleur en blanc
            }
        }
        foreach (Character member in members)
        {   
            if (member is Human humanMember)
            {
                humanMember.UpdateButtonStatus(Color.green);
            }
        }
    }
    /****************************************************************/
    /********** FIN FONCTIONS DE CHANGEMENT SUR LA TRIBU ************/
    /****************************************************************/

    /*******************************************************/
    /********** FONCTIONS DIVERSES SUR LA TRIBU ************/
    /*******************************************************/
    public bool AvailableAnimal (string type) //Permet de savoir si un animal est disponible pour être installé dans un habitat
    {
        foreach (Character member in members)
        {   
            if (type == "Bee")
            {
                if (member is SwarmBees swarmbeesMember && swarmbeesMember.beehive == null){return true;}
            }
        }
        return false;
    }



    private static List<string> maleNames = new List<string>
    {"Maël","Tanguy","Loïc","Owen","Ronan","Gwenaël","Ewen","Alan","Morgan","Cédric","Gaël","Erwan","Tristan","Gurvan","Loeiz","Bleuzen","Iwan","Riwan","Goulven","Peran","Elouan","Pierrick"};

    private static List<string> femaleNames = new List<string>
    {"Maëlle","Morgane","Léna","Enora","Manon","Rozenn","Nolwenn","Erell","Gwen","Tiphaine","Maïwenn","Yseult","Elouan","Gwennan","Katell","Anna","Louve","Katell","Anwen","Eulalie","Tifaine","Emeline"};

    private static List<string> chickenNames = new List<string>
    {"Clémentine","Coquette","Pépette","Plumette","Cocotte","Poulette","Rosalie","Marguerite","Agathe","Gisèle","Paquerette","Prunelle","Eglantine","Bérénice","Azura","Praline","Perle","Cannelle","Violette","Camille","Félicie","Hortense", "Odette", "Suzette", "Clarisse", "Eugénie", "Joséphine", "Solange"};

/*
    private static List<string> maleNames = new List<string>
    {"Benjamin","Gautier","Antoine","Tom","Geoffrey","Louis","Germain","Sébastien","Baptiste","Pierrick","Manu","Hugo","Romain","Julien","Samuel","Robin","Emilien","Thibault","Alexandre","",""};

    private static List<string> femaleNames = new List<string>
    {"Sophie","Elisa","Typhanie","Cécile","Céline","Lucie","","","","Emeline","","","","","","","","","","",""};
*/
}
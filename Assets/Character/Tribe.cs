using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

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
        Human man = CreateRandomCharacter(Gender.Male, "Human");
        man.AddSkill("Naturalisme", false);
        man.AddSkill("Bricolage", false);
        //man.CharacterStatus = Status.Cold;
        OnCharacterClick(man);
        Human woman = CreateRandomCharacter(Gender.Female, "Human");
        woman.AddSkill("Paysanerie", false);
        woman.AddSkill("Artisanat", false);
        members.Add(man); 
        members.Add(woman);
        /*activeMember = (Human)members[0];*/
        humanNumber = 2;
        UpdateCharacterMenu();
        OnCharacterClick((Human)members[0]);

        //-----------TEMPORAIRE POUR CREER DES ANIMAUX --------------
        // Créez les animaux avec les valeurs aléatoires
        CreateRandomAnimal (0);
        CreateRandomAnimal (1);
        CreateRandomAnimal (1);
        /*Chicken chicken = new Chicken("Chicken_1",  1, Gender.Female, currentdate);
        members.Add(chicken);
        Chicken chicken2 = new Chicken("Chicken_2",  2, Gender.Female, currentdate);
        members.Add(chicken2);
        Duck duck1 = new Duck("Duck_1", 1, Gender.Female, currentdate);
        members.Add(duck1);*/
        /*Chicken chicken4 = new Chicken("Chicken_4",  4, Gender.Female, currentdate);
        members.Add(chicken4);*/
        //----------- A SUPPRIMER -------------------    
    }

    public void CreateRandomAnimal (int type) //0 = Ruche, 1 = Poule, 2 = Coq, 3 = Lapine, 4 = Lapin, 5 = Cane, 6 = Canard, 7 = Chèvre, 8 = Bouc
    {
        //Les animaux ont entre 3 et 24 mois
        int randomMonthold = UnityEngine.Random.Range(3, 24);
        DateTime currentDate = new DateTime(TurnContext.Instance.year, TurnContext.Instance.month, 1); 
        DateTime targetDate = currentDate.AddMonths(-randomMonthold);

        if (type == 0) // Ruche
        {
            SwarmBees bees = new SwarmBees("Bees_1", 1, targetDate, 5, 3);
            members.Add(bees);
        }
        else if (type == 1) // Poule
        {
            //Nom random
            int randomNumber = UnityEngine.Random.Range(0, chickenNames.Count);
            string randomName = chickenNames[randomNumber];
            Chicken chicken = new Chicken(randomName, UnityEngine.Random.Range(1, 5), Gender.Female, targetDate);
            members.Add(chicken);
        }
        else if (type == 6) // Canard
        {
            //Nom random
            int randomNumber = UnityEngine.Random.Range(0, duckNames.Count);
            string randomName = duckNames[randomNumber];            
            Duck duck = new Duck(randomName, 1, Gender.Female, targetDate);
            members.Add(duck);
        }
    }

    public void ReinitializeTribe()
    {
        foreach (UnityEngine.Transform child in this.gameObject.transform){GameObject.Destroy(child.gameObject);} 
        foreach(Character member in members)
        {
            member.characterToken.DestroyToken();
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
    public int[] Need() 
    {
        int boisChauffage = 100; // En fonction d'un an de consommation de bois suivant nombre de maison
        int eauPotable = 10;
        int fruit = 10;
        int legume = 10;
        int cereale = 10;
        int proteine = 10;
        int autre = 0; // Autres besoins, par défaut à 0

        return new int[] {eauPotable, boisChauffage, fruit, legume, cereale, proteine, autre };
    }

    public int TotalHaveHome()
    {
        int result = 0;
        foreach (Character member in members)
        {
            if (!member.IsAvailable()){result ++;}
        }        
        return result;
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

    public Vector2Int EnclosureAvailableSpace() //Permet de retourner un emplacement disponible dans l'enclos
    {
        var occupiedPositions = new HashSet<Vector2Int>
        {
            new Vector2Int(-4, 9),
            new Vector2Int(-4, 10),
            new Vector2Int(-3, 9),
            new Vector2Int(-3, 10)
        };
        foreach (Character member in members)
        {
            if (member is Animal && member.Alive())
            {
                Vector2Int positionAnimal = member.characterToken.GetIsometricPosition();
                occupiedPositions.Remove(positionAnimal);
            }
        }
        if (occupiedPositions.Contains(new Vector2Int(-4, 9))) return new Vector2Int(-4, 9);
        if (occupiedPositions.Contains(new Vector2Int(-4, 10))) return new Vector2Int(-4, 10);
        if (occupiedPositions.Contains(new Vector2Int(-3, 9))) return new Vector2Int(-3, 9);
        if (occupiedPositions.Contains(new Vector2Int(-3, 10))) return new Vector2Int(-3, 10);
        return new Vector2Int(-1, 1);
    }

    public bool EnclosureAvailable() //Permet de savoir si un emplacement est disponible dans l'enclos d'entrée
    {
        var occupiedPositions = new HashSet<Vector2Int>
        {
            new Vector2Int(-4, 9),
            new Vector2Int(-4, 10),
            new Vector2Int(-3, 9),
            new Vector2Int(-3, 10)
        };

        foreach (Character member in members)
        {
            if (member is Animal && member.Alive())
            {
                Vector2Int positionAnimal = member.characterToken.GetIsometricPosition();
                occupiedPositions.Remove(positionAnimal);
            }
        }
        bool space1 = occupiedPositions.Contains(new Vector2Int(-4, 9));
        bool space2 = occupiedPositions.Contains(new Vector2Int(-4, 10));
        bool space3 = occupiedPositions.Contains(new Vector2Int(-3, 9));
        bool space4 = occupiedPositions.Contains(new Vector2Int(-3, 10));
        Debug.LogError($"voici le result space1: {space1}, space2: {space2}, space3: {space3}, space4: {space4}");
        return space1 || space2 || space3 || space4;
    }
    /*****************************************************************/
    /********** FIN FONCTIONS D'INFORMATION DE LA TRIBU **************/
    /*****************************************************************/

    /************************************************************/
    /********** FONCTIONS DE CHANGEMENT SUR LA TRIBU ************/
    /************************************************************/
    private static Human CreateRandomCharacter(Gender gender, string type)
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
                if (member is Human && randomid == member.characterToken.idImage && member.Alive() && gender == member.Gender)
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

        //TO DO : Ajouter une caractéristique et deux compétences de base

        return character;
    }


    private void AddMember() 
    {
        if (humanNumber < 20)
        {
            int randomNumber = UnityEngine.Random.Range(0, 2);
            Character newCharacter = null;
            if (randomNumber == 0) {newCharacter = CreateRandomCharacter(Gender.Male, "Human");}
            else {newCharacter = CreateRandomCharacter(Gender.Female, "Human");}
            //newCharacter
            members.Add(newCharacter);
            humanNumber++;
            UpdateCharacterMenu();
        }
        else 
        {
            EventPile.Instance.AddEvent("Vous avez atteint le maximum dans votre tribu...", "Unhappy",  2, new Vector2Int(-1,-1));
            Debug.LogWarning("Vous avez atteint le maximum dans votre tribu...");
        }
    }
    
    void Update() // Fonction pour changer de membre de tribu
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool found = false;
            Debug.Log(activeMember.FirstName + " est son nom + ");
            for (int i = 0; i < members.Count; i++)
            {
                Debug.Log("boucle number :" + i + "nom membre" + members[i].FirstName);
            }
            int currentIndex = members.IndexOf(activeMember);
            Debug.Log("current :" + currentIndex + "");
            for (int i = 0; i < members.Count; i++)
            {
                Debug.Log("boucle number :" + i + "nom membre" + members[i].FirstName);
            }
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

    public Character OnTileClick(Vector2Int clickedTilePosition)
    {
        Character characterFound = members.Where(member => member.Alive() == true).FirstOrDefault(member => member.characterToken.GetIsometricPosition().x == clickedTilePosition.x && member.characterToken.GetIsometricPosition().y == clickedTilePosition.y);
        //Debug.LogWarning($"planttrouvee = {planttrouve.position.x} et {planttrouve.position.y}");
        return characterFound;
    }

    public void  UpdateCharacterMenu()
    {
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
            ActionContext.Instance.HideActionMenu();
            GameSettings.Instance.hoverMode = 5; 
            activeMember = (Human)member;
            //CameraController.PlaceOnPosition(member.characterToken.transform.position); //TO DO : Voir pour aller au Token si on clique sur un personnage
            ActionPile.Instance.Refresh();
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
                    Debug.LogWarning("The Human transform is unfound.");
                }
            }
            else
            {
                Debug.LogWarning("Character Menu is unfound.");
            }
        }
        else {Debug.Log("You didn't select a valid Human."); }
        Debug.Log("Selected Human " + activeMember.FirstName);
    }

    public void WhitedCharacterIcons()
    {
        if (charactereMenu.transform is null) 
        {
            Debug.LogWarning("The Human transform is unfound.");
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
            else if (type == "Chicken")
            {
                if (member is Chicken chickenMember && chickenMember.chickenCoop == null){return true;}
            }
        }
        return false;
    }

    public void NextMonth()
    {
        Debug.Log("Passe par Tribe next month");
        //GESTION DE NOURRITURE FIN DE MOIS
        //Compteurs des besoins
        
        int fruitNeed = 20*humanNumber;
        int vegetableNeed = 25*humanNumber; //20 APRES ALCOOL
        int cerealNeed = 25*humanNumber; //20 APRES ALCOOL
        // Filtrer uniquement les membres de type Human
        var humanMembers = members.OfType<Human>();

        // Calculs basés sur les caractéristiques des Humans
        int vegetarianCount = humanMembers.Count(human => human.characteristic == "Végétarien");
        int vegeProteinNeed = 5 * vegetarianCount;

        int hungryCount = humanMembers.Count(human => human.characteristic == "Bon vivant");
        int hyperactiveCount = humanMembers.Count(human => human.characteristic == "Hyperactif");

        int totalNeed = 90 * humanNumber + (15 * (hyperactiveCount + hungryCount)) - (5 * vegetarianCount);

        int proteinNeed = 20 * (hungryCount+ hyperactiveCount) + 10*(humanNumber - hungryCount - hyperactiveCount - vegetarianCount);
        int alcoolNeed = 15 * hungryCount + 10 * (humanNumber - hungryCount - vegetarianCount) + 5 * vegetarianCount;
        int otherNeed = 5 * (hungryCount+ hyperactiveCount) + 4 * (humanNumber - hungryCount - hyperactiveCount);

        // Consommation originale des Humans
        List<int> result = Inventory.Instance.DecrementTypeStock("Fruit", fruitNeed, 1, false);
        int fruitConsome = result[0];
        int bonus = result[1];
        int variety = result[2];

        result = Inventory.Instance.DecrementTypeStock("Légume", vegetableNeed, 1, false);
        int vegetableConsome = result[0];
        bonus += result[1];
        variety += result[2];

        result = Inventory.Instance.DecrementTypeStock("Céréale consommable", cerealNeed, 1, false);
        int cerealConsome = result[0];   
        bonus += result[1];
        variety += result[2]; 

        /*result = Inventory.Instance.DecrementTypeStock("Alcool", alcoolNeed, 1, false);
        vegetableConsome = result[0];*/    
        int vinegarConsome = Math.Min(otherNeed, Inventory.Instance.GetItemStock("Vinaigre"));
        Inventory.Instance.ChangeItemStock("Vinaigre", - otherNeed);
  
        result = Inventory.Instance.DecrementTypeStock("Sucre", otherNeed, 1, false);
        int sugarConsome = result[0];    
        bonus += result[1];
        variety += result[2];

        result = Inventory.Instance.DecrementTypeStock("Matière grasse", otherNeed, 1, false);
        int fatConsome = result[0];            
        bonus += result[1];
        variety += result[2];

        result = Inventory.Instance.DecrementTypeStock("Condiments", otherNeed, 1, false);
        int condimentConsome = result[0];  
        bonus += result[1];
        variety += result[2];

        int saltConsome = Math.Min(otherNeed, Inventory.Instance.GetItemStock("Sel"));
        Inventory.Instance.ChangeItemStock("Sel", - otherNeed);

        //GESTION VEGE/PAS VEGE
        result = Inventory.Instance.DecrementTypeStock("Protéine Végétarien", vegeProteinNeed, 1, false);
        int vegeConsome = result[0];   
        int bonusvege = bonus + result[1];
        int varietyvege = variety + result[2];

        result = Inventory.Instance.DecrementTypeStock("Protéine", proteinNeed, 1, false);
        int proteinConsome = result[0];  
        bonus += result[1];
        variety += result[2];


        EventPile.Instance.AddEvent("Consommation total "+ fruitConsome + "-"+ vegetableConsome + "-"+ cerealConsome + "-"+ vegeConsome + "-"+ proteinConsome + "-"+ vinegarConsome + "-"+ sugarConsome + "-"+ fatConsome + "-"+  condimentConsome + "-"+ saltConsome, "Pending", 2, new Vector2Int(-1,-1));
        EventPile.Instance.AddEvent("Bonus total "+ bonus + "- Variété "+ variety , "Pending", 2, new Vector2Int(-1,-1));

        // Consommation pour compenser des Humans -- pas de bonus, pas de variété (car pas rempli par le bon type d'aliment) 
        int basicMissing = (fruitNeed - fruitConsome) + (vegetableNeed - vegetableConsome) + (cerealNeed - cerealConsome) + (vegeProteinNeed - vegeConsome) + (proteinNeed - proteinConsome);
        int basicMissingLess = basicMissing; 
        //TO DO : Ajouter une gestion plus logique de compensation (type le plus en stock par exemple)
        //compensation sucre par fruit
        int sugarMissing = otherNeed - sugarConsome;
        if (sugarMissing > 0) 
        {
            result = Inventory.Instance.DecrementTypeStock("Fruit", sugarMissing, 1, false);
            sugarMissing -= result[0];
        }

        //compendation des autres aliments        
        if (basicMissingLess > 0) 
        {
            result = Inventory.Instance.DecrementTypeStock("Légume", basicMissingLess, 1, false);
            basicMissingLess -= result[0];
        }
        if (basicMissingLess > 0) 
        {
            result = Inventory.Instance.DecrementTypeStock("Céréale consommable", basicMissingLess, 1, false);
            basicMissingLess -= result[0];
        }
        if (basicMissingLess > 0) 
        {
            result = Inventory.Instance.DecrementTypeStock("Protéine Végétarien", basicMissingLess, 1, false);
            basicMissingLess -= result[0];
        }
        //int vegenotConsome = result[0];
        if (basicMissingLess > 0) 
        {
            result = Inventory.Instance.DecrementTypeStock("Protéine", basicMissingLess, 1, false);
            basicMissingLess -= result[0];
        }

        if (basicMissingLess > 0) 
        {
            result = Inventory.Instance.DecrementTypeStock("Fruit", basicMissingLess, 1, false);
            basicMissingLess = basicMissing - result[0];
        }




        

        int totalConsome = fruitNeed + vegetableNeed + cerealNeed + vegeProteinNeed + proteinNeed + vinegarConsome + otherNeed - sugarMissing + fatConsome + condimentConsome + saltConsome - basicMissingLess;
        float slicedOfNeed = totalConsome/totalNeed;
        //Consommation individuelle de nourriture + mise à jour des indicateurs
        foreach (Character member in members)
        {
            if (member is Human human)
            {
                if (human.characteristic == "Végétarien")
                {
                    human.NextMonth(slicedOfNeed, bonusvege, varietyvege);
                }
                if (human.characteristic == "Végétarien")
                {
                    human.NextMonth(slicedOfNeed, bonus, variety);
                }
            }    
            else if (member is Animal animal)
            {
                animal.NextMonth();
            }
                
            /*
            if (finishedNextMonth != "Ok")
            {
                

                Debug.Log(member.FirstName+" : " +  finishedNextMonth); // TO DO : Ajouter le texte dans une console d.evenement
            }*/

        }
    }

    private static List<string> maleNames = new List<string>
    {"Maël","Tanguy","Loïc","Owen","Ronan","Gwenaël","Ewen","Alan","Morgan","Cédric","Gaël","Erwan","Tristan","Gurvan","Loeiz","Bleuzen","Iwan","Riwan","Goulven","Peran","Elouan","Pierrick"};

    private static List<string> femaleNames = new List<string>
    {"Maëlle","Morgane","Léna","Enora","Manon","Rozenn","Nolwenn","Erell","Gwen","Tiphaine","Maïwenn","Yseult","Elouan","Gwennan","Katell","Anna","Louve","Katell","Anwen","Eulalie","Tifaine","Emeline"};

    private static List<string> chickenNames = new List<string>
    {"Clémentine","Coquette","Pépette","Plumette","Cocotte","Poulette","Rosalie","Marguerite","Agathe","Gisèle","Paquerette","Prunelle","Eglantine","Bérénice","Azura","Praline","Perle","Cannelle","Violette","Camille","Félicie","Hortense", "Odette", "Suzette", "Clarisse", "Eugénie", "Joséphine", "Solange"};

    private static List<string> duckNames = new List<string>
    {"Donald", "Daisy", "Coincoin", "Nestor", "Canette", "Gédéon", "Bernard", "Patapon", "Gaston", "Marcel", "Mireille", "Anatole", "Quackie", "Firmin", "Gustave", "Balthazar", "Canelle", "Jules", "Maurice", "Ferdinand", "Blanche", "Margot", "Josette", "Louison", "Fifi", "Léon", "Simone", "Oscar", "Titou", "Ninon"};
/*
    private static List<string> maleNames = new List<string>
    {"Benjamin","Gautier","Antoine","Tom","Geoffrey","Louis","Germain","Sébastien","Baptiste","Pierrick","Manu","Hugo","Romain","Julien","Samuel","Robin","Emilien","Thibault","Alexandre","",""};

    private static List<string> femaleNames = new List<string>
    {"Sophie","Elisa","Typhanie","Cécile","Céline","Lucie","","","","Emeline","","","","","","","","","","",""};
*/


    /*******************************************************/
    /********** FONCTIONS SAUVEGARDE DE TRIBU **************/
    /*******************************************************/
    [Serializable]
    private class TribeSaveData
    {
        public List<CharacterSaveData> characters;
    }

    [Serializable]
    private class CharacterSaveData
    {
        public int Id;
        public string FirstName;
        public Gender Gender;
        public Status CharacterStatus;
        public DateTime DateOfBirth;
        public int feedLevel;
        public int happinessLevel;
        public int energyLevel;
        public int idImage;
        public Vector2Int targetPosition;
    }

    // Sauvegarde de la tribu
    public void SaveTribe(string filePath)
    {
        Debug.LogError("sauvegarde du fichier : " + filePath);
        var saveData = new TribeSaveData
        {
            characters = new List<CharacterSaveData>()
        };

        foreach (var character in members)
        {
            var characterToken = character.characterToken;
            var characterSaveData = new CharacterSaveData
            {
                Id = character.Id,
                FirstName = character.FirstName,
                Gender = character.Gender,
                CharacterStatus = character.CharacterStatus,
                DateOfBirth = character.DateOfBirth,
                feedLevel = character.feedLevel,
                happinessLevel = character.happinessLevel,
                energyLevel = character.energyLevel,
                idImage = characterToken?.idImage ?? 0,
                targetPosition = characterToken?.targetPosition ?? Vector2Int.zero
            };
            saveData.characters.Add(characterSaveData);
        }

        var json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    // Chargement de la tribu
    public void LoadTribe(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var json = File.ReadAllText(filePath);
        var saveData = JsonConvert.DeserializeObject<TribeSaveData>(json);

        members.Clear();
/*
        foreach (var characterData in saveData.characters)
        {
            var character = new Character
            {
                Id = characterData.Id,
                FirstName = characterData.FirstName,
                Gender = characterData.Gender,
                CharacterStatus = characterData.CharacterStatus,
                feedLevel = characterData.feedLevel,
                happinessLevel = characterData.happinessLevel,
                energyLevel = characterData.energyLevel,
                DateOfBirth = characterData.DateOfBirth
            };

            character.characterToken = new CharacterToken
            {
                idImage = characterData.idImage,
                targetPosition = characterData.targetPosition
            };

            members.Add(character);
        }*/
    }
}
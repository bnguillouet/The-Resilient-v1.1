using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class CharacterToken : MonoBehaviour
{
    public Character character;
    public UnityEngine.Transform tokenTransform;
    public Vector2Int targetPosition;
    public float speed = 5f;
    public Image characterImage;
    private List<Vector2Int> path;
    public int idImage { set; get; }
    private int counter = 0; //Cas d'une animation
    private int direction = 1; //Direction 1 à 8 pour aléatoire
    private float timer = 0f; //Cas d'une animation
    private Canvas canvas;
    private float changeInterval = 0.10f;
    private bool pending; //true = en train de faire quelque chose, false = fini
    public List<Action> actionsExecution = new List<Action>(); //Cas humain
    public bool leave = false; //cas animaux qui s'échapent

    /*****************************************/
    /********** CREATION DU TOKEN ************/
    /*****************************************/
    public void Initialize(Character characterInput, int idImageInput)
    {
        idImage = idImageInput;
        character = characterInput;
        string type = GetCharacterType(character);
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10002;
        characterImage = new GameObject("CharacterImage_" + idImage).AddComponent<Image>();
        characterImage.transform.SetParent(canvas.transform, false);
        path = null;
        transform.position = new Vector3(1,1,1);
        LoadCharacterImage(type, idImage);
        leave = false;
    }
    /*********************************************/
    /********** FIN CREATION DU TOKEN ************/
    /*********************************************/

    /******************************************/
    /********** MOUVEMENT DU TOKEN ************/
    /******************************************/
    private void Update()
    {
        Vector3 worldTargetPosition = IsometricToUnityCoordinates(targetPosition);
        worldTargetPosition.y = worldTargetPosition.y + 0.3f; 
        /*TO DO / Mettre à jour le Ratiospeed que si on change de tile ? */
        if (TurnContext.Instance.speed == 1) {transform.position = Vector3.MoveTowards(transform.position, worldTargetPosition, speed * RatioSpeed(transform.position) * Time.deltaTime);}
        else if (TurnContext.Instance.speed == 2) {transform.position = Vector3.MoveTowards(transform.position, worldTargetPosition, speed * 4 * RatioSpeed(transform.position) * Time.deltaTime);}
        canvas.sortingOrder = 10012 - (int)(12*((transform.position.y-0.4f)/0.3f));
        if (character is Human && actionsExecution.Count != 0)
        {
            ExecuteHumanActions();

        }
        if (character is SwarmBees)
        {
            UpdateSwarmBees();
        }
        if (character is Chicken || character is Duck) // Idem pour Canard et Mouton. Lapin a part car dans cage
        {
            UpdateAnimal();
        }
    }


    private void GoHome()
    {
        if (character is Human human)
        {
            targetPosition = human.house.position;
        }
        if (character is SwarmBees bees)
        {
            
        }
        if (character is Chicken chicken) 
        {
            targetPosition = chicken.chickenCoop.position;
            chicken.Lay();
        }
    }
    
    private void ExecuteHumanActions()
    {
        Action actionExecution = actionsExecution[0];
        Vector3Int currentCell = TileGrid.Instance.tilemap.WorldToCell(new Vector3(transform.position.x, transform.position.y -0.3f, transform.position.z));  
        if (actionExecution.actionPosition.x == currentCell.x && actionExecution.actionPosition.y == currentCell.y)
        {
            if (actionExecution.timer <= 0f)
            {
                actionExecution.BeginExecution();
                actionsExecution.RemoveAt(0);
                UpdateStatus("Sleep");
                //if (Tribe.activeMember == character){} //Refresh uniquement s'il s'agit de l'humain selectionné
                 ActionPile.Instance.Refresh(); // TO DO : REMETTRE
            }
            else
            {
                if (TurnContext.Instance.speed == 1) {actionExecution.timer -= Time.deltaTime;}
                else if (TurnContext.Instance.speed == 2) {actionExecution.timer -= Time.deltaTime * 4;}

                if (!actionExecution.begin)
                {
                    actionExecution.begin = true;
                    UpdateStatus(actionExecution.actionCategorie);
                    if(actionExecution.actionName == "Construire" || actionExecution.actionName == "Déplacer")
                    {
                        actionExecution.building.Construction();
                        //actionExecution.building.UpdateVisual();
                    }
                    //else {UpdateStatus("Pending");
                    //if (Tribe.activeMember == character){} //TO DO : Refresh uniquement s'il s'agit de l'humain selectionné
                    /*ActionPile.Instance.Refresh();*/ // TO DO : REMETTRE
                }
            }
        }
        else
        {
            SetTargetPosition(actionExecution.actionPosition);
            UpdateStatus("Walking");
        }
    }

    private void UpdateSwarmBees() //TO DO : Adapté a d'autre cas : Poules et animaux
    {
        if (TurnContext.Instance.speed == 1) {timer +=  Time.deltaTime;}
        else if (TurnContext.Instance.speed == 2) {timer +=  Time.deltaTime * 4;}
        if (timer >= changeInterval)
        {
            timer -= changeInterval;
            counter = (counter + 1) % 10;
            Texture2D randomCharacterTexture = Resources.Load<Texture2D>("Tribe/Icons/bees_1_" + counter);
            if (randomCharacterTexture != null)
            {
                characterImage.sprite = Sprite.Create(randomCharacterTexture, new Rect(0, 0, randomCharacterTexture.width, randomCharacterTexture.height), Vector2.one * 0.5f);
            }
        }
    }

    private void UpdateAnimal()
    {
        if (TurnContext.Instance.speed == 1) {timer +=  Time.deltaTime;}
        else if (TurnContext.Instance.speed == 2) {timer +=  Time.deltaTime * 4;}
        if (timer >= changeInterval)
        {
            timer = 0f;
            Vector3Int currentCell = TileGrid.Instance.tilemap.WorldToCell(new Vector3(transform.position.x, transform.position.y -0.3f, transform.position.z));  
            if (targetPosition.x == currentCell.x && targetPosition.y == currentCell.y && !leave)
            {
                int random = UnityEngine.Random.Range(1, 500);
                if (random == 1 && !((currentCell.x == -3 || currentCell.x == -4 ) && (currentCell.y == 10 || currentCell.y == 9)))
                {
                    GoHome();
                    float orientation = AngleInDegrees(new Vector2Int(currentCell.x, currentCell.y),targetPosition);
                    if (orientation < 45f) {direction = 5;}
                    else if (orientation < 90f) {direction = 4;}
                    else if (orientation < 135f) {direction = 3;}
                    else if (orientation < 180f) {direction = 2;}
                    else if (orientation < 225f) {direction = 1;}
                    else if (orientation < 270f) {direction = 8;}
                    else if (orientation < 315f) {direction = 7;}
                    else if (orientation < 360f) {direction = 6;}
                    else {direction = 5;}
                    counter = 0;
                    string type = GetCharacterType(character);
                    LoadCharacterImageDirection(type, idImage);
                }
                random = UnityEngine.Random.Range(1, 30);
                if (random == 1)
                {
                    if (counter > 0)
                    {
                        counter -= 1;
                    }
                    else
                    {
                        direction = UnityEngine.Random.Range(1, 9);
                        Vector2Int oldPosition = targetPosition;
                        if (direction == 1) {targetPosition.x = currentCell.x - 1;}
                        else if (direction == 2) {targetPosition.x = currentCell.x - 1; targetPosition.y = currentCell.y + 1;}
                        else if (direction == 3) {targetPosition.y = currentCell.y + 1;}
                        else if (direction == 4) {targetPosition.x = currentCell.x + 1; targetPosition.y = currentCell.y + 1;}
                        else if (direction == 5) {targetPosition.x = currentCell.x + 1;}
                        else if (direction == 6) {targetPosition.x = currentCell.x + 1; targetPosition.y = currentCell.y - 1;}
                        else if (direction == 7) {targetPosition.y = currentCell.y - 1;}
                        else if (direction == 8) {targetPosition.x = currentCell.x - 1; targetPosition.y = currentCell.y - 1;}
                        //test target
                        /*if (Settlement.Instance.OnTileClickBool(new Vector3Int((int)targetPosition.x, (int)targetPosition.y, 0)) || targetPosition.x < 0 || targetPosition.y < 0 || targetPosition.x >= GameSettings.Instance.gridSizeX || targetPosition.y >= GameSettings.Instance.gridSizeZ)
                        */
                        if (BlockedByEnclosure())
                        {
                            targetPosition = oldPosition;
                        }

                        if (targetPosition.x < 0 || targetPosition.y < 0 || targetPosition.x >= GameSettings.Instance.gridSize || targetPosition.y >= GameSettings.Instance.gridSize)
                        {
                            if (((currentCell.x == -3 || currentCell.x == -4 ) && (currentCell.y == 10 || currentCell.y == 9)) || GameSettings.Instance.DifficultyLevel == 0 || GameSettings.Instance.DifficultyLevel == 99)
                            {
                                targetPosition = oldPosition;
                            }

                            else
                            {
                                GoTargetPosition(targetPosition);
                                leave = true;
                            }
                        }

                    }

                    string type = GetCharacterType(character);
                    LoadCharacterImageDirection(type, idImage);

                }
                else if (random > 1 && random < 20 && (direction == 6 || direction == 5 || direction == 7 || direction == 1)) // enlever direction == 6
                {
                    if ((counter == 0  && random < 4) || counter == 1)
                    {
                        counter += 1;
                    }
                    else if (counter == 2)
                    {
                        if (random <3)
                        {

                        }
                        counter = 1;
                    }
                    string type = GetCharacterType(character);
                    LoadCharacterImageDirection(type, idImage);
                }
            }
        }
    }

    public bool BlockedByEnclosure()
    {
        return false;
    }
    public float AngleInDegrees(Vector2 position1, Vector2 position2)
    {
        // Calcul de la différence entre les coordonnées des deux points
        float deltaY = position2.y - position1.y;
        float deltaX = position2.x - position1.x;

        // Calcul de l'angle en radians, puis conversion en degrés
        float angleRadians = Mathf.Atan2(deltaY, deltaX);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;
        angleDegrees += 22.5f;
        // Pour avoir un angle entre 0 et 360 degrés
        if (angleDegrees < 0)
        {
            angleDegrees += 360;
        }

        return angleDegrees;
    }

    public void DestroyToken()
    {
        //Destroy(characterImage.transform.parent.gameObject);
        Destroy(this.gameObject);
    }
    /**********************************************/
    /********** FIN MOUVEMENT DU TOKEN ************/
    /**********************************************/

    /*******************************************************/
    /********** CHANGEMENT ET ATTRIBUT DU TOKEN ************/
    /*******************************************************/
    public void GoTargetPosition(Vector2Int target)
    {
        targetPosition = target;
        Vector3 worldTargetPosition = IsometricToUnityCoordinates(targetPosition);
        transform.position = worldTargetPosition;
    }

    public float RatioSpeed(Vector3 position)
    {
        Vector3Int currentCell = TileGrid.Instance.tilemap.WorldToCell(transform.position);
        if (currentCell.x >=0 && currentCell.y >=0)
        {
            Tile currentTile = TileGrid.Instance.tiles[currentCell.x, currentCell.y];
            if (currentTile.GetTileType() == TileType.Water){return 0.3f;}
            else if (currentTile.vegetationLevel >= 8){return 0.3f;}
            else if (currentTile.vegetationLevel >= 6){return 0.5f;}
            else if (currentTile.vegetationLevel >= 4){return 0.8f;}
            else return 1f;
        }
        else return 1f;
    }

    private string GetCharacterType(Character character) // TO DO : Faire évoluer pour les autres types de Token Poule,Insectes...
    {
        if (character is Human)
        {
            return character.Gender == Gender.Male ? "man" : "woman";
        }
        else if (character is SwarmBees){ return "bees";}
        else if (character is Chicken){ return "chicken";}
        else if (character is Duck){ return "duck";}
        else return "default";
    }

    private void LoadCharacterImage(string type, int idImage)
    {
        Texture2D randomCharacterTexture = Resources.Load<Texture2D>("Tribe/Icons/" + type + "_" + idImage);
        if (randomCharacterTexture != null)
        {
            characterImage.sprite = Sprite.Create(randomCharacterTexture, new Rect(0, 0, randomCharacterTexture.width, randomCharacterTexture.height), Vector2.one * 0.5f);
            characterImage.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
        }
    }

    private void LoadCharacterImageDirection(string type, int idImage)
    {
        Texture2D randomCharacterTexture = Resources.Load<Texture2D>("Tribe/Icons/" + type + "_" + idImage + "_" + direction);

        if (counter != 0)
        {
            randomCharacterTexture = Resources.Load<Texture2D>("Tribe/Icons/" + type + "_" + idImage + "_" + direction + "_" + counter);
        }

        if (randomCharacterTexture != null)
        {
            characterImage.sprite = Sprite.Create(randomCharacterTexture, new Rect(0, 0, randomCharacterTexture.width, randomCharacterTexture.height), Vector2.one * 0.5f);
            characterImage.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
        }
        if (leave)
        {
            characterImage.color = new Color(1f,0.5f,0.5f,0.5f);
        }
        else
        {
            characterImage.color = new Color(1f,1f,1f,1f);
        }
    }

    public void UpdateStatus (string statut)
    {
        //TO DO : Nettoyer statut
        Canvas canvas = gameObject.GetComponent<Canvas>();
        UnityEngine.Transform child = canvas.transform.Find("Icon_Status_1");
        if (child != null) {Destroy(child.gameObject);}
        UnityEngine.Transform child2 = canvas.transform.Find("Icon_Status_2");
        if (child2 != null) {Destroy(child2.gameObject);}

        float top = 0.3f;
        Texture2D texture = Resources.Load<Texture2D>("Icons/Status/"+character.CharacterStatus);
        Debug.LogWarning("mise a jour du status" + character.CharacterStatus + "et " + statut);
        if (texture != null)        
        {
            GameObject icon = new GameObject("Icon_Status_1");
            Image iconImage = icon.AddComponent<Image>();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(1f, 1f), 25);
            icon.GetComponent<Image>().sprite = sprite;
            //icon.transform.SetParent(tokenTransform);
            //icon.transform.SetParent(gameObject.transform, false);
            icon.transform.SetParent(canvas.transform, false);
            icon.transform.position = new Vector3 (gameObject.transform.position.x + 0.25f, gameObject.transform.position.y + top, 0);
            icon.transform.localScale = new Vector3 (0.002f,0.002f,1f);
            top = 0.1f;    
        }
        texture = Resources.Load<Texture2D>("Icons/Status/"+statut);
        if (texture == null)        
        {
            texture = Resources.Load<Texture2D>("Icons/Status/Pending");
        }
        if (texture != null)        
        {
            GameObject icon = new GameObject("Icon_Status_2");
            Image iconImage = icon.AddComponent<Image>();
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(1f, 1f), 25);
            icon.GetComponent<Image>().sprite = sprite;
            //icon.transform.SetParent(tokenTransform);
            //icon.transform.SetParent(gameObject.transform, false);
            icon.transform.SetParent(canvas.transform, false);
            
            icon.transform.position = new Vector3 (gameObject.transform.position.x + 0.25f, gameObject.transform.position.y + top, 0);
            icon.transform.localScale = new Vector3 (0.002f,0.002f,1f); 
        }        
    }

    private Vector3 IsometricToUnityCoordinates(Vector2 isometricCoordinates)
    {
        float scale = 0.6f;
        float unityX = (isometricCoordinates.x - isometricCoordinates.y)/2;
        float unityY = (isometricCoordinates.x + isometricCoordinates.y)/2* scale + 0.3f;
        return new Vector3(unityX, unityY, 0); // Retourner les coordonnées converties
    }

    public Vector2Int GetIsometricPosition()
    {
        Vector3Int grid3Pos = TileGrid.Instance.tilemap.WorldToCell(new Vector3 (transform.position.x, transform.position.y-(float)0.3, 0));
        return new Vector2Int ((int)Math.Round((float)grid3Pos.x), (int)Math.Round((float)grid3Pos.y));
    }

    // Méthode pour définir la position cible
    public void SetTargetPosition(Vector2Int target)
    {
        targetPosition = target;
    }
    public void ToPosition(Vector2Int target)
    {
        targetPosition = target;
        Vector3 worldTargetPosition = IsometricToUnityCoordinates(targetPosition);
        transform.position = worldTargetPosition;
    }
    
    /***********************************************************/
    /********** FIN CHANGEMENT ET ATTRIBUT DU TOKEN ************/
    /***********************************************************/
}






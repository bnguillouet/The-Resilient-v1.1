using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ImageObject : MonoBehaviour
{
    public Building building;
    public Plant plant;
    public Tile tile;
    public Vector2Int position;
    public int order;
    public bool inside;
    public Vector2Int positionExt;
    public Image mainImage;// { set; get; }
    public Image onImage;
    public Image onImage1;
    public Image onImage2;
    public Image onImage3;
    private Canvas canvas;
    public int size;
    public string imageObjectPath;
    public Color color;
    public bool canBuild;

    public void Initialize(Building buildingInput, Plant plantInput, Tile tileInput, string imageObjectPathInput, int orderInput, Vector2Int positionInput, Vector2Int positionExtInput, string subMode, int sizeInput, Color colorInput, int transparency) // typeCanvas = 0 avant, 1 arriere, 2 interieur
    {
        building = buildingInput;
        plant = plantInput;
        tile = tileInput;
        order = orderInput;
        position = positionInput;
        positionExt = positionExtInput;
        imageObjectPath = imageObjectPathInput;
        size = sizeInput;
        color = colorInput;
        canBuild = true;
        /*
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        */
        //Ajouter ou utiliser le canvas
        canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null) // Si le Canvas n'existe pas, on le crée
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }
        //canvas.sortingOrder = 20000 - 24 * (position.x + position.y + ((int)size/2)*2) + orderInput;
        canvas.sortingOrder = 10000 - 12 * (position.x + position.y + ((int)size/2)*2) + orderInput;
        if (imageObjectPath.Contains("Tree/")) // Cas particulier de l'arbre versus size dans sortingOrder
        {
            canvas.sortingOrder = 10000 - 12 * (position.x + position.y) + orderInput;
        }
        mainImage = new GameObject("Main").AddComponent<Image>();
        mainImage.transform.SetParent(canvas.transform, false);
        if (color != Color.white)
        {
            onImage = new GameObject("On").AddComponent<Image>();
            onImage.transform.SetParent(canvas.transform, false);
        }
        /*onImage1 = new GameObject("On1").AddComponent<Image>();
        onImage1.transform.SetParent(canvas.transform, false);*/
        /*onImage2 = new GameObject("On2").AddComponent<Image>();
        onImage2.transform.SetParent(canvas.transform, false);
        onImage3 = new GameObject("On3").AddComponent<Image>();
        onImage3.transform.SetParent(canvas.transform, false);*/
        LoadImage(subMode, transparency, false);
    }

    public void ResetOnImage()
    {

    }

    public void DeleteOnImage()
    {
        onImage1.enabled = false;
        onImage2.enabled = false;
        onImage3.enabled = false;
    }


    public void ChangePreview(string imageObjectPathInput, int sizeInput)
    {
        imageObjectPath = imageObjectPathInput;
        size = sizeInput;
        canvas = gameObject.GetComponent<Canvas>();
        canvas.sortingOrder = 1;
        LoadImage("", 80, false);
    }

    public void UpdatePreview(int orderInput, Vector2Int positionInput, Vector2Int positionExtInput, bool canBuildInput)
    {
        order = orderInput;
        position = positionInput;
        positionExt = positionExtInput;
        canBuild = canBuildInput;
        MoveImage();
    }
    public void MakeInvisible()
    {
        if (mainImage != null){mainImage.enabled = false;}
        if (onImage != null){onImage.enabled = false;}
    }

    public void MakeVisible()
    {
        if (mainImage != null){mainImage.enabled = true;}
        if (onImage != null){onImage.enabled = true;}
    }

    public void MoveImage() //Pour les images dynamiques
    {
        float deltay = (position.x+position.y)*0.3f + (size+2)/2 + 0.5f;//+ ((size+2)*0.6f) -((float)((int)Math.Ceiling(size / 2f))-0.5f)*0.3f;
        transform.position = new Vector3((position.x-position.y)*0.5f,deltay,1);
        if (!canBuild) {mainImage.color = new Color(1f,0.6f,0.6f,0.6f);}
        else 
        {
            if (order == 100){mainImage.color = new Color(0.85f, 0.7f, 0f, 0.8f);}
            else {mainImage.color = new Color(1f,1f,1f,0.9f);}
        }
        canvas = gameObject.GetComponent<Canvas>();
        canvas.sortingOrder = 10000 - 12 * (position.x + position.y + ((int)size/2)*2) + order;
    }
    public void LoadImage(string subMode, int transparency, bool hover)
    {
        //Debug.Log(imageObjectPath + subMode);
        Texture2D BuildingTexture = Resources.Load<Texture2D>(imageObjectPath + subMode);
        if (order == 0) {BuildingTexture = Resources.Load<Texture2D>(imageObjectPath + subMode + "_Back");}
        if (BuildingTexture != null)  
        {
            if (imageObjectPath.Contains("Enclosure"))
            {
                //Debug.LogError("transparency enclosure : " + transparency);
            }
            
            mainImage.sprite = Sprite.Create(BuildingTexture, new Rect(0, 0, BuildingTexture.width, BuildingTexture.height), Vector2.one * 0.5f);
            if (hover) {mainImage.color = new Color(0.5f,0.5f,0.5f,(float)transparency/100);}
            else if (!canBuild) {mainImage.color = new Color(1f,0.6f,0.6f,(float)transparency/100);}
            else {mainImage.color = new Color(1f,1f,1f,(float)transparency/100);}
            mainImage.rectTransform.sizeDelta = new Vector2(size+2, size + 2);
            float deltay = (position.x+position.y)*0.3f + (size+2)/2 + 0.5f;//+ ((size+2)*0.6f) -((float)((int)Math.Ceiling(size / 2f))-0.5f)*0.3f;
            if (imageObjectPath.Contains("Tree/"))
            {
                mainImage.rectTransform.sizeDelta = new Vector2(size*2/3, size*2/3);
                deltay = (position.x+position.y)*0.3f + (size)*0.3f+0.1f;
            }
            transform.position = new Vector3((position.x-position.y)*0.5f,deltay,1);
            //buildingImage.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
            MakeVisible();
        }
        else 
        {
            /*Debug.Log("La tuile avec le nom " + imageObjectPath + subMode + "_Back n'a pas été trouvée dans les ressources.");*/ //TO DO : VERIFIER POURQUOI CA NE MARCHE PAS
            //S'il existe on vide l'image du main
            mainImage.color = new Color(0.5f,0.5f,0.5f,0f);
        }

        if (color != Color.white)
        {
            BuildingTexture = Resources.Load<Texture2D>(imageObjectPath + subMode + "_on");
            if (BuildingTexture != null)
            {
                onImage.enabled = true;
                onImage.sprite = Sprite.Create(BuildingTexture, new Rect(0, 0, BuildingTexture.width, BuildingTexture.height), Vector2.one * 0.5f);
                if (hover) {onImage.color = new Color(color.r/2,color.g/2,color.b/2,(float)transparency/100);}
                else {onImage.color = new Color(color.r/2,color.g/2,color.b/2,(float)transparency/100);}
                onImage.rectTransform.sizeDelta = new Vector2(size+2, size + 2);
                float deltay = (position.x+position.y)*0.3f + (size+2)/2 + 0.5f;//+ ((size+2)*0.6f) -((float)((int)Math.Ceiling(size / 2f))-0.5f)*0.3f;
                transform.position = new Vector3((position.x-position.y)*0.5f,deltay,1);
                //buildingImage.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
            }
            else {onImage.enabled = false;}
        }
    }

    public void ReLoadImage (string imageObjectPathInput, string subMode, int transparency, bool hover)
    {
        imageObjectPath = imageObjectPathInput;
        LoadImage(subMode, transparency, hover);
    }

    public void ReOrderImage (Vector2Int positionExtInput, bool insideInput, int delta)
    {
        inside = insideInput;
        positionExt = positionExtInput;
        OrderImage(positionExtInput, inside, delta);
    }

    public void ReLoadandOrderImage (string imageObjectPathInput, string subMode, int transparency, bool hover, Vector2Int positionExtInput, bool insideInput, int sizeInput, int delta)
    {
        imageObjectPath = imageObjectPathInput;
        inside = insideInput;
        size = sizeInput;
        positionExt = positionExtInput;
        LoadImage(subMode, transparency, hover);
        OrderImage(positionExtInput, inside, delta);
    }

    public void OrderImage (Vector2Int positionExtInput, bool inside, int delta)
    {
        canvas = gameObject.GetComponent<Canvas>();
        if (inside)
        {
            /*int delta = Math.Min(1,Math.Max(10,(positionExtInput.x + positionExtInput.y - (position.x + position.y)) + 6));*/
            /*int varia = (positionExtInput.x + positionExtInput.y - (position.x + position.y)) + 9 + delta;*/
            int varia = Math.Max(1,Math.Min(10,(positionExtInput.x + positionExtInput.y - (position.x + position.y)) + 5));
            /*Debug.LogError("positionExtInput.x : " + positionExtInput.x + " positionExtInput.y :" + positionExtInput.y + " position.x :"+position.x +" position.y :"+ position.y);*/
            /*Debug.LogError("varia"+ varia);*/
            /*canvas.sortingOrder = 10000 - 12 * (positionExtInput.x + positionExtInput.y) + ((int)size/2)*2 + delta;*/
            canvas.sortingOrder = 10000 - 12 * (positionExtInput.x + positionExtInput.y) - ((int)size/2)*2 + varia;
            int test = 10000 - 12 * (positionExtInput.x + positionExtInput.y) - ((int)size/2)*2 + varia;
            /*Debug.LogError("Calcul" + test);*/
        }
        else 
        {
            /*canvas.sortingOrder = 10000 - 12 * (position.x + position.y + ((int)size/2)*2) + order;*/
            canvas.sortingOrder = 10000 - 12 * (position.x + position.y + ((int)(size/2))) + order;
        }
    }

    public void ChangeOrderImage (int neworder)
    {
        order = neworder;
        canvas.sortingOrder = 10000 - 12 * (position.x + position.y + ((int)(size/2))) + order;
    }
    
    public void DestroyImage()
    {
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
        
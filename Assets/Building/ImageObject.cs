using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class ImageObject : MonoBehaviour
{
    /*public Building building;
    public Plant plant;*/
    public Tile tile;
    public Vector2Int position;
    public int order;
    public Vector2Int positionExt;
    public Image mainImage;// { set; get; }
    public Image onImage;
    private Canvas canvas;
    public int size;
    public string imageObjectPath;
    public Color color;
    public bool canBuild;

    public void Initialize(Building buildingInput, /*Plant plantInput,*/ Tile tileInput, string imageObjectPathInput, int orderInput, Vector2Int positionInput, Vector2Int positionExtInput, string subMode, int sizeInput, Color colorInput, int transparency) // typeCanvas = 0 avant, 1 arriere, 2 interieur
    {
        /*building = buildingInput;
        plant = plantInput;*/
        tile = tileInput;
        order = orderInput;
        position = positionInput;
        positionExt = positionExtInput;
        imageObjectPath = imageObjectPathInput;
        size = sizeInput;
        color = colorInput;
        canBuild = true;

        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        //canvas.sortingOrder = 20000 - 24 * (position.x + position.y + ((int)size/2)*2) + orderInput;
        canvas.sortingOrder = 10000 - 12 * (position.x + position.y + ((int)size/2)*2) + orderInput;
        mainImage = new GameObject("Main").AddComponent<Image>();
        mainImage.transform.SetParent(canvas.transform, false);
        if (color != Color.white)
        {
            onImage = new GameObject("On").AddComponent<Image>();
            onImage.transform.SetParent(canvas.transform, false);
        }
        LoadImage(subMode, transparency, false);
    }


    public void ChangePreview(string imageObjectPathInput, int sizeInput)
    {
        imageObjectPath = imageObjectPathInput;
        size = sizeInput;
        canvas = gameObject.GetComponent<Canvas>();
        canvas.sortingOrder = 1;
        LoadImage("", 90, false);
    }

    public void UpdatePreview(int orderInput, Vector2Int positionInput, Vector2Int positionExtInput, bool canBuildInput)
    {
        order = orderInput;
        position = positionInput;
        positionExt = positionExtInput;
        canBuild = canBuildInput;
        MoveImage();
    }

    public void MoveImage()
    {
        float deltay = (position.x+position.y)*0.3f + (size+2)/2 + 0.5f;//+ ((size+2)*0.6f) -((float)((int)Math.Ceiling(size / 2f))-0.5f)*0.3f;
        transform.position = new Vector3((position.x-position.y)*0.5f,deltay,1);
        if (!canBuild) {mainImage.color = new Color(1f,0.6f,0.6f,0.6f);}
        else {mainImage.color = new Color(1f,1f,1f,0.9f);}
        canvas = gameObject.GetComponent<Canvas>();
        canvas.sortingOrder = 10000 - 12 * (position.x + position.y + ((int)size/2)*2) + order;
    }
    public void LoadImage(string subMode, int transparency, bool hover)
    {
        Texture2D BuildingTexture = Resources.Load<Texture2D>(imageObjectPath + subMode);
        if (order == 0) {BuildingTexture = Resources.Load<Texture2D>(imageObjectPath + subMode + "_Back");}
        if (BuildingTexture != null)  
        {
            mainImage.sprite = Sprite.Create(BuildingTexture, new Rect(0, 0, BuildingTexture.width, BuildingTexture.height), Vector2.one * 0.5f);
            if (hover) {mainImage.color = new Color(0.5f,0.5f,0.5f,(float)transparency/100);}
            else if (!canBuild) {mainImage.color = new Color(1f,0.6f,0.6f,(float)transparency/100);}
            else {mainImage.color = new Color(1f,1f,1f,(float)transparency/100);}
            mainImage.rectTransform.sizeDelta = new Vector2(size+2, size + 2);
            float deltay = (position.x+position.y)*0.3f + (size+2)/2 + 0.5f;//+ ((size+2)*0.6f) -((float)((int)Math.Ceiling(size / 2f))-0.5f)*0.3f;
            transform.position = new Vector3((position.x-position.y)*0.5f,deltay,1);
            //buildingImage.rectTransform.sizeDelta = new Vector2(0.5f, 0.5f);
        }
        else {Debug.Log("La tuile avec le nom " + imageObjectPath + subMode + "_Back n'a pas été trouvée dans les ressources.");}
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


}
        
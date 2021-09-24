using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class RandomStackMono : MonoBehaviour
{
    public Text count;

    public Slider sSpeed;
    public Slider sMinSpaceX;
    public Slider sMaxSpaceX;
    public Slider sMinSpaceY;
    public Slider sMaxSpaceY;
    public Slider sMinCellLength;
    public Slider sMaxCellLength;
    public Text tSpeed;
    public Text tMinSpaceX;
    public Text tMaxSpaceX;
    public Text tMinSpaceY;
    public Text tMaxSpaceY;
    public Text tMinCellLength;
    public Text tMaxCellLength;

    public RectTransform cell;

    private RandomStack randomStack;
    // Start is called before the first frame update
    void Start()
    {
        randomStack = new RandomStack();
        randomStack.sprites = new List<Sprite>();
        string dir = Path.Combine(Application.streamingAssetsPath, "images");
        Debug.Log(dir);
        foreach (string file in Directory.GetFiles(dir))
        {
            if (!file.ToLower().EndsWith(".jpg") && !file.ToLower().EndsWith(".png"))
                continue;
            byte[] bytes = File.ReadAllBytes(file);
            Texture2D texture = new Texture2D(10, 10);
            texture.LoadImage(bytes);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            randomStack.sprites.Add(sprite);
        }
        cell.gameObject.SetActive(false);
        randomStack.cell = cell;

        randomStack.Reset();



        sSpeed.value = randomStack.speed;
        sMinSpaceX.value = randomStack.minSpaceX;
        sMaxSpaceX.value = randomStack.maxSpaceX;
        sMinSpaceY.value = randomStack.minSpaceY;
        sMaxSpaceY.value = randomStack.maxSpaceY;
        sMinCellLength.value = randomStack.cellMinLength;
        sMaxCellLength.value = randomStack.cellMaxLength;

        sSpeed.onValueChanged.AddListener((_value) =>
        {
            randomStack.speed = _value;
        });
        sMinSpaceX.onValueChanged.AddListener((_value) =>
        {
            randomStack.minSpaceX = (int)_value;
        });
        sMaxSpaceX.onValueChanged.AddListener((_value) =>
        {
            randomStack.maxSpaceX = (int)_value;
        });
        sMinSpaceY.onValueChanged.AddListener((_value) =>
        {
            randomStack.minSpaceY = (int)_value;
        });
        sMaxSpaceY.onValueChanged.AddListener((_value) =>
        {
            randomStack.maxSpaceY = (int)_value;
        });
        sMinCellLength.onValueChanged.AddListener((_value) =>
        {
            randomStack.cellMinLength= (int)_value;
        });
        sMaxCellLength.onValueChanged.AddListener((_value) =>
        {
            randomStack.cellMaxLength= (int)_value;
        });


        randomStack.CreateNewBatch();
    }

    // Update is called once per frame
    void Update()
    {
        randomStack.Update();

        count.text = randomStack.CellCount.ToString();
        tSpeed.text = randomStack.speed.ToString();
        tMinSpaceX.text = randomStack.minSpaceX.ToString();
        tMaxSpaceX.text = randomStack.maxSpaceX.ToString();
        tMinSpaceY.text = randomStack.minSpaceY.ToString();
        tMaxSpaceY.text = randomStack.maxSpaceY.ToString();
        tMinCellLength.text = randomStack.cellMinLength.ToString();
        tMaxCellLength.text = randomStack.cellMaxLength.ToString();
    }
}

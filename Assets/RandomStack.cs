using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class RandomStack
{
    public RectTransform cell;
    public float speed = -30;
    public int column = 100;
    public int minSpaceX = 60;
    public int minSpaceY = 60;
    public int maxSpaceX = 120;
    public int maxSpaceY = 120;
    public int cellMinLength = 200;
    public int cellMaxLength = 600;
    public int countOfBatch = 30;
    public List<Sprite> sprites { get; set; }
    public int CellCount
    {
        get
        {
            return cells_.Count;
        }
    }

    private int spriteIndex_ = 0;
    private int columnIndex_ = 0;
    private int columnWidth_ = 0;
    private Rect viewport_;

    private int[] columnTopLines_ { get; set; }
    private List<RectTransform> cells_ = new List<RectTransform>();
    private List<RectTransform> needDeleteCells_ = new List<RectTransform>();
    private float totalOffset_;

    private class CellAnchor
    {
        public int posX;
        public int posY;
        public int width;
        public int height;
        public bool visible;
    }

    public void Reset()
    {
        RectTransform container = cell.parent.GetComponent<RectTransform>();
        columnIndex_ = 0;
        columnWidth_ = (int)container.rect.width / column;
        viewport_ = container.rect;
        columnTopLines_ = new int[column];
        cells_.Clear();
        totalOffset_ = 0;
    }

    public void CreateNewBatch()
    {
        for (int i = 0; i < countOfBatch; i++)
        {
            addCell();
        }
    }

    public void Update()
    {
        float offset = Time.deltaTime * speed;
        totalOffset_ += offset;
        needDeleteCells_.Clear();
        float bottomPosY = float.MaxValue;
        foreach (RectTransform rt in cells_)
        {
            var pos = rt.anchoredPosition;
            pos.y -= offset;
            rt.anchoredPosition = pos;
            if (rt.anchoredPosition.y > viewport_.height * 2)
                needDeleteCells_.Add(rt);
            bottomPosY = Mathf.Min(bottomPosY, rt.anchoredPosition.y);
        }

        // 删除超出范围的节点
        foreach (var rt in needDeleteCells_)
        {
            cells_.Remove(rt);
            GameObject.DestroyImmediate(rt.gameObject);
        }

        // 补充节点
        if (bottomPosY > -(viewport_.height * 2))
        {
            CreateNewBatch();
        }
    }

    private void addCell()
    {
        Sprite sprite = sprites[spriteIndex_];
        string name = spriteIndex_.ToString();
        //Debug.LogWarningFormat("############ name: {0}", name);

        // 创建实例
        GameObject clone = GameObject.Instantiate<GameObject>(cell.gameObject);
        clone.name = spriteIndex_.ToString();
        clone.transform.SetParent(cell.parent);
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localRotation = Quaternion.identity;
        clone.gameObject.SetActive(true);
        var rtCell = clone.GetComponent<RectTransform>();
        cells_.Add(rtCell);
        clone.transform.Find("Text").GetComponent<Text>().text = clone.name;

        // 赋值图片
        Image image = clone.GetComponent<Image>();
        image.sprite = sprite;
        image.SetNativeSize();

        // 计算初始大小
        int length = Random.Range(cellMinLength, cellMaxLength);
        int cellWidth = 0;
        int cellHeight = 0;
        if (sprite.texture.width > sprite.texture.height)
        {
            cellWidth = length;
            cellHeight = fitHeight(sprite.texture.width, sprite.texture.height, length);
        }
        else
        {
            cellHeight = length;
            cellWidth = fitWidth(sprite.texture.width, sprite.texture.height, length);
        }

        // 设置位置和大小
        int spaceX = Random.Range(minSpaceX, maxSpaceX);
        int spaceY = Random.Range(minSpaceY, maxSpaceY);
        CellAnchor anchor = calculateAnchor(spaceX, spaceY, cellWidth, cellHeight);
        rtCell.anchoredPosition = new Vector2(anchor.posX, anchor.posY - totalOffset_);
        rtCell.sizeDelta = new Vector2(anchor.width, anchor.height);
        clone.SetActive(anchor.visible);

        spriteIndex_ += 1;
        if (spriteIndex_ >= sprites.Count - 1)
            spriteIndex_ = 0;
    }
    private CellAnchor calculateAnchor(int _spaceX, int _spaceY, int _cellWidth, int _cellHeight)
    {
        //Debug.LogFormat("space is ({0}, {1})", _spaceX, _spaceY);
        //Debug.LogFormat("size of cell is ({0}, {1})", _cellWidth, _cellHeight);

        CellAnchor anchor = new CellAnchor();
        anchor.width = _cellWidth;
        anchor.height = _cellHeight;
        anchor.visible = true;

        // 1: 计算当前节点的占据的起始列
        int startX = columnIndex_ * columnWidth_ + _spaceX;
        int endX = startX + _cellWidth;
        int startColumn = columnIndex_;
        int endColumn = (int)endX / columnWidth_ + 1;
        if (endColumn > column - 1)
            endColumn = column - 1;
        columnIndex_ = (endColumn + 1) % column;
        //Debug.LogFormat("range of column is ({0}, {1}), next column is {2}", startColumn, endColumn, columnIndex_);
        anchor.posX = startX;

        // 2: 获取范围列中的最低位置
        int bottomY = int.MaxValue;
        for (int i = startColumn; i <= endColumn; ++i)
        {
            bottomY = Mathf.Min(bottomY, columnTopLines_[i]);
       }
        anchor.posY = bottomY;

        int topLine = bottomY - _cellHeight - _spaceY;
        // 3：更新范围列的顶线位置
        for (int i = startColumn; i <= endColumn; ++i)
        {
            columnTopLines_[i] = topLine;
        }

        // 4: 缩小超出视窗外的节点
        if(anchor.posX + anchor.width + minSpaceX  > viewport_.width)
        {
            anchor.width = (int)viewport_.width - anchor.posX - minSpaceX;
            anchor.height = fitHeight(_cellWidth, _cellHeight, anchor.width);
            anchor.visible = anchor.width >= cellMinLength && anchor.height >= cellMinLength;
        }


        //printColumnTopLines();

        return anchor;
    }

    private int fitHeight(int _originWidth, int _originHeight, int _width)
    {
        return (int)(_width / (_originWidth * 1.0f / _originHeight));

    }

    private int fitWidth(int _originWidth, int _originHeight, int _height)
    {
        return (int)(_height * (_originWidth * 1.0f / _originHeight));
    }

    private void printColumnTopLines()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < column; i++)
        {
            sb.Append(string.Format("{0}:{1}, ", i, columnTopLines_[i].ToString()));
        }
        Debug.Log(sb.ToString());
    }
}

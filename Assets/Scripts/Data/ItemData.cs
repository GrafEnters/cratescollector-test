using System;
using UnityEngine;

[Serializable]
public class ItemData {
    public int ID;
    public string Name;
    public Color Color;
    public bool Stackable;
    public int MaxStack;

    public ItemData(int id, string name, Color color, bool stackable, int maxStack) {
        ID = id;
        Name = name;
        Color = color;
        Stackable = stackable;
        MaxStack = maxStack;
    }
}

[Serializable]
public class ItemDataColor {
    public float R;
    public float G;
    public float B;
    public float A = 1f;

    public Color ToColor() {
        return new Color(R, G, B, A);
    }
}

[Serializable]
public class ItemDataJson {
    public int ID;
    public string Name;
    public ItemDataColor Color;
    public bool Stackable;
    public int MaxStack;

    public ItemData ToItemData() {
        return new ItemData(ID, Name, Color.ToColor(), Stackable, MaxStack);
    }
}

[Serializable]
public class ItemsDataContainer {
    public ItemDataJson[] Items;
}
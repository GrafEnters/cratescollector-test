using UnityEngine;

[System.Serializable]
public class ItemData {
    public int id;
    public string name;
    public Color color;
    public bool stackable;
    public int maxStack;

    public ItemData(int id, string name, Color color, bool stackable, int maxStack) {
        this.id = id;
        this.name = name;
        this.color = color;
        this.stackable = stackable;
        this.maxStack = maxStack;
    }
}

[System.Serializable]
public class ItemDataColor {
    public float r;
    public float g;
    public float b;
    public float a = 1f;

    public Color ToColor() {
        return new Color(r, g, b, a);
    }
}

[System.Serializable]
public class ItemDataJson {
    public int id;
    public string name;
    public ItemDataColor color;
    public bool stackable;
    public int maxStack;

    public ItemData ToItemData() {
        return new ItemData(id, name, color.ToColor(), stackable, maxStack);
    }
}

[System.Serializable]
public class ItemsDataContainer {
    public ItemDataJson[] items;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeItem
{
    public string name;
    public TreeItem parent;
    public List<TreeItem> children;

    public TreeItem(string name, TreeItem parent = null)
    {
        this.name = name;
        this.parent = parent;
        this.children = new();
    }
}

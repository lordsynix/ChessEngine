using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameTreeManager : MonoBehaviour
{
    public GameObject contentParent;
    public GameObject moveRowPrefab;
    public GameObject movePrefab;

    private TreeItem rootItem;

    private void Start()
    {
        rootItem = new("Root");

        TreeItem child1 = new("Child 1", rootItem);
        TreeItem child2 = new("Child 2", rootItem);

        rootItem.children.Add(child1);
        rootItem.children.Add(child2);

        LoadTree();
    }

    public void LoadTree()
    {
        var rootRow = Instantiate(moveRowPrefab, contentParent.transform);
        var move = Instantiate(movePrefab, rootRow.transform);

        move.transform.GetChild(0).GetComponent<Text>().text = rootItem.name;

        if (rootItem.children == null) return;

        var childrenRow = Instantiate(moveRowPrefab, contentParent.transform);

        foreach (TreeItem item in rootItem.children)
        {
            var child = Instantiate(movePrefab, childrenRow.transform);

            child.transform.GetChild(0).GetComponent<Text>().text = item.name;
        }
    }
}

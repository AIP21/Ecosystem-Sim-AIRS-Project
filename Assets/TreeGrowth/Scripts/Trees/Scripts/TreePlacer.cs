using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TreePlacer : MonoBehaviour
{
    public TreeGenerator TreeGenerator;
    public bool Generate = false;

    public void Update()
    {
        if (Generate)
        {
            Generate = false;

            GenerateTree(TreeGenerator.transform.position);
        }
    }

    public void GenerateTree(Vector3 position)
    {
        this.TreeGenerator.StartCoroutine(TreeGenerator.BuildCoroutine());
    }
}
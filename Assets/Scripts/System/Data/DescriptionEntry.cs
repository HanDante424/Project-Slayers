using System;
using UnityEngine;

[Serializable]
public class DescriptionEntry
{
    public string label;

    [TextArea(3, 8)]
    public string text;
}

using System.Collections.Generic;
using UnityEngine;

public class Category : MenuComp {
    // Contient la liste des GameObjects 
    public List<string> listAttachedElement = new List<string>(); 
    // Cache ou montre la liste associ�
    public bool hideList = false;
}
using UnityEngine;
using System.Collections.Generic;

public class Competence : MonoBehaviour {
	// Comp�tence impl�ment�
	public bool active = true;
	// Information sur la comp�tene
	public string info;
	// Liste des niveaux contenant la comp�tence
	public List<string> listLevel;
	// Quel competence coch� si celle-ci est selectionner
	public List<string> compLink;
	// Quel comp�tence griser si celle-ci est coch�
	public List<string> compNoPossible;
}
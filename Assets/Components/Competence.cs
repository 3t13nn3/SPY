using UnityEngine;
using System.Collections.Generic;

public class Competence : MenuComp {
	// Comp�tence impl�ment�
	public bool active = true;
	// Liste des niveaux contenant la comp�tence
	public List<string> listLevel;
	// Quel competence coch� si celle-ci est selectionner
	public List<string> compLink;
	// Quel comp�tence griser si celle-ci est coch�
	public List<string> compNoPossible;
	// Quel comp�tence d�coch� si celle-ci est d�coch�
	public List<string> compLinkUnselect;
}
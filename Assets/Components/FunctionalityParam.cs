using System.Collections.Generic;
using UnityEngine;

public class FunctionalityParam : MonoBehaviour {
	// Fonctionalit� impl�ment�e
	public Dictionary<string,bool> active = new Dictionary<string, bool>();
	// Fonctionalit� bas�e sur le level design
	public Dictionary<string, bool> levelDesign = new Dictionary<string, bool>();
	// Quelles sont les fonctionalit�s � activer si on active celle-ci
	public Dictionary<string, List<string>> activeFunc = new Dictionary<string, List<string>>();
	// Quelles sont les fonctionalit�s � desactiver si on active celle-ci
	public Dictionary<string, List<string>> enableFunc = new Dictionary<string, List<string>>();
	// Quelles sont les fonctionnalit�s qui doivent �tre activ�es dans le niveau selon les comp�tences selectionn�es par l'utilisateur
	public List<string> funcActiveInLevel = new List<string>();
	// Lors de l'activation de certaines fonctions, quels sont les �l�ments dont les limites sont � v�rifier
	public Dictionary<string, List<string>> elementRequiermentLibrary= new Dictionary<string, List<string>>();
	// Liste des capteurs
	public List<string> listCaptor = new List<string>();
}
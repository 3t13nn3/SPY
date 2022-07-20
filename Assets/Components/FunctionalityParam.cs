using System.Collections.Generic;
using UnityEngine;

public class FunctionalityParam : MonoBehaviour {
	// Fonctionalit� impl�ment�
	public Dictionary<string,bool> active = new Dictionary<string, bool>();
	// Fonctionalit� bas� sur le level design
	public Dictionary<string, bool> levelDesign = new Dictionary<string, bool>();
	// Quel sont les fonctionalit�s � activ� si on active celle-ci
	public Dictionary<string, List<string>> activeFunc = new Dictionary<string, List<string>>();
	// Quel sont les fonctionalit�s � desactiv� si on active celle-ci
	public Dictionary<string, List<string>> enableFunc = new Dictionary<string, List<string>>();
	// Quel sont les fonctionnalit�s qui doivent �tre activ�es dans le niveau selon les comp�tences selectionn�es par l'utilisateur
	public List<string> funcActiveInLevel = new List<string>();
	// Lors de l'activation de certaine fonction, quel sont les �l�ments dont les limites sont � v�rifi�
	public Dictionary<string, List<string>> elementRequiermentLibrary= new Dictionary<string, List<string>>();
	// List des capteurs
	public List<string> listCaptor = new List<string>();
}
using UnityEngine;
using System.Collections.Generic;

public class Competence : MenuComp {
	// Comp�tence impl�ment�
	public bool active = true;
	// A quelle fonction est li�e la comp�tence
	public List<string> compLinkWhitFunc = new List<string>();
	// A quel comp�tence est li�e la comp�tence
	public List<string> compLinkWhitComp = new List<string>();
	// List des comp�tences dont au moins une doit �tre selectionn�e
	public List<string> listSelectMinOneComp = new List<string>();
}
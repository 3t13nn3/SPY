using UnityEngine;
using System.Collections.Generic;

public class Competence : MenuComp {
	// Comp�tence impl�ment�
	public bool active = true;
	// A quel fonction est li� la comp�tence
	public List<string> compLinkWhitFunc = new List<string>();
	// A quel comp est li� la comp�tence
	public List<string> compLinkWhitComp = new List<string>();
	// List des comp dont au moins une doit �tre selectionn�
	public List<string> listSelectMinOneComp = new List<string>();
}
using UnityEngine;
using System.Collections.Generic;

public class Competence : MenuComp {
	// Comp�tence impl�ment�
	public bool active = true;
	// A quel fonction est li� la comp�tence
	public List<string> compLinkWhitFunc;
	// A quel comp est li� la comp�tence
	public List<string> compLinkWhitComp;
}
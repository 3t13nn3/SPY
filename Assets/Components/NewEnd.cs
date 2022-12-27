using UnityEngine;

public class NewEnd : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public static int Detected = 1;
	public static int Win = 2;
	public static int BadCondition = 3;
	public static int NoMoreAttempt = 4;

	// rajouter un nouveau newend pour la cas si plus de vie 
	public static int Dead = 5;

	public int endType;
}
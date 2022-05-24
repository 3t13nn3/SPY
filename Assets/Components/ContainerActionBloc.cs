using UnityEngine;

public class ContainerActionBloc : MonoBehaviour {
	// Advice: FYFY component aims to contain only public members (according to Entity-Component-System paradigm).
	public bool blockSpecial = false; // Determine si le container et un container pr�sent au sein d'un bloc If ou for
	public bool containerCondition = false; // Determine si le container doit contenir des conditons et op�rateurs uniquement ou bien seulement des actions
}
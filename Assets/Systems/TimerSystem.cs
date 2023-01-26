using UnityEngine;
using FYFY;
using Debug = UnityEngine.Debug;

public class TimerSystem : FSystem {
	//system qui permet de calculer le temps pris pour chaque niveau effectué

	public static TimerSystem instance;
	public static double beginTimer = 0;
	public static double duration = 0;

	public TimerSystem()
	{
		instance = this;
	}
	
	
	public static void pauseTimer()
	{
		duration = Time.timeAsDouble - beginTimer;
	}

	public static void resumeTimer()
	{
		beginTimer = Time.timeAsDouble;
	}
}
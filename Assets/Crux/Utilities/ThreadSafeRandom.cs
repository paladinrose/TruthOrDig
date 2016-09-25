using System;
using UnityEngine;
public class ThreadSafeRandom
{
	static ThreadSafeRandom _random;
	public static ThreadSafeRandom Random{
		get{ if(_random == null){_random = new ThreadSafeRandom();} return _random;}
	}

	public System.Random random;

	public double lowerLimit, upperLimit;
	double diff;
	public ThreadSafeRandom ()
	{
		random = new System.Random ();
		lowerLimit = 0; upperLimit = 1;
		diff = 1;
	}
	public ThreadSafeRandom(int i)
	{
		random = new System.Random (i);
		lowerLimit = 0; upperLimit = 1;
		diff = 1;
	}

	public double GetValue()
	{
		double value = random.NextDouble ();
		value *= diff;
		value += lowerLimit;
		return value;
	}

	public int GetInt(int l, int h)
	{
		return random.Next (l, h);
	}
	public void SetRange(double l, double u)
	{
		lowerLimit = l;
		upperLimit = u;
		diff = upperLimit - lowerLimit;
	}
}



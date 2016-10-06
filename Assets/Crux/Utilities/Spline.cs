using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

namespace Crux{
	[System.Serializable]
	public class Spline
	{

		#if UNITY_EDITOR
		public string name;
		public Color color;
		#endif

        [SerializeField]
		Vector[] points = new Vector[0];

		public Vector this[int index]{
			get { return points [index]; }
			set { points [index] = value; }
		}

		public int Length {
			get { return points.Length; }
		}

		Vector startingControlPoint, endingControlPoint, a, b, c, d;

		bool initialized = false;

        
		int sections;

		int currentSection = -1;

		public int defaultSectionType = 1;
		public float defaultWeight = 1f;
		public float defaultDropValue;

		public int[] sectionTypes;
		public float[] sectionWeights;

		
		public float weightedPercent, totalWeight;


		public bool wrap, splineType;

		public Spline()
		{
			
			defaultSectionType = 1;
			defaultWeight = 1f;
			defaultDropValue = 0;
			points = new Vector[0];
			sectionTypes = new int[0];
			sectionWeights = new float[0];
			Sections = 0;

			splineType = true;
		}
		public Spline(int p)
		{
			if(p <=1){p=2;}
			//Debug.Log ("Spline : " + p);
			points = new Vector[p];
			float inv = 1f / p;
			for (int i = p - 1; i >= 0; i--) {
				points [i] = new Vector (1);
				points [i] [0] = inv * i;
			}

			defaultSectionType = 1;
			defaultWeight = 1f;
			defaultDropValue = 0;
			sectionTypes = new int[0];
			sectionWeights = new float[0];
			Sections = p - 1;
		}

		public Spline(Spline spline, int p){
            if(p < 2) { p = 2; }
			points = new Vector[p];
			sectionWeights = new float[p-1];
			sectionTypes = new int[p-1];
			float per = 1f/p;
			int s;
            if (p > 2)
            {
                points[0] = spline.points[0];
                sectionWeights[0] = spline.sectionWeights[0];
                sectionTypes[0] = spline.sectionTypes[0];
                for (int i = 1; i < p-1; i++)
                {
                    s = spline.GetSectionAtPercent(per * i, true);
                    
                    points[i] = spline.GetPoint(per * i, true);
                    sectionWeights[i] = spline.sectionWeights[s];
                    sectionTypes[i] = spline.sectionTypes[s];
                    
                }
                points[p-1] = spline.points[spline.Length - 1];
            }
            else {
                points[0] = spline.points[0];
                sectionWeights[0] = spline.sectionWeights[0];
                sectionTypes[0] = spline.sectionTypes[0];
                points[1] = spline.points[spline.Length-1];
            }
		}
		public int Sections
		{
			get { return sections; }
			set {
				sections = value;
				if (sections >= Length) {
					sections = Length - 1;
				} 
				if (sections > 0) {
					int[] newSectionTypes = new int[sections];
					float[] newsectionWeights = new float[sections];
					totalWeight = 0;
					int lesser = sections;
					if (sectionTypes.Length < lesser) {
						lesser = sectionTypes.Length;
					}
					for (int i = lesser - 1; i >= 0; i--) {
						newSectionTypes [i] = sectionTypes [i];
						newsectionWeights [i] = sectionWeights [i];
						totalWeight += sectionWeights [i];
					}
					if (sections > lesser) {
						for (int i = lesser; i < sections; i++) {
							newSectionTypes [i] = defaultSectionType;
							newsectionWeights [i] = defaultWeight;
							totalWeight += defaultWeight;
						}
					}
					sectionTypes = newSectionTypes;
					sectionWeights = newsectionWeights;
				}
			}
		}

        public float GetPercentAtPoint(int p)
        {
            float per = 0;
            //We know our totalWeight
            //Our percent at 0 is 0.
            //Our percent at points.Length-1 is 1

            for(int i = 0; i < p; i++)
            {
                per += sectionWeights[i];
            }
            per = per / totalWeight;
            return per;
        }
		public int GetSectionAtPercent(float p, bool weighted = true)
		{
			int i;
			if(wrap)
			{
				if(p >1){i = Mathf.FloorToInt(p); p-=i;}
				else if(p < 0){i = Mathf.Abs(Mathf.FloorToInt(p)); p +=i;}
			}
            
                //Debug.Log (totalWeight);
                if (p >= 0 && p <= 1)
                {
                    float currentWeight = 0f, c;
                    float percentAsWeight = p * totalWeight;
                
                    for (i = 0; i < sections; i++)
                    {
                        c = currentWeight + sectionWeights[i];
                    //Debug.Log(percentAsWeight + ","+currentWeight + ","+c);
                    if (currentWeight <= percentAsWeight && c > percentAsWeight)
                        {
                            //What we want is the percent WITHIN the current section that our overall percent is at.
                            //Right now, currentWeight is less than or equal to our percent of our total tension.
                            //We subtract our current weight FROM our percent of total. This difference is a percentage of total weight. Namely, it's
                            //the percentage of total weight between current weight and our percentage.
                            //So difference/total weight = % / current weight.
                            //difference*current weight/total weight = %
                            weightedPercent = (percentAsWeight - currentWeight) * sectionWeights[i] / totalWeight;
                            return i;
                        }
                        else
                        {
                            currentWeight = c;
                        }
                    }
                }
                else
                {
                    weightedPercent = 0;
                    return 0;
                }
            
			return currentSection;
		}	


		public void SetSplinePoints(Vector[] p)
		{
			points = p;
			Sections = points.Length-1;
		}

		public void SetSlotOnPoints(float[] vals, int id = 0)
		{
			for(int i=0; i < vals.Length; i++)
			{
				if(i < points.Length)points[i].Add(vals[i], id);
			}
		}

		public void AddPoints(Vector[] p, int[] s, float[] t, int id = 0)
		{
			//Revamp this to do all the adding work itself, instead of repeatedly calling AddPoint.

			for(int i=0; i < p.Length; i++)
			{
				AddPoint(p[i], id+i, defaultSectionType, defaultWeight);
			}
		}

		public void AddPoint(Vector p, int id = 0, int s = -1 ,float t = -1f)
		{
			if (s < 0) {
				s = defaultSectionType;
			}
			if (t < 0) {
				t = defaultWeight;
			}

			id--;
			int npc = points.Length;
			int fillerSlots =0;
			if(id > npc)fillerSlots= id-npc;

			List<Vector> newPoints = new List<Vector>();
			List<float> newT = new List<float>();
			List<int> newSS = new List<int>();

			if(npc >0)
			{
				for(int i=0; i < npc; i++)
				{
					if(i == id)
					{newPoints.Add(p); newSS.Add(s); newT.Add(t);}

					newPoints.Add(points[i]);
					if(i < npc-1)
					{
						newSS.Add(sectionTypes[i]);
						newT.Add(sectionWeights[i]);
					}
				}
				if(id <0)
				{newPoints.Add(p); newSS.Add(s); newT.Add(t);}
			}
			else 
			{
				newPoints.Add(p); newSS.Add(s); newT.Add(t);
			}
			for(int i=0; i < fillerSlots; i++)
			{
				newPoints.Add(new Vector()); newSS.Add(defaultSectionType); newT.Add(defaultWeight);
			}

			points = newPoints.ToArray();
			sections = points.Length - 1;
			sectionTypes = newSS.ToArray();
			sectionWeights = newT.ToArray(); 
			newPoints.Clear ();
			newSS.Clear ();
			newT.Clear ();

			SetCapPoints();
		}

		public void RemovePoint(int id)
		{
			id--;
			if(id < 0 || id >= points.Length)
			{
				List<Vector> newPoints = new List<Vector>();
				List<float> newsectionWeights = new List<float>();
				List<int> newSmooth = new List<int>();

				for(int i=0; i < points.Length; i++)
				{
					if(i != id)
					{
						if(i != points.Length-1){newsectionWeights.Add(sectionWeights[i]); newSmooth.Add(sectionTypes[i]);}
						newPoints.Add(points[i]);
					}
				}
				points = newPoints.ToArray();	
				sections = points.Length - 1;
				sectionTypes = newSmooth.ToArray();
				sectionWeights = newsectionWeights.ToArray();
				newPoints.Clear();
				newSmooth.Clear();
				newsectionWeights.Clear();
			}
		}
		public void RemovePoints(int[] ids)
		{
			List<Vector> newPoints = new List<Vector>();
			List<float> newsectionWeights = new List<float>();
			List<int> newSmooth = new List<int>();

			for(int i=0; i < points.Length; i++)
			{
				bool found = false;
				for(int j = 0 ; j < ids.Length; j++)
				{
					if(i == ids[j])found=true;
				}
				if(!found)
				{
					newPoints.Add(points[i]);
					if(i != points.Length-1){newsectionWeights.Add(sectionWeights[i]); newSmooth.Add(sectionTypes[i]);}
				}
			}
			points = newPoints.ToArray();	
			sections = points.Length - 1;
			sectionTypes = newSmooth.ToArray();
			sectionWeights = newsectionWeights.ToArray(); 
			newPoints.Clear ();
			newSmooth.Clear ();
			newsectionWeights.Clear ();
		}

		public void SetCapPoints()
		{
			//First, how many points do we actually have?
			int l = points.Length;

			//We need at least two points.
			if(l >1){
				//Our starting control point is the difference between point 0 and point 1 subtracted from point 0.
				Vector pointDiff = new Vector(points[1]); pointDiff -= points[0];
				startingControlPoint = new Vector(points[0]); startingControlPoint -= pointDiff;

				if(l >=3){
					//We check to see if this is a loop - if the first and last official points are the same.
					// If so, we set our control points to form that perfect, continuous loop.
					if(points[0] == points[l-1]){
						startingControlPoint = points[l-2];
						endingControlPoint = points[1];
					}
					//If not, we create our two control points. Each is just a straight line from our second set of points, going inward, through our first 
					//set of points.	
					else{
						pointDiff = new Vector(points[l-1]); pointDiff -= points[l-2];
						endingControlPoint = new Vector(points[l-1]); endingControlPoint += pointDiff;
					}
				}
				else{
					endingControlPoint = new Vector(points[1]); endingControlPoint += pointDiff;
				}
			}
		}

		public Vector GetPoint(float percent, bool weighted = true)
		{
			if (!initialized) {
                Sections = Length - 1;
				SetCapPoints ();
				initialized = true;
			}
			Vector currentPoint;
			if (points.Length > 0) {
				if (points.Length > 1) {

					if (percent <= 0) {
						currentPoint= points [0];
					} else if (percent >= 1) {
						currentPoint=  points [points.Length - 1];
					}

					int cS = GetSectionAtPercent (percent, weighted);
					//Debug.Log (points[cS].Length + " " + cS.ToString () + " " + points[cS].defaultValue);
					//Debug.Log (percent.ToString() + "," + cS.ToString ());
					

					if (cS != currentSection) {
						currentSection = cS;
						GetControlPoints ();
					}
                    Vector point = new Vector(points[cS].Length, points[cS].defaultValue);
                    switch (sectionTypes [cS]) {
					//Catmull Rom
					case 0:
						for (int i=0; i < point.Length; i++) {
							point [i] = CatmullRom (weightedPercent, a [i], b [i], c [i], d [i]);
						}
						break;

						//Linear
					case 1:
                        point = Vector.Lerp(points[cS], points[cS + 1], weightedPercent);

						break;

						//CubicBezier
					case 2:
						for (int i=0; i < point.Length; i++) {
							point [i] = CubicBezier (weightedPercent, a [i], b [i], c [i], d [i]);
						}
						break;

						//Step
					case 3:
						point = new Vector (points [cS]);
						break;
					}

					currentPoint = point;
				} else {
					currentPoint = points [0];
				}
			} 
			else 
			{
				currentPoint = new Vector ();
			}
			return currentPoint;
		}

		public Spline SplitSpline(int id, bool includeCurrent)
		{

			if (points.Length > id) {
				Spline leftOvers = new Spline ();
				List<Vector> newPoints = new List<Vector> ();
				List<int> newSecSmooth = new List<int> ();
				List<float> newTens = new List<float> ();
				if (includeCurrent) {
					leftOvers.AddPoint (new Vector ());
				}
				for (int i=0; i < points.Length; i++) {
					if (i < id) {
						newPoints.Add (points [i]);
						newSecSmooth.Add (sectionTypes [i]);
						newTens.Add (sectionWeights [i]);
					} else {
						leftOvers.AddPoint (points [i]);
						if (leftOvers.sections > 0) {
							leftOvers.sectionTypes [i] = sectionTypes [i];
							leftOvers.sectionWeights [i] = sectionWeights [i];
						}
					}
				}

				points = newPoints.ToArray ();
				sections = points.Length - 1;
				sectionTypes = newSecSmooth.ToArray ();
				sectionWeights = newTens.ToArray ();
				newPoints.Clear();
				newSecSmooth.Clear();
				newTens.Clear();
				return leftOvers;
			} else 
			{
				return new Spline();
			}
		}

		public void GetControlPoints()
		{
			if(currentSection == 0 && sections >1)
			{
				a = startingControlPoint;
				b = points[0];
				c = points[1];
				d = points[2];
			}
			else if(currentSection == 0)
			{
				a = startingControlPoint;
				b = points[0];
				c = points[1];
				d = endingControlPoint;
			}
			else if(currentSection == sections-1)
			{
				a = points[sections-2];
				b = points[sections-1];
				c = points[sections];
				d = endingControlPoint;
			} 
			else if(currentSection >0)
			{
				a = points[currentSection-1];
				b = points[currentSection];
				c = points[currentSection+1];
				d = points[currentSection+2];
			}
		}


		public Vector ClosestPointOnSpline(Vector point, int samples)
		{
			//var cP:W_Vector;
			int closestSeg = -1;
			float currentDistance = 0;

			for(int p=0; p < points.Length; p++)
			{
				float newDist = Vector.Distance(point, points[p]);
				if(p == 0){currentDistance = newDist;}


				if(newDist < currentDistance)
				{
					closestSeg = p;
					currentDistance = newDist;
				}
			}
			int p1 = 0;
			int p2 = 0;
			if(closestSeg >0 && closestSeg < points.Length-1)
			{

				p1 = closestSeg-1;
				p2 = closestSeg+1;
			}
			else if(closestSeg == 0)
			{
				p1 = 0;
				p2 = 2;
			}
			else if(closestSeg == points.Length-1)
			{
				p1 = points.Length-3;
				p2 = points.Length-1;
			}

			float dist1 = Vector.Distance(point, points[p1]);
			float dist2 = Vector.Distance(point, points[p2]);

			if(dist1 <dist2)
			{
				closestSeg = p1;
			}

			float segPercent = 1f/sections;
			float currentP = segPercent*closestSeg;
			float sample = segPercent/samples;
			List<Vector> samplePoints = new List<Vector>();

			for(int q = 0; q < samples; q++){
				samplePoints.Add(GetPoint( currentP + (q*sample) ));
			}
			Vector[] sP = samplePoints.ToArray();
			samplePoints.Clear ();

			int closestPoint = -1;
			float cDist = 0f;
			for(int sne = 0; sne < sP.Length; sne++)
			{
				float dist = Vector.Distance(sP[sne],point);
				if(sne == 0) {cDist = dist;}

				if(dist < cDist)
				{
					closestPoint = sne;
					cDist = dist;
				}	
			}

			return sP[closestPoint];
		}


		public float SplineLength(float percent)
		{

			float splineLength = 0f;
			if(sections >0)
			{
				Vector prevPoint = points[0];

				for(float i = 0f; i <=1; i+=percent)
				{
					Vector currentPoint = GetPoint(i);
					splineLength += Vector.Distance(prevPoint, currentPoint);
					prevPoint = currentPoint;
				}
			}
			return splineLength;
		}
		public Vector AveragePoints()
		{

			int id = 0;
			bool hasSlot = true;
			Vector average = new Vector();
			float pointAv = 0f;
			int div = 0;
			while(hasSlot)
			{
				pointAv=0f;
				div=0;
				for(int i =0; i < points.Length; i++)
				{
					if(points[i].Length < id)
					{
						div++;
						pointAv+= points[i][id];
					}
				}
				if(div >0)
				{
					average.Add(pointAv/div);
					id++;
				}
				else
				{
					hasSlot = false;
				}
			}
			return average;
		}

		public float CatmullRom(float t, float pa, float pb, float pc, float pd)
		{
			
            return 0.5f*((-pa+3f*pb-3f*pc+pd)*(t*t*t)+(2f*pa-5f*pb+4f*pc-pd)*(t*t)+(-pa+pc)*t+(2f*pb));
		}
		public float CubicBezier(float t, float pa, float pb, float pc, float pd)
		{
			return pa*((1f-t)*(1f-t)*(1f-t))+pb*3f*((1f-t)*(1f-t))*t+pc*3f*(1f-t)*(t*t)+pd*(t*t*t);
		}

		public void ResamplePoints(float dropVal)
		{
			ResamplePoints(dropVal, points.Length);
		}

		public void ResamplePoints(float dropVal, int samples)
		{
			if(samples >2)
			{

				int highest=-1;
				float perc = 1f/(samples-1);
				//var pj:float;

				for(int i=0; i < points.Length; i++)
				{if(points[i].Length > highest)highest = points[i].Length;}

				Vector[] newPoints = new Vector[samples];
				for(int i=0; i < samples; i++)
				{
					newPoints[i] = new Vector(highest);
					newPoints[i].defaultValue = dropVal;
				}
				Spline resampler; 
				for(int i = 0; i < highest; i++)
				{
					//All our points are sized to the point with the greatest number of dimensions, in all of the points we're resampling.
					//We go through from 0 to that length with all of the following, setting each new points values all the way through.
					resampler = new Spline();
					for(int j=0; j < points.Length; j++)
					{
						//Now we go through our original points.
						//If the point has enough dimensions to sample from, and if that value isn't the dropVal...
						if(points[j].Length >i)
						{
							if(points[j][i] != dropVal)
							{
								//We add that points value to a spline we're building to resample from.
								Vector rs = new Vector(1);
								rs.defaultValue = dropVal;
								rs[0] = points[j][i];
								resampler.AddPoint(rs);
							}
						}
					}

					//Finally, with our resample spline builtin, we go through each of our new points and sample
					//their values for that slot off of the resample spline.

					newPoints[0][i] = resampler.points[0][0];
					for(int j =1; j < samples; j++){
						newPoints[j][i] = resampler.GetPoint(perc*j)[0];
					}
				}
				points = newPoints;
				Sections = points.Length-1;
			}
		}

		public void ResampleSpline(int p)
		{
			if(p >1)
			{
				Vector[] newPoints = new Vector[p];
				float[] newsectionWeights = new float[p-1];
				int[] newSm = new int[p-1];
				float per = 1f/p;
				int s;
				for(int i=0; i < p; i++)
				{
					s = GetSectionAtPercent(per*i);

					newPoints[i] = GetPoint(per*i);
					if(i < p)
					{
						newsectionWeights[i] = sectionWeights[s];
						newSm[i] = sectionTypes[s];
					}
				}
				points = newPoints;
				sectionWeights = newsectionWeights;
				sectionTypes = newSm;
			}
		}

		public float Distance(float p1, float p2)
		{
			return Distance(p1, p2, 20);
		}
		public float Distance(float p1, float p2, int samples)
		{
			float distance = 0f;
			float p = p2-p1/samples;
			float cP = p1;
			Vector currentPoint =GetPoint(p1);
			Vector prevPoint = currentPoint; 

			for(int i=samples-1; i >=0; i--)
			{
				cP+=p;
				currentPoint = GetPoint(cP);
				distance += Vector.Distance(prevPoint, currentPoint);
				prevPoint = currentPoint;
			}
			return distance;
		}
	}
}


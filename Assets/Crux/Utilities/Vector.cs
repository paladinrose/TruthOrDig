using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Crux {
	[System.Serializable]
	public struct Vector
	{
		/// <summary>
		/// The n-dimensional values for this vector.
		/// </summary>
        [SerializeField]
		float[] dimensions;

		public float this [int index] {
			get {
				if (index < dimensions.Length && index >= 0) {
					return dimensions [index];
				} else {
					return -1f;
				} }
			set { if (index < dimensions.Length && index >= 0) {
					dimensions [index] = value;
			}
			}
		}

		/// <summary>
		/// Gets the dimensional length of this vector.
		/// </summary>
		/// <value>The dimensional length.</value>
		public int Length {
			get { return dimensions.Length;}
		}

		/// <summary>
		/// The default value of each dimension, if one is added with no value specified.
		/// </summary>
		public float defaultValue;
		
		//CONSTRUCTORS//
		/// <summary>
		/// Initializes a new instance of the <see cref="Vector"/> struct.
		/// </summary>
		/// <param name="df">Default Value.</param>
		public Vector(float df){
			defaultValue = df;
			dimensions = new float[1];
			dimensions[0] = defaultValue;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector"/> struct.
		/// </summary>
		/// <param name="length">Vector Length.</param>
		public Vector(int length){
			defaultValue = 0f;
			dimensions = new float[length];
			for (int i = length-1; i >=0; i--) {dimensions[i] = defaultValue;}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector"/> struct.
		/// </summary>
		/// <param name="length">Vector Length.</param>
		/// <param name="val">Default Value.</param>
		public Vector(int length, float val){
			dimensions = new float[length];
			defaultValue = val;
			for (int i = length-1; i >=0; i--) {dimensions[i] = defaultValue;}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Vector"/> struct.
		/// </summary>
		/// <param name="v">Base Vector.</param>
		public Vector(Vector v){
			dimensions = new float[v.Length];
			defaultValue = v.defaultValue;
			for (int i = v.Length-1; i >=0; i--) {dimensions[i] = v[i];}
		}

		//LIST MANAGEMENT METHODS//

		/// <summary>
		/// Add the specified point.
		/// </summary>
		/// <param name="p">Point.</param>
		public void Add(float p){
			Array.Resize  (ref dimensions, Length+1);
			dimensions [Length - 1] = p;
		}

		/// <summary>
		/// Add the specified dimension at the specified id in the vector.
		/// </summary>
		/// <param name="d">Dimension to add.</param>
		/// <param name="id">List Identifier.</param>
		public void Add(float d,int id)
		{
			
			Array.Resize (ref dimensions, Length+1);
			
			if (id < 0 || id >= Length - 1) {
				dimensions [Length - 1] = d;
			} else {
				for(int i =Length-1; i > id; i--)
				{
					dimensions[i] = dimensions[i-1];
				}
				dimensions[id] = d;
			}
		}

		/// <summary>
		/// Add the specified dimensions.
		/// </summary>
		/// <param name="d">Dimensions to add.</param>
		public void Add(float[] d)
		{
			int l = Length, newDL = Length + d.Length;
			Array.Resize (ref dimensions, newDL);
			for (int i = 0; i < d.Length; i++) 
			{
				dimensions[i+l] = d[i];
			}
		}

		/// <summary>
		/// Add the specified dimensions starting at id.
		/// </summary>
		/// <param name="d">Dimensions to add.</param>
		/// <param name="id">Starting Identifier.</param>
		public void Add(float[] d, int id)
		{
			
			int l = Length, newDL = l + d.Length;
			int diff;

			Array.Resize (ref dimensions, newDL);
			if (id < 0 || id >= l) 
			{
				for (int i = 0; i < d.Length; i++) 
				{
					dimensions [i + l] = d [i];
				}
			} 
			else 
			{
				diff = l-id;
				for(int i = newDL-1; i >=newDL-diff; i--)
				{
					dimensions[i] = dimensions[i-diff];
				}
				for(int i= 0; i < d.Length; i++)
				{
					dimensions[i+id] = d[i];
				}
			}
		}

		/// <summary>
		/// Remove the specified dimension at id.
		/// </summary>
		/// <param name="id">Identifier of dimension to remove.</param>
		public void Remove(int id)
		{
			id --;
			float[] newDimensions = new float[Length - 1];
			if(id >= 0 && id < Length)
			{
				for(int i= Length- 1; i >id; i--)
				{
					newDimensions[i-1] = dimensions[i];
				}
				for(int i=id-1; i >=0; i--)
				{
					newDimensions[i] = dimensions[i];
				}
				dimensions = newDimensions;
			}
		}

		/// <summary>
		/// Remove the dimensions at specified ids.
		/// </summary>
		/// <param name="ids">Identifiers of dimensions to remove.</param>
		public void Remove(int[] ids)
		{
			List<float> newDimensions = new List<float>();
			
			for(int i=0; i < Length; i++)
			{
				bool found = false;
				for(int j=0; j < ids.Length; j++)
				{
					if(i == ids[j])found=true;
				}
				if(!found){newDimensions.Add(dimensions[i]);}
			}
			dimensions = newDimensions.ToArray();	
			newDimensions.Clear ();
		}
		
		//OPERATORS//
		public override bool Equals(System.Object obj)
		{
			// If parameter is null return false.
			if (obj == null)
			{
				return false;
			}
			
			// If parameter cannot be cast to Vector return false.
			Vector p = (Vector) obj ;
			if ((System.Object)p == null)
			{
				return false;
			}
			
			
			if (p.Length != Length) {return false;}
			
			for(int i = Length - 1; i >=0; i--)
			{
				if(dimensions[i] != p[i]) { return false; }
			}
			if(defaultValue != p.defaultValue){return false;}
			
			return true;	
		}
		
		public bool Equals(Vector p)
		{
			// If parameter is null return false:
			if ((object)p == null)
			{
				return false;
			}
			
			if (p.Length != Length) {return false;}
			
			for(int i = Length - 1; i >=0; i--)
			{
				if(dimensions[i] != p[i]) { return false; }
			}
			if(defaultValue != p.defaultValue){return false;}
			
			return true;
		}
		
		public override int GetHashCode()
		{
			return Mathf.RoundToInt(defaultValue * Length);
		}
		
		
		public static bool operator == (Vector a, Vector b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
			{
				return true;
			}
			
			// If one is null, but not both, return false.
			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}
			
			
			if (a.Length != b.Length) {return false;}
			
			for(int i = a.Length - 1; i >=0; i--)
			{
				if(a[i] != b[i]) { return false; }
			}
			if(a.defaultValue != b.defaultValue){return false;}
			return true;	
			
		}
		public static bool operator != (Vector a, Vector b)
		{
			return !(a == b);
		}
		public static Vector operator + (Vector a, float v)
		{
			
			for(int i = a.Length-1; i >=0; i--)
			{
				a[i] += v;
			}
			return a;
		}
		public static Vector operator + (Vector a, Vector b)
		{
			int maxLength;
			
			if(a.Length <= b.Length)
			{
				maxLength = a.Length;
			}
			else{maxLength = b.Length;}
			for(int i= maxLength-1; i >=0; i--)
			{
				a[i] += b[i];
			}
			return a;
		}
		
		public static Vector operator - (Vector a, float v)
		{
			for(int i = a.Length-1; i >=0; i--)
			{
				a[i] -= v;
			}
			return a;
		}
		public static Vector operator - (Vector a, Vector b)
		{
			int maxLength;
			
			if(a.Length <= b.Length)
			{
				maxLength = a.Length;
			}
			else{maxLength = b.Length;}
			for(int i= maxLength-1; i >=0; i--)
			{
				a[i] -= b[i];
			}
			return a;
		}
		
		public static Vector operator * (Vector a, float v)
		{
			for(int i = a.Length-1; i >=0; i--)
			{
				a[i] *= v;
			}
			return a;
		}
		public static Vector operator * (Vector a, Vector b)
		{
			int maxLength;
			
			if(a.Length <= b.Length)
			{
				maxLength = a.Length;
			}
			else{maxLength = b.Length;}
			for(int i= maxLength-1; i >=0; i--)
			{
				a[i] *= b[i];
			}
			return a;
		}
		
		public static Vector operator / (Vector a, float v)
		{
			if (v != 0)
			{
				for (int i = a.Length-1; i >=0; i--) 
				{
					a.dimensions [i] /= v;
				}
				
			}
			return a;
		}
		public static Vector operator / (Vector a, Vector b)
		{
			int maxLength;
			
			if(a.Length <= b.Length)
			{
				maxLength = a.Length;
			}
			else{maxLength = b.Length;}
			for(int i= maxLength-1; i >=0; i--)
			{
				if(b[i]!=0){ a[i] /= b[i]; }
			}
			return a;
		}

        //COMMON MATHEMATICAL METHODS//

        public static Vector Lerp(Vector a, Vector b, float t) {
            int l = a.Length;
            if(b.Length < l) { l = b.Length; }
            Vector vec = new Vector(l, a.defaultValue);
            for (int i = l - 1; i >= 0; i--){
                vec[i] = Mathf.Lerp(a[i], b[i], t);
            }
            return vec;
        }
		/// <summary>
		/// Average the specified vectors' dimensional values.
		/// </summary>
		/// <param name="v">Vectors to average.</param>
		public static Vector Average(Vector[] v)
		{
			int i, j, k, count, maxLength;
			float current;
			Vector ret;
			maxLength = 0;
			k = -1;
			for(i = v.Length-1; i >=0; i--)
			{
				if(v[i].Length > maxLength)
				{
					k= i; maxLength = v[i].Length;
				}
			}
			ret = v[k];
			for(i = 0; i < ret.Length; i++)
			{
				current = ret[i];
				count =1;
				for(j=0; j < v.Length; j++)
				{
					if(j!= k && v[j].Length >i)
					{
						
						count++;
						current+= v[j][i];
						
					}
				}
				ret[i] = current/count;
			}
			return ret;
		}

		/// <summary>
		/// Distance between specified vectors a and b.
		/// </summary>
		/// <param name="a">The starting vector.</param>
		/// <param name="b">The ending vector.</param>
		public static float Distance(Vector a, Vector b)
		{
			int maxLength, i;
			float distance, cP;
			distance = 0f;
			if(a.Length <= b.Length)
			{
				maxLength = a.Length;
			}
			else{maxLength = b.Length;}
			
			for(i = maxLength-1; i >=0; i--)
			{
				cP = b[i] - a[i];
				distance += cP*cP;
			}
			return Mathf.Sqrt(distance);
		}

		/// <summary>
		///  Applies a drag value to each dimension of the vector, moving them towards 0.
		/// </summary>
		/// <param name="d">Drag value.</param>
		public void ApplyDrag(float d)
		{
			ApplyDrag(d, 0f);
		}

		/// <summary>
		/// Applies a drag value to each dimension of the vector, moving them towards drag point.
		/// </summary>
		/// <param name="d">Drag value.</param>
		/// <param name="dragPoint">Value to drag towards.</param>
		public void ApplyDrag(float d, float dragPoint)
		{
			int i;
			float total, pd, pAbs;
			total = 0f;
			for(i=0; i < dimensions.Length; i++)
			{
				pAbs = dimensions[i]; if(pAbs <0) pAbs*= -1;
				total+= pAbs;
			}
			
			for(i=0; i < dimensions.Length; i++)
			{
				pAbs = dimensions[i]; if(pAbs <0) pAbs*= -1;
				pd = pAbs/total*d;
				if(dimensions[i] < dragPoint && dimensions[i] < dragPoint-pd){dimensions[i]+=pd;}
				else if(dimensions[i] >dragPoint && dimensions[i] > pd+dragPoint){dimensions[i]-=pd;}
				else{dimensions[i] = dragPoint;}
			}
		}
		

		//IMPLICIT CONVERSIONS WITH OTHER COMMON DATA TYPES//
		public static implicit operator Vector(Vector2 v)
		{
			Vector newVector = new Vector(2);
			newVector[0] = v.x;
			newVector[1] = v.y;
			return newVector;
		}
		public static implicit operator Vector2(Vector v)
		{
			Vector2 a = Vector2.zero;
			switch(v.Length)
			{
			case 0:
				break;
				
			case 1:
				a.x = v[0];
				break;
			default:
				a.x = v[0];
				a.y = v[1];
				break;
			}
			return a;
		}
		
		public static implicit operator Vector(Vector3 v)
		{
			Vector newVector = new Vector(3);
			newVector[0] = v.x;
			newVector[1] = v.y;
			newVector[2] = v.z;
			return newVector;
		}
		public static implicit operator Vector3(Vector v)
		{
			Vector3 a = Vector2.zero;
			switch(v.Length)
			{
			case 0:
				break;
				
			case 1:
				a.x = v[0];
				break;
				
			case 2:
				a.x = v[0];
				a.y = v[1];
				break;
				
			default:
				a.x = v[0];
				a.y = v[1];
				a.z = v[2];
				break;
			}
			return a;
		}
		
		public static implicit operator Vector(Vector4 v)
		{
			Vector newVector = new Vector(4);
			newVector[0] = v.x;
			newVector[1] = v.y;
			newVector[2] = v.z;
			newVector[3] = v.w;
			return newVector;
		}
		public static implicit operator Vector4(Vector v)
		{
			Vector4 a = Vector2.zero;
			switch(v.Length)
			{
			case 0:
				break;
				
			case 1:
				a.x = v[0];
				break;
				
			case 2:
				a.x = v[0];
				a.y = v[1];
				break;
				
			case 3:
				a.x = v[0];
				a.y = v[1];
				a.z = v[2];
				break;
				
			default:
				a.x = v[0];
				a.y = v[1];
				a.z = v[2];
				a.w = v[3];
				break;
			}
			return a;
		}
		
		public static implicit operator Vector(Color v)
		{
			Vector newVector = new Vector(4);
			newVector[0] = v.r;
			newVector[1] = v.g;
			newVector[2] = v.b;
			newVector[3] = v.a;
			return newVector;
		}

		public static implicit operator Color(Vector v)
		{
			Color a = Color.black;
			switch(v.Length)
			{
			case 0:
				break;
				
			case 1:
				a.r = v[0];
				break;
				
			case 2:
				a.r = v[0];
				a.g = v[1];
				break;
				
			case 3:
				a.r = v[0];
				a.g = v[1];
				a.b = v[2];
				break;
				
			default:
				a.r = v[0];
				a.g = v[1];
				a.b = v[2];
				a.a = v[3];
				break;
			}
			return a;
		}
		
		public static implicit operator Vector(Rect v)
		{
			Vector newVector = new Vector(4);
			newVector[0] = v.x;
			newVector[1] = v.y;
			newVector[2] = v.width;
			newVector[3] = v.height;
			return newVector;
		}
		public static implicit operator Rect(Vector v)
		{
			Rect a = new Rect();
			switch(v.Length)
			{
			case 0:
				break;
				
			case 1:
				a.x = v[0];
				break;
				
			case 2:
				a.x = v[0];
				a.y = v[1];
				break;
				
			case 3:
				a.x = v[0];
				a.y = v[1];
				a.width = v[2];
				break;
				
			default:
				a.x = v[0];
				a.y = v[1];
				a.width = v[2];
				a.height = v[3];
				break;
			}
			return a;
		}
		
		public static implicit operator Vector(Color32 v)
		{
			Vector newVector = new Vector(4);
			newVector[0] = v.r*3.92156e-3f;
			newVector[1] = v.g*3.92156e-3f;
			newVector[2] = v.b*3.92156e-3f;
			newVector[3] = v.a*3.92156e-3f;
			return newVector;
		}
		public static implicit operator Color32(Vector v)
		{
			Color32 a = new Color32 (0, 0, 0, 0);
			switch(v.Length)
			{
			case 0:
				break;
				
			case 1:
				a.r = (byte)Mathf.RoundToInt(v[0]*(float)255);
				break;
				
			case 2:
				a.r = (byte)Mathf.RoundToInt(v[0]*(float)255);
				a.g = (byte)Mathf.RoundToInt(v[1]*(float)255);
				break;
				
			case 3:
				a.r = (byte)Mathf.RoundToInt(v[0]*(float)255);
				a.g = (byte)Mathf.RoundToInt(v[1]*(float)255);
				a.b = (byte)Mathf.RoundToInt(v[2]*(float)255);
				break;
				
			default:
				a.r = (byte)Mathf.RoundToInt(v[0]*(float)255);
				a.g = (byte)Mathf.RoundToInt(v[1]*(float)255);
				a.b = (byte)Mathf.RoundToInt(v[2]*(float)255);
				a.a = (byte)Mathf.RoundToInt(v[3]*(float)255);
				break;
			}
			return a;
		}
		
		
		public override string ToString()
		{
			string final = "";
			for(int i=0; i < Length-1; i++)
			{
				final+= i + ": " + dimensions[i].ToString() + "   |   "; 
			}
			final += (Length - 1) + ": " + dimensions [Length - 1].ToString();
			return final;
		}
	}
}
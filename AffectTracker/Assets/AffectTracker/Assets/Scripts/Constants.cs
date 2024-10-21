using UnityEngine;
using System.Collections.Generic;

namespace Affect {
	public static class Constants {

        /// <summary>Grid properties</summary>
        public enum GridProperties { X_AXIS, Y_AXIS, RADIUS, ANGLE }
        
		/// <summary>Haptic controller definitions</summary>
        public enum HapticControllers {	LEFT, RIGHT, BOTH };

        /// <summary>Rating types supported</summary>
        public enum RatingType { CR, /* continues rating*/ SR } // single rating

        /// <summary>x Quadrants representing High <> Low positive and negative. Starting 12-15 as quadrant one, counterclockwise/// <summary>Grid properties</summary>
        public enum Quadrants { HP,LP,LN,HN	}

        /// <summary>Returns a solid gradient</summary>
        public static Gradient MakeSolidGradient(Color solidColor) {

			Gradient gradient = new Gradient();
			GradientColorKey[] colorKey;
			GradientAlphaKey[] alphaKey;

			// Populate the color keys at the relative time 0 and 1 (0 and 100%)
			colorKey = new GradientColorKey[2];
			colorKey[0].color = solidColor;
			colorKey[0].time = 0.0f;
			colorKey[1].color = solidColor;
			colorKey[1].time = 1.0f;

			// Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
			alphaKey = new GradientAlphaKey[2];
			alphaKey[0].alpha = solidColor.a;
			alphaKey[0].time = 0.0f;
			alphaKey[1].alpha = solidColor.a;
			alphaKey[1].time = 1.0f;

			gradient.SetKeys(colorKey, alphaKey);

			return gradient;
		}

        /// <summary>Returns a tranparent gradient</summary>
        public static Gradient AddTransparentToGradient(Gradient gradient) {
			// Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
			Gradient tmp = gradient;

			GradientAlphaKey[] alphaKey;
			alphaKey = new GradientAlphaKey[4];
			alphaKey[0].alpha = tmp.alphaKeys[0].alpha;
			alphaKey[0].time = 0.0f;
			alphaKey[1].alpha = 0.15f;
			alphaKey[1].time = 0.35f;
			alphaKey[2].alpha = 0.15f;
			alphaKey[2].time = 0.65f;
			alphaKey[3].alpha = 1.0f;
			alphaKey[3].time = tmp.alphaKeys[1].alpha;

			tmp.SetKeys(gradient.colorKeys, alphaKey);

			return tmp;

		}
        /// <summary>Returns a desaturated gradient</summary>
        public static Gradient AdjustGradientSaturation(Gradient sourceGradient, float targetSaturation) {
			Gradient newGradient = new Gradient();

			GradientColorKey[] colorKeys = sourceGradient.colorKeys;
			GradientAlphaKey[] alphaKeys = sourceGradient.alphaKeys;

			for (int i = 0; i < colorKeys.Length; i++) {
				Color originalColor = colorKeys[i].color;
				Color.RGBToHSV(originalColor, out float h, out float s, out float v);

				s = targetSaturation;
				
				Color newColor = Color.HSVToRGB(h, s, v);
				colorKeys[i].color = newColor;
			}

			newGradient.SetKeys(colorKeys, alphaKeys);

			return newGradient;
		}

        /// <summary>Returns gameobject with given name</summary>
        public static GameObject GetChildGameObject(GameObject fromGameObject, string withName) {
			Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
			foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
			return null;
		}

        /// <summary>Remaps value</summary>
        public static float remap(float value, float fromA, float fromB, float toA, float toB) {
			return toA + (value - fromA) * (toB - toA) / (fromB - fromA);
		}
	}

}
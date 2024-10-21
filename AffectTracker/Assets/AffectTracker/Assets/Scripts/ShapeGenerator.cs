using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace Affect {
	public class ShapeGenerator : MonoBehaviour {
		// Values to set manually before running
		Gradient grad;

		[HelpBox("<size=14><color=#00ee30><b>Shape generator component</b></color></size>\nGenerates the flubber.")]
		public UIPolygon basePolygon;
		public UIPolygon haloPolygon;
		public VisualFeedback visualFeedback;
		public LineRenderer outline;

		[Tooltip("Number of vertices used to calculate shapes.")]
		[Min(1)]
		public int vertices = 200;
		[Tooltip("Number of spikes in shape.")]
		[Min(1)]
		public int waves = 10;
		[Tooltip("Angle used to calculate natural triangle shapes for y_tri3")]
		[Min(0f)]
		public float tri3Angle = 45f;
		[Tooltip("Sigma used for Guassian distribution of spike amplitude variation. Not currently plugged in.")]
		[Min(0f)]
		public float maxSigma = 10f;
		[Tooltip("Value used for y_tri2 shape.")]
		[Range(0.0f, 1.0f)]
		public float k = 1.0f;

		// Values which can be changed manually/externally while running
		[Tooltip("Radius of the flubber's insides. Spikes are added on top of this distance.")]
		public float baseSize = 1;
		[Tooltip("Speed that flubber rotates (+CW, -CCW).")]
		public float rotationSpeed = 0.0f;
		[Tooltip("Added to baseSize to determine flubber radius at the trough of a spike.")]
		public float minSize = 0;
		[Tooltip("Added to baseSize to determine flubber radius at the peak of a spike.")]
		public float maxSize = 1;
		[Tooltip("Determines minimum size of flubber when pulsating.")]
		public float minScale = 1f;
		[Tooltip("Determines maximum size of flubber when pulsating.")]
		public float maxScale = 1.1f;
		[Tooltip("Mix ratio of flubber shapes 1 and 2 (shape types defined in script).")]
		[Range(0.0f, 1.0f)]
		public float shapeMix = 0.0f;
		[Tooltip("Determines rate of changes to spike size, pulsation, etc. Higher = faster.")]
		[Min(0f)]
		public float timeStepSize = 10f;
		[Tooltip("Temporal asynchronicity of spike size changes.")]
		[Range(0f, 1f)]
		public float spikeOffset = 0f;
		[Tooltip("Spatial (height) asynchronicity of spike size changes.")]
		[Range(0f, 1f)]
		public float spikeAmpDiff = 0f;
		[Tooltip("Size of Base polygon relative to line renderer. 1 = same size.")]
		public float pgBaseScale = 1f;
		[Tooltip("Size of Halo polygon relative to line renderer. 1 = same size.")]
		public float pgHaloScale = 1.1f;

		// Values automated in this script
		[Tooltip("Size of individual spikes. Automated by script.")]
		public float size = 1;
		[Tooltip("Flubber scale. Used for overall size and for pulsation. Automated by script.")]
		public float scale = 1f;

		// Internal values
		private bool polar = true; // Whether vertex positions are calculated for Cartesian or Polar coordinates
		private float length = 1; // Length of line renderer line
		private float amplitude = 1.0f; // Used to generate base shapes, no need to change in order to modify flubber.
		private float saturation = 0; // Flubber color saturation.
		private float time; // Used for tracking time between frames when necessary.
		private float[] timeOffsets; // Holds unique temporal offset value for each spike. Used for desynchronization of spikes.
		private float[] waveAmpMod; // Holds unique spatial (heigh) offset value for each spike. Used for desynchronization of spikes.
		private bool[] waning; // Bool for each spike indicating whether it is growing or shrinking based on the current time value.
		private Vector3[] baseShape1; // Holds positions for every vertex for Shape 1.
		private Vector3[] baseShape2; // Holds positions for every vertex for Shape 2.
		private float baseScale; // Calculates scale of flubber to change with Grid Dimensions.
		private Gradient lineGradient = new Gradient();
		private bool isUpdating; // Whether or not touchpad is active.
		private float baseGradientLocation; // color to use for base shape from the gradient provided
		

		private float Mod(float a, float n) => (a % n + n) % n;


		void Awake() {
			//visualFeedback = transform.parent.transform.parent.GetComponent<VisualFeedback>();

			/* For UIPolygon */

			basePolygon.sides = vertices;
			basePolygon.VerticesDistances = new float[vertices];
			haloPolygon.sides = vertices;
			haloPolygon.VerticesDistances = new float[vertices];

			/* For Line Renderer */

			outline.startWidth = visualFeedback.OutlineThickness;
			outline.endWidth = visualFeedback.OutlineThickness;
			outline.positionCount = vertices + 1;
			outline.material = visualFeedback.OutlineMaterial;
			timeOffsets = new float[waves];
			waveAmpMod = new float[waves];
			waning = new bool[waves];
			baseScale = ((maxScale - minScale) * 1 + minScale) * visualFeedback.Scaling;

			for (int waveNum = 0; waveNum < waves; waveNum++) {
				timeOffsets[waveNum] = Random.Range(-1f, 1f) * Mathf.PI;
				waveAmpMod[waveNum] = 1;
				waning[waveNum] = false;
			}

			/* Define shapes */

			baseShape1 = new Vector3[vertices + 1];
			baseShape2 = new Vector3[vertices + 1];

			for (int i = 0; i <= vertices; i++) {
				if (polar) {
					length = 1;
				}
				else {
					size = 0;
				}

				float x = i * length / vertices;
				float theta = i * 2 * Mathf.PI / vertices;
				float p = length / waves;
				float vertsPerWave = vertices / waves;
				float vertSizeRads = 2 * Mathf.PI / vertices;
				float alpha = Mathf.PI / 2 + tri3Angle * Mathf.PI / 180;

				/* Available shapes */

				// Sine wave
				float y_sine = amplitude * Mathf.Sin(waves * theta);
				// Cosine wave
				float y_cosi = amplitude * Mathf.Cos(waves * theta);
				// Randomly distributed noise
				float y_rand = amplitude * Random.Range(-1.0f, 1.0f);
				// Normally distributed noise
				float y_norm = RandNorm(amplitude, size);
				// Triangle wave 1: convex distortions when circularized
				float y_tria = (4 * amplitude / p) * Mathf.Abs(Mod(x - p / 4, p) - p / 2) - amplitude + size;
				// Triangle wave 2: wave calculated with inverse trigonometric fucntions which looks more like undistorted triangles when circularized.
				float y_tri2 = (Mathf.Cos((2 * Mathf.Asin(k) + Mathf.PI * (waves - 2)) / (2 * waves)) /
					Mathf.Cos((2 * Mathf.Asin(k * Mathf.Cos(waves * theta)) + Mathf.PI * (waves - 2)) / (2 * waves)));
				// Triangle wave 3: Assumes peaks and troughs fall on vertices, assumes a straight line between each peak/trough, and calculates the distance to that line from the center of the circle at regular angular intervals corresponding to the rest of the vertices.
				float y_tri3;
				if (i % vertsPerWave < vertsPerWave / 2) {
					y_tri3 = baseSize * Mathf.Sin(alpha) / Mathf.Sin(Mathf.PI - alpha - vertSizeRads * (Mathf.Floor(vertsPerWave / 2) - i % vertsPerWave));
				}
				else if (i % vertsPerWave > vertsPerWave / 2) {
					y_tri3 = baseSize * Mathf.Sin(alpha) / Mathf.Sin(Mathf.PI - alpha - vertSizeRads * (i % vertsPerWave - Mathf.Floor(vertsPerWave / 2)));
				}
				else {
					y_tri3 = baseSize;
				}

				/* Select previously defined shapes for Shape1 and 2 */

				baseShape1[i] = new Vector3(x, y_tri3, 0.0f);
				baseShape2[i] = new Vector3(x, y_cosi, 0.0f);
			}

			// Get maxes and mins of each shape
			float minYShape1 = Mathf.Infinity;
			float maxYShape1 = Mathf.NegativeInfinity;
			float minYShape2 = Mathf.Infinity;
			float maxYShape2 = Mathf.NegativeInfinity;

			for (int i = 0; i <= vertices; i++) {
				minYShape1 = (baseShape1[i].y < minYShape1) ? baseShape1[i].y : minYShape1;
				maxYShape1 = (baseShape1[i].y > maxYShape1) ? baseShape1[i].y : maxYShape1;
				minYShape2 = (baseShape2[i].y < minYShape2) ? baseShape2[i].y : minYShape2;
				maxYShape2 = (baseShape2[i].y > maxYShape2) ? baseShape2[i].y : maxYShape2;
			}

			// Normalize shapes
			for (int i = 0; i <= vertices; i++) {
				float x = i * length / vertices;
				baseShape1[i].y = (baseShape1[i].y - minYShape1) / (maxYShape1 - minYShape1);
				baseShape2[i].y = (baseShape2[i].y - minYShape2) / (maxYShape2 - minYShape2);
			}

			// Set colors
			SetColors();

			// Scale polygon
			basePolygon.gameObject.transform.localScale = new Vector3(pgBaseScale * 2f, pgBaseScale * 2f, 1);
			haloPolygon.gameObject.transform.localScale = new Vector3(pgHaloScale * 2f, pgHaloScale * 2f, 1);

			transform.localScale = new Vector3(baseScale, baseScale, 1f);
		}

		// Generate random values with a normal distribution based on given amplitude and base size desired
		public float RandNorm(float amplitude, float size) {
			float u, v, S, output;
			do {
				do {
					u = 2.0f * Random.value - 1.0f;
					v = 2.0f * Random.value - 1.0f;
					S = u * u + v * v;
				}
				while (S >= 1.0f);

				float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
				output = u * fac;
			}
			while (Mathf.Abs(output) > 3);

			return output * amplitude / 3 + size;
		}

		void Update() {

			// Calculate time steps for flubber changes
			float oldTime = time;
			time = (time + Time.deltaTime * 2 * Mathf.PI * timeStepSize) % (2 * Mathf.PI);

			// Stop Flubber pulsation if touchpad not active
			if (!isUpdating) {
				scale = baseScale;
			}
			else {
				scale = ((maxScale - minScale) * (Mathf.Sin(time) / 2 + 0.5f) + minScale) * visualFeedback.Scaling;
			}

			var points = new Vector3[vertices + 1];
			int lastWaveNum = -1;

			// Determine amplitude of each vertex
			for (int i = 0; i <= vertices; i++) {
				// Determine which spike the current vertex belongs to
				int waveNum = (int)Mathf.Floor((i + vertices / waves / 2) / (vertices / waves));
				waveNum = waveNum >= waves ? 0 : waveNum;

				// Determine overall size of current spike
				size = (maxSize - minSize) * (Mathf.Sin(time + spikeOffset * timeOffsets[waveNum]) / 2 + 0.5f) + minSize;

				// When looking at a new spike...
				if (lastWaveNum != waveNum) {
					float oldSize = (maxSize - minSize) * (Mathf.Sin(oldTime + spikeOffset * timeOffsets[waveNum]) / 2 + 0.5f) + minSize;

					// If spike is growing...
					if (oldSize < size) {
						// but marked as shrinking: assign new amplitude offset
						if (waning[waveNum]) {
							waveAmpMod[waveNum] = Random.Range(-spikeAmpDiff, spikeAmpDiff) + 1;
						}
						// mark it as growing
						waning[waveNum] = false;
					}
					else { waning[waveNum] = true; }
				}

				/* Base and Halo Polygons */

				// For use with 2 shapes
				float currentVertexRadius; // represents current distance from center of flubber to current vertex based on all previously defined parameters

				// Mix Shape1 and Shape2 if touchpad active, otherwise flubber is plain circle
				if (isUpdating) {
					currentVertexRadius = (baseShape1[i].y * (1 - shapeMix) + baseShape2[i].y * shapeMix) * size * waveAmpMod[waveNum] + baseSize;
				}
				else {
					currentVertexRadius = baseSize;
				}


				if (i < vertices) basePolygon.VerticesDistances[i] = currentVertexRadius;
				if (i < vertices) haloPolygon.VerticesDistances[i] = currentVertexRadius;

				/* For use with 3 shapes 
				Where the first shape is mixed across the whole range, 
				but shape2 is maxed at 0.5 and shape3 is maxed at 1.0
				I.e. at 0: 100% shape1, at 0.5: 50% shape1, 50% shape2, at 1: 100% shape3 */
				//float shape = (shapeMix <= 0.5f) ? shape1*(1-shapeMix) + shape2*shapeMix : shape1*(1-shapeMix) + shapeMix*(shape2*(1-2*(shapeMix-0.5f)) + shape3*2*(shapeMix-0.5f));

				/* Outline (line renderer) */
				outline.startWidth = Constants.remap(visualFeedback.OutlineThickness, 0, 1, 0, 0.1f);
				outline.endWidth = Constants.remap(visualFeedback.OutlineThickness, 0, 1, 0, 0.1f);

				float x = i * length / vertices;
				float theta = i * 2 * Mathf.PI / vertices;

				if (polar) {
					points[i] = new Vector3(currentVertexRadius * Mathf.Cos(theta), currentVertexRadius * Mathf.Sin(theta), 0.0f);
				}
				else {
					points[i] = new Vector3(x, currentVertexRadius, 0.0f);
				}

				lastWaveNum = waveNum;
			}

			outline.SetPositions(points);

			Vector3 currentOri = transform.localRotation.eulerAngles;
			currentOri.z -= rotationSpeed;

			transform.localRotation = Quaternion.Euler(currentOri);
			transform.localScale = new Vector3(scale, scale, 1f);

		}
		public void SetColors() {
			outline.colorGradient = Constants.MakeSolidGradient(visualFeedback.OutlineColor);
			haloPolygon.color = visualFeedback.HaloColor;
			basePolygon.color = visualFeedback.BaseGradientInternal.Evaluate(0);
		}

		// ======================================================================================================================================================

		public void UpdateShape(float newShapeMix) {
			// Shape Pointy <> Curvy is a combination of parameters
			//TODO add the combination of parameter to form spikes correctly relatively to the inputvalue OR put multiple remappings in the valueRemapping list
			shapeMix = newShapeMix;
		}

		public void UpdateMinSize(float newMinSize) {
			minSize = newMinSize;
		}

		public void UpdateMaxSize(float newMaxSize) {
			maxSize = newMaxSize;
		}

		public void UpdateColor(float newGradientLocation) {
			if (!isUpdating) {
				basePolygon.color = visualFeedback.BaseIdleColor;
			} else {
				basePolygon.color = visualFeedback.BaseGradientInternal.Evaluate(newGradientLocation);
				baseGradientLocation = newGradientLocation;
			}
		}

		public void UpdateSaturation(float newSaturation) {
			if (!isUpdating)
				return;

			if (newSaturation == saturation)
				return;

			visualFeedback.BaseGradientInternal = Constants.AdjustGradientSaturation(visualFeedback.BaseGradient, newSaturation);

			saturation = newSaturation;

			basePolygon.color = visualFeedback.BaseGradientInternal.Evaluate(baseGradientLocation);
		}

		public void UpdatePulse(float newTimeStepSize) {
			timeStepSize = newTimeStepSize;
		}

		public void UpdateSpikeOffset(float newSpikeOffset) {
			spikeOffset = newSpikeOffset;
		}

		public void UpdateSpikeAmpDiff(float newSpikeAmpDiff) {
			spikeAmpDiff = newSpikeAmpDiff;
		}

		/// <summary>Joystick touched or not</summary>
		public void UpdateTouchState(bool isTouched) {

			if (isTouched) { // touched and is allowed
				isUpdating = true; // 

			}
			else {
				isUpdating = false;
			}
		}
	}
}
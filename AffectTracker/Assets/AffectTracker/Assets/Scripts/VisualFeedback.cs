using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;

namespace Affect {
	public class VisualFeedback : MonoBehaviour {

		[HelpBox("<size=14><color=#00ee30><b>Visual Feedback component</b></color></size>\nSettings about the visual feedback of the ratings.")]
		[Header("Flubber Settings")]

		[Space(10)]
		public bool ShowBase = true;
		[Tooltip("Gradient color location used in `updateColor` mapping.")]
		public Gradient BaseGradient = new Gradient();
		[HideInInspector]
		public Gradient BaseGradientInternal = new Gradient();
		[Tooltip("Color when controller is not touched.")]
		public Color BaseIdleColor = Color.gray;
		[Space(10)]
		public bool ShowOutline = true;
		public Color OutlineColor = Color.white;
		[Range(0.1f, 1f)]
		public float OutlineThickness = 0.1f; 
		[Space(10)]
		public bool showHalo = true;
		public Color HaloColor = Color.white;

		[Space(10)]
		[Header("Grid Settings")]
		[Space(10)]
		public bool ShowGrid = false;
		public bool ShowGridXaxis = false;
		public bool ShowGridYaxis = false;
		[Range(0.1f, 1f)]
		public float GridLineThickness = 1f;
		private float GridLineThicknessInternal = 1f;
		public Color GridColor = Color.white;
		[Space(10)]
		public bool ShowGridOutline;
		public Color GridOutlineColor = Color.white;
		[Range(0.1f, 1f)]
		public float GridOutLineThickness = 0.1f; // 
		private float GridOutLineThicknessInternal = 0.1f; // 1
		[Space(10)]
		public bool ShowCursor = false;
		public Color CursorColor = Color.red;
        public bool ShowCursorOutline = false;
        public Color CursorOutlineColor = Color.red;
		[Range(0.1f,5)]
		public float CursorSize = 1f;

        [Tooltip("In meters")]
		[Header("Size")]
		[Range(0.1f, 1f)]
		public float Scaling = 1;

		[Space(10)]
		[Header("Materials")]
		public Material OutlineMaterial;
		public Material GridMaterial;

		Vector3 TL, TR, BL, BR, TM, BM, LM, RM, M; // keypoints of the grid
		float half = 1f; // Half of the gridsize

		GameObject grid = null; // Holder of the visual grid

		LineRenderer gridContour = null;
		LineRenderer gridContourOutline = null;
		LineRenderer xAxis = null;
		LineRenderer xAxisOutline = null;
		LineRenderer yAxis = null;
		LineRenderer yAxisOutline = null;

		Vector3 cursorLocation = new Vector3(0, 0, 0); // Position of the X,Y values presented as cursor

		GameObject shapeOutline = null;
		GameObject shapeBase = null;
		GameObject shapeHalo = null;
		GameObject cursor2D = null;
		Vector3[] GizmoPoints;

		// ======================================================================================================================================================

		void Awake() {
			GridLineThicknessInternal = Constants.remap(GridLineThickness, 0, 1, 0, 0.025f);
			GridOutLineThicknessInternal = Constants.remap(GridOutLineThickness, 0, 1, 0, 0.025f);
			BaseGradientInternal = BaseGradient;

			GetReferences();
			CreateGrid();
			CreateCursor2D();
			UpdateGrid();
		}

		private void GetReferences() {
			shapeOutline = Constants.GetChildGameObject(this.gameObject, "Outline");
			shapeBase = Constants.GetChildGameObject(this.gameObject, "Base");
			shapeHalo = Constants.GetChildGameObject(this.gameObject, "Halo");
			cursor2D = Constants.GetChildGameObject(this.gameObject, "Cursor2D");
		}

		void OnEnable() {
			UpdateCursorPosition(Vector2.zero);
			ShowCursor2D(ShowCursor);
			shapeOutline.SetActive(ShowOutline);
			shapeBase.SetActive(ShowBase);
			shapeHalo.SetActive(showHalo);
		}

		// ======================================================================================================================================================

		private void OnDrawGizmos() {
			Gizmos.color = Color.yellow;

			Gizmos.DrawWireCube(transform.position, new Vector3(0.35f, 0.35f, 0.0f));
			GizmoPoints = new Vector3[4]
			  {
				transform.position - new Vector3(0.15f, 0, 0),
				transform.position + new Vector3(0.15f, 0, 0),
				transform.position - new Vector3(0, 0.15f, 0),
				transform.position + new Vector3(0, 0.15f, 0)
			  };

			Gizmos.DrawLineList(GizmoPoints);
		}

		// ======================================================================================================================================================


		void CreateGrid() {
			// Grid holder
			grid = new GameObject("Grid");
			grid.transform.SetParent(this.transform);
			grid.transform.SetSiblingIndex(0);
			grid.transform.localPosition = new Vector3(0, 0, -0.01f);
			grid.transform.localRotation = Quaternion.identity;

			// Linerenderers for grid
			if (ShowGrid) { gridContour = CreateLineRendererObject("contour", 5, Constants.MakeSolidGradient(GridColor), GridLineThicknessInternal); gridContour.loop = true; }
			if (ShowGridXaxis) xAxis = CreateLineRendererObject("xAxis", 3, Constants.MakeSolidGradient(GridColor), GridLineThicknessInternal);
			if (ShowGridYaxis) yAxis = CreateLineRendererObject("yAxis", 3, Constants.MakeSolidGradient(GridColor), GridLineThicknessInternal);

			if (!ShowGridOutline)
				return;

			//// Linerenderers for grid outline
			if (ShowGrid) { gridContourOutline = CreateLineRendererObject("contourOutline", 5, Constants.MakeSolidGradient(GridOutlineColor), GridOutLineThicknessInternal * 2); gridContourOutline.loop = true; gridContourOutline.transform.Translate(0, 0, 0.001f, Space.Self); }
			if (ShowGridXaxis) { xAxisOutline = CreateLineRendererObject("xAxisOutline", 3, Constants.MakeSolidGradient(GridOutlineColor), GridOutLineThicknessInternal * 2); xAxisOutline.transform.Translate(0, 0, 0.001f, Space.Self); }
			if (ShowGridYaxis) { yAxisOutline = CreateLineRendererObject("yAxisOutline", 3, Constants.MakeSolidGradient(GridOutlineColor), GridOutLineThicknessInternal * 2); yAxisOutline.transform.Translate(0, 0, 0.001f, Space.Self); }

		}

		public void Reset() {
			UpdateCursorPosition(Vector2.zero);
			ShowCursor2D(false);
		}

		LineRenderer CreateLineRendererObject(string name, int vertexCount, Gradient lineGradient, float thickness) {
			GameObject g = new GameObject(name);
			g.transform.SetParent(grid.transform);
			g.transform.localPosition = new Vector3(0, 0, 0);
			g.transform.localRotation = Quaternion.identity;

			LineRenderer line = g.AddComponent<LineRenderer>();
			line.startWidth = thickness;
			line.endWidth = line.startWidth;
			line.useWorldSpace = false;
			line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			line.positionCount = vertexCount;
			line.material = GridMaterial;
			line.colorGradient = lineGradient;
			line.alignment = LineAlignment.TransformZ;

			return line;
		}

		void CreateCursor2D() {

			cursor2D.GetComponent<Image>().color = CursorColor;
			if (ShowCursor)
			{
				cursor2D.transform.GetChild(0).GetComponent<Image>().enabled = ShowCursorOutline;
				cursor2D.transform.GetChild(0).GetComponent<Image>().color = CursorOutlineColor;
			}
            cursor2D.transform.localPosition = cursorLocation;
			cursor2D.transform.localScale = new Vector3(0.00025f * CursorSize, 0.00025f * CursorSize, 0.00025f * CursorSize);

            ShowCursor2D(false);
		}

		/// <summary>Update cursor position in the 2dimensional grid </summary>
		public void UpdateCursorPosition(Vector2 obj) {

			if (!ShowCursor)
				return;

			cursorLocation = new Vector3(
				Constants.remap(obj.x, -1f, 1f, -half, half),
				Constants.remap(obj.y, -1f, 1f, -half, half),
				cursor2D.transform.localPosition.z
			);

			cursor2D.transform.localPosition = cursorLocation;
		}

		/// <summary>Update touch state </summary>
		public void UpdateTouchState(bool onOff) {
			ShowCursor2D(ShowCursor ? onOff : false);
		}

		void ShowCursor2D(bool onOff) {
			if (cursor2D == null)
				return;

			cursor2D.GetComponent<Image>().enabled = onOff;

			if (ShowCursorOutline)
				cursor2D.transform.GetChild(0).GetComponent<Image>().enabled = onOff;
		}


		/// <summary>Draw and position grid</summary>
		private void UpdateGrid() {
			half = Scaling / 2;

			M = new Vector3(0, 0, 0);

			TL = new Vector3(-half, half, 0);
			TR = new Vector3(half, half, 0);
			BL = new Vector3(-half, -half, 0);
			BR = new Vector3(half, -half, 0);

			TM = new Vector3(0, half, 0);
			BM = new Vector3(0, -half, 0);
			LM = new Vector3(-half, 0, 0);
			RM = new Vector3(half, 0, 0);

			if (ShowGrid) gridContour.SetPositions(new Vector3[] { BM, BL, TL, TR, BR });
			if (ShowGridXaxis) xAxis.SetPositions(new Vector3[] { LM, M, RM });
			if (ShowGridYaxis) yAxis.SetPositions(new Vector3[] { TM, M, BM });

			if (!ShowGridOutline)
				return;

			if (ShowGrid) gridContourOutline.SetPositions(new Vector3[] { BM, BL, TL, TR, BR });
			if (ShowGridXaxis) xAxisOutline.SetPositions(new Vector3[] { LM, M, RM });
			if (ShowGridYaxis) yAxisOutline.SetPositions(new Vector3[] { TM, M, BM });
		}

		// ======================================================================================================================================================

	}
}

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HelpBoxAttribute))]
public class HelpBoxAttributeDrawer : DecoratorDrawer {
	public override float GetHeight() {
		try {
			var helpBoxAttribute = attribute as HelpBoxAttribute;
			if (helpBoxAttribute == null) return base.GetHeight();
			var helpBoxStyle = (GUI.skin != null) ? GUI.skin.GetStyle("helpbox") : null;
			if (helpBoxStyle == null) return base.GetHeight();
			return Mathf.Max(80f, helpBoxStyle.CalcHeight(new GUIContent(helpBoxAttribute.text), EditorGUIUtility.currentViewWidth) + 4);
		}
		catch (System.ArgumentException) {
			return 3 * EditorGUIUtility.singleLineHeight; // Handle Unity 2022.2 bug by returning default value.
		}
	}

	public override void OnGUI(Rect position) {
		var helpBoxAttribute = attribute as HelpBoxAttribute;
		if (helpBoxAttribute == null) return;

		GUIStyle style = new GUIStyle(EditorStyles.helpBox);
		style.richText = true;
		GUI.color = Color.clear;
		EditorGUI.DrawTextureTransparent(new Rect(position.x + 12, position.y + 20, 40, 40), Resources.Load<Texture>("AffectLogo"));
		GUI.color = Color.white;
		EditorGUI.TextArea(new Rect(position.x + 70, position.y + 10, position.width - 80, 60), helpBoxAttribute.text, style);
	}
}



using System.Globalization;
using UnityEditor;
using UnityEngine;


// cette classe permet de n'afficher que les sous options appropriée selon le choix du type de deformation
[CustomEditor(typeof(CreationSimpleTerrain))]
public class CreationSimpleTerrainEditor : Editor
{

    public override void OnInspectorGUI()
    {
     

        serializedObject.Update();

        // Affichage du script en lecture seule
        GUI.enabled = false;
        EditorGUILayout.ObjectField(
            "Script",
            MonoScript.FromMonoBehaviour((CreationSimpleTerrain)target),
            typeof(CreationSimpleTerrain),
            false);
        GUI.enabled = true;


        DrawPropertiesExcluding(
      serializedObject,
      "choixModeDeformation",
      "m_Script",
      "numTexture",
      "textures",
      "typeFonction"
);
        EditorGUILayout.LabelField("Caractéristiques du maillage à venir: ", EditorStyles.boldLabel);
        CreationSimpleTerrain creationSimpleTerrain  = (CreationSimpleTerrain)target;
        EditorGUILayout.LabelField("Résolution = " + Mathf.Pow(2, creationSimpleTerrain.puissance2Resolution).ToString("N0", CultureInfo.GetCultureInfo("fr-FR")));

        long dimVert = (long)Mathf.Pow(2, creationSimpleTerrain.puissance2Resolution) * (long)Mathf.Pow(2, creationSimpleTerrain.puissance2Resolution);
        long dimTri = 3 * 2 * (long)(Mathf.Pow(2, creationSimpleTerrain.puissance2Resolution) - 1) * (long)(Mathf.Pow(2, creationSimpleTerrain.puissance2Resolution) - 1);
        EditorGUILayout.LabelField("Dim Vertices  ="+ dimVert.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")));
        EditorGUILayout.LabelField("Dim Triangles ="+ dimTri.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")));
        if ((dimVert >= uint.MaxValue) || (dimTri >= uint.MaxValue))
            EditorGUILayout.LabelField("Trop important même avec indexFormat à 32bits  |-o");
        else 
            if ((dimVert < ushort.MaxValue) && (dimTri < ushort.MaxValue))
                EditorGUILayout.LabelField("indexFormat : type indice conservé à 16 bits :-)");
            else
                EditorGUILayout.LabelField("indexFormat : type passe à 32 bits    :-o   ");



        EditorGUILayout.LabelField("Les modes de génération : ", EditorStyles.boldLabel);

        SerializedProperty type =
            serializedObject.FindProperty("choixModeDeformation");

        EditorGUILayout.PropertyField(type);

        CreationSimpleTerrain.ChoixModeDeformation choix =
            (CreationSimpleTerrain.ChoixModeDeformation)type.enumValueIndex;

        if (choix == CreationSimpleTerrain.ChoixModeDeformation.Texture
            )
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("numTexture"));

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("textures"));
        }
        else if (choix == CreationSimpleTerrain.ChoixModeDeformation.Fonction)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("typeFonction"));
           /* EditorGUILayout.LabelField("Sinusoide");
            EditorGUILayout.LabelField("Gaussienne");
            EditorGUILayout.LabelField("Perlin");*/ 

        }

        serializedObject.ApplyModifiedProperties();
    

    }

}
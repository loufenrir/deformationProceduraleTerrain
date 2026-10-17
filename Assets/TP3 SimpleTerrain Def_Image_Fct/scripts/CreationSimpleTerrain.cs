using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

using static CreationSimpleTerrain;
using System.Security.Cryptography;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]


public class CreationSimpleTerrain : MonoBehaviour
{
    [Range(1, 2000)] 
    public float dimension = 100;
    [Range(1, 15)]
    [Tooltip("La resolution sera 2 élévé à la puissance de cette valeur 2^n " )]
    public int puissance2Resolution = 1;
    public bool CentrerPivot = true;
    [HideInInspector] public ushort resolution;
    


    [Tooltip("Information sur le mode de génération :  ")]
    public enum ChoixModeDeformation
    {
        Fonction,
        Texture
    }
    public ChoixModeDeformation choixModeDeformation;
      public enum TypeFonction
    {
        Sinusoide,
        Collines,
        Perlin
    }
    [Tooltip("Choix de la première fonction à appliquer ")]
    public TypeFonction typeFonction;


    [Tooltip("Choix de la première HeightMap à appliquer ")] 
    public int numTexture;
    [Tooltip("Les HeightMaps disponibles ")]
    public List<Texture2D>  textures;




    private uint    p_dimVertices;
    private uint    p_dimTriangles;

    private MeshCollider p_meshCollider;
    private MeshFilter  p_meshFilter;
    private Mesh        p_mesh;
    private Vector3[]   p_vertices;
    private Vector3[]   p_normals;
    private int[]       p_triangles;



    private Camera p_cam;
    private LayerMask maskPickingTerrain;

    private float p_dimInterVertices;


    // cette méthode est appelée la première fois que le component est ajouté à l'objet (ou quand reset demandé sur le component depuis l'inspector)
    // equivalent du RequireComponent mais plus de possibilité (plusieurs components) ou intialisation automatique des paramètres public 
    void Reset()
    {
        if (GetComponent<MeshFilter>() == null)
            EditorUtility.DisplayDialog("Le component MeshFilter est nécessaire à la sélection", "MeshCollider ajouté au gameObject ! ", "j'ai compris ! ");
        //else        {
        //    EditorUtility.DisplayDialog("MeshFilter initial supprimé avant génération procédurale ! ", "Le script génére un mesh procédural . Le mesh existant " + GetComponent<MeshFilter>().mesh.name + " a été disocié de  l'objet " + gameObject.name, "j'ai compris ! ");
        //    DestroyImmediate(GetComponent<MeshFilter>());
        //}
        
        gameObject.AddComponent<MeshFilter>();

        Collider[] colliders = GetComponents<Collider>();
        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
                DestroyImmediate(collider);
            EditorUtility.DisplayDialog("Un seul MeshCollider", "Tous les collider sont supprimés ; seuls un meshCollider est associé au terrain", "OK ! ");
        }
        else
            EditorUtility.DisplayDialog("Le component MeshCollider est nécessaire à la sélection", "MeshCollider ajouté au gameObject ! ", "j'ai compris ! ");
        p_meshCollider = gameObject.AddComponent<MeshCollider>();
        
        if (GetComponent<MeshRenderer>() == null)
        {
            EditorUtility.DisplayDialog("Le component MeshRenderer est nécessaire à la sélection", "MeshCollider ajouté au gameObject ! ", "j'ai compris ! ");
            gameObject.AddComponent<MeshRenderer>();
        }


        
    }




    void Awake()
    {
        p_cam = Camera.main;

        // les rayCast exploiteront cette layer pour ne tester la collision qu'avec l'objet terrain
        gameObject.layer = LayerMask.NameToLayer("L_PickingTerrain");
        maskPickingTerrain = LayerMask.GetMask("L_PickingTerrain");

        /*   A COMPLETER !! */
   }





        private void appliquerDeformation_Fonction()
    {
            /*   A COMPLETER !! 
            switch (typeFonction)
        {
            case TypeFonction.Sinusoide:
                 //   A COMPLETER !! 
                break;
            case TypeFonction.Collines:
                //  A COMPLETER !! 
                break;
            case TypeFonction.Perlin:
                //  A COMPLETER !!
            }
            break;
        }
            */

        
 //   A COMPLETER !!
        
     }


    private void appliquerDeformation_Texture()
    {
    //   A COMPLETER !! 
}

private void calculerNormaleVertex(uint num_Vertex)
    {
    //   A COMPLETER !!
}

private void creerLeMeshTerrain()
    {
        //   A COMPLETER !!

    }

    void Start()
    {
        creerLeMeshTerrain();


    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            choixModeDeformation = ChoixModeDeformation.Fonction;
            appliquerDeformation_Fonction();
            // la fonction suivante sera utilisée au prochain appel     
            typeFonction = (TypeFonction)(((int)typeFonction + 1) % 3);
        }
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            choixModeDeformation = ChoixModeDeformation.Texture;
            appliquerDeformation_Texture();
            // l'image'suivante sera utilisée au prochain appel     
            //numImage = (numImage+ 1) % nbImages);
        }


    }
}

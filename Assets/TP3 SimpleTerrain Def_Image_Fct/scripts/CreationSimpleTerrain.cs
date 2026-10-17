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
    [Tooltip("La resolution sera 2 élévé à la puissance de cette valeur 2^n ")]
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
    public List<Texture2D> textures;




    private uint p_dimVertices;
    private uint p_dimTriangles;

    private MeshCollider p_meshCollider;
    private MeshFilter p_meshFilter;
    private Mesh p_mesh;
    private Vector3[] p_vertices;
    private Vector3[] p_normals;
    private int[] p_triangles;



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
        switch (typeFonction)
        {
            case TypeFonction.Sinusoide:

                float amplitude = 10f;
                float frequence = 0.1f;

                for (int i = 0; i < p_vertices.Length; i++)
                {
                    Vector3 vertex = p_vertices[i];

                    vertex.y = amplitude * Mathf.Sin(vertex.x * frequence);

                    p_vertices[i] = vertex;
                }
                break;

            case TypeFonction.Collines:
                {
                    float hauteur = 20f;
                    float largeur = 15f;

                    float centreX = 0f;
                    float centreZ = 0f;

                    for (int i = 0; i < p_vertices.Length; i++)
                    {
                        Vector3 vertex = p_vertices[i];

                        float dx = vertex.x - centreX;
                        float dz = vertex.z - centreZ;

                        float distanceCarree = dx * dx + dz * dz;

                        vertex.y = hauteur * Mathf.Exp(
                            -distanceCarree / (2f * largeur * largeur)
                        );

                        p_vertices[i] = vertex;
                    }
                    break;
                }

            case TypeFonction.Perlin:
                {
                    float amplitudePerlin = 20f;
                    float frequencePerlin = 0.05f;

                    // Ajout d'aléatoire au bruit de Perlin pour un effet plus natuel de montagne
                    float offsetX = Random.Range(0f, 1000f);
                    float offsetZ = Random.Range(0f, 1000f);

                    for (int i = 0; i < p_vertices.Length; i++)
                    {
                        Vector3 vertex = p_vertices[i];
                        float bruit = Mathf.PerlinNoise(
                        vertex.x * frequencePerlin + offsetX,
                        vertex.z * frequencePerlin + offsetZ
                        );

                        vertex.y = bruit * amplitudePerlin;
                        p_vertices[i] = vertex;
                    }
                    break;
                }
        }
        // Actualisation pos Mesh
        p_mesh.vertices = p_vertices;

        // Recalcul normales
        p_mesh.RecalculateNormals();

        // Actualisation limites Mesh
        p_mesh.RecalculateBounds();

        // Actu meshCollider
        p_meshCollider.sharedMesh = null;
        p_meshCollider.sharedMesh = p_mesh;
    }
    private void appliquerDeformation_Texture()
    {
        // A COMPLETER !!
    }



    private void creerColline(Vector3 positionClic)
    {
        // Hauteur aleat colline
        float hauteur = Random.Range(10f, 30f);
        // Largeur aleat colline
        float largeur = Random.Range(10f, 25f);

        // Conversion de la position du clic en coordonnées locales
        Vector3 centre = transform.InverseTransformPoint(positionClic);

        for (int i = 0; i < p_vertices.Length; i++)
        {
            Vector3 vertex = p_vertices[i];

            float dx = vertex.x - centre.x;
            float dz = vertex.z - centre.z;

            float distanceCarree = dx * dx + dz * dz;

            float elevation = hauteur * Mathf.Exp(
                -distanceCarree / (2f * largeur * largeur)
            );

            // Colline
            vertex.y += elevation;

            p_vertices[i] = vertex;
        }

        // Actualisation maillage
        p_mesh.vertices = p_vertices;
        p_mesh.RecalculateNormals();
        p_mesh.RecalculateBounds();

        // Recharger quand clic
        p_meshCollider.sharedMesh = null;
        p_meshCollider.sharedMesh = p_mesh;
    }

    private void calculerNormaleVertex(uint num_Vertex)
    {
    //   A COMPLETER !!
}

    private void creerLeMeshTerrain()
    {
        //Puissqnce de 2 resolution
        resolution = (ushort)(1 << puissance2Resolution);

        //Nb vertices
        p_dimVertices = (uint)resolution * resolution;

        // Nb triqngle
        p_dimTriangles = 2u * ((uint)resolution - 1) * ((uint)resolution - 1);

        // dist entre deux vertices voisin
        p_dimInterVertices = dimension / (resolution - 1);

        // Tableau de vertices
        p_vertices = new Vector3[(int)p_dimVertices];

        // Tableau normales
        p_normals = new Vector3[(int)p_dimVertices];

        // Tableau coordonnées UV
        Vector2[] uv = new Vector2[(int)p_dimVertices];

        // Tableau indices triangles
        p_triangles = new int[(int)p_dimTriangles * 3];

        // Pos vertices
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = z * resolution + x;
                float positionX = x * p_dimInterVertices;
                float positionZ = z * p_dimInterVertices;

                // centrage pivot pos
                if (CentrerPivot)
                {
                    positionX -= dimension / 2f;
                    positionZ -= dimension / 2f;
                }
                p_vertices[index] = new Vector3(positionX, 0, positionZ);

                // Coordonnées uv vertex
                float u = (float)x / (resolution - 1);
                float v = (float)z / (resolution - 1);
                uv[index] = new Vector2(u, v);

                // Normale vertex haut
                p_normals[index] = Vector3.up;
            }
        }
        // Création des triangles
        int indexTriangle = 0;

        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int basGauche = z * resolution + x;
                int basDroit = basGauche + 1;
                int hautGauche = basGauche + resolution;
                int hautDroit = hautGauche + 1;

                // Triangle 1
                p_triangles[indexTriangle++] = basGauche;
                p_triangles[indexTriangle++] = hautGauche;
                p_triangles[indexTriangle++] = basDroit;

                // Triangle d'apres
                p_triangles[indexTriangle++] = basDroit;
                p_triangles[indexTriangle++] = hautGauche;
                p_triangles[indexTriangle++] = hautDroit;
            }
        }

        // Création Mesh
        p_mesh = new Mesh();
        p_mesh.name = "Terrain Procedural";

        // Pouvoir avoir + de 65535 vertices
        p_mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Données Mesh
        p_mesh.vertices = p_vertices;
        p_mesh.triangles = p_triangles;
        p_mesh.uv = uv;
        p_mesh.normals = p_normals;

        // Recalcul des limites Mesh
        p_mesh.RecalculateBounds();

        // Avoir les composants
        p_meshFilter = GetComponent<MeshFilter>();
        p_meshCollider = GetComponent<MeshCollider>();

        // Partager au maillage
        p_meshFilter.sharedMesh = p_mesh;
        p_meshCollider.sharedMesh = p_mesh;
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
        // Clic impact
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray rayon = p_cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit impact;
            // Impact, vérif si ça marche, Debug
            if (Physics.Raycast(rayon, out impact, Mathf.Infinity, maskPickingTerrain))
            {
                Debug.Log("clic terrain: " + impact.point);
                // Clic coline
                if (choixModeDeformation == ChoixModeDeformation.Fonction &&
                    typeFonction == TypeFonction.Collines)
                {
                    creerColline(impact.point);
                }
            }
        }


    }
}

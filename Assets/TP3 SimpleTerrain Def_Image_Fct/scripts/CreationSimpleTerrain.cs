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

    // Les différentes méthodes de calcul des normales
    public enum ModeCalculNormales
    {
        Basique,
        Surface,
        Angle
    }

    // La methode selectionnée par user
    public ModeCalculNormales modeCalculNormales = ModeCalculNormales.Basique;

    [Tooltip("Choix de la première fonction à appliquer ")]
    public TypeFonction typeFonction;

    // Fonction actuelle sur terrain
    private TypeFonction fonctionActive;
    // Fonction bien générée avec F2?
    private bool fonctionGeneree = false;


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

    // Liste les triangles appartenant au vertex, triangle voisin
    private List<int>[] p_trianglesVoisins;





    private Camera p_cam;
    private LayerMask maskPickingTerrain;

    private float p_dimInterVertices;

    // Afficher / masquer normales
    private bool afficherNormales = false;

    // Appuie numéro 1 sur F11
    private bool attenteDeuxiemeF11 = false;


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

        // Calcul normales avec méthode
        calculerToutesLesNormales();

        // Actualisation limites Mesh
        p_mesh.RecalculateBounds();

        // Actu meshCollider
        p_meshCollider.sharedMesh = null;
        p_meshCollider.sharedMesh = p_mesh;
    }

    private void appliquerDeformation_Texture()
    {
        // Vérif HeightMaps bien présente
        if (textures == null || textures.Count == 0)
        {
            Debug.LogWarning("Pas de HeightMap");
            return;
        }

        // Vérif bon num texture
        if (numTexture < 0 || numTexture >= textures.Count)
        {
            Debug.LogWarning("Num HeightMap incorrect");
            return;
        }

        // Avoir HeightMap sélectionné
        Texture2D texture = textures[numTexture];

        if (texture == null)
        {
            Debug.LogWarning("HeightMap non assignée");
            return;
        }

        // Hauteur Max relief
        float hauteurMax = 30f;

        // Parcourir les vertices (terrain)
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int index = z * resolution + x;

                // Coordonnées HeightMap
                float u = (float)x / (resolution - 1);
                float v = (float)z / (resolution - 1);

                // Niveau de gris image
                float niveauGris = texture.GetPixelBilinear(u, v).grayscale;

                // Modif hauteur vertex
                Vector3 vertex = p_vertices[index];
                vertex.y = niveauGris * hauteurMax;
                p_vertices[index] = vertex;
            }
        }

        // Remise vertices du maillage
        p_mesh.vertices = p_vertices;

        // RecalculerToutes les normales
        calculerToutesLesNormales();

        // Remise limites du maillage
        p_mesh.RecalculateBounds();

        // Remise collisions
        p_meshCollider.sharedMesh = null;
        p_meshCollider.sharedMesh = p_mesh;
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
        calculerToutesLesNormales();


        //Partie ajoutee non demandé pour tester si bien changement
        // Comparer les 3 méthodes de calcul de normales
        comparerNormales();

        p_mesh.RecalculateBounds();

        // Recharger quand clic
        p_meshCollider.sharedMesh = null;
        p_meshCollider.sharedMesh = p_mesh;
    }



    //Ajout partieB_suite (voir git)
    private Vector3 calculerNormaleTriangle(int numTriangle)
    {
        // Indices des vertices
        int index = numTriangle * 3;

        int i0 = p_triangles[index];
        int i1 = p_triangles[index + 1];
        int i2 = p_triangles[index + 2];

        // Positions
        Vector3 v0 = p_vertices[i0];
        Vector3 v1 = p_vertices[i1];
        Vector3 v2 = p_vertices[i2];

        // Vecteur triangle
        Vector3 vecteur1 = v1 - v0;
        Vector3 vecteur2 = v2 - v0;

        // Avoir la normale
        Vector3 normale = Vector3.Cross(vecteur1, vecteur2);

        return normale.normalized;
    }

    // Nouvelle methode
    private Vector3 calculerNormaleTriangleSurface(int numTriangle)
    {
        int index = numTriangle * 3;

        int i0 = p_triangles[index];
        int i1 = p_triangles[index + 1];
        int i2 = p_triangles[index + 2];

        Vector3 v0 = p_vertices[i0];
        Vector3 v1 = p_vertices[i1];
        Vector3 v2 = p_vertices[i2];

        Vector3 vecteur1 = v1 - v0;
        Vector3 vecteur2 = v2 - v0;

        // Normale non normalisée, proportionnelle à la surface
        return Vector3.Cross(vecteur1, vecteur2);
    }

    // Calcul angle vertex
    private Vector3 calculerNormaleTriangleAngle(int numTriangle, int numVertex)
    {
        int index = numTriangle * 3;

        // Indices vertices triangle
        int i0 = p_triangles[index];
        int i1 = p_triangles[index + 1];
        int i2 = p_triangles[index + 2];

        // Position vertex ciblé et sommets
        Vector3 centre = p_vertices[numVertex];
        Vector3 v1;
        Vector3 v2;

        if (numVertex == i0)
        {
            v1 = p_vertices[i1];
            v2 = p_vertices[i2];
        }
        else if (numVertex == i1)
        {
            v1 = p_vertices[i2];
            v2 = p_vertices[i0];
        }
        else
        {
            v1 = p_vertices[i0];
            v2 = p_vertices[i1];
        }

        // Vecteurs partant du vertex ciblé
        Vector3 vecteur1 = v1 - centre;
        Vector3 vecteur2 = v2 - centre;

        // Calcul angle sur vertex
        float angle = Vector3.Angle(vecteur1, vecteur2);

        // Normale triangle (pondérée par l'angle)
        return calculerNormaleTriangle(numTriangle) * angle;
    }




    // PartieB_suite
    private void calculerNormaleVertex(uint num_Vertex)
    {
        // Normale cumulé triangles voisins
        Vector3 normale = Vector3.zero;

        // Avoir triangles voisins de ce vertex
        List<int> trianglesVoisins = p_trianglesVoisins[(int)num_Vertex];

        //PartieB_suite
        foreach (int numTriangle in trianglesVoisins)
        {
            Vector3 normaleTriangle = Vector3.zero;

            // Mode calcul des normales
            switch (modeCalculNormales)
            {
                case ModeCalculNormales.Basique:
                    normaleTriangle = calculerNormaleTriangle(numTriangle);
                    break;

                case ModeCalculNormales.Surface:
                    normaleTriangle = calculerNormaleTriangleSurface(numTriangle);
                    break;

                case ModeCalculNormales.Angle:
                    normaleTriangle = calculerNormaleTriangleAngle(
                        numTriangle,
                        (int)num_Vertex
                    );
                    break;
            }

            // Ajouter normale triangle
            normale += normaleTriangle;
        }

        // Normalize normale
        normale.Normalize();

        // Stocker normale num vertex
        p_normals[(int)num_Vertex] = normale;
    }
    private void calculerToutesLesNormales()
    {
        // Parcourir vertices
        for (uint i = 0; i < p_dimVertices; i++)
        {
            calculerNormaleVertex(i);
        }
        // Mettre normales sur maillage
        p_mesh.normals = p_normals;
    }

    //J'ai fait ça en plus parce que j'arrivais pas à voir les dif entre les methodes (sur les normales / traits verts)
    //C'était pas demandé
    private void comparerNormales()
    {
        // Prendre un vertex sur une pente de montagne
        int numVertex = resolution * (resolution / 2) + resolution / 2;

        Vector3 normaleBasique = Vector3.zero;
        Vector3 normaleSurface = Vector3.zero;
        Vector3 normaleAngle = Vector3.zero;

        // Avoir les triangles voisins
        List<int> trianglesVoisins = p_trianglesVoisins[numVertex];
        foreach (int numTriangle in trianglesVoisins)
        {
            normaleBasique += calculerNormaleTriangle(numTriangle);
            normaleSurface += calculerNormaleTriangleSurface(numTriangle);
            normaleAngle += calculerNormaleTriangleAngle(
                numTriangle,
                numVertex
            );
        }
        // Normalize sur les 3 méthodes
        normaleBasique.Normalize();
        normaleSurface.Normalize();
        normaleAngle.Normalize();
        // Mesurer diff orientation
        float differenceSurface = Vector3.Angle(
            normaleBasique,
            normaleSurface
        );

        float differenceAngle = Vector3.Angle(
            normaleBasique,
            normaleAngle
        );

        Debug.Log("Vertex testé : " + numVertex);
        Debug.Log("Différence Basique / Surface : " + differenceSurface + "°");
        Debug.Log("Différence Basique / Angle : " + differenceAngle + "°");
    }

    //Fin partie supp pour vérifier normales

    //Afficher normales (Debug.DrawRay)
    private void afficherLesNormales()
    {
        // Longueur traits normales
        float longueur = 2f;
        // Parcourir vertices
        for (int i = 0; i < p_vertices.Length; i++)
        {
            // Pos vertex coordonnées
            Vector3 position = transform.TransformPoint(p_vertices[i]);
            // Direction normale coordonnées
            Vector3 direction = transform.TransformDirection(p_normals[i]);
            // Dessiner normale
            Debug.DrawRay(position, direction * longueur, Color.green);
        }
    }


    // Remise à 0 du terrain
    private void reinitialiserTerrain()
    {
        // Remise vertices hauteur 0
        for (int i = 0; i < p_vertices.Length; i++)
        {
            Vector3 vertex = p_vertices[i];
            vertex.y = 0f;
            p_vertices[i] = vertex;
        }

        // Remise pos
        p_mesh.vertices = p_vertices;
        // Remise normales
        calculerToutesLesNormales();
        // Remise maillage
        p_mesh.RecalculateBounds();
        // Remise colision
        p_meshCollider.sharedMesh = null;
        p_meshCollider.sharedMesh = p_mesh;
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





        // Nouvelle liste des triangles voisins (pour chacun des différents vertex)
        p_trianglesVoisins = new List<int>[(int)p_dimVertices];

        for (int i = 0; i < p_trianglesVoisins.Length; i++)
        {
            p_trianglesVoisins[i] = new List<int>();
        }




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


                //Rajout partie B suite (voir git)

                // Triangle 1 et 2 sur carré actuel
                int triangle1 = (indexTriangle / 3) - 2;
                int triangle2 = (indexTriangle / 3) - 1;

                // Triangle 1 vertices
                p_trianglesVoisins[basGauche].Add(triangle1);
                p_trianglesVoisins[hautGauche].Add(triangle1);
                p_trianglesVoisins[basDroit].Add(triangle1);
                // Triangle 2 vertices
                p_trianglesVoisins[basDroit].Add(triangle2);
                p_trianglesVoisins[hautGauche].Add(triangle2);
                p_trianglesVoisins[hautDroit].Add(triangle2);
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

        //Ajout partieB_suite (voir git)

        // Affichage vérif triangles voisins d'un vertex
        if (resolution >= 4)
        {
            int indexVertex = resolution + 1;

            Debug.Log("Vertex" + indexVertex +
                      ": " + p_trianglesVoisins[indexVertex].Count +
                      " triangles voisins");
        }
    }

    void Start()
    {
        creerLeMeshTerrain();


    }

    // Update is called once per frame
    void Update()
    {
        // F1 : afficher l'aide
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            Debug.Log(
                "Commandes Terrain------------------------------------------------------------\n" +
                "F1 : Afficher l'aide\n" +
                "F2 : Changer la fonction (fonction dispo : Sinusoide, Collines, Perlin)\n" +
                "F3 : Changer la HeightMap\n" +
                "F10 : Afficher / masquer normales\n" +
                "F11 (2 fois) : Remettre le terrain à 0\n" +
                "F12 : Changer calcul normales\n" +
                "Clic souris gauche : Ajouter une colline\n" +
                "Déplacement cam (AZERTY normalement pris en charge si pb tester QWERTY)-------\n" +
                "W / Z : Avancer \n" +
                "A / Q : Aller à gauche\n" +
                "S : Reculer\n" +
                "D : Aller à droite\n" +
                "Clic droit enfoncé : Tourner cam\n" +
                "Molette : Zoom avant / arrière\n"
            );
        }

        //Modif ajoutée pour que pas de pb fonction (pour que l'on puisse bien clic sans pb après)
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            choixModeDeformation = ChoixModeDeformation.Fonction;

            // Enregister fonction appliquée
            fonctionActive = typeFonction;
            // Appliquer déformation
            appliquerDeformation_Fonction();
            // Fonction bien générée
            fonctionGeneree = true;
            // Prépa fonction d'après
            typeFonction = (TypeFonction)(((int)typeFonction + 1) % 3);
        }

        //Modif pour changement Heightmap
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            choixModeDeformation = ChoixModeDeformation.Texture;

            // Mettre la  bonne HeightMap ( celle sélectionnée)
            appliquerDeformation_Texture();

            // Prépa pour prochaine HeightMap
            if (textures != null && textures.Count > 0)
            {
                numTexture = (numTexture + 1) % textures.Count;
            }
        }


        //Ajout entrée clavier

        // F12 pour changer methodes
        if (Keyboard.current.f12Key.wasPressedThisFrame)
        {
            // Méthode d'après
            modeCalculNormales = (ModeCalculNormales)(((int)modeCalculNormales + 1) % 3);

            // Recalcul normale sans tout casser terrain
            calculerToutesLesNormales();

            Debug.Log("Mode de calcul des normales :" + modeCalculNormales);
        }
        // F10 pour afficher / masquer normales
        if (Keyboard.current.f10Key.wasPressedThisFrame)
        {
            afficherNormales = !afficherNormales;
            Debug.Log("Affichage des normales : " + afficherNormales);
        }
        // F11 pour remise a 0 du terrain (F11*2)
        if (Keyboard.current.f11Key.wasPressedThisFrame)
        {
            if (!attenteDeuxiemeF11)
            {
                // Premier appui
                attenteDeuxiemeF11 = true;
                Debug.Log("Appuyez de nouveau sur F11 pour remise à 0 du terrain");
            }
            else
            {
                // Deuxième appui
                reinitialiserTerrain();
                attenteDeuxiemeF11 = false;

                Debug.Log("Terrain remis à 0");
            }
        }
        // Dessin normales quand activées
        if (afficherNormales)
        {
            afficherLesNormales();
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
                    fonctionGeneree &&
                    fonctionActive == TypeFonction.Collines)
                {
                    creerColline(impact.point);
                }
            }
        }


    }
}

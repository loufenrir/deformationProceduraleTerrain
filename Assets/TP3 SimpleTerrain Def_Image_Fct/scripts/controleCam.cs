using UnityEngine;
using UnityEngine.InputSystem;

//Attention je suis en qwerty normalement j'ai testé aussi pour azerty c'est sensé fonctionner aussi
//Vitesse du zoom pour molette à mettre à 0.5 au lieu de 0.05 dans l'inspector pour zoom / dézoom plus prononcé
//Contrôles affichés dans F1

public class ControleCamera : MonoBehaviour
{
    // Vitesse
    public float vitesseDeplacement = 20f;
    // Sensibilité souris
    public float sensibiliteSouris = 0.15f;
    // Vitesse zoom
    public float vitesseZoom = 0.05f;

    // Terrain rotation
    public Transform terrain;

    // Vitesse rotation terrain
    public float vitesseRotationTerrain = 0.2f;

    void Update()
    {
        // Vérif clavier ok?
        if (Keyboard.current == null)
            return;

        Vector3 direction = Vector3.zero;

        // Avancer / reculer
        if (Keyboard.current.wKey.isPressed)
            direction += transform.forward;
        if (Keyboard.current.sKey.isPressed)
            direction -= transform.forward;

        // Gauche / droite
        if (Keyboard.current.aKey.isPressed)
            direction -= transform.right;
        if (Keyboard.current.dKey.isPressed)
            direction += transform.right;

        // Déplacer cam
        transform.position += direction.normalized * vitesseDeplacement * Time.deltaTime;


        // Rot caméra (clic droit)
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            Vector2 mouvementSouris = Mouse.current.delta.ReadValue();

            // Maj bien enfoncée?
            bool majEnfoncee = Keyboard.current.leftShiftKey.isPressed ||
                               Keyboard.current.rightShiftKey.isPressed;

            if (majEnfoncee && terrain != null)
            {
                Debug.Log("Rotation terrain détectée");

                // Rotation terrain axe haut
                terrain.Rotate(
                    0f,
                    mouvementSouris.x * vitesseRotationTerrain,
                    0f,
                    Space.World
                );
            }
            else if (!majEnfoncee)
            {
                // Rotation cam
                transform.Rotate(
                    -mouvementSouris.y * sensibiliteSouris,
                    mouvementSouris.x * sensibiliteSouris,
                    0f,
                    Space.Self
                );
            }
        }

        // Voir si fenêtre d'aide est ouverte
        CreationSimpleTerrain terrainActuel = FindFirstObjectByType<CreationSimpleTerrain>();

        // Zoom avec molette
        if (terrainActuel == null || !terrainActuel.afficherAide)
        {
            if (Mouse.current != null)
            {
                float molette = Mouse.current.scroll.ReadValue().y;

                if (molette != 0f)
                {
                    transform.position += transform.forward * molette * vitesseZoom;
                }
            }
        }

    }
}
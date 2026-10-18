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

            transform.Rotate(
                -mouvementSouris.y * sensibiliteSouris,
                mouvementSouris.x * sensibiliteSouris,
                0f,
                Space.Self
            );
        }

        // Zoom avec molette
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
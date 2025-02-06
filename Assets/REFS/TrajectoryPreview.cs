using UnityEngine;
using UnityEngine.SceneManagement;
using Wanderer;

public class TrajectoryPreview : MonoBehaviour
{
    [Header("Paramètres de simulation")]
    public int simulationSteps = 100;      // Nombre d'étapes de simulation
    public float simulationTimeStep = 0.02f; // Intervalle de temps entre chaque étape

    // Variables pour la scène de simulation
    private PhysicsScene physicsScene;
    private Scene simulationScene;
    private bool simulationSceneCreated = false;

    // On dessine les gizmos dans l'éditeur ou en mode play
    private void OnDrawGizmos()
    {
        // Si le jeu n'est pas lancé, on ne simule pas.
        if (!Application.isPlaying)
            return;

        // Crée la scène de simulation si ce n'est pas déjà fait
        if (!simulationSceneCreated)
            CreateSimulationScene();

        // Instancier une copie de l'objet dans la scène de simulation
        GameObject simulatedObject = Instantiate(gameObject, transform.position, transform.rotation);
        // On désactive ce script sur la copie pour éviter qu'elle ne lance sa propre simulation
        DestroyImmediate(simulatedObject.GetComponent<TrajectoryPreview>());

        // On déplace la copie dans la scène de simulation
        SceneManager.MoveGameObjectToScene(simulatedObject, simulationScene);

        // Récupérer le Rigidbody de la copie
        Rigidbody simulatedRb = simulatedObject.GetComponent<Rigidbody>();
        Rigidbody originalRb = GetComponent<Rigidbody>();

        // Synchroniser les états (vitesse, vitesse angulaire, etc.)
        simulatedRb.linearVelocity = originalRb.linearVelocity;
        simulatedRb.angularVelocity = originalRb.angularVelocity;

        // Tracer la trajectoire
        Vector3 previousPosition = simulatedObject.transform.position;
        Gizmos.color = Color.cyan;  // Choisissez la couleur souhaitée

        // Simuler la physique pas à pas
        for (int i = 0; i < simulationSteps; i++)
        {
            physicsScene.Simulate(simulationTimeStep);
            Vector3 currentPosition = simulatedObject.transform.position;
            Gizmos.DrawLine(previousPosition, currentPosition);
            previousPosition = currentPosition;
        }

        // Détruire l'objet simulé pour nettoyer la scène de simulation
        DestroyImmediate(simulatedObject);
    }

    // Création d'une scène de simulation dédiée avec physique
    private void CreateSimulationScene()
    {
        CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        simulationScene = SceneManager.CreateScene("Simulation", csp);
        physicsScene = simulationScene.GetPhysicsScene();
        simulationSceneCreated = true;
    }
}
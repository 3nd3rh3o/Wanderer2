using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

#if UNITY_EDITOR
namespace Wanderer
{
    public class PhysicControllerPathPreview : MonoBehaviour
    {
        [Header("Simulation parameters")]
        public int simulationSteps = 100;
        public float simulationTimeStep = 0.02f;

        // sim vars
        private PhysicsScene physicsScene;
        private Scene simulationScene;
        private bool simulationSceneCreated = false;

        // draw Gizoms in editor and playmode.
        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            if (!simulationSceneCreated)
            {
                CreateSimulationScene();
            }

            GameObject simulatedObject = Instantiate(gameObject, transform.position, transform.rotation);
            DestroyImmediate(simulatedObject.GetComponent<PhysicControllerPathPreview>());

            SceneManager.MoveGameObjectToScene(simulatedObject, simulationScene);

            // obtain all rb's and sync their params.
            Rigidbody[] simulatedRbs = new Rigidbody[simulatedObject.transform.childCount];
            Rigidbody[] originalRbs = new Rigidbody[transform.childCount];

            Vector3[] previousPositions = new Vector3[simulatedRbs.Length];


            for (int i = 0; i < simulatedRbs.Length; i++)
            {
                simulatedRbs[i] = simulatedObject.transform.GetChild(i).GetComponent<Rigidbody>();
                originalRbs[i] = transform.GetChild(i).GetComponent<Rigidbody>();
                simulatedRbs[i].linearVelocity = originalRbs[i].linearVelocity;
                simulatedRbs[i].angularVelocity = originalRbs[i].angularVelocity;
                previousPositions[i] = simulatedRbs[i].transform.position;
            }
            Gizmos.color = Color.cyan;
            // for each child of the controller, draw their trajectory with gizmos.
            for (int i = 0; i < simulationSteps; i++)
            {
                physicsScene.Simulate(simulationTimeStep);
                for (int j = 0; j < simulatedRbs.Length; j++)
                {
                    Vector3 currentPosition = simulatedRbs[j].transform.position;
                    Gizmos.DrawLine(previousPositions[j], currentPosition);
                    previousPositions[j] = currentPosition;
                }
            }

            // destroy simulated GO. (ensure it's childs got also destroyed)
            for (int i = 0; i < simulatedRbs.Length; i++)
            {
                DestroyImmediate(simulatedRbs[i].gameObject);
            }
            DestroyImmediate(simulatedObject);
        }
        private void CreateSimulationScene()
        {
            CreateSceneParameters csp = new(LocalPhysicsMode.Physics3D);
            simulationScene = SceneManager.CreateScene("Simulation", csp);
            physicsScene = simulationScene.GetPhysicsScene();
            simulationSceneCreated = true;
        }
    }
}
#endif
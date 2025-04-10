using System.Collections;
using UnityEngine;

namespace ReplayExampleScripts
{
    public class InstantReplayActivation : MonoBehaviour
    {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        public ReplayManager replay;
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                replay.SaveReplay("ShipsReplay");
            }

            if (Input.GetKeyDown(KeyCode.R) && !replay.ReplayMode())
            {
                replay.EnterReplayMode();
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                StartCoroutine(LoadAndEnterReplay());
            }
        }

        IEnumerator LoadAndEnterReplay()
        {
            foreach (ShipMovement ship in FindObjectsOfType<ShipMovement>())
            {
                Destroy(ship.gameObject);
            }

            replay.LoadReplay("ShipsReplay");
            yield return new WaitForEndOfFrame();
            
            if (!replay.ReplayMode())
            {
                replay.EnterReplayMode();
            }

=======
        // Update is called once per frame
        void Update()
        {
>>>>>>> Stashed changes
=======
        // Update is called once per frame
        void Update()
        {
>>>>>>> Stashed changes
=======
        // Update is called once per frame
        void Update()
        {
>>>>>>> Stashed changes

        }

        void Start()
        {
            if (replay == null)
            {
                replay = FindObjectOfType<ReplayManager>();
            }
        }
    }
}
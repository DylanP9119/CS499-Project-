using System.Collections;
using UnityEngine;
using Replay;

namespace ReplayExampleScripts
{
    public class InstantReplayActivation : MonoBehaviour
    {
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
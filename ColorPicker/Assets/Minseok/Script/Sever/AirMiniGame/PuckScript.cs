using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class PuckScript : MonoBehaviour
    {
        public ScoreScript ScoreScriptInstance;
        public GameObject Red;
        public GameObject Blue;
        public static bool WasGoal { get; private set; }
        private Rigidbody2D rb;
        public float MaxSpeed;

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            WasGoal = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!WasGoal)
            {
                if (other.tag == "AIGoal")
                {
                    ScoreScriptInstance.Increment(ScoreScript.Score.PlayerScore);
                    WasGoal = true;
                    //audioManager.PlayGoal();
                    StartCoroutine(ResetPuck(false));
                    
                }
                else if (other.tag == "PlayerGoal")
                {
                    ScoreScriptInstance.Increment(ScoreScript.Score.AiScore);
                    WasGoal = true;
                    //audioManager.PlayGoal();
                    StartCoroutine(ResetPuck(true));
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            //audioManager.PlayPuckCollision();
        }

        private IEnumerator ResetPuck(bool didAiScore)
        {
            yield return new WaitForSecondsRealtime(1);
            WasGoal = false;
            rb.velocity = rb.position = new Vector2(0, 0);

            // 각 플레이어를 초기 위치로 이동
            if (Red != null) Red.transform.position = new Vector3(7, 0, 0);
            if (Blue != null) Blue.transform.position = new Vector3(-7, 0, 0);

            if (didAiScore)
                rb.position = new Vector2(0, -1);
            else
                rb.position = new Vector2(0, 1);
        }

        private void FixedUpdate()
        {
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, MaxSpeed);
        }
    }
}
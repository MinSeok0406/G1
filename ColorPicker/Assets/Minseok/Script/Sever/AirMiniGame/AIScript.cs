using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class AIScript : MonoBehaviour
    {
        public float MaxMovementSpeed;
        private Rigidbody2D rb;
        private Vector2 startingPositionLocal;
        public Rigidbody2D Puck;

        public Transform PlayerBoundaryHolder;
        private Boundary playerBoundary;

        public Transform PuckBoundaryHolder;
        private Boundary puckBoundary;

        private Vector2 targetLocalPosition;

        private bool isFirstTimeInOpponentsHalf = true;
        private float offsetXFromTarget;

        public Transform Field; // 필드(플레이 공간) Transform 추가

        // 랜덤 오프셋 유지용
        private float randomXOffset = 0f;
        private float randomYOffset = 0f;
        private float offsetTimer = 0f;
        private float offsetUpdateInterval = 0.7f; // 오프셋 갱신 주기(초)

        private const float deadZone = 0.04f; // 이 거리 이내면 안 움직임

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            startingPositionLocal = Field.InverseTransformPoint(rb.position);

            playerBoundary = new Boundary(
                Field.InverseTransformPoint(PlayerBoundaryHolder.GetChild(0).position).y, // Up
                Field.InverseTransformPoint(PlayerBoundaryHolder.GetChild(1).position).y, // Down
                Field.InverseTransformPoint(PlayerBoundaryHolder.GetChild(2).position).x, // Left
                Field.InverseTransformPoint(PlayerBoundaryHolder.GetChild(3).position).x);  // Right

            puckBoundary = new Boundary(
                Field.InverseTransformPoint(PuckBoundaryHolder.GetChild(0).position).y,
                Field.InverseTransformPoint(PuckBoundaryHolder.GetChild(1).position).y,
                Field.InverseTransformPoint(PuckBoundaryHolder.GetChild(2).position).x,
                Field.InverseTransformPoint(PuckBoundaryHolder.GetChild(3).position).x);

            SetRandomOffset();
        }

        private void FixedUpdate()
        {
            if (!PuckScript.WasGoal)
            {
                // 랜덤 오프셋은 일정 시간마다만 갱신
                offsetTimer += Time.fixedDeltaTime;
                if (offsetTimer > offsetUpdateInterval)
                {
                    SetRandomOffset();
                    offsetTimer = 0f;
                }

                // 현재 위치, 퍽 위치 모두 Field의 로컬좌표로 변환
                Vector2 puckLocalPos = Field.InverseTransformPoint(Puck.position);
                Vector2 puckVelocity = Field.InverseTransformVector(Puck.velocity);

                float movementSpeed;
                float predictionTime = 0.2f;

                // 예측 위치 계산 (너무 멀리 예측하지 않게 Clamp)
                Vector2 predictedPuckPos = puckLocalPos + puckVelocity * predictionTime;

                // 실제 내 진영 구분: 내 진영은 playerBoundary.Down~playerBoundary.Up
                bool puckInMyZone = puckLocalPos.y <= playerBoundary.Up;

                // 공격/수비 모드 분리
                if (puckInMyZone)
                {
                    movementSpeed = Mathf.Lerp(MaxMovementSpeed * 0.55f, MaxMovementSpeed, 0.5f);

                    targetLocalPosition = new Vector2(
                        Mathf.Clamp(predictedPuckPos.x, playerBoundary.Left, playerBoundary.Right),
                        Mathf.Clamp(predictedPuckPos.y, playerBoundary.Down, playerBoundary.Up)
                    );
                }
                else 
                {
                    // 퍽이 상대 진영에 있으면, 수비적으로 내 중앙에서 대기
                    movementSpeed = MaxMovementSpeed * Random.Range(0.12f, 0.25f);

                    targetLocalPosition = new Vector2(
                        Mathf.Clamp(startingPositionLocal.x, playerBoundary.Left, playerBoundary.Right),
                        Mathf.Clamp(startingPositionLocal.y, playerBoundary.Down, playerBoundary.Up)
                    );
                }

                Vector2 targetWorldPos = Field.TransformPoint(targetLocalPosition);

                // dead zone(진동 방지): 목표 위치와 충분히 가까우면 이동하지 않음
                if ((rb.position - targetWorldPos).sqrMagnitude > deadZone * deadZone)
                {
                    rb.MovePosition(Vector2.MoveTowards(rb.position, targetWorldPos, movementSpeed * Time.fixedDeltaTime));
                }
                else
                {
                    rb.velocity = Vector2.zero;
                }
            }
        }

        void SetRandomOffset()
        {
            // 공격/수비 패턴 따라 다르게 하면 더욱 자연스럽게 가능
            randomXOffset = Random.Range(-0.13f, 0.13f);
            randomYOffset = Random.Range(-0.07f, 0.07f);
        }
    }
}
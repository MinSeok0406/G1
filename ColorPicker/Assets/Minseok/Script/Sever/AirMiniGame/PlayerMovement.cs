using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minseok
{
    public class PlayerMovement : MonoBehaviour
    {
        bool wasJustClicked = true;
        bool canMove;
        Vector2 playerSize;

        Rigidbody2D rb;

        public Transform BoundaryHolder;
        public Transform Field; // 필드(플레이 공간) Transform 추가

        Boundary playerBoundary;
        
        void Start()
        {
            playerSize = GetComponent<SpriteRenderer>().bounds.extents;
            rb = GetComponent<Rigidbody2D>();

            playerBoundary = new Boundary(
                Field.InverseTransformPoint(BoundaryHolder.GetChild(0).position).y, // Up
                Field.InverseTransformPoint(BoundaryHolder.GetChild(1).position).y, // Down
                Field.InverseTransformPoint(BoundaryHolder.GetChild(2).position).x, // Left
                Field.InverseTransformPoint(BoundaryHolder.GetChild(3).position).x  // Right
            );

        }
        
        void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Vector2 mouseLocalPos = Field.InverseTransformPoint(mouseWorldPos);
                Vector2 playerLocalPos = Field.InverseTransformPoint(transform.position);

                if (wasJustClicked)
                {
                    wasJustClicked = false;

                    if (Mathf.Abs(mouseLocalPos.x - playerLocalPos.x) <= playerSize.x &&
                        Mathf.Abs(mouseLocalPos.y - playerLocalPos.y) <= playerSize.y)
                    {
                        canMove = true;
                    }
                    else
                    {
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    Vector2 clampedLocalPos = new Vector2(
                        Mathf.Clamp(mouseLocalPos.x, playerBoundary.Left, playerBoundary.Right),
                        Mathf.Clamp(mouseLocalPos.y, playerBoundary.Down, playerBoundary.Up)
                    );
                    // 5. 다시 "월드좌표"로 변환해서 이동 적용
                    Vector2 clampedWorldPos = Field.TransformPoint(clampedLocalPos);
                    rb.MovePosition(clampedWorldPos);
                }
            }
            else
            {
                wasJustClicked = true;
            }
        }
    }
}
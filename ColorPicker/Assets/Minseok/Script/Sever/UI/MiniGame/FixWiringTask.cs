using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Minseok
{
    public enum EWireColor
    {
        None = -1,
        Red,
        Blue,
        Yellow,
        Magenta
    }

    public class FixWiringTask : MonoBehaviour
    {
        [SerializeField] 
        private List<LeftWire> mLeftWires;

        [SerializeField]
        private List<RightWire> mRightWires;

        private LeftWire mSelectedWire;

        private void OnEnable()
        {
            for (int i = 0; i < mLeftWires.Count; i++)
            {
                mLeftWires[i].ResetTarget();
                mLeftWires[i].DisconnectWire();
            }

            List<int> numberPool = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                numberPool.Add(i);
            }

            int index = 0;
            while (numberPool.Count != 0)
            {
                var number = numberPool[Random.Range(0, numberPool.Count)];
                mLeftWires[index++].SetWireColor((EWireColor)number);
                numberPool.Remove(number);
            }

            for (int i = 0; i < 4; i++)
            {
                numberPool.Add(i);
            }

            index = 0;
            while (numberPool.Count != 0)
            {
                var number = numberPool[Random.Range(0, numberPool.Count)];
                mRightWires[index++].SetWireColor((EWireColor)number);
                numberPool.Remove(number);
            }
        }

        void Update()
        {
            Vector2 pointerPos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : Input.mousePosition;

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(pointerPos, Vector2.right, 1f);
                if (hit.collider != null)
                {
                    var left = hit.collider.GetComponentInParent<LeftWire>();
                    if (left != null)
                    {
                        mSelectedWire = left;
                    }
                }
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetMouseButtonUp(0))
            {
                if (mSelectedWire != null)
                {
                    RaycastHit2D[] hits = Physics2D.RaycastAll(pointerPos, Vector2.right, 1f);
                    foreach (var hit in hits)
                    {
                        if (hit.collider != null)
                        {
                            var right = hit.collider.GetComponentInParent<RightWire>();
                            if (right != null)
                            {
                                mSelectedWire.SetTarget(hit.transform.position, -50f);
                                mSelectedWire.ConnectWire(right);
                                right.ConnectWire(mSelectedWire);
                                mSelectedWire = null;
                                CheckCompleteTast();
                                return;
                            }
                        }
                    }

                    mSelectedWire.ResetTarget();
                    mSelectedWire.DisconnectWire();
                    mSelectedWire = null;
                    CheckCompleteTast();
                }
            }

            if (mSelectedWire != null)
            {
                mSelectedWire.SetTarget(pointerPos, -15f);
            }
        }

        private void CheckCompleteTast()
        {
            bool isAllComplete = true;
            foreach (var wire in mLeftWires)
            {
                if (!wire.IsConnected)
                {
                    isAllComplete = false;
                    break;
                }
            }

            if (isAllComplete)
            {
                // TEMP Minseok
                // 미니게임 완수 후 작업 코드
                Debug.Log("임무 완료!!");
                Close();
            }
        }

        public void Open()
        {
            // TEMP Minseok
            // 플레이어 움직임 멈추게 하고 미니게임창 열리게 하기
        }

        public void Close()
        {
            // TEMP Minseok
            // 플레이어 움직임 가능하게 하고 미니게임창 닫기 
        }
    }
}
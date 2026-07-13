using UnityEngine;

public class StackSlot : MonoBehaviour
{
    public int capacity = 3;

    public Transform Top => transform.childCount > 0 ? transform.GetChild(transform.childCount - 1) : null;
    public bool IsFull => transform.childCount >= capacity;
    public bool Contains(Transform item) => item.parent == transform;
}
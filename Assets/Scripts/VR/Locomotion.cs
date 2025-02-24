//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

//public class Locomotion: ContinuousMoveProvider
//{
//    public enum MovementDirection
//    {
//        HeadRelative,
//        HandRelative,
//    }

//    [Space, Header("Movement Direction")]
//    [SerializeField] private MovementDirection movementDirection;
//    [SerializeField] private Transform m_HeadTransform;
//    [SerializeField] private Transform m_LeftControllerTransform;

//    protected override void Awake()
//    {
//        forwardSource = movementDirection == MovementDirection.HeadRelative ? m_HeadTransform : m_LeftControllerTransform;
//    }
//}
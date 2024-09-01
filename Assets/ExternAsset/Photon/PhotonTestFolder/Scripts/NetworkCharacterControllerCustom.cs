namespace Fusion
{
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using UnityEngine;

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [NetworkBehaviourWeaved(NetworkCCData.WORDS)]
    // ReSharper disable once CheckNamespace
    public sealed unsafe class NetworkCharacterControllerCustom : NetworkTRSP, INetworkTRSPTeleport, IBeforeAllTicks, IAfterAllTicks, IBeforeCopyPreviousState
    {
        new ref NetworkCCData Data => ref ReinterpretState<NetworkCCData>();

        [Header("Character Controller Settings")]
        public float gravity = -20.0f;
        public float jumpImpulse = 8.0f;
        public float acceleration = 10.0f;
        public float braking = 10.0f;
        public float maxSpeed = 2.0f;
        public float rotationSpeed = 15.0f;
        public float viewUpDownSpeed = 50.0f;

        Tick _initial;
        CharacterController _controller;

        public Vector3 Velocity
        {
            get => Data.Velocity;
            set => Data.Velocity = value;
        }

        public bool Grounded
        {
            get => Data.Grounded;
            set => Data.Grounded = value;
        }

        public void Teleport(Vector3? position = null, Quaternion? rotation = null)
        {
            _controller.enabled = false;
            NetworkTRSP.Teleport(this, transform, position, rotation);
            _controller.enabled = true;
        }


        public void Jump(bool ignoreGrounded = false, float? overrideImpulse = null)
        {
            if (Data.Grounded || ignoreGrounded)
            {
                var newVel = Data.Velocity;
                newVel.y += overrideImpulse ?? jumpImpulse;
                Data.Velocity = newVel;
            }
        }

        public void Move(Vector3 direction)
        {
            var deltaTime = Runner.DeltaTime;
            var previousPos = transform.position;
            var moveVelocity = Data.Velocity;

            direction = direction.normalized;

            if (Data.Grounded && moveVelocity.y < 0)
            {
                moveVelocity.y = 0f;
            }

            moveVelocity.y += gravity * Runner.DeltaTime;

            var horizontalVel = default(Vector3);
            horizontalVel.x = moveVelocity.x;
            horizontalVel.z = moveVelocity.z;

            if (direction == default)
            {
                horizontalVel = Vector3.Lerp(horizontalVel, default, braking * deltaTime);
            }
            else
            {
                horizontalVel = Vector3.ClampMagnitude(horizontalVel + direction * acceleration * deltaTime, maxSpeed);
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Runner.DeltaTime);
            }

            moveVelocity.x = horizontalVel.x;
            moveVelocity.z = horizontalVel.z;

            _controller.Move(moveVelocity * deltaTime);

            Data.Velocity = (transform.position - previousPos) * Runner.TickRate;
            Data.Grounded = _controller.isGrounded;
        }

        public override void Spawned()
        {
            _initial = default;
            TryGetComponent(out _controller);
            CopyToBuffer();
        }

        public override void Render()
        {
            NetworkTRSP.Render(this, transform, false, false, false, ref _initial);
        }

        void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount)
        {
            CopyToEngine();
        }

        void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount)
        {
            CopyToBuffer();
        }

        void IBeforeCopyPreviousState.BeforeCopyPreviousState()
        {
            CopyToBuffer();
        }

        void Awake()
        {
            TryGetComponent(out _controller);
        }

        void CopyToBuffer()
        {
            Data.TRSPData.Position = transform.position;
            Data.TRSPData.Rotation = transform.rotation;
        }

        void CopyToEngine()
        {
            // CC must be disabled before resetting the transform state
            _controller.enabled = false;

            // set position and rotation
            transform.SetPositionAndRotation(Data.TRSPData.Position, Data.TRSPData.Rotation);

            // Re-enable CC
            _controller.enabled = true;
        }

        public void Rotate(float rotationY)
        {
            transform.Rotate(0, rotationY * Runner.DeltaTime * rotationSpeed, 0);
        }
    }
}
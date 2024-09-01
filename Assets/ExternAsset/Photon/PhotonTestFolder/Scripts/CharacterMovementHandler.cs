using Fusion;
using UnityEngine;

public class CharacterMovementHandler : NetworkBehaviour
{
    // Rotation
    Camera _localCamera;

    // Other Component
    NetworkCharacterControllerCustom _networkdCharacterControllerCustom;

    void Awake()
    {
        _networkdCharacterControllerCustom = GetComponent<NetworkCharacterControllerCustom>();
        _localCamera = GetComponentInChildren<Camera>();
        
        
    }


    private void Start()
    {
        
    }
    
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData networkInputData))
        {

            transform.forward = networkInputData.aimForwardVector;

            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;


            // Move
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            _networkdCharacterControllerCustom.Move(moveDirection);

            // Jump
            if (networkInputData.isJumpPressed)
                _networkdCharacterControllerCustom.Jump();

            CheckFallRespawn();
        }
    }

    void CheckFallRespawn()
    {
        if(transform.position.y < -12)
        {
            transform.position = Utils.GetRandomSpawnPoint();
        }
    }
}

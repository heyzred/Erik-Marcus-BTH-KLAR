using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour {

	[SerializeField]
	private float speed = 5f;
	[SerializeField]
	private float lookSensitivity = 3f;

	[SerializeField]
	private float thrusterForce = 1000f;

	[SerializeField]
	private float thrusterFuelBurnSpeed = 1f;
	[SerializeField]
	private float thrusterFuelRegenSpeed = 0.3f;
	private float thrusterFuelAmount = 1f;

	public float GetThrusterFuelAmount ()
	{
		return thrusterFuelAmount;
	}

	[SerializeField]
	private LayerMask environmentMask;

	[Header("Spring settings:")]
	[SerializeField]
	private float jointSpring = 20f;
	[SerializeField]
	private float jointMaxForce = 40f;


	private PlayerMotor motor;
	private ConfigurableJoint joint;
	private Animator animator;

	void Start ()
	{
		motor = GetComponent<PlayerMotor>();
		joint = GetComponent<ConfigurableJoint>();
		animator = GetComponent<Animator>();

		SetJointSettings(jointSpring);
	}

	void Update ()
	{
		if (PauseMenu.IsOn)
		{
			if (Cursor.lockState != CursorLockMode.None)
				Cursor.lockState = CursorLockMode.None;

			motor.Move(Vector3.zero);
			motor.Rotate(Vector3.zero);
			motor.RotateCamera(0f);

			return;
		}

		if (Cursor.lockState != CursorLockMode.Locked)
		{
			Cursor.lockState = CursorLockMode.Locked;
		}


        // sätter fysiken rätt så att man kan flyga över objekt
		RaycastHit _hit;
		if (Physics.Raycast (transform.position, Vector3.down, out _hit, 100f, environmentMask))
		{
			joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
		} else
		{
			joint.targetPosition = new Vector3(0f, 0f, 0f);
		}

		//Kalkylerar velocity vectorn
		float _xMov = Input.GetAxis("Horizontal");
		float _zMov = Input.GetAxis("Vertical");

		Vector3 _movHorizontal = transform.right * _xMov;
		Vector3 _movVertical = transform.forward * _zMov;

		// Movement vectorn
		Vector3 _velocity = (_movHorizontal + _movVertical) * speed;

		// Animerar forwardVelocity vilket är när spelaren går fram "zMov"
		animator.SetFloat("ForwardVelocity", _zMov);

		//aktiverar movement
		motor.Move(_velocity);


        //kalkulerar rotationen som en 3D vector ( När man snurrar runt)
		float _yRot = Input.GetAxisRaw("Mouse X");

		Vector3 _rotation = new Vector3(0f, _yRot, 0f) * lookSensitivity;

		//aktiverar rotationen i motor skriptet
		motor.Rotate(_rotation);

            // kalkulerar kamera rotationen som en 3D vector (när man snurrar runt i Y led)
		float _xRot = Input.GetAxisRaw("Mouse Y");

		float _cameraRotationX = _xRot * lookSensitivity;

		//aktiverar kamera rotation i motor skriptet
		motor.RotateCamera(_cameraRotationX);


        //kalkulerar thrusterforce baserat på spelarens "JUMP" och om spelaren inte har mer än 0 i Fuel så flyger den inte
		Vector3 _thrusterForce = Vector3.zero;
		if (Input.GetButton ("Jump") && thrusterFuelAmount > 0f)
		{
			thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

			if (thrusterFuelAmount >= 0.01f)
			{
				_thrusterForce = Vector3.up * thrusterForce;
				SetJointSettings(0f);
			}
		} else
		{
			thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;
			SetJointSettings(jointSpring);
		}

		thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);

        // aktiverar thrusterforce i motor skriptet.
		motor.ApplyThruster(_thrusterForce);

	}

	private void SetJointSettings (float _jointSpring)
	{
		joint.yDrive = new JointDrive {
			positionSpring = _jointSpring,
			maximumForce = jointMaxForce
		};
	}

}

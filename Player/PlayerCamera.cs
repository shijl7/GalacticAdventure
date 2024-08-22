using UnityEngine;
using Cinemachine;
using System.Runtime.ExceptionServices;

[RequireComponent (typeof(CinemachineVirtualCamera))]
public class PlayerCamera : MonoBehaviour
{
	[Header("Camera Settings")]
	public Player player;
	public float maxDistance = 15f;
	public float initialAngle = 20f;
	public float heightOffset = 1f;

	[Header("Following Setting")]
	public float verticalUpDeadZone = 0.15f;
	public float verticalDownDeadZone = 0.15f;
	public float verticalAirUpDeadZone = 4f;
	public float verticalAirDownDeadZone = 0f;
	public float maxVerticalSpeed = 10f;
	public float maxAirVerticalSpeed = 100f;



	[Header("Orbit Setting")]
	public bool canOrbit = true;
	public bool canOrbitWithVelocity = true;
	public float orbitVelocityMultiplier = 5f;

	[Range(0, 90)]
	public float verticalMaxRotation = 80f;
	[Range(0, 90)]
	public float verticalMinRotation = -20f;


	protected CinemachineVirtualCamera m_camera;
	protected Cinemachine3rdPersonFollow m_cameraBody;
	protected CinemachineBrain m_brain;
	protected Transform m_target;
	protected float m_cameraDistance;
	protected float m_cameraTargetYaw;
	protected float m_cameraTargetPitch;
	protected Vector3 m_cameraTargetPosition;

	protected string k_targetName = "Player Follower Camera Target";

	protected virtual void Start()
	{
		InitializeComponent();
		InitializeFollower();
		InitializeCamera();
	}

	protected void LateUpdate()
	{
		HandleOrbit();
		HandleVelocityOrbit();
		HandleOffset();
		MoveTarget();
	}

	protected virtual void InitializeComponent()
	{
		if (!player)
		{
			player = FindObjectOfType<Player> ();
		}
		m_camera = GetComponent<CinemachineVirtualCamera> ();
		m_cameraBody = m_camera.AddCinemachineComponent<Cinemachine3rdPersonFollow> ();
		m_brain = Camera.main.GetComponent<CinemachineBrain> ();
	}

	protected virtual void InitializeFollower()
	{
		m_target = new GameObject(k_targetName).transform;
		m_target.position = player.transform.position;
	}

	protected virtual void InitializeCamera()
	{
		m_camera.Follow = m_target.transform;//此时再设置m_target的值就相当于设置了m_camera.Follow的值
		m_camera.LookAt = player.transform;
		Reset();
	}

	public virtual void Reset()
	{
		m_cameraDistance = maxDistance;
		m_cameraTargetPitch = initialAngle;
		m_cameraTargetYaw = player.transform.rotation.eulerAngles.y;
		//m_cameraTargetPosition等于角色上方的某点
		m_cameraTargetPosition = player.unsizePosition + Vector3.up * heightOffset;//unsizePosition具体是什么不太懂
		MoveTarget();
		m_brain.ManualUpdate();
	}

	protected virtual void MoveTarget()
	{
		//设置目标点的位置（初始化为角色上方的某点）
		m_target.position = m_cameraTargetPosition;
		m_target.rotation = Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0f);
		//设置相机距离目标点的距离
		m_cameraBody.CameraDistance = m_cameraDistance;
	}

	protected virtual void HandleOrbit()
	{
		if (canOrbit)
		{
			var direction = player.inputs.GetLookDirection();
			if (direction.sqrMagnitude > 0)
			{
				var usingMouse = player.inputs.IsLookingWithMouse();
				float deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;
				
				//根据鼠标输入的Vector2的值来设置目标点的旋转，即设置相机的旋转
				m_cameraTargetYaw += direction.x * deltaTimeMultiplier;
				m_cameraTargetPitch -= direction.z * deltaTimeMultiplier;
				//限制一下相机旋转的俯仰角
				m_cameraTargetPitch = ClampAngle(m_cameraTargetPitch, verticalMinRotation, verticalMaxRotation);

			}
		}
	}

	protected virtual void HandleVelocityOrbit()
	{
		if(canOrbitWithVelocity && player.isGrounded)
		{
			//将角色的速度向量转为m_target坐标系下的向量，然后使用角色x轴方向上的速度来旋转相机
			//将会实现：角色向左走，相机随之向左旋转，角色向右走，相机随之向右旋转
			var localVelocity = m_target.InverseTransformVector(player.velocity);
			m_cameraTargetYaw += localVelocity.x * orbitVelocityMultiplier * Time.deltaTime;
		}
	}

	protected virtual bool VerticalFollowingStates()
	{
		return player.states.IsCurrentOfType(typeof(SwimPlayerState)) ||
			player.states.IsCurrentOfType(typeof(WallDragPlayerState)) ||
			player.states.IsCurrentOfType(typeof(LedgeHangingPlayerState)) ||
			player.states.IsCurrentOfType(typeof(LedgeClimbingPlayerState)) ||
			player.states.IsCurrentOfType(typeof(RailGrindPlayerState));
	}

	protected virtual void HandleOffset()
	{
		var target = player.unsizePosition + Vector3.up * heightOffset;
		var previousPosition = m_cameraTargetPosition;
		var targetHeight = previousPosition.y;

		if(player.isGrounded || VerticalFollowingStates())
		{
			if(target.y > previousPosition.y + verticalUpDeadZone)
			{
				var offset = target.y - previousPosition.y - verticalUpDeadZone;
				targetHeight += Mathf.Min(offset, maxVerticalSpeed * Time.deltaTime);
			}
            else if(target.y < previousPosition.y + verticalDownDeadZone)
            {
				var offset = target.y - previousPosition.y + verticalUpDeadZone;
				targetHeight += Mathf.Max(offset, -maxVerticalSpeed * Time.deltaTime);
			}
		}
		else if (target.y > previousPosition.y + verticalAirUpDeadZone)
		{
			var offset = target.y - previousPosition.y - verticalAirUpDeadZone;
			targetHeight += Mathf.Min(offset, maxAirVerticalSpeed * Time.deltaTime);
		}
		else if (target.y < previousPosition.y - verticalAirDownDeadZone)
		{
			var offset = target.y - previousPosition.y + verticalAirDownDeadZone;
			targetHeight += Mathf.Max(offset, -maxAirVerticalSpeed * Time.deltaTime);
		}

		m_cameraTargetPosition = new Vector3(target.x, targetHeight, target.z);
	}



	protected virtual float ClampAngle(float angle, float min, float max)
	{
		if(angle < -360)
		{
			angle += 360;
		}
		if(angle > 360)
		{
			angle -= 360;
		}
		return Mathf.Clamp(angle, min, max);
	}


}

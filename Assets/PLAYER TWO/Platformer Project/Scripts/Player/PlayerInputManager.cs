using UnityEngine;
using UnityEngine.InputSystem;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Input Manager")]
	public class PlayerInputManager : MonoBehaviour
	{
		public InputActionAsset actions;

		/// <summary>
		/// �����ɫ���ƶ�����
		/// </summary>
		protected InputAction m_movement;

		/// <summary>
		/// �����ɫ�ı�������
		/// </summary>
		protected InputAction m_run;

		/// <summary>
		/// �����ɫ����Ծ����
		/// </summary>
		protected InputAction m_jump;

		/// <summary>
		/// �����ɫ�ĸ����Ǳˮ����
		/// </summary>
		protected InputAction m_dive;

		/// <summary>
		/// �����ɫ����ת����ת��������
		/// </summary>
		protected InputAction m_spin;

		/// <summary>
		/// �����ɫ��ʰȡ�ͷ�����Ʒ������
		/// </summary>
		protected InputAction m_pickAndDrop;

		/// <summary>
		/// �����ɫ�Ķ�������
		/// </summary>
		protected InputAction m_crouch;

		/// <summary>
		/// �����ɫ�ڿ��еĸ�������
		/// </summary>
		protected InputAction m_airDive;

		/// <summary>
		/// �����ɫ�Ĳ�̤��������
		/// </summary>
		protected InputAction m_stomp;

		/// <summary>
		/// �����ɫ�ɿ����ұ�Ե������
		/// </summary>
		protected InputAction m_releaseLedge;

		/// <summary>
		/// ������ͣ��Ϸ������
		/// </summary>
		protected InputAction m_pause;

		/// <summary>
		/// �����ɫ���ӽǿ�������
		/// </summary>
		protected InputAction m_look;

		/// <summary>
		/// �����ɫ���������
		/// </summary>
		protected InputAction m_glide;

		/// <summary>
		/// �����ɫ��̵�����
		/// </summary>
		protected InputAction m_dash;

		/// <summary>
		/// �����ɫ�ڻ���ʱɲ��������
		/// </summary>
		protected InputAction m_grindBrake;

		protected Camera m_camera;

		protected float m_movementDirectionUnlockTime;
		protected float? m_lastJumpTime;

		protected const string k_mouseDeviceName = "Mouse";

		protected const float k_jumpBuffer = 0.15f;

		protected virtual void CacheActions()
		{
			m_movement = actions["Movement"];
			m_run = actions["Run"];
			m_jump = actions["Jump"];
			m_dive = actions["Dive"];
			m_spin = actions["Spin"];
			m_pickAndDrop = actions["PickAndDrop"];
			m_crouch = actions["Crouch"];
			m_airDive = actions["AirDive"];
			m_stomp = actions["Stomp"];
			m_releaseLedge = actions["ReleaseLedge"];
			m_pause = actions["Pause"];
			m_look = actions["Look"];
			m_glide = actions["Glide"];
			m_dash = actions["Dash"];
			m_grindBrake = actions["Grind Brake"];
		}

		public virtual Vector3 GetMovementDirection()
		{
			if (Time.time < m_movementDirectionUnlockTime) return Vector3.zero;

			var value = m_movement.ReadValue<Vector2>();
			return GetAxisWithCrossDeadZone(value);
		}

		public virtual Vector3 GetLookDirection()
		{
			var value = m_look.ReadValue<Vector2>();

			if (IsLookingWithMouse())
			{
				return new Vector3(value.x, 0, value.y);
			}

			return GetAxisWithCrossDeadZone(value);
		}

		public virtual Vector3 GetMovementCameraDirection()
		{
			var direction = GetMovementDirection();

			if (direction.sqrMagnitude > 0)
			{
				var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);
				direction = rotation * direction;
				direction = direction.normalized;
			}

			return direction;
		}

		/// <summary>
		/// Remaps a given axis considering the Input System's default deadzone.
		/// This method uses a cross shape instead of a circle one to evaluate the deadzone range.
		/// </summary>
		/// <param name="axis">The axis you want to remap.</param>
		public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
		{
			var deadzone = InputSystem.settings.defaultDeadzoneMin;
			axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadzone(axis.x, deadzone) : 0;
			axis.y = Mathf.Abs(axis.y) > deadzone ? RemapToDeadzone(axis.y, deadzone) : 0;
			return new Vector3(axis.x, 0, axis.y);
		}

		public virtual bool IsLookingWithMouse()
		{
			if (m_look.activeControl == null)
			{
				return false;
			}

			return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
		}

		public virtual bool GetRun() => m_run.IsPressed();
		public virtual bool GetRunUp() => m_run.WasReleasedThisFrame();

		public virtual bool GetJumpDown()
		{
			if (m_lastJumpTime != null &&
				Time.time - m_lastJumpTime < k_jumpBuffer)
			{
				m_lastJumpTime = null;
				return true;
			}

			return false;
		}

		/// <summary>
		/// �����Ծ���Ƿ�����һ֡���ͷ�
		/// </summary>
		public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();

		/// <summary>
		/// ��鸩����Ƿ񱻰���
		/// </summary>
		public virtual bool GetDive() => m_dive.IsPressed();

		/// <summary>
		/// �����ת���Ƿ�����һ֡������
		/// </summary>
		public virtual bool GetSpinDown() => m_spin.WasPressedThisFrame();

		/// <summary>
		/// ���ʰȡ�ͷ�����Ʒ���Ƿ�����һ֡������
		/// </summary>
		public virtual bool GetPickAndDropDown() => m_pickAndDrop.WasPressedThisFrame();

		/// <summary>
		/// �����¼��Ƿ񱻰���
		/// </summary>
		public virtual bool GetCrouchAndCraw() => m_crouch.IsPressed();

		/// <summary>
		/// �����и�����Ƿ�����һ֡������
		/// </summary>
		public virtual bool GetAirDiveDown() => m_airDive.WasPressedThisFrame();

		/// <summary>
		/// ����̤���Ƿ�����һ֡������
		/// </summary>
		public virtual bool GetStompDown() => m_stomp.WasPressedThisFrame();

		/// <summary>
		/// ����ɿ����ұ�Ե���Ƿ�����һ֡������
		/// </summary>
		public virtual bool GetReleaseLedgeDown() => m_releaseLedge.WasPressedThisFrame();

		/// <summary>
		/// ��黬����Ƿ񱻰���
		/// </summary>
		public virtual bool GetGlide() => m_glide.IsPressed();

		/// <summary>
		/// ����̼��Ƿ�����һ֡������
		/// </summary>
		public virtual bool GetDashDown() => m_dash.WasPressedThisFrame();

		/// <summary>
		/// ��黬��ɲ�����Ƿ񱻰���
		/// </summary>
		public virtual bool GetGrindBrake() => m_grindBrake.IsPressed();

		/// <summary>
		/// �����ͣ���Ƿ�����һ֡������
		/// </summary>
		public virtual bool GetPauseDown() => m_pause.WasPressedThisFrame();


		/// <summary>
		/// ��һ��ֵ����ӳ�䵽 0-1 ��Χ�ڣ�ͬʱ���Ǹ�����������
		/// </summary>
		/// <param name="value">Ҫ����ӳ���ֵ��</param>
		/// <param name="deadzone">��С����ֵ��</param>
		protected float RemapToDeadzone(float value, float deadzone) => (value - deadzone) / (1 - deadzone);

		/// <summary>
		/// ��ʱ�����ƶ��������롣
		/// </summary>
		/// <param name="duration">����״̬�ĳ���ʱ�䣨�룩��</param>
		public virtual void LockMovementDirection(float duration = 0.25f)
		{
			m_movementDirectionUnlockTime = Time.time + duration;
		}


		protected virtual void Awake() => CacheActions();

		protected virtual void Start()
		{
			m_camera = Camera.main;
			actions.Enable();
		}

		protected virtual void Update()
		{
			if (m_jump.WasPressedThisFrame())
			{
				m_lastJumpTime = Time.time;
			}
		}

		protected virtual void OnEnable() => actions?.Enable();
		protected virtual void OnDisable() => actions?.Disable();
	}
}

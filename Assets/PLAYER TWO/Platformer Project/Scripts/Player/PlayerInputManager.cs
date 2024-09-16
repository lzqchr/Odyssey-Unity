using UnityEngine;
using UnityEngine.InputSystem;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Input Manager")]
	public class PlayerInputManager : MonoBehaviour
	{
		public InputActionAsset actions;

		/// <summary>
		/// 处理角色的移动输入
		/// </summary>
		protected InputAction m_movement;

		/// <summary>
		/// 处理角色的奔跑输入
		/// </summary>
		protected InputAction m_run;

		/// <summary>
		/// 处理角色的跳跃输入
		/// </summary>
		protected InputAction m_jump;

		/// <summary>
		/// 处理角色的俯冲或潜水输入
		/// </summary>
		protected InputAction m_dive;

		/// <summary>
		/// 处理角色的旋转或旋转攻击输入
		/// </summary>
		protected InputAction m_spin;

		/// <summary>
		/// 处理角色的拾取和放下物品的输入
		/// </summary>
		protected InputAction m_pickAndDrop;

		/// <summary>
		/// 处理角色的蹲下输入
		/// </summary>
		protected InputAction m_crouch;

		/// <summary>
		/// 处理角色在空中的俯冲输入
		/// </summary>
		protected InputAction m_airDive;

		/// <summary>
		/// 处理角色的踩踏攻击输入
		/// </summary>
		protected InputAction m_stomp;

		/// <summary>
		/// 处理角色松开悬挂边缘的输入
		/// </summary>
		protected InputAction m_releaseLedge;

		/// <summary>
		/// 处理暂停游戏的输入
		/// </summary>
		protected InputAction m_pause;

		/// <summary>
		/// 处理角色的视角控制输入
		/// </summary>
		protected InputAction m_look;

		/// <summary>
		/// 处理角色滑翔的输入
		/// </summary>
		protected InputAction m_glide;

		/// <summary>
		/// 处理角色冲刺的输入
		/// </summary>
		protected InputAction m_dash;

		/// <summary>
		/// 处理角色在滑行时刹车的输入
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
		/// 检查跳跃键是否在这一帧被释放
		/// </summary>
		public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();

		/// <summary>
		/// 检查俯冲键是否被按下
		/// </summary>
		public virtual bool GetDive() => m_dive.IsPressed();

		/// <summary>
		/// 检查旋转键是否在这一帧被按下
		/// </summary>
		public virtual bool GetSpinDown() => m_spin.WasPressedThisFrame();

		/// <summary>
		/// 检查拾取和放下物品键是否在这一帧被按下
		/// </summary>
		public virtual bool GetPickAndDropDown() => m_pickAndDrop.WasPressedThisFrame();

		/// <summary>
		/// 检查蹲下键是否被按下
		/// </summary>
		public virtual bool GetCrouchAndCraw() => m_crouch.IsPressed();

		/// <summary>
		/// 检查空中俯冲键是否在这一帧被按下
		/// </summary>
		public virtual bool GetAirDiveDown() => m_airDive.WasPressedThisFrame();

		/// <summary>
		/// 检查踩踏键是否在这一帧被按下
		/// </summary>
		public virtual bool GetStompDown() => m_stomp.WasPressedThisFrame();

		/// <summary>
		/// 检查松开悬挂边缘键是否在这一帧被按下
		/// </summary>
		public virtual bool GetReleaseLedgeDown() => m_releaseLedge.WasPressedThisFrame();

		/// <summary>
		/// 检查滑翔键是否被按下
		/// </summary>
		public virtual bool GetGlide() => m_glide.IsPressed();

		/// <summary>
		/// 检查冲刺键是否在这一帧被按下
		/// </summary>
		public virtual bool GetDashDown() => m_dash.WasPressedThisFrame();

		/// <summary>
		/// 检查滑行刹车键是否被按下
		/// </summary>
		public virtual bool GetGrindBrake() => m_grindBrake.IsPressed();

		/// <summary>
		/// 检查暂停键是否在这一帧被按下
		/// </summary>
		public virtual bool GetPauseDown() => m_pause.WasPressedThisFrame();


		/// <summary>
		/// 将一个值重新映射到 0-1 范围内，同时考虑给定的死区。
		/// </summary>
		/// <param name="value">要重新映射的值。</param>
		/// <param name="deadzone">最小死区值。</param>
		protected float RemapToDeadzone(float value, float deadzone) => (value - deadzone) / (1 - deadzone);

		/// <summary>
		/// 暂时锁定移动方向输入。
		/// </summary>
		/// <param name="duration">锁定状态的持续时间（秒）。</param>
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

using UnityEngine;
using UnityEngine.Splines;

namespace PLAYERTWO.PlatformerProject
{

	/// <summary>
	/// 一个抽象类 用来提供基础属性和一些状态判断
	/// </summary>
	public abstract class Entity : MonoBehaviour
	{
		public EntityEvents entityEvents;

		/// <summary>
		/// 在指定胶囊体区域内的所有碰撞体
		/// </summary>
		protected Collider[] m_contactBuffer = new Collider[10];
		/// <summary>
		/// 在指定正方体区域内的所有碰撞体
		/// </summary>
		protected Collider[] m_penetrationBuffer = new Collider[32];

		protected readonly float m_groundOffset = 0.1f;
		protected readonly float m_penetrationOffset = -0.1f;
		protected readonly float m_slopingGroundAngle = 20f;

		/// <summary>
		/// Returns the Character Controller of this Entity.
		/// </summary>
		public CharacterController controller { get; protected set; }

		/// <summary>
		/// The current velocity of this Entity.
		/// </summary>
		public Vector3 velocity { get; set; }

		/// <summary>
		/// The current XZ velocity of this Entity.
		/// </summary>
		public Vector3 lateralVelocity
		{
			get { return new Vector3(velocity.x, 0, velocity.z); }
			set { velocity = new Vector3(value.x, velocity.y, value.z); }
		}

		/// <summary>
		/// The current Y velocity of this Entity.
		/// </summary>
		public Vector3 verticalVelocity
		{
			get { return new Vector3(0, velocity.y, 0); }
			set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
		}

		/// <summary>
		/// Returns the Entity position in the previous frame.
		/// </summary>
		public Vector3 lastPosition { get; set; }

		/// <summary>
		/// Return the Entity position.
		/// </summary>
		public Vector3 position => transform.position + center;

		/// <summary>
		/// Returns the Entity position ignoring any collision resizing.
		/// </summary>
		public Vector3 unsizedPosition => position - transform.up * height * 0.5f + transform.up * originalHeight * 0.5f;

		/// <summary>
		/// Returns the bottom position of this Entity considering the stepOffset.
		/// </summary>
		public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);

		/// <summary>
		/// The distance between the last and current Entity position.
		/// </summary>
		public float positionDelta { get; protected set; }

		/// <summary>
		/// Returns the last frame this Entity was grounded.
		/// </summary>
		public float lastGroundTime { get; protected set; }

		/// <summary>
		/// Returns true if the Entity is on the ground.
		/// </summary>
		public bool isGrounded { get; protected set; } = true;

		/// <summary>
		/// Returns true if the Entity is on Rail.
		/// </summary>
		public bool onRails { get; set; }

		public float accelerationMultiplier { get; set; } = 1f;

		public float gravityMultiplier { get; set; } = 1f;

		public float topSpeedMultiplier { get; set; } = 1f;

		public float turningDragMultiplier { get; set; } = 1f;

		public float decelerationMultiplier { get; set; } = 1f;

		/// <summary>
		/// Returns the hit info of the ground.
		/// </summary>
		public RaycastHit groundHit;

		/// <summary>
		/// Returns the current rails this Entity is attached to.
		/// </summary>
		public SplineContainer rails { get; protected set; }

		/// <summary>
		/// Returns the angle of the current ground.
		/// </summary>
		public float groundAngle { get; protected set; }

		/// <summary>
		/// Returns the ground normal of the current ground.
		/// </summary>
		public Vector3 groundNormal { get; protected set; }

		/// <summary>
		/// Returns the local slope direction of the current ground.
		/// </summary>
		public Vector3 localSlopeDirection { get; protected set; }

		/// <summary>
		/// Returns the original height of this Entity.
		/// </summary>
		public float originalHeight { get; protected set; }

		/// <summary>
		/// Returns the collider height of this Entity.
		/// </summary>
		public float height => controller.height;

		/// <summary>
		/// Returns the collider radius of this Entity.
		/// </summary>
		public float radius => controller.radius;

		/// <summary>
		/// The center of the Character Controller collider.
		/// </summary>
		public Vector3 center => controller.center;

		protected CapsuleCollider m_collider;
		protected BoxCollider m_penetratorCollider;

		protected Rigidbody m_rigidbody;

		/// <summary>
		/// Returns true if a given point is bellow the Entity's step position.
		/// </summary>
		/// <param name="point">The point you want to evaluate.</param>
		public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;

		/// <summary>
		/// Returns true if the Player is on a sloping ground.
		/// </summary>
		/// <returns></returns>
		public virtual bool OnSlopingGround()
		{
			if (isGrounded && groundAngle > m_slopingGroundAngle)
			{
				if (Physics.Raycast(transform.position, -transform.up, out var hit, height * 2f,
					Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
					return Vector3.Angle(hit.normal, Vector3.up) > m_slopingGroundAngle;
				else
					return true;
			}

			return false;
		}

		/// <summary>
		/// Resizes the Character Controller to a given height 还.
		/// </summary>
		/// <param name="height">The desired height.</param>
		public virtual void ResizeCollider(float height)
		{
			var delta = height - this.height;
			controller.height = height;
			controller.center += Vector3.up * delta * 0.5f;
		}
		/// <summary>
		/// 在场景中射线是否碰撞.
		/// </summary>
		public virtual bool CapsuleCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
		{
			return CapsuleCast(direction, distance, out _, layer, queryTriggerInteraction);
		}

		public virtual bool CapsuleCast(Vector3 direction, float distance,
			out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
		{
			var origin = position - direction * radius + center;
			var offset = transform.up * (height * 0.5f - radius);
			var top = origin + offset;
			var bottom = origin - offset;
			return Physics.CapsuleCast(top, bottom, radius, direction,
				out hit, distance + radius, layer, queryTriggerInteraction);
		}
		/// <summary>
		/// 在场景中Sphere是否碰撞.
		/// </summary>
		public virtual bool SphereCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
		{
			return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
		}
		/// <summary>
		/// 沿射线投射球体并返回有关命中对象的详细信息
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="distance"></param>
		/// <param name="hit"></param>
		/// <param name="layer"></param>
		/// <param name="queryTriggerInteraction"></param>
		/// <returns></returns>
		public virtual bool SphereCast(Vector3 direction, float distance,
			out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
		{
			var castDistance = Mathf.Abs(distance - radius);
			return Physics.SphereCast(position, radius, direction,
				out hit, castDistance, layer, queryTriggerInteraction);
		}
		/// <summary>
		/// 检测设定半径内的碰撞体 储存
		/// </summary>
		public virtual int OverlapEntity(Collider[] result, float skinOffset = 0)
		{
			var contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
			var overlapsRadius = radius + contactOffset;
			var offset = (height + contactOffset) * 0.5f - overlapsRadius;
			var top = position + Vector3.up * offset;
			var bottom = position + Vector3.down * offset;
			return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapsRadius, result);
		}

		public virtual void ApplyDamage(int damage, Vector3 origin) { }
	}

	public abstract class Entity<T> : Entity where T : Entity<T>
	{
		protected IEntityContact[] m_contacts;

		/// <summary>
		/// Returns the State Manager of this Entity.
		/// </summary>
		public EntityStateManager<T> states { get; protected set; }

		/// <summary>
		/// 设置 State Manager of this Entity.
		/// </summary>
		protected virtual void InitializeController()
		{
			controller = GetComponent<CharacterController>();

			if (!controller)
			{
				controller = gameObject.AddComponent<CharacterController>();
			}

			controller.skinWidth = 0.005f;
			controller.minMoveDistance = 0;
			originalHeight = controller.height;
		}
		/// <summary>
		/// 设置box碰撞体
		/// </summary>
		protected virtual void InitializePenetratorCollider()
		{
			var xzSize = radius * 2f - controller.skinWidth;
			m_penetratorCollider = gameObject.AddComponent<BoxCollider>();
			m_penetratorCollider.size = new Vector3(xzSize, height - controller.stepOffset, xzSize);
			m_penetratorCollider.center = center + Vector3.up * controller.stepOffset * 0.5f;
			m_penetratorCollider.isTrigger = true;
		}

		protected virtual void InitializeCollider()
		{
			m_collider = gameObject.AddComponent<CapsuleCollider>();
			m_collider.height = controller.height;
			m_collider.radius = controller.radius;
			m_collider.center = controller.center;
			m_collider.isTrigger = true;
			m_collider.enabled = false;
		}

		protected virtual void InitializeRigidbody()
		{
			m_rigidbody = gameObject.AddComponent<Rigidbody>();
			m_rigidbody.isKinematic = true;
		}
		/// <summary>
		/// 设置控制管理
		/// </summary>
		protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();

		protected virtual void HandleStates() => states.Step();
		/// <summary>
		/// 移动
		/// </summary>
		protected virtual void HandleController()
		{
			if (controller.enabled)
			{
				controller.Move(velocity * Time.deltaTime);
				return;
			}

			transform.position += velocity * Time.deltaTime;
		}
		/// <summary>
		/// 处理角色在轨道上的行为
		/// </summary>
		protected virtual void HandleSpline()
		{
			var distance = (height * 0.5f) + height * 0.5f;
			//看人物下方有无轨道
			if (SphereCast(-transform.up, distance, out var hit) &&
				hit.collider.CompareTag(GameTags.InteractiveRail))
			{
				//角色下降
				if (!onRails && verticalVelocity.y <= 0)
				{
					EnterRail(hit.collider.GetComponent<SplineContainer>());
				}
			}
			else
			{
				ExitRail();
			}
		}
		/// <summary>
		/// 降落 和 站立处理
		/// </summary>
		protected virtual void HandleGround()
		{
			if (onRails) return;

			var distance = (height * 0.5f) + m_groundOffset;

			if (SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
			{
				if (!isGrounded)
				{
					if (EvaluateLanding(hit))
					{
						EnterGround(hit);
					}
					else
					{
						HandleHighLedge(hit);
					}
				}
				else if (IsPointUnderStep(hit.point))
				{
					UpdateGround(hit);

					//斜坡处理
					if (Vector3.Angle(hit.normal, Vector3.up) >= controller.slopeLimit)
					{
						HandleSlopeLimit(hit);
					}
				}
				else
				{
					HandleHighLedge(hit);
				}
			}
			else
			{
				ExitGround();
			}
		}
		/// <summary>
		/// 用于处理角色与其他碰撞体的接触
		/// </summary>
		protected virtual void HandleContacts()
		{
			var overlaps = OverlapEntity(m_contactBuffer);

			for (int i = 0; i < overlaps; i++)
			{
				if (!m_contactBuffer[i].isTrigger && m_contactBuffer[i].transform != transform)
				{
					OnContact(m_contactBuffer[i]);

					var listeners = m_contactBuffer[i].GetComponents<IEntityContact>();

					foreach (var contact in listeners)
					{
						contact.OnEntityContact((T)this);
					}
					//如果碰撞体在角色上方，将垂直速度设置为零或保持当前速度的最小值。
					if (m_contactBuffer[i].bounds.min.y > controller.bounds.max.y)
					{
						verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
					}
				}
			}
		}

		protected virtual void HandlePosition()
		{
			positionDelta = (position - lastPosition).magnitude;
			lastPosition = position;
		}

		/// <summary>
		/// 测和处理角色与其他碰撞体的穿透情况，确保角色不会卡在其他物体中
		/// </summary>
		protected virtual void HandlePenetration()
		{
			var xzSize = m_penetratorCollider.size.x * 0.5f;
			var ySize = (height - controller.stepOffset * 0.5f) * 0.5f;
			var origin = position + Vector3.up * controller.stepOffset * 0.5f;
			var halfExtents = new Vector3(xzSize, ySize, xzSize);
			//所有碰撞体存入m_penetrationBuffer,overlaps 数量
			var overlaps = Physics.OverlapBoxNonAlloc(origin, halfExtents, m_penetrationBuffer,
				Quaternion.identity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

			for (int i = 0; i < overlaps; i++)
			{
				//检查碰撞体不是触发器，不是完全重合，并且横向速度为零或碰撞体是平台。
				if (!m_penetrationBuffer[i].isTrigger && m_penetrationBuffer[i].transform != transform &&
					(lateralVelocity.sqrMagnitude == 0 || m_penetrationBuffer[i].CompareTag(GameTags.Platform)))
				{
					//计算方向和穿透距离
					if (Physics.ComputePenetration(m_penetratorCollider, position, Quaternion.identity,
						m_penetrationBuffer[i], m_penetrationBuffer[i].transform.position,
						m_penetrationBuffer[i].transform.rotation, out var direction, out float distance))
					{
						var pushDirection = new Vector3(direction.x, 0, direction.z).normalized;
						transform.position += pushDirection * distance;
					}
				}
			}
		}

		protected virtual void EnterGround(RaycastHit hit)
		{
			if (!isGrounded)
			{
				groundHit = hit;
				isGrounded = true;
				entityEvents.OnGroundEnter?.Invoke();
			}
		}

		protected virtual void ExitGround()
		{
			if (isGrounded)
			{
				isGrounded = false;
				transform.parent = null;
				lastGroundTime = Time.time;
				verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
				entityEvents.OnGroundExit?.Invoke();
			}
		}

		protected virtual void EnterRail(SplineContainer rails)
		{
			if (!onRails)
			{
				onRails = true;
				this.rails = rails;
				entityEvents.OnRailsEnter.Invoke();
			}
		}

		public virtual void ExitRail()
		{
			if (onRails)
			{
				onRails = false;
				entityEvents.OnRailsExit.Invoke();
			}
		}

		protected virtual void UpdateGround(RaycastHit hit)
		{
			if (isGrounded)
			{
				groundHit = hit;
				groundNormal = groundHit.normal;
				groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
				localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
				transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
			}
		}
		/// <summary>
		/// 判断能否站稳
		/// </summary>
		/// <param name="hit"></param>
		/// <returns></returns>
		protected virtual bool EvaluateLanding(RaycastHit hit)
		{
			return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
		}

		protected virtual void HandleSlopeLimit(RaycastHit hit) { }

		protected virtual void HandleHighLedge(RaycastHit hit) { }

		protected virtual void OnUpdate() { }

		protected virtual void OnContact(Collider other)
		{
			if (other)
			{
				states.OnContact(other);
			}
		}

		/// <summary>
		/// 方法根据输入方向、转向阻力、加速度和最高速度来调整角色的横向速度
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		/// <param name="turningDrag">How fast it will turn towards the new direction.</param>
		/// <param name="acceleration">How fast it will move over time.</param>
		/// <param name="topSpeed">The max movement magnitude.</param>
		public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
		{
			if (direction.sqrMagnitude > 0)
			{
				var speed = Vector3.Dot(direction, lateralVelocity);
				var velocity = direction * speed;
				var turningVelocity = lateralVelocity - velocity;
				var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
				var targetTopSpeed = topSpeed * topSpeedMultiplier;

				if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
				{
					speed += acceleration * accelerationMultiplier * Time.deltaTime;
					speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
				}

				velocity = direction * speed;
				turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
				lateralVelocity = velocity + turningVelocity;
			}
		}

		/// <summary>
		/// Smoothly moves Lateral Velocity to zero.
		/// </summary>
		/// <param name="deceleration">How fast it will decelerate over time.</param>
		public virtual void Decelerate(float deceleration)
		{
			var delta = deceleration * decelerationMultiplier * Time.deltaTime;
			lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
		}

		/// <summary>
		/// Smoothly moves vertical velocity to zero.
		/// </summary>
		/// <param name="gravity">How fast it will move over time.</param>
		public virtual void Gravity(float gravity)
		{
			if (!isGrounded)
			{
				verticalVelocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
			}
		}

		/// <summary>
		/// 根据倾斜角度增加横向速度。
		/// </summary>
		/// <param name="upwardForce">The force applied when moving upwards.</param>
		/// <param name="downwardForce">The force applied when moving downwards.</param>
		public virtual void SlopeFactor(float upwardForce, float downwardForce)
		{
			if (!isGrounded || !OnSlopingGround()) return;

			var factor = Vector3.Dot(Vector3.up, groundNormal);
			var downwards = Vector3.Dot(localSlopeDirection, lateralVelocity) > 0;
			var multiplier = downwards ? downwardForce : upwardForce;
			var delta = factor * multiplier * Time.deltaTime;
			lateralVelocity += localSlopeDirection * delta;
		}

		/// <summary>
		/// Applies a force towards the ground.
		/// </summary>
		/// <param name="force">The force you want to apply.</param>
		public virtual void SnapToGround(float force)
		{
			if (isGrounded && (verticalVelocity.y <= 0))
			{
				verticalVelocity = Vector3.down * force;
			}
		}

		/// <summary>
		/// Rotate the Player towards to a given direction.
		/// </summary>
		/// <param name="direction">The direction you want to face.</param>
		public virtual void FaceDirection(Vector3 direction)
		{
			if (direction.sqrMagnitude > 0)
			{
				var rotation = Quaternion.LookRotation(direction, Vector3.up);
				transform.rotation = rotation;
			}
		}

		/// <summary>
		/// Rotate the Player towards to a given direction.
		/// </summary>
		/// <param name="direction">The direction you want to face.</param>
		/// <param name="degreesPerSecond">How fast it should rotate over time.</param>
		public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
		{
			if (direction != Vector3.zero)
			{
				var rotation = transform.rotation;
				var rotationDelta = degreesPerSecond * Time.deltaTime;
				var target = Quaternion.LookRotation(direction, Vector3.up);
				transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
			}
		}

		/// <summary>
		/// Returns true if this Entity collider fits into a given position.
		/// </summary>
		/// <param name="position">The position you want to test if the Entity collider fits.</param>
		public virtual bool FitsIntoPosition(Vector3 position)
		{
			var bounds = controller.bounds;
			var radius = controller.radius - controller.skinWidth;
			var offset = height * 0.5f - radius;
			var top = position + Vector3.up * offset;
			var bottom = position - Vector3.up * offset;

			return !Physics.CheckCapsule(top, bottom, radius,
				Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
		}

		/// <summary>
		/// Enables or disables the custom collision. Disabling the Character Controller.
		/// </summary>
		/// <param name="value">If true, enables the custom collision.</param>
		public virtual void UseCustomCollision(bool value)
		{
			controller.enabled = !value;

			if (value)
			{
				InitializeCollider();
				InitializeRigidbody();
			}
			else
			{
				Destroy(m_collider);
				Destroy(m_rigidbody);
			}
		}

		protected virtual void Awake()
		{
			InitializeController();
			InitializePenetratorCollider();
			InitializeStateManager();
		}

		protected virtual void Update()
		{
			if (controller.enabled || m_collider != null)
			{
				HandleStates();
				HandleController();
				HandleSpline();
				HandleGround();
				HandleContacts();
				OnUpdate();
			}
		}

		protected virtual void LateUpdate()
		{
			if (controller.enabled)
			{
				HandlePosition();
				HandlePenetration();
			}
		}
	}
}

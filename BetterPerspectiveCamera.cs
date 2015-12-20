﻿using System;
using UnityEngine;
using System.Collections;
using Parkitect.UI;
using Parkitect;

namespace BetterPerspective
{
    public class BetterPerspectiveCamera : CameraController
    {

        public Vector3 LookAt;
        public float Distance;
        public float Rotation;
        public float Tilt;

        public bool Smoothing;
        public float MoveDampening;
        public float ZoomDampening;
        public float RotationDampening;
        public float TiltDampening;

        public Vector3 MinBounds;
        public Vector3 MaxBounds;

        public float MinDistance;
        public float MaxDistance;

        public float MinTilt;
        public float MaxTilt;

        public Func<float, float, float> GetTerrainHeight;

        public bool TerrainHeightViaPhysics;
        public LayerMask TerrainPhysicsLayerMask;

        public float LookAtHeightOffset;

        public bool TargetVisbilityViaPhysics;
        public float CameraRadius;
        public LayerMask TargetVisibilityIgnoreLayerMask;

        public bool FollowBehind;
        public float FollowRotationOffset;

        public Action<Transform> OnBeginFollow;
        public Action<Transform> OnEndFollow;

        public bool ShowDebugCameraTarget;



        private Vector3 _initialLookAt;
        private float _initialDistance;
        private float _initialRotation;
        private float _initialTilt;

        private float _currDistance;
        private float _currRotation;
        private float _currTilt;

        private Vector3 _moveVector;

        private GameObject _target;
        private MeshRenderer _targetRenderer;

        private Transform _followTarget;

        private bool _lastDebugCamera;





        public void Reset()
        {
            
            Smoothing = true;
            _lastDebugCamera = true;
            LookAtHeightOffset = 1f;
            TerrainHeightViaPhysics = false;
            TerrainPhysicsLayerMask = ~0;
            GetTerrainHeight = null;

            TargetVisbilityViaPhysics = false;
            CameraRadius = 1f;
            TargetVisibilityIgnoreLayerMask = 0;

            LookAt = new Vector3(10, 0, 22.5f);
            MoveDampening = 5f;
            MinBounds = new Vector3(0,3, 0);
            MaxBounds = new Vector3(100, 100, 100);

            Distance = 10f;
            MinDistance = 2f;
            MaxDistance = 32f;
            ZoomDampening = 5f;

            Rotation = -90f;
            RotationDampening = 5f;

            Tilt = 45f;
            MinTilt = 10f;
            MaxTilt = 85f;
            TiltDampening = 5f;

            FollowBehind = false;
            FollowRotationOffset = 0;
            
        }

        protected void Start()
        {

            StartCoroutine(Wait(2F));

            Reset();
            if (GetComponent<Rigidbody>())
            {

                GetComponent<Rigidbody>().freezeRotation = true;
            }


            _initialLookAt = LookAt;
            _initialDistance = Distance;
            _initialRotation = Rotation;
            _initialTilt = Tilt;


            _currDistance = Distance;
            _currRotation = Rotation;
            _currTilt = Tilt;


            CreateTarget();
        }

        protected void Update()
        {
            if (Time.timeScale > .2f)
            {
                MoveDampening = 7f / Time.timeScale;
                ZoomDampening = 7f / Time.timeScale;
                RotationDampening = 7f / Time.timeScale;
                TiltDampening = 7f / Time.timeScale;
            }
            if (lockedOnto != null)
            {
                Follow(lockedOnto, false);
            }



            if (_lastDebugCamera != ShowDebugCameraTarget)
            {
                if (_targetRenderer != null)
                {
                    _targetRenderer.enabled = ShowDebugCameraTarget;
                    _lastDebugCamera = ShowDebugCameraTarget;
                }
            }
        }

        protected void LateUpdate()
        {

            if (IsFollowing)
            {
                LookAt = _followTarget.position;
            }
            else
            {
                _moveVector.y = 0;
                LookAt += Quaternion.Euler(0, Rotation, 0) * _moveVector;
                LookAt.y = GetHeightAt(LookAt.x, LookAt.z);
            }
            LookAt.y += LookAtHeightOffset;


            Tilt = Mathf.Clamp(Tilt, MinTilt, MaxTilt);
            Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
            LookAt = new Vector3(Mathf.Clamp(LookAt.x, MinBounds.x, MaxBounds.x), Mathf.Clamp(LookAt.y, MinBounds.y, MaxBounds.y), Mathf.Clamp(LookAt.z, MinBounds.z, MaxBounds.z));

            if (Smoothing)
            {
                _currRotation = Mathf.LerpAngle(_currRotation, Rotation, Time.deltaTime * RotationDampening);
                _currDistance = Mathf.Lerp(_currDistance, Distance, Time.deltaTime * ZoomDampening);
                _currTilt = Mathf.LerpAngle(_currTilt, Tilt, Time.deltaTime * TiltDampening);
                _target.transform.position = Vector3.Lerp(_target.transform.position, LookAt, Time.deltaTime * MoveDampening);
            }
            else
            {
                _currRotation = Rotation;
                _currDistance = Distance;
                _currTilt = Tilt;
                _target.transform.position = LookAt;
            }

            _moveVector = Vector3.zero;

            if (IsFollowing && FollowBehind)
            {
                ForceFollowBehind();
            }


            if (IsFollowing && TargetVisbilityViaPhysics && DistanceToTargetIsLessThan(1f))
            {
                EnsureTargetIsVisible();
            }


            UpdateCamera();
        }




        public Transform CameraTarget
        {
            get { return _target.transform; }
        }


        public bool IsFollowing
        {
            get { return FollowTarget != null; }
        }


        public Transform FollowTarget
        {
            get { return _followTarget; }
        }


        public void ResetToInitialValues(bool includePosition, bool snap)
        {
            if (includePosition)
                LookAt = _initialLookAt;

            Distance = _initialDistance;
            Rotation = _initialRotation;
            Tilt = _initialTilt;

            if (snap)
            {
                _currDistance = Distance;
                _currRotation = Rotation;
                _currTilt = Tilt;
                _target.transform.position = LookAt;
            }
        }


        public void JumpTo(Vector3 toPosition, bool snap)
        {
            EndFollow();

            LookAt = toPosition;

            if (snap)
            {
                _target.transform.position = toPosition;
            }
        }


        public void JumpTo(Transform toTransform, bool snap)
        {
            JumpTo(toTransform.position, snap);
        }


        public void JumpTo(GameObject toGameObject, bool snap)
        {
            JumpTo(toGameObject.transform.position, snap);
        }


        public void Follow(Transform followTarget, bool snap)
        {
            if (_followTarget != null)
            {
                if (OnEndFollow != null)
                {
                    OnEndFollow(_followTarget);
                }
            }

            _followTarget = followTarget;

            if (_followTarget != null)
            {
                if (snap)
                {
                    LookAt = _followTarget.position;
                }

                if (OnBeginFollow != null)
                {
                    OnBeginFollow(_followTarget);
                }
            }
        }


        public void Follow(GameObject followTarget, bool snap)
        {
            
        }


        public void EndFollow()
        {
            Follow((Transform)null, false);
        }


        public void AddToPosition(float dx, float dy, float dz)
        {
            _moveVector += new Vector3(dx, dy, dz);
        }






        private float GetHeightAt(float x, float z)
        {

            if (GetTerrainHeight != null)
            {
                return GetTerrainHeight(x, z);
            }


            if (TerrainHeightViaPhysics)
            {
                var y = MaxBounds.y;
                var maxDist = MaxBounds.y - MinBounds.y + 1f;

                RaycastHit hitInfo;
                if (Physics.Raycast(new Vector3(x, y, z), new Vector3(0, -1, 0), out hitInfo, maxDist, TerrainPhysicsLayerMask))
                {
                    return hitInfo.point.y;
                }
                return 0;
            }


            return 0;
        }


        private void UpdateCamera()
        {
            var rotation = Quaternion.Euler(_currTilt, _currRotation, 0);
            var v = new Vector3(0.0f, 0.0f, -_currDistance);
            var position = rotation * v + _target.transform.position;

            if (GetComponent<Camera>().orthographic)
            {
                GetComponent<Camera>().orthographicSize = _currDistance;
            }



            transform.rotation = rotation;
            transform.position = position;
        }


        private void CreateTarget()
        {
            _target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _target.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            _target.GetComponent<Renderer>().material.color = Color.green;

            var targetCollider = _target.GetComponent<Collider>();
            if (targetCollider != null)
            {
                targetCollider.enabled = false;
            }

            _targetRenderer = _target.GetComponent<MeshRenderer>();
            _targetRenderer.enabled = false;

            _target.name = "CameraTarget";
            _target.transform.position = LookAt;
        }

        private bool DistanceToTargetIsLessThan(float sqrDistance)
        {
            if (!IsFollowing)
                return true;

            var p1 = _target.transform.position;
            var p2 = _followTarget.position;
            p1.y = p2.y = 0;
            var v = p1 - p2;
            var vd = v.sqrMagnitude;

            return vd < sqrDistance;
        }

        private void EnsureTargetIsVisible()
        {
            var direction = (transform.position - _target.transform.position);
            direction.Normalize();

            var distance = Distance;

            RaycastHit hitInfo;


            if (Physics.SphereCast(_target.transform.position, CameraRadius, direction, out hitInfo, distance, ~TargetVisibilityIgnoreLayerMask))
            {
                if (hitInfo.transform != _target)
                {
                    _currDistance = hitInfo.distance - 0.1f;
                }
            }
        }

        private void ForceFollowBehind()
        {
            var v = _followTarget.transform.forward * -1;
            var angle = Vector3.Angle(Vector3.forward, v);
            var sign = (Vector3.Dot(v, Vector3.right) > 0.0f) ? 1.0f : -1.0f;
            _currRotation = Rotation = 180f + (sign * angle) + FollowRotationOffset;
        }
        IEnumerator Wait(float waitTimeDelete)
        {
            yield return new WaitForSeconds(waitTimeDelete);
            foreach(CameraController CC in Camera.main.transform.GetComponents<CameraController>())
            {
                if (CC != this)
                {
                    CC.enabled = false;
                }

            }
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;


            Gizmos.DrawWireSphere(MaxBounds, 3);
            Gizmos.DrawWireSphere(MinBounds, 3);

            Gizmos.DrawLine(MaxBounds, new Vector3(MinBounds.x, MaxBounds.y, MaxBounds.z));
            Gizmos.DrawLine(MaxBounds, new Vector3(MaxBounds.x, MinBounds.y, MaxBounds.z));
            Gizmos.DrawLine(MaxBounds, new Vector3(MaxBounds.x, MaxBounds.y, MinBounds.z));

            Gizmos.DrawLine(new Vector3(MinBounds.x, MaxBounds.y, MaxBounds.z), new Vector3(MinBounds.x, MaxBounds.y, MinBounds.z));
            Gizmos.DrawLine(new Vector3(MaxBounds.x, MaxBounds.y, MinBounds.z), new Vector3(MinBounds.x, MaxBounds.y, MinBounds.z));

            Gizmos.DrawLine(new Vector3(MinBounds.x, MinBounds.y, MaxBounds.z), new Vector3(MinBounds.x, MinBounds.y, MinBounds.z));
            Gizmos.DrawLine(new Vector3(MaxBounds.x, MinBounds.y, MinBounds.z), new Vector3(MinBounds.x, MinBounds.y, MinBounds.z));


            Gizmos.DrawLine(new Vector3(MaxBounds.x, MinBounds.y, MinBounds.z), new Vector3(MaxBounds.x, MinBounds.y, MaxBounds.z));
            Gizmos.DrawLine(new Vector3(MinBounds.x, MinBounds.y, MaxBounds.z), new Vector3(MaxBounds.x, MinBounds.y, MaxBounds.z));

            Gizmos.DrawLine(new Vector3(MaxBounds.x, MinBounds.y, MinBounds.z), new Vector3(MaxBounds.x, MaxBounds.y, MinBounds.z));
            Gizmos.DrawLine(new Vector3(MinBounds.x, MinBounds.y, MaxBounds.z), new Vector3(MinBounds.x, MaxBounds.y, MaxBounds.z));

            Gizmos.DrawLine(MinBounds, new Vector3(MinBounds.x, MaxBounds.y, MinBounds.z));
        }
    }
}

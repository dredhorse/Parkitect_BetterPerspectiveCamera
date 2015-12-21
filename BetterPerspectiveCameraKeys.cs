﻿using UnityEngine;
using System.Collections;


namespace BetterPerspective
{
    public class BetterPerspectiveCameraKeys : MonoBehaviour
    {
        public bool AllowMove;
        public float MoveSpeed;
        public float MoveSpeedVar;
        public bool AllowFastMove;
        public float FastMoveSpeed;
        public KeyCode FastMoveKeyCode1;
        public KeyCode FastMoveKeyCode2;

        public bool AllowRotate;
        public float RotateSpeed;
        public float RotateSpeedVar;

        public bool AllowZoom;
        public float ZoomSpeed;
        public float ZoomSpeedVar;

        public bool AllowTilt;
        public float TiltSpeed;

        public KeyCode ResetKey;
        public bool IncludePositionOnReset;

        public bool MovementBreaksFollow;

        public string HorizontalInputAxis = "Horizontal";
        public string VerticalInputAxis = "Vertical";

        public bool RotateUsesInputAxis = false;
        public string RotateInputAxis = "KbCameraRotate";
        public KeyCode RotateLeftKey = KeyCode.Q;
        public KeyCode RotateRightKey = KeyCode.E;

        public bool ZoomUsesInputAxis = false;
        public string ZoomInputAxis = "KbCameraZoom";
        public KeyCode ZoomOutKey = KeyCode.Z;
        public KeyCode ZoomInKey = KeyCode.X;

        public bool TiltUsesInputAxis = false;
        public string TiltInputAxis = "KbCameraTilt";
        public KeyCode TiltUpKey = KeyCode.R;
        public KeyCode TiltDownKey = KeyCode.F;

        //

        private BetterPerspectiveCamera _rtsCamera;

        //

        public void Reset()
        {
            AllowMove = true;
            MoveSpeedVar = 15f;
            MoveSpeed = MoveSpeedVar;

            AllowFastMove = true;
            FastMoveSpeed = 40f;
            FastMoveKeyCode1 = KeyCode.LeftShift;
            FastMoveKeyCode2 = KeyCode.RightShift;

            AllowRotate = true;
            RotateSpeedVar = 180f;

            AllowZoom = true;
            ZoomSpeedVar = 15f;

            AllowTilt = true;
            TiltSpeed = 90f;

            ResetKey = KeyCode.Return;
            IncludePositionOnReset = true;

            MovementBreaksFollow = true;
        }

        protected void Start()
        {
           
            _rtsCamera = gameObject.GetComponent<BetterPerspectiveCamera>();
        }
        
        protected void Update()
        {
            if (Time.timeScale > .2f)
            {
                FastMoveSpeed = MoveSpeedVar * 2 / Time.timeScale;
                MoveSpeed = MoveSpeedVar / Time.timeScale;
                RotateSpeed = RotateSpeedVar / Time.timeScale;
                ZoomSpeed = ZoomSpeedVar / Time.timeScale;
                TiltSpeed = 90f / Time.timeScale;
            }
            if (_rtsCamera == null)
                return; // no camera, bail!

            if (AllowMove && (!_rtsCamera.IsFollowing || MovementBreaksFollow))
            {
                var hasMovement = false;

                var speed = MoveSpeed;
                if (AllowFastMove && (Input.GetKey(FastMoveKeyCode1) || Input.GetKey(FastMoveKeyCode2)))
                {
                    speed = FastMoveSpeed;
                }

                var h = Input.GetAxisRaw(HorizontalInputAxis);
                if (Mathf.Abs(h) > 0.001f)
                {
                    hasMovement = true;
                    _rtsCamera.AddToPosition(h * speed * Time.deltaTime, 0, 0);
                }

                var v = Input.GetAxisRaw(VerticalInputAxis);
                if (Mathf.Abs(v) > 0.001f)
                {
                    hasMovement = true;
                    _rtsCamera.AddToPosition(0, 0, v * speed * Time.deltaTime);
                }

                if (hasMovement && _rtsCamera.IsFollowing && MovementBreaksFollow)
                    _rtsCamera.EndFollow();
            }

            

            if (AllowRotate)
            {
                if (RotateUsesInputAxis)
                {
                    var rot = Input.GetAxisRaw(RotateInputAxis);
                    if (Mathf.Abs(rot) > 0.001f)
                    {
                        _rtsCamera.Rotation += rot * RotateSpeed * Time.deltaTime;
                    }
                }
                else
                {
                    if (Input.GetKey(RotateLeftKey))
                    {
                        _rtsCamera.Rotation += RotateSpeed * Time.deltaTime;
                    }
                    if (Input.GetKey(RotateRightKey))
                    {
                        _rtsCamera.Rotation -= RotateSpeed * Time.deltaTime;
                    }
                }
            }

            if (AllowZoom)
            {
                if (ZoomUsesInputAxis)
                {
                    var zoom = Input.GetAxisRaw(ZoomInputAxis);
                    if (Mathf.Abs(zoom) > 0.001f)
                    {
                        _rtsCamera.Distance += zoom * ZoomSpeed * Time.deltaTime;
                    }
                }
                else
                {
                    if (Input.GetKey(ZoomOutKey))
                    {
                        _rtsCamera.Distance += ZoomSpeed * Time.deltaTime;
                    }
                    if (Input.GetKey(ZoomInKey))
                    {
                        _rtsCamera.Distance -= ZoomSpeed * Time.deltaTime;
                    }
                }
            }

            if (AllowTilt)
            {
                if (TiltUsesInputAxis)
                {
                    var tilt = Input.GetAxisRaw(TiltInputAxis);
                    if (Mathf.Abs(tilt) > 0.001f)
                    {
                        _rtsCamera.Tilt += tilt * TiltSpeed * Time.deltaTime;
                    }
                }
                else
                {
                    if (Input.GetKey(TiltUpKey))
                    {
                        _rtsCamera.Tilt += TiltSpeed * Time.deltaTime;
                    }
                    if (Input.GetKey(TiltDownKey))
                    {
                        _rtsCamera.Tilt -= TiltSpeed * Time.deltaTime;
                    }
                }
            }

            //

            if (ResetKey != KeyCode.None)
            {
                if (Input.GetKeyDown(ResetKey))
                {
                    _rtsCamera.ResetToInitialValues(IncludePositionOnReset, false);
                }
            }
        }
    }
}

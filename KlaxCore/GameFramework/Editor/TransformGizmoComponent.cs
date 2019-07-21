using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ImGuiNET;
using KlaxCore.Core;
using KlaxCore.Core.View;
using KlaxIO.Input;
using KlaxMath;
using KlaxRenderer;
using KlaxRenderer.Debug;
using KlaxRenderer.Scene;
using SharpDX;
using Plane = SharpDX.Plane;
using Quaternion = SharpDX.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = SharpDX.Vector3;

namespace KlaxCore.GameFramework.Editor
{
    [Flags]
    public enum EGizmoAxis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
    }

    public enum EGizmoMode
    {
        Translation,
        Rotation,
        Scale
    }

    class CGizmoSelectionRegion
    {
        public CGizmoSelectionRegion(OrientedBoundingBox bounds, EGizmoAxis axis, Color drawColor)
        {
            BoundingBox = bounds;
            TargetAxis = axis;

            if (TargetAxis.HasFlag(EGizmoAxis.X))
            {
                NumAxis++;
            }

            if (TargetAxis.HasFlag(EGizmoAxis.Y))
            {
                NumAxis++;
            }

            if (TargetAxis.HasFlag(EGizmoAxis.Z))
            {
                NumAxis++;
            }

            DrawColor = drawColor.ToColor4();
        }

        public OrientedBoundingBox BoundingBox { get; set; }
        public EGizmoAxis TargetAxis { get; private set; }
        public int NumAxis { get; private set; }
        public Color4 DrawColor { get; set; }
    }

    public class CTransformGizmo : CWorldObject
    {
        public static Action<Transform, Vector3, Vector3> OnTranslationChanged;
        public static Action<Transform, Quaternion, Quaternion> OnRotationChanged;
        public static Action<Transform, Vector3, Vector3> OnScaleChanged;

        public override void Init(CWorld world, object userData)
        {
            base.Init(world, userData);
            m_updateScope = World.UpdateScheduler.Connect(Update, EUpdatePriority.Editor);
            Input.RegisterListener(OnButtonEvent);
		}

		public override void Destroy()
		{
			base.Destroy();
			m_updateScope?.Disconnect();
			Input.UnregisterListener(OnButtonEvent);
		}

		private void OnButtonEvent(ReadOnlyCollection<SInputButtonEvent> buttonEvents, string textInput)
        {
            foreach (var buttonEvent in buttonEvents)
            {
                switch (buttonEvent.button)
                {
                    case EInputButton.MouseLeftButton:
                        ProcessLeftMouse(buttonEvent.buttonEvent);
                        break;
                    case EInputButton.D1:
                        if (buttonEvent.buttonEvent == EButtonEvent.Pressed)
                        {
                            Mode = EGizmoMode.Translation;
                        }
                        break;
                    case EInputButton.D2:
                        if (buttonEvent.buttonEvent == EButtonEvent.Pressed)
                        {
                            Mode = EGizmoMode.Rotation;
                        }
                        break;
                    case EInputButton.D3:
                        if (buttonEvent.buttonEvent == EButtonEvent.Pressed)
                        {
                            Mode = EGizmoMode.Scale;
                        }
                        break;
                    case EInputButton.D4:
                        if (buttonEvent.buttonEvent == EButtonEvent.Pressed)
                        {
                            IsLocalAligned = !IsLocalAligned;
                        }
                        break;
                }
            }
        }

        private void ProcessLeftMouse(EButtonEvent buttonEvent)
        {
            if (buttonEvent == EButtonEvent.Pressed && m_controlledTransform != null)
            {
                if (IsHovered)
                {
                    Plane axisPlane;
                    if (m_activeAxisList.Count > 1)
                    {
                        Vector3 planeNormal = Vector3.Cross(m_activeAxisList[0], m_activeAxisList[1]);
                        axisPlane = new Plane(m_gizmoLocation, planeNormal);
                    }
                    else
                    {
                        Vector3 planeNormal;
                        if (Mode == EGizmoMode.Rotation)
                        {
                            // Rotation
                            planeNormal = m_activeAxisList[0];
                        }
                        else
                        {
                            //Translation / Scale
                            Vector3 toCamera = m_frameViewInfo.ViewLocation - m_gizmoLocation;
                            Vector3 planeTangent = Vector3.Cross(m_activeAxisList[0], toCamera);
                            planeNormal = Vector3.Cross(m_activeAxisList[0], planeTangent);
                        }

                        axisPlane = new Plane(m_gizmoLocation, planeNormal);
                    }

                    World.ViewManager.GetViewInfo(out SSceneViewInfo viewInfo);
                    Ray ray = CreateMouseRay(viewInfo);

                    if (ray.Intersects(ref axisPlane, out Vector3 intersectionPoint))
                    {
                        m_startLocation = intersectionPoint;

                        switch (Mode)
                        {
                            case EGizmoMode.Translation:
                                m_clickOffset = intersectionPoint - m_gizmoLocation;
                                m_originalPosition = m_controlledTransform.WorldPosition;
                                break;
                            case EGizmoMode.Rotation:
                                Vector3 toStart = m_startLocation - m_gizmoLocation;
                                m_rotationDragAxis = Vector3.Cross(toStart, m_activeAxisList[0]);
                                m_rotationDragAxis.Normalize();
                                m_originalRotation = m_controlledTransform.WorldRotation;
                                break;
                            case EGizmoMode.Scale:
                                m_originalScale = m_controlledTransform.WorldScale;
                                m_scaleAxis = Vector3.Zero;
                                foreach (Vector3 axis in m_activeAxisList)
                                {
                                    m_scaleAxis += Vector3.Transform(axis, Quaternion.Invert(m_controlledTransform.WorldRotation));
                                }

                                m_scaleAxis.Normalize();
                                m_scaleSizeFactor = m_originalScale.Length();
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        IsClicked = true;
                    }
                }
            }
            else if(m_controlledTransform != null)
            {
                if (IsClicked)
                {
                    switch (Mode)
                    {
                        case EGizmoMode.Translation:
                            Vector3 newPosition = m_controlledTransform.WorldPosition;
                            if (newPosition != m_originalPosition)
                            {
                                OnTranslationChanged?.Invoke(m_controlledTransform, m_originalPosition, newPosition);
                            }
                            break;
                        case EGizmoMode.Rotation:
                            Quaternion newRotation = m_controlledTransform.WorldRotation;
                            if (newRotation != m_originalRotation)
                            {
                                OnRotationChanged?.Invoke(m_controlledTransform, m_originalRotation, newRotation);
                            }
                            break;
                        case EGizmoMode.Scale:
                            Vector3 newScale = m_controlledTransform.WorldScale;
                            if (newScale != m_originalScale)
                            {
                                OnScaleChanged?.Invoke(m_controlledTransform, m_originalScale, newScale);
                            }
                            break;
                    }
                }

                IsClicked = false;
                m_totalAngleDelta = 0.0f;
                m_totalScaleDelta = 0.0f;
            }
        }

        public EGizmoAxis UpdateActiveAxis(Ray ray)
        {
            m_activeAxis = EGizmoAxis.None;
            m_activeAxisList.Clear();

            List<(float, CGizmoSelectionRegion)> hits = new List<(float, CGizmoSelectionRegion)>();
            foreach (var selectionRegion in m_selectionRegions)
            {
                if (selectionRegion.BoundingBox.Intersects(ref ray, out Vector3 intersectionPoint))
                {
                    float distOnRay = (intersectionPoint - ray.Position).Length();
                    hits.Add((distOnRay, selectionRegion));
                }
            }

            if (hits.Count > 0)
            {
                hits.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                int numActiveAxis = 0;
                foreach (var hit in hits)
                {
                    if (hit.Item2.NumAxis >= MaxActiveAxis)
                    {
                        m_activeAxis = hit.Item2.TargetAxis;
                        break;
                    }
                    else
                    {
                        m_activeAxis |= hit.Item2.TargetAxis;
                        numActiveAxis++;

                        if (numActiveAxis >= MaxActiveAxis)
                        {
                            break;
                        }
                    }
                }

                if (m_activeAxis.HasFlag(EGizmoAxis.Z))
                {
                    m_activeAxisList.Add(IsLocalAligned ? m_controlledTransform.Forward : Axis.Forward);
                }

                if (m_activeAxis.HasFlag(EGizmoAxis.X))
                {
                    m_activeAxisList.Add(IsLocalAligned ? m_controlledTransform.Right : Axis.Right);
                }

                if (m_activeAxis.HasFlag(EGizmoAxis.Y))
                {
                    m_activeAxisList.Add(IsLocalAligned ? m_controlledTransform.Up : Axis.Up);
                }
            }

            return m_activeAxis;
        }

        public void Deselect()
        {
            m_activeAxis = EGizmoAxis.None;
        }

        public void SetControlledTransform(Transform controlledTransform)
        {
            m_controlledTransform = controlledTransform;
        }

        private void Update(float deltaTime)
        {
            if (!IsActive)
            {
                return;
            }

            ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, 0);
            ImGui.PushStyleColor(ImGuiCol.Border, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
            ImGui.Begin("gizmo", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs);
            m_drawList = ImGui.GetWindowDrawList();
            ImGui.End();
            ImGui.PopStyleVar();
            ImGui.PopStyleColor(2);

            World.ViewManager.GetViewInfo(out m_frameViewInfo);
            m_gizmoLocation = m_controlledTransform.WorldPosition;
            m_cameraScaleFactor = m_frameViewInfo.GetScreenScaleFactor(in m_gizmoLocation);

            switch (Mode)
            {
                case EGizmoMode.Translation:
                    m_drawSize = TranslationSize * m_cameraScaleFactor;
                    CreateTranslationSelectionRegions();
                    DrawTranslationGizmo();
                    break;
                case EGizmoMode.Rotation:
                    m_drawSize = RotationSize * m_cameraScaleFactor;
                    CreateRotationSelectionRegions();
                    DrawRotationGizmo();
                    break;
                case EGizmoMode.Scale:
                    m_drawSize = TranslationSize * m_cameraScaleFactor;
                    CreateTranslationSelectionRegions();
                    DrawScaleGizmo();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (IsClicked)
            {
                switch (Mode)
                {
                    case EGizmoMode.Translation:
                        ProcessTranslationDrag();
                        break;
                    case EGizmoMode.Rotation:
                        ProcessRotationDrag();
                        break;
                    case EGizmoMode.Scale:
                        ProcessScaleDrag();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                Ray ray = CreateMouseRay(m_frameViewInfo);
                UpdateActiveAxis(ray);
            }
        }

        private void ProcessTranslationDrag()
        {
            if (m_activeAxis == EGizmoAxis.None)
            {
                return;
            }

            if (m_activeAxisList.Count <= 0)
            {
                return;
            }

            Ray pickRay = CreateMouseRay(in m_frameViewInfo);

            if (m_activeAxisList.Count > 1)
            {
                Vector3 planeNormal = Vector3.Cross(m_activeAxisList[0], m_activeAxisList[1]);
                Plane intersectionPlane = new Plane(m_startLocation, planeNormal);

                if (intersectionPlane.Intersects(ref pickRay, out Vector3 intersection))
                {
                    m_controlledTransform.SetWorldPosition(intersection - m_clickOffset);
                }
            }
            else
            {
                Vector3 planeTangent = Vector3.Cross(m_activeAxisList[0], m_frameViewInfo.ViewLocation - m_gizmoLocation);
                Vector3 planeNormal = Vector3.Cross(m_activeAxisList[0], planeTangent);
                Plane intersectionPlane = new Plane(m_startLocation, planeNormal);

                if (intersectionPlane.Intersects(ref pickRay, out Vector3 intersection))
                {
                    intersection -= m_originalPosition + m_clickOffset;
                    m_controlledTransform.SetWorldPosition(m_originalPosition + m_activeAxisList[0] * Vector3.Dot(intersection, m_activeAxisList[0]));
                }
            }
        }

        private void ProcessRotationDrag()
        {
            if (m_activeAxis == EGizmoAxis.None)
            {
                return;
            }

            if (m_activeAxisList.Count <= 0)
            {
                return;
            }

            Matrix viewProj = m_frameViewInfo.ViewMatrix * m_frameViewInfo.ProjectionMatrix;
            SharpDX.Vector2 mouseDelta = new SharpDX.Vector2(Input.GetNativeAxisValue(EInputAxis.MouseX), -Input.GetNativeAxisValue(EInputAxis.MouseY));
            SharpDX.Vector2 screenAxis = (SharpDX.Vector2)Vector3.TransformNormal(m_rotationDragAxis, viewProj);
            screenAxis.Normalize();

            float deltaMove = -SharpDX.Vector2.Dot(mouseDelta, screenAxis) * RotationSpeed;
            m_totalAngleDelta += deltaMove;

            float snappedAngle = m_totalAngleDelta;
            if (AngleSnap > 0.0f)
            {
                snappedAngle = m_totalAngleDelta - m_totalAngleDelta % AngleSnap;
            }

            Quaternion deltaRotation = Quaternion.RotationAxis(m_activeAxisList[0], snappedAngle);
            m_controlledTransform.SetWorldRotation(deltaRotation * m_originalRotation);
        }

        private void ProcessScaleDrag()
        {
            if (m_activeAxis == EGizmoAxis.None)
            {
                return;
            }

            if (m_activeAxisList.Count <= 0)
            {
                return;
            }

            Matrix viewProj = m_frameViewInfo.ViewMatrix * m_frameViewInfo.ProjectionMatrix;
            SharpDX.Vector2 mouseDelta = new SharpDX.Vector2(Input.GetNativeAxisValue(EInputAxis.MouseX), -Input.GetNativeAxisValue(EInputAxis.MouseY));
            SharpDX.Vector2 screenAxis;

            switch (m_activeAxisList.Count)
            {
                case 1:
                    screenAxis = (SharpDX.Vector2)Vector3.TransformNormal(m_activeAxisList[0], viewProj);
                    break;
                case 2:
                    screenAxis = (SharpDX.Vector2)Vector3.TransformNormal(m_activeAxisList[0] + m_activeAxisList[1], viewProj);
                    break;
                case 3:
                    screenAxis = new SharpDX.Vector2(0.5f, 0.5f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            screenAxis.Normalize();

            float deltaMove = SharpDX.Vector2.Dot(mouseDelta, screenAxis) * ScaleSpeed * m_scaleSizeFactor;
            m_totalScaleDelta += deltaMove;

            float snappedDelta = m_totalScaleDelta;
            if (ScaleSnap > 0.0f)
            {
                snappedDelta = m_totalScaleDelta - m_totalScaleDelta % AngleSnap;
            }

            m_controlledTransform.SetWorldScale(m_originalScale + m_scaleAxis * snappedDelta);
        }

        private Ray CreateMouseRay(in SSceneViewInfo viewInfo)
        {
            CViewManager viewManager = World.ViewManager;
            int mouseAbsX = System.Windows.Forms.Cursor.Position.X - (int)viewManager.ScreenLeft;
            int mouseAbsY = System.Windows.Forms.Cursor.Position.Y - (int)viewManager.ScreenTop;
            return Ray.GetPickRay(mouseAbsX, mouseAbsY, new ViewportF(0, 0, viewManager.ScreenWidth, viewManager.ScreenHeight), viewInfo.ViewMatrix * viewInfo.ProjectionMatrix);
        }

        private void DrawTranslationGizmo()
        {
            CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
            Matrix viewProj = m_frameViewInfo.ViewMatrix * m_frameViewInfo.ProjectionMatrix;
            foreach (var selectionRegion in m_selectionRegions)
            {
                if (selectionRegion.NumAxis < 2)
                {
                    selectionRegion.BoundingBox.Transformation.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
                    Vector3 axis = Vector3.Transform(Axis.Forward, rotation);
                    axis.Normalize();
                    float drawLength = selectionRegion.BoundingBox.Size.Z;

                    Color4 drawColor = m_activeAxis.HasFlag(selectionRegion.TargetAxis) ? SelectedColor : selectionRegion.DrawColor;
                    Vector3 toCamera = m_frameViewInfo.ViewLocation - m_gizmoLocation;
                    toCamera.Normalize();
                    Quaternion rot = Quaternion.RotationAxis(Axis.Right, MathUtil.PiOverTwo);


                    Vector3 startWorld = m_gizmoLocation;
                    Vector3 endWorld = startWorld + axis * drawLength;
                    debugRenderer.DrawCone(endWorld, drawLength * 0.15f, selectionRegion.BoundingBox.Size.X * 0.15f, rotation * rot, drawColor, 0.0f, EDebugDrawCommandFlags.NoDepthTest);

                    if (m_frameViewInfo.CameraFrustum.Contains(ref startWorld) == ContainmentType.Disjoint && m_frameViewInfo.CameraFrustum.Contains(ref endWorld) == ContainmentType.Disjoint)
                    {
                        continue;
                    }

                    Vector2 startWindow = m_frameViewInfo.WorldToScreenNumeric(startWorld);
                    Vector2 endWindow = m_frameViewInfo.WorldToScreenNumeric(endWorld);

                    uint uiColor = ImGui.ColorConvertFloat4ToU32(drawColor.ToVector4().ToNumericVector());
                    m_drawList.AddLine(startWindow, endWindow, uiColor, 3.0f);

                    if (IsClicked && m_activeAxis.HasFlag(selectionRegion.TargetAxis))
                    {
                        Vector3 lineStart = m_originalPosition - axis * 9999.9f;
                        Vector3 lineEnd = m_originalPosition + axis * 9999.9f;
                        debugRenderer.DrawLine(lineStart, selectionRegion.DrawColor, lineEnd, selectionRegion.DrawColor, 0.0f);
                    }
                }
                else
                {
                    DrawPlane(selectionRegion.BoundingBox, debugRenderer, m_activeAxis.HasFlag(selectionRegion.TargetAxis), selectionRegion.DrawColor);
                }
            }
        }

        private void DrawRotationGizmo()
        {
            CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
            float drawRadius = 0.0f;
            foreach (var selectionRegion in m_selectionRegions)
            {
                selectionRegion.BoundingBox.Transformation.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
                Vector3 axis = Vector3.Transform(Axis.Forward, rotation);
                axis.Normalize();
                drawRadius = selectionRegion.BoundingBox.Size.X / 2;

                Color4 drawColor = m_activeAxis.HasFlag(selectionRegion.TargetAxis) ? SelectedColor : selectionRegion.DrawColor;
                Vector3 toCamera = m_frameViewInfo.ViewLocation - m_gizmoLocation;
                toCamera.Normalize();
                Draw2DCircle(m_gizmoLocation, axis, toCamera, drawRadius, m_rotationCircleFraction, drawColor, 32);

                if (IsClicked)
                {
                    if (m_activeAxis.HasFlag(selectionRegion.TargetAxis))
                    {
                        debugRenderer.DrawLine(m_gizmoLocation - axis * 9999.9f, selectionRegion.DrawColor, m_gizmoLocation + axis * 9999.9f, selectionRegion.DrawColor, 0.0f, EDebugDrawCommandFlags.NoDepthTest);
                    }
                    debugRenderer.DrawArrow(m_startLocation, m_rotationDragAxis, drawRadius / 2, drawColor, 0.0f);
                }
            }

            BoundingSphere sphere = new BoundingSphere(m_gizmoLocation, drawRadius);
            if (m_frameViewInfo.CameraFrustum.Contains(sphere) != ContainmentType.Disjoint)
            {
                Vector2 circleCenter = m_frameViewInfo.WorldToScreenNumeric(in m_gizmoLocation);
                m_drawList.AddCircleFilled(circleCenter, RotationSize * m_frameViewInfo.ScreenHeight * m_circleFactor, ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0, 0, 0, 0.2f)), 32);
            }
        }

        private void DrawScaleGizmo()
        {
            CDebugRenderer debugRenderer = CRenderer.Instance.ActiveScene.DebugRenderer;
            Matrix viewProj = m_frameViewInfo.ViewMatrix * m_frameViewInfo.ProjectionMatrix;
            foreach (var selectionRegion in m_selectionRegions)
            {
                if (selectionRegion.NumAxis < 2)
                {
                    selectionRegion.BoundingBox.Transformation.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
                    Vector3 axis = Vector3.Transform(Axis.Forward, rotation);
                    axis.Normalize();
                    float drawLength = selectionRegion.BoundingBox.Size.Z;

                    Color4 drawColor = m_activeAxis.HasFlag(selectionRegion.TargetAxis) ? SelectedColor : selectionRegion.DrawColor;
                    Vector3 toCamera = m_frameViewInfo.ViewLocation - m_gizmoLocation;
                    toCamera.Normalize();
                    Quaternion rot = Quaternion.RotationAxis(Axis.Right, MathUtil.PiOverTwo);


                    Vector3 startWorld = m_gizmoLocation;
                    Vector3 endWorld = startWorld + axis * drawLength;
                    debugRenderer.DrawBox(endWorld, rotation, new Vector3(selectionRegion.BoundingBox.Size.X * 0.5f), drawColor, 0.0f, EDebugDrawCommandFlags.NoDepthTest);

                    if (m_frameViewInfo.CameraFrustum.Contains(ref startWorld) == ContainmentType.Disjoint && m_frameViewInfo.CameraFrustum.Contains(ref endWorld) == ContainmentType.Disjoint)
                    {
                        continue;
                    }

                    Vector2 startWindow = m_frameViewInfo.WorldToScreenNumeric(startWorld);
                    Vector2 endWindow = m_frameViewInfo.WorldToScreenNumeric(endWorld);

                    uint uiColor = ImGui.ColorConvertFloat4ToU32(drawColor.ToVector4().ToNumericVector());
                    m_drawList.AddLine(startWindow, endWindow, uiColor, 3.0f);

                    if (IsClicked && m_activeAxis.HasFlag(selectionRegion.TargetAxis))
                    {
                        Vector3 lineStart = m_gizmoLocation - axis * 9999.9f;
                        Vector3 lineEnd = m_gizmoLocation + axis * 9999.9f;
                        debugRenderer.DrawLine(lineStart, selectionRegion.DrawColor, lineEnd, selectionRegion.DrawColor, 0.0f);
                    }
                }
                else
                {
                    DrawPlane(selectionRegion.BoundingBox, debugRenderer, m_activeAxis.HasFlag(selectionRegion.TargetAxis), selectionRegion.DrawColor);
                }
            }
        }

        private void Draw2DCircle(Vector3 center, Vector3 axis, Vector3 up, float radius, float fraction, Color4 color, int segments)
        {
            // Check if inside camera frustum
            BoundingSphere boundsSphere = new BoundingSphere(center, radius);
            if (m_frameViewInfo.CameraFrustum.Contains(boundsSphere) == ContainmentType.Disjoint)
            {
                return;
            }

            fraction = MathUtil.Clamp(fraction, 0.0f, 1.0f);
            axis.Normalize();
            if (Vector3.NearEqual(axis, up, new Vector3(0.01f)))
            {
                up = Axis.Right;
            }

            Matrix circleMatrix = Matrix.LookAtLH(center, center + axis, up);
            circleMatrix.Invert();
            float step = MathUtil.TwoPi * fraction / segments;
            float rad = 0;

            Vector2[] screenPoints = new Vector2[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                Vector3 p1 = new Vector3(MathUtilities.Cosf(rad) * radius, MathUtilities.Sinf(rad) * radius, 0);
                p1 = Vector3.TransformCoordinate(p1, circleMatrix);
                screenPoints[i] = m_frameViewInfo.WorldToScreenNumeric(p1);

                rad += step;
            }

            m_drawList.AddPolyline(ref screenPoints[0], segments + 1, ImGui.ColorConvertFloat4ToU32(color.ToVector4().ToNumericVector()), false, 3.0f);
        }

        private void DrawPlane(OrientedBoundingBox obb, CDebugRenderer debugRenderer, bool bHovered, Color4 drawColor)
        {
            drawColor = bHovered ? SelectedColor : drawColor;
            obb.Transformation.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation);
            Vector3 boxExtent = obb.Size;
            boxExtent.Z = 0;
            obb.Extents = boxExtent;
            SharpDX.Vector2 quadExtent = new SharpDX.Vector2(boxExtent.X, boxExtent.Y);
            Vector3 p1 = Vector3.TransformCoordinate((Vector3)(new SharpDX.Vector2(-0.5f, 0.5f) * quadExtent), obb.Transformation);
            Vector3 p2 = Vector3.TransformCoordinate((Vector3)(new SharpDX.Vector2(0.5f, 0.5f) * quadExtent), obb.Transformation);
            Vector3 p3 = Vector3.TransformCoordinate((Vector3)(new SharpDX.Vector2(0.5f, -0.5f) * quadExtent), obb.Transformation);
            Vector3 p4 = Vector3.TransformCoordinate((Vector3)(new SharpDX.Vector2(-0.5f, -0.5f) * quadExtent), obb.Transformation);

            if (m_frameViewInfo.CameraFrustum.Contains(ref p1) == ContainmentType.Disjoint
                && m_frameViewInfo.CameraFrustum.Contains(ref p2) == ContainmentType.Disjoint
                && m_frameViewInfo.CameraFrustum.Contains(ref p3) == ContainmentType.Disjoint
                && m_frameViewInfo.CameraFrustum.Contains(ref p4) == ContainmentType.Disjoint)
            {
                return;
            }

            Vector2 scP1 = m_frameViewInfo.WorldToScreenNumeric(in p1);
            Vector2 scP2 = m_frameViewInfo.WorldToScreenNumeric(in p2);
            Vector2 scP3 = m_frameViewInfo.WorldToScreenNumeric(in p3);
            Vector2 scP4 = m_frameViewInfo.WorldToScreenNumeric(in p4);
            uint uiColor = ImGui.ColorConvertFloat4ToU32(drawColor.ToVector4().ToNumericVector());

            m_drawList.AddQuad(scP1, scP2, scP3, scP4, uiColor, 3.0f);
            Color4 fillColor = drawColor;
            fillColor.Alpha = 0.4f;
            m_drawList.AddQuadFilled(scP1, scP2, scP3, scP4, ImGui.ColorConvertFloat4ToU32(fillColor.ToVector4().ToNumericVector()));
        }

        private void CreateTranslationSelectionRegions()
        {
            m_selectionRegions.Clear();

            BoundingBox defaultAABB = new BoundingBox(new Vector3(-0.1f, -0.1f, 0.0f) * m_drawSize, new Vector3(0.1f, 0.1f, 1.0f) * m_drawSize);
            Quaternion forwardRotation = IsLocalAligned ? MathUtilities.CreateLookAtQuaternion(m_controlledTransform.Forward, m_controlledTransform.Up) : MathUtilities.CreateLookAtQuaternion(Axis.Forward, Axis.Up);
            Matrix forwardTransform = Matrix.AffineTransformation(1.0f, forwardRotation, m_gizmoLocation);
            OrientedBoundingBox forwardOBB = new OrientedBoundingBox(defaultAABB);
            forwardOBB.Transform(forwardTransform);
            CGizmoSelectionRegion forwardAxiSelectionRegion = new CGizmoSelectionRegion(forwardOBB, EGizmoAxis.Z, Color.Blue);
            m_selectionRegions.Add(forwardAxiSelectionRegion);

            Quaternion rightRotation = IsLocalAligned ? MathUtilities.CreateLookAtQuaternion(m_controlledTransform.Right, m_controlledTransform.Up) : MathUtilities.CreateLookAtQuaternion(Axis.Right, Axis.Up);
            Matrix rightTransform = Matrix.AffineTransformation(1.0f, rightRotation, m_gizmoLocation);
            OrientedBoundingBox rightOBB = new OrientedBoundingBox(defaultAABB);
            rightOBB.Transform(ref rightTransform);
            CGizmoSelectionRegion rightSelectionRegion = new CGizmoSelectionRegion(rightOBB, EGizmoAxis.X, Color.Red);
            m_selectionRegions.Add(rightSelectionRegion);

            Quaternion upRotation = IsLocalAligned ? MathUtilities.CreateLookAtQuaternion(m_controlledTransform.Up, m_controlledTransform.Right) : MathUtilities.CreateLookAtQuaternion(Axis.Up, Axis.Right);
            Matrix upTransform = Matrix.AffineTransformation(1.0f, upRotation, m_gizmoLocation);
            OrientedBoundingBox upOBB = new OrientedBoundingBox(defaultAABB);
            upOBB.Transform(upTransform);
            CGizmoSelectionRegion upSelectionRegion = new CGizmoSelectionRegion(upOBB, EGizmoAxis.Y, Color.Green);
            m_selectionRegions.Add(upSelectionRegion);

            BoundingBox defaultMulti = new BoundingBox(new Vector3(-0.15f, -0.15f, -0.05f) * m_drawSize, new Vector3(0.15f, 0.15f, 0.05f) * m_drawSize);
            Vector3 forwardAxis = IsLocalAligned ? m_controlledTransform.Forward : Axis.Forward;
            Vector3 rightAxis = IsLocalAligned ? m_controlledTransform.Right : Axis.Right;
            Vector3 upAxis = IsLocalAligned ? m_controlledTransform.Up : Axis.Up;

            Vector3 xyLocation = m_gizmoLocation + upAxis * m_drawSize * 0.3f + rightAxis * m_drawSize * 0.3f;
            OrientedBoundingBox xyBoundingBox = new OrientedBoundingBox(defaultMulti);
            xyBoundingBox.Transform(Matrix.AffineTransformation(1.0f, forwardRotation, xyLocation));
            m_selectionRegions.Add(new CGizmoSelectionRegion(xyBoundingBox, EGizmoAxis.X | EGizmoAxis.Y, Color.Blue));

            Vector3 xzLocation = m_gizmoLocation + forwardAxis * m_drawSize * 0.3f + rightAxis * m_drawSize * 0.3f;
            OrientedBoundingBox xzBoundingBox = new OrientedBoundingBox(defaultMulti);
            xzBoundingBox.Transform(Matrix.AffineTransformation(1.0f, upRotation, xzLocation));
            m_selectionRegions.Add(new CGizmoSelectionRegion(xzBoundingBox, EGizmoAxis.X | EGizmoAxis.Z, Color.Green));

            Vector3 zyLocation = m_gizmoLocation + forwardAxis * m_drawSize * 0.3f + upAxis * m_drawSize * 0.3f;
            OrientedBoundingBox zyBoundingBox = new OrientedBoundingBox(defaultMulti);
            zyBoundingBox.Transform(Matrix.AffineTransformation(1.0f, rightRotation, zyLocation));
            m_selectionRegions.Add(new CGizmoSelectionRegion(zyBoundingBox, EGizmoAxis.Y | EGizmoAxis.Z, Color.Red));
        }

        private void CreateRotationSelectionRegions()
        {
            m_selectionRegions.Clear();

            Quaternion forwardRotation = IsLocalAligned ? MathUtilities.CreateLookAtQuaternion(m_controlledTransform.Forward, m_controlledTransform.Up) : MathUtilities.CreateLookAtQuaternion(Axis.Forward, Axis.Up);
            Quaternion rightRotation = IsLocalAligned ? MathUtilities.CreateLookAtQuaternion(m_controlledTransform.Right, m_controlledTransform.Up) : MathUtilities.CreateLookAtQuaternion(Axis.Right, Axis.Up);
            Quaternion upRotation = IsLocalAligned ? MathUtilities.CreateLookAtQuaternion(m_controlledTransform.Up, m_controlledTransform.Right) : MathUtilities.CreateLookAtQuaternion(Axis.Up, Axis.Right);

            BoundingBox defaultMulti = new BoundingBox(new Vector3(-0.5f, -0.5f, -0.05f) * m_drawSize, new Vector3(0.5f, 0.5f, 0.05f) * m_drawSize);

            OrientedBoundingBox xyBoundingBox = new OrientedBoundingBox(defaultMulti);
            xyBoundingBox.Transform(Matrix.AffineTransformation(1.0f, forwardRotation, m_gizmoLocation));
            m_selectionRegions.Add(new CGizmoSelectionRegion(xyBoundingBox, EGizmoAxis.Z, Color.Blue));

            OrientedBoundingBox xzBoundingBox = new OrientedBoundingBox(defaultMulti);
            xzBoundingBox.Transform(Matrix.AffineTransformation(1.0f, upRotation, m_gizmoLocation));
            m_selectionRegions.Add(new CGizmoSelectionRegion(xzBoundingBox, EGizmoAxis.Y, Color.Green));

            OrientedBoundingBox zyBoundingBox = new OrientedBoundingBox(defaultMulti);
            zyBoundingBox.Transform(Matrix.AffineTransformation(1.0f, rightRotation, m_gizmoLocation));
            m_selectionRegions.Add(new CGizmoSelectionRegion(zyBoundingBox, EGizmoAxis.X, Color.Red));
        }

		private void DrawEditBox()
		{
			ImGui.Begin("Transform Editor");
			System.Numerics.Vector3 localLocation = m_controlledTransform.Position.ToNumericVector();
			if (ImGui.DragFloat3("Location", ref localLocation, 0.1f))
			{
				m_controlledTransform.Position = localLocation.ToSharpVector();
			}

			System.Numerics.Vector3 localRotationEuler = m_controlledTransform.Rotation.ToEuler().ToNumericVector();
			//System.Numerics.Vector3 localRotationEuler = m_controlledTransform.LocalMatrix.GetEulerRotation().ToNumericVector();
			ImGui.Checkbox("Apply Rotation", ref m_bApplyRotation);
			if (ImGui.DragFloat3("Rotation", ref localRotationEuler, 0.01f))
			{
				if (m_bApplyRotation)
				{
					//m_controlledTransform.Rotation = Quaternion.RotationYawPitchRoll(localRotationEuler.Y, localRotationEuler.X, localRotationEuler.Z);
					m_controlledTransform.Rotation = localRotationEuler.ToSharpVector().EulerToQuaternion();
				}
			}

			System.Numerics.Vector3 localScale = m_controlledTransform.Scale.ToNumericVector();
			if (ImGui.DragFloat3("Scale", ref localScale, 0.001f))
			{
				m_controlledTransform.Scale = localScale.ToSharpVector();
			}
			ImGui.End();
		}

        public float TranslationSize { get; set; } = 0.3f;
        public float RotationSize { get; set; } = 0.5f;
        public bool IsLocalAligned { get; set; } = true;
        public float RotationSpeed { get; set; } = 0.005f;
        public float ScaleSpeed { get; set; } = 0.01f;
        public int MaxActiveAxis { get; private set; } = 2;

        public float AngleSnap { get; set; } = MathUtil.DegreesToRadians(5.0f);
        public float ScaleSnap { get; set; } = 0.0f;

        public bool IsClicked { get; private set; }
        public bool IsActive { get; set; }

        private EGizmoMode m_mode = EGizmoMode.Translation;
        public EGizmoMode Mode
        {
            get { return m_mode; }
            set
            {
                if (m_mode != value)
                {
                    m_mode = value;
                    IsClicked = false;
                    switch (m_mode)
                    {
                        case EGizmoMode.Translation:
                            MaxActiveAxis = 2;
                            break;
                        case EGizmoMode.Rotation:
                            MaxActiveAxis = 1;
                            break;
                        case EGizmoMode.Scale:
                            MaxActiveAxis = 3;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public bool IsHovered
        {
            get { return m_activeAxis != EGizmoAxis.None; }
        }

        public Color4 SelectedColor { get; set; } = Color.Gold.ToColor4();

        private readonly List<CGizmoSelectionRegion> m_selectionRegions = new List<CGizmoSelectionRegion>();

        private CUpdateScope m_updateScope;
        private EGizmoAxis m_activeAxis = EGizmoAxis.None;
        private List<Vector3> m_activeAxisList = new List<Vector3>();

        public float m_rotationCircleFraction = 0.5f;
        private Vector3 m_clickOffset;
        private Vector3 m_startLocation;
        private Vector3 m_originalPosition;
        private Vector3 m_rotationDragAxis;
        private Quaternion m_originalRotation;
        private Vector3 m_originalScale;

        private Vector3 m_gizmoLocation;
        private float m_cameraScaleFactor;
        private float m_drawSize;

        private SSceneViewInfo m_frameViewInfo;
        private ImDrawListPtr m_drawList;
        private float m_circleFactor = 0.25f;

        private float m_totalAngleDelta = 0.0f;
        private float m_totalScaleDelta = 0.0f;

        private float m_scaleSizeFactor;
        private Vector3 m_scaleAxis;

        private Transform m_controlledTransform = new Transform();

		private bool m_bApplyRotation = false;
	}
}
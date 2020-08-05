using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace CustomURP
{
    public class PlaneReflectionProcessor
    {
        public void SetResolutionType(RESOLUTION_TYPE type) { m_setting.resolutionType = type; }
        public void SetPlaneOffest(float planeOffset) { m_setting.planeOffset = planeOffset; }
        public void SetClipPlaneOffest(float clipPlaneOffest) { m_setting.clipPlaneOffest = clipPlaneOffest; }
        public void SetReflectLayers(ref LayerMask reflectLayers) { m_setting.reflectLayers = reflectLayers; }
        public void SetTargetPosition(ref Vector3 position) { m_setting.targetPosition = position; }
        public void SetTargetNormal(ref Vector3 normal) { m_setting.targetNormal = normal; }
        public void SetPlaneReflectionSettingInfo(ref PlaneReflectionSettingInfo setting)
        {
            m_setting.Assign(ref setting);
        }

        public RenderTexture GetReflectionRenderTexture() { return m_reflectionRT; }

        public bool Initialize()
        {
            RenderPipelineManager.beginCameraRendering += ProcessPlanarReflection;
            return true;
        }
        public bool Release()
        {
            RenderPipelineManager.beginCameraRendering -= ProcessPlanarReflection;
            if (m_reflectionRT)
            {
                RenderTexture.ReleaseTemporary(m_reflectionRT);
                m_reflectionRT = null;
            }
            
            if (m_reflectionCamera)
            {
                m_reflectionCamera.targetTexture = null;
                GameObject taragetGameObject = m_reflectionCamera.gameObject;
                if (Application.isEditor)
                    GameObject.DestroyImmediate(taragetGameObject);
                else
                    GameObject.Destroy(taragetGameObject);
                m_reflectionCamera = null;
            }
            return true;
        }
        private void ProcessPlanarReflection(ScriptableRenderContext context, Camera camera)
        {
            // we dont want to render planar reflections in reflections or previews
            if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
                return;
            SetupReflectionTexture(camera);
            UpdateReflectionCamera(camera);

            //render plane reflection render setting...
            GL.invertCulling = true;
            bool originalFogSetate = RenderSettings.fog;
            int originalMaxLODLevel = QualitySettings.maximumLODLevel;
            float originalLODBias = QualitySettings.lodBias;
            RenderSettings.fog = false; // disable fog for now as it's incorrect with projection
            QualitySettings.maximumLODLevel = 1;
            QualitySettings.lodBias = originalLODBias * 0.5f;

            //觸發一個plane reflection的事件…外面需要知道嗎?…
            //BeginPlanarReflections?.Invoke(context, _reflectionCamera); // callback Action for PlanarReflection
            UniversalRenderPipeline.RenderSingleCamera(context, m_reflectionCamera); // render planar reflections

            //restore original render setting...
            GL.invertCulling = false;
            RenderSettings.fog = originalFogSetate;
            QualitySettings.maximumLODLevel = originalMaxLODLevel;
            QualitySettings.lodBias = originalLODBias;

            // Assign texture to water shader
            Shader.SetGlobalTexture(m_planarReflectionTextureId, m_reflectionRT);
        }

        private void UpdateReflectionCamera(Camera targetCamera)
        {
            if (m_reflectionCamera == null)
                m_reflectionCamera = CreateCameraObject();

            CopyTargetCameraData(targetCamera);
            
            Vector3 position = m_setting.targetPosition;
            Vector3 normal = m_setting.targetNormal;
            position.y += m_setting.planeOffset;

            // Render reflection
            // Reflect camera around reflection plane
            var d = -Vector3.Dot(normal, position) - m_setting.clipPlaneOffest;
            var reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            var reflection = Matrix4x4.identity;
            reflection *= Matrix4x4.Scale(new Vector3(1, -1, 1));

            CalculateReflectionMatrix(ref reflection, reflectionPlane);
            var oldPosition = targetCamera.transform.position - new Vector3(0, position.y * 2, 0);
            var newPosition = ReflectPosition(oldPosition);
            m_reflectionCamera.transform.forward = Vector3.Scale(targetCamera.transform.forward, new Vector3(1, -1, 1));
            m_reflectionCamera.worldToCameraMatrix = targetCamera.worldToCameraMatrix * reflection;

            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            var clipPlane = CameraSpacePlane(m_reflectionCamera, position - Vector3.up * 0.1f, normal, 1.0f, m_setting.clipPlaneOffest);
            var projection = targetCamera.CalculateObliqueMatrix(clipPlane);

            m_reflectionCamera.projectionMatrix = projection;

            //use in water, remember ingore ware...
            m_reflectionCamera.cullingMask = m_setting.reflectLayers;
            m_reflectionCamera.transform.position = newPosition;
            //每個frame都要設…
            m_reflectionCamera.targetTexture = m_reflectionRT;
        }

        private static Vector3 ReflectPosition(Vector3 position)
        {
            var newPos = new Vector3(position.x, -position.y, position.z);
            return newPos;
        }

        private bool CopyTargetCameraData(Camera targetCamera)
        {
            if ((!m_reflectionCamera) || (!targetCamera))
                return false;
            m_reflectionCamera.CopyFrom(targetCamera);
            m_reflectionCamera.useOcclusionCulling = false;
            return true;
        }

        private Camera CreateCameraObject()
        {
            var go = new GameObject("Planar Reflections", typeof(Camera));
            var reflectionCamera = go.GetComponent<Camera>();
            reflectionCamera.enabled = false;
            go.hideFlags = HideFlags.HideAndDontSave;
            return reflectionCamera;
        }

        private void SetupReflectionTexture(Camera camera)
        {
            if (!camera)
                return;

            float scaleRatio = UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.renderScale * GetScaleValue();

            int width = (int)(camera.pixelWidth * scaleRatio);
            int height = (int)(camera.pixelHeight * scaleRatio);

            if (m_reflectionRT)
            {
                if ((m_reflectionRT.width == width) && (m_reflectionRT.height == height))
                    return;
                else
                {
                    RenderTexture.ReleaseTemporary(m_reflectionRT);
                    m_reflectionRT = null;                    
                }
            }
            const bool useHdr10 = true;
            const RenderTextureFormat hdrFormat = useHdr10 ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.DefaultHDR;
            //m_reflectionRT = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
            m_reflectionRT = RenderTexture.GetTemporary(width, height, 16, GraphicsFormatUtility.GetGraphicsFormat(hdrFormat, true));
        }

        // Calculates reflection matrix around the given plane
        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Camera camera, Vector3 position, Vector3 normal, float sideSign, float clipPlaneOffset)
        {
            //var offsetPos = pos + normal * m_settings.m_ClipPlaneOffset;
            var offsetPos = position + normal * clipPlaneOffset;
            var m = camera.worldToCameraMatrix;
            var cameraPosition = m.MultiplyPoint(offsetPos);
            var cameraNormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
        }

        private float GetScaleValue()
        {
            switch (m_setting.resolutionType)
            {
                case RESOLUTION_TYPE.FULL:
                    return 1f;
                case RESOLUTION_TYPE.HALF:
                    return 0.5f;
                case RESOLUTION_TYPE.THIRD:
                    return 0.33f;
                case RESOLUTION_TYPE.QUARTER:
                    return 0.25f;
                default:
                    return 0.5f; // default to half res
            }
        }

        private RenderTexture m_reflectionRT = null;        

        private static Camera m_reflectionCamera = null;

        [System.Serializable]
        public enum RESOLUTION_TYPE
        {
            FULL,
            HALF,
            THIRD,
            QUARTER
        }

        [System.Serializable]
        public class PlaneReflectionSettingInfo
        {
            //public float RTScaleRatio = 0.5f;
            public RESOLUTION_TYPE resolutionType = RESOLUTION_TYPE.HALF;
            public float planeOffset = 0.0f;
            public float clipPlaneOffest = 0.07f;
            public LayerMask reflectLayers = ~0;
            public Vector3 targetPosition = Vector3.zero;
            public Vector3 targetNormal = Vector3.up;            
            public void Assign(ref PlaneReflectionSettingInfo setting)
            {
                //RTScaleRatio = setting.RTScaleRatio;
                resolutionType = setting.resolutionType;
                planeOffset = setting.planeOffset;
                clipPlaneOffest = setting.clipPlaneOffest;
                reflectLayers = setting.reflectLayers;
                targetPosition = setting.targetPosition;
                targetNormal = setting.targetNormal;
            }
        };

        private PlaneReflectionSettingInfo m_setting = new PlaneReflectionSettingInfo();
        private readonly int m_planarReflectionTextureId = Shader.PropertyToID("_PlanarReflectionTexture");
    }
}

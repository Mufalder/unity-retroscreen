using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorthLab.Effects
{
    public class RetroScreen : MonoBehaviour
    {

        private struct CameraSettings
        {

            public bool Enabled { get; private set; }
            public string Tag { get; private set; }
            public bool MSAA { get; private set; }
            public bool HDR { get; private set; }
            public int CullingMask { get; private set; }
            public RenderTexture TargetTexture { get; private set; }

            public CameraSettings(bool enabled, string tag, bool mSAA, bool hDR, int cullingMask, RenderTexture renderTexture)
            {
                Enabled = enabled;
                Tag = tag;
                MSAA = mSAA;
                HDR = hDR;
                CullingMask = cullingMask;
                TargetTexture = renderTexture;
            }

            public CameraSettings(Camera cam)
            {
                Enabled = cam.enabled;
                Tag = cam.tag;
                MSAA = cam.allowMSAA;
                HDR = cam.allowHDR;
                CullingMask = cam.cullingMask;
                TargetTexture = cam.targetTexture;
            }

            public void SetToCamera(Camera camera)
            {
                camera.enabled = Enabled;
                camera.tag = Tag;
                camera.allowMSAA = MSAA;
                camera.allowHDR = HDR;
                camera.cullingMask = CullingMask;
                camera.targetTexture = TargetTexture;
            }

        }

        [SerializeField, Tooltip("Assign built-in quad mesh here.")]
        private Mesh quad = null;
        [SerializeField, Tooltip("The target height of the resolution, the width will be calculated according to the current screen resolution ratio.")]
        private int targetHeight = 240;
        /// <summary>
        /// The target height of the resolution, the width will be calculated according to the current screen resolution ratio.
        /// </summary>
        public int TargetHeight
        {
            get => targetHeight;
            set
            {
                if (targetHeight == value)
                    return;

                targetHeight = value;
                CheckForChanges(true);
            }
        }
        [SerializeField, Tooltip("When true camera will render with specified FPS.")]
        private bool overrideFPS = true;
        /// <summary>
        /// When true camera will render with specified <see cref="FPS"/>.
        /// </summary>
        public bool OverrideFPS
        {
            get => overrideFPS;
            set
            {
                overrideFPS = value;
            }
        }
        [SerializeField, Range(MinFPS, MaxFPS), Tooltip("Texture render frame rate.")]
        private int fps = 30;
        /// <summary>
        /// Texture render frame rate.
        /// </summary>
        public int FPS
        {
            get => fps;
            set
            {
                if (fps == value)
                    return;

                fps = value;
                CheckForChanges(true);
            }
        }
        [SerializeField, Tooltip("Render texture filtering mode. Use Point for that retro look.")]
        private FilterMode filterMode = FilterMode.Point;
        /// <summary>
        /// Render texture filtering mode. Use Point for that retro look.
        /// </summary>
        public FilterMode FilterMode
        {
            get => filterMode;
            set
            {
                if (filterMode == value)
                    return;

                filterMode = value;
                CheckForChanges(true);
            }
        }
        [SerializeField, Header("Shader settings"), Tooltip("The shader will be assigned to the quad with render texture. Do not recommend touching these two next fields.")]
        private string shaderName = "Unlit/Texture";
        [SerializeField]
        private string mainTextureName = "_MainTex";

        /// <summary>
        /// The camera that renders to the render texture.
        /// </summary>
        public Camera RenderCamera { get; private set; }
        /// <summary>
        /// Camera that shows the quad with the render texture.
        /// </summary>
        public Camera DisplayCamera { get; private set; }
        /// <summary>
        /// Quad with the render texture.
        /// </summary>
        public Transform DisplayQuad { get; private set; }
        /// <summary>
        /// Material of the <see cref="DisplayQuad"/>.
        /// </summary>
        public Material DisplayMaterial { get; private set; }
        /// <summary>
        /// Frame render delay. In seconds.
        /// </summary>
        public float Delay { get; private set; }
        public bool Initialized { get; private set; } = false;

        private RenderTexture rt;
        private int oldScreenWidth;
        private int oldScreenHeight;
        private int oldHeight;
        private int oldFps;
        private FilterMode oldFilterMode;
        private CameraSettings oldSettings;
        private CameraSettings newSettings;
        private IEnumerator coroutine;
        private bool quitting;

        private const string FallbackShader = "Unlit/Texture";
        private const string FallbackMainTexture = "_MainTex";
        private const string LayerName = "RetroScreen";
        private const int MinHeight = 2;
        private const int MinFPS = 1;
        private const int MaxFPS = 240;

        public delegate void OnSettingsChanged(RetroScreen sender);
        public static OnSettingsChanged onSettingsChanged;

        public delegate void OnFrameRendered(RetroScreen sender);
        /// <summary>
        /// Only works when <see cref="OverrideFPS"/> is true.
        /// </summary>
        public static OnFrameRendered onFrameRendered;

        public static RetroScreen SceneInstance { get; private set; } = null;

        public static int? DisplayLayer { get; private set; }

        private void Awake()
        {
            //Keeps only one instance per scene
            if (SceneInstance == null)
            {
                SceneInstance = this;
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        private void OnEnable()
        {
            if (!Initialized)
            {
                Init();
            }
            else
            {
                DisplayCamera.gameObject.SetActive(true);
                newSettings.SetToCamera(RenderCamera);
                CheckForChanges(true);
                StartCoroutine(coroutine);
            }
        }

        private void OnDisable()
        {
            if (quitting)
                return;

            if (Initialized)
            {
                DisplayCamera.gameObject.SetActive(false);
                oldSettings.SetToCamera(RenderCamera);
                StopCoroutine(coroutine);
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                CheckForChanges(false);
            }
        }

        private void Update()
        {
            CheckForChanges(true);
        }

        private void LateUpdate()
        {
            if (!Initialized)
                return;

            DisplayCamera.transform.position = RenderCamera.transform.position;
            DisplayCamera.transform.rotation = RenderCamera.transform.rotation;
        }

        private void OnApplicationQuit()
        {
            quitting = true;
        }

        private void Init()
        {
            if (Initialized)
                return;

            UpdateFPS();
            coroutine = RenderToTexture();
            StartCoroutine(coroutine);

            if (DisplayLayer == null)
            {
                DisplayLayer = LayerMask.NameToLayer(LayerName);

                if (DisplayLayer.Value < 0)
                {
                    Debug.LogError(string.Format("Unable to find layer \"{0}\". Make sure you created a layer with that name.", LayerName));
                    return;
                }
            }

            if (quad == null)
            {
                Debug.LogError("Quad mesh is not assigned!");
                return;
            }

            RenderCamera = GetComponent<Camera>();
            if (!RenderCamera || RenderCamera.tag != "MainCamera")
            {
                Debug.LogError("This script is not attached to the camera or it is not main!");
                return;
            }

            float ratio = (float)Screen.width / (float)Screen.height;
            int width = (int)(ratio * targetHeight);
            oldHeight = targetHeight;

            rt = new RenderTexture(width, targetHeight, 24);
            rt.filterMode = filterMode;
            RenderTexture.active = rt;

            oldSettings = new CameraSettings(RenderCamera);
            RenderCamera.tag = "Untagged";
            RenderCamera.allowMSAA = false;
            RenderCamera.allowHDR = false;
            RenderCamera.targetTexture = rt;
            RenderCamera.cullingMask = RenderCamera.cullingMask & ~(1 << DisplayLayer.Value);
            RenderCamera.enabled = false;
            RenderCamera.Render();
            onFrameRendered?.Invoke(this);
            newSettings = new CameraSettings(RenderCamera);

            DisplayCamera = new GameObject("DisplayCamera").AddComponent<Camera>();
            DisplayCamera.transform.tag = "MainCamera";
            //DisplayCamera.transform.position = Vector3.up * 1000;
            DisplayCamera.clearFlags = CameraClearFlags.Depth;
            DisplayCamera.cullingMask = 1 << DisplayLayer.Value;
            DisplayCamera.orthographic = true;
            DisplayCamera.orthographicSize = 0.5f;
            DisplayCamera.farClipPlane = 5;
            DisplayCamera.allowHDR = false;
            DisplayCamera.allowMSAA = false;

            if (!Shader.Find(shaderName))
            {
                Debug.LogError(string.Format("Shader with the name \"{0}\" not found. Falling back to the \"{1}\" shader.", shaderName, FallbackShader));
                shaderName = FallbackShader;
                mainTextureName = FallbackMainTexture;
            }

            DisplayQuad = new GameObject("DisplayQuad").transform;
            DisplayQuad.gameObject.layer = DisplayLayer.Value;
            DisplayQuad.SetParent(DisplayCamera.transform);
            DisplayQuad.localPosition = Vector3.forward * 2;
            DisplayQuad.localScale = new Vector3(ratio, 1, 1);
            MeshFilter mf = DisplayQuad.gameObject.AddComponent<MeshFilter>();
            mf.mesh = quad;
            MeshRenderer mr = DisplayQuad.gameObject.AddComponent<MeshRenderer>();
            DisplayMaterial = new Material(Shader.Find(shaderName));
            DisplayMaterial.SetTexture(mainTextureName, rt);
            mr.material = DisplayMaterial;

            Initialized = true;
        }

        //render - immediately re-render new texture. Should be false if called from the OnValidate method to avoid errors.
        private void CheckForChanges(bool render)
        {
            ValidateValues();

            if (!enabled)
                return;

            bool changed = false;
            if (filterMode != oldFilterMode || oldHeight != targetHeight || (oldScreenWidth != Screen.width || oldScreenHeight != Screen.height))
            {
                UpdateResolution(render);
                changed = true;
            }

            if (oldFps != fps)
            {
                UpdateFPS();
                changed = true;
            }

            if (changed)
                onSettingsChanged?.Invoke(this);
        }

        private void ValidateValues()
        {
            if (targetHeight < MinHeight)
                targetHeight = MinHeight;
            else if (targetHeight > Screen.height)
                targetHeight = Screen.height;

            if (fps < MinFPS)
                fps = MinFPS;
            else if (fps > MaxFPS)
                fps = MaxFPS;
        }

        private void UpdateResolution(bool render)
        {
            if (!Initialized)
                return;

            oldFilterMode = filterMode;

            float ratio = (float)Screen.width / (float)Screen.height;
            int width = (int)(ratio * targetHeight);
            oldHeight = targetHeight;
            oldScreenWidth = Screen.width;
            oldScreenHeight = Screen.height;

            RenderTexture.active = null;

            //This part fixes the memory leak
            rt.Release();
            Destroy(rt);
            //

            rt = new RenderTexture(width, targetHeight, 24);
            rt.filterMode = filterMode;
            RenderTexture.active = rt;
            RenderCamera.targetTexture = rt;
            DisplayMaterial.SetTexture("_MainTex", rt);
            DisplayQuad.localScale = new Vector3(ratio, 1, 1);

            if (render)
            {
                RenderCamera.Render();
                onFrameRendered?.Invoke(this);
            }
        }

        private void UpdateFPS()
        {
            oldFps = fps;
            Delay = 1f / fps;
        }

        private IEnumerator RenderToTexture()
        {
            while (true)
            {
                if (overrideFPS)
                {
                    yield return new WaitForSecondsRealtime(Delay);
                    RenderCamera.Render();
                    onFrameRendered?.Invoke(this);
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                    RenderCamera.Render();
                }
            }
        }

    }
}
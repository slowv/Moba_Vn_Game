using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityStandardAssets.ImageEffects;

namespace MOBA.Standard_Assets.Effects.ImageEffects.Scripts
{
    public enum AAMode
    {
        Fxaa2 = 0,
        Fxaa3Console = 1,
        Fxaa1PresetA = 2,
        Fxaa1PresetB = 3,
        Nfaa = 4,
        Ssaa = 5,
        Dlaa = 6,
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("Image Effects/Other/Antialiasing")]
    public class Antialiasing : PostEffectsBase
    {
        public AAMode mode = AAMode.Fxaa3Console;

        public bool showGeneratedNormals;
        public float offsetScale = 0.2f;
        public float blurRadius = 18.0f;

        public float edgeThresholdMin = 0.05f;
        public float edgeThreshold = 0.2f;
        public float edgeSharpness = 4.0f;

        public bool dlaaSharp;

        public Shader ssaaShader;
        private Material _ssaa;
        public Shader dlaaShader;
        private Material _dlaa;
        [NonSerialized] private readonly Shader _nfaaShader;
        private Material _nfaa;
        [FormerlySerializedAs("shaderFXAAPreset2")] public Shader shaderFxaaPreset2;
        private Material _materialFxaaPreset2;
        [FormerlySerializedAs("shaderFXAAPreset3")] public Shader shaderFxaaPreset3;
        private Material _materialFxaaPreset3;
        [FormerlySerializedAs("shaderFXAAII")] public Shader shaderFxaaii;
        private Material _materialFxaaii;
        [FormerlySerializedAs("shaderFXAAIII")] public Shader shaderFxaaiii;
        private Material _materialFxaaiii;
        private static readonly int OffsetScale = Shader.PropertyToID("_OffsetScale");
        private static readonly int BlurRadius = Shader.PropertyToID("_BlurRadius");
        private static readonly int EdgeThresholdMin = Shader.PropertyToID("_EdgeThresholdMin");
        private static readonly int EdgeThreshold = Shader.PropertyToID("_EdgeThreshold");
        private static readonly int EdgeSharpness = Shader.PropertyToID("_EdgeSharpness");
        public Antialiasing(Shader nfaaShader)
        {
            _nfaaShader = nfaaShader;
        }


        public Material CurrentAAMaterial()
        {
            Material returnValue = null;

            switch (mode)
            {
                case AAMode.Fxaa3Console:
                    returnValue = _materialFxaaiii;
                    break;
                case AAMode.Fxaa2:
                    returnValue = _materialFxaaii;
                    break;
                case AAMode.Fxaa1PresetA:
                    returnValue = _materialFxaaPreset2;
                    break;
                case AAMode.Fxaa1PresetB:
                    returnValue = _materialFxaaPreset3;
                    break;
                case AAMode.Nfaa:
                    returnValue = _nfaa;
                    break;
                case AAMode.Ssaa:
                    returnValue = _ssaa;
                    break;
                case AAMode.Dlaa:
                    returnValue = _dlaa;
                    break;
            }

            return returnValue;
        }


        public override bool CheckResources()
        {
            CheckSupport(false);

            _materialFxaaPreset2 = CreateMaterial(shaderFxaaPreset2, _materialFxaaPreset2);
            _materialFxaaPreset3 = CreateMaterial(shaderFxaaPreset3, _materialFxaaPreset3);
            _materialFxaaii = CreateMaterial(shaderFxaaii, _materialFxaaii);
            _materialFxaaiii = CreateMaterial(shaderFxaaiii, _materialFxaaiii);
            _nfaa = CreateMaterial(_nfaaShader, _nfaa);
            _ssaa = CreateMaterial(ssaaShader, _ssaa);
            _dlaa = CreateMaterial(dlaaShader, _dlaa);

            if (!ssaaShader.isSupported)
            {
                NotSupported();
                ReportAutoDisable();
            }

            return isSupported;
        }


        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

			// ----------------------------------------------------------------
            // FXAA antialiasing modes

            if (mode == AAMode.Fxaa3Console && (_materialFxaaiii != null))
            {
                _materialFxaaiii.SetFloat(EdgeThresholdMin, edgeThresholdMin);
                _materialFxaaiii.SetFloat(EdgeThreshold, edgeThreshold);
                _materialFxaaiii.SetFloat(EdgeSharpness, edgeSharpness);

                Graphics.Blit(source, destination, _materialFxaaiii);
            }
            else if (mode == AAMode.Fxaa1PresetB && (_materialFxaaPreset3 != null))
            {
                Graphics.Blit(source, destination, _materialFxaaPreset3);
            }
            else if (mode == AAMode.Fxaa1PresetA && _materialFxaaPreset2 != null)
            {
                source.anisoLevel = 4;
                Graphics.Blit(source, destination, _materialFxaaPreset2);
                source.anisoLevel = 0;
            }
            else if (mode == AAMode.Fxaa2 && _materialFxaaii != null)
            {
                Graphics.Blit(source, destination, _materialFxaaii);
            }
            else if (mode == AAMode.Ssaa && _ssaa != null)
            {
				// ----------------------------------------------------------------
                // SSAA antialiasing
                Graphics.Blit(source, destination, _ssaa);
            }
            else if (mode == AAMode.Dlaa && _dlaa != null)
            {
				// ----------------------------------------------------------------
				// DLAA antialiasing

                source.anisoLevel = 0;
                RenderTexture interim = RenderTexture.GetTemporary(source.width, source.height);
                Graphics.Blit(source, interim, _dlaa, 0);
                Graphics.Blit(interim, destination, _dlaa, dlaaSharp ? 2 : 1);
                RenderTexture.ReleaseTemporary(interim);
            }
            else if (mode == AAMode.Nfaa && _nfaa != null)
            {
                // ----------------------------------------------------------------
                // nfaa antialiasing

                source.anisoLevel = 0;

                _nfaa.SetFloat(OffsetScale, offsetScale);
                _nfaa.SetFloat(BlurRadius, blurRadius);

                Graphics.Blit(source, destination, _nfaa, showGeneratedNormals ? 1 : 0);
            }
            else
            {
                // none of the AA is supported, fallback to a simple blit
                Graphics.Blit(source, destination);
            }
        }
    }
}

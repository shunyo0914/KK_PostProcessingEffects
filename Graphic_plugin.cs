using System;
using System.Linq;
using BepInEx;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using BepInEx.Configuration;

namespace PostEffectController
{
    [BepInPlugin("org.bepinex.plugins.postprocessingeffects", "PostProcessingEffects", "1.0.0.0")]
    public class PostProcessing : BaseUnityPlugin
    {
        public PostProcessingProfile profile;

        private ConfigEntry<bool> AAenable { get; set; }
        private ConfigEntry<AntialiasingModel.Method> AAMethod { get; set; }
        private ConfigEntry<AntialiasingModel.FxaaPreset> FXAAPreset { get; set; }
        private ConfigEntry<float> TAAjittetSpeed { get; set; }
        private ConfigEntry<float> TAAsharpen { get; set; }
        private ConfigEntry<float> TAAstationaryBlending { get; set; }
        private ConfigEntry<float> TAAmotionBlending { get; set; }

        private ConfigEntry<bool> AOenable { get; set; }
        private ConfigEntry<float> AOintensity { get; set; }
        private ConfigEntry<bool> AOdownsampling { get; set; }
        private ConfigEntry<bool> AOforceForwardCompatibility { get; set; }
        private ConfigEntry<AmbientOcclusionModel.SampleCount> AOsampleCount { get; set; }
        private ConfigEntry<bool> AOhighPrecision { get; set; }
        private ConfigEntry<bool> AOambientOnly { get; set; }
        private ConfigEntry<float> AOradius { get; set; }

        private ConfigEntry<bool> SSRenable { get; set; }
        private ConfigEntry<ScreenSpaceReflectionModel.SSRReflectionBlendType> SSRrefBlendtype { get; set; }
        private ConfigEntry<ScreenSpaceReflectionModel.SSRResolution> SSRrefQuality { get; set; }
        private ConfigEntry<float> SSRmaxDistance { get; set; }
        private ConfigEntry<int> SSRiterationCount { get; set; }
        private ConfigEntry<int> SSRstepSize { get; set; }
        private ConfigEntry<float> SSRwidthModifier { get; set; }
        private ConfigEntry<float> SSRrefBlur { get; set; }
        private ConfigEntry<bool> SSRrefBackfaces { get; set; }
        private ConfigEntry<float> SSRrefMultiplier { get; set; }
        private ConfigEntry<float> SSRfadeDistance { get; set; }
        private ConfigEntry<float> SSRfresnelFade { get; set; }
        private ConfigEntry<float> SSRfresnelFadePower { get; set; }
        private ConfigEntry<float> SSRscreenEdgemaskIntensity { get; set; }

        private ConfigEntry<bool> Bloomenable { get; set; }
        private ConfigEntry<float> Bloomintensity { get; set; }
        private ConfigEntry<float> Bloomradius { get; set; }
        private ConfigEntry<float> BloomsoftKnee { get; set; }
        private ConfigEntry<float> Bloomthreshold { get; set; }
        private ConfigEntry<bool> BloomAntiFk { get; set; }


        void Awake()
        {
            AAenable = Config.Bind("AntiAliasingSettings", "AntiAliasing Enable", true, "");
            AAMethod = Config.Bind("AntiAliasingSettings", "AntiAliasing Method", AntialiasingModel.Method.Taa, "FXAA or TAA");
            FXAAPreset = Config.Bind("AntiAliasingSettings", "FXAA Preset", AntialiasingModel.FxaaPreset.Quality, "");
            TAAjittetSpeed = Config.Bind("AntiAliasingSettings", "TAA JitterSpeed", 0.75f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 1f)));
            TAAsharpen = Config.Bind("AntiAliasingSettings", "TAA Sharpen", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 3f)));
            TAAstationaryBlending = Config.Bind("AntiAliasingSettings", "TAA StationaryBlending", 0.95f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 0.99f)));
            TAAmotionBlending = Config.Bind("AntiAliasingSettings", "TAA MotionBlending", 0.85f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 0.99f)));

            AOenable = Config.Bind("Ambient Occulusion", "Ambient Occulusion Enable", true, "");
            AOintensity = Config.Bind("Ambient Occulusion", "Ambient Occulusion Intensity", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 4f)));
            AOdownsampling = Config.Bind("Ambient Occulusion", "Ambient Occulusion Downsampling", true, "");
            AOforceForwardCompatibility = Config.Bind("Ambient Occulusion", "Ambient Occulusion ForwardCompatibility", false, "");
            AOsampleCount = Config.Bind("Ambient Occulusion", "Ambient Occulusion SampleCount", AmbientOcclusionModel.SampleCount.Medium, "");
            AOhighPrecision = Config.Bind("Ambient Occulusion", "Ambient Occulusion HighPrecision", false, "");
            AOradius = Config.Bind("Ambient Occulusion", "Ambient Occulusion Radius", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0.0001f, 10f)));
            AOambientOnly = Config.Bind("Ambient Occulusion", "Ambient Occulusion OnlyMode", false, "");

            SSRenable = Config.Bind("ScreenSpaceReflection", "SSR Enable", true, "");
            SSRrefBlendtype = Config.Bind("ScreenSpaceReflection", "SSR Reflection BlendType", ScreenSpaceReflectionModel.SSRReflectionBlendType.PhysicallyBased, "");
            SSRrefQuality = Config.Bind("ScreenSpaceReflection", "SSR Reflection Quality", ScreenSpaceReflectionModel.SSRResolution.Low, "");
            SSRmaxDistance = Config.Bind("ScreenSpaceReflection", "SSR Reflection MaxDistance",100f , new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 300f)));
            SSRiterationCount = Config.Bind("ScreenSpaceReflection", "SSR Reflection iterationCount", 256, new ConfigDescription("", new AcceptableValueRange<int>(16, 1024)));
            SSRstepSize = Config.Bind("ScreenSpaceReflection", "SSR Reflection StepSize", 3, new ConfigDescription("", new AcceptableValueRange<int>(1, 16)));
            SSRwidthModifier = Config.Bind("ScreenSpaceReflection", "SSR Reflection WidthModifier", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 10f)));
            SSRrefBlur = Config.Bind("ScreenSpaceReflection", "SSR Reflection Blur", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 8f)));
            SSRrefBackfaces = Config.Bind("ScreenSpaceReflection", "SSR Reflection Backfaces", false, "");
            SSRrefMultiplier = Config.Bind("ScreenSpaceReflection", "SSR Reflection Multiplier", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 2f)));
            SSRfadeDistance = Config.Bind("ScreenSpaceReflection", "SSR FadeDistance", 100f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1000f)));
            SSRfresnelFade = Config.Bind("ScreenSpaceReflection", "SSR FresnelFade", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            SSRfresnelFadePower = Config.Bind("ScreenSpaceReflection", "SSR FresnelFadePower", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10f)));
            SSRscreenEdgemaskIntensity = Config.Bind("ScreenSpaceReflection", "SSR ScreenEdgeMask Intensity", 0.03f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));

            Bloomenable = Config.Bind("Bloom", "Bloom Enable", true, "");
            Bloomintensity = Config.Bind("Bloom", "Bloom Intensity", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 10f)));
            Bloomradius = Config.Bind("Bloom", "Bloom Radius", 4f, new ConfigDescription("", new AcceptableValueRange<float>(1f, 7f)));
            BloomsoftKnee = Config.Bind("Bloom", "Bloom Softknee", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            Bloomthreshold = Config.Bind("Bloom", "Bloom Threshold", 1.1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 10f)));
            BloomAntiFk = Config.Bind("Bloom", "Bloom AntiFlicker", true, "Reduces flashing noise with an additional filter.");

            Config.SettingChanged += OnSettingChanged;
        }

        protected void OnSettingChanged(object sender,SettingChangedEventArgs e)
        { 
            Settings();
            //Debug.Log("Setting Changed");
        }

        void Start()
        {
            SceneManager.sceneLoaded += SceneLoaded;
        }


        void SceneLoaded(Scene Scene, LoadSceneMode mode)
        {
            //Debug.Log(Scene.name);
            //Debug.Log(mode);

            Camera camera = Camera.main;
            profile = ScriptableObject.CreateInstance<PostProcessingProfile>();
            if (!camera)
            {
                Debug.LogError("Failed to get reference to MainCamera; mod startup aborted.");
                return;
            }
            Settings();
            PostProcessingBehaviour post = camera.GetOrAddComponent<PostProcessingBehaviour>();
            rendermode2();
            post.profile = profile;
            Debug.Log("Effects Loaded");
        }

        public void Settings()
        {
            profile.antialiasing.enabled = AAenable.Value;
            AntialiasingModel.Settings AAsettings = profile.antialiasing.settings;
            AAsettings.method = AAMethod.Value;//AntialiasingModel.Method.Taa;
            AAsettings.fxaaSettings.preset = FXAAPreset.Value;//AntialiasingModel.FxaaPreset.ExtremePerformance;
            AAsettings.taaSettings.jitterSpread = TAAjittetSpeed.Value;
            AAsettings.taaSettings.sharpen = TAAsharpen.Value;
            AAsettings.taaSettings.stationaryBlending = TAAstationaryBlending.Value;
            AAsettings.taaSettings.motionBlending = TAAmotionBlending.Value;
            profile.antialiasing.settings = AAsettings;

            profile.ambientOcclusion.enabled = AOenable.Value;
            AmbientOcclusionModel.Settings AOsettings = profile.ambientOcclusion.settings;
            AOsettings.intensity = AOintensity.Value;
            AOsettings.downsampling = AOdownsampling.Value;
            AOsettings.forceForwardCompatibility = AOforceForwardCompatibility.Value;
            AOsettings.sampleCount = AOsampleCount.Value;//AmbientOcclusionModel.SampleCount.High;
            AOsettings.highPrecision = AOhighPrecision.Value;
            AOsettings.ambientOnly = AOambientOnly.Value;
            AOsettings.radius = AOradius.Value;
            profile.ambientOcclusion.settings = AOsettings;

            profile.screenSpaceReflection.enabled = SSRenable.Value;
            ScreenSpaceReflectionModel.Settings SSRsettings = profile.screenSpaceReflection.settings;
            SSRsettings.intensity.reflectionMultiplier = SSRrefMultiplier.Value;
            SSRsettings.intensity.fadeDistance = SSRfadeDistance.Value;
            SSRsettings.intensity.fresnelFade = SSRfresnelFade.Value;
            SSRsettings.intensity.fresnelFadePower = SSRfresnelFadePower.Value;
            SSRsettings.reflection.blendType = SSRrefBlendtype.Value;
            SSRsettings.reflection.reflectionQuality = SSRrefQuality.Value;
            SSRsettings.reflection.maxDistance = SSRmaxDistance.Value;
            SSRsettings.reflection.iterationCount = SSRiterationCount.Value;
            SSRsettings.reflection.stepSize = SSRstepSize.Value;
            SSRsettings.reflection.widthModifier = SSRwidthModifier.Value;
            SSRsettings.reflection.reflectionBlur = SSRrefBlur.Value;
            SSRsettings.reflection.reflectBackfaces = SSRrefBackfaces.Value;
            SSRsettings.screenEdgeMask.intensity = SSRscreenEdgemaskIntensity.Value;
            profile.screenSpaceReflection.settings = SSRsettings;

            profile.bloom.enabled = Bloomenable.Value;
            BloomModel.Settings bloomsetting = profile.bloom.settings;
            bloomsetting.bloom.intensity = Bloomintensity.Value;
            bloomsetting.bloom.radius = Bloomradius.Value;
            bloomsetting.bloom.softKnee = BloomsoftKnee.Value;
            bloomsetting.bloom.threshold = Bloomthreshold.Value;
            bloomsetting.bloom.antiFlicker = BloomAntiFk.Value;
            profile.bloom.settings = bloomsetting;
            //Debug.LogError("testtesttest");
        }

        void rendermode2()
        {
            GameObject root = GameObject.Find("CommonSpace");
            //Debug.Log(root);
            GameObject[] children = root.GetComponentsInChildren<Transform> ().Select(t => t.gameObject).ToArray();
            //Debug.Log(children);
            foreach (GameObject go in children)
            {
                //Debug.Log(go);
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                foreach (Renderer render in renderers)
                {
                    Material[] materials = render.materials;
                    foreach (Material material in materials)
                    {
                        if ("TransparentCutout" == material.GetTag("RenderType", false))
                        {
                            material.SetOverrideTag("RenderType", "Opaque");
                            //Debug.Log(go + ":RenderType Changed");
                        }
                    }
                }
            }
        }
    }
}




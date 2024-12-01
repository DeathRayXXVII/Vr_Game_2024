using System.Collections;
using UnityEngine;

namespace ZPTools
{
    public class VRScreenManager : ScreenManager
    {
        private Renderer _renderer;
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
        private static readonly int ZWriteId = Shader.PropertyToID("_ZWriteMode");
        
        [SerializeField] private Color _baseColor;
        
        [SerializeField] private bool _transitionFromPrimaryToSecondaryColor;
        public bool transitionFromPrimaryToSecondaryColor 
        { 
            get => _transitionFromPrimaryToSecondaryColor; 
            set => _transitionFromPrimaryToSecondaryColor = value; 
        }
        
        [SerializeField] private Color _transitionToColor;
        
        public void SetPrimaryColor(RGBAColorData color) => _baseColor = color;
        public void SetSecondaryColor(RGBAColorData color) => _transitionToColor = color;
        
        public bool fadeToBlackAfterTransition = false;
        
        private float _alphaIn;
        private float _alphaOut;
        
        protected override void Initialize() => EnsureRendererExists();
        
        private void OnValidate()
        {
            if (!EnsureRendererExists()) return;

            UpdateMaterial(_baseColor);
        }

        private bool EnsureRendererExists()
        {
            if (_renderer == null)
            {
                _renderer = GetComponent<Renderer>();
            }

            if (_renderer != null) return true;
            
            Debug.LogWarning($"Renderer not found on {name}", this);
            return false;
        }

        private void UpdateMaterial(Color color)
        {
            var targetMaterial = GetMaterial();

            if (targetMaterial == null)
            {
                Debug.LogWarning("Material not found on renderer", this);
            }

            targetMaterial?.SetColor(ColorPropertyId, color);
            targetMaterial?.SetInt(ZWriteId, color.a < 1 ? 0 : 1);
        }

        private Material GetMaterial()
        {
            // Use sharedMaterial in edit mode, material in play mode
            if (Application.isPlaying)
            {
                return _renderer.material; // Unique material for runtime
            }

#if UNITY_EDITOR
            return _renderer.sharedMaterial; // Shared material for editor
#else
            return null;
#endif
        }

        public override IEnumerator TransitionIn()
        {
            if (isTransitioning && _fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                StopCoroutine(TransitionCoroutine);
                _fadeCoroutine = TransitionCoroutine = null;
            }
            
            // Transition will fade from 1 to 0 (opaque to transparent) on an exponential growth curve
            transitionType = (int)TransitionType.In;
            yield return _fadeCoroutine ??= StartCoroutine(Fade(1, 0));
            yield return WaitFixed; 
        }

        public override IEnumerator TransitionOut()
        {
            if (isTransitioning && _fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                StopCoroutine(TransitionCoroutine);
                _fadeCoroutine = TransitionCoroutine = null;
            }
            
            // Transition will fade from 0 to 1 (transparent to opaque) on an exponential decay curve
            transitionType = (int)TransitionType.Out;
            yield return  _fadeCoroutine ??= StartCoroutine(Fade(0, 1));
            yield return WaitFixed; 
        }
        
        private Coroutine _fadeCoroutine;
        public IEnumerator Fade(float alphaIn, float alphaOut)
        {
            if (TransitionCoroutine != null)
            {
                yield break;
            }

            _alphaIn = alphaIn;
            _alphaOut = alphaOut;
            
            TransitionCoroutine ??= StartCoroutine(ExecuteTransition());
            yield return new WaitUntil(() => TransitionCoroutine == null);
        }

        protected override IEnumerator ExecuteTransition()
        {
            var fadeColor = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _alphaIn);
            var startColor = fadeColor;
            _transitionToColor = transitionFromPrimaryToSecondaryColor ? _transitionToColor : _baseColor;
            var endColor = new Color(_transitionToColor.r, _transitionToColor.g, _transitionToColor.b, _alphaOut);
            
            yield return HandleColorTransition(startColor, endColor);
            yield return WaitFixed; 
            
            yield return fadeToBlackAfterTransition ? HandleColorTransition(endColor, Color.black) : null;
            yield return WaitFixed; 
            
            // Debug.LogWarning($"Transition completed at game time: {Time.time}", this);
            TransitionCoroutine = null;
        }

        private static float LogarithmicLerp(float start, float end, float normalizedTime, float exponentialFactor, float timeScale = 1f)
        {
            // Clamp normalized time to [0, 1]
            normalizedTime = Mathf.Clamp01(normalizedTime);

            // Scale normalized time to adjust transition behavior
            float scaledTime = Mathf.Pow(normalizedTime, timeScale);

            // Calculate the exponential term
            float exponentialTerm;
            if (exponentialFactor > 0)
            {
                // Growth
                exponentialTerm = 1 - Mathf.Exp(-exponentialFactor * scaledTime);
            }
            else
            {
                // Decay
                exponentialTerm = Mathf.Exp(exponentialFactor * scaledTime) - 1;
            }

            // Compute and return the interpolated value
            return start + (end - start) * exponentialTerm;
        }
        
        private IEnumerator HandleColorTransition(Color startColor, Color endColor)
        {
#if UNITY_EDITOR
            if (allowDebug)
            {
                Debug.Log($"[DEBUG] Starting color transition at {Time.time}. Start Color: {startColor}, End Color: {endColor}", this);
            }
#endif
            var startTime = Time.time;
            var elapsedTime = 0f;
            var lerpTime = fadeToBlackAfterTransition ? transitionDuration * 0.6f : transitionDuration;


            // Determine exponential factor based on transition type (In: positive -> growth, Out: negative -> decay)
            const float timeScalar = 2f;
            const float exponentialBaseFactor = 3f;
            var exponentialFactor = transitionType == (int)TransitionType.In 
                ? exponentialBaseFactor 
                : -exponentialBaseFactor;

            if (allowDebug)
            {
                Debug.LogWarning($"[DEBUG] Exponential factor: {exponentialFactor} Performing Exponential " +
                                 $"{(exponentialFactor <= 0 ? "Decay" : "Growth")}", this);
            }

#if UNITY_EDITOR
            var debugSpacer = 0;
            const int debugMod = 20;
        #endif

            while (elapsedTime <= transitionDuration)
            {
                // Normalize elapsed time to a range [0, 1] for interpolation.
                // normalizedTime = elapsedTime (current progress) / lerpTime (total transition duration)
                var normalizedTime = elapsedTime / lerpTime;
                
                float r = LogarithmicLerp(startColor.r, endColor.r, normalizedTime, exponentialFactor, timeScalar);
                float g = LogarithmicLerp(startColor.g, endColor.g, normalizedTime, exponentialFactor, timeScalar);
                float b = LogarithmicLerp(startColor.b, endColor.b, normalizedTime, exponentialFactor, timeScalar);
                float a = LogarithmicLerp(startColor.a, endColor.a, normalizedTime, exponentialFactor, timeScalar);


                // Compute interpolated color using logarithmic lerp for each channel
                Color interpolatedColor = new(r, g, b, a);
                
#if UNITY_EDITOR
                if (allowDebug && debugSpacer++ % debugMod == 0)
                {
                    Debug.Log($"[DEBUG] Time: {Time.time}, Elapsed: {elapsedTime} / {transitionDuration}, " +
                              $"Normalized: {normalizedTime}, Color Value: {interpolatedColor}", this);
                }
#endif
                // Apply the interpolated color to the material
                UpdateMaterial(interpolatedColor);

                // Wait for the next frame
                yield return null;

                // Update elapsed time
                elapsedTime = Time.time - startTime;
            }

            // Ensure the material ends with the exact target color
            UpdateMaterial(endColor);
            yield return WaitFixed;

#if UNITY_EDITOR
            if (allowDebug)
            {
                Debug.Log($"[DEBUG] Color transition completed at {Time.time}. Total Time: {elapsedTime}, Final Color: {endColor}", this);
            }
#endif
        }
    }
}

Shader "Custom/ScreenDissolve"
{
    Properties
    {
        _MainTex ("Screen Texture", 2D) = "white" {} // This is set by the render feature.
        _NoiseTex ("Noise Texture", 2D) = "white" {}  
        _OverlayTex ("Overlay Texture", 2D) = "white" {}  
        _Progress ("Dissolve Progress", Range(0,1)) = 0  
        _NoiseStrength ("Noise Offset", Range(0,0.1)) = 0.05  
        _WiggleSpeed ("Wiggle Speed", Range(0,10)) = 2  
        _DissolveEdge ("Dissolve Edge", Range(0,0.5)) = 0.1  
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex);
                OUT.uv = IN.uv;
                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            sampler2D _OverlayTex;
            float _Progress;
            float _NoiseStrength;
            float _WiggleSpeed;
            float _DissolveEdge;

            half4 frag (Varyings IN) : SV_Target
            {
                half4 screenCol = tex2D(_MainTex, IN.uv);
                half4 overlayCol = tex2D(_OverlayTex, IN.uv);

                float timeVal = _Time.y * _WiggleSpeed;
                float2 wiggleOffset = float2(
                    sin((IN.uv.y + timeVal) * 10.0),
                    cos((IN.uv.x - timeVal) * 10.0)
                ) * _NoiseStrength;

                float2 noiseUV = IN.uv + wiggleOffset;
                float noiseVal = tex2D(_NoiseTex, noiseUV).r;

                float mask = smoothstep(_Progress - _DissolveEdge, _Progress + _DissolveEdge, noiseVal);
                half4 finalCol = lerp(screenCol, overlayCol, mask);
                return finalCol;
            }
            ENDHLSL
        }
    }
    FallBack Off
}

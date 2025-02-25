Shader "Custom/BubbleWiggle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WiggleAmount ("Wiggle Amount", Range(0, 0.1)) = 0.05
        _WiggleSpeed ("Wiggle Speed", Range(0, 10)) = 2
        _WiggleFreq ("Wiggle Frequency", Range(1, 20)) = 10
        _WrinkleAmount ("Wrinkle Amount", Range(0, 0.1)) = 0.02
        _PhaseOffset ("Phase Offset Factor", Range(0,10)) = 5
        _ImpactPoint ("Impact Point", Vector) = (0,0,0,0)
        _ImpactStrength ("Impact Strength", Range(0, 0.2)) = 0.1
        _ImpactWiggleAmount ("Impact Wiggle Amount", Range(0, 10)) = 0.0
        _ImpactRadius ("Impact Radius", Range(0, 100)) = 100.0  // New property!
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _WiggleAmount;
            float _WiggleSpeed;
            float _WiggleFreq;
            float _WrinkleAmount;
            float _PhaseOffset;
            float4 _ImpactPoint;
            float _ImpactStrength;
            float _ImpactWiggleAmount;
            float _ImpactRadius; // New property

            // Basic noise function.
            float myNoise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Smooth noise with bilinear interpolation.
            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);  // smoothstep interpolation.
                float a = myNoise(i);
                float b = myNoise(i + float2(1.0, 0.0));
                float c = myNoise(i + float2(0.0, 1.0));
                float d = myNoise(i + float2(1.0, 1.0));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                // Convert the vertex position into object space.
                float3 localPos = mul(unity_WorldToObject, v.vertex).xyz;
                float2 pos = localPos.xy;
                float r = length(pos);
                float angle = atan2(pos.y, pos.x);

                // Time factor for animation.
                float t = _Time.y * _WiggleSpeed;
                // Add a per-vertex random phase offset based on the angle.
                float randomPhase = smoothNoise(float2(angle, 0.0)) * _PhaseOffset;

                // Compute base noise terms.
                float lowNoise = smoothNoise(float2(angle * _WiggleFreq, t + randomPhase)) - 0.5;
                float highNoise = smoothNoise(float2(angle * _WiggleFreq * 3.0, t + randomPhase * 1.5)) - 0.5;
                float displacement = lowNoise * _WiggleAmount + highNoise * _WrinkleAmount;

                // Impact deformation: push vertices inward near the impact point.
                float2 impactDir = pos - _ImpactPoint.xy;
                float impactDist = length(impactDir);
                if(impactDist < 0.5) // Adjust the radius as needed.
                {
                    float impactEffect = (0.5 - impactDist) * _ImpactStrength;
                    displacement -= impactEffect;

                    // Extra localized wiggle near the impact.
                    float impactFactor = (_ImpactRadius - impactDist) / _ImpactRadius;
                    float extraWiggle = (smoothNoise(impactDir * 10.0 + float2(t, t)) - 0.5) * _ImpactWiggleAmount;
                    displacement += extraWiggle * impactFactor;
                }

                float rNew = r + displacement;
                float2 newPos = float2(cos(angle) * rNew, sin(angle) * rNew);
                localPos.xy = newPos;
                v.vertex = float4(localPos, 1.0);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}

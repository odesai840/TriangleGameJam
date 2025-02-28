Shader "Custom/BubbleWiggle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _WiggleAmount ("Edge Wiggle Amount", Range(0, 0.2)) = 0.00
        _WiggleSpeed ("Edge Wiggle Speed", Range(0, 10)) = 2
        _WiggleFreq ("Edge Wiggle Frequency", Range(1, 20)) = 10

        _WrinkleAmount ("Edge Wrinkle Amount", Range(0, 0.2)) = 0.05
        _PhaseOffset ("Phase Offset Factor", Range(0,10)) = 5

        _ImpactPoint ("Impact Point", Vector) = (0,0,0,0)
        _ImpactStrength ("Impact Strength", Range(0, 0.2)) = 0.1
        _ImpactWiggleAmount ("Impact Wiggle Amount", Range(0, 10)) = 0.0
        _ImpactRadius ("Impact Radius", Range(0, 2)) = 2

        // Interior wiggle props
        _InteriorWiggleAmount ("Interior Wiggle Amplitude", Range(0, 0.2)) = 0.1
        _InteriorWiggleSpeed ("Interior Wiggle Speed", Range(0, 5)) = 1
        _InteriorWiggleFreq  ("Interior Wiggle Frequency", Range(0.1, 10)) = 3

        // Unique seed per bubble instance
        _BubbleSeed ("Bubble Random Seed", Float) = 0

        // NEW: Darken Amount
        _DarkenAmount ("Darken Amount", Range(0, 1)) = 0.0
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

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            // Edge Wiggle / Wrinkle Props
            float _WiggleAmount;
            float _WiggleSpeed;
            float _WiggleFreq;
            float _WrinkleAmount;
            float _PhaseOffset;

            // Impact Props
            float4 _ImpactPoint;
            float  _ImpactStrength;
            float  _ImpactWiggleAmount;
            float  _ImpactRadius;

            // Interior Wiggle
            float _InteriorWiggleAmount;
            float _InteriorWiggleSpeed;
            float _InteriorWiggleFreq;

            // Unique seed
            float _BubbleSeed;

            // Darken
            float _DarkenAmount;

            // Basic noise function
            float myNoise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Smooth noise with bilinear interpolation
            float smoothNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                // Smoothstep interpolation
                f = f * f * (3.0 - 2.0 * f);

                float a = myNoise(i);
                float b = myNoise(i + float2(1.0, 0.0));
                float c = myNoise(i + float2(0.0, 1.0));
                float d = myNoise(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert (appdata_t v)
            {
                v2f o;

                float3 localPos = v.vertex.xyz;
                float2 pos = localPos.xy;
                float r = length(pos);

                // Keep center stable if extremely close to zero
                if (r < 0.001)
                {
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                //------------------------------------
                // 1) Edge Wiggle: radial displacement
                //------------------------------------
                float angle = atan2(pos.y, pos.x);
                float tEdge = _Time.y * _WiggleSpeed;

                // Combine angle + bubble seed for a unique offset per bubble
                float randomPhase = smoothNoise(float2(angle, _BubbleSeed)) * _PhaseOffset;

                float lowNoise  = smoothNoise(float2(angle * _WiggleFreq, tEdge + randomPhase)) - 0.5;
                float highNoise = smoothNoise(float2(angle * _WiggleFreq * 3.0, tEdge + randomPhase * 1.5)) - 0.5;
                float displacement = lowNoise * _WiggleAmount + highNoise * _WrinkleAmount;

                //------------------------------------
                // 2) Impact Deformation
                //------------------------------------
                float2 impactDir = pos - _ImpactPoint.xy;
                float impactDist = length(impactDir);
                if (impactDist < _ImpactRadius)
                {
                    float impactEffect = (_ImpactRadius - impactDist) * _ImpactStrength;
                    displacement -= impactEffect;

                    float impactFactor = (_ImpactRadius - impactDist) / _ImpactRadius;
                    float extraWiggle = (smoothNoise(impactDir * 10.0 + float2(tEdge, tEdge)) - 0.5) * _ImpactWiggleAmount;
                    displacement += extraWiggle * impactFactor;
                }

                float rNew = r + displacement;
                float2 newPos = float2(cos(angle) * rNew, sin(angle) * rNew);
                localPos.xy = newPos;

                //------------------------------------
                // 3) Interior Wiggle: slower, bigger in-plane offset
                //------------------------------------
                float tInterior = _Time.y * _InteriorWiggleSpeed;

                // We'll incorporate _BubbleSeed in the noise calls
                float noiseX = smoothNoise(float2(pos.x * _InteriorWiggleFreq + tInterior + _BubbleSeed * 13.0,
                                                  pos.y + _BubbleSeed));
                float noiseY = smoothNoise(float2(pos.x + _BubbleSeed,
                                                  pos.y * _InteriorWiggleFreq + tInterior + _BubbleSeed * 7.0));

                // Shift [-0.5 .. 0.5]
                noiseX -= 0.5;
                noiseY -= 0.5;

                // Apply amplitude
                localPos.x += noiseX * _InteriorWiggleAmount;
                localPos.y += noiseY * _InteriorWiggleAmount;

                //------------------------------------
                // Final
                //------------------------------------
                v.vertex = float4(localPos, 1.0);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);

                // Darken the color by _DarkenAmount
                // e.g. if _DarkenAmount=0.3, we reduce the brightness by 30%
                // c.rgb = c.rgb * (1 - _DarkenAmount);
                // Or you can do a direct lerp to black:
                c.rgb = lerp(c.rgb, 0, _DarkenAmount);

                return c;
            }

            ENDCG
        }
    }
}

Shader "Custom/RouletteWheel"
{
    Properties
    {
        _MainTex       ("(не используется, для совместимости с RawImage)", 2D) = "white" {}
        _DigitTex      ("Digit Texture (0-9 strip)", 2D) = "white" {}
        _SectorCount   ("Sector Count", Int)     = 17
        _Aspect        ("Aspect (W/H)", Float)   = 1.0
        _OuterRadius   ("Outer Radius", Float)   = 0.5
        _InnerRadius   ("Inner Radius", Float)   = 0.05
        _DividerWidth  ("Divider Width", Float)  = 0.006
        _LabelRadius   ("Label Radius", Float)   = 0.32
        _LabelHalfSize ("Label Half Size", Float)= 0.05
        _LabelColor    ("Label Color", Color)    = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _DigitTex;
            int    _SectorCount;
            float  _Aspect;
            float  _OuterRadius;
            float  _InnerRadius;
            float  _DividerWidth;
            float  _LabelRadius;
            float  _LabelHalfSize;
            fixed4 _LabelColor;

            // До 100 секторов
            float4 _SectorColors[100];
            float  _QuestionCounts[100];

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            // Сэмплирует цифру d (0-9) из горизонтального стрипа
            fixed4 SampleDigit(int d, float2 uv)
            {
                float u = (clamp(d, 0, 9) + uv.x) / 10.0;
                return tex2D(_DigitTex, float2(u, uv.y));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 coord = i.uv - 0.5;          // центрированные UV: -0.5..0.5
                // Коррекция аспекта — чтобы круг не стал эллипсом
                float2 corrected = float2(coord.x * _Aspect, coord.y);
                float  r         = length(corrected);

                if (r > _OuterRadius || r < _InnerRadius)
                    return fixed4(0, 0, 0, 0);

                const float TWO_PI = 6.28318530718;
                int sc = max(_SectorCount, 1);
                float sectorStep = TWO_PI / sc;

                // Угол: 0 сверху, по часовой стрелке, 0..2π (считаем от скорректированных координат)
                float angle = fmod(atan2(corrected.x, corrected.y) + TWO_PI, TWO_PI);
                int si = clamp((int)(angle / sectorStep), 0, sc - 1);

                // Разделители секторов
                float localAngle = fmod(angle, sectorStep);
                if (min(localAngle, sectorStep - localAngle) * r < _DividerWidth)
                    return fixed4(0.05, 0.05, 0.05, 1);

                fixed4 col = _SectorColors[si];

                // Рендер цифры в центре сектора
                float centerAngle = (si + 0.5) * sectorStep;
                // Позиция центра лейбла в corrected-пространстве → обратно в UV
                float2 lblCenter  = float2(sin(centerAngle) / _Aspect, cos(centerAngle)) * _LabelRadius;
                float2 rel        = coord - lblCenter;
                float2 relC       = float2(rel.x * _Aspect, rel.y);

                // Поворачиваем в локальный фрейм сектора (цифра всегда прямо)
                float ca = cos(-centerAngle), sa = sin(-centerAngle);
                float2 local = float2(relC.x * ca - relC.y * sa,
                                      relC.x * sa + relC.y * ca);

                float hs = _LabelHalfSize;
                if (abs(local.x) < hs && abs(local.y) < hs)
                {
                    int cnt = clamp((int)_QuestionCounts[si], 0, 99);
                    // UV внутри ячейки цифры
                    float2 cellUV = float2(
                        local.x / (hs * 2.0) + 0.5,
                        1.0 - (local.y / (hs * 2.0) + 0.5)
                    );

                    fixed4 digitPx;
                    if (cnt >= 10)
                    {
                        // Две цифры: левая половина — десятки, правая — единицы
                        if (cellUV.x < 0.5)
                            digitPx = SampleDigit(cnt / 10,  float2(cellUV.x * 2.0, cellUV.y));
                        else
                            digitPx = SampleDigit(cnt % 10, float2((cellUV.x - 0.5) * 2.0, cellUV.y));
                    }
                    else
                    {
                        digitPx = SampleDigit(cnt, cellUV);
                    }

                    // Блендим цифру поверх цвета сектора
                    col = lerp(col, _LabelColor, digitPx.a);
                }

                return col;
            }
            ENDCG
        }
    }
}

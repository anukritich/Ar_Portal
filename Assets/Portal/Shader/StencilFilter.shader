Shader "Custom/StencilFilter"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        [Enum(Equal,3,NotEqual,6)] _StencilTest ("Stencil Test", int) = 3
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" } // Render normally

        Stencil
        {
            Ref 1
            Comp [_StencilTest] // Uses the stencil value from the mask
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 _Color;

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color; // Render objects inside portal
            }
            ENDCG
        }
    }
}

/*
MIT License

Copyright(c) 2017 Justin

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions :

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
© 2019 GitHub, Inc.
*/

Shader "Custom/WireframeTransparent"
{
    SubShader
    {
        Pass
        {
            Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 200
            ZWrite On
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma target 5.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
        
            struct v2g
            {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
            };

            struct g2f
            {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
                float3 dist : TEXCOORD1;
            };

            v2g vert(appdata_base v)
            {
                v2g OUT;
                OUT.pos = UnityObjectToClipPos(v.vertex);
                OUT.uv = v.texcoord;
                return OUT;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                float2 WIN_SCALE = float2(_ScreenParams.x / 2.0, _ScreenParams.y / 2.0);

                //frag position
                float2 p0 = WIN_SCALE * IN[0].pos.xy / IN[0].pos.w;
                float2 p1 = WIN_SCALE * IN[1].pos.xy / IN[1].pos.w;
                float2 p2 = WIN_SCALE * IN[2].pos.xy / IN[2].pos.w;

                //barycentric position
                float2 v0 = p2 - p1;
                float2 v1 = p2 - p0;
                float2 v2 = p1 - p0;
                //triangles area
                float area = abs(v1.x*v2.y - v1.y * v2.x) * 3;

                g2f OUT;
                OUT.pos = IN[0].pos;
                OUT.uv = IN[0].uv;
                OUT.dist = float3(area / length(v0),0,0);
                triStream.Append(OUT);

                OUT.pos = IN[1].pos;
                OUT.uv = IN[1].uv;
                OUT.dist = float3(0,area / length(v1),0);
                triStream.Append(OUT);

                OUT.pos = IN[2].pos;
                OUT.uv = IN[2].uv;
                OUT.dist = float3(0,0,area / length(v2));
                triStream.Append(OUT);

            }

            half4 frag(g2f IN) : COLOR
            {
                //distance of frag from triangles center
                float d = min(IN.dist.x, min(IN.dist.y, IN.dist.z));
                //fade based on dist from center
                float e = exp2(d*-.2);
                 
                half4 result = half4(
                    0,
                    e,
                    e,
                    e + .2
                    );

                return result;
            }

            ENDCG

        }
    }
}
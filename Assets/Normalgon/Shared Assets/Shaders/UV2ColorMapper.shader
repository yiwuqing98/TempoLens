Shader "Custom/UV2ColorMapper"
{
    Properties
    {
        [Header(Bottom 4 Rows)]
        [HideInInspector] _Color0_0 ("", Color) = (1, 0, 0, 1)
        [HideInInspector] _Color0_1 ("", Color) = (1, 0.5, 0, 1)
        [HideInInspector] _Color0_2 ("", Color) = (1, 1, 0, 1)
        [HideInInspector] _Color0_3 ("", Color) = (0.5, 1, 0, 1)
        [HideInInspector] _Color0_4 ("", Color) = (0, 1, 0, 1)
        [HideInInspector] _Color0_5 ("", Color) = (0, 1, 0.5, 1)
        [HideInInspector] _Color0_6 ("", Color) = (0, 1, 1, 1)
        [HideInInspector] _Color0_7 ("", Color) = (0, 0.5, 1, 1)
        [HideInInspector] _Color0_8 ("", Color) = (0.5, 0.5, 0.5, 1)
        [HideInInspector] _Color0_9 ("", Color) = (0.8, 0.8, 0.8, 1)
        [HideInInspector] _Color1_0 ("", Color) = (0, 0, 1, 1)
        [HideInInspector] _Color1_1 ("", Color) = (0.5, 0, 1, 1)
        [HideInInspector] _Color1_2 ("", Color) = (1, 0, 1, 1)
        [HideInInspector] _Color1_3 ("", Color) = (1, 0, 0.5, 1)
        [HideInInspector] _Color1_4 ("", Color) = (0.5, 0.5, 0.5, 1)
        [HideInInspector] _Color1_5 ("", Color) = (0.75, 0.75, 0.75, 1)
        [HideInInspector] _Color1_6 ("", Color) = (1, 1, 1, 1)
        [HideInInspector] _Color1_7 ("", Color) = (0.25, 0.25, 0.25, 1)
        [HideInInspector] _Color1_8 ("", Color) = (0.6, 0.3, 0.9, 1)
        [HideInInspector] _Color1_9 ("", Color) = (0.9, 0.6, 0.3, 1)
        [HideInInspector] _Color2_0 ("", Color) = (0.8, 0.2, 0.2, 1)
        [HideInInspector] _Color2_1 ("", Color) = (0.2, 0.8, 0.2, 1)
        [HideInInspector] _Color2_2 ("", Color) = (0.2, 0.2, 0.8, 1)
        [HideInInspector] _Color2_3 ("", Color) = (0.8, 0.8, 0.2, 1)
        [HideInInspector] _Color2_4 ("", Color) = (0.8, 0.2, 0.8, 1)
        [HideInInspector] _Color2_5 ("", Color) = (0.2, 0.8, 0.8, 1)
        [HideInInspector] _Color2_6 ("", Color) = (0.6, 0.3, 0.1, 1)
        [HideInInspector] _Color2_7 ("", Color) = (0.1, 0.6, 0.3, 1)
        [HideInInspector] _Color2_8 ("", Color) = (0.3, 0.6, 0.1, 1)
        [HideInInspector] _Color2_9 ("", Color) = (0.6, 0.1, 0.3, 1)
        [HideInInspector] _Color3_0 ("", Color) = (0.3, 0.1, 0.6, 1)
        [HideInInspector] _Color3_1 ("", Color) = (0.9, 0.5, 0.1, 1)
        [HideInInspector] _Color3_2 ("", Color) = (0.5, 0.9, 0.1, 1)
        [HideInInspector] _Color3_3 ("", Color) = (0.1, 0.5, 0.9, 1)
        [HideInInspector] _Color3_4 ("", Color) = (0.7, 0.7, 0.1, 1)
        [HideInInspector] _Color3_5 ("", Color) = (0.7, 0.1, 0.7, 1)
        [HideInInspector] _Color3_6 ("", Color) = (0.1, 0.7, 0.7, 1)
        [HideInInspector] _Color3_7 ("", Color) = (0.4, 0.4, 0.4, 1)
        [HideInInspector] _Color3_8 ("", Color) = (0.2, 0.9, 0.5, 1)
        [HideInInspector] _Color3_9 ("", Color) = (0.9, 0.2, 0.8, 1)
        [HideInInspector] _Color4_0 ("", Color) = (0.8, 0.2, 0.2, 1)
        [HideInInspector] _Color4_1 ("", Color) = (0.2, 0.8, 0.2, 1)
        [HideInInspector] _Color4_2 ("", Color) = (0.2, 0.2, 0.8, 1)
        [HideInInspector] _Color4_3 ("", Color) = (0.8, 0.8, 0.2, 1)
        [HideInInspector] _Color4_4 ("", Color) = (0.8, 0.2, 0.8, 1)
        [HideInInspector] _Color4_5 ("", Color) = (0.2, 0.8, 0.8, 1)
        [HideInInspector] _Color4_6 ("", Color) = (0.6, 0.3, 0.1, 1)
        [HideInInspector] _Color4_7 ("", Color) = (0.1, 0.6, 0.3, 1)
        [HideInInspector] _Color4_8 ("", Color) = (0.3, 0.6, 0.1, 1)
        [HideInInspector] _Color4_9 ("", Color) = (0.6, 0.1, 0.3, 1)
        [HideInInspector] _Color5_0 ("", Color) = (0.3, 0.1, 0.6, 1)
        [HideInInspector] _Color5_1 ("", Color) = (0.9, 0.5, 0.1, 1)
        [HideInInspector] _Color5_2 ("", Color) = (0.5, 0.9, 0.1, 1)
        [HideInInspector] _Color5_3 ("", Color) = (0.1, 0.5, 0.9, 1)
        [HideInInspector] _Color5_4 ("", Color) = (0.7, 0.7, 0.1, 1)
        [HideInInspector] _Color5_5 ("", Color) = (0.7, 0.1, 0.7, 1)
        [HideInInspector] _Color5_6 ("", Color) = (0.1, 0.7, 0.7, 1)
        [HideInInspector] _Color5_7 ("", Color) = (0.4, 0.4, 0.4, 1)
        [HideInInspector] _Color5_8 ("", Color) = (0.2, 0.9, 0.5, 1)
        [HideInInspector] _Color5_9 ("", Color) = (0.9, 0.2, 0.8, 1)

        [Header(Top 4 Rows Gradients)]
        [HideInInspector] _GradientRow6_Start ("Gradient Row 6 Start", Color) = (1, 1, 1, 1)
        [HideInInspector] _GradientRow6_End ("Gradient Row 6 End", Color) = (0, 0, 0, 1)
        [HideInInspector] _GradientRow7_Start ("Gradient Row 7 Start", Color) = (1, 0, 1, 1)
        [HideInInspector] _GradientRow7_End ("Gradient Row 7 End", Color) = (0, 1, 1, 1)
        [HideInInspector] _GradientRow8_Start ("Gradient Row 8 Start", Color) = (1, 0.5, 0, 1)
        [HideInInspector] _GradientRow8_End ("Gradient Row 8 End", Color) = (0.5, 0, 1, 1)
        [HideInInspector] _GradientRow9_Start ("Gradient Row 9 Start", Color) = (0, 0, 0, 1)
        [HideInInspector] _GradientRow9_End ("Gradient Row 9 End", Color) = (1, 1, 1, 1)
        
        [Header(Surface Properties)]
        [NoScaleOffset] _NormalMap ("Normal Map (UV1)", 2D) = "bump" {}
        _Smoothness ("Smoothness (when color alpha > 0.5)", Range(0, 1)) = 0
        _Metallic ("Metallic (when color alpha > 0.5)", Range(0, 1)) = 0
        
        [Header(Options)]
        [Toggle] _GammaCorrectVertexColors ("Gamma Correct Vertex Colors", Float) = 1
        [Toggle] _DoubleSided ("Double Sided", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }
    
    CustomEditor "Normalgon.SharedAssets.EditorScripts.UV2ColorMapperGUI"
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                float2 uv2 : TEXCOORD4;
                float4 vertexColor : TEXCOORD5;
                float4 shadowCoord : TEXCOORD6;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                // Bottom 6 rows - 6x10 Color Grid
                float4 _Color0_0, _Color0_1, _Color0_2, _Color0_3, _Color0_4, _Color0_5, _Color0_6, _Color0_7, _Color0_8, _Color0_9;
                float4 _Color1_0, _Color1_1, _Color1_2, _Color1_3, _Color1_4, _Color1_5, _Color1_6, _Color1_7, _Color1_8, _Color1_9;
                float4 _Color2_0, _Color2_1, _Color2_2, _Color2_3, _Color2_4, _Color2_5, _Color2_6, _Color2_7, _Color2_8, _Color2_9;
                float4 _Color3_0, _Color3_1, _Color3_2, _Color3_3, _Color3_4, _Color3_5, _Color3_6, _Color3_7, _Color3_8, _Color3_9;
                float4 _Color4_0, _Color4_1, _Color4_2, _Color4_3, _Color4_4, _Color4_5, _Color4_6, _Color4_7, _Color4_8, _Color4_9;
                float4 _Color5_0, _Color5_1, _Color5_2, _Color5_3, _Color5_4, _Color5_5, _Color5_6, _Color5_7, _Color5_8, _Color5_9;

                // Top 4 rows - Horizontal gradients
                float4 _GradientRow6_Start, _GradientRow6_End;
                float4 _GradientRow7_Start, _GradientRow7_End;
                float4 _GradientRow8_Start, _GradientRow8_End;
                float4 _GradientRow9_Start, _GradientRow9_End;

                float _Smoothness;
                float _Metallic;
                float _GammaCorrectVertexColors;
                float _DoubleSided;
                float _Cull;
            CBUFFER_END

            // Helper function to get color from grid or gradient
            float4 GetColor(int row, int col, float uvX)
            {
                // Rows 0-5: 6x10 grid (bottom 6 rows)
                if (row == 0)
                {
                    if (col == 0) return _Color0_0;
                    if (col == 1) return _Color0_1;
                    if (col == 2) return _Color0_2;
                    if (col == 3) return _Color0_3;
                    if (col == 4) return _Color0_4;
                    if (col == 5) return _Color0_5;
                    if (col == 6) return _Color0_6;
                    if (col == 7) return _Color0_7;
                    if (col == 8) return _Color0_8;
                    return _Color0_9;
                }
                else if (row == 1)
                {
                    if (col == 0) return _Color1_0;
                    if (col == 1) return _Color1_1;
                    if (col == 2) return _Color1_2;
                    if (col == 3) return _Color1_3;
                    if (col == 4) return _Color1_4;
                    if (col == 5) return _Color1_5;
                    if (col == 6) return _Color1_6;
                    if (col == 7) return _Color1_7;
                    if (col == 8) return _Color1_8;
                    return _Color1_9;
                }
                else if (row == 2)
                {
                    if (col == 0) return _Color2_0;
                    if (col == 1) return _Color2_1;
                    if (col == 2) return _Color2_2;
                    if (col == 3) return _Color2_3;
                    if (col == 4) return _Color2_4;
                    if (col == 5) return _Color2_5;
                    if (col == 6) return _Color2_6;
                    if (col == 7) return _Color2_7;
                    if (col == 8) return _Color2_8;
                    return _Color2_9;
                }
                else if (row == 3)
                {
                    if (col == 0) return _Color3_0;
                    if (col == 1) return _Color3_1;
                    if (col == 2) return _Color3_2;
                    if (col == 3) return _Color3_3;
                    if (col == 4) return _Color3_4;
                    if (col == 5) return _Color3_5;
                    if (col == 6) return _Color3_6;
                    if (col == 7) return _Color3_7;
                    if (col == 8) return _Color3_8;
                    return _Color3_9;
                }
                else if (row == 4)
                {
                    if (col == 0) return _Color4_0;
                    if (col == 1) return _Color4_1;
                    if (col == 2) return _Color4_2;
                    if (col == 3) return _Color4_3;
                    if (col == 4) return _Color4_4;
                    if (col == 5) return _Color4_5;
                    if (col == 6) return _Color4_6;
                    if (col == 7) return _Color4_7;
                    if (col == 8) return _Color4_8;
                    return _Color4_9;
                }
                else if (row == 5)
                {
                    if (col == 0) return _Color5_0;
                    if (col == 1) return _Color5_1;
                    if (col == 2) return _Color5_2;
                    if (col == 3) return _Color5_3;
                    if (col == 4) return _Color5_4;
                    if (col == 5) return _Color5_5;
                    if (col == 6) return _Color5_6;
                    if (col == 7) return _Color5_7;
                    if (col == 8) return _Color5_8;
                    return _Color5_9;
                }
                // Rows 6-9: Horizontal gradients (top 4 rows)
                else if (row == 6)
                {
                    return lerp(_GradientRow6_Start, _GradientRow6_End, uvX);
                }
                else if (row == 7)
                {
                    return lerp(_GradientRow7_Start, _GradientRow7_End, uvX);
                }
                else if (row == 8)
                {
                    return lerp(_GradientRow8_Start, _GradientRow8_End, uvX);
                }
                else // row == 9
                {
                    return lerp(_GradientRow9_Start, _GradientRow9_End, uvX);
                }
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = float4(normalInput.tangentWS.xyz, input.tangentOS.w);
                output.uv = input.uv;
                output.uv2 = input.uv2;
                output.vertexColor = input.color;
                output.shadowCoord = GetShadowCoord(vertexInput);

                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Convert UV2 to grid coordinates
                // UV convention: (0,0) = bottom-left, (1,1) = top-right
                // Now using 10 rows: 0-5 are 10-column grids, 6-9 are gradients
                int row = clamp(floor(input.uv2.y * 10.0), 0, 9);
                int col = clamp(floor(input.uv2.x * 10.0), 0, 9);

                // Get color (pass UV X for gradient interpolation)
                float4 gridColor = GetColor(row, col, input.uv2.x);
                float4 albedo = gridColor;
                
                // Check metallic/smooth toggle BEFORE vertex color replacement
                float useMetallicSmooth = gridColor.a > 0.5;
                float metallic = useMetallicSmooth * _Metallic;
                float smoothness = useMetallicSmooth * _Smoothness;
                
                // If color slot is pure black (0,0,0), use vertex color instead
                if (gridColor.r == 0.0 && gridColor.g == 0.0 && gridColor.b == 0.0)
                {
                    albedo = input.vertexColor;
                    // Optionally gamma correct vertex colors
                    if (_GammaCorrectVertexColors > 0.5)
                    {
                        albedo.rgb = pow(abs(albedo.rgb), 2.2);
                    }
                    // Keep alpha at 1 for rendering, but metallic/smooth already determined above
                    albedo.a = 1.0;
                }

                // Sample normal map
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                float3 bitangentWS = cross(input.normalWS, input.tangentWS.xyz) * input.tangentWS.w;
                float3x3 tangentToWorld = float3x3(input.tangentWS.xyz, bitangentWS, input.normalWS);
                float3 normalWS = normalize(mul(normalTS, tangentToWorld));

                // PBR lighting setup
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = input.shadowCoord;
                inputData.fogCoord = 0;
                inputData.vertexLighting = 0;
                inputData.bakedGI = SampleSH(normalWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = metallic;
                surfaceData.specular = 0.04;
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = 0;
                surfaceData.occlusion = 1;
                surfaceData.alpha = 1.0; // Force opaque

                return UniversalFragmentPBR(inputData, surfaceData);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            float3 _LightDirection;
            float3 _LightPosition;

            struct AttributesShadow
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsShadow
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 GetShadowPositionHClip(AttributesShadow input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif

                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            VaryingsShadow ShadowPassVertex(AttributesShadow input)
            {
                VaryingsShadow output = (VaryingsShadow)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            float4 ShadowPassFragment(VaryingsShadow input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AttributesDepthNormals
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsDepthNormals
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            VaryingsDepthNormals DepthNormalsVertex(AttributesDepthNormals input)
            {
                VaryingsDepthNormals output = (VaryingsDepthNormals)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = vertexInput.positionCS;
                output.normalWS = normalInput.normalWS;

                return output;
            }

            float4 DepthNormalsFragment(VaryingsDepthNormals input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                
                float3 normalWS = normalize(input.normalWS);
                return float4(normalWS, 0.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct AttributesDepthOnly
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VaryingsDepthOnly
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            VaryingsDepthOnly DepthOnlyVertex(AttributesDepthOnly input)
            {
                VaryingsDepthOnly output = (VaryingsDepthOnly)0;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;

                return output;
            }

            float4 DepthOnlyFragment(VaryingsDepthOnly input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
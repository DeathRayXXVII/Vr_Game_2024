Shader "Unlit/Unlit Transparent Color" {
    Properties {
        _Color ("Main Color", Color) = (0,0,0,1) // Default to black
        _ZWriteMode ("Depth Write Mode", Float) = 1 // Default to opaque mode (1), but can be set to 0 for transparent mode
    }

    SubShader {
        Tags {"Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        Fog {Mode Off}

        ZWrite [_ZWriteMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Color [_Color]

        Pass {}
    }
}
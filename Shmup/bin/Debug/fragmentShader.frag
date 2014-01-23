uniform vec4 multiColor;
uniform sampler2D textureUnit;

uniform float offsetX;

#if __VERSION__ >= 130

in vec2 texCoord;

out vec4 gl_FragColor;

#else

varying vec2 texCoord;

#endif

void main()
{
    gl_FragColor = texture2D(textureUnit, vec2(texCoord.x + offsetX,
        texCoord.y)) * multiColor;
}
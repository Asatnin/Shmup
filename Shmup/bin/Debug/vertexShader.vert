uniform mat4 myProjectionMatrix;
uniform mat4 myModelViewMatrix;

#if __VERSION__ >= 130

in vec2 vertexPosition;

in vec2 LTexCoord;
out vec2 texCoord;

#else

attribute vec2 vertexPosition;

attribute vec2 LTexCoord;
varying vec2 texCoord;

#endif

void main()
{
    texCoord = LTexCoord;
    
    gl_Position = myProjectionMatrix * myModelViewMatrix * vec4(vertexPosition,
        0.0, 1.0);
}
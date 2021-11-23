using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using System;
using itk.simple;

namespace LearnOpenTK.Common
{
    // A helper class, much like Shader, meant to simplify loading textures.
    public class Texture3D
    {
        public readonly int Handle;

        public static Texture3D LoadFromFile(string path)
        {
            // Generate handle
            int handle = GL.GenTexture();

            // Bind the handle
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture3D, handle);
            

            ImageFileReader reader = new ImageFileReader();
            reader.SetFileName(path);
            Image image = reader.Execute();

            var buffer = image.GetBufferAsUInt8();
            var size = image.GetSize();
            uint xs = size[0];
            uint ys = size[1];
            uint zs = size[2];
            GL.TexImage3D(TextureTarget.Texture3D,
                    0,
                    PixelInternalFormat.R8Snorm,
                    (int)xs,
                    (int)ys,
                    (int)zs,
                    0,
                    PixelFormat.Red,
                    PixelType.UnsignedShort,
                    buffer);

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);

            return new Texture3D(handle);
        }

        public Texture3D(int glHandle)
        {
            Handle = glHandle;
        }

        // Activate texture
        // Multiple textures can be bound, if your shader needs more than just one.
        // If you want to do that, use GL.ActiveTexture to set which slot GL.BindTexture binds to.
        // The OpenGL standard requires that there be at least 16, but there can be more depending on your graphics card.
        public void Use(TextureUnit unit)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture3D, Handle);
        }
    }
}

using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using System;
using itk.simple;
using System.Diagnostics;
using OpenTK.Mathematics;

namespace LearnOpenTK
{
    // A helper class, much like Shader, meant to simplify loading textures.
    public class Texture3D
    {
        public readonly int Handle;

        public readonly double ImageHighestIntensity;
        public readonly double ImageLowestIntensity;

        public readonly Matrix4 Transformation;
        public readonly Vector3 Dimensions;

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
            var aa = image.GetMetaDataKeys();
            for(int i = 0; i<aa.Count; i++)
            {
                var key = aa[i];
                var ss = image.GetMetaData(aa[i]);
            }

            var srow_x = image.GetMetaData("srow_x");
            var srow_y = image.GetMetaData("srow_y");
            var srow_z = image.GetMetaData("srow_z");

            string[] srow_x_parts = srow_x.Split(' ');
            string[] srow_y_parts = srow_y.Split(' ');
            string[] srow_z_parts = srow_z.Split(' ');

            Matrix4 mat = new Matrix4()
            {
                Row0 = new Vector4(
                    float.Parse(srow_x_parts[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_x_parts[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_x_parts[2], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_x_parts[3], System.Globalization.CultureInfo.InvariantCulture)
                    ),
                Row1 = new Vector4(
                    float.Parse(srow_y_parts[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_y_parts[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_y_parts[2], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_y_parts[3], System.Globalization.CultureInfo.InvariantCulture)
                    ),
                Row2 = new Vector4(
                    float.Parse(srow_z_parts[0], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_z_parts[1], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_z_parts[2], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(srow_z_parts[3], System.Globalization.CultureInfo.InvariantCulture)
                    ),
                Row3 = new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
            };

            MinimumMaximumImageFilter filter = new MinimumMaximumImageFilter();
            filter.Execute(image);
            double minIntensity = (filter.GetMinimum() + 32767.0) / 65535.0;
            double maxIntensity = (filter.GetMaximum() + 32767.0) / 65535.0;
            filter.Dispose();

            //CastImageFilter cast = new CastImageFilter();
            //cast.SetOutputPixelType(PixelIDValueEnum.sitkInt32);
            //Image destImage = cast.Execute(image);

            var buffer = image.GetBufferAsInt16();
            var size = image.GetSize();
            int xs = (int)size[0];
            int ys = (int)size[1];
            int zs = (int)size[2];
            GL.TexImage3D(TextureTarget.Texture3D,
                    0,
                    PixelInternalFormat.R16,
                    xs,
                    ys,
                    zs,
                    0,
                    PixelFormat.Red,
                    PixelType.Short,
                    buffer);

            var error = GL.GetError();
            Debug.Assert(error == OpenTK.Graphics.OpenGL4.ErrorCode.NoError);

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture3D);

            return new Texture3D(handle, minIntensity, maxIntensity, mat, new Vector3(xs, ys, zs));
        }

        public Texture3D(int glHandle, double minIntensity, double maxIntensity, Matrix4 transformation, Vector3 dimensions)
        {
            Handle = glHandle;
            ImageLowestIntensity = minIntensity;
            ImageHighestIntensity = maxIntensity;
            Transformation = transformation;
            Dimensions = dimensions;
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

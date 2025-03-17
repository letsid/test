using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer;

internal class Resources
{
	private static byte[] _isometricEffect;

	private static byte[] _xBREffect;

	public static byte[] IsometricEffect => _isometricEffect ?? (_isometricEffect = GetResource("ClassicUO.shaders.IsometricWorld.fxc"));

	public static byte[] xBREffect => _xBREffect ?? (_xBREffect = GetResource("ClassicUO.shaders.xBR.fxc"));

	public static byte[] StandardEffect
	{
		get
		{
			Stream manifestResourceStream = typeof(SpriteBatch).Assembly.GetManifestResourceStream("Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.fxb");
			using MemoryStream memoryStream = new MemoryStream();
			manifestResourceStream.CopyTo(memoryStream);
			return memoryStream.ToArray();
		}
	}

	private static byte[] GetResource(string name)
	{
		Stream manifestResourceStream = typeof(UltimaBatcher2D).Assembly.GetManifestResourceStream(name);
		using MemoryStream memoryStream = new MemoryStream();
		manifestResourceStream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}
}

namespace ZLibNative;

public class Adler32
{
	private const int _base = 65521;

	private const int _nmax = 5550;

	private uint a = 1u;

	private uint b;

	private int pend;

	public void Update(byte data)
	{
		if (pend >= 5550)
		{
			UpdateModulus();
		}
		a += data;
		b += a;
		pend++;
	}

	public void Update(byte[] data)
	{
		Update(data, 0, data.Length);
	}

	public void Update(byte[] data, int offset, int length)
	{
		int num = 5550 - pend;
		for (int i = 0; i < length; i++)
		{
			if (i == num)
			{
				UpdateModulus();
				num = i + 5550;
			}
			a += data[i + offset];
			b += a;
			pend++;
		}
	}

	public void Reset()
	{
		a = 1u;
		b = 0u;
		pend = 0;
	}

	private void UpdateModulus()
	{
		a %= 65521u;
		b %= 65521u;
		pend = 0;
	}

	public uint GetValue()
	{
		if (pend > 0)
		{
			UpdateModulus();
		}
		return (b << 16) | a;
	}
}

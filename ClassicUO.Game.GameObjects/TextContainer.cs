namespace ClassicUO.Game.GameObjects;

internal class TextContainer : LinkedObject
{
	public int Size;

	public int MaxSize = 5;

	public void Add(TextObject obj)
	{
		PushToBack(obj);
		if (Size >= MaxSize)
		{
			((TextObject)Items)?.Destroy();
			Remove(Items);
		}
		else
		{
			Size++;
		}
	}

	public new void Clear()
	{
		TextObject textObject = (TextObject)Items;
		Items = null;
		while (textObject != null)
		{
			TextObject obj = (TextObject)textObject.Next;
			textObject.Next = null;
			textObject.Destroy();
			Remove(textObject);
			textObject = obj;
		}
		Size = 0;
	}
}

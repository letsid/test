using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers;

internal sealed class AnchorManager
{
	private enum AnchorDirection
	{
		Left,
		Top,
		Right,
		Bottom
	}

	public class AnchorGroup
	{
		private AnchorableGump[,] controlMatrix;

		private int updateCount;

		public AnchorGroup(AnchorableGump initial)
		{
			controlMatrix = new AnchorableGump[initial.WidthMultiplier, initial.HeightMultiplier];
			AddControlToMatrix(0, 0, initial);
		}

		public AnchorGroup()
		{
			controlMatrix = new AnchorableGump[0, 0];
		}

		public void AddControlToMatrix(int xinit, int yInit, AnchorableGump control)
		{
			for (int i = 0; i < control.WidthMultiplier; i++)
			{
				for (int j = 0; j < control.HeightMultiplier; j++)
				{
					controlMatrix[i + xinit, j + yInit] = control;
				}
			}
		}

		public void Save(XmlTextWriter writer)
		{
			writer.WriteStartElement("anchored_group_gump");
			writer.WriteAttributeString("matrix_w", controlMatrix.GetLength(0).ToString());
			writer.WriteAttributeString("matrix_h", controlMatrix.GetLength(1).ToString());
			for (int i = 0; i < controlMatrix.GetLength(1); i++)
			{
				for (int j = 0; j < controlMatrix.GetLength(0); j++)
				{
					AnchorableGump anchorableGump = controlMatrix[j, i];
					if (anchorableGump != null)
					{
						writer.WriteStartElement("gump");
						anchorableGump.Save(writer);
						writer.WriteAttributeString("matrix_x", j.ToString());
						writer.WriteAttributeString("matrix_y", i.ToString());
						writer.WriteEndElement();
					}
				}
			}
			writer.WriteEndElement();
		}

		public void MakeTopMost()
		{
			for (int i = 0; i < controlMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < controlMatrix.GetLength(1); j++)
				{
					if (controlMatrix[i, j] != null)
					{
						UIManager.MakeTopMostGump(controlMatrix[i, j]);
					}
				}
			}
		}

		public void DetachControl(AnchorableGump control)
		{
			for (int i = 0; i < controlMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < controlMatrix.GetLength(1); j++)
				{
					if (controlMatrix[i, j] == control)
					{
						controlMatrix[i, j] = null;
					}
				}
			}
		}

		public void UpdateLocation(Control control, int deltaX, int deltaY)
		{
			if (updateCount != 0)
			{
				return;
			}
			updateCount++;
			HashSet<Control> hashSet = new HashSet<Control>();
			for (int i = 0; i < controlMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < controlMatrix.GetLength(1); j++)
				{
					if (controlMatrix[i, j] != null && controlMatrix[i, j] != control && !hashSet.Contains(controlMatrix[i, j]))
					{
						controlMatrix[i, j].X += deltaX;
						controlMatrix[i, j].Y += deltaY;
						hashSet.Add(controlMatrix[i, j]);
					}
				}
			}
			updateCount--;
		}

		public void AnchorControlAt(AnchorableGump control, AnchorableGump host, Point relativePosition)
		{
			Point? controlCoordinates = GetControlCoordinates(host);
			if (!controlCoordinates.HasValue)
			{
				return;
			}
			int num = controlCoordinates.Value.X + relativePosition.X;
			int num2 = controlCoordinates.Value.Y + relativePosition.Y;
			if (IsEmptyDirection(num, num2))
			{
				if (num < 0)
				{
					ResizeMatrix(controlMatrix.GetLength(0) + control.WidthMultiplier, controlMatrix.GetLength(1), control.WidthMultiplier, 0);
				}
				else if (num > controlMatrix.GetLength(0) - control.WidthMultiplier)
				{
					ResizeMatrix(controlMatrix.GetLength(0) + control.WidthMultiplier, controlMatrix.GetLength(1), 0, 0);
				}
				if (num2 < 0)
				{
					ResizeMatrix(controlMatrix.GetLength(0), controlMatrix.GetLength(1) + control.HeightMultiplier, 0, control.HeightMultiplier);
				}
				else if (num2 > controlMatrix.GetLength(1) - 1)
				{
					ResizeMatrix(controlMatrix.GetLength(0), controlMatrix.GetLength(1) + control.HeightMultiplier, 0, 0);
				}
				controlCoordinates = GetControlCoordinates(host);
				if (controlCoordinates.HasValue)
				{
					num = controlCoordinates.Value.X + relativePosition.X;
					num2 = controlCoordinates.Value.Y + relativePosition.Y;
					AddControlToMatrix(num, num2, control);
				}
			}
		}

		public bool IsEmptyDirection(AnchorableGump draggedControl, AnchorableGump host, Point relativePosition)
		{
			Point? controlCoordinates = GetControlCoordinates(host);
			bool flag = true;
			if (controlCoordinates.HasValue)
			{
				Point point = controlCoordinates.Value + relativePosition;
				for (int i = 0; i < draggedControl.WidthMultiplier; i++)
				{
					for (int j = 0; j < draggedControl.HeightMultiplier; j++)
					{
						flag &= IsEmptyDirection(point.X + i, point.Y + j);
					}
				}
			}
			return flag;
		}

		public bool IsEmptyDirection(int x, int y)
		{
			if (x < 0 || x > controlMatrix.GetLength(0) - 1 || y < 0 || y > controlMatrix.GetLength(1) - 1)
			{
				return true;
			}
			return controlMatrix[x, y] == null;
		}

		private Point? GetControlCoordinates(AnchorableGump control)
		{
			for (int i = 0; i < controlMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < controlMatrix.GetLength(1); j++)
				{
					if (controlMatrix[i, j] == control)
					{
						return new Point(i, j);
					}
				}
			}
			return null;
		}

		public void ResizeMatrix(int xCount, int yCount, int xInitial, int yInitial)
		{
			AnchorableGump[,] array = new AnchorableGump[xCount, yCount];
			for (int i = 0; i < controlMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < controlMatrix.GetLength(1); j++)
				{
					array[i + xInitial, j + yInitial] = controlMatrix[i, j];
				}
			}
			controlMatrix = array;
		}

		private void printMatrix()
		{
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			for (int i = 0; i < controlMatrix.GetLength(1); i++)
			{
				for (int j = 0; j < controlMatrix.GetLength(0); j++)
				{
					if (controlMatrix[j, i] != null)
					{
						Console.Write(" " + controlMatrix[j, i].LocalSerial + " ");
					}
					else
					{
						Console.Write(" ---------- ");
					}
				}
				Console.WriteLine();
			}
		}
	}

	private static readonly Vector2[][] _anchorTriangles = new Vector2[4][]
	{
		new Vector2[3]
		{
			new Vector2(0f, 0f),
			new Vector2(0.5f, 0.5f),
			new Vector2(0f, 1f)
		},
		new Vector2[3]
		{
			new Vector2(0f, 0f),
			new Vector2(0.5f, 0.5f),
			new Vector2(1f, 0f)
		},
		new Vector2[3]
		{
			new Vector2(1f, 0f),
			new Vector2(0.5f, 0.5f),
			new Vector2(1f, 1f)
		},
		new Vector2[3]
		{
			new Vector2(0f, 1f),
			new Vector2(0.5f, 0.5f),
			new Vector2(1f, 1f)
		}
	};

	private static readonly Point[] _anchorDirectionMatrix = new Point[4]
	{
		new Point(-1, 0),
		new Point(0, -1),
		new Point(1, 0),
		new Point(0, 1)
	};

	private static readonly Point[] _anchorMultiplierMatrix = new Point[4]
	{
		new Point(0, 0),
		new Point(0, 0),
		new Point(1, 0),
		new Point(0, 1)
	};

	private readonly Dictionary<AnchorableGump, AnchorGroup> reverseMap = new Dictionary<AnchorableGump, AnchorGroup>();

	public AnchorGroup this[AnchorableGump control]
	{
		get
		{
			reverseMap.TryGetValue(control, out var value);
			return value;
		}
		set
		{
			if (reverseMap.ContainsKey(control) && value == null)
			{
				reverseMap.Remove(control);
			}
			else
			{
				reverseMap.Add(control, value);
			}
		}
	}

	public void Save(XmlTextWriter writer)
	{
		foreach (AnchorGroup item in reverseMap.Values.Distinct())
		{
			item.Save(writer);
		}
	}

	public void DropControl(AnchorableGump draggedControl, AnchorableGump host)
	{
		if (host.AnchorType != draggedControl.AnchorType || this[draggedControl] != null)
		{
			return;
		}
		Point? item = GetAnchorDirection(draggedControl, host).Item1;
		if (item.HasValue)
		{
			if (this[host] == null)
			{
				this[host] = new AnchorGroup(host);
			}
			if (this[host].IsEmptyDirection(draggedControl, host, item.Value))
			{
				this[host].AnchorControlAt(draggedControl, host, item.Value);
				this[draggedControl] = this[host];
			}
		}
	}

	public Point GetCandidateDropLocation(AnchorableGump draggedControl, AnchorableGump host)
	{
		if (host.AnchorType == draggedControl.AnchorType && this[draggedControl] == null)
		{
			var (point, anchorableGump) = GetAnchorDirection(draggedControl, host);
			if (point.HasValue && (this[host] == null || this[host].IsEmptyDirection(draggedControl, host, point.Value)))
			{
				Point point2 = point.Value * new Point(anchorableGump.GroupMatrixWidth, anchorableGump.GroupMatrixHeight);
				return new Point(host.X + point2.X, host.Y + point2.Y);
			}
		}
		return draggedControl.Location;
	}

	public AnchorableGump GetAnchorableControlUnder(AnchorableGump draggedControl)
	{
		return ClosestOverlappingControl(draggedControl);
	}

	public void DetachControl(AnchorableGump control)
	{
		if (this[control] == null)
		{
			return;
		}
		List<AnchorableGump> list = (from o in reverseMap
			where o.Value == this[control]
			select o.Key).ToList();
		if (list.Count == 2)
		{
			foreach (AnchorableGump item in list)
			{
				this[item].DetachControl(item);
				this[item] = null;
			}
			return;
		}
		this[control].DetachControl(control);
		this[control] = null;
	}

	public void DisposeAllControls(AnchorableGump control)
	{
		if (this[control] == null)
		{
			return;
		}
		foreach (AnchorableGump item in (from o in reverseMap
			where o.Value == this[control]
			select o.Key).ToList())
		{
			this[item] = null;
			item.Dispose();
		}
	}

	private (Point?, AnchorableGump) GetAnchorDirection(AnchorableGump draggedControl, AnchorableGump host)
	{
		int num = Math.Abs(draggedControl.X - host.X) * 100 / host.Width;
		int num2 = Math.Abs(draggedControl.Y - host.Y) * 100 / host.Height;
		if (num > num2)
		{
			if (draggedControl.X > host.X)
			{
				return (new Point(host.WidthMultiplier, 0), host);
			}
			return (new Point(-draggedControl.WidthMultiplier, 0), draggedControl);
		}
		if (draggedControl.Y > host.Y)
		{
			return (new Point(0, host.HeightMultiplier), host);
		}
		return (new Point(0, -draggedControl.HeightMultiplier), draggedControl);
	}

	private bool IsPointInPolygon(Vector2[] polygon, Vector2 point)
	{
		bool flag = false;
		int num = 0;
		int num2 = polygon.Length - 1;
		while (num < polygon.Length)
		{
			if (polygon[num].Y > point.Y != polygon[num2].Y > point.Y && point.X < (polygon[num2].X - polygon[num].X) * (point.Y - polygon[num].Y) / (polygon[num2].Y - polygon[num].Y) + polygon[num].X)
			{
				flag = !flag;
			}
			num2 = num++;
		}
		return flag;
	}

	public AnchorableGump ClosestOverlappingControl(AnchorableGump control)
	{
		if (control == null || control.IsDisposed)
		{
			return null;
		}
		AnchorableGump result = null;
		int num = 99999;
		foreach (Gump gump in UIManager.Gumps)
		{
			if (!gump.IsDisposed && gump is AnchorableGump anchorableGump && anchorableGump.AnchorType == control.AnchorType && IsOverlapping(control, anchorableGump))
			{
				int num2 = Math.Abs(control.X - anchorableGump.X) + Math.Abs(control.Y - anchorableGump.Y);
				if (num2 < num)
				{
					num = num2;
					result = anchorableGump;
				}
			}
		}
		return result;
	}

	private bool IsOverlapping(AnchorableGump control, AnchorableGump host)
	{
		if (control == host)
		{
			return false;
		}
		if (control.Bounds.Top > host.Bounds.Bottom || control.Bounds.Bottom < host.Bounds.Top)
		{
			return false;
		}
		if (control.Bounds.Right < host.Bounds.Left || control.Bounds.Left > host.Bounds.Right)
		{
			return false;
		}
		return true;
	}
}

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Enyim.Iris.Server.Inspection;

public enum DebugNodeKind { Element, Document, Text }

/// <summary>
/// Neutral, CDP-free representation of a node in the host app's control tree. The host app
/// builds a <see cref="DebugNode"/> graph and pushes it via <c>IDebugServer.PublishTree</c>;
/// the server maps it to DOM types internally.
/// </summary>
public sealed record DebugNode(
	int Id,
	string Name,
	DebugNodeKind Kind = DebugNodeKind.Element,
	NodeBoxModel? BoxModel = null,
	IReadOnlyList<string>? Classes = null,
	IReadOnlyDictionary<string, string>? Attributes = null,
	IReadOnlyDictionary<string, string>? ComputedStyle = null,
	IReadOnlyList<DebugNode>? Children = null);

public enum DebugLogLevel { Verbose, Info, Warning, Error }

public readonly record struct DebugLogEntry(
	DebugLogLevel Level,
	string Text,
	string? Source = null,
	DateTimeOffset? Timestamp = null);

public readonly record struct MemoryStats(long HeapBytes, long Gen0, long Gen1, long Gen2);

// x1,y1 is top-left; corners are provided clockwise (assumes axis-aligned rectangle)
public readonly record struct Quad(int X1, int Y1, int X2, int Y2, int X3, int Y3, int X4, int Y4);

public sealed record NodeBoxModel(Quad Content, Quad Padding, Quad Border, Quad Margin);

public static class NodeBoxModelExt
{
	extension(NodeBoxModel)
	{
		public static NodeBoxModel FromScreenRect(Rect screenRect, MultiValue<int> border, MultiValue<int> margin, MultiValue<int> padding)
		{
			var marginBox  = screenRect;
			var borderBox  = marginBox.Deflate(margin.Top, margin.Right, margin.Bottom, margin.Left);
			var paddingBox = borderBox.Deflate(border.Top, border.Right, border.Bottom, border.Left);
			var contentBox = paddingBox.Deflate(padding.Top, padding.Right, padding.Bottom, padding.Left);

			return new NodeBoxModel(
				Content: ToQuad(contentBox),
				Padding: ToQuad(paddingBox),
				Border:  ToQuad(borderBox),
				Margin:  ToQuad(marginBox));
		}
	}

	private static Quad ToQuad(Rect r) =>
		new(r.Left, r.Top, r.Left + r.Width, r.Top, r.Left + r.Width, r.Top + r.Height, r.Left, r.Top + r.Height);
}

public readonly record struct Rect(int Top, int Left, int Width, int Height)
{
	public int Width { get; } = Validate(Width, nameof(Width));
	public int Height { get; } = Validate(Height, nameof(Height));

	private static int Validate(int v, string name)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(v, name);
		return v;
	}
}

public readonly record struct Size(int Width, int Height)
{
	public int Width { get; } = Validate(Width, nameof(Width));
	public int Height { get; } = Validate(Height, nameof(Height));

	private static int Validate(int v, string name)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(v, name);
		return v;
	}
}

public static class RectExt
{
	extension(Rect self)
	{
		public Rect Offset(int offsetX, int offsetY)
		{
			return new Rect(
				self.Left + offsetX,
				self.Top + offsetY,
				self.Width,
				self.Height
			);
		}

		public Rect Inflate(int width, int height)
		{
			var rw = self.Width + width + width;
			var rh = self.Height + height + height;

			if (rw <= 0 || rh <= 0) return default;

			return new Rect(
				self.Left - width,
				self.Top - height,
				rw,
				rh
			);
		}

		public Rect Deflate(int width, int height) => self.Inflate(-width, -height);

		public Rect Deflate(int amountTop, int amountRight, int amountBottom, int amountLeft)
		{
			var (l, t, w, h) = self;

			l += amountLeft;
			t += amountTop;
			w -= amountLeft + amountRight;
			h -= amountTop + amountBottom;

			if (w < 0) w = 0;
			if (h < 0) h = 0;

			return new(l, t, w, h);
		}
	}
}

public readonly struct MultiValue<T> : IStructuralEquatable, IStructuralComparable, ITuple, IEquatable<T>, IEquatable<MultiValue<T>>
	where T : struct
{
	public MultiValue(T uniform) : this(uniform, uniform, uniform, uniform) { }
	public MultiValue(T horizontal, T vertical) : this(vertical, horizontal, vertical, horizontal) { }
	public MultiValue(T top, T side, T bottom) : this(top, side, bottom, side) { }

	public MultiValue(T top, T right, T bottom, T left)
	{
		Top = top;
		Right = right;
		Bottom = bottom;
		Left = left;
	}

	public T Top { get; }
	public T Right { get; }
	public T Bottom { get; }
	public T Left { get; }

	public bool IsUniform
	{
		get
		{
			var c = EqualityComparer<T>.Default;

			return c.Equals(Top, Bottom)
				&& c.Equals(Left, Right)
				&& c.Equals(Top, Left);
		}
	}

	public bool IsEmpty
	{
		get
		{
			var c = EqualityComparer<T>.Default;

			return c.Equals(Top, default)
				&& c.Equals(Right, default)
				&& c.Equals(Bottom, default)
				&& c.Equals(Left, default);
		}
	}

	public T this[int index] => index switch
	{
		MultiValue.TOP => Top,
		MultiValue.RIGHT => Right,
		MultiValue.BOTTOM => Bottom,
		MultiValue.LEFT => Left,
		_ => throw new ArgumentOutOfRangeException(nameof(index))
	};

	public MultiValue<T> With(int index, T value) => index switch
	{
		MultiValue.TOP => new(value, Right, Bottom, Left),
		MultiValue.RIGHT => new(Top, value, Bottom, Left),
		MultiValue.BOTTOM => new(Top, Right, value, Left),
		MultiValue.LEFT => new(Top, Right, Bottom, value),
		_ => throw new ArgumentOutOfRangeException(nameof(index))
	};

	int ITuple.Length => 4;

	object? ITuple.this[int index] => index switch
	{
		MultiValue.TOP => Top,
		MultiValue.RIGHT => Right,
		MultiValue.BOTTOM => Bottom,
		MultiValue.LEFT => Left,
		_ => throw new ArgumentOutOfRangeException(nameof(index))
	};

	public void Deconstruct(out T top, out T right, out T bottom, out T left)
	{
		top = Top;
		right = Right;
		bottom = Bottom;
		left = Left;
	}

	public override int GetHashCode() => HashCode.Combine(Top, Right, Bottom, Left);

	bool IStructuralEquatable.Equals(object? other, IEqualityComparer comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		return other is MultiValue<T> o && Equals(o, comparer);
	}

	private bool Equals(MultiValue<T> o, IEqualityComparer comparer)
	{
		return comparer.Equals(Top, o.Top)
			&& comparer.Equals(Right, o.Right)
			&& comparer.Equals(Bottom, o.Bottom)
			&& comparer.Equals(Left, o.Left);
	}

	int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		return HashCode.Combine(comparer.GetHashCode(Top), comparer.GetHashCode(Right), comparer.GetHashCode(Bottom), comparer.GetHashCode(Left));
	}

	int IStructuralComparable.CompareTo(object? other, IComparer comparer)
	{
		ArgumentNullException.ThrowIfNull(comparer);

		if (other is null) return 1;
		if (other is not MultiValue<T> o) throw new ArgumentException();

		var a = comparer.Compare(Top, o.Top);
		if (a != 0) return a;

		a = comparer.Compare(Right, o.Right);
		if (a != 0) return a;

		a = comparer.Compare(Bottom, o.Bottom);
		if (a != 0) return a;

		a = comparer.Compare(Left, o.Left);

		return a;
	}

	public override bool Equals(object? obj)
	{
		return obj switch
		{
			T uniform => Equals(uniform),
			MultiValue<T> other => Equals(other),
			_ => false
		};
	}

	public bool Equals(T other)
	{
		var c = EqualityComparer<T>.Default;

		return c.Equals(Top, other)
			&& c.Equals(Right, other)
			&& c.Equals(Bottom, other)
			&& c.Equals(Left, other);
	}

	public bool Equals(MultiValue<T> other)
	{
		var c = EqualityComparer<T>.Default;

		return Equals(other, c);
	}

	public static bool operator ==(MultiValue<T> a, MultiValue<T> b) => a.Equals(b);
	public static bool operator !=(MultiValue<T> a, MultiValue<T> b) => !a.Equals(b);

	public static bool operator ==(MultiValue<T> value, T uniform) => value.Equals(uniform);
	public static bool operator !=(MultiValue<T> value, T uniform) => !value.Equals(uniform);

	public static bool operator ==(T uniform, MultiValue<T> value) => value.Equals(uniform);
	public static bool operator !=(T uniform, MultiValue<T> value) => !value.Equals(uniform);

	public static implicit operator MultiValue<T>(T uniform) => new(uniform);
	public static implicit operator MultiValue<T>((T horizontal, T vertical) value) => new(value.horizontal, value.vertical);
	public static implicit operator MultiValue<T>((T top, T side, T bottom) value) => new(value.top, value.side, value.bottom);
	public static implicit operator MultiValue<T>((T top, T right, T bottom, T left) value) => new(value.top, value.right, value.bottom, value.left);
}

public static class MultiValue
{
	public static MultiValue<T> Create<T>(T uniform) where T : struct
		=> new(uniform);

	public static MultiValue<T> Create<T>(T horizontal, T vertical) where T : struct
		=> new(horizontal, vertical);

	public static MultiValue<T> Create<T>(T top, T side, T bottom) where T : struct
		=> new(top, side, bottom);

	public static MultiValue<T> Create<T>(T top, T right, T bottom, T left) where T : struct
		=> new(top, right, bottom, left);

	public const int TOP = 0;
	public const int RIGHT = 1;
	public const int BOTTOM = 2;
	public const int LEFT = 3;
}

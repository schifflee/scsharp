
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Starcraft {
	[Flags]
	public enum ElementFlags {
		Unknown00000001   = 0x00000001,
		Unknown00000002   = 0x00000002,
		Unknown00000004   = 0x00000004,
		Visible           = 0x00000008,
		RespondToMouse    = 0x00000010,
		Unknown00000020   = 0x00000020,
		CancelButton      = 0x00000040,
		NoSoundOnMouseOvr = 0x00000080,
		Unknown00000100   = 0x00000100,
		HasHotkey         = 0x00000200,
		FontSmall         = 0x00000400,
		FontMedium        = 0x00000800,
		Unknown00001000   = 0x00001000,
		Transparent       = 0x00002000,
		FontLarge         = 0x00004000,
		Unused00008000    = 0x00008000,
		Unused00010000    = 0x00010000,
		Unused00020000    = 0x00020000,
		ApplyTranslucency = 0x00040000,
		DefaultButton     = 0x00080000,
		Unused00100000    = 0x00100000,
		GreenText         = 0x00200000,
		Unused00400000    = 0x00400000,
		Unused00800000    = 0x00800000,
		Unused01000000    = 0x01000000,
		Unused02000000    = 0x02000000,
		NoClickSound      = 0x04000000,
		Unused08000000    = 0x08000000
	}

	public enum ElementType {
		DialogBox = 0,
		DefaultButton = 1,
		Button = 2,
		OptionButton = 3,
		CheckBox = 4,
		Image = 5,
		Slider = 6,
		TextBox = 8,
		LabelLeftAlign = 9,
		LabelCenterAlign = 10,
		LabelRightAlign = 11,
		ListBox = 12,
		ComboBox = 13,
		ButtonWithoutBorder = 14
	}

	public class BinElement
	{
		public ushort x1;
		public ushort y1;
		public ushort x2;
		public ushort y2;

		public ushort width;
		public ushort height;

		public byte hotkey;
		public string text;
		public uint text_offset;

		public ElementFlags flags;
		public ElementType type;

		public object resolvedData;

		public BinElement (byte[] buf, int position, uint stream_length)
		{
			x1 = Util.ReadWord (buf, position + 4);
			y1 = Util.ReadWord (buf, position + 6);
			x2 = Util.ReadWord (buf, position + 8);
			y2 = Util.ReadWord (buf, position + 10);
			width = Util.ReadWord (buf, position + 12);
			height = Util.ReadWord (buf, position + 14);
			text_offset = Util.ReadDWord (buf, position + 20);

			flags = (ElementFlags)Util.ReadDWord (buf, position + 24);
			type = (ElementType)buf[position + 34];

			if (text_offset < stream_length) {
				uint text_length = 0;
				while (buf[text_offset + text_length] != 0) text_length ++;

				text = Encoding.ASCII.GetString (buf, (int)text_offset, (int)text_length);

				if ((flags & ElementFlags.HasHotkey) == ElementFlags.HasHotkey) {
					hotkey = Encoding.ASCII.GetBytes (new char[] {text[0]})[0];
					text = text.Substring (1);
				}
			}
			else
				text = "";
		}

		public void DumpFlags ()
		{
			Console.Write ("Flags: ");
			foreach (ElementFlags f in Enum.GetValues (typeof (ElementFlags)))
				if ((flags & f) == f) {
					Console.Write (f);
					Console.Write (" ");
				}
			Console.WriteLine ();
		}

		public override string ToString ()
		{
			return String.Format ("{0} ({1})", type, text);
		}

		public string Text {
			get { return text; }
			set {
				text = value;
				resolvedData = null;
			}
		}
	}

	public class Bin : MpqResource {
		Stream stream;
		List<BinElement> elements;

		public Bin ()
		{
			elements = new List<BinElement> ();
		}

		public void ReadFromStream (Stream stream)
		{
			this.stream = stream;
			ReadElements ();
		}

		void ReadElements ()
		{
			int position;

			byte[] buf = new byte[stream.Length];

			stream.Read (buf, 0, (int)stream.Length);

			position = 0;
			do {
				BinElement element = new BinElement (buf, position, (uint)stream.Length);

				elements.Add (element);

				position += 86;
			} while (position < ((BinElement)elements[0]).text_offset);
		}

		BinElement[] arr;
		public BinElement[] Elements {
			get {
				if (arr == null)
					arr = elements.ToArray();
				return arr;
			}
		}
	}
}
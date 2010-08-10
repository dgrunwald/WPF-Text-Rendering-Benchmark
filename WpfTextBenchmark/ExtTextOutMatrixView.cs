/* 
 * Copyright (C) 2010 Daniel Grunwald
 *
 * This sourcecode is licenced under The GNU Lesser General Public License
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
 * NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
 * USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
 
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WpfTextBenchmark
{
	public unsafe class ExtTextOutMatrixView : WinFormsMatrixView
	{
		TextView view;
		
		public ExtTextOutMatrixView()
		{
			host.Child = view = new TextView() { parent = this };
		}
		
		public override string ToString()
		{
			return "GDI (ExtTextOut)";
		}
		
		System.Windows.Media.Typeface cachedTypeface;
		List<MyText> texts = new List<MyText>();
		
		public override IText AddText(string text, double fontSize, System.Windows.Media.SolidColorBrush brush)
		{
			if (cachedTypeface == null) {
				cachedTypeface = CreateTypeface();
			}
			
			LOGFONT logfont = new LOGFONT();
			logfont.lfFaceName = cachedTypeface.FontFamily.Source;
			logfont.lfHeight = -(int)fontSize;
			//logfont.lfOutPrecision = 4;
			//logfont.lfQuality = 5;
			
			MyText myText = new MyText {
				parent = this,
				text = text,
				color = brush.Color.R | (brush.Color.G << 8) | (brush.Color.B << 16),
				font = CreateFontIndirect(ref logfont)
			};
			//myText.brush = new SolidBrush(myText.color);
			
			texts.Add(myText);
			return myText;
		}
		
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		struct GCP_RESULTS
		{
			public int lStructSize;
			public char* lpOutString;
			public uint* lpOrder;
			public int* lpDx;
			public int* lpCaretPos;
			public byte* lpClass;
			public char* lpGlyphs;
			public uint nGlyphs;
			public int nMaxFit;
		}
		
		[DllImportAttribute("gdi32.dll", CharSet=CharSet.Unicode)]
		static extern int GetCharacterPlacement(
			IntPtr hdc,
			string lpString,
			int nCount,
			int nMaxExtent,
			ref GCP_RESULTS lpResults,
			uint dwFlags
		);
		
		[DllImportAttribute("gdi32.dll")]
		static extern IntPtr CreateFontIndirect(ref LOGFONT lplf);
		
		[DllImportAttribute("gdi32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool DeleteObject(IntPtr hObject);
		
		[DllImportAttribute("gdi32.dll")]
		static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
		
		[DllImportAttribute("gdi32.dll", CharSet=CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool ExtTextOut(
			IntPtr hdc,
			int X,
			int Y,
			uint fuOptions,
			void* lprc,
			string lpString,
			int cbCount,
			int[] lpDx
		);
		
		const int GCP_REORDER = 2;
		
		[DllImport("gdi32.dll")]
		static extern int SetBkMode(IntPtr hdc, int iBkMode);
		
		[DllImport("gdi32.dll")]
		static extern int SetTextColor(IntPtr hdc, int crColor);
		
		const int LF_FACESIZE = 32;
		
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		struct LOGFONT
		{
			public int lfHeight;
			public int lfWidth;
			public int lfEscapement;
			public int lfOrientation;
			public int lfWeight;
			public byte lfItalic;
			public byte lfUnderline;
			public byte lfStrikeOut;
			public byte lfCharSet;
			public byte lfOutPrecision;
			public byte lfClipPrecision;
			public byte lfQuality;
			public byte lfPitchAndFamily;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=32)]
			public string lfFaceName;
		}
		
		class MyText : IText
		{
			public string text;
			public ExtTextOutMatrixView parent;
			public int color;
			public IntPtr font;
			public System.Windows.Point Position { get; set; }
			
			public void Remove()
			{
				DeleteObject(font);
				parent.texts.Remove(this);
			}
		}
		
		sealed class TextView : Control
		{
			public ExtTextOutMatrixView parent;
			
			public TextView()
			{
				this.BackColor = Color.Black;
				SetStyle(
					ControlStyles.AllPaintingInWmPaint |
					ControlStyles.DoubleBuffer |
					ControlStyles.Opaque |
					ControlStyles.UserPaint, true);
			}
			
			protected override void OnPaint(PaintEventArgs e)
			{
				e.Graphics.Clear(Color.Black);
				IntPtr hdc = e.Graphics.GetHdc();
				try {
					SetBkMode(hdc, 1); // TRANSPARENT
					foreach (MyText text in parent.texts) {
						SetTextColor(hdc, text.color);
						SelectObject(hdc, text.font);
						ExtTextOut(hdc, (int)text.Position.X, (int)text.Position.Y,
						           0, null, text.text, text.text.Length, null);
					}
				} finally {
					e.Graphics.ReleaseHdc();
				}
				parent.OnFrameRendered(EventArgs.Empty);
			}
		}
	}
}

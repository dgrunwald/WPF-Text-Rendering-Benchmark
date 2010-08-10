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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfTextBenchmark
{
	public interface IText
	{
		Point Position { get; set; }
		void Remove();
	}
	
	public abstract class MatrixView : FrameworkElement
	{
		public MatrixView()
		{
			this.ClipToBounds = true;
		}
		
		public abstract IText AddText(string text, double fontSize, SolidColorBrush brush);
		
		public event EventHandler FrameRendered;
		
		protected virtual void OnFrameRendered(EventArgs e)
		{
			if (FrameRendered != null) {
				FrameRendered(this, e);
			}
		}
		
		protected Typeface CreateTypeface()
		{
			return new Typeface((FontFamily)GetValue(TextBlock.FontFamilyProperty),
			                    (FontStyle)GetValue(TextBlock.FontStyleProperty),
			                    (FontWeight)GetValue(TextBlock.FontWeightProperty),
			                    (FontStretch)GetValue(TextBlock.FontStretchProperty));
		}
		
		public virtual void Refresh()
		{
			InvalidateVisual();
		}
	}
}
